namespace JoinBoxAcad;

/// <summary>
/// 初始化日志系统 - 使用增强的撤销重做机制
/// </summary>
public class LoggerInitializer
{
    [IFoxInitialize]
    public void StartLogging(Document doc)
    {
        // 禁用原生撤销/重做
        Acap.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;

        // 使用增强撤销重做管理器初始化当前文档
        EnhancedUndoRedoManager.EnableEnhancedUndoRedo(doc);
        var logger = EnhancedUndoRedoManager.GetLogger(doc);

        Env.Printl($"CAD增强日志系统已启用");
        Env.Printl($"可用命令:");
        Env.Printl($"  MYUNDO - 增强撤销（优先逆命令，否则数据回滚）");
        Env.Printl($"  MYREDO - 增强重做");
        Env.Printl($"  SHOWHISTORY - 显示操作历史");
        Env.Printl($"  SHOWENTITYHISTORY - 显示实体历史");
        Env.Printl($"  SHOWCURRENTPOSITION - 显示当前位置");
        Env.Printl($"  SHOWDAGSTRUCTURE - 显示DAG结构");
        Env.Printl($"  ENABLEENHANCEDUNDO - 启用增强撤销重做");
        Env.Printl($"  DISABLEENHANCEDUNDO - 禁用增强撤销重做");
        Env.Printl($"{logger?.GetCurrentStatus()}\n");
        Env.Printl("提示: 新的增强机制支持智能逆向选择（逆命令优先，数据回滚兜底）\n");

        // 这些命令需要排除,避免循环执行
        EnhancedActionLogger.ExcludedCommands.Add(nameof(ShowHistory));
        EnhancedActionLogger.ExcludedCommands.Add(nameof(ShowEntityHistory));
        EnhancedActionLogger.ExcludedCommands.Add(nameof(ShowCurrentPosition));
        EnhancedActionLogger.ExcludedCommands.Add(nameof(ShowDAGStructure));
        EnhancedActionLogger.ExcludedCommands.Add(nameof(StartLogging));
        EnhancedActionLogger.ExcludedCommands.Add(nameof(StopLogging));
    }

    /// <summary>
    /// 文档创建事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        // 使用增强撤销重做管理器初始化当前文档
        EnhancedUndoRedoManager.EnableEnhancedUndoRedo(doc);
        var logger = EnhancedUndoRedoManager.GetLogger(doc);
    }

    /// <summary>
    /// 文档释放事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        EnhancedUndoRedoManager.DisableEnhancedUndoRedo(doc);
    }

    /// <summary>
    /// 否决原生的撤销命令
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (StringHelper.IsNullOrWhiteSpace(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;

        switch (e.GlobalCommandName.ToUpper())
        {
            case "U":
            case "UNDO":
            {
                // 屏蔽原生撤销,否则导致不知道官方撤销点.
                e.Veto();
                //e.Document?.SendStringToExecute(nameof(MyUndo) + "\n", false, false, false);
                //SendCommand(nameof(MyUndo) + " ", RunCmdFlag.AcedCommand);

                var doc = Application.DocumentManager.MdiActiveDocument;
                EnhancedUndoRedoManager.Undo(doc);

                // 消除计数释放
                // 这里不命令事件消除计数,而是在事件上,因为它比较特殊
                var logger = EnhancedUndoRedoManager.GetLogger(doc);
                logger?.AsyncCmdsPop("UNDO");
            }
            break;
            case "MREDO":
            case "REDO":
            {
                // 屏蔽原生重做
                e.Veto();
                // 模仿 _mredo 输入动作数目或 [全部(A)/上一个(L)]:
                // 直接执行方法,这样可以处理命令行参数,而不是发送命令
                var doc = e.Document;
                var ed = doc.Editor;

                var pko = new PromptKeywordOptions("\n输入动作数目或 ");
                // 添加可接受的关键字
                pko.Keywords.Add("a", "a", "全部(A)");  // 全部选项
                pko.Keywords.Add("l", "l", "上一个(L)");  // 上一个选项
                pko.Keywords.Default = "l";

                // 设置允许用户输入数字
                pko.AllowNone = true;
                pko.AllowArbitraryInput = true;

                var prompt = ed.GetKeywords(pko);
                if (prompt.Status != PromptStatus.OK)
                    return;
                var input = prompt.StringResult.Trim().ToUpper();
                // 处理命令行参数
                ProcessRedoInput(doc, ed, input);

                // 消除计数释放
                // 这里不命令事件消除计数,而是在事件上,因为它比较特殊
                var logger = EnhancedUndoRedoManager.GetLogger(doc);
                logger?.AsyncCmdsPop("REDO");
            }
            break;
        }
    }

    [CommandMethod(nameof(StopLogging))]
    public void StopLogging()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;

        // 清理当前文档的增强日志器
        EnhancedUndoRedoManager.DisableEnhancedUndoRedo(doc);

        // 启用原生撤销
        Acap.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
        Acap.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed -= DocumentManager_DocumentToBeDestroyed;

        Env.Printl("CAD日志系统已停止");
    }


    // 处理重做输入的辅助方法
    private void ProcessRedoInput(Document doc, Editor ed, string input)
    {
        if (input == "A" || input == "全部")
        {
            // 执行全部重做
            EnhancedUndoRedoManager.RedoAll(doc);
        }
        else if (input == "L" || input == "上一个" || StringHelper.IsNullOrWhiteSpace(input))
        {
            // 执行一次重做
            EnhancedUndoRedoManager.Redo(doc);
        }
        else if (int.TryParse(input, out int count) && count > 0)
        {
            // 执行指定次数的重做
            EnhancedUndoRedoManager.Redo(doc, count);
        }
    }

    [CommandMethod(nameof(ShowHistory))]
    public void ShowHistory()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        EnhancedUndoRedoManager.ShowHistory(doc);
    }

    [CommandMethod(nameof(ShowEntityHistory))]
    public void ShowEntityHistory()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;

        // 提示用户选择实体
        var options = new PromptEntityOptions("\n选择要查看历史的实体: ");
        var result = ed.GetEntity(options);

        if (result.Status != PromptStatus.OK)
            return;

        var entityId = result.ObjectId;
        var logger = EnhancedUndoRedoManager.GetLogger(doc);

        if (logger == null)
        {
            ed.WriteMessage("\n当前文档未初始化增强日志系统\n");
            return;
        }

        var history = logger.GetEntityHistory(entityId);

        ed.WriteMessage($"\n=== 实体 {entityId.Handle.Value} 的历史 ===\n");

        if (history.Count == 0)
        {
            ed.WriteMessage("该实体没有历史记录。\n");
            return;
        }

        foreach (var action in history)
        {
            ed.WriteMessage($"{action.Timestamp:HH:mm:ss} - {action.Description}\n");

            if (action is ModifyEntityAction modifyAction)
            {
                ed.WriteMessage($"  修改了 {modifyAction.FieldChanges.Count} 个属性\n");
            }
        }

        // 显示实体状态摘要
        var status = logger.GetEntityStatusSummary(entityId);
        ed.WriteMessage($"\n{status}\n");
    }

    [CommandMethod(nameof(ShowCurrentPosition))]
    public void ShowCurrentPosition()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        EnhancedUndoRedoManager.ShowStatus(doc);
    }

    [CommandMethod(nameof(ShowDAGStructure))]
    public void ShowDAGStructure()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;

        var logger = EnhancedUndoRedoManager.GetLogger(doc);
        if (logger == null)
        {
            ed.WriteMessage("\n当前文档未初始化增强日志系统\n");
            return;
        }

        try
        {
            var dagVisualization = logger.GetDAGVisualization();
            ed.WriteMessage($"\n{dagVisualization}\n");
        }
        catch
        {
            // 兼容模式 - 显示基本信息
            var history = logger.GetHistory();
            ed.WriteMessage($"\n=== DAG 结构 ===\n");
            ed.WriteMessage($"总操作数: {history.Count}\n");
            ed.WriteMessage($"当前位置: {(history.Count > 0 ? "最后操作" : "根节点")}\n");
        }
    }
}