using System.Diagnostics;
using System.Threading;

namespace IFoxCAD.Basal;

public static class Debugx
{
    /// <summary>
    /// cad命令切换: Debugx
    /// </summary>
    /// <param name="message">打印信息</param>
    /// <param name="time">打印时间</param>
    [DebuggerHidden]
    public static void Printl(object message, bool time = true)
    {
        var flag = Environment.GetEnvironmentVariable("debugx", EnvironmentVariableTarget.User);
        if (flag == null || flag == "0")
            return;

        if (time)
            //message = $"{DateTime.Now.ToLongDateString() + DateTime.Now.TimeOfDay}\n" +
            message = $"{DateTime.Now.TimeOfDay} ThreadId:{Thread.CurrentThread.ManagedThreadId}\n" +
            $"\t\t{message}";

        //System.Diagnostics.Debug.Indent();
#if DEBUG
        System.Diagnostics.Debug.WriteLine(message);
#else
        System.Diagnostics.Trace.WriteLine(message);
#endif
        //System.Diagnostics.Debug.Unindent();
    }
}