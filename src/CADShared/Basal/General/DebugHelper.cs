namespace IFoxCAD.Basal;
/// <summary>
/// 调试工具
/// </summary>
public static class DebugEx
{
    /// <summary>
    /// cad命令切换: DebugEx
    /// </summary>
    /// <param name="message">打印信息</param>
    /// <param name="time">打印时间</param>
    [DebuggerHidden]
    public static void Printl(object message, bool time = true)
    {
        var flag = Environment.GetEnvironmentVariable("debugx", EnvironmentVariableTarget.User);
        if (flag is null or "0")
            return;

        if (time)
        {
            message = $"{DateTime.Now.TimeOfDay} ThreadId:{Thread.CurrentThread.ManagedThreadId}\n" + $"\t\t{message}";
        }
        //System.Diagnostics.Debug.Indent();
#if DEBUG
        Debug.WriteLine(message);
#else
        System.Diagnostics.Trace.WriteLine(message);
#endif
        //System.Diagnostics.Debug.Unindent();
    }
}