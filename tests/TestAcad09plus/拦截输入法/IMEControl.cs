namespace Gstar_IMEFilter;

using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;

public class IMEControl
{
    // 豁免命令组: 默认和配置的
    internal static HashSet<string> DefaultCmds_AutoEn2Cn;
    // 豁免命令组: 默认和配置的+用户面板输入的
    internal static HashSet<string> ExceptCmds_AutoEn2Cn;
    static string _ftFile_AutoEn2Cn;

    // 豁免命令组: 默认和配置的
    internal static HashSet<string> DefaultCmds_AutoCn2En;
    // 括免命令组: 自动切换为英文输入法
    internal static HashSet<string> ExceptCmds_AutoCn2En;
    static string _ftFile_AutoCn2En;

    static readonly Regex CMDReg = new("\\(C:.*\\)");
    /*某些窗口没有 WM_KEYDOWN 消息，就只有 WM_KEYUP 消息*/
    const int WM_KEYDOWN = 256;
    const int WM_KEYUP = 257;

    static WindowsAPI.CallBack? _hookProc;
    static IntPtr _nextHookProc;
    static Process _process;

    // 优化内存,减少消息循环时候,频繁创建此类
    static StringBuilder _lpClassName = new(byte.MaxValue);
    static string[] _separator = new string[] { "," };


    static IMEControl()
    {
        _nextHookProc = IntPtr.Zero;
        _process = Process.GetCurrentProcess();
        WindowsAPI.CheckLowLevelHooksTimeout();

        ExceptCmds_AutoEn2Cn = new();
        {
            DefaultCmds_AutoEn2Cn = new() { "MTEXT", "DDEDIT", "MTEDIT", "TABLEDIT", "MLEADER",
            "QLEADER", "MLEADERCONTENTEDIT", "MLEADEREDIT", "TEXTEDIT", "TEXT", "QLEADER" };
            string? lines = null;
            _ftFile_AutoEn2Cn = Path.Combine(Settings.MyDir, nameof(Gstar_IMEFilter) + nameof(ExceptCmds_AutoEn2Cn) + ".ft");
            if (File.Exists(_ftFile_AutoEn2Cn))
                lines = File.ReadAllText(_ftFile_AutoEn2Cn, Encoding.UTF8);
            else
                Debugx.Printl("配置文件丢失: " + _ftFile_AutoEn2Cn);
            if (lines != null)
            {
                var ls = lines.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ls.Length; i++)
                    DefaultCmds_AutoEn2Cn.Add(ls[i]);
            }
        }
        ExceptCmds_AutoCn2En = new();
        {
            DefaultCmds_AutoCn2En = new() { "BLOCK", "GROUP" };
            string? lines = null;
            _ftFile_AutoCn2En = Path.Combine(Settings.MyDir, nameof(Gstar_IMEFilter) + nameof(ExceptCmds_AutoCn2En) + ".ft");
            if (File.Exists(_ftFile_AutoCn2En))
                lines = File.ReadAllText(_ftFile_AutoCn2En, Encoding.UTF8);
            else
                Debugx.Printl("配置文件丢失: " + _ftFile_AutoCn2En);
            if (lines != null)
            {
                var ls = lines.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ls.Length; i++)
                    DefaultCmds_AutoCn2En.Add(ls[i]);
            }
        }

        // 命令反应器
        var dm = Acap.DocumentManager;
        if (dm.Count != 0)
            foreach (Document doc in dm)
            {
                doc.CommandWillStart += Doc_CommandWillStart;
                doc.CommandEnded += Doc_CommandEnded;
            }

        // 卸载钩子
        Acap.QuitWillStart += (s, e) => {
            IMEControl.UnIMEHook();
            IMEControl.SaveFt();
        };
    }

    public static void SaveFt()
    {
        HashSet<string> lst = new();
        foreach (var item in ExceptCmds_AutoEn2Cn)
            if (!DefaultCmds_AutoEn2Cn.Contains(item))
                lst.Add(item);
        if (lst.Any())
        {
            var jo = string.Join(",", lst.ToArray());
            File.WriteAllText(_ftFile_AutoEn2Cn, jo, Encoding.UTF8);
        }

        lst.Clear();
        foreach (var item in ExceptCmds_AutoCn2En)
            if (!DefaultCmds_AutoCn2En.Contains(item))
                lst.Add(item);
        if (lst.Any())
        {
            var jo = string.Join(",", lst.ToArray());
            File.WriteAllText(_ftFile_AutoCn2En, jo, Encoding.UTF8);
        }
    }

    #region 切换输入法
    // 关键字问题:
    // 是中文输入,自动切换到英文
    // 命令行监控服务 实例 得到命令行监控
    // var commandLineMonitor = CommandLineMonitorServices.Instance().GetCommandLineMonitor(doc);

    /// <summary>
    /// 此状态用于豁免命令图中自动切换到中文,<br/>
    /// 命令中依然能够切换到英文输入,<br/>
    /// 命令结束时候从 命令结束反应器 恢复拦截<br/>
    /// </summary>
    static LoopState _sendKeyState = new();

    // 命令结束反应器
    static void Doc_CommandEnded(object sender, CommandEventArgs e)
    {
        /*
         * 英文状态 IsStop
         * 英文状态和被程序切换 IsStop && IsExceptional
         * 中文状态 IsBreak
         * 中文状态和被程序切换 IsBreak && IsExceptional
         * 保持不变 IsCancel
         * 钩子不走任何 !Run 状态
         */
        // 如果程序切换了,就恢复原本的
        // 当前是英文状态被切换到中文(当前),{然后用户切换了英文,此时应该保证是用户},而不是发送切换(会这样变成中文)
        if (ExceptCmds_AutoEn2Cn.Contains(e.GlobalCommandName))
        {
            if (_sendKeyState.IsStop && _sendKeyState.IsExceptional)
            {
                if (IsOpenIEM())
                {
                    SendKey();
                    Debugx.Printl("恢复}", false);
                }
                else
                {
                    Debugx.Printl("不恢复}");
                }
            }
            _sendKeyState.Reset();
        }
        if (ExceptCmds_AutoCn2En.Contains(e.GlobalCommandName))
        {
            if (_sendKeyState.IsBreak && _sendKeyState.IsExceptional)
            {
                if (!IsOpenIEM())
                {
                    SendKey();
                    Debugx.Printl("恢复}");
                }
                else
                {
                    Debugx.Printl("不恢复}");
                }
            }
            _sendKeyState.Reset();
        }
    }
    // 命令开始反应器
    static void Doc_CommandWillStart(object sender, CommandEventArgs e)
    {
        if (ExceptCmds_AutoCn2En.Contains(e.GlobalCommandName))
        {
            IMESwitch_AutoCn2En();
            return;
        }
    }


    /// <summary>
    /// 如果是中文输入法状态就是true
    /// </summary>
    /// <returns></returns>
    static bool IsOpenIEM()
    {
        var focusW = WindowsAPI.GetForegroundWindow();
        var context = WindowsAPI.ImmGetContext(focusW);
        WindowsAPI.ImmGetConversionStatus(context, out int mode/*输入模式*/, out _);
        return WindowsAPI.ImmGetOpenStatus(context);
    }

    /// <summary>
    /// 切换输入法(英文状态就切到中文)
    /// </summary>
    static void IMESwitch_AutoEn2Cn()
    {
        if (Settings.IMEInputSwitch == IMESwitchMode.NotSwitch)
            return;
        // 切换只能发生在第一次,第2.+次需要不执行
        if (!IsOpenIEM())
        {
            Debugx.Printl("现在是英文状态,切换前{");
            _sendKeyState.Stop();
            _sendKeyState.Exceptional();
            SendKey();
        }
        else
        {
            // 中文状态虽然不变,
            // 但是为了避免命令中用户手动切换 中文切换到英文,然后再触发上面 英文状态转中文逻辑,
            // 所以此处也要设置状态
            Debugx.Printl("现在是中文状态,保持不变");
            _sendKeyState.Cancel();
        }
    }

    /// <summary>
    /// 切换输入法(中文状态就切到英文)
    /// </summary>
    static void IMESwitch_AutoCn2En()
    {
        if (Settings.IMEInputSwitch == IMESwitchMode.NotSwitch)
            return;
        // 切换只能发生在第一次,第2.+次需要不执行
        if (!IsOpenIEM())
        {
            Debugx.Printl("现在是英文状态,保持不变");
            _sendKeyState.Cancel();
        }
        else
        {
            Debugx.Printl("现在是中文状态,切换前{");
            _sendKeyState.Break();
            _sendKeyState.Exceptional();
            SendKey();
        }
    }

    static void SendKey()
    {
        Debugx.Printl("触发了切换输入法", false);
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
    #endregion

    /// <summary>
    /// 设置钩子
    /// </summary>
    internal static void SetIMEHook()
    {
        UnIMEHook();
        if (_nextHookProc != IntPtr.Zero)
            return;

        #region 读取配置
        ExceptCmds_AutoEn2Cn.Clear();
        {
            foreach (var item in DefaultCmds_AutoEn2Cn)
                ExceptCmds_AutoEn2Cn.Add(item);
            var ss = Settings.UserFilter_AutoEn2Cn.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in ss)
                ExceptCmds_AutoEn2Cn.Add(item);
        }

        ExceptCmds_AutoCn2En.Clear();
        {
            foreach (var item in DefaultCmds_AutoCn2En)
                ExceptCmds_AutoCn2En.Add(item);
            var ss = Settings.UserFilter_AutoCn2En.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in ss)
                ExceptCmds_AutoCn2En.Add(item);
        }
        #endregion

        if (Settings.IMEHookStyle == IMEHookStyle.Process)
        {
            Debugx.Printl($"切换到进程钩子控制:{DateTime.Now}");
            _hookProc = (nCode, wParam, lParam) => {
                if (nCode >= 0)
                {
                    // 高版本cad基本上不能用进程钩子:
                    // 搜狗输入法如果连续按着,那么此时拦截失效
                    var lp = lParam.ToInt64();
                    if ((lp > 0) && ((lp & 0xC0000001) == 1))//按下某个键
                    {
                        // 如果是ctrl就跳过
                        if (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Shift)
                        {
                            Debugx.Printl($"进程钩子按了这个{Control.ModifierKeys}^{DateTime.Now}");
                            if (IMEHook(nCode, wParam, lParam))
                            {
                                Debugx.Printl($"进程钩子拦截成功^{DateTime.Now}");
                                return (IntPtr)1;
                            }
                        }
                    }
                }
                return WindowsAPI.CallNextHookEx(_nextHookProc, nCode, wParam, lParam);
            };
            _nextHookProc = WindowsAPI.SetWindowsHookEx(HookType.WH_KEYBOARD, _hookProc,
                                                        IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
            return;
        }

        if (Settings.IMEHookStyle == IMEHookStyle.Global)
        {
            Debugx.Printl($"切换到全局钩子控制:{DateTime.Now}");
            var moduleHandle = WindowsAPI.GetModuleHandle(_process.MainModule.ModuleName);
            _hookProc = (nCode, wParam, lParam) => {
                if (nCode >= 0)
                {
                    if (!MK1(wParam) && Mk2(nCode, wParam, lParam))
                        return (IntPtr)1;
                }
                return WindowsAPI.CallNextHookEx(_nextHookProc, nCode, wParam, lParam);
            };
            _nextHookProc = WindowsAPI.SetWindowsHookEx(HookType.WH_KEYBOARD_LL,
                                                        _hookProc, moduleHandle, 0);
            return;
        }
    }

    /// <summary>
    /// 豁免命令处理
    /// </summary>
    /// <returns></returns>
    static bool ExceptCmds_AutoEn2Cn_Task()
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return false;
        var doc = dm.MdiActiveDocument;
        if (doc == null)
            return false;
        if (!WindowsAPI.IsWindowEnabled(Acap.MainWindow.Handle))
            return false;

        // 豁免命令进行中输入会触发,实现在豁免命令中允许输入法
        string input = doc.CommandInProgress;
        Match match = CMDReg.Match(input);
        if (match.Success)
            input = input.Substring(checked(match.Index + 3), checked(match.Length - 4));

        if (ExceptCmds_AutoEn2Cn.Contains(input.ToUpper()))
        {
            IMESwitch_AutoEn2Cn();
            return false;
        }
        return true;
    }

    /// <summary>
    /// 钩子的消息处理
    /// </summary>r
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns>false不终止回调,true终止回调</returns>
    public static bool IMEHook(int nCode, int wParam, IntPtr lParam)
    {
        if (_sendKeyState.IsExceptional)
            return false;

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
            //Debugx.Printl(wParam);

            // 先判断键入的数字更快,再判断豁免命令
            if (!ExceptCmds_AutoEn2Cn_Task())
                return false;

            var focus = WindowsAPI.GetFocus();
            WindowsAPI.GetClassName(focus, _lpClassName, checked(_lpClassName.Capacity + 1));
            string left = _lpClassName.ToString().ToLower();
            if (left.StartsWith("afx"))// 在08输入的都从这里进入
            {
                {
                    var focusW = WindowsAPI.GetForegroundWindow();
                    WindowsAPI.GetClassName(focusW, _lpClassName, checked(_lpClassName.Capacity + 1));
                    left = _lpClassName.ToString().ToLower();
                    // cad08启动时候会滚动某些信息,此时鼠标狂点入到vs代码编辑器中,然后等一段时间cad完成,vs就会无法输入了.
                    // 会被拦截到了这个"afx"处理,所有的输入都跑cad了,需要加入如下代码进行处理:
                    // 狂点鼠标进入vs是 hwndwrapper
                    // 狂点鼠标进入qq是 txguifoundation
                    // Debugx.Printl($"afx...{left}...{DateTime.Now}");
                    if (!left.StartsWith("afx"))
                    {
                        Debugx.Printl($"afx...拦截...{DateTime.Now}");
                        return false;
                    }
                }
                WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x10001));
                return true;
            }

            if (left.StartsWith("hwndwrapper"))//cad21会进入
            {
                Debugx.Printl($"hwndwrapper::{DateTime.Now}");

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
                Debugx.Printl($"edit::{DateTime.Now}");

                var parent = WindowsAPI.GetParent(focus);
                WindowsAPI.GetClassName(parent, _lpClassName, checked(_lpClassName.Capacity + 1));

                var dm = Acap.DocumentManager;
                var doc = dm.MdiActiveDocument;
                if (_lpClassName.ToString().ToLower().StartsWith("afx") &&
                    WindowsAPI.GetParent(parent) != doc.Window.Handle)
                {
                    WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x390001/*3735553*/));
                    return true;
                }
            }

            if (left == "cicerouiwndframe")
            {
                Debugx.Printl($"cicerouiwndframe::{DateTime.Now}");
                return true;
            }
        }
        return false;
    }

    static bool MK1(int wParam)
    {
        return Settings.IMEInputSwitch == IMESwitchMode.Disable ||
               WindowsAPI.IsIconic(Acap.MainWindow.Handle.ToInt32()) ||
               WindowsAPI.GetKeyState(91) < 0 ||
               WindowsAPI.GetKeyState(92) < 0 ||
               wParam != WM_KEYDOWN;
    }

    static bool Mk2(int nCode, int wParam, IntPtr lParam)
    {
        IntPtr focus;
        if (Marshal.SizeOf(typeof(IntPtr)) == 4)
            focus = WindowsAPI.GetFocus();
        else
            focus = WindowsAPI.GetForegroundWindow();

        WindowsAPI.GetWindowThreadProcessId(focus, out uint lpdwProcessId);
        if (lpdwProcessId != _process.Id)
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

            if (IMEHook(nCode, key.Value.VkCode, IntPtr.Zero))
                return true;
        }
        else if (Control.ModifierKeys == Keys.Shift)
        {
            key ??= WindowsAPI.KeyboardHookStruct.Create(lParam);
            if (IMEHook(nCode, key.Value.VkCode, IntPtr.Zero))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 卸载钩子
    /// </summary>
    internal static void UnIMEHook()
    {
        if (_nextHookProc != IntPtr.Zero)
        {
            WindowsAPI.UnhookWindowsHookEx(_nextHookProc);
            _nextHookProc = IntPtr.Zero;
        }
    }
}