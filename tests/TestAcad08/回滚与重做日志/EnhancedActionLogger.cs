namespace JoinBoxAcad;

/// <summary>
/// 增强的动作日志记录器
/// </summary>
public class EnhancedActionLogger : IDisposable
{
    private readonly Document _document;
    private readonly EnhancedDatabaseMonitor _dbMonitor;
    private readonly CommandEventMonitor _cmdMonitor;
    private readonly ActionDAG _dag = new();
    private bool _isExecutingUndoRedo = false;

    // 实体动作映射 - 跟踪每个实体的所有相关动作
    private readonly Dictionary<ObjectId, List<VersionNode>> _entityActionMap = new();

    public EnhancedActionLogger(Document document)
    {
        _document = document;
        _dbMonitor = new EnhancedDatabaseMonitor(document);
        _cmdMonitor = new CommandEventMonitor(document);
    }

    public void StartLogging()
    {
        _dbMonitor.StartMonitoring();
        _cmdMonitor.StartMonitoring();
        _document.CommandWillStart += OnCommandWillStart;
        _document.CommandEnded += OnCommandEnded;
        _document.CommandCancelled += OnCommandCancelled;
        _document.CommandFailed += OnCommandFailed;
    }

    public void StopLogging()
    {
        _dbMonitor.StopMonitoring();
        _cmdMonitor.StopMonitoring();
        _document.CommandWillStart -= OnCommandWillStart;
        _document.CommandEnded -= OnCommandEnded;
        _document.CommandCancelled -= OnCommandCancelled;
        _document.CommandFailed -= OnCommandFailed;
    }

    /// <summary>
    /// 命令即将开始
    /// </summary>
    private void OnCommandWillStart(object? sender, CommandEventArgs e)
    {
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        if (_isExecutingUndoRedo)
            return;

        // 开始新的命令上下文
        _dbMonitor.StartCommandContext(e.GlobalCommandName, []);
        Env.Printl($"[DEBUG] 命令开始: {e.GlobalCommandName}");
    }

    /// <summary>
    /// 命令结束
    /// </summary>
    private void OnCommandEnded(object? sender, CommandEventArgs e)
    {
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        if (_isExecutingUndoRedo)
            return;

        // 结束命令上下文并记录动作
        var context = _dbMonitor.EndCommandContext();
        if (context != null && context.Changes.Count > 0)
        {
            var action = new EnhancedCommandAction(e.GlobalCommandName, context.Parameters, context);
            LogAction(action);
            Env.Printl($"[DEBUG] 记录命令动作: {e.GlobalCommandName}, 变更数: {context.Changes.Count}");
        }
    }

    /// <summary>
    /// 命令被取消
    /// </summary>
    private void OnCommandCancelled(object? sender, CommandEventArgs e)
    {
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        if (_isExecutingUndoRedo)
            return;

        // 取消的命令不记录，只清理上下文
        _dbMonitor.Clear();
        Env.Printl($"[DEBUG] 命令取消: {e.GlobalCommandName}");
    }

    /// <summary>
    /// 命令失败
    /// </summary>
    private void OnCommandFailed(object? sender, CommandEventArgs e)
    {
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        if (_isExecutingUndoRedo)
            return;

        // 失败的命令不记录，只清理上下文
        _dbMonitor.Clear();
        Env.Printl($"[DEBUG] 命令失败: {e.GlobalCommandName}");
    }

    HashSet<string> excludedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "U", "UNDO", "MREDO", "REDO", "_U", "_UNDO", "_MREDO", "_REDO",
        "MYUNDO", "MYREDO", "SHOWHISTORY", "SHOWENTITYHISTORY", "SHOWCURRENTPOSITION", "SHOWDAGSTRUCTURE",
        "STARTLOGGING", "STOPLOGGING"
    };

    /// <summary>
    /// 是否应该排除命令
    /// </summary>
    private bool ShouldExcludeCommand(string commandName)
    {
        if (StringHelper.IsNullOrWhiteSpace(commandName))
            return true;
        return excludedCommands.Contains(commandName);
    }

    /// <summary>
    /// 记录动作
    /// </summary>
    public void LogAction(IAction action)
    {
        var node = _dag.AddAction(action);

        // 如果是实体动作，记录到实体动作映射中
        if (action is EntityAction entityAction)
        {
            RecordEntityAction(entityAction.DBObjectId, node);
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
        return new List<VersionNode>();
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
            var currentAction = _dag.Current.Action;
            Env.Printl($"\n开始撤销: {currentAction.Description}");

            // 获取逆向动作
            var inverseAction = currentAction.GetInverseAction();
            if (inverseAction != null)
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
            _isExecutingUndoRedo = false;
        }
    }

    /// <summary>
    /// 执行重做
    /// </summary>
    public void Redo()
    {
        if (_dag.Current.Children.Count == 0)
        {
            Env.Printl("\n没有可重做的操作。\n");
            return;
        }

        _isExecutingUndoRedo = true;
        try
        {
            // 切换到第一个子节点
            var nextNode = _dag.Current.Children[0];
            var action = nextNode.Action;

            Env.Printl($"\n开始重做: {action.Description}");
            action.Execute();

            _dag.Current = nextNode;
            Env.Printl("重做完成\n");
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

    public void Dispose()
    {
        StopLogging();
    }
}