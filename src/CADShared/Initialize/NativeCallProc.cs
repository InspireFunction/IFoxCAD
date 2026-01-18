namespace IFoxCAD.Basal;

using System;
using System.Windows.Forms;

#if true2
/// <summary>
/// 窗口控件子类化
/// </summary>
public class NativeCallProc : NativeWindow, IDisposable
{
    Func<Message, bool>? WndProcEvent;

    /// <summary>
    /// 窗口控件子类化<br/>
    /// 声明一定要写到类成员上,否则导致GC不确定的释放,从而触发析构
    /// </summary>
    /// <param name="intPtr">窗体句柄</param>
    public NativeCallProc(IntPtr intPtr)
    {
        this.AssignHandle(intPtr);
    }

    /// <summary>
    /// 消息循环:传委托进去不断替换
    /// </summary>
    /// <param name="WndProc">消息,true不拦截回调</param>
    public void WndProc(Func<Message, bool> WndProc)
    {
        WndProcEvent = WndProc;
    }

#line hidden
    /// <summary>
    /// 窗口过程,此处会不断进行消息循环
    /// </summary>
    /// <param name="msg"></param>
    protected override void WndProc(ref Message msg)
    {
        if (WndProcEvent is null)
            return;
        if (WndProcEvent.Invoke(msg))
            base.WndProc(ref msg);
    }
#line default

    #region IDisposable接口相关函数
    /// <summary>
    /// 
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 手动调用释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数调用释放
    /// </summary>
    ~NativeCallProc()
    {
        Dispose(false);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放,并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;

        // 释放占用窗体的句柄
        ReleaseHandle();
    }
    #endregion
}


#endif


/// <summary>
/// AutoCAD窗口消息拦截器 - 支持自定义空闲事件
/// </summary>
public class AcadWindowProc : NativeWindow, IDisposable
{
    #region Win32 API
    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    private const int GWL_WNDPROC = -4;
    private const int WM_NULL = 0x0000;
    private const int WM_ENTERIDLE = 0x0121;
    #endregion

    // 原窗口过程地址
    private IntPtr _oldWndProc = IntPtr.Zero;

    /// <summary>
    /// 空闲事件委托
    /// </summary>
    public event Action<object, EventArgs>? OnIdle;

    /// <summary>
    /// 消息过滤器委托
    /// </summary>
    public Func<Message, bool>? MessageFilter;

    // <summary>
    // 当前窗口句柄
    // </summary>
    // public IntPtr Handle { get; private set; }

    /// <summary>
    /// 是否已安装消息钩子
    /// </summary>
    public bool IsHooked { get; private set; }

    /// <summary>
    /// 安装消息钩子到指定窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    public AcadWindowProc(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            throw new ArgumentException("无效的窗口句柄");

        // this.Handle = hWnd;
        this.AssignHandle(hWnd);
        HookWindowProc();
    }

    /// <summary>
    /// 安装消息钩子
    /// </summary>
    private void HookWindowProc()
    {
        if (IsHooked || Handle == IntPtr.Zero)
            return;

        // 保存原窗口过程
        _oldWndProc = GetWindowLong(Handle, GWL_WNDPROC);

        // 设置新的窗口过程
        SetWindowLong(Handle,
            GWL_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(new WndProcDelegate(WindowProc)));

        IsHooked = true;
    }

    /// <summary>
    /// 卸载消息钩子
    /// </summary>
    private void UnhookWindowProc()
    {
        if (!IsHooked || Handle == IntPtr.Zero || _oldWndProc == IntPtr.Zero)
            return;

        // 恢复原窗口过程
        SetWindowLong(Handle, GWL_WNDPROC, _oldWndProc);
        _oldWndProc = IntPtr.Zero;
        IsHooked = false;
    }

    // 窗口过程委托声明
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 自定义窗口过程
    /// </summary>
    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        var message = Message.Create(hWnd, (int)msg, wParam, lParam);

        // 调用消息过滤器
        bool callBase = true;
        if (MessageFilter != null)
        {
            callBase = MessageFilter.Invoke(message);
        }

        // 检测空闲消息
        if (msg == WM_ENTERIDLE || msg == WM_NULL)
        {
            OnIdle?.Invoke(this, EventArgs.Empty);
        }

        // 调用基类窗口过程
        if (callBase)
        {
            if (_oldWndProc != IntPtr.Zero)
            {
                DefWndProc(ref message);
                return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
            }
            else
            {
                base.WndProc(ref message);
            }
        }

        return message.Result;
    }

    /// <summary>
    /// 手动触发空闲事件
    /// </summary>
    public void DoIdle()
    {
        OnIdle?.Invoke(this, EventArgs.Empty);
    }

    #region IDisposable 实现
    private bool _disposed = false;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~AcadWindowProc()
    {
        Dispose(false);
    }

    /// <summary>
    /// 释放托管和非托管资源
    /// </summary>
    /// <param name="disposing">是否由Dispose调用</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // 卸载窗口钩子
        UnhookWindowProc();

        // 释放句柄
        if (Handle != IntPtr.Zero)
        {
            ReleaseHandle();
            // Handle = IntPtr.Zero;
        }

        _disposed = true;
    }
    #endregion
}

/// <summary>
/// AutoCAD空闲事件管理器
/// </summary>
public static class AcadIdleManager
{
    private static AcadWindowProc? _windowProc;
    private static System.Timers.Timer? _idleTimer;

    // 添加一个虚拟控件用于线程同步
    private static Control? _dummyControl;

    /// <summary>
    /// 空闲事件间隔（毫秒），默认100ms
    /// </summary>
    public static int IdleInterval { get; set; } = 100;

    /// <summary>
    /// AutoCAD主窗口句柄
    /// </summary>
    public static IntPtr MainWindowHandle { get; private set; }

    /// <summary>
    /// 空闲事件
    /// </summary>
#if ac2008
    public static event EventHandler? OnIdle;
#else
    public static event EventHandler? OnIdle
    {
        add
        {
            Acap.Idle += value;
        }
        remove
        {
            Acap.Idle -= value;
        }
    }
#endif


    static AcadIdleManager()
    {
        MainWindowHandle = Acap.MainWindow.Handle;
#if ac2008
        // 创建窗口过程拦截器
        _windowProc = new AcadWindowProc(MainWindowHandle);

        // 订阅空闲事件
        _windowProc.OnIdle += (s, e) => OnIdle?.Invoke(s, e);

        // 设置消息过滤器
        _windowProc.MessageFilter = (msg) => {
            // 可以在这里过滤特定消息
            return true; // 返回true表示继续处理消息
        };

        // 创建一个虚拟控件用于线程同步
        _dummyControl = new Control();
        _dummyControl.CreateControl(); // 确保控件句柄被创建

        // 启动定时器模拟空闲事件
        _idleTimer = new System.Timers.Timer(IdleInterval);
        _idleTimer.Elapsed += (s, e) => {
            // 使用虚拟控件确保在UI线程上执行
            if (_dummyControl.InvokeRequired)
            {
                // 使用Invoke确保在UI线程上执行，同时保持上下文
                _dummyControl.Invoke(new Action(() => {
                    _windowProc?.DoIdle();
                }));
            }
            else
            {
                // 如果已经在UI线程上，直接调用空闲事件
                _windowProc?.DoIdle();
            }
        };
        _idleTimer.Start();
#endif
    }

    /// <summary>
    /// 停止空闲事件管理器
    /// </summary>
    public static void Shutdown()
    {
#if ac2008
        if (_idleTimer != null)
        {
            _idleTimer.Stop();
            _idleTimer.Dispose();
        }

        if (_dummyControl != null)
        {
            _dummyControl.Dispose();
            _dummyControl = null;
        }

        if (_windowProc != null)
        {
            _windowProc.Dispose();
        }
        OnIdle = null;
#endif
    }
}