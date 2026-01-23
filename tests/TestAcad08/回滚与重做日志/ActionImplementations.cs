using System.Windows.Media.Animation;

namespace JoinBoxAcad;

// 基础动作实现
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
            Console.WriteLine($"Action Serialize Success: {this.GetType().Name}, Result Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Action Serialize Error: {this.GetType().Name}, Message: {ex.Message}");
            Console.WriteLine($"Action Serialize Error Details: {ex}");
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
        try
        {
            Console.WriteLine($"Action Deserialize Input Json Length: {json?.Length}, Content Preview: {(json?.Length > 100 ? json.Substring(0, 100) : json)}");
            var s = MyJson.DeserializeObject<BaseAction>(json);
            Console.WriteLine($"Action Deserialize Success: {(s != null ? s.GetType().Name : "null")}");
            if (s is null)
                return null;
            return s;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Action Deserialize Error, Message: {ex.Message}");
            Console.WriteLine($"Action Deserialize Error Details: {ex}");
            Console.WriteLine($"Problematic JSON: {(json?.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            throw;
        }
    }
}


// 实体动作基类
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
    /// <summary>
    /// 快照
    /// </summary>
    public string EntitySnapshot { get; internal set; }

    protected EntityAction(ObjectId entityId)
    {
        DBObjectId = entityId;

        using var tr = DBTrans.Create(entityId.Database);
        using var entity = tr.GetObject(entityId);
        if (entity is null) throw new ArgumentNullException();
        EntityType = entity.GetType().Name;

        var dwgFiler = new DwgFilerEx();
        dwgFiler.DwgOut(entity);
        EntitySnapshot = dwgFiler.SerializeObject();
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
        // 如果id对象已经删除,那么这里重新加入
        using var tr = DBTrans.Create(DBObjectId.Database);
        using var obj = tr.GetObject(DBObjectId, OpenMode.ForRead, true, true);
        if (obj is not null && obj.IsErased)
        {
            var dwgFiler = DwgFilerEx.DeserializeObject(EntitySnapshot);
            if (dwgFiler != null)
            {
                dwgFiler.DwgIn();
            }
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

// 删除实体动作
public class DeleteEntityAction : EntityAction
{
    public override ActionType Type => ActionType.DatabaseDelete;
    public override string Description => $"删除实体: {EntityType}";

    public DeleteEntityAction(ObjectId entityId) : base(entityId) { }

    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        var entity = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (entity != null && !entity.IsErased)
            entity.Erase();
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

// 属性变化结构体
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
public class ModifyEntityAction : EntityAction
{
    public Dictionary<string, (object OldValue, object NewValue)> PropertyChanges { get; }
    /// <summary>
    /// 旧值(首次加入就没有旧值)
    /// </summary>
    public string OldSnapshot { get; }
    /// <summary>
    /// 新值
    /// </summary>
    public string NewSnapshot { get; }

    public override ActionType Type => ActionType.DatabaseModify;
    public override string Description => $"修改实体: {EntityType} ({PropertyChanges.Count} 个属性)";

    public ModifyEntityAction(ObjectId entityId,
        string? oldSnapshot,
        string newSnapshot,
        Dictionary<string, (object, object)> changes) : base(entityId)
    {
        OldSnapshot = oldSnapshot;
        NewSnapshot = newSnapshot;
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
        if (OldSnapshot is null) throw new InvalidOperationException();
        return new ModifyEntityAction(DBObjectId, NewSnapshot, OldSnapshot, inverseChanges);
    }

    public override IAction Clone()
    {
        if (OldSnapshot is null)
            throw new InvalidOperationException();
        if (NewSnapshot is null)
            throw new InvalidOperationException();

        return new ModifyEntityAction(DBObjectId, OldSnapshot, NewSnapshot, new Dictionary<string, (object, object)>(PropertyChanges));
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
            OldSnapshot,
            otherModify.NewSnapshot,
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
    public object Result { get; set; }

    public override ActionType Type => ActionType.CommandExecution;
    public override string Description => $"执行命令: {CommandName}";

    public CommandAction(string commandName, params object[] parameters)
    {
        CommandName = commandName;
        Parameters = parameters;
    }

    public override void Execute()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        doc?.SendStringToExecute($"{CommandName} ", true, false, true);
    }

    public override IAction GetInverseAction()
    {
        // 查找命令的逆命令
        var inverseCommand = CommandInverseMap.GetInverseCommand(CommandName);
        if (inverseCommand != null)
        {
            return new CommandAction(inverseCommand, Parameters);
        }

        // 如果没有预定义的逆命令，返回一个通用的撤销命令
        return new CommandAction("U");
    }

    public override IAction Clone() => new CommandAction(CommandName, Parameters);

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("命令动作不能合并");
}


// 命令映射表
public static class CommandInverseMap
{
    private static readonly Dictionary<string, string> _commandPairs = new()
{
    // CAD命令映射
    { "LINE", "ERASE LAST" },
    { "CIRCLE", "ERASE LAST" },
    { "RECTANG", "ERASE LAST" },
    { "MOVE", "U" },  // UNDO
    { "COPY", "U" },
    { "ROTATE", "U" },
    { "SCALE", "U" },
    { "MIRROR", "U" },
    { "ERASE", "OOPS" },
    { "OOPS", "ERASE LAST" },
    
    // 图层命令
    { "-LAYER", "U" },
    { "LAYISO", "LAYUNISO" },
    { "LAYUNISO", "LAYISO" },
    { "LAYFRZ", "LAYTHW" },
    { "LAYTHW", "LAYFRZ" },
    { "LAYLOK", "LAYULK" },
    { "LAYULK", "LAYLOK" },
    
    // 在位编辑命令
    { "REFEDIT", "REFCLOSE _D" },
    { "REFCLOSE _S", "U" },  // 保存修改
    { "REFCLOSE _D", "U" },  // 放弃修改
};

    public static string GetInverseCommand(string command)
    {
        if (_commandPairs.TryGetValue(command.ToUpper(), out var inverse))
            return inverse;

        // 尝试匹配模式
        if (command.StartsWith("_"))
            return GetInverseCommand(command.Substring(1));

        return string.Empty;
    }
}
