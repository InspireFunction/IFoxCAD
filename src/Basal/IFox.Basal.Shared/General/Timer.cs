namespace IFoxCAD.Basal;

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

/// <summary>
/// 时间定时类
/// </summary>
public class Timer
{
    /// <summary>
    /// 时间单位枚举
    /// </summary>
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
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <exception cref="Win32Exception"></exception>
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
    /// <summary>
    /// 秒
    /// </summary>
    public double Second => _Second;
    /// <summary>
    /// 毫秒
    /// </summary>
    public double Millisecond => _Second * 1000.0;
    /// <summary>
    /// 微秒
    /// </summary>
    public double Microsecond => _Second * 1000000.0;
    /// <summary>
    /// 纳秒
    /// </summary>
    public double Nanosecond => _Second * 1000000000.0;
    /// <summary>
    /// 计算执行委托的时间
    /// </summary>
    /// <param name="action">要执行的委托</param>
    /// <param name="timeEnum">时间单位</param>
    /// <returns>执行委托的时间</returns>
    public static double RunTime(Action action,
        TimeEnum timeEnum = TimeEnum.Millisecond)
    {
        var nanoSecond = new Timer();
        nanoSecond.Start();
        action();
        nanoSecond.Stop();

        var time = 0.0;
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
        //string timeNameZn = "";
        //switch (timeEnum)
        //{
        //    case TimeEnum.Second:
        //    timeNameZn = " 秒";
        //    break;
        //    case TimeEnum.Millisecond:
        //    timeNameZn = " 毫秒";
        //    break;
        //    case TimeEnum.Microsecond:
        //    timeNameZn = " 微秒";
        //    break;
        //    case TimeEnum.Nanosecond:
        //    timeNameZn = " 纳秒";
        //    break;
        //}
        return time;
    }
}