namespace IFoxCAD.Cad;

/// <summary>
/// 圆扩展类
/// </summary>
public static class CircleEx
{
    /// <summary>
    /// 两点创建圆(两点中点为圆心)
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <returns>圆</returns>
    public static Circle CreateCircle(Point3d startPoint, Point3d endPoint)
    {
        Circle circle = new();
        circle.SetDatabaseDefaults();
        circle.Center = startPoint.GetMidPointTo(endPoint);
        circle.Radius = startPoint.DistanceTo(endPoint) * 0.5;
        return circle;
    }

    /// <summary>
    /// 三点法创建圆(失败则返回Null)
    /// </summary>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <param name="pt3">第三点</param>
    /// <returns>圆</returns>
    public static Circle? CreateCircle(Point3d pt1, Point3d pt2, Point3d pt3)
    {
        // 先判断三点是否共线,得到pt1点指向pt2、pt2点的矢量
        var va = pt2 - pt1;
        var vb = pt3 - pt2;
        // 如两矢量夹角为0或180度（π弧度),则三点共线.
        if (va.GetAngleTo(vb) == 0 | va.GetAngleTo(vb).Equals(Math.PI))
            return null;

        // 创建一个几何类的圆弧对象
        CircularArc3d geArc = new(pt1, pt2, pt3);
        return geArc.ToCircle();
    }

    /// <summary>
    /// 通过圆心,半径绘制圆形
    /// </summary>
    /// <param name="center">圆心</param>
    /// <param name="radius">半径</param>
    /// <param name="vex">法向量的X</param>
    /// <param name="vey">法向量的Y</param>
    /// <param name="vez">法向量的Z</param>
    /// <returns>圆</returns>
    public static Circle CreateCircle(Point3d center, double radius, double vex = 0, double vey = 0, double vez = 1)
    {
        return new Circle(center, new Vector3d(vex, vey, vez), radius); // 平面法向量XY方向
    }
}