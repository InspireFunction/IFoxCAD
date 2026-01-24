namespace JoinBoxAcad;

/// <summary>
/// 增强的数据库监控器，提供完整的命令上下文跟踪
/// </summary>
public class EnhancedDatabaseMonitor
{
    private readonly Document _document;
    private CommandContext? _currentCommandContext;
    private readonly Dictionary<ObjectId, EntitySnapshot> _entitySnapshots = new();
    private readonly List<DatabaseChange> _pendingChanges = new();

    public EnhancedDatabaseMonitor(Document document)
    {
        _document = document;
    }

    public void StartMonitoring()
    {
        _document.Database.ObjectAppended += OnObjectAppended;
        _document.Database.ObjectModified += OnObjectModified;
        _document.Database.ObjectErased += OnObjectErased;
    }

    public void StopMonitoring()
    {
        _document.Database.ObjectAppended -= OnObjectAppended;
        _document.Database.ObjectModified -= OnObjectModified;
        _document.Database.ObjectErased -= OnObjectErased;
    }

    /// <summary>
    /// 开始新的命令上下文
    /// </summary>
    public void StartCommandContext(string commandName, object[] parameters)
    {
        _currentCommandContext = new CommandContext(commandName, parameters);
        _pendingChanges.Clear();
        Env.Printl($"[DEBUG] 开始命令上下文: {commandName}");
    }

    /// <summary>
    /// 结束当前命令上下文
    /// </summary>
    public CommandContext? EndCommandContext()
    {
        if (_currentCommandContext == null)
            return null;

        _currentCommandContext.Complete();
        _currentCommandContext.Changes.AddRange(_pendingChanges);

        Env.Printl($"[DEBUG] 结束命令上下文: {_currentCommandContext.CommandName}, 变更数: {_pendingChanges.Count}");

        var completedContext = _currentCommandContext;
        _currentCommandContext = null;
        _pendingChanges.Clear();

        return completedContext;
    }

    /// <summary>
    /// 获取当前命令上下文
    /// </summary>
    public CommandContext? GetCurrentCommandContext()
    {
        return _currentCommandContext;
    }

    /// <summary>
    /// 创建实体快照
    /// </summary>
    private EntitySnapshot CreateEntitySnapshot(DBObject entity)
    {
        var snapshot = new EntitySnapshot(entity.ObjectId)
        {
            EntityType = entity.GetType().Name
        };

        var type = entity.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            // 过滤掉索引器属性（索引器有参数）
            var indexParameters = property.GetIndexParameters();
            if (indexParameters.Length > 0)
                continue;

            try
            {
                if (jump.Contains(property.Name))
                {
                    continue;
                }
                var value = property.GetValue(entity, null);
                snapshot.Properties[property.Name] = value;
            }
            catch
            {
                // 忽略无法读取的属性
                jump.Add(property.Name);
            }
        }

        return snapshot;
    }

    HashSet<string> jump = ["IncludingErased", "PreviewIcon"];


    private void OnObjectAppended(object sender, ObjectEventArgs e)
    {
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject as Entity;
        if (entity == null)
            return;

        var change = new DatabaseChange(ActionType.DatabaseAdd, entity.ObjectId)
        {
            EntityType = entity.GetType().Name,
            NewValue = entity
        };

        _pendingChanges.Add(change);

        // 创建实体快照用于后续比较
        var snapshot = CreateEntitySnapshot(entity);
        _entitySnapshots[entity.ObjectId] = snapshot;

        Env.Printl($"[DEBUG] 记录添加操作: {entity.ObjectId}, 类型: {entity.GetType().Name}");
    }

    private void OnObjectModified(object sender, ObjectEventArgs e)
    {
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject;
        if (entity == null)
            return;

        // 获取或创建实体快照
        if (!_entitySnapshots.TryGetValue(entity.ObjectId, out var oldSnapshot))
        {
            oldSnapshot = CreateEntitySnapshot(entity);
            _entitySnapshots[entity.ObjectId] = oldSnapshot;
        }

        // 创建新的快照进行比较
        var newSnapshot = CreateEntitySnapshot(entity);
        var propertyChanges = CompareSnapshots(oldSnapshot, newSnapshot);

        if (propertyChanges.Count > 0)
        {
            var change = new DatabaseChange(ActionType.DatabaseModify, entity.ObjectId)
            {
                EntityType = entity.GetType().Name,
                PropertyChanges = propertyChanges
            };

            _pendingChanges.Add(change);
            _entitySnapshots[entity.ObjectId] = newSnapshot;

            Env.Printl($"[DEBUG] 记录修改操作: {entity.ObjectId}, 属性变更数: {propertyChanges.Count}");
        }
    }

    private void OnObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject;
        if (entity == null)
            return;

        var change = new DatabaseChange(ActionType.DatabaseDelete, entity.ObjectId)
        {
            EntityType = entity.GetType().Name,
            OldValue = entity
        };

        _pendingChanges.Add(change);

        // 移除快照
        _entitySnapshots.Remove(entity.ObjectId);

        Env.Printl($"[DEBUG] 记录删除操作: {entity.ObjectId}, 类型: {entity.GetType().Name}");
    }

    /// <summary>
    /// 比较两个快照，找出变更的属性
    /// </summary>
    private Dictionary<string, (object OldValue, object NewValue)> CompareSnapshots(
        EntitySnapshot oldSnapshot, EntitySnapshot newSnapshot)
    {
        var changes = new Dictionary<string, (object, object)>();

        foreach (var kvp in newSnapshot.Properties)
        {
            var propertyName = kvp.Key;
            var newValue = kvp.Value;
            var oldValue = oldSnapshot.Properties.TryGetValue(propertyName, out var oldVal) ? oldVal : null;

            if (!AreEqual(oldValue, newValue))
            {
                changes[propertyName] = (oldValue!, newValue!);
            }
        }

        return changes;
    }

    /// <summary>
    /// 判断两个值是否相等
    /// </summary>
    private bool AreEqual(object? obj1, object? obj2)
    {
        if (obj1 == null && obj2 == null) return true;
        if (obj1 == null || obj2 == null) return false;
        return obj1.Equals(obj2);
    }

    /// <summary>
    /// 是否应该记录变更
    /// </summary>
    private bool ShouldRecordChange()
    {
        // 检查是否正在执行撤销/重做操作
        var logger = EnhancedUndoRedoManager.GetLogger(_document);
        if (logger == null)
            return false;

        if (logger.IsExecutingUndoRedo)
            return false;

        // 检查是否有当前命令上下文
        return _currentCommandContext != null;
    }

    /// <summary>
    /// 获取当前命令的所有变更
    /// </summary>
    public List<DatabaseChange> GetCurrentChanges()
    {
        return new List<DatabaseChange>(_pendingChanges);
    }

    /// <summary>
    /// 获取实体的最新快照
    /// </summary>
    public EntitySnapshot? GetEntitySnapshot(ObjectId entityId)
    {
        return _entitySnapshots.TryGetValue(entityId, out var snapshot) ? snapshot : null;
    }

    /// <summary>
    /// 清除所有快照和待处理变更
    /// </summary>
    public void Clear()
    {
        _entitySnapshots.Clear();
        _pendingChanges.Clear();
        _currentCommandContext = null;
    }
}