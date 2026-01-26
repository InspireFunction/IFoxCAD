using static IFoxCAD.Cad.PostCmd;

namespace JoinBoxAcad;

/// <summary>
/// 基础动作实现
/// </summary>
public abstract class BaseAction : IAction
{
    public string GuId { get; } = Guid.NewGuid().ToString();
    public abstract ActionType Type { get; }
    public abstract string Description { get; }
    public DateTime Timestamp { get; } = DateTime.Now;

    public abstract void Execute();
    public abstract IAction GetInverseAction();
    public abstract IAction Clone();

    public abstract bool CanMergeWith(IAction otherAction);
    public abstract IAction MergeWith(IAction otherAction);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public virtual string Serialize()
    {
        try
        {
            var result = MyJson.SerializeObject(this, Formatting.Indented);
            Env.Printl($"Action Serialize Success: {this.GetType().Name}, Result Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Env.Printl($"Action Serialize Error: {this.GetType().Name}, Message: {ex.Message}");
            Env.Printl($"Action Serialize Error Details: {ex}");
            throw;
        }
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IAction? Deserialize(string json)
    {
        if (json is null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        try
        {
            Env.Printl($"Action Deserialize Input Json Length: {json?.Length}, Content Preview: {(json?.Length > 100 ? json.Substring(0, 100) : json)}");
            var s = MyJson.DeserializeObject<BaseAction>(json);
            Env.Printl($"Action Deserialize Success: {(s != null ? s.GetType().Name : "null")}");
            if (s is null)
                return null;
            return s;
        }
        catch (Exception ex)
        {
            Env.Printl($"Action Deserialize Error, Message: {ex.Message}");
            Env.Printl($"Action Deserialize Error Details: {ex}");
            Env.Printl($"Problematic JSON: {(json?.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            throw;
        }
    }
}


/// <summary>
/// 实体动作基类
/// </summary>
public abstract class EntityAction : BaseAction
{
    /// <summary>
    /// id
    /// </summary>
    public ObjectId DBObjectId { get; internal set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string? EntityType { get; internal set; }

    protected EntityAction(ObjectId entityId)
    {
        DBObjectId = entityId;
        using var tr = DBTrans.Create(entityId.Database);
        using var entity = tr.GetObject(entityId, OpenMode.ForWrite, true, true);
        if (entity is null)
        {
            Env.Printl("怎么它是null呢");
        }
        EntityType = entity?.GetType().Name;
    }
}

/// <summary>
/// 创建实体动作
/// </summary>
public class CreateEntityAction : EntityAction
{
    public override ActionType Type => ActionType.DatabaseAdd;
    public override string Description => $"创建实体: {EntityType}";

    /// <summary>
    /// 创建实体动作
    /// </summary>
    public CreateEntityAction(ObjectId entityId) : base(entityId) { }

    /// <summary>
    /// 正向动作(由于cad已经创建,这里不需要)
    /// </summary>
    public override void Execute()
    {
        // 如果对象被删除,使用Erase(false)恢复它
        using var tr = DBTrans.Create(DBObjectId.Database);
        using var obj = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (obj is not null && obj.IsErased)
        {
            obj.Erase(false); // 使用false参数恢复被删除的对象
        }
    }

    /// <summary>
    /// 逆向动作
    /// </summary>
    public override IAction GetInverseAction() => new DeleteEntityAction(DBObjectId);

    public override IAction Clone()
    {
        return new CreateEntityAction(DBObjectId);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("创建动作不能合并");
}

/// <summary>
/// 删除实体动作
/// </summary>
public class DeleteEntityAction : EntityAction
{
    public override ActionType Type => ActionType.DatabaseDelete;
    public override string Description => $"删除实体: {EntityType}";

    /// <summary>
    /// 删除实体动作
    /// </summary>
    public DeleteEntityAction(ObjectId entityId) : base(entityId) { }

    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        var entity = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (entity != null && !entity.IsErased)
            entity.Erase(true); // 使用true参数确保正确删除
    }

    public override IAction GetInverseAction()
    {
        return new CreateEntityAction(DBObjectId);
    }

    public override IAction Clone() => new DeleteEntityAction(DBObjectId);

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("删除动作不能合并");
}

/// <summary>
/// 属性变化结构体
/// </summary>
public struct PropertyChange
{
    public object OldValue { get; set; }
    public object NewValue { get; set; }

    public PropertyChange(object oldValue, object newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// 修改的动作
/// </summary>
public class ModifyEntityAction : EntityAction
{
    public Dictionary<string, (object OldValue, object NewValue)> PropertyChanges { get; }

    public override ActionType Type => ActionType.DatabaseModify;
    public override string Description => $"修改实体: {EntityType} ({PropertyChanges.Count} 个属性)";

    public ModifyEntityAction(ObjectId entityId,
        Dictionary<string, (object, object)> changes) : base(entityId)
    {
        PropertyChanges = changes;
    }

    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        var entity = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (entity != null)
        {
            ApplyProperties(entity, PropertyChanges, true);
        }
    }

    public override IAction GetInverseAction()
    {
        var inverseChanges = PropertyChanges.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.NewValue, kvp.Value.OldValue)
        );
        return new ModifyEntityAction(DBObjectId, inverseChanges);
    }

    public override IAction Clone()
    {
        return new ModifyEntityAction(DBObjectId, new Dictionary<string, (object, object)>(PropertyChanges));
    }

    public override bool CanMergeWith(IAction otherAction)
    {
        if (otherAction is not ModifyEntityAction otherModify)
            return false;

        return DBObjectId == otherModify.DBObjectId &&
               CanMergeProperties(otherModify.PropertyChanges);
    }

    public override IAction MergeWith(IAction otherAction)
    {
        if (!CanMergeWith(otherAction))
            throw new InvalidOperationException("动作不能合并");

        var otherModify = (ModifyEntityAction)otherAction;
        var mergedChanges = MergePropertyChanges(PropertyChanges, otherModify.PropertyChanges);

        return new ModifyEntityAction(
            DBObjectId,
            mergedChanges
        );
    }

    private bool CanMergeProperties(Dictionary<string, (object, object)> otherChanges)
    {
        // 检查属性变化是否可以合并
        foreach (var kvp in otherChanges)
        {
            if (PropertyChanges.TryGetValue(kvp.Key, out var currentChange))
            {
                // 如果属性被多次修改，需要检查是否兼容
                if (!(currentChange.NewValue?.Equals(kvp.Value.Item1) ?? false))
                    return false;
            }
        }
        return true;
    }

    private Dictionary<string, (object, object)> MergePropertyChanges(
        Dictionary<string, (object, object)> changes1,
        Dictionary<string, (object, object)> changes2)
    {
        var merged = new Dictionary<string, (object, object)>();

        // 合并第一个变化集
        foreach (var kvp in changes1)
        {
            merged[kvp.Key] = kvp.Value;
        }

        // 合并第二个变化集
        foreach (var kvp in changes2)
        {
            merged[kvp.Key] = (changes1.TryGetValue(kvp.Key, out var change) ?
                change.Item1 : kvp.Value.Item1, kvp.Value.Item2);
        }

        return merged;
    }

    private void ApplyProperties(
        DBObject entity,
        Dictionary<string, (object OldValue, object NewValue)> changes,
        bool applyNewValue)
    {
        var type = entity.GetType();
        foreach (var change in changes)
        {
            var property = type.GetProperty(change.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var value = applyNewValue ? change.Value.Item2 : change.Value.Item1;
                    if (value is not null)
                        property.SetValue(entity, value, null);
                }
                catch
                {
                    // 记录日志
                }
            }
        }
    }
}


// 命令动作
public class CommandAction : BaseAction
{
    public string CommandName { get; }
    public object[] Parameters { get; }
    public object? Result { get; set; }

    public override ActionType Type => ActionType.CommandExecution;
    public override string Description => $"执行命令: {CommandName}";

    public CommandAction(string commandName, params object[] parameters)
    {
        CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        Parameters = parameters ?? [];

        // 验证参数，确保没有可能导致CAD API问题的类型
        if (parameters != null)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && !IsValidParameterType(parameters[i]))
                {
                    Env.Printl($"[WARNING] CommandAction参数类型可能有问题: {parameters[i].GetType().Name}, value: {parameters[i]}");
                    parameters[i] = parameters[i].ToString(); // 转换为字符串
                }
            }
        }
    }

    private bool IsValidParameterType(object param)
    {
        if (param == null) return true;

        var type = param.GetType();
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type.IsEnum;
    }

    public override void Execute()
    {
        // 命令动作的执行由CAD系统自动处理，这里不需要额外操作
        // 避免递归调用 SendStringToExecute
        Env.Printl($"[DEBUG] 将要执行命令 {CommandName}");
        var doc = Acap.DocumentManager.MdiActiveDocument;

        doc?.SendStringToExecute($"{CommandName}\n", false, false, false);
    }

    public override IAction GetInverseAction()
    {
        // 查找命令的逆命令
        var inverseCommand = CommandInverseMap.GetInverseCommand(CommandName);
        if (inverseCommand != string.Empty) // 存在逆命令
        {
            return new CommandAction(inverseCommand, Parameters);
        }

        // 如果没有预定义的逆命令,尝试使用数据库监控器提供的数据来构建逆操作
        // 如果仍然无法确定逆操作，返回一个描述性的命令动作
        return null;
    }

    public override IAction Clone() => new CommandAction(CommandName, Parameters);

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("命令动作不能合并");
}

/// <summary>
/// 在位编辑添加动作
/// </summary>
public class InPlaceAddAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }

    public override ActionType Type => ActionType.InPlaceAdd;
    public override string Description => $"添加到在位编辑: {ObjectIds.Length} 个对象";

    public InPlaceAddAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        // 选中 ObjectIds, 然后发送命令,在位编辑添加
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            // 发送异步命令需要添加进容器,外部 IsExecutingUndoRedo 就不会设置
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFSET");

            var ed = doc.Editor;
            // 设置新的选择集
            ed.SetImpliedSelection(ObjectIds);
            // 发送异步命令,添加
            doc.SendStringToExecute("REFSET\nA\n", false, false, false);
        }
        Env.Printl($"[DEBUG] 执行在位编辑添加操作，对象数: {ObjectIds.Length}");
    }

    public override IAction GetInverseAction()
    {
        // 逆向动作是从在位编辑中移除这些对象
        return new InPlaceRemoveAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceAddAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑添加动作不能合并");
}

/// <summary>
/// 在位编辑移除动作
/// </summary>
public class InPlaceRemoveAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }

    public override ActionType Type => ActionType.InPlaceRemove;
    public override string Description => $"从在位编辑移除: {ObjectIds.Length} 个对象";

    public InPlaceRemoveAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        // 选中 ObjectIds, 然后发送命令,在位编辑移除
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            // 发送异步命令需要添加进容器,外部 IsExecutingUndoRedo 就不会设置
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFSET");

            var ed = doc.Editor;
            // 设置新的选择集
            ed.SetImpliedSelection(ObjectIds);
            // 发送异步命令,移除
            doc.SendStringToExecute("REFSET\nR\n", false, false, false);
        }
        Env.Printl($"[DEBUG] 执行在位编辑移除操作，对象数: {ObjectIds.Length}");
    }

    public override IAction GetInverseAction()
    {
        // 逆向动作是添加这些对象到在位编辑
        return new InPlaceAddAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceRemoveAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑移除动作不能合并");
}
