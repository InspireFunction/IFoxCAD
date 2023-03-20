namespace IFoxCAD.Cad;

/// <summary>
/// 圆弧扩展类
/// </summary>
public static class ArcEx
{
    #region 圆弧

    /// <summary>
    /// 根据圆心、起点、终点来创建圆弧(二维)
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="centerPoint">圆心</param>
    /// <param name="endPoint">终点</param>
    /// <returns>圆弧</returns>
    public static Arc CreateArcSCE(Point3d startPoint, Point3d centerPoint, Point3d endPoint)
    {
        Arc arc = new();
        arc.SetDatabaseDefaults();
        arc.Center = centerPoint;
        arc.Radius = centerPoint.DistanceTo(startPoint);
        Vector2d startVector = new(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
        Vector2d endVector = new(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
        // 计算起始和终止角度
        arc.StartAngle = startVector.Angle;
        arc.EndAngle = endVector.Angle;
        return arc;
    }
    /// <summary>
    /// 三点法创建圆弧(二维)
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="pointOnArc">圆弧上的点</param>
    /// <param name="endPoint">终点</param>
    /// <returns>圆弧</returns>
    public static Arc CreateArc(Point3d startPoint, Point3d pointOnArc, Point3d endPoint)
    {
        // 创建一个几何类的圆弧对象
        CircularArc3d geArc = new(startPoint, pointOnArc, endPoint);
        // 将几何类圆弧对象的圆心和半径赋值给圆弧
#if !gcad

        return (Arc)Curve.CreateFromGeCurve(geArc);
#else
        return (Arc)geArc.ToCurve();
#endif
    }

    /// <summary>
    /// 根据起点、圆心和圆弧角度创建圆弧(二维)
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="centerPoint">圆心</param>
    /// <param name="angle">圆弧角度</param>
    /// <returns>圆弧</returns>
    public static Arc CreateArc(Point3d startPoint, Point3d centerPoint, double angle)
    {
        Arc arc = new();
        arc.SetDatabaseDefaults();
        arc.Center = centerPoint;
        arc.Radius = centerPoint.DistanceTo(startPoint);
        Vector2d startVector = new(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
        arc.StartAngle = startVector.Angle;
        arc.EndAngle = startVector.Angle + angle;
        return arc;
    }

    #endregion
}