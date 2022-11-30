namespace IFoxCAD.Com;

/// <summary>
/// 后绑代码工具
/// </summary>
public static class LateBinding
{
    /// <summary>
    /// 从运行对象表 (ROT) 获取指定对象的运行实例
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static object GetInstance(string appName)
    {
        return Marshal.GetActiveObject(appName);
    }
    /// <summary>
    /// 创建实例
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static object CreateInstance(string appName)
    {
        return Activator.CreateInstance(Type.GetTypeFromProgID(appName));
    }
    /// <summary>
    /// 获取或创建实例
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static object GetOrCreateInstance(string appName)
    {
        try { return GetInstance(appName); }
        catch { return CreateInstance(appName); }
    }
    /// <summary>
    /// 释放实例
    /// </summary>
    /// <param name="obj"></param>
    public static void ReleaseInstance(this object obj)
    {
        Marshal.ReleaseComObject(obj);
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static object GetProperty(this object obj, string propName, params object[] parameter)
    {
        return obj.GetType().InvokeMember(propName,
            BindingFlags.GetProperty,
            null, obj, parameter);
    }
    /// <summary>
    /// 设置属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <param name="parameter"></param>
    public static void SetProperty(this object obj, string propName, params object[] parameter)
    {
        obj.GetType().InvokeMember(propName,
            BindingFlags.SetProperty,
            null, obj, parameter);
    }
    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="memberName"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static object Invoke(this object obj, string memberName, params object[] parameter)
    {
        return obj.GetType().InvokeMember(memberName,
            BindingFlags.Public | BindingFlags.InvokeMethod,
            null, obj, parameter);
    }
}