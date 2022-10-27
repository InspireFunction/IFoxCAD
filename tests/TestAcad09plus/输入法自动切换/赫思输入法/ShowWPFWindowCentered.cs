using System.Windows;
using System.Windows.Interop;

namespace Gstar_IMEFilter;

public class ShowWPFWindowCentered
{
    internal static bool Show(Window window, bool modal)
    {
        new WindowInteropHelper(window).Owner = Acap.MainWindow.Handle;
        WindowsAPI.RECT lpRect = new();
        WindowsAPI.GetWindowRect(Acap.MainWindow.Handle, ref lpRect);

        var dpi = DPI.CurrentDPI();
        window.Left = (lpRect.Width / dpi - window.Width) / 2.0
                      + lpRect.Left / dpi;

        window.Top = (lpRect.Height / dpi - window.Height) / 2.0
                      + lpRect.Top / dpi;

        if (modal)
        {
            var flag = window.ShowDialog();
            if (flag is null)
                return false;
            return flag.Value;
        }
        window.Show();
        return false;
    }
}