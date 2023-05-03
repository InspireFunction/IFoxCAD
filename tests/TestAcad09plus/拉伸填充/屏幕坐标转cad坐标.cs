//#define cpp
namespace JoinBoxAcad;

using System.Drawing;
using static IFoxCAD.Basal.WindowsAPI;

public partial class Screen
{
    [CommandMethod(nameof(GetScreenToCadxx))]
    public static void GetScreenToCadxx()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        var ucsPoint = GetScreenToCad();
        ed.WriteMessage(ucsPoint.ToString() + "\n");
    }

    /// <summary>
    /// 屏幕坐标转cad坐标
    /// </summary>
    public static Point3d GetScreenToCad()
    {
        // 两种获取方式都可以
        var cursorPos = System.Windows.Forms.Control.MousePosition;
        // GetCursorPos(out Point cursorPos);
        return ScreenToCad(cursorPos);
    }

    /// <summary>
    /// 屏幕像素点转cad图纸坐标点
    /// </summary>
    /// <param name="cursorPos">屏幕像素点</param>
    /// <returns>返回ucs的点</returns>
    public static Point3d ScreenToCad(Point cursorPos)
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        var mid = WindowsAPI.GetParent(doc.Window.Handle);

        ScreenToClient(mid, ref cursorPos);
        var vn = ed.GetViewportNumber(cursorPos);// System.Windows.Forms.Control.MousePosition
        var wcsPoint = ed.PointToWorld(cursorPos, vn);
        var ucsPoint = wcsPoint.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
        return ucsPoint;
    }

    /// <summary>
    /// 屏幕坐标到客户区坐标转换
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="lpPoint">点结构,返回屏幕坐标</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);
    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    public static Point CadToScreen(Point3d pt3d, Point mousePosition)
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        var mid = WindowsAPI.GetParent(doc.Window.Handle);

        var vn = ed.GetViewportNumber(mousePosition);//System.Windows.Forms.Control.MousePosition
        var ptScr = Env.Editor.PointToScreen(pt3d, vn);// 高版本这个不一样,转为客户区
        var ptScrWin = new Point((int)ptScr.X, (int)ptScr.Y);
        ClientToScreen(mid, ref ptScrWin);
        return ptScrWin;
    }

    //#if NET35
    //    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPAVCView@@XZ")] //acad08
    //#else
    //    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPEAVCView@@XZ")]//acad21
    //#endif
    //    static extern IntPtr AcedGetAcadDwgview();

    delegate IntPtr DelegateAcedGetAcadDwgview();
    static DelegateAcedGetAcadDwgview? acedGetAcadDwgView;
    /// <summary>
    /// 获取视口指针
    /// </summary>
    public static IntPtr AcedGetAcadDwgview()
    {
        if (acedGetAcadDwgView is null)
        {
            acedGetAcadDwgView = AcadPeInfo
                .GetDelegate<DelegateAcedGetAcadDwgview>(
                nameof(acedGetAcadDwgView), AcadPeEnum.AcadExe);
        }
        if (acedGetAcadDwgView is not null)
            return acedGetAcadDwgView.Invoke();// 调用方法
        return IntPtr.Zero;
    }

    delegate int DelegateAcedGetWinNum(int x, int y);
    static DelegateAcedGetWinNum? acedGetWinNum;
    /// <summary>
    /// 获取窗口数字
    /// </summary>
    public static int AcedGetWinNum(int x, int y)
    {
        if (acedGetWinNum is null)
            acedGetWinNum = AcadPeInfo
                .GetDelegate<DelegateAcedGetWinNum>(
                nameof(acedGetWinNum), AcadPeEnum.ExeAndCore);
        if (acedGetWinNum is not null)
            return acedGetWinNum.Invoke(x, y);// 调用方法
        return 0;
    }

    /// <summary>
    /// 将坐标从绘图窗口转换为活动视口坐标系
    /// </summary>
    /// <param name="windnum"></param>
    /// <param name="pt"></param>
    /// <param name="ptOut"></param>
    /// <returns></returns>
#if NET35
    // 此处都是acad08这个有重载,不知道PeInfo能不能正常运行
    [DllImport("acad.exe", EntryPoint = "?acedCoordFromPixelToWorld@@YAHHVCPoint@@QAN@Z")]
    static extern int AcedCoordFromPixelToWorld(int windnum, Point pt, out Point3D ptOut);

    [DllImport("acad.exe", EntryPoint = "?acedCoordFromPixelToWorld@@YAXABVCPoint@@QAN@Z")]//这个重载参数不知道
    static extern int AcedCoordFromPixelToWorld(Point pt, out Point3D ptOut);
#else
    [DllImport("accore.dll", EntryPoint = "?acedCoordFromPixelToWorld@@YAHHVCPoint@@QEAN@Z")]
    static extern int AcedCoordFromPixelToWorld(int windnum, Point pt, out Point3D ptOut);
#endif
}