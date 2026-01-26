namespace JoinBoxAcad;

/// <summary>
/// 命令上下文，包含命令执行期间的所有数据库变更
/// </summary>
public class CommandContext
{
    public string CommandName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<DatabaseChange> Changes { get; set; } = new();
    public object[] Parameters { get; set; } = [];
    public object? Result { get; set; }

    public CommandContext(string commandName, object[] parameters)
    {
        CommandName = commandName;
        Parameters = parameters;
        StartTime = DateTime.Now;
    }

    public void Complete()
    {
        EndTime = DateTime.Now;
    }
}

/// <summary>
/// 数据库变更记录
/// </summary>
public class DatabaseChange
{
    public ActionType Type { get; set; }
    public ObjectId EntityId { get; set; }
    // 字段名,旧值,新值
    public Dictionary<string, (object OldValue, object NewValue)> FieldChanges { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public DatabaseChange(ActionType type, ObjectId entityId,
        string? fieldName = null, object? oldValue = null, object? newValue = null)
    {
        Type = type;
        EntityId = entityId;
        if (fieldName is null || oldValue is null)
            return;
        FieldChanges[fieldName] = (oldValue, newValue!);
    }
}

/// <summary>
/// 实体状态快照
/// </summary>
public class EntitySnapshot
{
    public ObjectId EntityId { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public EntitySnapshot(ObjectId entityId)
    {
        EntityId = entityId;
    }
}