namespace JoinBoxAcad;

/// <summary>
/// 命令事件监听
/// </summary>
public class CommandEventMonitor
{
    // 需要排除的命令列表（自定义命令不应被记录）
    private readonly HashSet<string> _excludedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        // 自定义日志命令
        nameof(LoggerInitializer.MyUndo),
        nameof(LoggerInitializer.MyRedo),
        nameof(LoggerInitializer.ShowHistory),
        nameof(LoggerInitializer.ShowEntityHistory),
        nameof(LoggerInitializer.ShowCurrentPosition),
        nameof(LoggerInitializer.ShowDAGStructure),
        nameof(LoggerInitializer.StartLogging),
        nameof(LoggerInitializer.StopLogging),
        
        // 系统命令（不应记录）
        "SHOWCURRENTPOSITION",
        "SHOWDAGSTRUCTURE",
        "SHOWENTITYHISTORY",
        "SHOWHISTORY",
        "MYUNDO",
        "MYREDO",
        "STARTLOGGING",
        "STOPLOGGING",
        
        // CAD内部命令（可能导致问题）
        "U",           // 原生撤销
        "UNDO",        // 原生撤销
        "MREDO",       // 原生重做
        "REDO",        // 原生重做
        "_U",          // 命令行撤销
        "_UNDO",       // 命令行撤销
        "_MREDO",      // 命令行重做
        "_REDO",       // 命令行重做
        "PASTECLIP",   // 粘贴命令（系统级）
        "_PASTECLIP"   // 命令行粘贴
    };

    readonly Document _doc;
    public CommandEventMonitor(Document doc)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
    }

    public void StartMonitoring()
    {
        _doc.CommandWillStart += OnCommandWillStart;
        _doc.CommandEnded += OnCommandEnded;
        _doc.CommandCancelled += OnCommandCancelled;
        _doc.CommandFailed += OnCommandFailed;
    }

    public void StopMonitoring()
    {
        _doc.CommandWillStart -= OnCommandWillStart;
        _doc.CommandEnded -= OnCommandEnded;
        _doc.CommandCancelled -= OnCommandCancelled;
        _doc.CommandFailed -= OnCommandFailed;
    }

    // 检查命令是否应该被排除
    private bool ShouldExcludeCommand(string commandName)
    {
        if (StringHelper.IsNullOrWhiteSpace(commandName))
            return true;

        // 排除空命令或只有特殊字符的命令
        if (commandName == "#" || commandName == ".")
            return true;

        return _excludedCommands.Contains(commandName);
    }

    private void OnCommandWillStart(object sender, CommandEventArgs e)
    {
        // 检查是否是系统命令或空命令
        if (StringHelper.IsNullOrWhiteSpace(e.GlobalCommandName))
            return;

        // 如果正在执行撤销/重做操作，不记录命令 
        var logger = EnhancedUndoRedoManager.GetLogger(_doc);
        if (logger == null)
            return;
        if (logger.IsExecutingUndoRedo)
            return;

        // 排除自定义命令，避免循环记录
        if (ShouldExcludeCommand(e.GlobalCommandName))
        {
            //Env.Printl($"[DEBUG] (命令开始)跳过排除的命令: {e.GlobalCommandName}");
            return;
        }

        // 记录命令开始，兼容新旧接口
        try
        {
            var method = logger.GetType().GetMethod("LogCommand");
            if (method != null)
            {
                method.Invoke(logger, new object[] { e.GlobalCommandName });
            }
        }
        catch
        {
            // 如果方法不存在，跳过记录
        }
    }

    private void OnCommandEnded(object sender, CommandEventArgs e)
    {
        // 检查是否是系统命令或空命令
        if (StringHelper.IsNullOrWhiteSpace(e.GlobalCommandName))
            return;

        // 如果正在执行撤销/重做操作，不记录命令
        var logger = EnhancedUndoRedoManager.GetLogger(_doc);
        if (logger == null)
            return;
        if (logger.IsExecutingUndoRedo)
            return;

        // 排除自定义命令，避免循环记录
        if (ShouldExcludeCommand(e.GlobalCommandName))
        {
            //Env.Printl($"[DEBUG] (命令结束)跳过排除的命令: {e.GlobalCommandName}");
            return;
        }

        // 记录命令结束，兼容新旧接口
        try
        {
            var method = logger.GetType().GetMethod("LogCommand");
            if (method != null)
            {
                method.Invoke(logger, new object[] { e.GlobalCommandName });
            }
        }
        catch
        {
            // 如果方法不存在，跳过记录
        }
    }

    private void OnCommandCancelled(object sender, CommandEventArgs e)
    {
        // 如果正在执行撤销/重做操作，不记录命令
        var logger = EnhancedUndoRedoManager.GetLogger(_doc);
        if (logger == null)
            return;
        if (logger.IsExecutingUndoRedo)
            return;

        // 排除自定义命令，避免循环记录
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 记录命令取消，兼容新旧接口
        try
        {
            var method = logger.GetType().GetMethod("LogCommand");
            if (method != null)
            {
                method.Invoke(logger, new object[] { e.GlobalCommandName });
            }
        }
        catch
        {
            // 如果方法不存在，跳过记录
        }
    }

    private void OnCommandFailed(object sender, CommandEventArgs e)
    {
        // 如果正在执行撤销/重做操作，不记录命令
        var logger = EnhancedUndoRedoManager.GetLogger(_doc);
        if (logger == null)
            return;
        if (logger.IsExecutingUndoRedo)
            return;

        // 排除自定义命令，避免循环记录
        if (ShouldExcludeCommand(e.GlobalCommandName))
            return;

        // 记录命令失败，兼容新旧接口
        try
        {
            var method = logger.GetType().GetMethod("LogCommand");
            if (method != null)
            {
                method.Invoke(logger, new object[] { e.GlobalCommandName });
            }
        }
        catch
        {
            // 如果方法不存在，跳过记录
        }
    }
}