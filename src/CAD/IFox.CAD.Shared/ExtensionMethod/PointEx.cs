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
            _PlaneCache = new Plane(new Point3d(), direction.Value);
        if (_PlaneCache == null)
            _PlaneCache = new Plane(new Point3d(), Vector3d.ZAxis);
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
    /// 获取两个点之间的中点
    /// </summary>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <returns>返回两个点之间的中点</returns>
    public static Point3d GetMidPointTo(this Point3d pt1, Point3d pt2)
    {
        return new(pt1.X * 0.5 + pt2.X * 0.5,
            pt1.Y * 0.5 + pt2.Y * 0.5,
            pt1.Z * 0.5 + pt2.Z * 0.5);
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

    /// <summary>
    /// 将三维点转换为二维点
    /// </summary>
    /// <param name="pt">三维点</param>
    /// <returns>二维点</returns>
    public static Point2d Point2d(this Point3d pt)
    {
        return new(pt.X, pt.Y);
    }
    /// <summary>
    /// 将三维点集转换为二维点集
    /// </summary>
    /// <param name="pts">三维点集</param>
    /// <returns>二维点集</returns>
    public static IEnumerable<Point2d> Point2d(this IEnumerable<Point3d> pts)
    {
        return pts.Select(pt => pt.Point2d());
    }
    /// <summary>
    /// 将二维点转换为三维点
    /// </summary>
    /// <param name="pt">二维点</param>
    /// <param name="z">Z值</param>
    /// <returns>三维点</returns>
    public static Point3d Point3d(this Point2d pt, double z = 0)
    {
        return new(pt.X, pt.Y, z);
    }



    /// <summary>
    /// 根据世界坐标计算用户坐标
    /// </summary>
    /// <param name="basePt">基点世界坐标</param>
    /// <param name="userPt">基点用户坐标</param>
    /// <param name="transPt">目标世界坐标</param>
    /// <param name="ang">坐标网旋转角，按x轴正向逆时针弧度</param>
    /// <returns>目标用户坐标</returns>
    public static Point3d TransPoint(this Point3d basePt, Point3d userPt, Point3d transPt, double ang)
    {
        Matrix3d transMat = Matrix3d.Displacement(userPt - basePt);
        Matrix3d roMat = Matrix3d.Rotation(-ang, Vector3d.ZAxis, userPt);
        return transPt.TransformBy(roMat * transMat);
    }
    /// <summary>
    /// 计算指定距离和角度的点
    /// </summary>
    /// <remarks>本函数仅适用于x-y平面</remarks>
    /// <param name="pt">基点</param>
    /// <param name="ang">角度，x轴正向逆时针弧度</param>
    /// <param name="len">距离</param>
    /// <returns>目标点</returns>
    public static Point3d Polar(this Point3d pt, double ang, double len)
    {
        return pt + Vector3d.XAxis.RotateBy(ang, Vector3d.ZAxis) * len;
    }
    /// <summary>
    /// 计算指定距离和角度的点
    /// </summary>
    /// <remarks>本函数仅适用于x-y平面</remarks>
    /// <param name="pt">基点</param>
    /// <param name="ang">角度，x轴正向逆时针弧度</param>
    /// <param name="len">距离</param>
    /// <returns>目标点</returns>
    public static Point2d Polar(this Point2d pt, double ang, double len)
    {
        return pt + Vector2d.XAxis.RotateBy(ang) * len;
    }
    /// http://www.lee-mac.com/bulgeconversion.html
    /// <summary>
    /// 求凸度,判断三点是否一条直线上
    /// </summary>
    /// <param name="arc1">圆弧起点</param>
    /// <param name="arc2">圆弧腰点</param>
    /// <param name="arc3">圆弧尾点</param>
    /// <param name="tol">容差</param>
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
        if (bulge is > 0.9999 and < 1.0001)
            bulge = 1;
        else if (bulge is < -0.9999 and > -1.0001)
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