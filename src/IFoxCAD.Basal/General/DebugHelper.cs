namespace IFoxCAD.Basal;

public static class Debugx
{
    public static void Printl(object message)
    {
        var str = message + $"::{DateTime.Now.ToLongDateString() + DateTime.Now.TimeOfDay}";
#if DEBUG
        System.Diagnostics.Debug.WriteLine(str);
#else
        System.Diagnostics.Trace.WriteLine(str);
#endif
    }
}