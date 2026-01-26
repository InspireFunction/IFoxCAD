namespace JoinBoxAcad;

/// <summary>
/// 增强的动作日志记录器
/// </summary>
public class EnhancedActionLogger : IDisposable
{
    private readonly Document _doc;
    private readonly EnhancedDatabaseMonitor _dbMonitor;
    private readonly InPlaceEditHandler _inPlaceEditHandler;
    private readonly ActionDAG _dag = new();
    private bool _isExecutingUndoRedo = false;
    private bool _IsDisposed;

    // 实体动作映射 - 跟踪每个实体的所有相关动作
    private readonly Dictionary<ObjectId, List<VersionNode>> _entityActionMap = [];

    public EnhancedActionLogger(Document document)
    {
        _doc = document;
        _doc.CommandWillStart += OnCommandWillStart;
        _doc.CommandEnded += OnCommandEnded;
        _doc.CommandCancelled += OnCommandCancelled;
        _doc.CommandFailed += OnCommandFailed;

        _dbMonitor = new(document, this);
        _inPlaceEditHandler = new(document, this);
    }

    /// <summary>
    /// 命令即将开始
    /// </summary>
    private void OnCommandWillStart(object? sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandWillStart - {DateTime.Now}");
        if (_isExecutingUndoRedo)
            return;
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 结束无命令上下文并记录动作
        var context = _dbMonitor.EndCommandContext();
        if (context != null && context.Changes.Count > 0)
        {
            var action = new EnhancedCommandAction(e.GlobalCommandName, context.Parameters, context);
            LogAction(action);
            Env.Printl($"[DEBUG] 结束录制无命令期间的动作, 变更数: {context.Changes.Count}");
        }
        else
        {
            Env.Printl($"[DEBUG] 结束录制无命令期间 无变化");
        }

        // 开始新的命令上下文
        _dbMonitor.StartCommandContext(e.GlobalCommandName, []);
        DebugEx.Printl($"[DEBUG] 开始录制命令期间: {e.GlobalCommandName}");
    }

    /// <summary>
    /// 命令结束
    /// </summary>
    private void OnCommandEnded(object? sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandEnded - {DateTime.Now}");
        if (_isExecutingUndoRedo)
            return;
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 结束有命令上下文并记录动作
        var context = _dbMonitor.EndCommandContext();
        if (context != null && context.Changes.Count > 0)
        {
            var action = new EnhancedCommandAction(e.GlobalCommandName, context.Parameters, context);
            LogAction(action);
            DebugEx.Printl($"[DEBUG] 结束录制命令期间的动作: {e.GlobalCommandName}, 变更数: {context.Changes.Count}");
        }

        // 开始无命令期间录制
        _dbMonitor.StartCommandContext("无命令", []);
        DebugEx.Printl($"[DEBUG] 开始录制无命令期间");
    }

    /// <summary>
    /// 命令被取消
    /// </summary>
    private void OnCommandCancelled(object? sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandCancelled - {DateTime.Now}");
        if (_isExecutingUndoRedo)
            return;
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 取消的命令不记录，只清理上下文
        _dbMonitor.Clear();
        DebugEx.Printl($"[DEBUG] 命令取消: {e.GlobalCommandName}");
    }

    /// <summary>
    /// 命令失败
    /// </summary>
    private void OnCommandFailed(object? sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandFailed - {DateTime.Now}");
        if (_isExecutingUndoRedo)
            return;
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 失败的命令不记录，只清理上下文
        _dbMonitor.Clear();
        DebugEx.Printl($"[DEBUG] 命令失败: {e.GlobalCommandName}");
    }


    // 需要排除的命令列表（自定义命令不应被记录）
    // 避免循环执行
    internal static readonly HashSet<string> ExcludedCommands = new(StringComparer.OrdinalIgnoreCase)
    { 
        // CAD内部命令（可能导致问题）
        "U",           // 原生撤销
        "UNDO",        // 原生撤销
        "MREDO",       // 原生重做
        "REDO",        // 原生重做
        "_U",          // 命令行撤销
        "_UNDO",       // 命令行撤销
        "_MREDO",      // 命令行重做
        "_REDO",       // 命令行重做

        // 这些感觉也是需要记录啊
        //"PASTECLIP",   // 粘贴命令（系统级）
        //"_PASTECLIP"   // 命令行粘贴
    };


    /// <summary>
    /// 是否应该排除命令
    /// </summary>
    private bool ShouldExcludeCommand(string commandName)
    {
        if (StringHelper.IsNullOrWhiteSpace(commandName))
            return true;
        return ExcludedCommands.Contains(commandName);
    }

    /// <summary>
    /// 记录动作
    /// </summary>
    public void LogAction(IAction action)
    {
        // 检查是否正在执行undo/redo操作
        if (_isExecutingUndoRedo)
            return;

        // 如果是EnhancedCommandAction，创建子动作
        if (action is EnhancedCommandAction commandAction)
        {
            // 将数据库变更转换为子动作
            foreach (var change in commandAction.Context.Changes)
            {
                IAction? childAction = null;
                switch (change.Type)
                {
                    case ActionType.DatabaseAdd:
                    childAction = new CreateEntityAction(change.EntityId);
                    break;
                    case ActionType.DatabaseDelete:
                    childAction = new DeleteEntityAction(change.EntityId);
                    break;
                    case ActionType.DatabaseModify:
                    if (change.PropertyChanges.Count > 0)
                        childAction = new ModifyEntityAction(change.EntityId, change.PropertyChanges);
                    break;
                }
                if (childAction != null)
                    commandAction.ChildActions.Add(childAction);
            }
        }

        var node = _dag.AddAction(action);

        // 如果是实体动作，记录到实体动作映射中
        if (action is EntityAction entityAction)
        {
            RecordEntityAction(entityAction.DBObjectId, node);
        }
        // 如果是EnhancedCommandAction，记录其子动作
        else if (action is EnhancedCommandAction commandAction2)
        {
            foreach (var childAction in commandAction2.ChildActions)
            {
                if (childAction is EntityAction childEntityAction)
                {
                    RecordEntityAction(childEntityAction.DBObjectId, node);
                }
            }
        }
    }

    /// <summary>
    /// 记录实体动作节点
    /// </summary>
    private void RecordEntityAction(ObjectId entityId, VersionNode node)
    {
        if (!_entityActionMap.ContainsKey(entityId))
            _entityActionMap[entityId] = new List<VersionNode>();

        _entityActionMap[entityId].Add(node);
    }

    /// <summary>
    /// 获取实体相关的所有动作节点
    /// </summary>
    public List<VersionNode> GetEntityActionNodes(ObjectId entityId)
    {
        if (_entityActionMap.TryGetValue(entityId, out var nodes))
            return nodes;
        return [];
    }

    /// <summary>
    /// 执行撤销
    /// </summary>
    public void Undo()
    {
        if (IsAtRoot)
        {
            Env.Printl("\n已经到达初始状态，无法继续撤销。\n");
            return;
        }

        if (_dag.Current.Parent == null)
        {
            Env.Printl("\n没有可撤销的操作。\n");
            return;
        }

        _isExecutingUndoRedo = true;
        try
        {
            // 执行撤销之前,把当前动作加入redolog以便回滚?
            // 但是似乎DAG图,拥有完整的数据了.
            // 但是为什么撤回 在位编辑器 添加 移除 这两个没有成功记录呢?
            var currentAction = _dag.Current.Action;
            Env.Printl($"\n开始撤销: {currentAction.Description}");

            // 获取逆向动作
            var inverseAction = currentAction.GetInverseAction();
            if (inverseAction is not null)
            {
                Env.Printl($"使用逆向动作: {inverseAction.Description}");
                inverseAction.Execute();
            }
            else
            {
                Env.Printl("[WARNING] 无法获取逆向动作");
            }

            // 切换到父节点
            _dag.Current = _dag.Current.Parent;
            Env.Printl("撤销完成\n");
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 撤销失败: {ex.Message}");
        }
        finally
        {
            // #260126a 使用了异步命令这里就不清理了,在命令结束后事件清理
            if (AsyncCmds.Count == 0)
                _isExecutingUndoRedo = false;
        }
    }

    /// <summary>
    /// 执行重做
    /// </summary>
    public void Redo()
    {
        Redo(1);
    }

    /// <summary>
    /// 执行多次重做
    /// </summary>
    /// <param name="count">重做次数</param>
    public void Redo(int count)
    {
        if (_dag.Current.Children.Count == 0)
        {
            Env.Printl("\n居然没有可重做的操作。\n");
            return;
        }

        _isExecutingUndoRedo = true;

        try
        {
            int executed = 0;
            for (int i = 0; i < count && _dag.Current.Children.Count > 0; i++)
            {
                // 切换到第一个子节点
                var nextNode = _dag.Current.Children[0];
                var action = nextNode.Action;

                Env.Printl($"\n开始重做: {action.Description}");
                action.Execute();

                _dag.Current = nextNode;
                executed++;
            }

            if (executed > 0)
            {
                Env.Printl($"\n已完成 {executed} 个操作的重做\n");
            }
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 重做失败: {ex.Message}");
        }
        finally
        {
            _isExecutingUndoRedo = false;
        }
    }

    /// <summary>
    /// 执行全部重做
    /// </summary>
    public void RedoAll()
    {
        if (_dag.Current.Children.Count == 0)
        {
            Env.Printl("\n居然没有可重做的操作。\n");
            return;
        }

        _isExecutingUndoRedo = true;

        try
        {
            int executed = 0;
            while (_dag.Current.Children.Count > 0)
            {
                // 切换到第一个子节点
                var nextNode = _dag.Current.Children[0];
                var action = nextNode.Action;

                Env.Printl($"\n开始重做: {action.Description}");
                action.Execute();

                _dag.Current = nextNode;
                executed++;
            }

            if (executed > 0)
            {
                Env.Printl($"\n已完成全部 {executed} 个操作的重做\n");
            }
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 重做失败: {ex.Message}");
        }
        finally
        {
            _isExecutingUndoRedo = false;
        }
    }

    /// <summary>
    /// 是否处于根节点
    /// </summary>
    public bool IsAtRoot => _dag.Current == _dag.Root;

    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public string GetCurrentStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== 撤销重做状态 ===");
        sb.AppendLine($"当前动作: {_dag.Current.Action.Description}");
        sb.AppendLine($"是否根节点: {IsAtRoot}");
        sb.AppendLine($"可撤销: {!IsAtRoot}");
        sb.AppendLine($"可重做: {_dag.Current.Children.Count > 0}");
        return sb.ToString();
    }

    /// <summary>
    /// 获取历史记录
    /// </summary>
    public List<string> GetHistory()
    {
        return _dag.History.Select(n => n.Action.Description).ToList();
    }

    /// <summary>
    /// 获取实体的完整历史
    /// </summary>
    public List<IAction> GetEntityHistory(ObjectId entityId)
    {
        var nodes = GetEntityActionNodes(entityId);
        return nodes.Select(n => n.Action).ToList();
    }

    /// <summary>
    /// 获取实体的创建动作
    /// </summary>
    public CreateEntityAction? GetEntityCreation(ObjectId entityId)
    {
        var history = GetEntityHistory(entityId);
        return history.OfType<CreateEntityAction>().FirstOrDefault();
    }

    /// <summary>
    /// 获取实体的删除动作
    /// </summary>
    public DeleteEntityAction? GetEntityDeletion(ObjectId entityId)
    {
        var history = GetEntityHistory(entityId);
        return history.OfType<DeleteEntityAction>().LastOrDefault();
    }

    /// <summary>
    /// 获取实体的所有修改动作
    /// </summary>
    public List<ModifyEntityAction> GetEntityModifications(ObjectId entityId)
    {
        var history = GetEntityHistory(entityId);
        return history.OfType<ModifyEntityAction>().ToList();
    }

    /// <summary>
    /// 获取实体是否被删除
    /// </summary>
    public bool IsEntityDeleted(ObjectId entityId)
    {
        var deletion = GetEntityDeletion(entityId);
        return deletion != null;
    }

    /// <summary>
    /// 获取实体的当前状态摘要
    /// </summary>
    public string GetEntityStatusSummary(ObjectId entityId)
    {
        var history = GetEntityHistory(entityId);
        if (history.Count == 0)
            return "实体未跟踪";

        var creation = history.OfType<CreateEntityAction>().FirstOrDefault();
        var modifications = history.OfType<ModifyEntityAction>().Count();
        var deletion = history.OfType<DeleteEntityAction>().LastOrDefault();

        var status = deletion != null ? "已删除" : "活跃";
        return $"实体 {entityId.Handle.Value}: {status}, 创建于 {creation?.Timestamp:HH:mm:ss}, 修改 {modifications} 次";
    }

    /// <summary>
    /// 获取DAG结构的可视化表示
    /// </summary>
    public string GetDAGVisualization()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== DAG 结构 ===");
        sb.AppendLine($"根节点: {_dag.Root.Action.Description}");
        sb.AppendLine($"当前节点: {_dag.Current.Action.Description}");
        sb.AppendLine($"当前深度: {_dag.Current.Depth}");
        sb.AppendLine($"总节点数: {_dag.History.Count}");

        if (_dag.Current == _dag.Root)
        {
            sb.AppendLine("当前处于根节点，无法继续撤销。");
        }
        else if (_dag.Current.Children.Count == 0)
        {
            sb.AppendLine("当前处于最新状态，无法继续重做。");
        }

        // 显示当前路径
        var path = new List<VersionNode>();
        var current = _dag.Current;
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();

        sb.AppendLine("\n当前路径:");
        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
            var marker = node == _dag.Current ? "-> " : "   ";
            sb.AppendLine($"{marker}[{i}] {node.Action.Description} ({node.Depth})");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 清空历史
    /// </summary>
    public void Clear()
    {
        _dbMonitor.Clear();
    }

    /// <summary>
    /// 获取是否正在执行撤销重做
    /// </summary>
    public bool IsExecutingUndoRedo
    {
        get => _isExecutingUndoRedo;
        set => _isExecutingUndoRedo = value;
    }


    #region 异步命令计数器
    /// <summary>
    /// 异步命令计数器
    /// </summary>
    Dictionary<string, int> AsyncCmds = new(StringComparer.OrdinalIgnoreCase);

    public int AsyncCmdsCount => AsyncCmds.Count;


    /// <summary>
    /// 如果含有就计数+1,否则添加
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>返回计数</returns>
    public int AsyncCmdsPush(string cmd)
    {
        if (AsyncCmds.TryGetValue(cmd, out var counter))
        {
            counter++;
            AsyncCmds[cmd] = counter;
            return counter;
        }
        else
        {
            AsyncCmds[cmd] = 1;
            return 1;
        }
    }

    /// <summary>
    /// 如果含有就计数-1,为0移除
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>返回true表示含有</returns>
    public bool AsyncCmdsPop(string cmd)
    {
        if (AsyncCmds.TryGetValue(cmd, out var counter))
        {
            counter--;
            if (counter == 0)
                AsyncCmds.Remove(cmd);
            else
                AsyncCmds[cmd] = counter;
            return true;
        }
        return false;
    }
    #endregion



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
            // 释放在位编辑处理器
            _inPlaceEditHandler?.Dispose();

            // 释放托管资源
            _dbMonitor.Dispose();
            _doc.CommandWillStart -= OnCommandWillStart;
            _doc.CommandEnded -= OnCommandEnded;
            _doc.CommandFailed -= OnCommandFailed;
            _doc.CommandCancelled -= OnCommandCancelled;
        }
    }
}