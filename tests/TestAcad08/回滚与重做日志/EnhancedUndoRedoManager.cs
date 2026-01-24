namespace JoinBoxAcad;

/// <summary>
/// 增强撤销重做管理器 - 统一的管理器，消除重复存储
/// </summary>
public class EnhancedUndoRedoManager
{
    // 统一使用这一个字典，消除 DocumentLoggers 的重复存储
    private static Dictionary<Document, EnhancedActionLogger> _loggers = new();

    /// <summary>
    /// 为文档启用增强的撤销重做功能
    /// </summary>
    public static void EnableEnhancedUndoRedo(Document doc)
    {
        if (_loggers.ContainsKey(doc))
        {
            Env.Printl("文档已启用增强撤销重做功能");
            return;
        }

        var logger = new EnhancedActionLogger(doc);
        logger.StartLogging();
        _loggers[doc] = logger;

        Env.Printl($"已为文档启用增强撤销重做功能: {doc.Name}");
    }

    /// <summary>
    /// 为文档禁用增强的撤销重做功能
    /// </summary>
    public static void DisableEnhancedUndoRedo(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.StopLogging();
            logger.Dispose();
            _loggers.Remove(doc);

            Env.Printl($"已禁用文档的增强撤销重做功能: {doc.Name}");
        }
    }

    /// <summary>
    /// 执行撤销操作
    /// </summary>
    public static void Undo(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.Undo();
        }
        else
        {
            Env.Printl("文档未启用增强撤销重做功能");
        }
    }

    /// <summary>
    /// 执行重做操作
    /// </summary>
    public static void Redo(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.Redo();
        }
        else
        {
            Env.Printl("文档未启用增强撤销重做功能");
        }
    }

    /// <summary>
    /// 显示当前状态
    /// </summary>
    public static void ShowStatus(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            Env.Printl(logger.GetCurrentStatus());
        }
        else
        {
            Env.Printl("文档未启用增强撤销重做功能");
        }
    }

    /// <summary>
    /// 显示历史记录
    /// </summary>
    public static void ShowHistory(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            var history = logger.GetHistory();
            Env.Printl("=== 操作历史 ===");
            for (int i = 0; i < history.Count; i++)
            {
                var marker = i == 0 ? "-> " : "   ";
                Env.Printl($"{marker}[{i}] {history[i]}");
            }
        }
        else
        {
            Env.Printl("文档未启用增强撤销重做功能");
        }
    }

    /// <summary>
    /// 获取指定文档的日志器（内部使用）
    /// </summary>
    internal static EnhancedActionLogger? GetLogger(Document doc)
    {
        return _loggers.TryGetValue(doc, out var logger) ? logger : null;
    }

    /// <summary>
    /// 清空历史记录
    /// </summary>
    public static void ClearHistory(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.Clear();
            Env.Printl("已清空历史记录");
        }
    }
}

/// <summary>
/// 增强撤销重做命令
/// </summary>
public class EnhancedUndoRedoCommands
{
    [CommandMethod("MYENHANCEDUNDO")]
    public void MyEnhancedUndo()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.Undo(doc);
        }
    }

    [CommandMethod("MYENHANCEDREDO")]
    public void MyEnhancedRedo()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.Redo(doc);
        }
    }

    [CommandMethod("SHOWENHANCEDSTATUS")]
    public void ShowEnhancedStatus()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.ShowStatus(doc);
        }
    }

    [CommandMethod("SHOWENHANCEDHISTORY")]
    public void ShowEnhancedHistory()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.ShowHistory(doc);
        }
    }

    [CommandMethod("CLEARENHANCEDHISTORY")]
    public void ClearEnhancedHistory()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.ClearHistory(doc);
        }
    }

    [CommandMethod("ENABLEENHANCEDUNDO")]
    public void EnableEnhancedUndo()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.EnableEnhancedUndoRedo(doc);
        }
    }

    [CommandMethod("DISABLEENHANCEDUNDO")]
    public void DisableEnhancedUndo()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            EnhancedUndoRedoManager.DisableEnhancedUndoRedo(doc);
        }
    }
}