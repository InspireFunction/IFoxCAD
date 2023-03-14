namespace IFoxCAD.Cad;

using System;
using Exception = Exception;


#region 序列化
[Serializable]
public class LogTxt
{
    public string? 当前时间;
    public string? 备注信息;
    public string? 异常信息;
    public string? 异常对象;
    public string? 触发方法;
    public string? 调用堆栈;

    public LogTxt() { }

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


