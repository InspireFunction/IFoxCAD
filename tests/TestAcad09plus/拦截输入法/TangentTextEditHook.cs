namespace Gstar_IMEFilter;

/// <summary>
/// 天正单行文字编辑框钩子 by 小叶|Moy QQ:838840554
/// </summary>
internal class TangentTextEditHook : IDisposable
{
    #region Windows Api
    // 钩子:钩进程的窗体创建事件.
    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // 事件常量
    private const uint EVENT_OBJECT_CREATE = 0x8000;
    private const uint EVENT_OBJECT_DESTROY = 0x8001;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

    #endregion

    // 委托定义
    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

    private IntPtr _hookHandle;
    private WinEventDelegate _eventDelegate;

    /// <summary>
    /// 事件:天正单行文字编辑框 - 窗体创建后
    /// </summary>
    internal event Action<IntPtr>? Created;

    /// <summary>
    /// 事件:天正单行文字编辑框 - 窗体销毁后
    /// </summary>
    internal event Action<IntPtr>? Destroyed;

    /// <summary>
    /// 新建钩子
    /// </summary>
    internal TangentTextEditHook()
    {
        uint processId = 0;
        GetWindowThreadProcessId(Acap.MainWindow.Handle, out processId);
        _eventDelegate = new WinEventDelegate(WinEventProc);
        _hookHandle = SetWinEventHook(
        EVENT_OBJECT_CREATE,
        EVENT_OBJECT_DESTROY,
        IntPtr.Zero,
        _eventDelegate,
        processId,  // 只监控cad线程
        0,  // 不指定进程
        WINEVENT_OUTOFCONTEXT
    );
    }

    /// <summary>
    /// 检查目标句柄是否是天正单行文字编辑框
    /// 特征1.编辑框是CAD MainWindow的子窗体(高版本是MdiActiveDocument.Window的子窗体)
    /// 特征2.编辑框窗体类名是"Edit"
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    private bool IsTangentTextEdit(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return false;
        // 获取目标句柄的窗体类名
        StringBuilder className = new StringBuilder(256);
        GetClassName(hWnd, className, className.Capacity);
        // 判断是不是天正单行文字编辑框窗体类名
        if (className.ToString() != "Edit") return false;
        // 判断是不是Acap.MainWindow的子窗体
        return GetParent(hWnd) == Acap.MainWindow.Handle;
    }


    private IntPtr? _currentEditHwnd;//缓存Edit句柄

    // 事件处理函数
    private void WinEventProc(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        // 天正单行文字编辑框:窗体创建
        if (eventType == EVENT_OBJECT_CREATE && IsTangentTextEdit(hwnd))
        {
            if (_currentEditHwnd is not null)// 窗体show时会触发两次EVENT_OBJECT_CREATE消息
                return;
            _currentEditHwnd = hwnd;
            Created?.Invoke(hwnd);
        }

        // 天正单行文字编辑框:窗体销毁
        // 窗体销毁后GetClassName返回空,所以不要进行IsTangentTextEdit判断,用缓存的_currentEditHwnd判断
        if (eventType == EVENT_OBJECT_DESTROY && hwnd == _currentEditHwnd)
        {
            _currentEditHwnd = null;
            Destroyed?.Invoke(hwnd);
        }
    }

    /// <summary>
    /// 卸载Hook
    /// </summary>
    private void UnHook()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWinEvent(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    /// <summary>
    /// 卸载Hook
    /// </summary>
    public void Dispose() => UnHook();
}
