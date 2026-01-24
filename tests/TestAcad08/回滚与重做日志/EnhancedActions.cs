namespace JoinBoxAcad;

/// <summary>
/// 增强的命令动作，包含完整的上下文信息
/// </summary>
public class EnhancedCommandAction : BaseAction
{
    public CommandContext Context { get; }
    public List<IAction> ChildActions { get; set; } = new();

    public override ActionType Type => ActionType.CommandExecution;
    public override string Description => $"执行命令: {Context.CommandName}";

    public EnhancedCommandAction(string commandName, object[] parameters, CommandContext context) : base()
    {
        Context = context;
        Context.Parameters = parameters;
    }

    /// <summary>
    /// 执行命令（正向执行）
    /// </summary>
    public override void Execute()
    {
        Env.Printl($"[DEBUG] 执行命令: {Context.CommandName}");

        // 如果有子动作，执行子动作
        foreach (var action in ChildActions)
        {
            try
            {
                action.Execute();
            }
            catch (Exception ex)
            {
                Env.Printl($"[ERROR] 执行子动作失败: {action.Description}, 错误: {ex.Message}");
                throw;
            }
        }

        // 如果没有子动作，尝试通过CAD命令执行
        if (ChildActions.Count == 0)
        {
            var doc = Acap.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                try
                {
                    doc.SendStringToExecute($"{Context.CommandName}\n", false, false, false);
                }
                catch (Exception ex)
                {
                    Env.Printl($"[ERROR] 执行CAD命令失败: {ex.Message}");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// 获取逆向动作 - 优先使用逆命令，如果没有则使用数据回滚
    /// </summary>
    public override IAction GetInverseAction()
    {
        // 1. 首先尝试获取逆命令
        var inverseCommand = CommandInverseMap.GetInverseCommand(Context.CommandName);
        if (!string.IsNullOrEmpty(inverseCommand))
        {
            Env.Printl($"[DEBUG] 找到逆命令: {Context.CommandName} -> {inverseCommand}");
            var inverseContext = new CommandContext(inverseCommand, Context.Parameters);
            return new EnhancedCommandAction(inverseCommand, Context.Parameters, inverseContext);
        }

        // 2. 如果没有逆命令，但有子动作，创建逆向子动作集合
        if (ChildActions.Count > 0)
        {
            Env.Printl($"[DEBUG] 没有逆命令，使用数据回滚: {Context.CommandName}");
            var inverseAction = new DataRollbackAction(Context);
            return inverseAction;
        }

        // 3. 如果既没有逆命令也没有子动作，返回数据回滚动作
        Env.Printl($"[DEBUG] 使用数据回滚: {Context.CommandName}");
        return new DataRollbackAction(Context);
    }

    public override IAction Clone()
    {
        var clonedAction = new EnhancedCommandAction(Context.CommandName, Context.Parameters, Context);
        clonedAction.ChildActions = ChildActions.Select(a => a.Clone()).ToList();
        return clonedAction;
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("命令动作不能合并");
}

/// <summary>
/// 数据回滚动作 - 当没有逆命令时使用
/// </summary>
public class DataRollbackAction : BaseAction
{
    public CommandContext OriginalContext { get; }

    public override ActionType Type => ActionType.CommandExecution;
    public override string Description => $"数据回滚: {OriginalContext.CommandName}";

    public DataRollbackAction(CommandContext originalContext)
    {
        OriginalContext = originalContext;
    }

    /// <summary>
    /// 执行数据回滚
    /// </summary>
    public override void Execute()
    {
        Env.Printl($"[DEBUG] 执行数据回滚: {OriginalContext.CommandName}");

        // 按相反顺序回滚所有变更
        for (int i = OriginalContext.Changes.Count - 1; i >= 0; i--)
        {
            var change = OriginalContext.Changes[i];
            RollbackChange(change);
        }
    }

    /// <summary>
    /// 回滚单个变更
    /// </summary>
    private void RollbackChange(DatabaseChange change)
    {
        try
        {
            using var tr = DBTrans.Create(change.EntityId.Database);
            var entity = tr.GetObject(change.EntityId, OpenMode.ForWrite, true, true);

            if (entity == null)
            {
                Env.Printl($"[WARNING] 回滚时找不到实体: {change.EntityId}");
                return;
            }

            switch (change.Type)
            {
                case ActionType.DatabaseAdd:
                // 如果是添加操作，回滚就是删除
                if (!entity.IsErased)
                {
                    entity.Erase(true);
                    Env.Printl($"[DEBUG] 回滚添加操作: 删除实体 {change.EntityId}");
                }
                break;

                case ActionType.DatabaseDelete:
                // 如果是删除操作，回滚就是恢复
                if (entity.IsErased)
                {
                    entity.Erase(false); // 恢复被删除的实体
                    Env.Printl($"[DEBUG] 回滚删除操作: 恢复实体 {change.EntityId}");
                }
                break;

                case ActionType.DatabaseModify:
                // 如果是修改操作，回滚就是恢复旧值
                if (change.PropertyChanges != null)
                {
                    RollbackPropertyChanges(entity, change.PropertyChanges);
                }
                else if (change.OldValue != null)
                {
                    // 简单的属性回滚
                    var type = entity.GetType();
                    var properties = type.GetProperties();
                    foreach (var prop in properties)
                    {
                        if (prop.CanWrite && prop.Name == "GenericChange")
                        {
                            prop.SetValue(entity, change.OldValue, null);
                            break;
                        }
                    }
                }
                break;
            }

            tr.Commit();
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 回滚变更失败: {change.Type}, 实体: {change.EntityId}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 回滚属性变更
    /// </summary>
    private void RollbackPropertyChanges(DBObject entity, Dictionary<string, (object OldValue, object NewValue)> propertyChanges)
    {
        var type = entity.GetType();

        foreach (var kvp in propertyChanges)
        {
            var property = type.GetProperty(kvp.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    property.SetValue(entity, kvp.Value.OldValue, null);
                    Env.Printl($"[DEBUG] 回滚属性: {kvp.Key} = {kvp.Value.OldValue}");
                }
                catch (Exception ex)
                {
                    Env.Printl($"[ERROR] 回滚属性失败: {kvp.Key}, 错误: {ex.Message}");
                }
            }
        }
    }

    public override IAction GetInverseAction()
    {
        // 数据回滚的逆向动作就是重新执行原命令
        return new EnhancedCommandAction(OriginalContext.CommandName, OriginalContext.Parameters, OriginalContext);
    }

    public override IAction Clone()
    {
        return new DataRollbackAction(OriginalContext);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("数据回滚动作不能合并");
}