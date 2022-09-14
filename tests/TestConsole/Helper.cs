namespace TestConsole;

public class LoopState
{
    const int PLS_NONE = 0;
    const int PLS_EXCEPTIONAL = 1;
    const int PLS_BROKEN = 2;
    const int PLS_STOPPED = 4;
    const int PLS_CANCELED = 8;

    private volatile int _LoopStateFlags = PLS_NONE;

    public bool IsRun => _LoopStateFlags == PLS_NONE;
    public bool IsCancel => _LoopStateFlags == PLS_CANCELED;
    public bool IsExceptional => _LoopStateFlags == PLS_EXCEPTIONAL;

    public bool IsBreak => (_LoopStateFlags & PLS_BROKEN) == PLS_BROKEN;
    public bool IsStop => (_LoopStateFlags & PLS_STOPPED) == PLS_STOPPED;
    public void Stop() => _LoopStateFlags = PLS_STOPPED;
    public void Break() => _LoopStateFlags = PLS_BROKEN;
}

public static class Helper
{
    public static void ForEach<T>(this IEnumerable<T> ints, Action<T, LoopState> action)
    {
        LoopState state = new();
        foreach (var item in ints)
        {
            action(item, state);
            if (state.IsBreak)
                break;
        }

        //int forNum = 5;
        //var result = Parallel.For(0, forNum, (int i, ParallelLoopState pls) => {
        //    if (i > 2)
        //        pls.Break();
        //    Task.Delay(10).Wait();
        //});
    }
}