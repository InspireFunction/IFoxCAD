

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

    // 为了频繁触发所以弄个缓存
    static Plane? _PlaneCache;
    /// <summary>
    /// 两点计算弧度范围0到2Pi
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <param name="direction">方向</param>
    /// <returns>弧度值</returns>
    public static double GetAngle(this Point3d startPoint, Point3d endPoint, Vector3d? direction = null)
    {
        if (direction != null)
            _PlaneCache = new Plane(Point3d.Origin, direction.Value);
        if (_PlaneCache == null)
            _PlaneCache = new Plane(Point3d.Origin, Vector3d.ZAxis);
        return startPoint.GetVectorTo(endPoint).AngleOnPlane(_PlaneCache);
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

    /// <summary>
    /// 获取中点
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Point2d GetMidPointTo(this Point2d a, Point2d b)
    {
        // (p1 + p2) / 2; // 溢出风险
        return new Point2d(a.X * 0.5 + b.X * 0.5,
                           a.Y * 0.5 + b.Y * 0.5);
    }
    /// <summary>
    /// Z值归零
    /// </summary>
    /// <param name="point">点</param>
    /// <returns>新点</returns>
    internal static Point3d Z20(this Point3d point)
    {
        return new Point3d(point.X, point.Y, 0);
    }

    /// http://www.lee-mac.com/bulgeconversion.html
    /// <summary>
    /// 求凸度,判断三点是否一条直线上
    /// </summary>
    /// <param name="arc1">圆弧起点</param>
    /// <param name="arc2">圆弧腰点</param>
    /// <param name="arc3">圆弧尾点</param>
    /// <returns>逆时针为正,顺时针为负</returns>
    public static double GetArcBulge(this Point2d arc1, Point2d arc2, Point2d arc3, double tol = 1e-10)
    {
        double dStartAngle = arc2.GetAngle(arc1);
        double dEndAngle = arc2.GetAngle(arc3);
        // 求的P1P2与P1P3夹角
        var talAngle = (Math.PI - dStartAngle + dEndAngle) / 2;
        // 凸度==拱高/半弦长==拱高比值/半弦长比值
        // 有了比值就不需要拿到拱高值和半弦长值了,因为接下来是相除得凸度
        double bulge = Math.Sin(talAngle) / Math.Cos(talAngle);

        // 处理精度
        if (bulge > 0.9999 && bulge < 1.0001)
            bulge = 1;
        else if (bulge < -0.9999 && bulge > -1.0001)
            bulge = -1;
        else if (Math.Abs(bulge) < tol)
            bulge = 0;
        return bulge;
    }


    #region 首尾相连
    /// <summary>
    /// 首尾相连
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void End2End(this Point2dCollection ptcol)
    {
        ptcol.NotNull(nameof(ptcol));

        if (ptcol.Count == 0 || ptcol[0].Equals(ptcol[^1]))// 首尾相同直接返回
            return;

        // 首尾不同,去加一个到最后
        var lst = new Point2d[ptcol.Count + 1];
        for (int i = 0; i < lst.Length; i++)
            lst[i] = ptcol[i];
        lst[^1] = lst[0];

        ptcol.Clear();
        ptcol.AddRange(lst);
    }
    /// <summary>
    /// 首尾相连
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void End2End(this Point3dCollection ptcol)
    {
        ptcol.NotNull(nameof(ptcol));
        if (ptcol.Count == 0 || ptcol[0].Equals(ptcol[^1]))// 首尾相同直接返回
            return;

        // 首尾不同,去加一个到最后
        var lst = new Point3d[ptcol.Count + 1];
        for (int i = 0; i < lst.Length; i++)
            lst[i] = ptcol[i];
        lst[^1] = lst[0];

        ptcol.Clear();
        for (int i = 0; i < lst.Length; i++)
            ptcol.Add(lst[i]);
    }
    #endregion
}