namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

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
    public static string? WriteLog(this System.Exception? ex, string? remarks = null, bool printDebugWindow = true)
    {
        if (ex == null)
            return null;

        try
        {
            _logWriteLock.EnterWriteLock();// 写模式锁定 读写锁

            var logtxt = new LogTxt(ex, remarks);
            //var logtxtJson = Newtonsoft.Json.JsonConvert.SerializeObject(logtxt, Formatting.Indented);
            var logtxtJson = logtxt.ToString();

            //把异常信息输出到文件
            var sw = new StreamWriter(_logAddress, true/*当天日志文件存在就追加,否则就创建*/);
            sw.Write(logtxtJson);
            sw.Flush();
            sw.Close();
            sw.Dispose();
#if DEBUG
            if (printDebugWindow)
            {
                Debug.WriteLine("错误日志: " + _logAddress);
                Debug.Write(logtxtJson);
            }
            //Debugger.Break(); 
            //Debug.Assert(false, "终止进程");
#endif
            return logtxtJson;
        }
        finally
        {
            _logWriteLock.ExitWriteLock();// 解锁 读写锁
        }
    }
}

[Serializable]
public class LogTxt
{
    public string 当前时间;
    public string 备注信息;
    public string 异常信息;
    public string 异常对象;
    public string 触发方法;
    public string 调用堆栈;

    public LogTxt(System.Exception ex, string? remarks = null)
    {
        // 以不同语言显示日期
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("es-ES"))
        // DateTime.Now.ToString("f", new System.Globalization.CultureInfo("zh-cn"))
        // 为了最小信息熵,所以用这样的格式,并且我喜欢补0
        this.当前时间 = DateTime.Now.ToString("yy-MM-dd hh:mm:ss");
        this.备注信息 = remarks ?? string.Empty;
        this.异常信息 = ex.Message;
        this.异常对象 = ex.Source;
        this.触发方法 = ex.TargetSite == null ? string.Empty : ex.TargetSite.ToString();
        this.调用堆栈 = ex.StackTrace == null ? string.Empty : ex.StackTrace.Trim();
    }

    /// 为了不引入json的dll,所以这里自己构造
    public override string ToString()
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