namespace IFoxCAD.Cad;

public static partial class CollectionEx
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
            action.Invoke(element);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        var i = 0;
        foreach (var element in source)
        {
            action.Invoke(i, element);
            i++;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, CtrlState> action)
    {
        CtrlState state = new();
        state.Start();
        foreach (var element in source)
        {
            action.Invoke(element, state);
            if (state.IsBreak)
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, CtrlState, int> action)
    {
        var i = 0;
        CtrlState state = new();
        state.Start();
        foreach (var element in source)
        {
            action.Invoke(element, state, i);
            if (state.IsBreak)
                break;
            i++;
        }
    }
}
