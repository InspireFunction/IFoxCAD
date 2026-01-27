using System;
using System.Windows.Controls;

namespace JoinBoxAcad;

/// <summary>
/// 增强的动作日志记录器
/// </summary>
public class EnhancedActionLogger : IDisposable
{
    private readonly Document _doc;
    private readonly EnhancedDatabaseMonitor _dbMonitor;
    private readonly LongTransaction _inPlaceEditHandler;
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

        EndCommandContext("无命令");
        StartCommandContext(e.GlobalCommandName);
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
        EndCommandContext(e.GlobalCommandName);
        StartCommandContext("无命令");
    }

    /// <summary>
    /// 开始上下文
    /// </summary>
    /// <param name="cmd"></param>
    private void StartCommandContext(string cmd)
    {
        _dbMonitor.StartCommandContext(cmd, []);
        DebugEx.Printl($"[DEBUG] >>>>开始录制 {cmd} 命令期间");
    }

    /// <summary>
    /// 结束上下文
    /// </summary>
    /// <param name="cmd"></param>
    private void EndCommandContext(string cmd)
    {
        // 结束有命令上下文并记录动作
        var context = _dbMonitor.EndCommandContext();
        if (context is null)
            return;
        //Env.Printl($"[DEBUG] 结束录制命令期间 无变化");

        if (context.Changes.Count > 0)
        {
            var action = new EnhancedCommandAction(context);
            LogAction(action);
            DebugEx.Printl($"[DEBUG] <<<<结束录制 {cmd} 命令期间的动作, 变更数: {context.Changes.Count}");
        }
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
        LogAction(action, "无命令");
    }

    /// <summary>
    /// 记录动作（带命令上下文）
    /// </summary>
    /// <param name="action">动作</param>
    /// <param name="commandContext">命令上下文</param>
    public void LogAction(IAction action, string commandContext)
    {
        // 检查是否正在执行undo/redo操作
        if (_isExecutingUndoRedo)
            return;

        // 如果是EnhancedCommandAction，将每个数据库变更作为独立动作添加
        if (action is EnhancedCommandAction commandAction)
        {
            // 使用动作自身的命令上下文，而不是从dbMonitor获取
            // 因为此时dbMonitor的上下文可能已经被清除（在命令结束时）
            string actionCommandContext = commandAction.Context.CommandName;

            // 将数据库变更转换为独立动作
            foreach (var change in commandAction.Context.Changes)
            {
                IAction? entityAction = null;
                switch (change.Type)
                {
                    case ActionType.DatabaseAdd:
                    entityAction = new CreateEntityAction(change.EntityId);
                    break;
                    case ActionType.DatabaseDelete:
                    entityAction = new DeleteEntityAction(change.EntityId);
                    break;
                    case ActionType.DatabaseModify:
                    if (change.FieldChanges.Count > 0)
                        entityAction = new ModifyEntityAction(change.EntityId, change.FieldChanges);
                    break;
                }
                if (entityAction != null)
                {
                    // 添加到DAG，传递命令上下文
                    var node = _dag.AddAction(entityAction, actionCommandContext);
                    // 记录实体动作映射
                    if (entityAction is EntityAction ea)
                    {
                        RecordEntityAction(ea.DBObjectId, node);
                    }
                }
            }
        }
        else
        {
            // 使用指定的命令上下文
            var node = _dag.AddAction(action, commandContext);

            // 如果是实体动作，记录到实体动作映射中
            if (action is EntityAction entityAction)
            {
                RecordEntityAction(entityAction.DBObjectId, node);
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
    public void Undo(int count = 1)
    {
        if (IsAtRoot)
        {
            Env.Printl("\n已经到达初始状态，无法继续撤销。\n");
            return;
        }

        // 这里要先结束 无命令 修改
        EndCommandContext("无命令");

        _isExecutingUndoRedo = true;

        try
        {
            int executed = 0;
            int commandsUndone = 0;

            while (commandsUndone < count && !IsAtRoot)
            {
                // 获取当前节点
                var currentNode = _dag.Current;

                // 打印回滚节点的命令
                Env.Printl($"开始撤销节点: {currentNode.CommandContext}，包含 {currentNode.Actions.Count} 个动作");

                // 如果有逆命令,直接执行,不需要处理数据了.
                var inverseCommand = CommandInverseMap.GetInverseCommand(currentNode.CommandContext);
                if (!string.IsNullOrEmpty(inverseCommand))
                {
                    // TODO 组块,在位编辑,撤回,撤回...仍然有错误
                    var logger = EnhancedUndoRedoManager.GetLogger(_doc);
                    logger?.AsyncCmdsPush(inverseCommand);
                    _doc?.SendStringToExecute($"{inverseCommand}\n", true, false, false);
                }
                else
                {
                    // 回退当前节点的所有动作，按逆序执行
                    var reversedActions = currentNode.Actions.ToList();
                    reversedActions.Reverse();
                    foreach (var action in reversedActions)
                    {
                        Env.Printl($"  撤销动作: {action.Description}");
                        var inverseAction = action.GetInverseAction();
                        if (inverseAction is not null)
                        {
                            Env.Printl($"  使用逆向动作: {inverseAction.Description}");
                            inverseAction.Execute();
                            executed++;
                        }
                        else
                        {
                            Env.Printl($"  [WARNING] 无法获取逆向动作: {action.Description}");
                        }
                    }
                }

                // 切换到父节点
                _dag.Current = currentNode.Parent;
                Env.Printl($"撤销完成，当前节点GUID: {_dag.Current.Id}\n");

                // 增加命令撤销计数
                commandsUndone++;

                // 如果当前节点不是"无命令"，则认为已经撤销了一个完整的命令
                if (currentNode.CommandContext != "无命令")
                {
                    // 检查是否还有连续的"无命令"节点需要一并撤销
                    while (!IsAtRoot && _dag.Current.CommandContext == "无命令")
                    {
                        var noCommandNode = _dag.Current;
                        Env.Printl($"开始撤销节点: {noCommandNode.CommandContext}，包含 {noCommandNode.Actions.Count} 个动作");

                        // 回退无命令节点的所有动作
                        var reversedNoCommandActions = noCommandNode.Actions.ToList();
                        reversedNoCommandActions.Reverse();
                        foreach (var action in reversedNoCommandActions)
                        {
                            Env.Printl($"  撤销动作: {action.Description}");
                            var inverseAction = action.GetInverseAction();
                            if (inverseAction is not null)
                            {
                                Env.Printl($"  使用逆向动作: {inverseAction.Description}");
                                inverseAction.Execute();
                                executed++;
                            }
                            else
                            {
                                Env.Printl($"  [WARNING] 无法获取逆向动作: {action.Description}");
                            }
                        }

                        // 切换到父节点
                        _dag.Current = noCommandNode.Parent;
                        Env.Printl($"撤销完成，当前节点GUID: {_dag.Current.Id}\n");
                    }
                }
            }

            if (executed > 0)
            {
                Env.Printl($"\n已完成 {executed} 个操作的撤销\n");
            }
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 撤销失败: {ex.Message}");
        }
        finally
        {
            // #260126a 使用了异步命令这里就不清理了,在命令结束后事件清理
            if (AsyncCmds.Count == 0)
            {
                _isExecutingUndoRedo = false;
                StartCommandContext("无命令");
            }
        }
    }



    /// <summary>
    /// 执行多次重做
    /// </summary>
    /// <param name="count">重做次数</param>
    public void Redo(int count = 1)
    {
        if (_dag.Current.Children.Count == 0)
        {
            Env.Printl("\n居然没有可重做的操作。\n");
            return;
        }

        EndCommandContext("无命令");

        _isExecutingUndoRedo = true;

        try
        {
            int executed = 0;
            for (int i = 0; i < count && _dag.Current.Children.Count > 0; i++)
            {
                // 切换到第一个子节点
                var nextNode = _dag.Current.Children[0];

                Env.Printl($"\n开始重做节点: {nextNode.CommandContext}，包含 {nextNode.Actions.Count} 个动作");

                // 执行该节点的所有动作
                foreach (var action in nextNode.Actions)
                {
                    Env.Printl($"  重做动作: {action.Description}");
                    action.Execute();
                    executed++;
                }

                _dag.Current = nextNode;
                Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
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
            // #260126a 使用了异步命令这里就不清理了,在命令结束后事件清理
            if (AsyncCmds.Count == 0)
            {
                _isExecutingUndoRedo = false;
                StartCommandContext("无命令");
            }
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

                Env.Printl($"\n开始重做节点: {nextNode.CommandContext}，包含 {nextNode.Actions.Count} 个动作");

                // 执行该节点的所有动作
                foreach (var action in nextNode.Actions)
                {
                    Env.Printl($"  重做动作: {action.Description}");
                    action.Execute();
                    executed++;
                }

                _dag.Current = nextNode;
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
        sb.AppendLine($"当前节点: {_dag.Current.CommandContext}");
        sb.AppendLine($"当前节点动作数: {_dag.Current.Actions.Count}");
        if (_dag.Current.Actions.Count > 0)
        {
            sb.AppendLine($"最近动作: {_dag.Current.LastAction.Description}");
        }
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
        return _dag.History.Select(n =>
            n.Actions.Count > 0 ?
            $"{n.CommandContext} - {n.Actions.Count}个动作" :
            n.CommandContext).ToList();
    }

    /// <summary>
    /// 获取实体的完整历史
    /// </summary>
    public List<IAction> GetEntityHistory(ObjectId entityId)
    {
        var nodes = GetEntityActionNodes(entityId);
        return nodes.SelectMany(n => n.Actions).ToList();
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
        sb.AppendLine($"根节点: {_dag.Root.LastAction.Description}");
        sb.AppendLine($"当前节点: {_dag.Current.CommandContext}");
        sb.AppendLine($"当前节点动作数: {_dag.Current.Actions.Count}");
        if (_dag.Current.Actions.Count > 0)
        {
            sb.AppendLine($"当前节点最近动作: {_dag.Current.LastAction.Description}");
        }
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
            var nodeDesc = node.Actions.Count > 0 ?
                $"{node.CommandContext} - {node.Actions.Count}个动作" :
                node.CommandContext;
            sb.AppendLine($"{marker}[{i}] {nodeDesc} ({node.Depth})");
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