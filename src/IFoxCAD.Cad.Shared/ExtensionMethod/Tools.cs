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
        Timer.TimeEnum timeEnum = Timer.TimeEnum.Millisecond)
    {
        double time = Timer.RunTime(() => {
            for (int i = 0; i < count; i++)
                action();
        }, timeEnum);

        string timeNameZn = "";
        switch (timeEnum)
        {
            case Timer.TimeEnum.Second:
            timeNameZn = " 秒";
            break;
            case Timer.TimeEnum.Millisecond:
            timeNameZn = " 毫秒";
            break;
            case Timer.TimeEnum.Microsecond:
            timeNameZn = " 微秒";
            break;
            case Timer.TimeEnum.Nanosecond:
            timeNameZn = " 纳秒";
            break;
        }

        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({timeNameZn})");
    }
}

/*
// 测试例子,同时验证两个计时器
var stopwatch = new Stopwatch();
Timer.RunTime(() => {
    stopwatch.Start();
    for (int i = 0; i < 10000000; i++)
        i++;
    stopwatch.Stop();
}, Timer.TimeEnum.Millisecond, "运行:");
Console.WriteLine("运行毫秒:" + stopwatch.ElapsedMilliseconds);
 */

public class Timer
{
    [Flags]
    public enum TimeEnum
    {
        /// <summary>
        /// 秒
        /// </summary>
        Second,
        /// <summary>
        /// 毫秒
        /// </summary>
        Millisecond,
        /// <summary>
        /// 微秒
        /// </summary>
        Microsecond,
        /// <summary>
        /// 纳秒
        /// </summary>
        Nanosecond,
    }

    [DllImport("Kernel32.dll")]
    static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    /// <summary>
    /// 这个函数会检索性能计数器的频率.
    /// 性能计数器的频率在系统启动时是固定的,并且在所有处理器上都是一致的
    /// 因此，只需在应用初始化时查询频率,即可缓存结果
    /// 在运行 Windows XP 或更高版本的系统上,该函数将始终成功,因此永远不会返回零
    /// </summary>
    /// <param name="lpFrequency"></param>
    /// <returns></returns>
    [DllImport("Kernel32.dll")]
    static extern bool QueryPerformanceFrequency(out long lpFrequency);

    long _startTime, _stopTime;
    readonly long _freq;

    public Timer()
    {
        _startTime = 0;
        _stopTime = 0;

        if (!QueryPerformanceFrequency(out _freq))
            throw new Win32Exception("不支持高性能计数器");
    }

    /// <summary>
    /// 开始计时器
    /// </summary>
    public void Start()
    {
        System.Threading.Thread.Sleep(0);
        QueryPerformanceCounter(out _startTime);
    }

    /// <summary>
    /// 停止计时器
    /// </summary>
    public void Stop()
    {
        QueryPerformanceCounter(out _stopTime);
        _Second = (double)(_stopTime - _startTime) / _freq;
    }
    double _Second = 0;

    // 返回计时器经过时间
    public double Second => _Second;
    public double Millisecond => _Second * 1000.0;
    public double Microsecond => _Second * 1000000.0;
    public double Nanosecond => _Second * 1000000000.0;

    public static double RunTime(Action action, TimeEnum timeEnum = TimeEnum.Millisecond, string? msg = null)
    {
        var nanoSecond = new Timer();
        nanoSecond.Start();
        action();
        nanoSecond.Stop();

        double time = 0;
        switch (timeEnum)
        {
            case TimeEnum.Second:
            time = nanoSecond.Second;
            break;
            case TimeEnum.Millisecond:
            time = nanoSecond.Millisecond;
            break;
            case TimeEnum.Microsecond:
            time = nanoSecond.Microsecond;
            break;
            case TimeEnum.Nanosecond:
            time = nanoSecond.Nanosecond;
            break;
        }
        if (msg != null)
        {
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
            Env.Print(msg + " " + time + timeNameZn);
        }
        return time;
    }
}