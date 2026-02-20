namespace Gstar_IMEFilter;

using IFoxCAD.Basal;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;


//#line hidden


public class IMEControl
{
    static readonly Regex CMDReg = new("\\(C:.*\\)");

    /*某些窗口没有 WM_KEYDOWN 消息，就只有 WM_KEYUP 消息*/
    const int WM_KEYDOWN = 256;
    const int WM_KEYUP = 257;

    static WindowsAPI.CallBack? _hookProc;
    static IntPtr _nextHookProc;
    static Process _process;

    static IMEControl()
    {
        _nextHookProc = IntPtr.Zero;
        _process = Process.GetCurrentProcess();
        WindowsAPI.CheckLowLevelHooksTimeout();

        AcadIdleManager.OnIdleOnce(() => {
            // 命令反应器
            var dm = Acap.DocumentManager;
            if (dm.Count != 0)
                foreach (Document doc in dm)
                {
                    doc.CommandWillStart += Doc_CommandWillStart;
                    doc.CommandEnded += Doc_CommandEnded;
                    doc.CommandCancelled += Doc_CommandCancelled; ;
                }

            // 卸载钩子
            Acap.QuitWillStart += (s, e) => {
                IMEControl.UnIMEHook();
            };
        });
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
    static CtrlState _sendKeyState = new();


    // 取消命令
    static void Doc_CommandCancelled(object sender, CommandEventArgs e)
    {
        Doc_CommandEndedOrCancel(sender, e);
    }

    // 命令结束反应器
    static void Doc_CommandEnded(object sender, CommandEventArgs e)
    {
        Doc_CommandEndedOrCancel(sender, e);
    }

    static void Doc_CommandEndedOrCancel(object sender, CommandEventArgs e)
    {
        try
        {
            /*
             * 英文状态 IsStop
             * 英文状态和被程序切换 IsStop && IsExceptional
             * 中文状态 IsBreak
             * 中文状态和被程序切换 IsBreak && IsExceptional
             * 保持不变 IsCancel
             * 钩子不走任何 !_sendKeyState.Run 状态
             */
            // 如果程序切换了,就恢复原本的
            // 当前是英文状态被切换到中文(当前),{然后用户切换了英文,此时应该保证是用户},而不是发送切换(会这样变成中文)
            if (Settings.AutoEn2Cn.Contains(e.GlobalCommandName))
            {
                if (_sendKeyState.IsStop && _sendKeyState.IsExceptional)
                {
                    if (IsOpenIEM())
                    {
                        SendKey("");
                        DebugEx.Printl("恢复}", false);
                    }
                    else
                    {
                        DebugEx.Printl("不恢复}");
                    }
                }
                _sendKeyState.Reset();
            }
            if (Settings.AutoCn2En.Contains(e.GlobalCommandName))
            {
                if (_sendKeyState.IsBreak && _sendKeyState.IsExceptional)
                {
                    if (!IsOpenIEM())
                    {
                        SendKey("");
                        DebugEx.Printl("恢复}");
                    }
                    else
                    {
                        DebugEx.Printl("不恢复}");
                    }
                }
                _sendKeyState.Reset();
            }
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"Doc_CommandEnded 异常: {ex.Message}");
            _sendKeyState.Reset();
        }
    }
    // 命令开始反应器
    static void Doc_CommandWillStart(object sender, CommandEventArgs e)
    {
        DebugEx.Printl("Doc_CommandWillStart: " + e.GlobalCommandName);
        try
        {
            if (Settings.AutoCn2En.Contains(e.GlobalCommandName))
            {
                IMESwitch_AutoCn2En();
                return;
            }
            else if (Settings.AutoEn2Cn.Contains(e.GlobalCommandName))
            {
                IMESwitch_AutoEn2Cn();
                return;
            }
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"Doc_CommandWillStart 异常: {ex.Message}");
        }
    }


    /// <summary>
    /// 如果是中文输入法状态就是true
    /// </summary>
    /// <returns></returns>
    static bool IsOpenIEM()
    {
        try
        {
            var focusW = WindowsAPI.GetForegroundWindowSafe();
            if (focusW == IntPtr.Zero || !WindowsAPI.IsWindow(focusW))
                return false;

            var context = WindowsAPI.ImmGetContext(focusW);
            if (context == IntPtr.Zero)
            {
                DebugEx.Printl($"[IsOpenIEM] ImmGetContext 返回空指针");
                return false;
            }

            bool statusResult = WindowsAPI.ImmGetConversionStatus(context, out int mode/*输入模式*/, out _);
            if (!statusResult)
            {
                DebugEx.Printl($"[IsOpenIEM] ImmGetConversionStatus 调用失败");
            }

            bool openStatus = WindowsAPI.ImmGetOpenStatus(context);
            return openStatus;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[IsOpenIEM] 异常: {ex.Message}");
            return false;
        }
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
            DebugEx.Printl("现在是英文状态,切换前{");
            _sendKeyState.Stop();
            _sendKeyState.Exceptional(true);
            SendKey("中文");
        }
        else
        {
            // 中文状态虽然不变,
            // 但是为了避免命令中用户手动切换 中文切换到英文,然后再触发上面 英文状态转中文逻辑,
            // 所以此处也要设置状态
            DebugEx.Printl("现在是中文状态,保持不变");
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
            DebugEx.Printl("现在是英文状态,保持不变");
            _sendKeyState.Cancel();
        }
        else
        {
            DebugEx.Printl("现在是中文状态,切换前{");
            _sendKeyState.Break();
            _sendKeyState.Exceptional(true);
            SendKey("英文");
        }
    }

    static void SendKey(string msg)
    {
        DebugEx.Printl($"触发了切换输入法 {msg}", false);
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
        try
        {
            UnIMEHook();
            if (_nextHookProc != IntPtr.Zero)
                return;

            if (Settings.IMEHookStyle == IMEHookStyle.Process)
            {
                DebugEx.Printl($"切换到进程钩子控制:{DateTime.Now}");
                _hookProc = (nCode, wParam, lParam) => {
                    try
                    {
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
                                    DebugEx.Printl($"进程钩子按了这个{Control.ModifierKeys}^{DateTime.Now}");
                                    if (IMEHook(nCode, wParam, lParam))
                                    {
                                        DebugEx.Printl($"进程钩子拦截成功^{DateTime.Now}");
                                        return (IntPtr)1;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 吞掉异常，防止崩溃
                        Debugger.Break();
                    }
                    return WindowsAPI.CallNextHookExSafe(_nextHookProc, nCode, wParam, lParam);
                };
                _nextHookProc = WindowsAPI.SetWindowsHookExSafe(
                    HookType.WH_KEYBOARD, _hookProc, IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
            }
            else if (Settings.IMEHookStyle == IMEHookStyle.Global)
            {
                DebugEx.Printl($"切换到全局钩子控制:{DateTime.Now}");
                var moduleHandle = WindowsAPI.GetModuleHandleSafe(_process.MainModule.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    DebugEx.Printl("全局钩子: 获取模块句柄失败，回退到进程钩子");
                    Settings.IMEHookStyle = IMEHookStyle.Process;
                    SetIMEHook();
                    return;
                }
                _hookProc = (nCode, wParam, lParam) => {
                    try
                    {
                        if (nCode >= 0)
                        {
                            if (!MK1(wParam) && Mk2(nCode, wParam, lParam))
                                return (IntPtr)1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        DebugEx.Printl($"全局钩子回调异常: {ex.Message}");
                    }
                    return WindowsAPI.CallNextHookExSafe(_nextHookProc, nCode, wParam, lParam);
                };

                _nextHookProc = WindowsAPI.SetWindowsHookExSafe(
                    HookType.WH_KEYBOARD_LL, _hookProc, moduleHandle, 0);
                if (_nextHookProc == IntPtr.Zero)
                {
                    DebugEx.Printl("全局钩子: 设置钩子失败，回退到进程钩子");
                    Debugger.Break();
                    Settings.IMEHookStyle = IMEHookStyle.Process;
                    SetIMEHook();
                    return;
                }
            }

            _TangentTextEditHook = new();
        }
        catch (Exception ex)
        {
            Debugger.Break();
            DebugEx.Printl($"SetIMEHook 异常: {ex.Message}");
            UnIMEHook();
        }
    }

    /// <summary>
    /// 天正窗口拦截
    /// </summary>
    static TangentTextEditHook? _TangentTextEditHook;


    /// <summary>
    /// 豁免命令处理
    /// </summary>
    /// <returns></returns>
    static bool ExceptCmds_AutoEn2Cn_Task()
    {
        try
        {
            // 重复校验
            var focus = WindowsAPI.GetFocusSafe();
            if (focus == IntPtr.Zero || !WindowsAPI.IsWindow(focus))
            {
                DebugEx.Printl("[ExceptCmds_AutoEn2Cn_Task] 获取焦点窗口失败或窗口无效");
                return false;
            }

            if (!WindowsAPI.IsWindowEnabled(Acap.MainWindow.Handle))
            {
                DebugEx.Printl("[ExceptCmds_AutoEn2Cn_Task] 主窗口未启用");
                return false;
            }

            var dm = Acap.DocumentManager;
            if (dm == null || dm.Count == 0)
                return false;
            var doc = dm.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return false;

            // 豁免命令进行中输入会触发,实现在豁免命令中允许输入法
            string input = doc.CommandInProgress;
            Match match = CMDReg.Match(input);
            if (match.Success)
                input = input.Substring(checked(match.Index + 3), checked(match.Length - 4));

            if (Settings.AutoEn2Cn.Contains(input.ToUpper()))
            {
                IMESwitch_AutoEn2Cn();
                return false;
            }
            return true;
        }
        catch
        {
            Debugger.Break();
            return false;
        }
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
        try
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

                // 必须先焦点
                IntPtr focus;
                if (Marshal.SizeOf(typeof(IntPtr)) == 4)
                    focus = WindowsAPI.GetFocusSafe();
                else
                    focus = WindowsAPI.GetForegroundWindowSafe();
                if (focus == IntPtr.Zero || !WindowsAPI.IsWindow(focus))
                {
                    DebugEx.Printl("[IMEHook] 获取焦点窗口失败或窗口无效");
                    return false;
                }

                // 判断键入的数字更快,再判断豁免命令
                if (!ExceptCmds_AutoEn2Cn_Task())
                    return false;

                StringBuilder lpClassName = new(byte.MaxValue);
                if (!WindowsAPI.GetClassNameSafe(focus, lpClassName, checked(lpClassName.Capacity + 1)))
                {
                    DebugEx.Printl("[IMEHook] GetClassNameSafe 失败");
                    return false;
                }
                string left = lpClassName.ToString().ToLower();
                if (left.StartsWith("afx"))// 在08输入的都从这里进入
                {
                    {
                        var focusW = WindowsAPI.GetForegroundWindowSafe();
                        if (focusW == IntPtr.Zero || !WindowsAPI.IsWindow(focusW))
                        {
                            DebugEx.Printl("[IMEHook] 获取前台窗口失败或窗口无效");
                            return false;
                        }

                        StringBuilder lpClassName2 = new(byte.MaxValue);
                        if (!WindowsAPI.GetClassNameSafe(focusW, lpClassName2, checked(lpClassName2.Capacity + 1)))
                        {
                            DebugEx.Printl("[IMEHook] GetClassNameSafe(前台窗口) 失败");
                            return false;
                        }
                        left = lpClassName2.ToString().ToLower();
                        // cad08启动时候会滚动某些信息,此时鼠标狂点入到vs代码编辑器中,然后等一段时间cad完成,vs就会无法输入了.
                        // 会被拦截到了这个"afx"处理,所有的输入都跑cad了,需要加入如下代码进行处理:
                        // 狂点鼠标进入vs是 hwndwrapper
                        // 狂点鼠标进入qq是 txguifoundation
                        // Debugx.Printl($"afx...{left}...{DateTime.Now}");
                        if (!left.StartsWith("afx"))
                        {
                            DebugEx.Printl($"afx...拦截...{DateTime.Now}");
                            return false;
                        }
                    }
                    if (!WindowsAPI.PostMessageSafe(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x10001)))
                    {
                        DebugEx.Printl("[IMEHook] PostMessageSafe(WM_KEYDOWN) 失败");
                    }
                    return true;
                }

                if (left.StartsWith("hwndwrapper"))//cad21会进入
                {
                    DebugEx.Printl($"hwndwrapper::{DateTime.Now}");

                    var parent = WindowsAPI.GetParentSafe(focus);
                    if (parent == IntPtr.Zero)
                    {
                        DebugEx.Printl("[IMEHook] GetParentSafe 返回空指针");
                        return false;
                    }
                    StringBuilder lpString = new(byte.MaxValue);
                    if (!WindowsAPI.GetWindowTextSafe(parent, lpString, checked(lpString.Capacity + 1)))
                    {
                        DebugEx.Printl("[IMEHook] GetWindowTextSafe 失败");
                        return false;
                    }
                    if (lpString.ToString().ToLower() != "cli palette")//"CLI Palette".ToLower()
                    {
                        StringBuilder lpClassName3 = new(byte.MaxValue);
                        if (!WindowsAPI.GetClassNameSafe(parent, lpClassName3, checked(lpClassName3.Capacity + 1)))
                        {
                            DebugEx.Printl("[IMEHook] GetClassNameSafe(父窗口) 失败");
                            return false;
                        }
                        if (!lpClassName3.ToString().ToLower().StartsWith("afxmdiframe"))
                            return false;
                    }
                    if (!WindowsAPI.PostMessageSafe(focus, WM_KEYUP, new IntPtr(wParam), new IntPtr(0x10001)))
                    {
                        DebugEx.Printl("[IMEHook] PostMessageSafe(WM_KEYUP) 失败");
                    }
                    return true;
                }

                if (left.StartsWith("edit"))
                {
                    DebugEx.Printl($"edit::{DateTime.Now}");

                    var parent = WindowsAPI.GetParentSafe(focus);
                    if (parent == IntPtr.Zero)
                    {
                        DebugEx.Printl("[IMEHook] GetParentSafe(edit) 返回空指针");
                        return false;
                    }

                    StringBuilder lpClassName4 = new(byte.MaxValue);
                    if (!WindowsAPI.GetClassNameSafe(parent, lpClassName4, checked(lpClassName4.Capacity + 1)))
                    {
                        DebugEx.Printl("[IMEHook] GetClassNameSafe(edit父窗口) 失败");
                        return false;
                    }

                    var dm = Acap.DocumentManager;
                    if (dm == null || dm.Count == 0)
                        return false;

                    var doc = dm.MdiActiveDocument;
                    if (doc == null || doc.IsDisposed)
                        return false;

                    if (lpClassName4.ToString().ToLower().StartsWith("afx") &&
                        WindowsAPI.GetParentSafe(parent) != doc.Window.Handle)
                    {
                        if (!WindowsAPI.PostMessageSafe(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x390001/*3735553*/)))
                        {
                            DebugEx.Printl("[IMEHook] PostMessageSafe(edit WM_KEYDOWN) 失败");
                        }
                        return true;
                    }
                }

                if (left == "cicerouiwndframe")
                {
                    DebugEx.Printl($"cicerouiwndframe::{DateTime.Now}");
                    return true;
                }
            }
            return false;
        }
        catch
        {
            Debugger.Break();
            return false;
        }
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
        try
        {
            IntPtr focus;
            if (Marshal.SizeOf(typeof(IntPtr)) == 4)
                focus = WindowsAPI.GetFocusSafe();
            else
                focus = WindowsAPI.GetForegroundWindowSafe();

            if (focus == IntPtr.Zero || !WindowsAPI.IsWindow(focus))
            {
                DebugEx.Printl("[Mk2] 获取焦点窗口失败或窗口无效");
                return false;
            }

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
        catch (Exception ex)
        {
            DebugEx.Printl($"[Mk2] 异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 卸载钩子
    /// </summary>
    internal static void UnIMEHook()
    {
        try
        {
            if (_nextHookProc != IntPtr.Zero)
            {
                WindowsAPI.UnhookWindowsHookExSafe(_nextHookProc);
                _nextHookProc = IntPtr.Zero;

                _TangentTextEditHook?.Dispose();
                _TangentTextEditHook = null;
            }
        }
        catch (Exception)
        {
        }
    }

}
#line default