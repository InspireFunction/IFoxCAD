namespace Test;

public static class Tools
{
    /// <summary>
    /// 计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes2(int count, string message, Action action)
    {
        System.Diagnostics.Stopwatch watch = new();
        watch.Start(); // 开始监视代码运行时间
        for (var i = 0; i < count; i++)
            action.Invoke(); // 需要测试的代码
        watch.Stop(); // 停止监视
        var timespan = watch.Elapsed; // 获取当前实例测量得出的总时间
        var time = timespan.TotalMilliseconds;
        var name = "毫秒";
        if (timespan.TotalMilliseconds > 1000)
        {
            time = timespan.TotalSeconds;
            name = "秒";
        }

        $"{message} 代码执行 {count} 次的时间：{time} ({name})".Print(); // 总毫秒数
    }

    /// <summary>
    /// 计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes3(int count, string message, Action<int> action)
    {
        System.Diagnostics.Stopwatch watch = new();
        watch.Start(); // 开始监视代码运行时间
        for (var i = 0; i < count; i++)
            action.Invoke(i); // 需要测试的代码
        watch.Stop(); // 停止监视
        var timespan = watch.Elapsed; // 获取当前实例测量得出的总时间
        var time = timespan.TotalMilliseconds;
        var name = "毫秒";
        if (timespan.TotalMilliseconds > 1000)
        {
            time = timespan.TotalSeconds;
            name = "秒";
        }

        $"{message} 代码执行 {count} 次的时间：{time} ({name})".Print(); // 总毫秒数
    }


    /// <summary>
    /// 纳秒计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes(int count, string message, Action action,
        Timer.TimeEnum timeEnum = Timer.TimeEnum.Millisecond)
    {
        var time = Timer.RunTime(() =>
        {
            for (var i = 0; i < count; i++)
                action.Invoke();
        }, timeEnum);

        var timeNameZn = timeEnum switch
        {
            Timer.TimeEnum.Millisecond => " 毫秒",
            Timer.TimeEnum.Microsecond => " 微秒",
            Timer.TimeEnum.Nanosecond => " 纳秒",
            _ => " 秒"
        };

        $"{message} 代码执行 {count} 次的时间：{time} ({timeNameZn})".Print();
    }
}