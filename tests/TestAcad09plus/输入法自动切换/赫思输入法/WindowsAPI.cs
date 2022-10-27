namespace Gstar_IMEFilter;

public class WindowsAPI
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr GetFocus();

    [DllImport("user32.dll")]
    internal static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll ")]
    internal static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll ")]
    internal static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    internal static extern long GetHandleInformation(long hObject, ref long lpdwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    internal static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    internal static extern int GetCurrentThreadId();

    [DllImport("user32.dll")]
    internal static extern IntPtr SetWindowsHookEx(HookType idHook, CallBackX86 lpfn, long hmod, int dwThreadId);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowsHookEx(HookType idHook, CallBack lpfn, int hmod, int dwThreadId);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetWindowsHookEx(HookType idHook, CallBackX64 lpfn, long hmod, int dwThreadId);

    [DllImport("user32.dll")]
    internal static extern int UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("user32.dll")]
    internal static extern int CallNextHookEx(IntPtr hHook, int ncode, int wParam, long lParam);

    [DllImport("user32.dll")]
    internal static extern int CallNextHookEx(int hHook, int ncode, int wParam, int lParam);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetModuleHandle(string ModuleName);

    [DllImport("user32.dll")]
    internal static extern int ToAscii(int uVirtKey, int uScancode, byte[] lpdKeyState, byte[] lpwTransKey, int fuState);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern long GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern short GetKeyState(int nVirtKey);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool IsIconic(int hWnd);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);

    public delegate IntPtr CallBackX86(int nCode, int wParam, int lParam);
    public delegate IntPtr CallBackX64(int nCode, int wParam, long lParam);
    public delegate IntPtr CallBack(int nCode, int wParam, int lParam);
    public enum HookType
    {
        WH_KEYBOARD = 2,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14,
    }

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int Width => checked(Right - Left);
        public int Height => checked(Bottom - Top);
    }
}