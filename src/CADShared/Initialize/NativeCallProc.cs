#pragma warning disable CS0169

namespace IFoxCAD.Basal;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


#line hidden

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

    // 关键修复：保持对委托的引用，防止被垃圾回收
    private WndProcDelegate? _wndProcDelegate;
    private readonly object _eventLock = new object();

    /// <summary>
    /// 空闲事件委托
    /// </summary>
    public event Action<object, EventArgs>? OnIdle;

    /// <summary>
    /// 消息过滤器委托
    /// </summary>
    public Func<Message, bool>? MessageFilter;

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

        // 创建委托并保持引用
        _wndProcDelegate = new WndProcDelegate(WindowProc);

        // 设置新的窗口过程
        SetWindowLong(Handle,
            GWL_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));

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

        // 释放委托引用
        if (_wndProcDelegate != null)
        {
            // 注意：不能手动释放委托，但可以清空引用
            _wndProcDelegate = null;
        }

        _oldWndProc = IntPtr.Zero;
        IsHooked = false;
    }

    // 窗口过程委托声明
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 自定义窗口过程
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        try
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
                Action<object, EventArgs>? tempHandler = null;
                lock (_eventLock)
                {
                    tempHandler = OnIdle;
                }
                tempHandler?.Invoke(this, EventArgs.Empty);
            }

            // 调用基类窗口过程
            if (callBase)
            {
                if (_oldWndProc != IntPtr.Zero)
                {
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
                }
                else
                {
                    DefWndProc(ref message);
                }
            }
            return message.Result;
        }
        catch (Exception e)
        {
            DebugEx.Printl(e);
            throw;
        }
    }

    /// <summary>
    /// 手动触发空闲事件
    /// </summary>
    public void DoIdle()
    {
        Action<object, EventArgs>? tempHandler = null;
        lock (_eventLock)
        {
            tempHandler = OnIdle;
        }
        tempHandler?.Invoke(this, EventArgs.Empty);
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
        _disposed = true;

        // 卸载窗口钩子
        UnhookWindowProc();

        // 释放句柄
        if (Handle != IntPtr.Zero)
        {
            ReleaseHandle();
        }
    }
    #endregion
}

/// <summary>
/// AutoCAD空闲事件管理器
/// </summary>
public static class AcadIdleManager
{
    #region Win32 API补充
    /// <summary>
    /// 是窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    #endregion

    /// <summary>
    /// 检查是否有模态窗口正在活动
    /// </summary>
    /// <returns></returns>
    private static bool IsModalWindowActive()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero || foregroundWindow == MainWindowHandle)
        {
            // 如果没有前台窗口或前台就是主窗口，则没有模态窗口
            return false;
        }

        // 获取前台窗口的进程ID
        GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);
        // 获取主窗口的进程ID
        GetWindowThreadProcessId(MainWindowHandle, out uint mainProcessId);

        // 如果前台窗口与主窗口属于同一进程，且不是主窗口本身，则很可能是模态对话框
        return foregroundProcessId == mainProcessId && foregroundWindow != MainWindowHandle;
    }

    private static AcadWindowProc? _windowProc;
    private static System.Timers.Timer? _idleTimer;
    private static Control? _dummyControl;

    // 添加锁对象确保线程安全
    private static readonly object _lock = new object();

    // 保存原始事件处理程序和对应的lambda表达式之间的映射关系
#if ac2008
    private static readonly Dictionary<EventHandler, Action<object, EventArgs>> _eventHandlers = new();
    private static readonly object _handlersLock = new object();
#endif


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
    public static event EventHandler? OnIdle
    {
        add
        {
#if ac2008
            if (_windowProc != null && value != null)
            {
                lock (_handlersLock)
                {
                    // 创建lambda表达式并保存映射关系
                    Action<object, EventArgs> handler = (s, e) => value?.Invoke(s, e);
                    _eventHandlers[value] = handler;
                    _windowProc.OnIdle += handler;
                }
            }
#else
            Acap.Idle += value;
#endif
        }
        remove
        {
#if ac2008
            if (_windowProc != null && value != null)
            {
                lock (_handlersLock)
                {
                    // 从映射中获取对应的lambda表达式并移除
                    if (_eventHandlers.TryGetValue(value, out var handler))
                    {
                        _windowProc.OnIdle -= handler;
                        _eventHandlers.Remove(value);
                    }
                }
            }
#else
            Acap.Idle -= value;
#endif
        }
    }


    static AcadIdleManager()
    {
        Initialize();
    }



    /// <summary>
    /// 初始化2008版本的空闲管理器
    /// </summary>
    private static void Initialize()
    {
#if ac2008
        if (_windowProc != null) return;

        lock (_lock)
        {
            if (_windowProc != null) return;

            try
            {
                MainWindowHandle = Acap.MainWindow.Handle;

                // 创建窗口过程拦截器
                _windowProc = new AcadWindowProc(MainWindowHandle);

                // 创建虚拟控件用于线程同步
                _dummyControl = new Control();
                _dummyControl.CreateControl();

                // 启动定时器模拟空闲事件
                _idleTimer = new System.Timers.Timer(IdleInterval);
                _idleTimer.Elapsed += (s, e) => {
                    if (_dummyControl != null && _dummyControl.InvokeRequired)
                    {
                        // 检查主窗口句柄是否仍然有效，确保仍在AutoCAD环境中
                        // 并且检查当前没有模态窗口阻塞
                        if (MainWindowHandle != IntPtr.Zero && IsWindow(MainWindowHandle) && !IsModalWindowActive())
                        {
                            _dummyControl.BeginInvoke(new Action(() => {
                                _windowProc?.DoIdle();
                            }));
                        }
                    }
                    else
                    {
                        // 在主线程上也需要检查主窗口有效性
                        if (MainWindowHandle != IntPtr.Zero && IsWindow(MainWindowHandle) && !IsModalWindowActive())
                        {
                            _windowProc?.DoIdle();
                        }
                    }
                };
                _idleTimer.Start();
            }
            catch (Exception ex)
            {
                // 记录错误
                System.Diagnostics.Debug.WriteLine($"AcadIdleManager初始化失败: {ex.Message}");
            }
        }
#endif
    }

    /// <summary>
    /// 停止空闲事件管理器
    /// </summary>
    public static void Shutdown()
    {
#if ac2008
        lock (_lock)
        {
            if (_idleTimer != null)
            {
                _idleTimer.Stop();
                _idleTimer.Dispose();
                _idleTimer = null;
            }

            if (_dummyControl != null)
            {
                if (_dummyControl.InvokeRequired)
                {
                    // 检查主窗口句柄是否仍然有效，确保仍在AutoCAD环境中
                    if (MainWindowHandle != IntPtr.Zero && IsWindow(MainWindowHandle))
                    {
                        _dummyControl.Invoke(new Action(() => {
                            _dummyControl.Dispose();
                        }));
                    }
                }
                else
                {
                    _dummyControl.Dispose();
                }
                _dummyControl = null;
            }

            if (_windowProc != null)
            {
                _windowProc.Dispose();
                _windowProc = null;
            }
        }
#endif
    }
}


#line default