#if NET35
namespace Autodesk.AutoCAD.Internal
{
    public class Utils
    {
        public static void SetFocusToDwgView()
        {
            IntPtr window;
            if (Acap.DocumentManager.Count == 0)
            {
                window = Acap.MainWindow.Handle;
            }
            else
            {
                // 它们是层级关系
                // Main
                // -->MDI(大小被 DwgView 局限)
                // ---->docW(比MDI大)
                // -------->msctls_statusbar32
                // -------->DwgView
                var docW = Acap.DocumentManager.MdiActiveDocument.Window.Handle;
                var msctls_statusbar32 = IFoxCAD.Basal.WindowsAPI.GetTopWindow(docW);
                window = IFoxCAD.Basal.WindowsAPI.GetWindow(msctls_statusbar32, 2U);
            }
            if (window != IntPtr.Zero)
                IFoxCAD.Basal.WindowsAPI.SetFocus(window);
        }
    }
}
#endif