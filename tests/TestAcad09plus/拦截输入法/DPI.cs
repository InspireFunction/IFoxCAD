using System.Drawing;
using System.Runtime.CompilerServices;

namespace Gstar_IMEFilter;

public class DPI
{
    [MethodImpl]
    public static double CurrentDPI() => (double)Graphics.FromHwnd(IntPtr.Zero).DpiX / 96.0;
}