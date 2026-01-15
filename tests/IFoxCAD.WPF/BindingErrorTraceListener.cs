namespace IFoxCAD.WPF;
using System.Text.RegularExpressions;

/*
   xaml 需要绑定失败时候报错(vs默认是不报错的):
   https://cloud.tencent.com/developer/article/1342661
   所有的绑定输出,重写方法就可以转发,构造函数上加入:
   public MainWindow()
   {
       PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
       PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorTraceListener());
       App.Current.DispatcherUnhandledException += DispatcherUnhandledException;
       // InitializeComponent();
       // DataContext = new ViewModel();
   }
   private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
   {
       if (e.Exception is BindingErrorException bindingErrorException)
       {
           MessageBox.Show($"Binding error. {bindingErrorException.SourceObject}.{bindingErrorException.SourceProperty} {bindingErrorException.TargetElement}.{bindingErrorException.TargetProperty}");
       }
   }
*/

/// <summary>
/// 属性绑定错误异常
/// </summary>
public class BindingErrorException : Exception
{
    /// <summary>
    /// 来源对象
    /// </summary>
    public string? SourceObject { get; set; }
    /// <summary>
    /// 来源属性
    /// </summary>
    public string? SourceProperty { get; set; }
    /// <summary>
    /// 目标元素
    /// </summary>
    public string? TargetElement { get; set; }
    /// <summary>
    /// 目标属性
    /// </summary>
    public string? TargetProperty { get; set; }

    public BindingErrorException() : base()
    {
    }

    public BindingErrorException(string message) : base(message)
    {
    }
}

/// <summary>
/// 属性绑定错误侦听器
/// </summary>
public class BindingErrorTraceListener : TraceListener
{
    const string BindingErrorPattern =
       @"^BindingExpression path error(?:.+)'(.+)' property not found(?:.+)object[\s']+(.+?)'(?:.+)target element is '(.+?)'(?:.+)target property is '(.+?)'(?:.+)$";

    public override void Write(string message)
    {
        Trace.WriteLine(string.Format("[Write]{0}", message));
        Debug.WriteLine(string.Format("[Write]{0}", message));
    }

    public override void WriteLine(string message)
    {
        var match = Regex.Match(message, BindingErrorPattern);
        if (match.Success)
        {
            throw new BindingErrorException(message)
            {
                SourceObject = match.Groups[2].ToString(),
                SourceProperty = match.Groups[1].ToString(),
                TargetElement = match.Groups[3].ToString(),
                TargetProperty = match.Groups[4].ToString()
            };
        }
    }
}