using System.Drawing;

namespace JoinBoxAcad;

public partial class Screen
{
    [CommandMethod(nameof(TestScreenToCad))]
    public void TestScreenToCad()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;
        var db = doc.Database;

        // 1. 获取当前鼠标位置的屏幕坐标
        var screenPos = System.Windows.Forms.Control.MousePosition;

        // 2. 转换为CAD坐标
        var cadPoint = ScreenToCad(screenPos);

        // 3. 绘制从原点到该点的直线
        using (var tr = db.TransactionManager.StartTransaction())
        {
            var line = new Line(new Point3d(0, 0, 0), cadPoint);
            line.Layer = "0";

            using var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
            btr.AppendEntity(line);
            tr.AddNewlyCreatedDBObject(line, true);
            tr.Commit();
        }

        ed.WriteMessage($"\n已绘制直线到: ({cadPoint.X:F2}, {cadPoint.Y:F2})");
    }

    /// <summary>
    /// 屏幕像素点转cad图纸坐标点
    /// </summary>
    /// <param name="cursorPos">屏幕像素点</param>
    /// <returns>返回ucs的点</returns>
    public static Point3d ScreenToCad(Point cursorPos)
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;

        // 直接使用AutoCAD主窗口句柄
        IntPtr hWnd = doc.Window.Handle;

        // 创建屏幕坐标副本
        Point clientPt = new Point(cursorPos.X, cursorPos.Y);

        // 将屏幕坐标转换为主窗口的客户区坐标
        ScreenToClient(hWnd, ref clientPt);

        // 获取视口编号
        int viewportNumber = ed.GetViewportNumber(clientPt);

        // 将客户区坐标转换为世界坐标
        Point3d wcsPoint = ed.PointToWorld(clientPt, viewportNumber);

        // 转换为当前UCS坐标
        // Point3d ucsPoint = wcsPoint.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());

        return wcsPoint;
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
}