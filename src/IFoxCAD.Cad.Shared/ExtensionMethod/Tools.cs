using static IFoxCAD.Basal.Timer;

namespace IFoxCAD.Cad;

public static class Tools
{
    /// <summary>
    /// 计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes2(int count, string message, Action action)
    {
        System.Diagnostics.Stopwatch watch = new();
        watch.Start();  // 开始监视代码运行时间
        for (int i = 0; i < count; i++)
            action.Invoke();// 需要测试的代码
        watch.Stop();  // 停止监视
        TimeSpan timespan = watch.Elapsed; // 获取当前实例测量得出的总时间
        double time = timespan.TotalMilliseconds;
        string name = "毫秒";
        if (timespan.TotalMilliseconds > 1000)
        {
            time = timespan.TotalSeconds;
            name = "秒";
        }
        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({name})");  // 总毫秒数
    }

    /// <summary>
    /// 纳秒计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes(int count, string message, Action action,
        TimeEnum timeEnum = TimeEnum.Millisecond)
    {
        var time = RunTime(() => {
            for (int i = 0; i < count; i++)
                action();
        }, timeEnum);

        string timeNameZn = "";
        switch (timeEnum)
        {
            case TimeEnum.Second:
            timeNameZn = " 秒";
            break;
            case TimeEnum.Millisecond:
            timeNameZn = " 毫秒";
            break;
            case TimeEnum.Microsecond:
            timeNameZn = " 微秒";
            break;
            case TimeEnum.Nanosecond:
            timeNameZn = " 纳秒";
            break;
        }

        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({timeNameZn})");
    }
}