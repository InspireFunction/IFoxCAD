using System.Threading;

namespace TestAcad08.MCP;

/// <summary>
/// 命令执行器
/// 使用AcadIdleManager.OnIdle在空闲时执行命令
/// </summary>
public class CommandExecutor
{
    private readonly object _lockObj = new object();

    /// <summary>
    /// 执行CAD命令
    /// </summary>
    public CommandResult Execute(string command, int timeoutSeconds)
    {
        lock (_lockObj)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var doc = Acap.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return new CommandResult
                    {
                        Success = false,
                        Message = "No active document",
                        ExecutionTime = sw.ElapsedMilliseconds
                    };
                }

                // 解析命令和参数
                var parts = ParseCommand(command);
                if (parts.Count == 0)
                {
                    return new CommandResult
                    {
                        Success = false,
                        Message = "Empty command",
                        ExecutionTime = sw.ElapsedMilliseconds
                    };
                }

                // 在空闲事件中执行命令
                bool completed = false;
                Exception? execException = null;

                // 使用AcadIdleManager在空闲时执行
                AcadIdleManager.OnIdleOnce(() => {
                    try
                    {
                        // 发送命令到CAD
                        doc.SendStringToExecute(parts[0] + " ", true, false, true);

                        // 发送参数
                        for (int i = 1; i < parts.Count; i++)
                        {
                            doc.SendStringToExecute(parts[i] + " ", true, false, true);
                        }

                        completed = true;
                    }
                    catch (Exception ex)
                    {
                        execException = ex;
                        completed = true;
                    }
                });

                // 等待命令完成或超时
                int timeoutMs = timeoutSeconds * 1000;
                while (!completed && sw.ElapsedMilliseconds < timeoutMs)
                {
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                }

                sw.Stop();

                if (!completed)
                {
                    // 超时，取消订阅并发送ESC
                    SendEscape();
                    return new CommandResult
                    {
                        Success = false,
                        Message = "Command timeout",
                        ExecutionTime = sw.ElapsedMilliseconds,
                        Status = CommandStatus.Timeout
                    };
                }

                if (execException != null)
                {
                    return new CommandResult
                    {
                        Success = false,
                        Message = $"Command execution error: {execException.Message}",
                        ExecutionTime = sw.ElapsedMilliseconds,
                        Status = CommandStatus.Error
                    };
                }

                // 记录成功历史
                HistoryManager.AddEntry(new HistoryEntry
                {
                    Timestamp = DateTime.Now,
                    Command = command,
                    Success = true,
                    ExecutionTime = sw.ElapsedMilliseconds
                });

                return new CommandResult
                {
                    Success = true,
                    Message = "Command executed successfully",
                    ExecutionTime = sw.ElapsedMilliseconds,
                    Status = CommandStatus.Success
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new CommandResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ExecutionTime = sw.ElapsedMilliseconds,
                    Status = CommandStatus.Error
                };
            }
        }
    }

    /// <summary>
    /// 发送ESC键取消当前命令
    /// </summary>
    public void SendEscape()
    {
        try
        {
            var doc = Acap.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                // 使用空闲事件发送ESC
                AcadIdleManager.OnIdleOnce(() => {
                    doc.SendStringToExecute("\x1B", true, false, false);
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SendEscape error: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析命令字符串
    /// </summary>
    private List<string> ParseCommand(string command)
    {
        var parts = new List<string>();
        var current = "";
        bool inQuotes = false;

        foreach (char c in command)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current += c;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (!string.IsNullOrEmpty(current))
                {
                    parts.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            parts.Add(current);
        }

        return parts;
    }
}

/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public long ExecutionTime { get; set; }
    public CommandStatus Status { get; set; }
}

/// <summary>
/// 命令状态
/// </summary>
public enum CommandStatus
{
    Success,
    Error,
    Timeout,
    Cancelled
}
