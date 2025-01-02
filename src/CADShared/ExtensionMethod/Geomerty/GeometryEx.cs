#if a2024
using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif

namespace IFoxCAD.Cad;

using System.Drawing;

/// <summary>
/// 图形扩展类
/// </summary>
public static class GeometryEx
{
    #region Point&Circle

    /// <summary>
    /// 判断点与多边形的关系
    /// </summary>
    /// <param name="pts">多边形顶点集合</param>
    /// <param name="pt">点</param>
    /// <returns>点与多边形的关系</returns>
    public static PointOnRegionType PointOnRegion(this IEnumerable<Point2d> pts, Point2d pt)
    {
        var ptList = pts.ToList();
        if (ptList.Count < 3)
            return PointOnRegionType.Error;
        if (ptList[0] == ptList[1])
        {
            ptList.RemoveAt(ptList.Count - 1);
        }

        if (ptList.Count < 3)
            return PointOnRegionType.Error;

        List<Curve2d> ls2ds = [];
        for (var i = 0; i < ptList.Count - 1; i++)
        {
            ls2ds.Add(new LineSegment2d(ptList[i], ptList[i + 1]));
        }

        ls2ds.Add(new LineSegment2d(ptList[^1], ptList[0]));
        using var cc2d = new CompositeCurve2d(ls2ds.ToArray());

        // 释放资源
        foreach (var curve2d in ls2ds)
        {
            using (curve2d)
            {
            }
        }

        // 在多边形上?
        if (cc2d.IsOn(pt))
            return PointOnRegionType.On;

        // 在最小包围矩形外?
        var bb2d = cc2d.BoundBlock;
        if (!bb2d.Contains(pt))
            return PointOnRegionType.Outside;

        #region 旧版疑似有问题的代码

        // //
        // var flag = false;
        // foreach (var node in ptlst.GetNodes())
        // {
        //     var pt1 = node.Value;
        //     var pt2 = node.Next!.Value;
        //     if (pt.Y < pt1.Y && pt.Y < pt2.Y)
        //         continue;
        //     if (pt1.X < pt.X && pt2.X < pt.X)
        //         continue;
        //     var vec = pt2 - pt1;
        //     var t = (pt.X - pt1.X) / vec.X;
        //     var y = t * vec.Y + pt1.Y;
        //     if (y < pt.Y && t >= 0 && t <= 1)
        //         flag = !flag;
        // }
        // return
        //     flag ?
        //     PointOnRegionType.Inside : PointOnRegionType.Outside;

        #endregion

        var flag = false;
        var j = ptList.Count - 1;
        for (var i = 0; i < ptList.Count; i++)
        {
            var pi = ptList[i];
            var pj = ptList[j];
            if ((pi.Y < pt.Y && pj.Y >= pt.Y || pj.Y < pt.Y && pi.Y >= pt.Y) &&
                (pi.X <= pt.X || pj.X <= pt.X))
            {
                flag ^= pi.X + (pt.Y - pi.Y) / (pj.Y - pi.Y) * (pj.X - pi.X) < pt.X;
            }

            j = i;
        }

        return flag ? PointOnRegionType.Inside : PointOnRegionType.Outside;
    }

    /// <summary>
    /// 判断点与多边形的关系
    /// </summary>
    /// <param name="pts">多边形顶点集合</param>
    /// <param name="pt">点</param>
    /// <returns>点与多边形的关系</returns>
    public static PointOnRegionType PointOnRegion(this IEnumerable<Point3d> pts, Point3d pt)
    {
        var ptList = pts.ToList();
        if (ptList.Count < 3)
            return PointOnRegionType.Error;
        if (ptList[0] == ptList[1])
        {
            ptList.RemoveAt(ptList.Count - 1);
        }

        if (ptList.Count < 3)
            return PointOnRegionType.Error;

        List<Curve3d> ls3ds = [];
        for (var i = 0; i < ptList.Count - 1; i++)
        {
            ls3ds.Add(new LineSegment3d(ptList[i], ptList[i + 1]));
        }

        ls3ds.Add(new LineSegment3d(ptList[^1], ptList[0]));
        using var cc3d = new CompositeCurve3d(ls3ds.ToArray());

        // 释放资源
        foreach (var curve3d in ls3ds)
        {
            using (curve3d)
            {
            }
        }

        // 在多边形上?
        if (cc3d.IsOn(pt))
            return PointOnRegionType.On;

        // 在最小包围矩形外?
        var bb3d = cc3d.BoundBlock;
        if (!bb3d.Contains(pt))
            return PointOnRegionType.Outside;


        var flag = false;
        var j = ptList.Count - 1;
        for (var i = 0; i < ptList.Count; i++)
        {
            var pi = ptList[i];
            var pj = ptList[j];
            if ((pi.Y < pt.Y && pj.Y >= pt.Y || pj.Y < pt.Y && pi.Y >= pt.Y) &&
                (pi.X <= pt.X || pj.X <= pt.X))
            {
                flag ^= pi.X + (pt.Y - pi.Y) / (pj.Y - pi.Y) * (pj.X - pi.X) < pt.X;
            }

            j = i;
        }

        return flag ? PointOnRegionType.Inside : PointOnRegionType.Outside;
    }

    /// <summary>
    /// 按两点返回最小包围圆
    /// </summary>
    /// <param name="pt1">基准点</param>
    /// <param name="pt2">基准点</param>
    /// <param name="ptlst">输出圆上的点</param>
    /// <returns>解析类圆对象</returns>
    public static CircularArc2d GetMinCircle(Point2d pt1, Point2d pt2, out LoopList<Point2d> ptlst)
    {
        ptlst = [pt1, pt2];
        return new CircularArc2d((pt1 + pt2.GetAsVector()) / 2, pt1.GetDistanceTo(pt2) / 2);
    }

    /// <summary>
    /// 按三点返回最小包围圆
    /// </summary>
    /// <param name="pt1">基准点</param>
    /// <param name="pt2">基准点</param>
    /// <param name="pt3">基准点</param>
    /// <param name="ptlst">输出圆上的点</param>
    /// <returns>解析类圆对象</returns>
    public static CircularArc2d GetMinCircle(Point2d pt1, Point2d pt2, Point2d pt3,
        out LoopList<Point2d> ptlst)
    {
        ptlst = [pt1, pt2, pt3];

        // 遍历各点与下一点的向量长度,找到距离最大的两个点
        var maxNode = ptlst.GetNodes()
            .FindByMax(out _, node => node.Value.GetDistanceTo(node.Next!.Value));

        // 以两点做最小包围圆
        var ca2d = GetMinCircle(maxNode.Value, maxNode.Next!.Value, out var tptlst);

        // 如果另一点属于该圆
        if (ca2d.IsIn(maxNode.Previous!.Value))
        {
            // 返回
            ptlst = tptlst;
            return ca2d;
        }

        // 否则按三点做圆
        // ptlst.SetFirst(maxNode);
        ptlst = [maxNode.Value, maxNode.Next.Value, maxNode.Previous.Value];
        ca2d = new CircularArc2d(pt1, pt2, pt3);
        ca2d.SetAngles(0, Math.PI * 2);
        return ca2d;
    }

    /// <summary>
    /// 按四点返回最小包围圆
    /// </summary>
    /// <param name="pt1">基准点</param>
    /// <param name="pt2">基准点</param>
    /// <param name="pt3">基准点</param>
    /// <param name="pt4">基准点</param>
    /// <param name="ptlst">输出圆上的点</param>
    /// <returns>解析类圆对象</returns>
    public static CircularArc2d? GetMinCircle(Point2d pt1, Point2d pt2, Point2d pt3, Point2d pt4,
        out LoopList<Point2d>? ptlst)
    {
        var iniptlst = new LoopList<Point2d>() { pt1, pt2, pt3, pt4 };
        ptlst = null;
        CircularArc2d? ca2d = null;

        // 遍历C43的组合,环链表的优势在这里
        foreach (var firstNode in iniptlst.GetNodes())
        {
            // 获取各组合下三点的最小包围圆
            var secondNode = firstNode.Next;
            var thirdNode = secondNode!.Next;
            var tca2d = GetMinCircle(firstNode.Value, secondNode.Value, thirdNode!.Value,
                out var tptlst);

            // 如果另一点属于该圆,并且半径小于当前值就把它做为候选解
            if (!tca2d.IsIn(firstNode.Previous!.Value))
                continue;
            if (ca2d is null || tca2d.Radius < ca2d.Radius)
            {
                ca2d = tca2d;
                ptlst = tptlst;
            }
        }

        // 返回直径最小的圆
        return ca2d;
    }

    /// <summary>
    /// 计算三点围成的有向面积
    /// </summary>
    /// <param name="ptBase">基准点</param>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <returns>三点围成的三角形的有向面积</returns>
    private static double CalArea(Point2d ptBase, Point2d pt1, Point2d pt2)
    {
        return (pt2 - ptBase).DotProduct((pt1 - ptBase).GetPerpendicularVector()) * 0.5;
    }

    /// <summary>
    /// 计算三点围成的三角形的真实面积
    /// </summary>
    /// <param name="ptBase">基准点</param>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <returns>三点围成的三角形的真实面积</returns>
    public static double GetArea(this Point2d ptBase, Point2d pt1, Point2d pt2)
    {
        return Math.Abs(CalArea(ptBase, pt1, pt2));
    }

    /// <summary>
    /// 判断三点是否为逆时针，也就是说判断三点是否为左转
    /// </summary>
    /// <param name="ptBase">基点</param>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <returns>OrientationType 类型值</returns>
    public static OrientationType IsClockWise(this Point2d ptBase, Point2d pt1, Point2d pt2)
    {
        return CalArea(ptBase, pt1, pt2) switch
        {
            > 0 => OrientationType.CounterClockWise,
            < 0 => OrientationType.ClockWise,
            _ => OrientationType.Parallel
        };
    }

    /// <summary>
    /// 计算两个二维向量围成的平行四边形的有向面积
    /// </summary>
    /// <param name="vecBase">基向量</param>
    /// <param name="vec">向量</param>
    /// <returns>有向面积</returns>
    private static double CalArea(Vector2d vecBase, Vector2d vec)
    {
        return vec.DotProduct(vecBase.GetPerpendicularVector()) / 2;
    }

    /// <summary>
    /// 计算两个二维向量围成的平行四边形的真实面积
    /// </summary>
    /// <param name="vecBase">基向量</param>
    /// <param name="vec">向量</param>
    /// <returns>真实面积</returns>
    public static double GetArea(Vector2d vecBase, Vector2d vec)
    {
        return Math.Abs(CalArea(vecBase, vec));
    }

    /// <summary>
    /// 判断两个二维向量是否左转
    /// </summary>
    /// <param name="vecBase">基向量</param>
    /// <param name="vec">向量</param>
    /// <returns>OrientationType 类型值</returns>
    public static OrientationType IsClockWise(Vector2d vecBase, Vector2d vec)
    {
        return CalArea(vecBase, vec) switch
        {
            > 0 => OrientationType.CounterClockWise,
            < 0 => OrientationType.ClockWise,
            _ => OrientationType.Parallel
        };
    }

    #region PointList

    /// <summary>
    /// 计算点集的有向面积
    /// </summary>
    /// <param name="pnts">点集</param>
    /// <returns>有向面积</returns>
    private static double CalArea(IEnumerable<Point2d> pnts)
    {
        using var itor = pnts.GetEnumerator();
        if (!itor.MoveNext())
            throw new System.ArgumentNullException(nameof(pnts));
        var start = itor.Current;
        var p2 = start;
        double area = 0;

        while (itor.MoveNext())
        {
            var p1 = p2;
            p2 = itor.Current;
            area += (p1.X * p2.Y - p2.X * p1.Y);
        }

        area = (area + (p2.X * start.Y - start.X * p2.Y)) / 2.0;
        return area;
    }

    /// <summary>
    /// 计算点集的真实面积
    /// </summary>
    /// <param name="pnts">点集</param>
    /// <returns>面积</returns>
    public static double GetArea(this IEnumerable<Point2d> pnts)
    {
        return Math.Abs(CalArea(pnts));
    }

    /// <summary>
    /// 判断点集的点序
    /// </summary>
    /// <param name="pnts">点集</param>
    /// <returns>OrientationType 类型值</returns>
    public static OrientationType IsClockWise(this IEnumerable<Point2d> pnts)
    {
        return CalArea(pnts) switch
        {
            < 0 => OrientationType.ClockWise,
            > 0 => OrientationType.CounterClockWise,
            _ => OrientationType.Parallel
        };
    }

    /// <summary>
    /// 按点集返回最小包围圆
    /// </summary>
    /// <param name="pnts">点集</param>
    /// <param name="ptlst">输出圆上的点</param>
    /// <returns>解析类圆对象</returns>
    public static CircularArc2d? GetMinCircle(this List<Point2d> pnts, out LoopList<Point2d>? ptlst)
    {
        // 点数较小时直接返回
        switch (pnts.Count)
        {
            case 0:
                ptlst = null;
                return null;

            case 1:
                ptlst = [pnts[0]];
                return new CircularArc2d(pnts[0], 0);

            case 2:
                return GetMinCircle(pnts[0], pnts[1], out ptlst);

            case 3:
                return GetMinCircle(pnts[0], pnts[1], pnts[2], out ptlst);

            case 4:
                return GetMinCircle(pnts[0], pnts[1], pnts[2], pnts[3], out ptlst);
        }

        // 按前三点计算最小包围圆
        var tpnts = new Point2d[4];
        pnts.CopyTo(0, tpnts, 0, 3);
        var ca2d = GetMinCircle(tpnts[0], tpnts[1], tpnts[2], out ptlst);

        // 找到点集中距离圆心的最远点为第四点
        tpnts[3] = pnts.FindByMax(pnt => ca2d.Center.GetDistanceTo(pnt));

        // 如果最远点属于圆结束
        while (!ca2d.IsIn(tpnts[3]))
        {
            // 如果最远点不属于圆,按此四点计算最小包围圆
            ca2d = GetMinCircle(tpnts[0], tpnts[1], tpnts[2], tpnts[3], out ptlst);

            // 将结果作为新的前三点
            if (ptlst!.Count == 3)
            {
                tpnts[2] = ptlst.Last!.Value;
            }
            else
            {
                // 第三点取另两点中距离圆心较远的点
                // 按算法中描述的任选其中一点的话,还是无法收敛......
                tpnts[2] = tpnts.Except(ptlst).FindByMax(pnt => ca2d!.Center.GetDistanceTo(pnt));
            }

            tpnts[0] = ptlst.First!.Value;
            tpnts[1] = ptlst.First.Next!.Value;

            // 按此三点计算最小包围圆
            ca2d = GetMinCircle(tpnts[0], tpnts[1], tpnts[2], out ptlst);

            // 找到点集中圆心的最远点为第四点
            tpnts[3] = pnts.FindByMax(pnt => ca2d.Center.GetDistanceTo(pnt));
        }

        return ca2d;
    }

    /// <summary>
    /// 获取点集的凸包
    /// </summary>
    /// <param name="points">点集</param>
    /// <returns>凸包</returns>
    public static List<Point2d> ConvexHull(this List<Point2d> points)
    {
        if (points.Count <= 1)
            return points;

        int n = points.Count, k = 0;
        List<Point2d> H = [..new Point2d[2 * n]];

        points.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

        // Build lower hull
        for (var i = 0; i < n; ++i)
        {
            while (k >= 2 && IsClockWise(H[k - 2], H[k - 1], points[i]) ==
                   OrientationType.CounterClockWise)
                k--;
            H[k++] = points[i];
        }

        // Build upper hull
        for (int i = n - 2, t = k + 1; i >= 0; i--)
        {
            while (k >= t && IsClockWise(H[k - 2], H[k - 1], points[i]) ==
                   OrientationType.CounterClockWise)
                k--;
            H[k++] = points[i];
        }

        return H.Take(k - 1).ToList();
    }

    #endregion PointList

    #endregion Point&Circle

    #region Ucs

    /// <summary>
    /// ucs到wcs的点变换
    /// </summary>
    /// <param name="point">点</param>
    /// <returns>变换后的点</returns>
    public static Point3d Ucs2Wcs(this Point3d point)
    {
        return point.TransformBy(Env.Editor.CurrentUserCoordinateSystem);
    }

    /// <summary>
    /// wcs到ucs的点变换
    /// </summary>
    /// <param name="point">点</param>
    /// <returns>变换后的点</returns>
    public static Point3d Wcs2Ucs(this Point3d point)
    {
        return point.TransformBy(Env.Editor.CurrentUserCoordinateSystem.Inverse());
    }

    /// <summary>
    /// ucs到wcs的向量变换
    /// </summary>
    /// <param name="vec">向量</param>
    /// <returns>变换后的向量</returns>
    public static Vector3d Ucs2Wcs(this Vector3d vec)
    {
        return vec.TransformBy(Env.Editor.CurrentUserCoordinateSystem);
    }

    /// <summary>
    /// wcs到ucs的向量变换
    /// </summary>
    /// <param name="vec">向量</param>
    /// <returns>变换后的向量</returns>
    public static Vector3d Wcs2Ucs(this Vector3d vec)
    {
        return vec.TransformBy(Env.Editor.CurrentUserCoordinateSystem.Inverse());
    }

    /// <summary>
    /// 模拟 trans 函数
    /// </summary>
    /// <param name="point">点</param>
    /// <param name="from">源坐标系</param>
    /// <param name="to">目标坐标系</param>
    /// <returns>变换后的点</returns>
    public static Point3d Trans(this Point3d point, CoordinateSystemCode from,
        CoordinateSystemCode to)
    {
        return Env.Editor.GetMatrix(from, to) * point;
    }

    /// <summary>
    /// 模拟 trans 函数
    /// </summary>
    /// <param name="vec">向量</param>
    /// <param name="from">源坐标系</param>
    /// <param name="to">目标坐标系</param>
    /// <returns>变换后的向量</returns>
    public static Vector3d Trans(this Vector3d vec, CoordinateSystemCode from,
        CoordinateSystemCode to)
    {
        return vec.TransformBy(Env.Editor.GetMatrix(from, to));
    }

    /// <summary>
    /// wcs到dcs的点变换
    /// </summary>
    /// <param name="point">点</param>
    /// <param name="atPaperSpace">是否为图纸空间</param>
    /// <returns>变换后的点</returns>
    public static Point3d Wcs2Dcs(this Point3d point, bool atPaperSpace)
    {
        return Trans(point, CoordinateSystemCode.Wcs,
            atPaperSpace ? CoordinateSystemCode.PDcs : CoordinateSystemCode.MDcs);
    }

    /// <summary>
    /// wcs到dcs的向量变换
    /// </summary>
    /// <param name="vec">向量</param>
    /// <param name="atPaperSpace">是否为图纸空间</param>
    /// <returns>变换后的向量</returns>
    public static Vector3d Wcs2Dcs(this Vector3d vec, bool atPaperSpace)
    {
        return Trans(vec, CoordinateSystemCode.Wcs,
            atPaperSpace ? CoordinateSystemCode.PDcs : CoordinateSystemCode.MDcs);
    }

    #endregion Ucs

    /// <summary>
    /// 返回不等比例变换矩阵
    /// </summary>
    /// <param name="point">基点</param>
    /// <param name="x">x方向比例</param>
    /// <param name="y">y方向比例</param>
    /// <param name="z">z方向比例</param>
    /// <returns>三维矩阵</returns>
    public static Matrix3d GetScaleMatrix(this Point3d point, double x, double y, double z)
    {
        var matdata = new double[16];
        matdata[0] = x;
        matdata[3] = point.X * (1 - x);
        matdata[5] = y;
        matdata[7] = point.Y * (1 - y);
        matdata[10] = z;
        matdata[11] = point.Z * (1 - z);
        matdata[15] = 1;
        return new(matdata);
    }

    /// <summary>
    /// 获取坐标范围的大小
    /// </summary>
    /// <param name="ext">坐标范围</param>
    /// <returns>尺寸对象</returns>
    public static Size GetSize(this Extents3d ext)
    {
        var width = (int)Math.Floor(ext.MaxPoint.X - ext.MinPoint.X);
        var height = (int)Math.Ceiling(ext.MaxPoint.Y - ext.MinPoint.Y);
        return new(width, height);
    }

    /// <summary>
    /// 重绘
    /// </summary>
    /// <param name="geometry">图形界面几何</param>
    /// <param name="drawables">可绘制的对象列表</param>
    public static void Draw(this Geometry geometry, IEnumerable<Drawable> drawables)
    {
        drawables.ForEach(d => geometry.Draw(d));
    }

    /// <summary>
    /// 重绘
    /// </summary>
    /// <param name="geometry">图形界面几何</param>
    /// <param name="drawables">可绘制的对象列表</param>
    public static void Draw(this Geometry geometry, params Drawable[] drawables)
    {
        drawables.ForEach(d => geometry.Draw(d));
    }
}

/// <summary>
/// 向量扩展类
/// </summary>
public static class VectorEx
{
    /// <summary>
    /// 转换为2d向量
    /// </summary>
    /// <param name="vector3d">3d向量</param>
    /// <returns>2d向量</returns>
    public static Vector2d Convert2d(this Vector3d vector3d)
    {
        return new Vector2d(vector3d.X, vector3d.Y);
    }

    /// <summary>
    /// 转换为3d向量
    /// </summary>
    /// <param name="vector2d">2d向量</param>
    /// <param name="z">z值</param>
    /// <returns>3d向量</returns>
    public static Vector3d Convert3d(this Vector2d vector2d, double z = 0)
    {
        return new Vector3d(vector2d.X, vector2d.Y, z);
    }

    /// <summary>
    /// 2d叉乘
    /// </summary>
    /// <param name="a">向量a</param>
    /// <param name="b">向量b</param>
    /// <returns>叉乘值</returns>
    public static double Cross2d(this Vector3d a, Vector3d b)
    {
        return a.X * b.Y - b.X * a.Y;
    }

    /// <summary>
    /// 2d叉乘
    /// </summary>
    /// <param name="a">向量a</param>
    /// <param name="b">向量b</param>
    /// <returns>叉乘值</returns>
    public static double Cross2d(this Vector2d a, Vector2d b)
    {
        return a.X * b.Y - b.X * a.Y;
    }

    /// <summary>
    /// 向量Z值归零
    /// </summary>
    /// <param name="vector3d">向量</param>
    /// <returns></returns>
    public static Vector3d Z20(this Vector3d vector3d)
    {
        return new Vector3d(vector3d.X, vector3d.Y, 0);
    }

    /// <summary>
    /// 向量在平面上的弧度
    /// </summary>
    /// <param name="vector">向量</param>
    /// <param name="plane">平面</param>
    /// <returns>弧度</returns>
    public static double AngleOnPlane(this Vector3d vector, Plane? plane = null)
    {
        return vector.AngleOnPlane(plane ?? PlaneEx.Z);
    }
}

/// <summary>
/// 平面
/// </summary>
public static class PlaneEx
{
    /// <summary>
    /// X
    /// </summary>
    public static readonly Plane X = new(Point3d.Origin, Vector3d.XAxis);

    /// <summary>
    /// Y
    /// </summary>
    public static readonly Plane Y = new(Point3d.Origin, Vector3d.YAxis);

    /// <summary>
    /// Z
    /// </summary>
    public static readonly Plane Z = new(Point3d.Origin, Vector3d.ZAxis);
}