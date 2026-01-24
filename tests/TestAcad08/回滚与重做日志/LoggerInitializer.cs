using TestAcad08.回滚与重做日志;

namespace JoinBoxAcad;

/// <summary>
/// 初始化日志系统 - 使用增强的撤销重做机制
/// </summary>
public class LoggerInitializer
{
    [IFoxInitialize]
    public void StartLogging(Document doc)
    {
        // 禁用原生撤回/重做
        Acap.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;

        // 使用增强撤销重做管理器初始化当前文档
        EnhancedUndoRedoManager.EnableEnhancedUndoRedo(doc);
        var logger = EnhancedUndoRedoManager.GetLogger(doc);

        doc.Editor.WriteMessage($"\nCAD增强日志系统已启用");
        doc.Editor.WriteMessage($"\n可用命令:");
        doc.Editor.WriteMessage($"\n  MYUNDO - 增强撤销（优先逆命令，否则数据回滚）");
        doc.Editor.WriteMessage($"\n  MYREDO - 增强重做");
        doc.Editor.WriteMessage($"\n  SHOWHISTORY - 显示操作历史");
        doc.Editor.WriteMessage($"\n  SHOWENTITYHISTORY - 显示实体历史");
        doc.Editor.WriteMessage($"\n  SHOWCURRENTPOSITION - 显示当前位置");
        doc.Editor.WriteMessage($"\n  SHOWDAGSTRUCTURE - 显示DAG结构");
        doc.Editor.WriteMessage($"\n  ENABLEENHANCEDUNDO - 启用增强撤销重做");
        doc.Editor.WriteMessage($"\n  DISABLEENHANCEDUNDO - 禁用增强撤销重做");
        doc.Editor.WriteMessage($"\n{logger.GetCurrentStatus()}\n");
        doc.Editor.WriteMessage("提示: 新的增强机制支持智能逆向选择（逆命令优先，数据回滚兜底）\n");
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
    /// 否决原生的撤回命令
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (StringHelper.IsNullOrWhiteSpace(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;

        // 获取当前文档的日志器
        var logger = EnhancedUndoRedoManager.GetLogger(e.Document);
        if (logger == null)
            return;

        // 如果正在执行自定义撤销/重做，不处理
        if (logger.IsExecutingUndoRedo)
            return;

        switch (e.GlobalCommandName.ToUpper())
        {
            case "U":
            case "UNDO":
            {
                // 屏蔽原生撤回,否则导致不知道官方撤回点.
                e.Veto();
                e.Document?.SendStringToExecute(nameof(MyUndo) + "\n", false, false, false);
                //SendCommand(nameof(MyUndo) + " ", RunCmdFlag.AcedCommand);
            }
            break;
            case "MREDO":
            case "REDO":
            {
                // 屏蔽原生重做
                e.Veto();
                e.Document?.SendStringToExecute(nameof(MyRedo) + "\n", false, false, false);
                //SendCommand(nameof(MyRedo) + " ", RunCmdFlag.AcedCommand);
            }
            break;
        }
    }

    [CommandMethod("STOPLOGGING")]
    public void StopLogging()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;

        // 清理当前文档的增强日志器
        EnhancedUndoRedoManager.DisableEnhancedUndoRedo(doc);

        // 启用原生撤回
        Acap.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
        Acap.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed -= DocumentManager_DocumentToBeDestroyed;

        Env.Printl("CAD日志系统已停止");
    }

    [CommandMethod(nameof(MyUndo))]
    public void MyUndo()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        EnhancedUndoRedoManager.Undo(doc);
    }

    [CommandMethod(nameof(MyRedo))]
    public void MyRedo()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        EnhancedUndoRedoManager.Redo(doc);
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
                ed.WriteMessage($"  修改了 {modifyAction.PropertyChanges.Count} 个属性\n");
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