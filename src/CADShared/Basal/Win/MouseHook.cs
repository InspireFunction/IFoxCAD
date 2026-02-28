namespace IFoxCAD.Cad;

using IFoxCAD.Basal;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// 提供鼠标钩子功能的类，用于捕获和处理鼠标事件。
/// 支持进程级和全局级的鼠标钩子设置。
/// </summary>
public class MouseHook
{
    // 事件锁对象，确保线程安全
    private readonly object _eventLock = new object();

    // 私有委托字段
    private MouseEventHandler? _mouseDown;
    private MouseEventHandler? _mouseUp;
    private MouseEventHandler? _mouseMove;
    private MouseEventHandler? _mouseWheel;
    private EventHandler? _click;
    private EventHandler? _doubleClick;

    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    public event MouseEventHandler? MouseDown
    {
        add { lock (_eventLock) _mouseDown += value; }
        remove { lock (_eventLock) _mouseDown -= value; }
    }

    /// <summary>
    /// 松开鼠标事件
    /// </summary>
    public event MouseEventHandler? MouseUp
    {
        add { lock (_eventLock) _mouseUp += value; }
        remove { lock (_eventLock) _mouseUp -= value; }
    }

    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    public event MouseEventHandler? MouseMove
    {
        add { lock (_eventLock) _mouseMove += value; }
        remove { lock (_eventLock) _mouseMove -= value; }
    }

    /// <summary>
    /// 鼠标滚轮事件
    /// </summary>
    public event MouseEventHandler? MouseWheel
    {
        add { lock (_eventLock) _mouseWheel += value; }
        remove { lock (_eventLock) _mouseWheel -= value; }
    }

    /// <summary>
    /// 鼠标单击事件
    /// </summary>
    public event EventHandler? Click
    {
        add { lock (_eventLock) _click += value; }
        remove { lock (_eventLock) _click -= value; }
    }

    /// <summary>
    /// 鼠标双击事件
    /// </summary>
    public event EventHandler? DoubleClick
    {
        add { lock (_eventLock) _doubleClick += value; }
        remove { lock (_eventLock) _doubleClick -= value; }
    }


    bool _isHookBreak = false;
    /// <summary>
    /// 否决本次输入:设置不向下回调
    /// </summary>
    public void Vote()
    {
        _isHookBreak = true;
    }

    /// <summary>
    /// 不要试图省略此变量,否则将会导致GC变量池满后释放。<br/>
    /// 提示:激活 CallbackOnCollectedDelegate 托管调试助手(MDA)。
    /// </summary>
    internal static WindowsAPI.CallBack? HookProc;
    /// <summary>
    /// 挂载成功的标记
    /// </summary>
    internal static IntPtr _NextHookProc;
    /// <summary>
    /// 当前进程
    /// </summary>
    public readonly Process Process;


    static readonly Stopwatch _watch = new();

    /// <summary>
    /// 安装鼠标钩子
    /// </summary>
    /// <param name="setLowLevel">低级钩子超时时间</param>
    public MouseHook(int setLowLevel = 25000)
    {
        _NextHookProc = IntPtr.Zero;
        Process = Process.GetCurrentProcess();
        WindowsAPI.CheckLowLevelHooksTimeout(setLowLevel);
        _watch.Start();
    }

    void UnHook()
    {
        if (_NextHookProc != IntPtr.Zero)
        {
            try
            {
                WindowsAPI.UnhookWindowsHookExSafe(_NextHookProc);
            }
            catch (BadImageFormatException ex)
            {
                DebugEx.Printl($"[MouseHook.UnHook] BadImageFormatException: {ex.Message}");
            }
            catch (Exception ex)
            {
                DebugEx.Printl($"[MouseHook.UnHook] 异常: {ex.Message}");
            }
            finally
            {
                _NextHookProc = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// 设置钩子
    /// </summary>
    /// <param name="processHook">false进程钩子,true全局钩子</param>
    public void SetHook(bool processHook = false)
    {
        UnHook();
        if (_NextHookProc != IntPtr.Zero)
            return;

        try
        {
            if (processHook)
            {
                // 创建委托并保持引用，防止GC回收
                HookProc = (nCode, wParam, lParam) => {
                    try
                    {
                        if (nCode >= 0 && HookTask(nCode, wParam, lParam))
                            return (IntPtr)1;
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                    catch (BadImageFormatException ex)
                    {
                        DebugEx.Printl($"[MouseHook.HookProc] BadImageFormatException: {ex.Message}");
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                    catch (Exception ex)
                    {
                        DebugEx.Printl($"[MouseHook.HookProc] 异常: {ex.Message}");
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                };
                _NextHookProc = WindowsAPI.SetWindowsHookExSafe(HookType.WH_MOUSE, HookProc!,
                                                            IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
            }
            else
            {
                var moduleHandle = WindowsAPI.GetModuleHandle(Process.MainModule?.ModuleName ?? Process.ProcessName);
                // 创建委托并保持引用，防止GC回收
                HookProc = (nCode, wParam, lParam) => {
                    try
                    {
                        if (nCode >= 0 && HookTask(nCode, wParam, lParam))
                            return (IntPtr)1;
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                    catch (BadImageFormatException ex)
                    {
                        DebugEx.Printl($"[MouseHook.HookProc] BadImageFormatException: {ex.Message}");
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                    catch (Exception ex)
                    {
                        DebugEx.Printl($"[MouseHook.HookProc] 异常: {ex.Message}");
                        return WindowsAPI.CallNextHookExSafe(_NextHookProc, nCode, wParam, lParam);
                    }
                };
                _NextHookProc = WindowsAPI.SetWindowsHookExSafe(HookType.WH_MOUSE_LL, HookProc!,
                                                            moduleHandle, 0);
            }

            if (_NextHookProc == IntPtr.Zero)
            {
                DebugEx.Printl("[MouseHook.SetHook] 钩子安装失败");
                HookProc = null;
            }
            else
            {
                DebugEx.Printl($"[MouseHook.SetHook] 钩子安装成功，类型: {(processHook ? "进程钩子" : "全局钩子")}");
            }
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[MouseHook.SetHook] BadImageFormatException: {ex.Message}");
            HookProc = null;
            _NextHookProc = IntPtr.Zero;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[MouseHook.SetHook] 异常: {ex.Message}");
            HookProc = null;
            _NextHookProc = IntPtr.Zero;
        }
    }



    MouseButtons _button;
    int _clickCount = 0;
    bool _down = false;
    bool _up = false;
    bool _ck = false;

    /// <summary>
    /// 钩子的消息处理
    /// </summary>
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns>false不终止回调,true终止回调</returns>
    bool HookTask(int nCode, int wParam, IntPtr lParam)
    {
        try
        {
            // 获取事件处理程序的本地副本（线程安全）
            MouseEventHandler? mouseDownHandler, mouseUpHandler, mouseMoveHandler, mouseWheelHandler;
            EventHandler? clickHandler, doubleClickHandler;
            lock (_eventLock)
            {
                mouseDownHandler = _mouseDown;
                mouseUpHandler = _mouseUp;
                mouseMoveHandler = _mouseMove;
                mouseWheelHandler = _mouseWheel;
                clickHandler = _click;
                doubleClickHandler = _doubleClick;
            }

            if (mouseDownHandler is null
             && mouseUpHandler is null
             && mouseMoveHandler is null
             && mouseWheelHandler is null
             && clickHandler is null
             && doubleClickHandler is null)
                return false;

            _button = MouseButtons.None;
            _clickCount = 0;
            _down = false;
            _up = false;
            _ck = false;

            switch ((WM)wParam)
            {
                case WM.WM_LBUTTONDOWN:
                _button = MouseButtons.Left;
                _clickCount = 1;
                _down = true;
                _ck = true;
                break;
                case WM.WM_LBUTTONUP:
                _button = MouseButtons.Left;
                _clickCount = 1;
                _up = true;
                break;
                case WM.WM_LBUTTONDBLCLK:
                _button = MouseButtons.Left;
                _clickCount = 2;
                _ck = true;
                break;
                case WM.WM_RBUTTONDOWN:
                _button = MouseButtons.Right;
                _clickCount = 1;
                _down = true;
                _ck = true;
                break;
                case WM.WM_RBUTTONUP:
                _button = MouseButtons.Right;
                _clickCount = 1;
                _up = true;
                break;
                case WM.WM_RBUTTONDBLCLK:
                _button = MouseButtons.Right;
                _clickCount = 2;
                _ck = true;
                break;
                case WM.WM_MBUTTONDOWN:
                _button = MouseButtons.Middle;
                _clickCount = 1;
                _ck = true;
                break;
                case WM.WM_MOUSEWHEEL:
                // 滚轮
                break;
                case WM.WM_MOUSEMOVE:
                // 移动
                // 假设想要限制鼠标在屏幕中的移动区域能够在此处设置
                // 后期须要考虑实际的x y的容差
                // if (!Screen.PrimaryScreen.Bounds.Contains(e.X, e.Y))
                //     // return 1;
                // if (button == MouseButtons.Left)
                // {
                //     GetCursorPos(out POINT pt);
                //     // 防止频繁获取导致出错
                //     if (pt0ld.Leng(pt) > 20)
                //         pt0ld = pt;
                // }
                break;
            }

            // 从回调函数中得到鼠标的信息
            var mouseMsg = MouseHookStruct.Create(lParam);
            MouseEventArgs e = new(_button, _clickCount, mouseMsg.Point.X, mouseMsg.Point.Y, 0);
            if (_down)
                mouseDownHandler?.Invoke(this, e);
            if (_up)
                mouseUpHandler?.Invoke(this, e);
            if (_ck)
                clickHandler?.Invoke(this, e);
            if (_clickCount == 2)
            {
                // 如果不用时间控制,那么双击会执行两次
                if (_watch.Elapsed.TotalMilliseconds > WindowsAPI.GetDoubleClickTime())
                {
                    doubleClickHandler?.Invoke(this, e);
                    _watch.Reset();
                    _watch.Start();
                }
            }
            mouseMoveHandler?.Invoke(this, e);
            mouseWheelHandler?.Invoke(this, e);

            // 屏蔽此输入
            if (_isHookBreak)
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Hook鼠标数据结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseHookStruct
    {
        /// <summary>
        /// 鼠标在屏幕上的x,y坐标
        /// </summary>
        public Point Point;
        /// <summary>
        /// 点击窗体的句柄
        /// </summary>
        public IntPtr hWnd;
        /// <summary>
        /// <see cref="WM.WM_NCHITTEST"/> 消息
        /// </summary>
        public int wHitTestCode;
        /// <summary>
        /// 扩展信息,可以使用GetMessageExtraInfo的返回值
        /// </summary>
        public int dwExtraInfo;

        /// <summary>
        /// 从 IntPtr 创建鼠标钩子结构
        /// </summary>
        /// <param name="lParam">指向结构体的指针</param>
        /// <returns>鼠标钩子结构体</returns>
        public static MouseHookStruct Create(IntPtr lParam)
        {
            return (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
        }

        /// <summary>
        /// 将当前结构体转换为IntPtr
        /// </summary>
        /// <param name="lParam">目标指针</param>
        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }
    }


    #region IDisposable接口相关函数
    /// <summary>
    /// 获取对象是否已释放
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
    ~MouseHook()
    {
        Dispose(false);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否由 Dispose 方法调用</param>
    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放,并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;
        UnHook();
    }
    #endregion
}