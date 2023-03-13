namespace IFoxCAD.Basal;

public class MouseHook
{
    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    public event MouseEventHandler? MouseDown;
    /// <summary>
    /// 松开鼠标事件
    /// </summary>
    public event MouseEventHandler? MouseUp;
    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    public event MouseEventHandler? MouseMove;
    /// <summary>
    /// 鼠标滚轮事件
    /// </summary>
    public event MouseEventHandler? MouseWheel;
    /// <summary>
    /// 鼠标单击事件
    /// </summary>
    public event EventHandler? Click;
    /// <summary>
    /// 鼠标双击事件
    /// </summary>
    public event EventHandler? DoubleClick;


    bool _isHookBreak = false;
    /// <summary>
    /// 否决本次输入:设置不向下回调
    /// </summary>
    public void Vote()
    {
        _isHookBreak = true;
    }

    /// 不要试图省略此变量,否则将会导致GC变量池满后释放<br/>
    /// 提示:激活 CallbackOnCollectedDelegate 托管调试助手(MDA)
    internal static WindowsAPI.CallBack? HookProc;
    internal static IntPtr _NextHookProc;//挂载成功的标记
    public readonly Process Process;


    [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]
    public extern static int GetDoubleClickTime();
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
            WindowsAPI.UnhookWindowsHookEx(_NextHookProc);
            _NextHookProc = IntPtr.Zero;
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

        if (processHook)
        {
            HookProc = (nCode, wParam, lParam) => {
                if (nCode >= 0 && HookTask(nCode, wParam, lParam))
                    return (IntPtr)1;
                return WindowsAPI.CallNextHookEx(_NextHookProc, nCode, wParam, lParam);
            };
            _NextHookProc = WindowsAPI.SetWindowsHookEx(HookType.WH_MOUSE, HookProc,
                                                        IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
        }
        else
        {
            var moduleHandle = WindowsAPI.GetModuleHandle(Process.MainModule.ModuleName);
            HookProc = (nCode, wParam, lParam) => {
                if (nCode >= 0 && HookTask(nCode, wParam, lParam))
                    return (IntPtr)1;
                return WindowsAPI.CallNextHookEx(_NextHookProc, nCode, wParam, lParam);
            };
            _NextHookProc = WindowsAPI.SetWindowsHookEx(HookType.WH_MOUSE_LL, HookProc,
                                                        moduleHandle, 0);
        }
    }



    MouseButtons _button;
    int _clickCount = 0;
    bool _down = false;
    bool _up = false;
    bool _ck = false;
    bool _wheel = false;
    bool _move = false;

    /// <summary>
    /// 钩子的消息处理
    /// </summary>
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns>false不终止回调,true终止回调</returns>
    bool HookTask(int nCode, int wParam, IntPtr lParam)
    {
        if (MouseDown is null
         && MouseUp is null
         && MouseMove is null
         && MouseWheel is null
         && Click is null
         && DoubleClick is null)
            return false;

        _button = MouseButtons.None;
        _clickCount = 0;
        _down = false;
        _up = false;
        _ck = false;
        _wheel = false;
        _move = false;

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
            case WM.WM_MBUTTONUP:
            _button = MouseButtons.Middle;
            _clickCount = 1;
            _up = true;
            break;
            case WM.WM_MBUTTONDBLCLK:
            _button = MouseButtons.Middle;
            _clickCount = 2;
            _ck = true;
            break;
            case WM.WM_MOUSEWHEEL:
            _wheel = true;
            // 滚轮
            break;
            case WM.WM_MOUSEMOVE:
            _move = true;
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
            MouseDown?.Invoke(this, e);
        if (_up)
            MouseUp?.Invoke(this, e);
        if (_ck)
            Click?.Invoke(this, e);
        if (_clickCount == 2)
        {
            // 如果不用时间控制,那么双击会执行两次
            if (_watch.Elapsed.TotalMilliseconds > GetDoubleClickTime())
            {
                DoubleClick?.Invoke(this, e);
                _watch.Reset();
                _watch.Start();
            }
        }
        if (_move)
        {
            MouseMove?.Invoke(this, e);
        }
        if (_wheel)
        {
            MouseWheel?.Invoke(this, e);
        }

        // 屏蔽此输入
        if (_isHookBreak)
            return true;

        return false;
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

        public static MouseHookStruct Create(IntPtr lParam)
        {
            return (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
        }

        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }
    }


    #region IDisposable接口相关函数
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

    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放,并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;
        UnHook();
    }
    #endregion
}