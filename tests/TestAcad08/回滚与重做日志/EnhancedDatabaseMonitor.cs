namespace JoinBoxAcad;

/// <summary>
/// 增强的数据库监控器，提供完整的命令上下文跟踪
/// </summary>
public class EnhancedDatabaseMonitor : IDisposable
{
    private readonly Document _document;
    private CommandContext? _currentCommandContext;
    private EnhancedActionLogger _logger;
    private readonly Dictionary<ObjectId, EntitySnapshot> _entitySnapshots = [];
    /// <summary>
    /// 数据改变记录
    /// </summary>
    private readonly List<DatabaseChange> _pendingChanges = [];
    private bool _IsDisposed;

    public EnhancedDatabaseMonitor(Document document, EnhancedActionLogger logger)
    {
        _document = document;
        _logger = logger;
        _document.Database.ObjectAppended += OnObjectAppended;
        _document.Database.ObjectModified += OnObjectModified;
        _document.Database.ObjectErased += OnObjectErased;
    }

    // 公共 Dispose 方法
    public void Dispose()
    {
        Dispose(true);
        // 阻止垃圾回收器调用析构函数
        GC.SuppressFinalize(this);
    }

    // 受保护的虚拟 Dispose 方法
    protected virtual void Dispose(bool disposing)
    {
        if (_IsDisposed)
            return;
        _IsDisposed = true;

        if (disposing)
        {
            _document.Database.ObjectAppended -= OnObjectAppended;
            _document.Database.ObjectModified -= OnObjectModified;
            _document.Database.ObjectErased -= OnObjectErased;

            Clear();
        }
    }

    /// <summary>
    /// 开始录制新的命令上下文
    /// </summary>
    public void StartCommandContext(string commandName, object[] parameters)
    {
        _currentCommandContext = new(commandName, parameters);
        _pendingChanges.Clear();
    }

    /// <summary>
    /// 结束录制当前命令上下文
    /// </summary>
    public CommandContext? EndCommandContext()
    {
        if (_currentCommandContext == null)
            return null;

        _currentCommandContext.Complete();
        _currentCommandContext.Changes.AddRange(_pendingChanges);

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
    private EntitySnapshot CreateSnapshot(DBObject entity)
    {
        var snapshot = new EntitySnapshot(entity.ObjectId);
        MemberHelper.GetMemberInHierarchy(entity, snapshot, entity.GetType());
        return snapshot;
    }

    private void OnObjectAppended(object sender, ObjectEventArgs e)
    {
        DebugEx.Printl($"OnObjectAppended - {DateTime.Now}");
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject as Entity;
        if (entity == null)
            return;

        var change = new DatabaseChange(ActionType.DatabaseAdd, entity.ObjectId);
        _pendingChanges.Add(change);

        // 创建实体快照用于后续比较
        var snapshot = CreateSnapshot(entity);
        _entitySnapshots[entity.ObjectId] = snapshot;

        DebugEx.Printl($"[DEBUG] 记录添加操作: {entity.ObjectId}, 类型: {entity.GetType().Name}");
    }

    private void OnObjectModified(object sender, ObjectEventArgs e)
    {
        DebugEx.Printl($"OnObjectModified - {DateTime.Now}");
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject;
        if (entity == null)
            return;

        // 获取或创建实体快照
        if (!_entitySnapshots.TryGetValue(entity.ObjectId, out var oldSnapshot))
        {
            oldSnapshot = CreateSnapshot(entity);
            _entitySnapshots[entity.ObjectId] = oldSnapshot;
        }

        // 为什么这里没有记录 颜色的字段呢? 难道cad是远程字段?也就是属性=>cpp/cli字段?
        // 如果真的是这样,那么我们就不能只反射字段了,但是属性改起来可能会异常...
        // 创建新的快照进行比较
        var newSnapshot = CreateSnapshot(entity);
        var fieldChanges = CompareSnapshots(oldSnapshot, newSnapshot);

        if (fieldChanges.Count > 0)
        {
            var change = new DatabaseChange(ActionType.DatabaseModify, entity.ObjectId)
            {
                FieldChanges = fieldChanges
            };

            _pendingChanges.Add(change);
            _entitySnapshots[entity.ObjectId] = newSnapshot;

            DebugEx.Printl($"[DEBUG] 记录修改操作: {entity.ObjectId}, 字段变更数: {fieldChanges.Count}");
        }
    }

    private void OnObjectErased(object sender, ObjectErasedEventArgs e)
    {
        DebugEx.Printl($"OnObjectErased - {DateTime.Now}");
        if (!ShouldRecordChange())
            return;

        var entity = e.DBObject;
        if (entity == null)
            return;

        var change = new DatabaseChange(ActionType.DatabaseDelete, entity.ObjectId);
        _pendingChanges.Add(change);

        // 移除快照
        _entitySnapshots.Remove(entity.ObjectId);

        DebugEx.Printl($"[DEBUG] 记录删除操作: {entity.ObjectId}, 类型: {entity.GetType().Name}");
    }

    /// <summary>
    /// 比较两个快照，找出变更的字段
    /// </summary>
    private Dictionary<string, (object OldValue, object NewValue)> CompareSnapshots(
        EntitySnapshot oldSnapshot, EntitySnapshot newSnapshot)
    {
        var changes = new Dictionary<string, (object, object)>();

        // 只比较新快照中的字段
        foreach (var kvp in newSnapshot.Fields)
        {
            var fieldName = kvp.Key;
            var newValue = kvp.Value;
            var oldValue = oldSnapshot.Fields.TryGetValue(fieldName, out var oldVal) ? oldVal : null;

            if (!AreEqual(oldValue, newValue))
            {
                changes[fieldName] = (oldValue!, newValue!);
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
        return [.. _pendingChanges];
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