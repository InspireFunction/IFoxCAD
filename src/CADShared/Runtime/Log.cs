namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.Threading;
using Exception = Exception;

#region 写入日志到不同的环境中
// https://zhuanlan.zhihu.com/p/338492989
/// <summary>
/// 日志基类，提供日志操作的基本抽象。
/// </summary>
public abstract class LogBase
{
    /// <summary>
    /// 删除日志
    /// </summary>
    public abstract void DeleteLog();
    /// <summary>
    /// 读取日志
    /// </summary>
    /// <returns>日志内容数组</returns>
    public abstract string[] ReadLog();
    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="message">日志消息</param>
    public abstract void WriteLog(string message);
}

/// <summary>
/// 日志输出环境
/// </summary>
public enum LogTarget
{
    /// <summary>
    /// 文件(包含错误和备注)
    /// </summary>
    File = 1,
    /// <summary>
    /// 文件(不包含错误,也就是只写备注信息)
    /// </summary>
    FileNotException = 2,
    /// <summary>
    /// 数据库
    /// </summary>
    Database = 4,
    /// <summary>
    /// windows日志
    /// </summary>
    EventLog = 8,
}

/// <summary>
/// 写入到文件中
/// </summary>
public class FileLogger : LogBase
{
    /// <summary>
    /// 删除日志
    /// </summary>
    public override void DeleteLog()
    {
        File.Delete(LogHelper.LogAddress);
    }

    /// <summary>
    /// 读取日志
    /// </summary>
    /// <returns></returns>
    public override string[] ReadLog()
    {
        List<string> lines = new();
        using (var sr = new StreamReader(LogHelper.LogAddress, true/*自动识别文件头*/))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
                lines.Add(line);
        }
        return lines.ToArray();
    }

    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="message"></param>
    public override void WriteLog(string? message)
    {
        // 把异常信息输出到文件
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
    /// <summary>
    /// 删除日志
    /// </summary>
    public override void DeleteLog()
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 读取日志
    /// </summary>
    /// <returns>日志内容数组</returns>
    public override string[] ReadLog()
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="message">日志消息</param>
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
    // 需要win权限
    // https://blog.csdn.net/weixin_38208401/article/details/77870909
    // NET50要加 <FrameworkReference Include="Microsoft.WindowsDesktop.App" />
    // https://docs.microsoft.com/en-us/answers/questions/526018/windows-event-log-with-net-5.html

    /// <summary>
    /// 日志名称
    /// </summary>
    public string LogName = "IFoxCadLog";

    /// <summary>
    /// 删除日志
    /// </summary>
    public override void DeleteLog()
    {
#if !NET5_0 && !NET6_0
        if (EventLog.Exists(LogName))
            EventLog.Delete(LogName);
#endif
    }
    /// <summary>
    /// 读取日志
    /// </summary>
    /// <returns>日志内容数组</returns>
    public override string[] ReadLog()
    {
        List<string> lines = new();
#if !NET5_0 && !NET6_0
        try
        {
            EventLog eventLog = new()
            {
                Log = LogName
            };
            foreach (EventLogEntry entry in eventLog.Entries)
                lines.Add(entry.Message);
        }
        catch (System.Security.SecurityException e)
        {
            throw new Exception("您没有权限读取win日志::" + e.Message);
        }
#endif
        return lines.ToArray();
    }
    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="message">日志消息</param>
    public override void WriteLog(string? message)
    {
#if !NET5_0 && !NET6_0
        try
        {
            EventLog eventLog = new()
            {
                Source = LogName
            };
            eventLog.WriteEntry(message, EventLogEntryType.Information);
        }
        catch (System.Security.SecurityException e)
        {
            throw new Exception("您没有权限写入win日志::" + e.Message);
        }
#endif
    }
}

#endregion

#region 静态方法
/// <summary>
/// 日志帮助类，提供日志记录的静态方法。
/// </summary>
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

    /// <summary>
    /// 写入日志消息
    /// </summary>
    /// <param name="message">日志消息</param>
    /// <param name="target">日志目标</param>
    /// <returns>格式化后的日志内容</returns>
    public static string WriteLog(this string? message,
                                 LogTarget target = LogTarget.File)
    {
        if (message == null)
            return string.Empty;
        return LogAction(null, message, target);
    }

    /// <summary>
    /// 写入异常日志
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <param name="target">日志目标</param>
    /// <returns>格式化后的日志内容</returns>
    public static string WriteLog(this Exception? exception,
                                  LogTarget target = LogTarget.File)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, null, target);
    }

    /// <summary>
    /// 写入异常和消息日志
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <param name="message">附加消息</param>
    /// <param name="target">日志目标</param>
    /// <returns>格式化后的日志内容</returns>
    public static string WriteLog(this Exception? exception, string? message,
                                  LogTarget target = LogTarget.File)
    {
        if (exception == null)
            return string.Empty;
        return LogAction(exception, message, target);
    }


    /// <summary>
    /// 执行日志操作
    /// </summary>
    /// <param name="ex">错误</param>
    /// <param name="message">备注信息</param>
    /// <param name="target">记录方式</param>
    /// <returns>格式化后的日志内容</returns>
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
                DebugEx.Printl("错误日志: " + LogAddress);
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
#endregion

#region 序列化
/// <summary>
/// 日志文本类，用于存储日志信息。
/// </summary>
[Serializable]
public class LogTxt
{
    /// <summary>
    /// 当前时间
    /// </summary>
    public string? 当前时间;
    /// <summary>
    /// 备注信息
    /// </summary>
    public string? 备注信息;
    /// <summary>
    /// 异常信息
    /// </summary>
    public string? 异常信息;
    /// <summary>
    /// 异常对象
    /// </summary>
    public string? 异常对象;
    /// <summary>
    /// 触发方法
    /// </summary>
    public string? 触发方法;
    /// <summary>
    /// 调用堆栈
    /// </summary>
    public string? 调用堆栈;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public LogTxt() { }

    /// <summary>
    /// 使用异常和消息初始化日志
    /// </summary>
    /// <param name="ex">异常对象</param>
    /// <param name="message">消息</param>
    public LogTxt(Exception? ex, string? message) : this()
    {
        if (ex == null && message == null)
            throw new ArgumentNullException(nameof(ex));

        // 以不同语言显示日期
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("es-ES"))
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("zh-cn"))
        // 为了最小信息熵,所以用这样的格式,并且我喜欢补0
        当前时间 = DateTime.Now.ToString("yy-MM-dd hh:mm:ss");

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

    /// <summary>
    /// 将日志对象转换为字符串表示形式
    /// </summary>
    /// <returns>日志对象的字符串表示</returns>
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