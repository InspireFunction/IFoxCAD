using System.Diagnostics;
using System.Windows.Forms;

namespace Gstar_IMEFilter;

public class IMEControl
{
    static readonly Regex CMDReg = new("\\(C:.*\\)");
    const int WM_KEYDOWN = 256;
    const int WM_KEYUP = 257;
    //const int WM_CHAR = 258;
    //const int WM_KILLFOCUS = 8;

    internal static string[] DefaultCMDs;
    internal static List<string> ExceptCMDs;
    internal static int AcadPID;
    internal static IntPtr hnexthookproc;
    internal static WindowsAPI.CallBackX86? HookProcX86;
    internal static WindowsAPI.CallBackX64? HookProcX64;
    internal static WindowsAPI.CallBack? HookProc;

    static IMEControl()
    {
        DefaultCMDs = new string[] { "MTEXT", "DDEDIT", "MTEDIT", "TABLEDIT", "MLEADER", "QLEADER", "MLEADERCONTENTEDIT", "MLEADEREDIT", "TEXTEDIT", "TEXT", "QLEADER" };
        ExceptCMDs = new();
        AcadPID = Process.GetCurrentProcess().Id;
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

        string input = doc.CommandInProgress;
        Match match = CMDReg.Match(input);
        if (match.Success)
            input = input.Substring(checked(match.Index + 3), checked(match.Length - 4));

        if (ExceptCMDs.Contains(input.ToUpper()))
            return false;

        if (wParam >= 65 && wParam <= 90
            || (wParam == 32 || wParam == 188)
            || (wParam == 190 || wParam == 222 || (wParam == 189 || wParam == 13))
            || (wParam == 187 || wParam == 186 || (wParam == 191 || wParam == 192) ||
               (wParam == 219 || wParam == 220 || (wParam == 221 || wParam == 222)))
            || (wParam == 223 || wParam >= 48 && wParam <= 57 || (wParam == 27 || wParam >= 96 && wParam <= 105) || wParam == 110))
        {
            StringBuilder lpClassName1 = new(byte.MaxValue);
            IntPtr focus = WindowsAPI.GetFocus();
            WindowsAPI.GetClassName(focus, lpClassName1, checked(lpClassName1.Capacity + 1));

            string left = lpClassName1.ToString().ToLower();
            if (left.StartsWith("afx"))
            {
                WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(65537));
                return true;
            }

            if (left.StartsWith("hwndwrapper"))
            {
                IntPtr parent = WindowsAPI.GetParent(focus);

                StringBuilder lpString = new(byte.MaxValue);
                WindowsAPI.GetWindowText(parent, lpString, checked(lpString.Capacity + 1));
                if (lpString.ToString().ToLower() == "CLI Palette".ToLower())
                {
                    WindowsAPI.PostMessage(focus, WM_KEYUP, new IntPtr(wParam), new IntPtr(65537));
                    return true;
                }

                StringBuilder lpClassName2 = new(byte.MaxValue);
                WindowsAPI.GetClassName(parent, lpClassName2, checked(lpClassName2.Capacity + 1));
                if (lpClassName2.ToString().ToLower().StartsWith("afxmdiframe"))
                {
                    WindowsAPI.PostMessage(focus, WM_KEYUP, new IntPtr(wParam), new IntPtr(65537));
                    return true;
                }
            }

            if (left.StartsWith("edit"))
            {
                IntPtr parent = WindowsAPI.GetParent(focus);
                StringBuilder lpClassName2 = new(byte.MaxValue);
                WindowsAPI.GetClassName(parent, lpClassName2, checked(lpClassName2.Capacity + 1));
                if (lpClassName2.ToString().ToLower().StartsWith("afx") && WindowsAPI.GetParent(parent) != doc.Window.Handle)
                {
                    WindowsAPI.PostMessage(focus, WM_KEYDOWN, new IntPtr(wParam), new IntPtr(3735553));
                    return true;
                }
            }

            if (left == "cicerouiwndframe")
                return true;
        }
        return false;
    }

    internal static void UnIMEHook()
    {
        if (!(hnexthookproc != IntPtr.Zero))
            return;

        WindowsAPI.UnhookWindowsHookEx(hnexthookproc);
        hnexthookproc = IntPtr.Zero;
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
            HookProc = MyKeyboardProc;
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD, HookProc, 0, WindowsAPI.GetCurrentThreadId());
            return;
        }

        var moduleHandle = (long)WindowsAPI.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
        if (Marshal.SizeOf(typeof(IntPtr)) == 4)
        {
            HookProcX86 = (nCode, wParam, lParam) => {
                return (IntPtr)MyKeyboardProcX86(nCode, wParam, lParam);
            };
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX86, moduleHandle, 0);
        }
        else
        {
            HookProcX64 = (nCode, wParam, lParam) => {
                return (IntPtr)MyKeyboardProcX64(nCode, wParam, lParam);
            };
            hnexthookproc = WindowsAPI.SetWindowsHookEx(WindowsAPI.HookType.WH_KEYBOARD_LL, HookProcX64, moduleHandle, 0);
        }
    }

    public static IntPtr MyKeyboardProc(int nCode, int wParam, int lParam)
    {
        if (nCode >= 0)
        {
            bool flag = !(lParam > 0 & (lParam & -1073741823) == 1) ||
                        (Control.ModifierKeys != Keys.None && Control.ModifierKeys != Keys.Shift) ||
                        !IMEHook(nCode, wParam, lParam);
            if (!flag)
                return (IntPtr)1;
        }
        return (IntPtr)WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
    }

    static int MyKeyboardProcX64(int nCode, int wParam, long lParam)
    {
        if (!Settings.Use ||
            WindowsAPI.IsIconic(Acap.MainWindow.Handle.ToInt32()) ||
            WindowsAPI.GetKeyState(91) < 0 ||
            WindowsAPI.GetKeyState(92) < 0 ||
            wParam != WM_KEYDOWN ||
            nCode < 0)
            return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

        IFoxCAD.Cad.WindowsAPI.GetWindowThreadProcessId(WindowsAPI.GetForegroundWindow(), out uint lpdwProcessId);
        if (lpdwProcessId != AcadPID)
            return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

        var keyb = KeyboardHookStruct.Create(new IntPtr(lParam));
        switch (Control.ModifierKeys)
        {
            case Keys.None:
            {
                if (WindowsAPI.GetKeyState(162) < 0 ||
                    WindowsAPI.GetKeyState(163) < 0 ||
                    WindowsAPI.GetKeyState(17) < 0 ||
                    WindowsAPI.GetKeyState(262144) < 0)
                    return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

                if (IMEHook(nCode, keyb.vkCode, 0))
                    return 1;
                break;
            }
            case Keys.Shift:
            {
                if (IMEHook(nCode, keyb.vkCode, 0))
                    return 1;
                break;
            }
        }
        return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
    }

    static int MyKeyboardProcX86(int nCode, int wParam, int lParam)
    {
        if (!Settings.Use ||
            WindowsAPI.IsIconic(Acap.MainWindow.Handle.ToInt32()) ||
            WindowsAPI.GetKeyState(91) < 0 ||
            WindowsAPI.GetKeyState(92) < 0 ||
            wParam != WM_KEYDOWN ||
            nCode < 0)
            return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

        IFoxCAD.Cad.WindowsAPI.GetWindowThreadProcessId(WindowsAPI.GetFocus(), out uint lpdwProcessId);
        if (lpdwProcessId != AcadPID)
            return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

        var keyb = KeyboardHookStruct.Create(new IntPtr(lParam));
        switch (Control.ModifierKeys)
        {
            case Keys.None:
            {
                if (WindowsAPI.GetKeyState(162) < 0 ||
                    WindowsAPI.GetKeyState(163) < 0 ||
                    WindowsAPI.GetKeyState(17) < 0 ||
                    WindowsAPI.GetKeyState(262144) < 0)
                    return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);

                if (IMEHook(nCode, keyb.vkCode, 0))
                    return 1;
                break;
            }
            case Keys.Shift:
            {
                if (IMEHook(nCode, keyb.vkCode, 0))
                    return 1;
                break;
            }
        }
        return WindowsAPI.CallNextHookEx(hnexthookproc, nCode, wParam, lParam);
    }

    static void CheckLowLevelHooksTimeout()
    {
        const string llh = "LowLevelHooksTimeout";

        RegistryKey hkml = Registry.CurrentUser;
        RegistryKey registryKey = hkml.OpenSubKey("Control Panel\\Desktop", true);
        if ((int)registryKey.GetValue(llh, 0) == 0)
            registryKey.SetValue(llh, 25000, RegistryValueKind.DWord);
        registryKey.Close();
    }

    public struct KeyboardHookStruct
    {
        public int vkCode;
        public int ScanCode;
        public int Flags;
        public int Time;
        public int DwExtraInfo;

        public static KeyboardHookStruct Create(IntPtr lParam)
        {
            return (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
        }
    }
}