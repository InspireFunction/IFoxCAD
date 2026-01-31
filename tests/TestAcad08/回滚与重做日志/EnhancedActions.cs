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

    public EnhancedCommandAction(CommandContext context) : base()
    {
        Context = context;
    }

    /// <summary>
    /// 执行命令（正向执行）
    /// </summary>
    public override void Execute()
    {
        Env.Printl($"[DEBUG] 执行命令: {Context.CommandName}");

        // 由于实体动作已经作为独立节点添加，这里不需要执行任何操作
        // 命令级别的动作已经被分解为多个实体动作节点
    }

    /// <summary>
    /// 获取逆向动作 - 现在实体动作已经作为独立节点，不需要命令级别的逆向动作
    /// </summary>
    public override IAction GetInverseAction()
    {
        // 由于实体动作已经作为独立节点添加，返回一个空动作
        // 每个实体动作都会有自己的逆向动作
        return new EmptyAction();
    }

    /// <summary>
    /// 空动作类，用于表示无逆向动作的情况
    /// </summary>
    private class EmptyAction : BaseAction
    {
        public override ActionType Type => ActionType.OtherOperation;
        public override string Description => "空动作";
        public override void Execute() { }
        public override IAction GetInverseAction() => this;
        public override IAction Clone() => this;
        public override bool CanMergeWith(IAction otherAction) => false;
        public override IAction MergeWith(IAction otherAction) => this;
    }

    public override IAction Clone()
    {
        var clonedAction = new EnhancedCommandAction(Context);
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

        // 只回滚当前操作的变更，不影响其他操作
        // 每个命令动作对应一个操作，所以只处理该操作的变更
        foreach (var change in OriginalContext.Changes)
        {
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
            using var entity = tr.GetObject(change.EntityId, OpenMode.ForWrite, true, true);

            if (entity == null)
            {
                Env.Printl($"[WARNING] 回滚时找不到实体: {change.EntityId}");
                return;
            }

            switch (change.Type)
            {
                case ActionType.DatabaseAdd:
                // 如果是添加操作，回滚就是删除
                // 但只有当该操作是添加实体时才执行，避免误删
                if (!entity.IsErased && OriginalContext.Changes.Count == 1)
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
                if (change.FieldChanges != null && change.FieldChanges.Count > 0)
                {
                    RollbackFieldChanges(entity, change.FieldChanges);
                }
                break;
            }
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 回滚变更失败: {change.Type}, 实体: {change.EntityId}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 回滚字段变更
    /// </summary>
    private void RollbackFieldChanges(DBObject entity, Dictionary<string, (object OldValue, object NewValue)> fieldChanges)
    {
        foreach (var kvp in fieldChanges)
        {
            // 优先尝试直接操作字段
            bool success = MemberHelper.TryRollbackMember(entity, kvp.Key, kvp.Value.OldValue, true);
            if (!success)
            {
                // 字段操作失败，尝试属性操作
                MemberHelper.TryRollbackMember(entity, kvp.Key, kvp.Value.OldValue, false);
            }
        }
    }

    public override IAction GetInverseAction()
    {
        // 数据回滚的逆向动作就是重新执行原命令
        return new EnhancedCommandAction(OriginalContext);
    }

    public override IAction Clone()
    {
        return new DataRollbackAction(OriginalContext);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("数据回滚动作不能合并");
}