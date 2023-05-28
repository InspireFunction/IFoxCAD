namespace IFoxCAD.Event;
public static class EventFactory
{
    /// <summary>
    /// 使用Cad事件
    /// </summary>
    /// <param name="cadEvent">事件枚举</param>
    /// <param name="assembly">程序集</param>
    public static void UseCadEvent(CadEvent cadEvent, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        IdleAction.Add(() =>
        {
            if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
            {
                SystemVariableChangedEvent.Initlize(assembly);
            }
            if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
            {
                DocumentLockModeChangedEvent.Initlize(assembly);
            }
            if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
            {
                BeginDoubleClickEvent.Initlize(assembly);
            }
        });
    }
    /// <summary>
    /// 临时关闭事件(需要添加枚举)
    /// </summary>
    /// <param name="cadEvent"></param>
    /// <returns></returns>
    public static EventTemporaryShutdownManager TemporaryShutdown(CadEvent cadEvent = CadEvent.All)
    {
        return new EventTemporaryShutdownManager(cadEvent);
    }
    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="cadEvent">事件枚举</param>
    public static void AddEvent(CadEvent cadEvent)
    {
        if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
        {
            SystemVariableChangedEvent.AddEvent();
        }
        if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
        {
            DocumentLockModeChangedEvent.AddEvent();
        }
        if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
        {
            BeginDoubleClickEvent.AddEvent();
        }
    }
    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="cadEvent">事件枚举</param>
    public static void RemoveEvent(CadEvent cadEvent)
    {
        if ((cadEvent & CadEvent.SystemVariableChanged) != 0)
        {
            SystemVariableChangedEvent.RemoveEvent();
        }
        if ((cadEvent & CadEvent.DocumentLockModeChanged) != 0)
        {
            DocumentLockModeChangedEvent.RemoveEvent();
        }
        if ((cadEvent & CadEvent.BeginDoubleClick) != 0)
        {
            BeginDoubleClickEvent.RemoveEvent();
        }
    }

    internal static Func<bool> closeCheck = () => true;
#if Debug
    /// <summary>
    /// 设置卸载全部事件的条件，仅在Debug环境使用，
    /// 方便动态加载多次覆盖时卸载掉之前的dll，具体卸载条件需要用户自理
    /// </summary>
    /// <param name="condition">条件</param>
    public static void SetCloseCondition(this Func<bool> condition)
    {
        closeCheck = condition;
    }
#endif
}