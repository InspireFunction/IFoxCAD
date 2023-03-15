namespace IFoxCAD.Cad;
#region 写入日志到不同的环境中
// https://zhuanlan.zhihu.com/p/338492989
public abstract class LogBase
{
    public abstract void DeleteLog();
    public abstract string[] ReadLog();
    public abstract void WriteLog(string message);
}
#endregion


#if false // 最简单的实现
public static class Log
{
    /// <summary>
    /// <a href="https://www.cnblogs.com/Tench/p/CSharpSimpleFileWriteLock.html">读写锁</a>
    /// <para>当资源处于写入模式时,其他线程写入需要等待本次写入结束之后才能继续写入</para>
    /// </summary>
    static readonly ReaderWriterLockSlim _logWriteLock = new();

    /// <summary>
    /// 日志文件完整路径
    /// </summary>
    static readonly string _logAddress;

    static Log()
    {
        // 微软回复:静态构造函数只会被调用一次,
        // 并且在它执行完成之前,任何其它线程都不能创建这个类的实例或使用这个类的静态成员
        // https://blog.csdn.net/weixin_34204722/article/details/90095812
        var sb = new StringBuilder();
        sb.Append(Environment.CurrentDirectory);
        sb.Append("\\ErrorLog");

        // 新建文件夹
        var path = sb.ToString();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path)
                     .Attributes = FileAttributes.Normal; // 设置文件夹属性为普通
        }

        sb.Append('\\');
        sb.Append(DateTime.Now.ToString("yy-MM-dd"));
        sb.Append(".log");
        _logAddress = sb.ToString();
    }


    /// <summary>
    /// 将异常打印到日志文件
    /// </summary>
    /// <param name="ex">异常</param>
    /// <param name="remarks">备注</param>
    /// <param name="printDebugWindow">DEBUG模式打印到vs输出窗口</param>
    public static string? WriteLog(this Exception? ex,
        string? remarks = null,
        bool printDebugWindow = true)
    {
        try
        {
            _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁

            var logtxt = new LogTxt(ex, remarks);
            // var logtxtJson = Newtonsoft.Json.JsonConvert.SerializeObject(logtxt, Formatting.Indented);
            var logtxtJson = logtxt.ToString();

            if (logtxtJson == null)
                return string.Empty;

            // 把异常信息输出到文件
            var sw = new StreamWriter(_logAddress, true/*当天日志文件存在就追加,否则就创建*/);
            sw.Write(logtxtJson);
            sw.Flush();
            sw.Close();
            sw.Dispose();

            if (printDebugWindow)
            {
                Debugx.Printl("错误日志: " + _logAddress);
                Debug.Write(logtxtJson);
                // Debugger.Break();
                // Debug.Assert(false, "终止进程");
            }
            return logtxtJson;
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }
}
#endif