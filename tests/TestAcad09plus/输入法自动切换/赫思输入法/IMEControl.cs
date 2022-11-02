namespace Gstar_IMEFilter;

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;

public class IMEControl
{
    internal static HashSet<string> DefaultCMDs;
    internal static HashSet<string> ExceptCMDs;

    static readonly Regex CMDReg = new("\\(C:.*\\)");
    /*某些窗口没有 WM_KEYDOWN 消息，就只有 WM_KEYUP 消息*/
    const int WM_KEYDOWN = 256;
    const int WM_KEYUP = 257;

    internal static WindowsAPI.CallBackX86? HookProcX86;
    internal static WindowsAPI.CallBackX64? HookProcX64;
    internal static WindowsAPI.CallBack? HookProc;
    internal static IntPtr _NextHookProc;//挂载成功的标记
    internal static Process _Process;

    // 优化内存,减少消息循环时候,频繁创建此类
    static StringBuilder _lpClassName = new(byte.MaxValue);
    static string[] _separator = new string[] { "," };

    static IMEControl()
    {
        _NextHookProc = IntPtr.Zero;
        _Process = Process.GetCurrentProcess();
        WindowsAPI.CheckLowLevelHooksTimeout();

        ExceptCMDs = new();
        DefaultCMDs = new() { "MTEXT", "DDEDIT", "MTEDIT", "TABLEDIT", "MLEADER",
            "QLEADER", "MLEADERCONTENTEDIT", "MLEADEREDIT", "TEXTEDIT", "TEXT", "QLEADER" };

        string? lines = null;
        var ftFile = Path.Combine(Settings.MyDir, "ExceptCMDs.ft");
        if (File.Exists(ftFile))
            lines = File.ReadAllText(ftFile, Encoding.UTF8);
        else
            Debug.WriteLine("配置文件丢失: " + ftFile);

        if (lines != null)
        {
            var ls = lines.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ls.Length; i++)
                DefaultCMDs.Add(ls[i]);
        }

        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
    }

    #region 切换输入法
    // TODO 关键字是中文输入,自动切换到英文
    // 命令行监控服务 实例 得到命令行监控
    // var commandLineMonitor = CommandLineMonitorServices.Instance().GetCommandLineMonitor(doc);

    /// <summary>
    /// 此状态用于括免命令图中自动切换到中文,<br/>
    /// 命令中依然能够切换到英文输入,<br/>
    /// 命令结束时候从否决事件恢复拦截<br/>
    /// </summary>
    public static LoopState SendKeyState = new();
    static void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (e.GlobalCommandName == "#")
            return;
        if (!e.GlobalCommandName.StartsWith("#"))
            return;
        var up = e.GlobalCommandName.ToUpper()[1..];
        if (ExceptCMDs.Contains(up))
            SendKeyState = new();
    }

    /// <summary>
    /// 切换输入法
    /// </summary>
    static void IMESwitch()
    {
        // 检测是否中文状态,不是就切为中文状态
        // 切换只能发生在第一次,第2+次需要不执行
        var focusW = WindowsAPI.GetForegroundWindow();
        var context = WindowsAPI.ImmGetContext(focusW);
        WindowsAPI.ImmGetConversionStatus(context, out int mode/*输入模式*/, out _);
        if (!WindowsAPI.ImmGetOpenStatus(context))
        {
            Debug.WriteLine("现在是英文状态,切换到中文");
            SendKeyState.Exceptional();

            switch (Settings.IMEInputSwitch)
            {
                case IMESwitchMode.Shift:
                {
                    WindowsAPI.KeybdEvent(16, 0, 0, 0);
                    WindowsAPI.KeybdEvent(16, 0, 2, 0);
                }
                break;
                case IMESwitchMode.Ctrl:
                {
                    WindowsAPI.KeybdEvent(17, 0, 0, 0);
                    WindowsAPI.KeybdEvent(17, 0, 2, 0);
                }
                break;
                case IMESwitchMode.CtrlAndSpace:
                {
                    WindowsAPI.KeybdEvent(17, 0, 0, 0);
                    WindowsAPI.KeybdEvent(32, 0, 0, 0);
                    WindowsAPI.KeybdEvent(32, 0, 2, 0);
                    WindowsAPI.KeybdEvent(17, 0, 2, 0);
                    if (WindowsAPI.GetKeyState(20) == 1)
                    {
                        WindowsAPI.KeybdEvent(20, 0, 0, 0);
                        WindowsAPI.KeybdEvent(20, 0, 2, 0);
                    }
                }
                break;
                case IMESwitchMode.CtrlAndShift:
                {
                    WindowsAPI.KeybdEvent(16, 0, 0, 0);
                    WindowsAPI.KeybdEvent(17, 0, 0, 0);
                    WindowsAPI.KeybdEvent(17, 0, 2, 0);
                    WindowsAPI.KeybdEvent(16, 0, 2, 0);
                    if (WindowsAPI.GetKeyState(20) == 1)
                    {
                        WindowsAPI.KeybdEvent(20, 0, 0, 0);
                        WindowsAPI.KeybdEvent(20, 0, 2, 0);
                    }
                }
                break;
                case IMESwitchMode.WinAndSpace:
                {
                    WindowsAPI.KeybdEvent(91, 0, 0, 0);
                    WindowsAPI.KeybdEvent(32, 0, 0, 0);
                    WindowsAPI.KeybdEvent(32, 0, 2, 0);
                    WindowsAPI.KeybdEvent(91, 0, 2, 0);
                    if (WindowsAPI.GetKeyState(20) == 1)
                    {
                        WindowsAPI.KeybdEvent(20, 0, 0, 0);
                        WindowsAPI.KeybdEvent(20, 0, 2, 0);
                    }
                }
                break;
            }
        }
        else
        {
            // 中文状态虽然不变,
            // 但是为了避免命令中用户手动切换 中文切换到英文,然后再触发上面 英文状态转中文逻辑,
            // 所以此处也要设置状态
            // 在命令否决事件上面恢复拦截
            Debug.WriteLine("现在是中文状态,保持不变");
            SendKeyState.Exceptional();
        }
    }
    #endregion

    /// <summary>
    /// 设置钩子
    /// </summary>
    internal static void SetIMEHook()
    {
        UnIMEHook();
        if (_NextHookProc != IntPtr.Zero || !Settings.Use)
            return;

        ExceptCMDs.Clear();
        foreach (var item in DefaultCMDs)
            ExceptCMDs.Add(item);
        var ss = Settings.UserFilter.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in ss)
            ExceptCMDs.Add(item);

        if (Settings.IMEStyle != IMEHookStyle.Global)
        {
            HookProc = (nCode, wParam, lParam) => {
                if (nCode >= 0)
                {
                    bool flag = !((lParam > 0) & ((lParam & -1073741823) == 1)) ||
                                (Control.ModifierKeys != Keys.None && Control.ModifierKeys != Keys.Shift) ||
                                !IMEHook(nCode, wParam, lParam);
                    if (!flag)
                        return (IntPtr)1;
                }
                return WindowsAPI.CallNextHookEx(_NextHookProc, nCode, wParam, lParam);
            };
            _NextHookProc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD, HookProc, 0, WindowsAPI.GetCurrentThreadId());
            return;
        }

        var moduleHandle = (long)WindowsAPI.GetModuleHandle(_Process.MainModule.ModuleName);
        if (Marshal.SizeOf(typeof(IntPtr)) == 4)
        {
            HookProcX86 = (nCode, wParam, lParam) => {
                if (!MK1(nCode, wParam) && Mk2(nCode, wParam, new IntPtr(lParam)))
                    return (IntPtr)1;
                return WindowsAPI.CallNextHookEx(_NextHookProc, nCode, wParam, lParam);
            };
            _NextHookProc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX86, moduleHandle, 0);
        }
        else
        {
            HookProcX64 = (nCode, wParam, lParam) => {
                if (!MK1(nCode, wParam) && Mk2(nCode, wParam, new IntPtr(lParam)))
                    return (IntPtr)1;
                return WindowsAPI.CallNextHookEx(_NextHookProc, nCode, wParam, lParam);
            };
            _NextHookProc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX64, moduleHandle, 0);
        }
    }

    /// <summary>
    /// 钩子的消息处理
    /// </summary>r
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns>false不终止回调,true终止回调</returns>
    public static bool IMEHook(int nCode, int wParam, int lParam)
    {
        if (SendKeyState.IsExceptional)
            return false;

        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return false;
        var doc = dm.MdiActiveDocument;
        if (doc == null)
            return false;
        if (!WindowsAPI.IsWindowEnabled(Acap.MainWindow.Handle))
            return false;

        // 括免命令进行中输入会触发,实现在括免命令中允许输入法
        string input = doc.CommandInProgress;
        Match match = CMDReg.Match(input);
        if (match.Success)
            input = input.Substring(checked(match.Index + 3), checked(match.Length - 4));

        if (ExceptCMDs.Contains(input.ToUpper()))
        {
            IMESwitch();
            return false;
        }

        // 键盘按键值
        if ((65 <= wParam && wParam <= 90/*a~z*/) ||
            (48 <= wParam && wParam <= 57/*数字键*/) ||
            (96 <= wParam && wParam <= 105/*小数字键盘数字*/) ||
             wParam == 27/*esc*/ ||
             wParam == 32/*空格*/ ||
             wParam == 13/*回车,大小回车都是它*/ ||
             wParam == 186/*;*/ ||
             wParam == 187/*=*/ ||
             wParam == 188/*,*/ ||
             wParam == 189/*-*/ ||
             wParam == 190/*.*/ ||
             wParam == 191/*?*/ ||
             wParam == 192/*`~*/ ||
             wParam == 219/*[*/ ||
             wParam == 220/*\*/ ||
             wParam == 221/*]*/ ||
             wParam == 222/*'*/ ||
             wParam == 223 ||
             wParam == 110/*小数字键盘.*/)
        {
            //Debug.WriteLine(wParam);

            var focus = WindowsAPI.GetFocus();
            WindowsAPI.GetClassName(focus, _lpClassName, checked(_lpClassName.Capacity + 1));
            string left = _lpClassName.ToString().ToLower();
            if (left.StartsWith("afx"))// 在08输入的都从这里进入
            {
                Debug.WriteLine($"afx::{DateTime.Now}");

                {
                    // cad08启动时候会滚动某些信息,此时立马鼠标狂点入到vs中,vs就会无法输入
                    // 会被拦截到了"afx"处理,然后所有的输入都跑cad了,需要加入如下代码:
                    var focusW = WindowsAPI.GetForegroundWindow();
                    WindowsAPI.GetClassName(focusW, _lpClassName, checked(_lpClassName.Capacity + 1));
                    left = _lpClassName.ToString().ToLower();
                    // 狂点入vs是 hwndwrapper
                    // 狂点入qq是 txguifoundation
                    Debug.WriteLine($"afx...{left}...{DateTime.Now}");
                    if (!left.StartsWith("afx"))
                        return false;
                }

                WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x10001));
                return true;
            }

            if (left.StartsWith("hwndwrapper"))//cad21会进入,高版本的命令提示器?
            {
                Debug.WriteLine($"hwndwrapper::{DateTime.Now}");

                var parent = WindowsAPI.GetParent(focus);
                if (parent == IntPtr.Zero)
                    return false;
                StringBuilder lpString = new(byte.MaxValue);
                WindowsAPI.GetWindowText(parent, lpString, checked(lpString.Capacity + 1));
                if (lpString.ToString().ToLower() != "cli palette")//"CLI Palette".ToLower()
                {
                    WindowsAPI.GetClassName(parent, _lpClassName, checked(_lpClassName.Capacity + 1));
                    if (!_lpClassName.ToString().ToLower().StartsWith("afxmdiframe"))
                        return false;
                }
                WindowsAPI.PostMessage(focus, WM_KEYUP, new IntPtr(wParam), new IntPtr(0x10001));
                return true;
            }

            if (left.StartsWith("edit"))
            {
                Debug.WriteLine($"edit::{DateTime.Now}");

                var parent = WindowsAPI.GetParent(focus);
                WindowsAPI.GetClassName(parent, _lpClassName, checked(_lpClassName.Capacity + 1));
                if (_lpClassName.ToString().ToLower().StartsWith("afx") &&
                    WindowsAPI.GetParent(parent) != doc.Window.Handle)
                {
                    WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(3735553));
                    return true;
                }
            }

            if (left == "cicerouiwndframe")
            {
                Debug.WriteLine($"cicerouiwndframe::{DateTime.Now}");
                return true;
            }
        }
        return false;
    }

    static bool MK1(int nCode, int wParam)
    {
        return !Settings.Use ||
                WindowsAPI.IsIconic(Acap.MainWindow.Handle.ToInt32()) ||
                WindowsAPI.GetKeyState(91) < 0 ||
                WindowsAPI.GetKeyState(92) < 0 ||
                wParam != WM_KEYDOWN ||
                nCode < 0;
    }

    static bool Mk2(int nCode, int wParam, IntPtr lParam)
    {
        IntPtr focus;
        if (Marshal.SizeOf(typeof(IntPtr)) == 4)
            focus = WindowsAPI.GetFocus();
        else
            focus = WindowsAPI.GetForegroundWindow();

        WindowsAPI.GetWindowThreadProcessId(focus, out uint lpdwProcessId);
        if (lpdwProcessId != _Process.Id)
            return false;

        WindowsAPI.KeyboardHookStruct? key = null;
        if (Control.ModifierKeys == Keys.None)
        {
            key = WindowsAPI.KeyboardHookStruct.Create(lParam);
            if (WindowsAPI.GetKeyState(162) < 0 ||
                WindowsAPI.GetKeyState(163) < 0 ||
                WindowsAPI.GetKeyState(17) < 0 ||
                WindowsAPI.GetKeyState(262144/*alt键*/) < 0)
                return false;

            if (IMEHook(nCode, key.Value.VkCode, 0))
                return true;
        }
        else if (Control.ModifierKeys == Keys.Shift)
        {
            key ??= WindowsAPI.KeyboardHookStruct.Create(lParam);
            if (IMEHook(nCode, key.Value.VkCode, 0))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 卸载钩子
    /// </summary>
    internal static void UnIMEHook()
    {
        if (_NextHookProc != IntPtr.Zero)
        {
            WindowsAPI.UnhookWindowsHookEx(_NextHookProc);
            _NextHookProc = IntPtr.Zero;
        }
    }
}