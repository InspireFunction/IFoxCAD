namespace IFoxCAD.Basal;

public static class Debugx
{
    /// <summary>
    /// cad命令切换: IFoxDebugx
    /// </summary>
    /// <param name="message"></param>
    public static void Printl(object message)
    {
        var flag = Environment.GetEnvironmentVariable("debugx", EnvironmentVariableTarget.User);
        if (flag == null || flag == "0")
            return;
        var str = message + $"::{DateTime.Now.ToLongDateString() + DateTime.Now.TimeOfDay}";
#if DEBUG
        System.Diagnostics.Debug.WriteLine(str);
#else
        System.Diagnostics.Trace.WriteLine(str);
#endif
    }
}