namespace IFoxCAD.Cad;

public static class PointEx
{

    /// <summary>
    /// 获取点的hash字符串，同时可以作为pt的字符串表示
    /// </summary>
    /// <param name="pt">点</param>
    /// <param name="xyz">指示计算几维坐标的标志，1为计算x，2为计算x，y，其他为计算x，y，z</param>
    /// <param name="decimalRetain">保留的小数位数</param>
    /// <returns>hash字符串</returns>
    public static string GetHashString(this Point3d pt, int xyz = 3, int decimalRetain = 6)
    {
        var de = $"f{decimalRetain}";
        return xyz switch
        {
            1 => $"({pt.X.ToString(de)})",
            2 => $"({pt.X.ToString(de)},{pt.Y.ToString(de)})",
            _ => $"({pt.X.ToString(de)},{pt.Y.ToString(de)},{pt.Z.ToString(de)})"
        };
    }
    /// <summary>
    /// 两点计算弧度范围0到2Pi
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <param name="direction">方向</param>
    /// <returns>弧度值</returns>
    public static double GetAngle(this Point3d startPoint, Point3d endPoint, Vector3d? direction = null)
    {
        return startPoint.GetVectorTo(endPoint).AngleOnPlane(new Plane(Point3d.Origin, direction ?? Vector3d.ZAxis));
    }
    /// <summary>
    /// 两点计算弧度范围0到2Pi
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <returns>弧度值</returns>
    public static double GetAngle(this Point2d startPoint, Point2d endPoint)
    {
        return startPoint.GetVectorTo(endPoint).Angle;
    }


}