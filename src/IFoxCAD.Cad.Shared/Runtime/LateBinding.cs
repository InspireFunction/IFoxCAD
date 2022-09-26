namespace IFoxCAD.Cad;

/// <summary>
/// 后绑代码工具
/// </summary>
public static class LateBinding
{
    public static object GetInstance(string appName)
    {
        return Marshal.GetActiveObject(appName);
    }
    public static object CreateInstance(string appName)
    {
        return Activator.CreateInstance(Type.GetTypeFromProgID(appName));
    }
    public static object GetOrCreateInstance(string appName)
    {
        try { return GetInstance(appName); }
        catch { return CreateInstance(appName); }
    }
    public static void ReleaseInstance(this object obj)
    {
        Marshal.ReleaseComObject(obj);
    }

    public static object GetProperty(this object obj, string propName, params object[] parameter)
    {
        return obj.GetType().InvokeMember(propName,
            BindingFlags.GetProperty,
            null, obj, parameter);
    }
    public static void SetProperty(this object obj, string propName, params object[] parameter)
    {
        obj.GetType().InvokeMember(propName,
            BindingFlags.SetProperty,
            null, obj, parameter);
    }
    public static object Invoke(this object obj, string memberName, params object[] parameter)
    {
        return obj.GetType().InvokeMember(memberName,
            BindingFlags.Public | BindingFlags.InvokeMethod,
            null, obj, parameter);
    }
}