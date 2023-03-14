namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.Threading;
using Exception = Exception;




public static class LogHelper
{
#pragma warning disable CA2211 // 非常量字段应当不可见
    /// <summary>
    /// 日志文件完整路径
    /// </summary>
    public static string? LogAddress;
    /// <summary>
    /// 输出错误信息到日志文件的开关
    /// </summary>
    public static bool FlagOutFile = false;
    /// <summary>
    /// 输出错误信息到vs输出窗口的开关
    /// </summary>
    public static bool FlagOutVsOutput = true;
#pragma warning restore CA2211 // 非常量字段应当不可见

    /// <summary>
    /// <a href="https://www.cnblogs.com/Tench/p/CSharpSimpleFileWriteLock.html">读写锁</a>
    /// <para>当资源处于写入模式时,其他线程写入需要等待本次写入结束之后才能继续写入</para>
    /// </summary>
    static readonly ReaderWriterLockSlim _logWriteLock = new();

    /// <summary>
    /// 提供给外部设置log文件保存路径
    /// </summary>
    /// <param name="newlogAddress">null就生成默认配置</param>
    public static void OptionFile(string? newlogAddress = null)
    {
        _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁
        try
        {
            LogAddress = newlogAddress;
            if (string.IsNullOrEmpty(LogAddress))
                LogAddress = GetDefaultOption(DateTime.Now.ToString("yy-MM-dd") + ".log");
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }

    /// <summary>
    /// 输入文件名,获取保存路径的完整路径
    /// </summary>
    /// <param name="fileName">文件名,null获取默认路径</param>
    /// <param name="createDirectory">创建路径</param>
    /// <returns>完整路径</returns>
    public static string GetDefaultOption(string fileName, bool createDirectory = true)
    {
        // 微软回复:静态构造函数只会被调用一次,
        // 并且在它执行完成之前,任何其它线程都不能创建这个类的实例或使用这个类的静态成员
        // https://blog.csdn.net/weixin_34204722/article/details/90095812
        var sb = new StringBuilder();
        sb.Append(Environment.CurrentDirectory);
        sb.Append("\\ErrorLog");

        // 新建文件夹
        if (createDirectory)
        {
            var path = sb.ToString();
            if (!Directory.Exists(path))
            {
                // 设置文件夹属性为普通
                Directory.CreateDirectory(path)
                         .Attributes = FileAttributes.Normal;
            }
        }
        sb.Append('\\');
        sb.Append(fileName);
        return sb.ToString();
    }

    public static string WriteLog(this string? message,
                                 LogTarget target = LogTarget.File)
    {
        if (message == null)
            return string.Empty;
        return LogAction(null, message, target);
    }

    public static string WriteLog(this Exception? exception,
                                  LogTarget target = LogTarget.File)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, null, target);
    }

    public static string WriteLog(this Exception? exception, string? message,
                                  LogTarget target = LogTarget.File)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, message, target);
    }


    /// <param name="ex">错误</param>
    /// <param name="message">备注信息</param>
    /// <param name="target">记录方式</param>
    static string LogAction(Exception? ex,
                            string? message,
                            LogTarget target)
    {
        if (ex == null && message == null)
            return string.Empty;

        if (LogAddress == null)
        {
            if (target == LogTarget.File ||
                target == LogTarget.FileNotException)
                OptionFile();
        }

        // 不写入错误
        if (target == LogTarget.FileNotException)
            ex = null;

        try
        {
            _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁

            var logtxt = new LogTxt(ex, message);
            // var logtxtJson = Newtonsoft.Json.JsonConvert.SerializeObject(logtxt, Formatting.Indented);
            var logtxtJson = logtxt?.ToString();
            if (logtxtJson == null)
                return string.Empty;

            if (FlagOutFile)
            {
                LogBase? logger;
                switch (target)
                {
                    case LogTarget.File:
                    logger = new FileLogger();
                    logger.WriteLog(logtxtJson);
                    break;
                    case LogTarget.FileNotException:
                    logger = new FileLogger();
                    logger.WriteLog(logtxtJson);
                    break;
                    case LogTarget.Database:
                    logger = new DBLogger();
                    logger.WriteLog(logtxtJson);
                    break;
                    case LogTarget.EventLog:
                    logger = new EventLogger();
                    logger.WriteLog(logtxtJson);
                    break;
                }
            }

            if (FlagOutVsOutput)
            {
                Debugx.Printl("错误日志: " + LogAddress);
                Debug.Write(logtxtJson);
            }
            return logtxtJson;
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }
}





