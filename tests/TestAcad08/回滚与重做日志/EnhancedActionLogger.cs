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
            LogAction(action, context.CommandName);
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

        // TODO 为什么 BEDIT 然后撤回会发生 [DEBUG] 命令取消: BEDIT
        // 难道不是进入undo才对吗?
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
        // "PASTECLIP",   // 粘贴命令（系统级）
        // "_PASTECLIP"   // 命令行粘贴
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
            return;
        }

        // 使用指定的命令上下文
        var node2 = _dag.AddAction(action, commandContext);

        // 如果是实体动作，记录到实体动作映射中
        if (action is EntityAction entityAction2)
        {
            RecordEntityAction(entityAction2.DBObjectId, node2);
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

                // 在位编辑-保存在位-撤销,就会触发这里
                if (currentNode.CommandContext == "REFCLOSE")
                {
                    var last = currentNode.Actions.LastOrDefault();
                    if (last is InPlaceSaveAction inPlace)
                    {
                        Env.Printl($"[DEBUG] 处理 REFCLOSE 撤销: 节点ID={currentNode.Id.Substring(0, 8)}..., 动作数={currentNode.Actions.Count}");
                        inPlace.Execute();
                        // 切换到父节点
                        _dag.Current = currentNode.Parent;
                        Env.Printl($"撤销完成，当前节点GUID: {_dag.Current.Id}\n");
                        // 增加命令撤销计数
                        commandsUndone++;
                        Env.Printl($"[DEBUG] 完成 REFCLOSE 撤销处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }

                // TODO BEDIT 这里很有趣耶
                // 1,BEDIT(无修改任何)撤回,会发生命令取消事件: [DEBUG] 命令取消: BEDIT
                // 2,BEDIT(有修改/画了对象)撤回,会发生: UNDO.
                // 我倒是想让它发生undo啊??怎么修改?有属性?强行加入一次对象?
                // 弹出教程窗口造成?
                if (currentNode.CommandContext == "BEDIT")
                {

                }
                else
                {

                }


                if (currentNode.CommandContext == "BCLOSE")
                {
                    var last = currentNode.Actions.LastOrDefault();
                    if (last is InBlockEditSaveAction inPlace)
                    {
                        Env.Printl($"[DEBUG] 处理 BCLOSE 撤销: 节点ID={currentNode.Id.Substring(0, 8)}..., 动作数={currentNode.Actions.Count}");
                        inPlace.Execute();
                        // 切换到父节点
                        _dag.Current = currentNode.Parent;
                        Env.Printl($"撤销完成，当前节点GUID: {_dag.Current.Id}\n");
                        // 增加命令撤销计数
                        commandsUndone++;
                        Env.Printl($"[DEBUG] 完成 BCLOSE 撤销处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }



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

                // 切换到父节点
                _dag.Current = currentNode.Parent;
                Env.Printl($"撤销完成，当前节点GUID: {_dag.Current.Id}\n");

                // 增加命令撤销计数
                commandsUndone++;
            }

            if (executed > 0)
            {
                Env.Printl($"\n已完成 {executed} 个操作的撤销\n");
            }
        }
        catch (Exception ex)
        {
            Env.Printl($"[ERROR] 撤销失败: {ex.Message}");
            Debugger.Break();
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
            int commandsRedone = 0;

            while (commandsRedone < count && _dag.Current.Children.Count > 0)
            {
                // 切换到时间最新的子节点
                // 重做是用历史的上下文,不用逆命令
                var nextNode = _dag.Current.Children
                    .OrderByDescending(child => GetLatestActionTimestamp(child))
                    .First();

                Env.Printl($"开始重做节点: {nextNode.CommandContext}，包含 {nextNode.Actions.Count} 个动作");


                // 260128a 进入在位编辑器状态,发送异步命令,最后重做参数.
                // 虽然触发了面板,但是逻辑是成功的(可以用钩子点击面板)
                // 1,虽然回来到编辑器状态了,但是再次REDO它会再进入死循环,命令上下文毕竟没有改变过.
                // 因此发送前登记一个节点编号,然后再发送.
                if (nextNode.CommandContext == "REFEDIT")
                {
                    var last = nextNode.Actions.LastOrDefault();
                    if (last is InPlaceCreateAction inPlace)
                    {
                        Env.Printl($"[DEBUG] 处理 REFEDIT 重做: 节点ID={nextNode.Id.Substring(0, 8)}..., 动作数={nextNode.Actions.Count}");
                        inPlace.Execute();
                        // 更新当前节点到下一个节点
                        _dag.Current = nextNode;
                        Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
                        // 增加命令重做计数
                        commandsRedone++;
                        Env.Printl($"[DEBUG] 完成 REFEDIT 重做处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }

                if (nextNode.CommandContext == "REFCLOSE")
                {
                    var last = nextNode.Actions.LastOrDefault();
                    if (last is InPlaceSaveAction action)
                    {
                        Env.Printl($"[DEBUG] 处理 REFCLOSE 重做: 节点ID={nextNode.Id.Substring(0, 8)}..., 动作数={nextNode.Actions.Count}");

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

                        // 更新当前节点到下一个节点
                        _dag.Current = nextNode;
                        Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
                        // 增加命令重做计数
                        commandsRedone++;
                        Env.Printl($"[DEBUG] 完成 REFCLOSE 重做处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }

                if (nextNode.CommandContext == "BEDIT")
                {
                    var last = nextNode.Actions.LastOrDefault();
                    if (last is BlockEditCreateAction inPlace)
                    {
                        Env.Printl($"[DEBUG] 处理 BEDIT 重做: 节点ID={nextNode.Id.Substring(0, 8)}..., 动作数={nextNode.Actions.Count}");
                        inPlace.Execute();
                        // 更新当前节点到下一个节点
                        _dag.Current = nextNode;
                        Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
                        // 增加命令重做计数
                        commandsRedone++;
                        Env.Printl($"[DEBUG] 完成 BEDIT 重做处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }

                if (nextNode.CommandContext == "BCLOSE")
                {
                    var last = nextNode.Actions.LastOrDefault();
                    if (last is InBlockEditSaveAction action)
                    {
                        Env.Printl($"[DEBUG] 处理 BCLOSE 重做: 节点ID={nextNode.Id.Substring(0, 8)}..., 动作数={nextNode.Actions.Count}");

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

                        // 更新当前节点到下一个节点
                        _dag.Current = nextNode;
                        Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
                        // 增加命令重做计数
                        commandsRedone++;
                        Env.Printl($"[DEBUG] 完成 BCLOSE 重做处理，当前节点: {_dag.Current.CommandContext}");
                        return;
                    }
                }

                // 执行该节点的所有动作
                foreach (var action in nextNode.Actions)
                {
                    Env.Printl($"  重做动作: {action.Description}");
                    action.Execute();
                    executed++;
                }

                _dag.Current = nextNode;
                Env.Printl($"重做完成，当前节点GUID: {nextNode.Id}");
                commandsRedone++;
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

    public void RedoAll()
    {
        // 计算剩余可重做的节点数量
        var remainingCount = _dag.Current.Children.Count;
        if (remainingCount > 0)
        {
            Redo(remainingCount); // 调用Redo方法执行全部重做
        }
        else
        {
            Env.Printl("\n没有可重做的操作。\n");
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

    /// <summary>
    /// 获取节点中最新的动作时间戳
    /// </summary>
    /// <param name="node">版本节点</param>
    /// <returns>最新动作的时间戳</returns>
    private DateTime GetLatestActionTimestamp(VersionNode node)
    {
        if (node.Actions == null || node.Actions.Count == 0)
            return DateTime.MinValue;

        return node.Actions.Max(action => action is BaseAction baseAction ? baseAction.Timestamp : DateTime.MinValue);
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
            Env.Printl($"AsyncCmdsPush添加了 {cmd}");
            return counter;
        }
        else
        {
            AsyncCmds[cmd] = 1;
            Env.Printl($"AsyncCmdsPush=1 {cmd}");
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
            {
                AsyncCmds.Remove(cmd);
                Env.Printl($"AsyncCmdsPop移除了 {cmd}");
            }
            else
            {
                AsyncCmds[cmd] = counter;
                Env.Printl($"AsyncCmdsPop减一 {cmd}");
            }
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