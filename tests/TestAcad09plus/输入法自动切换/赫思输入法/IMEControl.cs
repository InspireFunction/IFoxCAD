namespace Gstar_IMEFilter;

using System.Diagnostics;
using System.Windows.Forms;

public class IMEControl
{
    static readonly Regex CMDReg = new("\\(C:.*\\)");
    /*某些窗口没有 WM_KEYDOWN 消息，就只有 WM_KEYUP 消息*/
    const int WM_KEYDOWN = 256;
    const int WM_KEYUP = 257;
    //const int WM_CHAR = 258;
    //const int WM_KILLFOCUS = 8;

    internal static string[] DefaultCMDs;
    internal static List<string> ExceptCMDs;
    internal static IntPtr hnexthookproc;
    internal static WindowsAPI.CallBackX86? HookProcX86;
    internal static WindowsAPI.CallBackX64? HookProcX64;
    internal static WindowsAPI.CallBack? HookProc;

    internal static Process _Process;
    internal static int AcadPID => _Process.Id;

    static IMEControl()
    {
        DefaultCMDs = new string[] { "MTEXT", "DDEDIT", "MTEDIT", "TABLEDIT", "MLEADER", "QLEADER", "MLEADERCONTENTEDIT", "MLEADEREDIT", "TEXTEDIT", "TEXT", "QLEADER" };
        ExceptCMDs = new();
        _Process = Process.GetCurrentProcess();
        hnexthookproc = IntPtr.Zero;
        CheckLowLevelHooksTimeout();

        string? lines = null;
        var ftFile = Path.Combine(Settings.MyDir, "ExceptCMDs.ft");
        if (File.Exists(ftFile))
            lines = File.ReadAllText(ftFile, Encoding.UTF8);
        else
            Debug.WriteLine("配置文件丢失: " + ftFile);

        if (lines != null)
            DefaultCMDs = lines.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
    }

    // 优化内存,减少消息循环时候,频繁创建此类
    static StringBuilder _lpClassName = new(byte.MaxValue);

    public static bool IMEHook(int nCode, int wParam, int lParam)
    {
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
            //Debug.WriteLine(wParam);

            var focus = WindowsAPI.GetFocus();
            WindowsAPI.GetClassName(focus, _lpClassName, checked(_lpClassName.Capacity + 1));

            string left = _lpClassName.ToString().ToLower();
            if (left.StartsWith("afx"))// 在08输入的都从这里进入
            {
                //Debug.WriteLine("afx");

                WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(0x10001));
                return true;
            }

            if (left.StartsWith("hwndwrapper"))//cad21会进入,高版本的命令提示器?
            {
                //Debug.WriteLine("hwndwrapper");

                var parent = WindowsAPI.GetParent(focus);
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
                //Debug.WriteLine("edit");

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
                //Debug.WriteLine("cicerouiwndframe");
                return true;
            }
        }
        return false;
    }

    internal static void UnIMEHook()
    {
        if (hnexthookproc != IntPtr.Zero)
        {
            WindowsAPI.UnhookWindowsHookEx(hnexthookproc);
            hnexthookproc = IntPtr.Zero;
        }
    }

    internal static void SetIMEHook()
    {
        UnIMEHook();
        ExceptCMDs.Clear();
        if (hnexthookproc != IntPtr.Zero || !Settings.Use)
            return;

        ExceptCMDs.AddRange(DefaultCMDs);
        ExceptCMDs.AddRange(Settings.UserFilter.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

        if (Settings.IMEStyle != Settings.IMEStyleS.Global)
        {
            HookProc = (int nCode, int wParam, int lParam) => {
                if (nCode >= 0)
                {
                    bool flag = !(lParam > 0 & (lParam & -1073741823) == 1) ||
                                (Control.ModifierKeys != Keys.None && Control.ModifierKeys != Keys.Shift) ||
                                !IMEHook(nCode, wParam, lParam);
                    if (!flag)
                        return (IntPtr)1;
                }
                return (IntPtr)WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
            };
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD, HookProc, 0, WindowsAPI.GetCurrentThreadId());
            return;
        }

        var moduleHandle = (long)WindowsAPI.GetModuleHandle(_Process.MainModule.ModuleName);
        if (Marshal.SizeOf(typeof(IntPtr)) == 4)
        {
            HookProcX86 = (nCode, wParam, lParam) => {
                if (!MK1(nCode, wParam) && Mk2(nCode, wParam, new IntPtr(lParam)))
                    return (IntPtr)1;
                return (IntPtr)WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
            };
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX86, moduleHandle, 0);
        }
        else
        {
            HookProcX64 = (nCode, wParam, lParam) => {
                if (!MK1(nCode, wParam) && Mk2(nCode, wParam, new IntPtr(lParam)))
                    return (IntPtr)1;
                return (IntPtr)WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
            };
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX64, moduleHandle, 0);
        }
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
        if (lpdwProcessId != AcadPID)
            return false;

        if (Control.ModifierKeys == Keys.None)
        {
            var hook = KeyboardHookStruct.Create(lParam);
            if (WindowsAPI.GetKeyState(162) < 0 ||
                WindowsAPI.GetKeyState(163) < 0 ||
                WindowsAPI.GetKeyState(17) < 0 ||
                WindowsAPI.GetKeyState(262144) < 0)
                return false;
            if (IMEHook(nCode, hook.vkCode, 0))
                return true;
        }
        if (Control.ModifierKeys == Keys.Shift)
        {
            var hook = KeyboardHookStruct.Create(lParam);
            if (IMEHook(nCode, hook.vkCode, 0))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 注册表增加低级钩子超时处理,防止系统不允许,
    /// 否则:偶发性出现 键盘钩子不能用了,而且退出时产生 1404 错误
    /// https://www.cnblogs.com/songr/p/5131655.html
    /// </summary>
    static void CheckLowLevelHooksTimeout()
    {
        const string llh = "LowLevelHooksTimeout";
        using var registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
        if ((int)registryKey.GetValue(llh, 0) == 0)
            registryKey.SetValue(llh, 25000, RegistryValueKind.DWord);
    }


    [ComVisible(true)]
    [Serializable]
    //[DebuggerDisplay("{DebuggerDisplay,nq}")]
    //[DebuggerTypeProxy(typeof(KeyboardHookStruct))]
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardHookStruct
    {
        public int vkCode;
        public int ScanCode;
        public int Flags;
        public int Time;
        public int DwExtraInfo;

        public static KeyboardHookStruct Create(IntPtr intPtr)
        {
            return (KeyboardHookStruct)Marshal.PtrToStructure(intPtr, typeof(KeyboardHookStruct));
        }
    }
}