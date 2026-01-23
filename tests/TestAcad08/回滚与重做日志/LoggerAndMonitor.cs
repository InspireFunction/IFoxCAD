//namespace JoinBoxAcad;

//using static IFoxCAD.Cad.PostCmd;

//// 初始化日志系统
//public class LoggerInitializer
//{
//    private DatabaseEventMonitor? _dbMonitor;
//    private CommandEventMonitor? _cmdMonitor;
//    private ActionLogger? _logger;

//    [IFoxInitialize]
//    public void StartLogging(Document doc)
//    {
//        // 禁用原生撤回/重做
//        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;

//        // 初始化日志器
//        _logger = ActionLogger.Instance;

//        // 启动事件监听
//        _dbMonitor = new DatabaseEventMonitor();
//        _dbMonitor.StartMonitoring(doc.Database);

//        _cmdMonitor = new CommandEventMonitor();
//        _cmdMonitor.StartMonitoring();

//        doc.Editor.WriteMessage("\nCAD日志系统已启用");
//    }

//    [CommandMethod("STOPLOGGING")]
//    public void StopLogging()
//    {
//        // 停止事件监听
//        _dbMonitor?.StopMonitoring();
//        _cmdMonitor?.StopMonitoring();

//        // 启用原生撤回
//        Acap.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;

//        var doc = Application.DocumentManager.MdiActiveDocument;
//        doc?.Editor.WriteMessage("\nCAD日志系统已停止");
//    }

//    [CommandMethod(nameof(MyUndo), CommandFlags.Session)]
//    public void MyUndo()
//    {
//        ActionLogger.Instance.Undo();
//    }

//    [CommandMethod(nameof(MyRedo), CommandFlags.Session)]
//    public void MyRedo()
//    {
//        ActionLogger.Instance.Redo();
//    }

//    [CommandMethod(nameof(ShowHistory))]
//    public void ShowHistory()
//    {
//        var history = ActionLogger.Instance.GetHistory();
//        var ed = Application.DocumentManager.MdiActiveDocument.Editor;

//        ed.WriteMessage("\n=== 操作历史 ===\n");
//        foreach (var action in history)
//        {
//            ed.WriteMessage($"{action.Timestamp:HH:mm:ss} - {action.Description}\n");
//        }
//    }

//    /// <summary>
//    /// 否决原生的撤回命令
//    /// </summary>
//    /// <param name="sender"></param>
//    /// <param name="e"></param>
//    private void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
//    {
//        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
//            return;
//        switch (e.GlobalCommandName.ToUpper())
//        {
//            case "U":
//            {
//                // 屏蔽原生撤回,否则导致不知道官方撤回点.
//                e.Veto();
//                SendCommand(nameof(MyUndo) + " ", RunCmdFlag.AcedCommand);
//            }
//            break;
//            case "MREDO":
//            {
//                // 屏蔽原生重做
//                e.Veto();
//                SendCommand(nameof(MyRedo) + " ", RunCmdFlag.AcedCommand);
//            }
//            break;
//        }
//    }
//}

//public class ActionLogger
//{
//    private static ActionLogger? _instance;
//    public static ActionLogger Instance => _instance ??= new();

//    private ActionDAG _dag = new();
//    private readonly object _lock = new();

//    // 标记是否正在执行撤销/重做操作，防止循环记录
//    private bool _isExecutingUndoRedo = false;

//    // 获取/设置是否正在执行撤销/重做操作
//    public bool IsExecutingUndoRedo
//    {
//        get => _isExecutingUndoRedo;
//        set => _isExecutingUndoRedo = value;
//    }

//    // 动作映射表
//    private Dictionary<ActionType, Func<IAction, IAction>> _inverseActions = [];

//    public ActionLogger()
//    {
//        InitializeInverseActions();
//    }

//    private void InitializeInverseActions()
//    {
//        // 注册标准逆动作
//        _inverseActions[ActionType.DatabaseAdd] = action =>
//            new DeleteEntityAction(((EntityAction)action).DBObjectId);

//        _inverseActions[ActionType.DatabaseDelete] = action =>
//            new CreateEntityAction(((EntityAction)action).DBObjectId);

//        _inverseActions[ActionType.DatabaseModify] = action =>
//            ((ModifyEntityAction)action).GetInverseAction();

//        _inverseActions[ActionType.CommandExecution] = action =>
//            ((CommandAction)action).GetInverseAction();
//    }

//    // 记录动作
//    public void LogAction(IAction action)
//    {
//        lock (_lock)
//        {
//            _dag.AddAction(action);
//        }
//    }

//    // 记录实体创建
//    public void LogEntityCreation(ObjectId entityId)
//    {
//        var action = new CreateEntityAction(entityId);
//        LogAction(action);
//    }

//    // 记录实体删除
//    public void LogEntityDeletion(ObjectId entityId)
//    {
//        var action = new DeleteEntityAction(entityId);
//        LogAction(action);
//    }

//    // 记录实体修改
//    public void LogEntityModification(
//        ObjectId entityId,
//        Dictionary<string, (object, object)> changes)
//    {
//        using var tr = DBTrans.Create(entityId.Database);
//        var entity = tr.GetObject(entityId, OpenMode.ForRead) as DBObject;
//        if (entity != null)
//        {
//            var oldSnapshot = GetEntitySnapshot(entityId);
//            var newSnapshot = EntitySerializer.SerializeEntityToSnapshot(entity);

//            var action = new ModifyEntityAction(entityId, oldSnapshot, newSnapshot, changes);
//            LogAction(action);

//            // 更新快照
//            UpdateEntitySnapshot(entityId, newSnapshot);
//        }
//    }

//    // 记录命令
//    public void LogCommand(string command, params object[] parameters)
//    {
//        var action = new CommandAction(command, parameters);
//        LogAction(action);
//    }

//    // 执行撤回
//    public void Undo()
//    {
//        lock (_lock)
//        {
//            if (_dag.Current.Parent != null)
//            {
//                // 设置正在执行撤销操作的标志，防止循环记录
//                IsExecutingUndoRedo = true;

//                var inverseAction = _dag.Current.Action.GetInverseAction();
//                inverseAction.Execute();

//                _dag.SwitchTo(_dag.Current.Parent);

//                // 重置标志
//                IsExecutingUndoRedo = false;
//            }
//            else
//            {
//                // 即使无法撤销也要确保标志被重置
//                IsExecutingUndoRedo = false;
//            }
//        }
//    }

//    // 执行重做
//    public void Redo()
//    {
//        lock (_lock)
//        {
//            if (_dag.Current.Children.Count > 0)
//            {
//                // 设置正在执行重做操作的标志，防止循环记录
//                IsExecutingUndoRedo = true;

//                // 默认选择第一个子节点
//                var nextNode = _dag.Current.Children[0];
//                _dag.SwitchTo(nextNode);

//                // 重置标志
//                IsExecutingUndoRedo = false;
//            }
//            else
//            {
//                // 即使无法重做也要确保标志被重置
//                IsExecutingUndoRedo = false;
//            }
//        }
//    }

//    // 获取历史
//    public List<IAction> GetHistory()
//    {
//        return _dag.History.Select(n => n.Action).ToList();
//    }

//    // 清空历史
//    public void Clear()
//    {
//        lock (_lock)
//        {
//            _dag = new ActionDAG();
//        }
//    }

//    // 快照管理
//    private Dictionary<ObjectId, string> _entitySnapshots = new();

//    private string? GetEntitySnapshot(ObjectId entityId)
//    {
//        if (_entitySnapshots.TryGetValue(entityId, out var snapshot))
//            return snapshot;
//        return null;
//    }

//    private void UpdateEntitySnapshot(ObjectId entityId, string snapshot)
//    {
//        _entitySnapshots[entityId] = snapshot;
//    }

//}

//// 数据库事件监听
//public class DatabaseEventMonitor
//{
//    private Database _database;
//    private readonly Dictionary<ObjectId, string> _trackedEntities = [];

//    public void StartMonitoring(Database database)
//    {
//        _database = database;
//        _database.ObjectAppended += OnObjectAppended;
//        _database.ObjectModified += OnObjectModified;
//        _database.ObjectErased += OnObjectErased;
//    }

//    public void StopMonitoring()
//    {
//        if (_database != null)
//        {
//            _database.ObjectAppended -= OnObjectAppended;
//            _database.ObjectModified -= OnObjectModified;
//            _database.ObjectErased -= OnObjectErased;
//        }
//    }

//    private void OnObjectAppended(object sender, ObjectEventArgs e)
//    {
//        // 如果正在执行撤销/重做操作，则不记录此事件
//        if (ActionLogger.Instance.IsExecutingUndoRedo)
//            return;

//        var entity = e.DBObject as Entity;
//        if (entity != null)
//        {
//            // 记录创建
//            ActionLogger.Instance.LogEntityCreation(e.DBObject.ObjectId);

//            // 开始跟踪
//            StartTracking(e.DBObject.ObjectId);
//        }
//    }

//    private void OnObjectModified(object sender, ObjectEventArgs e)
//    {
//        // 如果正在执行撤销/重做操作，则不记录此事件
//        if (ActionLogger.Instance.IsExecutingUndoRedo)
//            return;

//        if (_trackedEntities.TryGetValue(e.DBObject.ObjectId, out var oldSnapshot))
//        {
//            // 从字符串快照反序列化出旧实体
//            var oldEntity = EntitySerializer.DeserializeEntityFromSnapshot(oldSnapshot, e.DBObject.GetType());
//            if (oldEntity != null)
//            {
//                var changes = CompareObjects(oldEntity, e.DBObject);
//                if (changes.Count > 0)
//                {
//                    ActionLogger.Instance.LogEntityModification(e.DBObject.ObjectId, changes);
//                }
//            }

//            // 更新快照
//            _trackedEntities[e.DBObject.ObjectId] = EntitySerializer.SerializeEntityToSnapshot(e.DBObject);
//        }
//    }

//    private void OnObjectErased(object sender, ObjectErasedEventArgs e)
//    {
//        // 如果正在执行撤销/重做操作，则不记录此事件
//        if (ActionLogger.Instance.IsExecutingUndoRedo)
//            return;

//        if (!e.DBObject.IsErased)
//        {
//            // 记录删除
//            ActionLogger.Instance.LogEntityDeletion(e.DBObject.ObjectId);

//            // 停止跟踪
//            _trackedEntities.Remove(e.DBObject.ObjectId);
//        }
//    }

//    private void StartTracking(ObjectId entityId)
//    {
//        using var tr = DBTrans.Create(_database);
//        using var entity = tr.GetObject(entityId, OpenMode.ForRead);
//        if (entity != null)
//        {
//            _trackedEntities[entityId] = EntitySerializer.SerializeEntityToSnapshot(entity);
//        }
//    }

//    private Dictionary<string, (object, object)> CompareObjects(DBObject oldObj, DBObject newObj)
//    {
//        var changes = new Dictionary<string, (object, object)>();

//        var type = oldObj.GetType();
//        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

//        foreach (var property in properties)
//        {
//            if (!property.CanRead || !property.CanWrite)
//                continue;

//            try
//            {
//                var oldValue = property.GetValue(oldObj, null);
//                var newValue = property.GetValue(newObj, null);

//                if (!AreEqual(oldValue, newValue))
//                {
//                    changes[property.Name] = (oldValue, newValue);
//                }
//            }
//            catch
//            {
//                continue;
//            }
//        }

//        return changes;
//    }

//    private bool AreEqual(object obj1, object obj2)
//    {
//        if (obj1 == null && obj2 == null) return true;
//        if (obj1 == null || obj2 == null) return false;
//        return obj1.Equals(obj2);
//    }
//}

//// 命令事件监听
//public class CommandEventMonitor
//{
//    public void StartMonitoring()
//    {
//        var doc = Application.DocumentManager.MdiActiveDocument;
//        if (doc != null)
//        {
//            doc.CommandWillStart += OnCommandWillStart;
//            doc.CommandEnded += OnCommandEnded;
//            doc.CommandCancelled += OnCommandCancelled;
//            doc.CommandFailed += OnCommandFailed;
//        }
//    }

//    public void StopMonitoring()
//    {
//        var doc = Application.DocumentManager.MdiActiveDocument;
//        if (doc != null)
//        {
//            doc.CommandWillStart -= OnCommandWillStart;
//            doc.CommandEnded -= OnCommandEnded;
//            doc.CommandCancelled -= OnCommandCancelled;
//            doc.CommandFailed -= OnCommandFailed;
//        }
//    }

//    private void OnCommandWillStart(object sender, CommandEventArgs e)
//    {
//        // 记录命令开始
//        ActionLogger.Instance.LogCommand(e.GlobalCommandName, "开始");
//    }

//    private void OnCommandEnded(object sender, CommandEventArgs e)
//    {
//        // 记录命令结束
//        ActionLogger.Instance.LogCommand(e.GlobalCommandName, "结束");
//    }

//    private void OnCommandCancelled(object sender, CommandEventArgs e)
//    {
//        // 记录命令取消
//        ActionLogger.Instance.LogCommand(e.GlobalCommandName, "取消");
//    }

//    private void OnCommandFailed(object sender, CommandEventArgs e)
//    {
//        // 记录命令失败
//        ActionLogger.Instance.LogCommand(e.GlobalCommandName, "失败");
//    }
//}