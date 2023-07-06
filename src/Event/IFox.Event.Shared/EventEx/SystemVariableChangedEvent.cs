namespace IFoxCAD.Event;
internal static class SystemVariableChangedEvent
{
    private static readonly Type returnType = typeof(void);
    private static readonly Type firstType = typeof(object);
    private static readonly Type secondType = typeof(SystemVariableChangedEventArgs);
    private static readonly Dictionary<string, HashSet<EventMethodInfo>> dic = new();
    internal static void Initlize(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (!type.IsClass)
                continue;
            foreach (var methodInfo in type.GetMethods())
            {
                foreach (Attribute att in methodInfo.GetCustomAttributes(typeof(SystemVariableChangedAttribute), false))
                {
                    if (att is not SystemVariableChangedAttribute targetAtt)
                        continue;
                    if (!methodInfo.IsStatic)
                        throw new ArgumentException($"标记{nameof(SystemVariableChangedAttribute)}特性的方法{type.Name}.{methodInfo.Name},应为静态方法");
                    if (methodInfo.ReturnType != returnType)
                        throw new ArgumentException($"标记{nameof(SystemVariableChangedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，返回值应为void");
                    var args = methodInfo.GetParameters();
                    var key = targetAtt.Name.ToUpper();
                    if (!dic.ContainsKey(key))
                    {
                        dic.Add(key, new());
                    }
                    if (args.Length > 2)
                        throw new ArgumentException($"标记{nameof(SystemVariableChangedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，参数类型错误");


                    EventParameterType? ept = null;
                    if (args.Length == 0)
                        ept = EventParameterType.None;
                    else if (args.Length == 1)
                    {
                        if (args[0].ParameterType == firstType)
                            ept = EventParameterType.Object;
                        else if (args[0].ParameterType == secondType)
                            ept = EventParameterType.EventArgs;
                    }
                    else if (args.Length == 2 && args[0].ParameterType == firstType && args[1].ParameterType == secondType)
                    {
                        ept = EventParameterType.Complete;
                    }
                    if (ept is null)
                        throw new ArgumentException($"标记{nameof(SystemVariableChangedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，参数类型错误");
                    dic[key].Add(new(methodInfo, ept.Value, targetAtt.Level));
                }
            }
        }
        AddEvent();
    }
    internal static void AddEvent()
    {
        Acap.SystemVariableChanged -= Acap_SystemVariableChanged;
        Acap.SystemVariableChanged += Acap_SystemVariableChanged;
    }
    internal static void RemoveEvent()
    {
        Acap.SystemVariableChanged -= Acap_SystemVariableChanged;
    }
    private static void Acap_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
    {
        var key = e.Name.ToUpper();
        if (!dic.ContainsKey(key))
            return;

#if Debug
        if (!EventFactory.closeCheck.Invoke())
        {
            EventFactory.RemoveEvent(CadEvent.All);
            return;
        }
#endif

        foreach (var eventMethodInfo in dic[key].OrderByDescending(a => a.Level))
        {
            switch (eventMethodInfo.ParameterType)
            {
                case EventParameterType.None:
                    eventMethodInfo.Method.Invoke(null, new object[0]);
                    break;
                case EventParameterType.Object:
                    eventMethodInfo.Method.Invoke(null, new[] { sender });
                    break;
                case EventParameterType.EventArgs:
                    eventMethodInfo.Method.Invoke(null, new[] { e });
                    break;
                case EventParameterType.Complete:
                    eventMethodInfo.Method.Invoke(null, new[] { sender, e });
                    break;
            }
        }
    }

}
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class SystemVariableChangedAttribute : Attribute
{
    /// <summary>s
    /// 系统变量修改时触发被标记的函数\n
    /// \n返回值应为void
    /// 参数不大于2个且只能为object和SystemVariableChangedEventArgs
    /// </summary>
    /// <param name="name">系统变量名</param>
    /// <param name="level">级别(越高越先触发)</param>
    public SystemVariableChangedAttribute(string name, int level = -1)
    {
        Name = name;
        Level = level;
    }

    /// <summary>
    /// 系统变量名
    /// </summary>
    public string Name { get; }
    public int Level { get; }
}