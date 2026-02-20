namespace IFoxCAD.Basal;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// 提供键盘钩子功能的类，用于捕获和处理键盘事件。
/// 支持进程级和全局级的键盘钩子设置。
/// </summary>
public class KeyboardHook : IDisposable
{
    /// <summary>
    /// 键盘按下事件
    /// </summary>
    public event KeyEventHandler? KeyDown;

    /// <summary>
    /// 键盘抬起事件
    /// </summary>
    public event KeyEventHandler? KeyUp;

    /// <summary>
    /// 键盘按下事件（字符级别）
    /// </summary>
    public event KeyPressEventHandler? KeyPress;


    bool _isHookBreak = false;

    /// <summary>
    /// 否决本次输入：设置不向下回调
    /// </summary>
    public void Vote()
    {
        _isHookBreak = true;
    }

    /// <summary>
    /// 不要试图省略此变量，否则将会导致GC变量池满后释放。<br/>
    /// 提示：激活 CallbackOnCollectedDelegate 托管调试助手(MDA)。
    /// </summary>
    WindowsAPI.CallBack? _hookProc;

    /// <summary>
    /// 挂载成功的标记
    /// </summary>
    IntPtr _nextHookProc;

    /// <summary>
    /// 当前进程
    /// </summary>
    public readonly Process Process;

    /// <summary>
    /// 钩子类型
    /// </summary>
    public HookType HookType { get; private set; }

    /// <summary>
    /// 安装键盘钩子
    /// </summary>
    /// <param name="setLowLevel">低级钩子超时时间</param>
    public KeyboardHook(int setLowLevel = 25000)
    {
        _nextHookProc = IntPtr.Zero;
        Process = Process.GetCurrentProcess();
        WindowsAPI.CheckLowLevelHooksTimeout(setLowLevel);
    }

    /// <summary>
    /// 卸载钩子
    /// </summary>
    void UnHook()
    {
        if (_nextHookProc != IntPtr.Zero)
        {
            try
            {
                WindowsAPI.UnhookWindowsHookExSafe(_nextHookProc);
            }
            catch (BadImageFormatException ex)
            {
                DebugEx.Printl($"[KeyboardHook.UnHook] BadImageFormatException: {ex.Message}");
            }
            catch (Exception ex)
            {
                DebugEx.Printl($"[KeyboardHook.UnHook] 异常: {ex.Message}");
            }
            finally
            {
                _nextHookProc = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// 设置进程级钩子
    /// </summary>
    public void SetProcessHook()
    {
        SetHook(HookType.WH_KEYBOARD, IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
    }

    /// <summary>
    /// 设置全局钩子
    /// </summary>
    public bool SetGlobalHook()
    {
        var moduleHandle = WindowsAPI.GetModuleHandle(Process.MainModule?.ModuleName ?? Process.ProcessName);
        if (moduleHandle == IntPtr.Zero)
        {
            DebugEx.Printl("[KeyboardHook.SetGlobalHook] 获取模块句柄失败");
            return false;
        }
        SetHook(HookType.WH_KEYBOARD_LL, moduleHandle, 0);
        return _nextHookProc != IntPtr.Zero;
    }

    /// <summary>
    /// 设置钩子
    /// </summary>
    /// <param name="hookType">钩子类型</param>
    /// <param name="moduleHandle">模块句柄</param>
    /// <param name="threadId">线程ID</param>
    void SetHook(HookType hookType, IntPtr moduleHandle, int threadId)
    {
        UnHook();
        if (_nextHookProc != IntPtr.Zero)
            return;

        try
        {
            HookType = hookType;

            // 创建委托并保持引用，防止GC回收
            _hookProc = (nCode, wParam, lParam) =>
            {
                try
                {
                    if (nCode >= 0 && HookTask(nCode, wParam, lParam))
                        return (IntPtr)1;
                    return WindowsAPI.CallNextHookExSafe(_nextHookProc, nCode, wParam, lParam);
                }
                catch (BadImageFormatException ex)
                {
                    DebugEx.Printl($"[KeyboardHook.HookProc] BadImageFormatException: {ex.Message}");
                    return WindowsAPI.CallNextHookExSafe(_nextHookProc, nCode, wParam, lParam);
                }
                catch (Exception ex)
                {
                    DebugEx.Printl($"[KeyboardHook.HookProc] 异常: {ex.Message}");
                    return WindowsAPI.CallNextHookExSafe(_nextHookProc, nCode, wParam, lParam);
                }
            };

            _nextHookProc = WindowsAPI.SetWindowsHookExSafe(hookType, _hookProc, moduleHandle, threadId);

            if (_nextHookProc == IntPtr.Zero)
            {
                DebugEx.Printl($"[KeyboardHook.SetHook] 钩子安装失败，类型: {hookType}");
                _hookProc = null;
            }
            else
            {
                DebugEx.Printl($"[KeyboardHook.SetHook] 钩子安装成功，类型: {hookType}");
            }
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[KeyboardHook.SetHook] BadImageFormatException: {ex.Message}");
            _hookProc = null;
            _nextHookProc = IntPtr.Zero;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[KeyboardHook.SetHook] 异常: {ex.Message}");
            _hookProc = null;
            _nextHookProc = IntPtr.Zero;
        }
    }

    /// <summary>
    /// 钩子的消息处理
    /// </summary>
    /// <param name="nCode">钩子代码</param>
    /// <param name="wParam">消息参数</param>
    /// <param name="lParam">消息参数</param>
    /// <returns>false不终止回调，true终止回调</returns>
    bool HookTask(int nCode, int wParam, IntPtr lParam)
    {
        try
        {
            if (KeyDown is null && KeyUp is null && KeyPress is null)
                return false;

            // 从回调函数中得到键盘的信息
            var keyHookStruct = WindowsAPI.KeyboardHookStruct.Create(lParam);
            var keyData = (Keys)keyHookStruct.VkCode;

            // 判断按键状态
            bool isKeyDown = wParam == (int)WM.WM_KEYDOWN || wParam == (int)WM.WM_SYSKEYDOWN;
            bool isKeyUp = wParam == (int)WM.WM_KEYUP || wParam == (int)WM.WM_SYSKEYUP;

            // 获取修饰键状态
            bool control = (WindowsAPI.GetKeyState((int)Keys.ControlKey) & 0x8000) != 0;
            bool shift = (WindowsAPI.GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
            bool alt = (WindowsAPI.GetKeyState((int)Keys.Menu) & 0x8000) != 0;

            // 创建 KeyEventArgs
            KeyEventArgs keyEventArgs = new(keyData);

            if (isKeyDown)
            {
                KeyDown?.Invoke(this, keyEventArgs);
            }
            else if (isKeyUp)
            {
                KeyUp?.Invoke(this, keyEventArgs);
            }

            // 屏蔽此输入
            if (_isHookBreak)
                return true;

            return keyEventArgs.Handled;
        }
        catch
        {
            return false;
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
    ~KeyboardHook()
    {
        Dispose(false);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否由 Dispose 方法调用</param>
    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放，并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;
        UnHook();
    }

    #endregion
}
