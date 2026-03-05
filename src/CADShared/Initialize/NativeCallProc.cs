#pragma warning disable CS0169

namespace IFoxCAD.Basal;

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
#if NET40_OR_GREATER
using System.Runtime.ExceptionServices;
using System.Security;
#endif


/// <summary>
/// AutoCAD窗口消息拦截器 - 支持自定义空闲事件,子类化
/// </summary>
public class AcadWindowProc : NativeWindow, IDisposable
{
    #region Win32 API
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // 32位和64位使用不同的API
#if x64
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
#else
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
#endif

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
        InitializeWindowProc(hWnd);
    }

    /// <summary>
    /// 初始化窗口过程
    /// </summary>
    [DebuggerHidden]
    private void InitializeWindowProc(IntPtr hWnd)
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
        HookWindowProcCore();
    }

    /// <summary>
    /// 安装消息钩子核心逻辑
    /// </summary>
    [DebuggerHidden]
    private void HookWindowProcCore()
    {
        if (IsHooked || Handle == IntPtr.Zero)
            return;

        try
        {
            // 保存原窗口过程
            _oldWndProc = GetWindowLong(Handle, GWL_WNDPROC);

            // 验证原窗口过程是否有效
            if (_oldWndProc == IntPtr.Zero)
            {
                DebugEx.Printl("[HookWindowProc] 错误: 无法获取原窗口过程");
                return;
            }

            // 创建委托并保持引用 - 使用StdCall调用约定
            _wndProcDelegate = new WndProcDelegate(WindowProc);

            // 获取委托的函数指针
            IntPtr procPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            if (procPtr == IntPtr.Zero)
            {
                DebugEx.Printl("[HookWindowProc] 错误: 无法获取委托的函数指针");
                _wndProcDelegate = null;
                _oldWndProc = IntPtr.Zero;
                return;
            }

            // 设置新的窗口过程
            IntPtr result = SetWindowLong(Handle, GWL_WNDPROC, procPtr);
            if (result == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                DebugEx.Printl($"[HookWindowProc] 错误: SetWindowLong 失败，错误码: {error}");
                _wndProcDelegate = null;
                _oldWndProc = IntPtr.Zero;
                return;
            }

            IsHooked = true;
            DebugEx.Printl($"[HookWindowProc] 消息钩子安装成功，窗口句柄: {Handle}");
        }
        catch (BadImageFormatException ex)
        {
            // 这是错误调用了x86/x64版本的闪退拦截
            DebugEx.Printl($"[HookWindowProc] BadImageFormatException: {ex.Message}");
            DebugEx.Printl($"[HookWindowProc] 调用栈: {ex.StackTrace}");
            _wndProcDelegate = null;
            _oldWndProc = IntPtr.Zero;
            IsHooked = false;
        }
        catch (Exception ex)
        {
            // 全局异常拦截，防止钩子安装失败导致 Acad 崩溃
            DebugEx.Printl($"[HookWindowProc] 异常: {ex.Message}");
            DebugEx.Printl($"[HookWindowProc] 调用栈: {ex.StackTrace}");
            _wndProcDelegate = null;
            _oldWndProc = IntPtr.Zero;
            IsHooked = false;
        }
    }

    /// <summary>
    /// 卸载消息钩子
    /// </summary>
    private void UnhookWindowProc()
    {
        UnhookWindowProcCore();
    }

    /// <summary>
    /// 卸载消息钩子核心逻辑
    /// </summary>
    [DebuggerHidden]
    private void UnhookWindowProcCore()
    {
        if (!IsHooked || Handle == IntPtr.Zero || _oldWndProc == IntPtr.Zero)
            return;

        try
        {
            // 恢复原窗口过程
            SetWindowLong(Handle, GWL_WNDPROC, _oldWndProc);
            DebugEx.Printl($"[UnhookWindowProc] 消息钩子已卸载，窗口句柄: {Handle}");
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[UnhookWindowProc] BadImageFormatException: {ex.Message}");
            DebugEx.Printl($"[UnhookWindowProc] 调用栈: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[UnhookWindowProc] 异常: {ex.Message}");
        }
        finally
        {
            // 释放委托引用
            if (_wndProcDelegate != null)
            {
                // 注意：不能手动释放委托，但可以清空引用
                _wndProcDelegate = null;
            }

            _oldWndProc = IntPtr.Zero;
            IsHooked = false;
        }
    }

    // 窗口过程委托声明 - 使用StdCall调用约定以确保x86兼容性
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 自定义窗口过程
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
#if NET40_OR_GREATER
    [HandleProcessCorruptedStateExceptions]
    [SecurityCritical]
#endif
    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        return WindowProcCore(hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// 窗口过程核心逻辑
    /// </summary>
    [DebuggerHidden]
    private IntPtr WindowProcCore(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        // 首先验证句柄有效性
        if (hWnd == IntPtr.Zero)
        {
            DebugEx.Printl("[WindowProc] 错误: 窗口句柄为空");
            return IntPtr.Zero;
        }

        // 验证原窗口过程是否有效
        if (_oldWndProc == IntPtr.Zero)
        {
            DebugEx.Printl("[WindowProc] 错误: 原窗口过程为空");
            return IntPtr.Zero;
        }

        try
        {
            var message = Message.Create(hWnd, (int)msg, wParam, lParam);

            // 调用消息过滤器
            bool callBase = true;
            if (MessageFilter != null)
            {
                try
                {
                    callBase = MessageFilter.Invoke(message);
                }
                catch (BadImageFormatException ex)
                {
                    DebugEx.Printl($"[WindowProc] MessageFilter 中发生 BadImageFormatException: {ex.Message}");
                    DebugEx.Printl($"[WindowProc] MessageFilter 调用栈: {ex.StackTrace}");
                    // 继续调用原窗口过程，不中断消息流
                    callBase = true;
                }
                catch (AccessViolationException ex)
                {
                    DebugEx.Printl($"[WindowProc] MessageFilter 中发生 AccessViolationException: {ex.Message}");
                    callBase = true;
                }
            }

            // 检测空闲消息
            if (msg == WM_ENTERIDLE || msg == WM_NULL)
            {
                try
                {
                    // 检查编辑器是否处于空闲状态(可以发送透明命令)
                    var doc = Acap.DocumentManager?.MdiActiveDocument;
                    if (doc?.Editor?.IsQuiescent == true)
                    {
                        Action<object, EventArgs>? tempHandler = null;
                        lock (_eventLock)
                        {
                            tempHandler = OnIdle;
                        }
                        tempHandler?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    DebugEx.Printl($"[WindowProc] OnIdle 事件中发生 BadImageFormatException: {ex.Message}");
                    DebugEx.Printl($"[WindowProc] OnIdle 调用栈: {ex.StackTrace}");
                }
                catch (AccessViolationException ex)
                {
                    DebugEx.Printl($"[WindowProc] OnIdle 事件中发生 AccessViolationException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    DebugEx.Printl($"[WindowProc] OnIdle 事件中发生异常: {ex.Message}");
                }
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
        catch (BadImageFormatException ex)
        {
            // BadImageFormatException 通常表示 P/Invoke 调用约定不匹配或架构问题
            DebugEx.Printl($"[WindowProc] 捕获到 BadImageFormatException: {ex.Message}");
            DebugEx.Printl($"[WindowProc] 调用栈: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                DebugEx.Printl($"[WindowProc] 内部异常: {ex.InnerException.Message}");
                DebugEx.Printl($"[WindowProc] 内部异常调用栈: {ex.InnerException.StackTrace}");
            }

            // 尝试调用原窗口过程，让系统继续处理消息
            try
            {
                if (_oldWndProc != IntPtr.Zero)
                {
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
                }
            }
            catch
            {
                // 如果原窗口过程也失败，返回默认处理
            }
            return IntPtr.Zero;
        }
        catch (AccessViolationException ex)
        {
            // 访问冲突异常 - 通常是内存损坏或无效指针
            DebugEx.Printl($"[WindowProc] 捕获到 AccessViolationException: {ex.Message}");

            // 尝试调用原窗口过程
            try
            {
                if (_oldWndProc != IntPtr.Zero)
                {
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
                }
            }
            catch
            {
                // 忽略二次异常
            }
            return IntPtr.Zero;
        }
        catch (SEHException ex)
        {
            // 结构化异常处理异常
            DebugEx.Printl($"[WindowProc] 捕获到 SEHException: {ex.Message}");

            try
            {
                if (_oldWndProc != IntPtr.Zero)
                {
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
                }
            }
            catch
            {
                // 忽略二次异常
            }
            return IntPtr.Zero;
        }
        catch (Exception e)
        {
            DebugEx.Printl($"[WindowProc] 未处理的异常: {e.Message}");
            DebugEx.Printl($"  异常类型: {e.GetType().FullName}");
            DebugEx.Printl($"  堆栈跟踪: {e.StackTrace}");

            // 尝试调用原窗口过程
            try
            {
                if (_oldWndProc != IntPtr.Zero)
                {
                    return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
                }
            }
            catch
            {
                // 忽略二次异常
            }
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 手动触发空闲事件
    /// </summary>
    public void DoIdle()
    {
        DoIdleCore();
    }

    /// <summary>
    /// 手动触发空闲事件核心逻辑
    /// </summary>
    [DebuggerHidden]
    private void DoIdleCore()
    {
        // 检查编辑器是否处于空闲状态(可以发送透明命令)
        var doc = Acap.DocumentManager?.MdiActiveDocument;
        if (doc?.Editor?.IsQuiescent == true)
        {
            Action<object, EventArgs>? tempHandler = null;
            lock (_eventLock)
            {
                tempHandler = OnIdle;
            }
            tempHandler?.Invoke(this, EventArgs.Empty);
        }
    }

    #region IDisposable 实现
    private bool _disposed = false;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        DisposeCore(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~AcadWindowProc()
    {
        DisposeCore(false);
    }

    /// <summary>
    /// 释放托管和非托管资源核心逻辑
    /// </summary>
    /// <param name="disposing">是否由Dispose调用</param>
    [DebuggerHidden]
    private void DisposeCore(bool disposing)
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

    /// <summary>
    /// 释放托管和非托管资源
    /// </summary>
    /// <param name="disposing">是否由Dispose调用</param>
    protected virtual void Dispose(bool disposing)
    {
        DisposeCore(disposing);
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
                var acadWin = Acap.MainWindow.Handle;
                if (acadWin == IntPtr.Zero)
                    return;

                // 创建窗口过程拦截器
                _windowProc = new AcadWindowProc(acadWin);

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
                        var acadWin = Acap.MainWindow.Handle;
                        if (acadWin == IntPtr.Zero)
                            return;
                        if (!WindowsAPI.IsWindow(acadWin))
                            return;
                        if (!WindowsAPI.IsWindowEnabled(acadWin))
                            return;
                        // 检查当前没有模态窗口阻塞
                        if (WindowsAPI.IsModalWindowActive(acadWin))
                            return;
                        _dummyControl.BeginInvoke(() => {
                            _windowProc?.DoIdle();
                        });
                    }
                    else
                    {
                        var acadWin = Acap.MainWindow.Handle;
                        if (acadWin == IntPtr.Zero)
                            return;
                        if (!WindowsAPI.IsWindow(acadWin))
                            return;
                        if (!WindowsAPI.IsWindowEnabled(acadWin))
                            return;
                        // 检查当前没有模态窗口阻塞
                        if (WindowsAPI.IsModalWindowActive(acadWin))
                            return;
                        _windowProc?.DoIdle();
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

            if (_dummyControl != null && _dummyControl.InvokeRequired)
            {
                _dummyControl.Invoke(() => {
                    _dummyControl.Dispose();
                });
            }
            else
            {
                _dummyControl?.Dispose();
            }
            _dummyControl = null;

            if (_windowProc != null)
            {
                _windowProc.Dispose();
                _windowProc = null;
            }
        }
#endif
    }

    /// <summary>
    /// 添加仅执行一次的空闲事件处理程序
    /// 在下一个空闲事件触发时执行指定操作，然后自动取消订阅
    /// </summary>
    /// <param name="action">要执行的操作</param>
    public static void OnIdleOnce(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

#if ac2008
        // 使用局部变量来保存事件处理程序，以便在lambda中引用自身进行取消订阅
        EventHandler? handler = null;
        handler = (s, e) => {
            // 立即取消订阅，确保只执行一次
            OnIdle -= handler;
            // 执行用户操作
            action();
        };
        OnIdle += handler;
#else
        // 高版本使用 Application.Idle 事件
        EventHandler? handler = null;
        handler = (s, e) => {
            Acap.Idle -= handler;
            action();
        };
        Acap.Idle += handler;
#endif
    }


    /// <summary>
    /// 执行一次的空闲事件处理程序,但是根据状态判断是否退出.
    /// </summary>
    /// <param name="action">要执行的操作</param>
    public static void OnIdleOnce(Action<CtrlState> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        CtrlState ctrlState = new();
        ctrlState.Reset();
#if ac2008
        // 使用局部变量来保存事件处理程序，以便在lambda中引用自身进行取消订阅
        EventHandler? handler = null;
        handler = (s, e) => {
            // 执行用户操作
            action(ctrlState);
            // 用户选择跳过状态,就不结束,
            // 下次执行时候可能根据环境而改变
            if (!ctrlState.IsContinue)
            {
                OnIdle -= handler;
                return;
            }
            // 又再次初始化,相当于来回拨动开关,积极终止循环
            ctrlState.Reset();
        };
        OnIdle += handler;
#else
        // 高版本使用 Application.Idle 事件
        EventHandler? handler = null;
        handler = (s, e) => {
            // 执行用户操作
            action(ctrlState);
            // 用户选择跳过状态,就不结束,
            // 下次执行时候可能根据环境而改变
            if (!ctrlState.IsContinue)
            {
                OnIdle -= handler;
                return;
            }
            // 又再次初始化,相当于来回拨动开关,积极终止循环
            ctrlState.Reset();
        };
        Acap.Idle += handler;
#endif
    }

    /// <summary>
    /// 添加仅执行一次的空闲事件处理程序（带发送者和事件参数）
    /// 在下一个空闲事件触发时执行指定操作，然后自动取消订阅
    /// </summary>
    /// <param name="handler">要执行的事件处理程序</param>
    public static void OnIdleOnce(EventHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

#if ac2008
        // 使用局部变量来保存包装后的事件处理程序
        EventHandler? wrapper = null;
        wrapper = (s, e) => {
            // 立即取消订阅，确保只执行一次
            OnIdle -= wrapper;
            // 执行用户处理程序
            handler(s, e);
        };
        OnIdle += wrapper;
#else
        // 高版本使用 Application.Idle 事件
        EventHandler? wrapper = null;
        wrapper = (s, e) => {
            Acap.Idle -= wrapper;
            handler(s, e);
        };
        Acap.Idle += wrapper;
#endif
    }
}
