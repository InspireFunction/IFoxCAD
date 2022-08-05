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



    #region 首尾相连
    /// <summary>
    /// 首尾相连
    /// </summary>
    public static Point2dCollection End2End(this Point2dCollection ptcol!!)
    {
        if (ptcol.Count == 0 || ptcol[0].Equals(ptcol[^1]))//首尾相同直接返回
            return ptcol;

        //首尾不同,去加一个到最后
        var lst = new Point2d[ptcol.Count + 1];
        for (int i = 0; i < lst.Length; i++)
            lst[i] = ptcol[i];
        lst[^1] = lst[0];

        return new(lst);
    }
    /// <summary>
    /// 首尾相连
    /// </summary>
    public static Point3dCollection End2End(this Point3dCollection ptcol!!)
    {
        if (ptcol.Count == 0 || ptcol[0].Equals(ptcol[^1]))//首尾相同直接返回
            return ptcol;

        //首尾不同,去加一个到最后
        var lst = new Point3d[ptcol.Count + 1];
        for (int i = 0; i < lst.Length; i++)
            lst[i] = ptcol[i];
        lst[^1] = lst[0];

        return new(lst);
    }
    #endregion
}