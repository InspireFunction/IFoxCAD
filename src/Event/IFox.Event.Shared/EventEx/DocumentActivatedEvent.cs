namespace IFoxCAD.Event;
internal static class DocumentActivatedEvent
{
    private static readonly Type returnType = typeof(void);
    private static readonly Type firstType = typeof(object);
    private static readonly Type secondType = typeof(DocumentCollectionEventArgs);
    private static readonly HashSet<EventMethodInfo> dic = new();
    internal static void Initlize(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (!type.IsClass)
                continue;
            foreach (var methodInfo in type.GetMethods())
            {
                foreach (Attribute att in methodInfo.GetCustomAttributes(typeof(DocumentActivatedAttribute), false))
                {
                    if (att is not DocumentActivatedAttribute targetAtt)
                        continue;
                    if (!methodInfo.IsStatic)
                        throw new ArgumentException($"标记{nameof(DocumentActivatedAttribute)}特性的方法{type.Name}.{methodInfo.Name},应为静态方法");
                    if (methodInfo.ReturnType != returnType)
                        throw new ArgumentException($"标记{nameof(DocumentActivatedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，返回值应为void");
                    var args = methodInfo.GetParameters();
                    if (args.Length > 2)
                        throw new ArgumentException($"标记{nameof(DocumentActivatedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，参数类型错误");


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
                        throw new ArgumentException($"标记{nameof(DocumentActivatedAttribute)}特性的方法{type.Name}.{methodInfo.Name}，参数类型错误");
                    dic.Add(new(methodInfo, ept.Value, targetAtt.Level));
                    break;
                }
            }
        }
        AddEvent();
    }

    internal static void AddEvent()
    {
        Acap.DocumentManager.DocumentActivated -= DocumentManager_DocumentActivated;
        Acap.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
    }

    internal static void RemoveEvent()
    {
        Acap.DocumentManager.DocumentActivated -= DocumentManager_DocumentActivated;
    }
    private static void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
    {
#if Debug
        if (!EventFactory.closeCheck.Invoke())
        {
            EventFactory.RemoveEvent(CadEvent.All);
            return;
        }
#endif
        foreach (var eventMethodInfo in dic.OrderByDescending(a => a.Level))
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
public class DocumentActivatedAttribute : Attribute
{
    /// <summary>s
    /// 系统变量修改时触发被标记的函数
    /// 返回值应为void
    /// 参数不大于2个且只能为object和DocumentCollectionEventArgs
    /// </summary>
    /// <param name="level">级别(越高越先触发)</param>
    public DocumentActivatedAttribute(int level = -1)
    {
        Level = level;
    }
    public int Level { get; }
}