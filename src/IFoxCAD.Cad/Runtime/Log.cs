namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

#region 写入日志到不同的环境中
//https://zhuanlan.zhihu.com/p/338492989
public abstract class LogBase
{
    public abstract void ReadLog(string message);
    public abstract void WriteLog(string message);
    public abstract void DeleteLog(string message);
}

/// <summary>
/// 日志输出环境
/// </summary>
public enum LogTarget
{
    /// <summary>
    /// 文件
    /// </summary>
    File = 1,
    /// <summary>
    /// 数据库
    /// </summary>
    Database = 2,
    /// <summary>
    /// windows日志
    /// </summary>
    EventLog = 4,
}

/// <summary>
/// 写入到文件中
/// </summary>
public class FileLogger : LogBase
{
    public override void DeleteLog(string message)
    {
        throw new NotImplementedException();
    }

    public override void ReadLog(string message)
    {
        throw new NotImplementedException();
    }

    public override void WriteLog(string? message)
    {
        //把异常信息输出到文件
        var sw = new StreamWriter(LogHelper.LogAddress, true/*当天日志文件存在就追加,否则就创建*/);
        sw.Write(message);
        sw.Flush();
        sw.Close();
        sw.Dispose();
    }
}

/// <summary>
/// 写入到数据库(暂时不支持)
/// </summary>
public class DBLogger : LogBase
{
    public override void DeleteLog(string message)
    {
        throw new NotImplementedException();
    }

    public override void ReadLog(string message)
    {
        throw new NotImplementedException();
    }

    public override void WriteLog(string? message)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 写入到win日志
/// </summary>
public class EventLogger : LogBase
{
    // https://docs.microsoft.com/en-us/answers/questions/526018/windows-event-log-with-net-5.html
    // net50要加 <FrameworkReference Include="Microsoft.WindowsDesktop.App" />
    // 需要win权限

    public string LogName = "IFoxCadLog";
    public override void DeleteLog(string message)
    {
#if !NET5_0 && !NET6_0
        if (EventLog.Exists(LogName))
            EventLog.Delete(LogName);
#endif
    }

    public override void ReadLog(string message)
    {
#if !NET5_0 && !NET6_0
        EventLog eventLog = new();
        eventLog.Log = LogName;
        foreach (EventLogEntry entry in eventLog.Entries)
        {
            //Write your custom code here
        }
#endif
    }

    public override void WriteLog(string? message)
    {
#if !NET5_0 && !NET6_0
        try
        {
            EventLog eventLog = new();
            eventLog.Source = LogName;
            eventLog.WriteEntry(message, EventLogEntryType.Information);
        }
        catch (System.Security.SecurityException e)
        {
            throw new Exception("您没有权限写入win日志中");
        }
#endif
    }
}

#endregion

#region 静态方法
public static class LogHelper
{
    /// <summary>
    /// <a href="https://www.cnblogs.com/Tench/p/CSharpSimpleFileWriteLock.html">读写锁</a>
    /// <para>当资源处于写入模式时,其他线程写入需要等待本次写入结束之后才能继续写入</para>
    /// </summary>
    static readonly ReaderWriterLockSlim _logWriteLock = new();

    /// <summary>
    /// 日志文件完整路径
    /// </summary>
#pragma warning disable CA2211 // 非常量字段应当不可见
    public static string? LogAddress;
#pragma warning restore CA2211 // 非常量字段应当不可见

    /// <summary>
    /// 提供给外部设置log文件保存路径
    /// </summary>
    /// <param name="newlogAddress">空的话就为运行的dll旁边的一个文件夹上</param>
    public static void OptionFile(string? newlogAddress = null)
    {
        _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁
        try
        {
            LogAddress = newlogAddress;
            if (string.IsNullOrEmpty(LogAddress))
            {
                //微软回复:静态构造函数只会被调用一次,
                //并且在它执行完成之前,任何其它线程都不能创建这个类的实例或使用这个类的静态成员
                //https://blog.csdn.net/weixin_34204722/article/details/90095812
                var sb = new StringBuilder();
                sb.Append(Environment.CurrentDirectory);
                sb.Append("\\ErrorLog");

                //新建文件夹
                var path = sb.ToString();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path)
                             .Attributes = FileAttributes.Normal; //设置文件夹属性为普通
                }

                sb.Append('\\');
                sb.Append(DateTime.Now.ToString("yy-MM-dd"));
                sb.Append(".log");
                LogAddress = sb.ToString();
            }
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }

    public static string WriteLog(this string? message,
                                LogTarget target = LogTarget.File,
                                bool printDebugWindow = true)
    {
        if (message == null)
            return string.Empty;
        return LogAction(null, message, target, printDebugWindow);
    }

    public static string WriteLog(this Exception? exception,
                                LogTarget target = LogTarget.File,
                                bool printDebugWindow = true)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, null, target, printDebugWindow);
    }

    public static string WriteLog(this Exception? exception,
                                string? message,
                                LogTarget target = LogTarget.File,
                                bool printDebugWindow = true)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, message, target, printDebugWindow);
    }



    static string LogAction(Exception? ex,
                          string? message,
                          LogTarget target,
                          bool printDebugWindow)
    {
        if (ex == null && message == null)
            return string.Empty;

        if (target == LogTarget.File && LogAddress == null)
            OptionFile();

        try
        {
            _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁

            var logtxt = new LogTxt(ex, message);
            //var logtxtJson = Newtonsoft.Json.JsonConvert.SerializeObject(logtxt, Formatting.Indented);
            var logtxtJson = logtxt?.ToString();
            if (logtxtJson == null)
                return string.Empty;

            LogBase? logger;
            switch (target)
            {
                case LogTarget.File:
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

            if (printDebugWindow)
            {
                Debug.WriteLine("错误日志: " + LogAddress);
                Debug.Write(logtxtJson);
                //Debugger.Break(); 
                //Debug.Assert(false, "终止进程");
            }
            return logtxtJson;
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }
}
#endregion

#region 序列化
[Serializable]
public class LogTxt
{
    public string 当前时间;
    public string? 备注信息;
    public string? 异常信息;
    public string? 异常对象;
    public string? 触发方法;
    public string? 调用堆栈;

    LogTxt()
    {
        // 以不同语言显示日期
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("es-ES"))
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("zh-cn"))
        // 为了最小信息熵,所以用这样的格式,并且我喜欢补0
        当前时间 = DateTime.Now.ToString("yy-MM-dd hh:mm:ss");
    }

    public LogTxt(Exception? ex, string? message) : this()
    {
        if (ex == null && message == null)
            throw new ArgumentNullException(nameof(ex));

        if (ex != null)
        {
            异常信息 = ex.Message;
            异常对象 = ex.Source;
            触发方法 = ex.TargetSite == null ? string.Empty : ex.TargetSite.ToString();
            调用堆栈 = ex.StackTrace == null ? string.Empty : ex.StackTrace.Trim();
        }
        if (message != null)
            备注信息 = message;
    }

    /// 为了不引入json的dll,所以这里自己构造
    public override string? ToString()
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append(Environment.NewLine);
        sb.AppendLine($"  \"{nameof(当前时间)}\": \"{当前时间}\"");
        sb.AppendLine($"  \"{nameof(备注信息)}\": \"{备注信息}\"");
        sb.AppendLine($"  \"{nameof(异常信息)}\": \"{异常信息}\"");
        sb.AppendLine($"  \"{nameof(异常对象)}\": \"{异常对象}\"");
        sb.AppendLine($"  \"{nameof(触发方法)}\": \"{触发方法}\"");
        sb.AppendLine($"  \"{nameof(调用堆栈)}\": \"{调用堆栈}\"");
        sb.Append('}');
        return sb.ToString();
    }
}
#endregion


#if false //最简单的实现
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
        //微软回复:静态构造函数只会被调用一次,
        //并且在它执行完成之前,任何其它线程都不能创建这个类的实例或使用这个类的静态成员
        //https://blog.csdn.net/weixin_34204722/article/details/90095812
        var sb = new StringBuilder();
        sb.Append(Environment.CurrentDirectory);
        sb.Append("\\ErrorLog");

        //新建文件夹
        var path = sb.ToString();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path)
                     .Attributes = FileAttributes.Normal; //设置文件夹属性为普通
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
            //var logtxtJson = Newtonsoft.Json.JsonConvert.SerializeObject(logtxt, Formatting.Indented);
            var logtxtJson = logtxt.ToString();

            if (logtxtJson == null)
                return string.Empty;

            //把异常信息输出到文件
            var sw = new StreamWriter(_logAddress, true/*当天日志文件存在就追加,否则就创建*/);
            sw.Write(logtxtJson);
            sw.Flush();
            sw.Close();
            sw.Dispose();

            if (printDebugWindow)
            {
                Debug.WriteLine("错误日志: " + _logAddress);
                Debug.Write(logtxtJson);
                //Debugger.Break(); 
                //Debug.Assert(false, "终止进程");
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