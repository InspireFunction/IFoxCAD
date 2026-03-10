using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Test;

/// <summary>
/// 窗口边框绘制器 - 用于在AutoCAD文档视图周围绘制红色边框，标识在位编辑器运行状态
/// 使用子类化拦截窗口消息，实现边框跟随，无闪烁
/// </summary>
public class WindowBorderDrawer : IDisposable
{
    #region Win32 API 声明

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    private static extern int GetLastError();

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // Windows 消息常量
    private const uint WM_PAINT = 0x000F;
    private const uint WM_SIZE = 0x0005;
    private const uint WM_MOVE = 0x0003;
    private const uint WM_WINDOWPOSCHANGED = 0x0047;
    private const uint WM_WINDOWPOSCHANGING = 0x0046;
    private const uint WM_NCPAINT = 0x0085;
    private const uint WM_ERASEBKGND = 0x0014;
    private const uint WM_SYSCOMMAND = 0x0112;
    private const uint WM_EXITSIZEMOVE = 0x0232;
    private const uint WM_ENTERSIZEMOVE = 0x0231;
    private const uint WM_GETMINMAXINFO = 0x0024;
    private const uint WM_DISPLAYCHANGE = 0x007E;
    private const uint WM_SHOWWINDOW = 0x0018;
    private const uint WM_ACTIVATE = 0x0006;
    private const uint WM_MDIACTIVATE = 0x0222;
    private const uint WM_TIMER = 0x0113;

    // SC 命令
    private const uint SC_MAXIMIZE = 0xF030;
    private const uint SC_MINIMIZE = 0xF020;
    private const uint SC_RESTORE = 0xF120;

    // RedrawWindow 标志
    private const uint RDW_INVALIDATE = 0x0001;
    private const uint RDW_ERASE = 0x0004;
    private const uint RDW_FRAME = 0x0400;
    private const uint RDW_ALLCHILDREN = 0x0080;
    private const uint RDW_UPDATENOW = 0x0100;

    // 画笔样式
    private const int PS_SOLID = 0;

    // 颜色转换：RGB to COLORREF
    private static uint RGB(byte r, byte g, byte b) => (uint)(r | (g << 8) | (b << 16));

    #endregion

    #region 字段

    /// <summary>
    /// 目标文档窗口句柄
    /// </summary>
    private IntPtr _docHwnd = IntPtr.Zero;

    /// <summary>
    /// MDIClient 窗口句柄
    /// </summary>
    private IntPtr _mdiClientHwnd = IntPtr.Zero;

    /// <summary>
    /// 主窗口句柄
    /// </summary>
    private IntPtr _mainHwnd = IntPtr.Zero;

    /// <summary>
    /// 窗口过程拦截器列表
    /// </summary>
    private List<AcadWindowProc> _windowProcs = [];

    /// <summary>
    /// 边框宽度
    /// </summary>
    private readonly int _borderWidth;

    /// <summary>
    /// 边框颜色
    /// </summary>
    private readonly uint _borderColor;

    /// <summary>
    /// 是否正在绘制
    /// </summary>
    private bool _isActive = false;

    /// <summary>
    /// 定时刷新定时器
    /// </summary>
    private System.Windows.Forms.Timer? _refreshTimer;

    #endregion

    #region 事件

    /// <summary>
    /// 窗口消息事件 - 当拦截到指定消息时触发
    /// </summary>
    public event EventHandler<WindowMessageEventArgs>? WindowMessage;

    #endregion

    #region 构造函数

    /// <summary>
    /// 创建窗口边框绘制器
    /// </summary>
    /// <param name="borderWidth">边框宽度</param>
    /// <param name="r">红色分量 0-255</param>
    /// <param name="g">绿色分量 0-255</param>
    /// <param name="b">蓝色分量 0-255</param>
    public WindowBorderDrawer(int borderWidth, byte r = 255, byte g = 0, byte b = 0)
    {
        _borderWidth = borderWidth;
        _borderColor = RGB(r, g, b);
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 开始在指定窗口周围绘制边框
    /// </summary>
    /// <param name="docHwnd">文档窗口句柄</param>
    /// <param name="mainHwnd">主窗口句柄</param>
    /// <returns>是否启动成功</returns>
    public bool Start(IntPtr docHwnd, IntPtr mainHwnd)
    {
        if (docHwnd == IntPtr.Zero)
        {
            Debug.WriteLine("[WindowBorderDrawer.Start] 文档窗口句柄为空");
            return false;
        }

        // 如果已经在运行，先停止
        if (_isActive)
        {
            Stop();
        }

        _docHwnd = docHwnd;
        _mainHwnd = mainHwnd;

        // 查找 MDIClient 窗口
        _mdiClientHwnd = FindWindowEx(mainHwnd, IntPtr.Zero, "MDIClient", null);
        Debug.WriteLine($"[WindowBorderDrawer.Start] MDIClient 句柄: {_mdiClientHwnd}");

        _isActive = true;

        try
        {
            // 创建文档窗口的过程拦截器
            var docProc = new AcadWindowProc(docHwnd);
            docProc.MessageFilter = (msg) => OnWindowMessage(msg, "Doc");
            _windowProcs.Add(docProc);

            // 如果找到 MDIClient，也拦截它的消息
            if (_mdiClientHwnd != IntPtr.Zero)
            {
                var mdiProc = new AcadWindowProc(_mdiClientHwnd);
                mdiProc.MessageFilter = (msg) => OnWindowMessage(msg, "MDIClient");
                _windowProcs.Add(mdiProc);
            }

            // 拦截主窗口消息（用于最大化/最小化等）
            if (mainHwnd != IntPtr.Zero && mainHwnd != docHwnd)
            {
                var mainProc = new AcadWindowProc(mainHwnd);
                mainProc.MessageFilter = (msg) => OnWindowMessage(msg, "Main");
                _windowProcs.Add(mainProc);
            }

            // 启动定时刷新定时器（每100ms刷新一次，确保边框始终显示）
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 100;
            _refreshTimer.Tick += (s, e) => {
                if (_isActive && _docHwnd != IntPtr.Zero)
                {
                    DrawBorder();
                }
            };
            _refreshTimer.Start();

            // 立即绘制一次
            DrawBorder();

            Debug.WriteLine($"[WindowBorderDrawer.Start] 边框绘制已启动，拦截 {_windowProcs.Count} 个窗口");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WindowBorderDrawer.Start] 启动失败: {ex.Message}");
            Stop();
            return false;
        }
    }

    /// <summary>
    /// 停止绘制边框
    /// </summary>
    public void Stop()
    {
        _isActive = false;

        // 停止定时刷新定时器
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _refreshTimer = null;
        }

        // 释放所有窗口过程拦截器
        foreach (var proc in _windowProcs)
        {
            try
            {
                proc.MessageFilter = null;
                proc.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WindowBorderDrawer.Stop] 释放拦截器失败: {ex.Message}");
            }
        }
        _windowProcs.Clear();

        // 清除边框
        ClearBorder();

        _docHwnd = IntPtr.Zero;
        _mdiClientHwnd = IntPtr.Zero;
        _mainHwnd = IntPtr.Zero;

        Debug.WriteLine("[WindowBorderDrawer.Stop] 边框绘制已停止");
    }

    /// <summary>
    /// 重新绘制边框（手动刷新）
    /// </summary>
    public void Refresh()
    {
        if (_isActive && _docHwnd != IntPtr.Zero)
        {
            DrawBorder();
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 窗口消息处理回调
    /// </summary>
    /// <param name="message">窗口消息</param>
    /// <param name="source">消息来源</param>
    /// <returns>是否继续调用原窗口过程</returns>
    private bool OnWindowMessage(Message message, string source)
    {
        // 触发消息事件
        WindowMessage?.Invoke(this, new WindowMessageEventArgs(message, source));

        uint msg = (uint)message.Msg;
        bool needRedraw = false;

        // 检查是否需要重绘边框的消息
        switch (msg)
        {
            // 窗口重绘 - 必须处理，否则边框会被擦除
            case WM_PAINT:
            case WM_ERASEBKGND:
            needRedraw = true;
            Debug.WriteLine($"[WindowBorderDrawer] [{source}] 收到消息 {GetMessageName(msg)}，需要重绘");
            break;

            // 窗口大小/位置变化
            case WM_SIZE:
            case WM_MOVE:
            case WM_WINDOWPOSCHANGED:
            case WM_WINDOWPOSCHANGING:
            case WM_NCPAINT:
            case WM_EXITSIZEMOVE:
            case WM_MDIACTIVATE:
            needRedraw = true;
            Debug.WriteLine($"[WindowBorderDrawer] [{source}] 收到消息 {GetMessageName(msg)}，需要重绘");
            break;

            // 系统命令（最大化/最小化/还原）
            case WM_SYSCOMMAND:
            uint cmd = (uint)message.WParam.ToInt32() & 0xFFF0;
            if (cmd == SC_MAXIMIZE || cmd == SC_MINIMIZE || cmd == SC_RESTORE)
            {
                needRedraw = true;
                Debug.WriteLine($"[WindowBorderDrawer] [{source}] 收到系统命令 {cmd:X}，需要重绘");
            }
            break;

            // 显示/激活变化
            case WM_SHOWWINDOW:
            case WM_ACTIVATE:
            needRedraw = true;
            Debug.WriteLine($"[WindowBorderDrawer] [{source}] 收到消息 {GetMessageName(msg)}，需要重绘");
            break;
        }

        if (needRedraw)
        {
            // 延迟重绘，确保窗口布局已完成
            BeginDrawBorder();
        }

        // 继续调用原窗口过程
        return true;
    }

    /// <summary>
    /// 获取消息名称（用于调试）
    /// </summary>
    private string GetMessageName(uint msg)
    {
        return msg switch
        {
            WM_PAINT => "WM_PAINT",
            WM_SIZE => "WM_SIZE",
            WM_MOVE => "WM_MOVE",
            WM_WINDOWPOSCHANGED => "WM_WINDOWPOSCHANGED",
            WM_WINDOWPOSCHANGING => "WM_WINDOWPOSCHANGING",
            WM_NCPAINT => "WM_NCPAINT",
            WM_ERASEBKGND => "WM_ERASEBKGND",
            WM_SYSCOMMAND => "WM_SYSCOMMAND",
            WM_EXITSIZEMOVE => "WM_EXITSIZEMOVE",
            WM_ENTERSIZEMOVE => "WM_ENTERSIZEMOVE",
            WM_SHOWWINDOW => "WM_SHOWWINDOW",
            WM_ACTIVATE => "WM_ACTIVATE",
            WM_MDIACTIVATE => "WM_MDIACTIVATE",
            WM_TIMER => "WM_TIMER",
            _ => $"0x{msg:X4}"
        };
    }

    /// <summary>
    /// 异步开始绘制边框（避免在消息回调中直接绘制）
    /// </summary>
    private void BeginDrawBorder()
    {
        // 使用延迟执行确保窗口布局已完成
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 10; // 10ms 延迟
        timer.Tick += (s, e) => {
            timer.Stop();
            timer.Dispose();
            DrawBorder();
        };
        timer.Start();
    }

    /// <summary>
    /// 立即绘制边框（直接绘制到屏幕DC）
    /// </summary>
    private void DrawBorder()
    {
        if (!_isActive || _docHwnd == IntPtr.Zero)
            return;

        IntPtr windowDc = IntPtr.Zero;
        IntPtr redPen = IntPtr.Zero;
        IntPtr oldPen = IntPtr.Zero;

        try
        {
            // 获取窗口设备上下文
            windowDc = GetWindowDC(_docHwnd);
            if (windowDc == IntPtr.Zero)
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] GetWindowDC 失败，错误码: {error}");
                return;
            }

            // 获取窗口矩形（相对于屏幕）
            if (!GetWindowRect(_docHwnd, out RECT windowRect))
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] GetWindowRect 失败，错误码: {error}");
                return;
            }

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            // 获取客户区矩形
            if (!GetClientRect(_docHwnd, out RECT clientRect))
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] GetClientRect 失败，错误码: {error}");
                return;
            }

            // 转换客户区坐标到屏幕坐标
            POINT clientTopLeft = new POINT { X = clientRect.Left, Y = clientRect.Top };
            if (!ClientToScreen(_docHwnd, ref clientTopLeft))
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] ClientToScreen 失败，错误码: {error}");
                return;
            }

            POINT clientBottomRight = new POINT { X = clientRect.Right, Y = clientRect.Bottom };
            if (!ClientToScreen(_docHwnd, ref clientBottomRight))
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] ClientToScreen 失败，错误码: {error}");
                return;
            }

            // 计算非客户区尺寸
            int captionHeight = clientTopLeft.Y - windowRect.Top;  // 标题栏+上边框
            int leftBorder = clientTopLeft.X - windowRect.Left;    // 左边框
            int rightBorder = windowRect.Right - clientBottomRight.X; // 右边框
            int bottomBorder = windowRect.Bottom - clientBottomRight.Y; // 下边框

            // 创建红色画笔（1像素宽度）
            redPen = CreatePen(PS_SOLID, 1, _borderColor);
            if (redPen == IntPtr.Zero)
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] CreatePen 失败，错误码: {error}");
                return;
            }

            oldPen = SelectObject(windowDc, redPen);
            if (oldPen == IntPtr.Zero)
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] SelectObject 失败，错误码: {error}");
                DeleteObject(redPen);
                redPen = IntPtr.Zero;
                return;
            }

            // 在非客户区绘制边框
            // 绘制多条线实现加粗效果
            for (int i = 0; i < _borderWidth; i++)
            {
                int offset = i;

                // 上边（在标题栏区域下方）
                if (!MoveToEx(windowDc, leftBorder + offset, captionHeight + offset, IntPtr.Zero))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] MoveToEx 失败，错误码: {GetLastError()}");
                }
                if (!LineTo(windowDc, width - rightBorder - offset, captionHeight + offset))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] LineTo 失败，错误码: {GetLastError()}");
                }

                // 下边
                if (!MoveToEx(windowDc, leftBorder + offset, height - bottomBorder - offset - 1, IntPtr.Zero))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] MoveToEx 失败，错误码: {GetLastError()}");
                }
                if (!LineTo(windowDc, width - rightBorder - offset, height - bottomBorder - offset - 1))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] LineTo 失败，错误码: {GetLastError()}");
                }

                // 左边
                if (!MoveToEx(windowDc, leftBorder + offset, captionHeight + offset, IntPtr.Zero))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] MoveToEx 失败，错误码: {GetLastError()}");
                }
                if (!LineTo(windowDc, leftBorder + offset, height - bottomBorder - offset))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] LineTo 失败，错误码: {GetLastError()}");
                }

                // 右边
                if (!MoveToEx(windowDc, width - rightBorder - offset - 1, captionHeight + offset, IntPtr.Zero))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] MoveToEx 失败，错误码: {GetLastError()}");
                }
                if (!LineTo(windowDc, width - rightBorder - offset - 1, height - bottomBorder - offset))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] LineTo 失败，错误码: {GetLastError()}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] 绘制失败: {ex.Message}");
            Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] 堆栈跟踪: {ex.StackTrace}");
        }
        finally
        {
            // 恢复原始画笔
            if (oldPen != IntPtr.Zero && windowDc != IntPtr.Zero)
            {
                if (SelectObject(windowDc, oldPen) == IntPtr.Zero)
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] 恢复画笔失败，错误码: {GetLastError()}");
                }
            }

            // 删除红色画笔
            if (redPen != IntPtr.Zero)
            {
                if (!DeleteObject(redPen))
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] DeleteObject 失败，错误码: {GetLastError()}");
                }
            }

            // 释放窗口DC
            if (windowDc != IntPtr.Zero && _docHwnd != IntPtr.Zero)
            {
                int result = ReleaseDC(_docHwnd, windowDc);
                if (result != 1) // ReleaseDC 成功返回 1
                {
                    Debug.WriteLine($"[WindowBorderDrawer.DrawBorder] ReleaseDC 失败，错误码: {GetLastError()}");
                }
            }
        }
    }

    /// <summary>
    /// 清除绘制的边框
    /// </summary>
    private void ClearBorder()
    {
        if (_docHwnd == IntPtr.Zero)
            return;

        try
        {
            // 使用RedrawWindow刷新窗口，清除绘制的线条
            if (!RedrawWindow(_docHwnd, IntPtr.Zero, IntPtr.Zero,
                RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW))
            {
                int error = GetLastError();
                Debug.WriteLine($"[WindowBorderDrawer.ClearBorder] RedrawWindow 失败，错误码: {error}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WindowBorderDrawer.ClearBorder] 清除失败: {ex.Message}");
            Debug.WriteLine($"[WindowBorderDrawer.ClearBorder] 堆栈跟踪: {ex.StackTrace}");
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~WindowBorderDrawer()
    {
        if (_isActive)
        {
            try
            {
                Stop();
            }
            catch
            {
                // 忽略析构函数中的异常
            }
        }
    }

    #endregion
}

/// <summary>
/// 窗口消息事件参数
/// </summary>
public class WindowMessageEventArgs : EventArgs
{
    /// <summary>
    /// 窗口消息
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// 消息来源
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public uint Msg => (uint)Message.Msg;

    /// <summary>
    /// wParam参数
    /// </summary>
    public IntPtr WParam => Message.WParam;

    /// <summary>
    /// lParam参数
    /// </summary>
    public IntPtr LParam => Message.LParam;

    /// <summary>
    /// 窗口句柄
    /// </summary>
    public IntPtr HWnd => Message.HWnd;

    /// <summary>
    /// 创建窗口消息事件参数
    /// </summary>
    /// <param name="message">窗口消息</param>
    /// <param name="source">消息来源</param>
    public WindowMessageEventArgs(Message message, string source)
    {
        Message = message;
        Source = source;
    }

    /// <summary>
    /// 获取消息名称
    /// </summary>
    public string MessageName
    {
        get
        {
            return Msg switch
            {
                0x000F => "WM_PAINT",
                0x0005 => "WM_SIZE",
                0x0003 => "WM_MOVE",
                0x0047 => "WM_WINDOWPOSCHANGED",
                0x0046 => "WM_WINDOWPOSCHANGING",
                0x0085 => "WM_NCPAINT",
                0x0014 => "WM_ERASEBKGND",
                0x0112 => "WM_SYSCOMMAND",
                0x0232 => "WM_EXITSIZEMOVE",
                0x0231 => "WM_ENTERSIZEMOVE",
                0x0018 => "WM_SHOWWINDOW",
                0x0006 => "WM_ACTIVATE",
                0x0222 => "WM_MDIACTIVATE",
                _ => $"0x{Msg:X4}"
            };
        }
    }
}
