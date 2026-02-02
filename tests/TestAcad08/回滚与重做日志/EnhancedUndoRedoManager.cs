namespace JoinBoxAcad;

/// <summary>
/// 增强撤销重做管理器 - 统一的管理器，消除重复存储
/// </summary>
public class EnhancedUndoRedoManager
{
    // 文档,动作日志记录器
    private static Dictionary<Document, EnhancedActionLogger> _loggers = [];

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
    /// 执行多次重做操作
    /// </summary>
    /// <param name="doc">文档</param>
    /// <param name="count">重做次数</param>
    public static void Redo(Document doc, int count = 1)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.Redo(count);
        }
        else
        {
            Env.Printl("文档未启用增强撤销重做功能");
        }
    }

    /// <summary>
    /// 执行全部重做操作
    /// </summary>
    /// <param name="doc">文档</param>
    public static void RedoAll(Document doc)
    {
        if (_loggers.TryGetValue(doc, out var logger))
        {
            logger.RedoAll();
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