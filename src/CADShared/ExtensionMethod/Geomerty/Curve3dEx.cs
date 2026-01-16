// ReSharper disable SuggestVarOrType_SimpleTypes

namespace IFoxCAD.Cad;

/// <summary>
/// 三维解析类曲线转换为三维实体曲线扩展类
/// </summary>
public static class Curve3dEx
{
    /// <summary>
    /// 判断两个浮点数是否相等
    /// </summary>
    /// <param name="tol">容差</param>
    /// <param name="d1">第一个数</param>
    /// <param name="d2">第二个数</param>
    /// <returns>两个数的差值的绝对值小于容差返回 <see langword="true"/>,反之返回 <see langword="false"/></returns>
    [MethodImpl]
    public static bool IsEqualPoint(this Tolerance tol, double d1, double d2)
    {
        return Math.Abs(d1 - d2) < tol.EqualPoint;
    }

    #region Curve3d

    /// <summary>
    /// 获取三维解析类曲线(自交曲线)的交点参数
    /// </summary>
    /// <param name="c3d">三维解析类曲线</param>
    /// <param name="sort">是否排序</param>
    /// <returns>曲线参数的列表</returns>
    public static List<double> GetParamsAtIntersectionPoints(this Curve3d c3d, bool sort = true)
    {
        CurveCurveIntersector3d cci = new(c3d, c3d, Vector3d.ZAxis);
        List<double> pars = [];
        for (var i = 0; i < cci.NumberOfIntersectionPoints; i++)
            pars.AddRange(cci.GetIntersectionParameters(i));
        if (sort)
            pars.Sort();
        return pars;
    }

    /// <summary>
    /// 获取三维解析类子曲线
    /// </summary>
    /// <param name="curve">三维解析类曲线</param>
    /// <param name="from">子段曲线起点参数</param>
    /// <param name="to">子段曲线终点参数</param>
    /// <returns>三维解析类曲线</returns>
    public static Curve3d GetSubCurve(this Curve3d curve, double from, double to)
    {
        Interval inter = curve.GetInterval();
        var atStart = Tolerance.Global.IsEqualPoint(inter.LowerBound, from);
        var atEnd = Tolerance.Global.IsEqualPoint(inter.UpperBound, to);
        if (atStart && atEnd)
            return (Curve3d)curve.Clone();
        if (curve is NurbCurve3d)
        {
            if (from < to)
            {
                NurbCurve3d clone = (NurbCurve3d)curve.Clone();
                if (atStart || atEnd)
                {
                    clone.HardTrimByParams(from, to);
                    return clone;
                }

                clone.HardTrimByParams(inter.LowerBound, to);
                clone.HardTrimByParams(from, to);
                return clone;
            }

            NurbCurve3d clone1 = (NurbCurve3d)curve.Clone();
            clone1.HardTrimByParams(from, inter.UpperBound);
            NurbCurve3d clone2 = (NurbCurve3d)curve.Clone();
            clone2.HardTrimByParams(inter.LowerBound, to);
            clone1.JoinWith(clone2);
            return clone1;
        }

        {
            Curve3d clone = (Curve3d)curve.Clone();
            clone.SetInterval(new Interval(from, to, Tolerance.Global.EqualPoint));
            return clone;
        }
    }

    /// <summary>
    /// 将三维解析类曲线转换为三维实体类曲线
    /// </summary>
    /// <param name="curve">三维解析类曲线</param>
    /// <returns>三维实体类曲线</returns>
    public static Curve? ToCurve(this Curve3d curve)
    {
        return curve switch
        {
            CompositeCurve3d co => ToCurve(co),
            LineSegment3d li => ToCurve(li),
            EllipticalArc3d el => ToCurve(el),
            CircularArc3d ci => ToCurve(ci),
            NurbCurve3d nu => ToCurve(nu),
            PolylineCurve3d pl => ToCurve(pl),
            Line3d l3 => ToCurve(l3),
            _ => null
        };
    }

    /// <summary>
    /// 将三维解析类曲线转换为三维解析类Nurb曲线
    /// </summary>
    /// <param name="curve">三维解析类曲线</param>
    /// <returns>三维解析类Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Curve3d curve)
    {
        return curve switch
        {
            LineSegment3d line => new NurbCurve3d(line),
            EllipticalArc3d el => new NurbCurve3d(el),
            CircularArc3d cir => new NurbCurve3d(ToEllipticalArc3d(cir)),
            NurbCurve3d nur => nur,
            PolylineCurve3d pl => new NurbCurve3d(3, pl, false),
            _ => null
        };
    }

    #endregion Curve3d

    #region CompositeCurve3d

    /// <summary>
    /// 判断是否为圆和椭圆
    /// </summary>
    /// <param name="curve">三维解析类曲线</param>
    /// <returns>完整圆及完整的椭圆返回 <see langword="true"/>,反之返回 <see langword="false"/></returns>
    public static bool IsCircular(this Curve3d curve)
    {
        return curve switch
        {
            CircularArc3d or EllipticalArc3d => curve.IsClosed(),
            _ => false
        };
    }

    /// <summary>
    /// 将三维复合曲线按曲线参数分割
    /// </summary>
    /// <param name="c3d">三维复合曲线</param>
    /// <param name="pars">曲线参数列表</param>
    /// <returns>三维复合曲线列表</returns>
    public static List<CompositeCurve3d>? GetSplitCurves(this CompositeCurve3d c3d,
        List<double> pars)
    {
        // 曲线参数剔除重复的
        if (pars.Count > 0)
        {
            pars.Sort();
            for (var i = pars.Count - 1; i > 0; i--)
                if (Tolerance.Global.IsEqualPoint(pars[i], pars[i - 1]))
                    pars.RemoveAt(i);
        }

        if (pars.Count == 0)
            return null;

        // 这个是曲线参数类
        var inter = c3d.GetInterval();
        // 曲线们
        var c3ds = c3d.GetCurves();
        if (c3ds.Length == 1 && c3ds[0].IsClosed())
        {
            // 闭合曲线不允许打断于一点
            if (pars.Count < 2)
                return null;

            // 如果包含起点
            if (Tolerance.Global.IsEqualPoint(pars[0], inter.LowerBound))
            {
                pars[0] = inter.LowerBound;
                // 又包含终点,去除终点
                if (Tolerance.Global.IsEqualPoint(pars[^1], inter.UpperBound))
                {
                    pars.RemoveAt(pars.Count - 1);
                    if (pars.Count == 1)
                        return null;
                }
            }
            else if (Tolerance.Global.IsEqualPoint(pars[^1], inter.UpperBound))
            {
                pars[^1] = inter.UpperBound;
            }

            // 加入第一点以支持反向打断
            pars.Add(pars[0]);
        }
        else
        {
            // 非闭合曲线加入起点和终点
            if (Tolerance.Global.IsEqualPoint(pars[0], inter.LowerBound))
                pars[0] = inter.LowerBound;
            else
                pars.Insert(0, inter.LowerBound);
            if (Tolerance.Global.IsEqualPoint(pars[^1], inter.UpperBound))
                pars[^1] = inter.UpperBound;
            else
                pars.Add(inter.UpperBound);
        }

        List<CompositeCurve3d> curves = [];
        List<Curve3d> cc3ds = [];
        for (var i = 0; i < pars.Count - 1; i++)
        {
            cc3ds.Clear();
            // 复合曲线参数转换到包含曲线参数
            var cp1 = c3d.GlobalToLocalParameter(pars[i]);
            var cp2 = c3d.GlobalToLocalParameter(pars[i + 1]);
            if (cp1.SegmentIndex == cp2.SegmentIndex)
            {
                cc3ds.Add(
                    c3ds[cp1.SegmentIndex].GetSubCurve(cp1.LocalParameter, cp2.LocalParameter));
            }
            else
            {
                inter = c3ds[cp1.SegmentIndex].GetInterval();
                cc3ds.Add(c3ds[cp1.SegmentIndex].GetSubCurve(cp1.LocalParameter, inter.UpperBound));

                for (var j = cp1.SegmentIndex + 1; j < cp2.SegmentIndex; j++)
                    cc3ds.Add((Curve3d)c3ds[j].Clone());

                inter = c3ds[cp2.SegmentIndex].GetInterval();
                cc3ds.Add(c3ds[cp2.SegmentIndex].GetSubCurve(inter.LowerBound, cp2.LocalParameter));
            }

            curves.Add(new(cc3ds.ToArray()));
        }

        // 封闭多段线 口口 并排形状,第二个口切割不成功,注释下面就成功了
        //if (c3d.IsClosed() && c3ds.Length > 1)
        //{
        //    var cus1 = curves[^1].GetCurves();
        //    var cus2 = curves[0].GetCurves();
        //    var cs = cus1.Combine2(cus2);
        //    curves[^1] = new(cs);
        //    curves.RemoveAt(0);
        //}
        return curves;
    }

    /// <summary>
    /// 将复合曲线转换为实体类曲线
    /// </summary>
    /// <param name="curve">三维复合曲线</param>
    /// <returns>实体曲线</returns>
    public static Curve? ToCurve(this CompositeCurve3d curve)
    {
        Curve3d[] cs = curve.GetCurves();
        if (cs.Length == 0)
            return null;
        if (cs.Length == 1)
            return ToCurve(cs[0]);

        var hasNurb = false;

        foreach (var c in cs)
        {
            if (c is not (NurbCurve3d or EllipticalArc3d))
                continue;
            hasNurb = true;
            break;
        }

        if (hasNurb)
        {
            var nc3d = cs[0].ToNurbCurve3d();
            for (var i = 1; i < cs.Length; i++)
                nc3d?.JoinWith(cs[i].ToNurbCurve3d());
            return nc3d?.ToCurve();
        }

        return ToPolyline(curve);
    }

    /// <summary>
    /// 将三维复合曲线转换为实体类多段线
    /// </summary>
    /// <param name="cc3d">三维复合曲线</param>
    /// <returns>实体类多段线</returns>
    public static Polyline ToPolyline(this CompositeCurve3d cc3d)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        pl.Elevation = cc3d.StartPoint[2];

        Plane plane = pl.GetPlane();
        Point2d endVer = Point2d.Origin;
        var i = 0;
        foreach (Curve3d c3d in cc3d.GetCurves())
        {
            if (c3d is CircularArc3d ca3d)
            {
                var b = Math.Tan(0.25 * (ca3d.EndAngle - ca3d.StartAngle)) * ca3d.Normal[2];
                pl.AddVertexAt(i, c3d.StartPoint.Convert2d(plane), b, 0, 0);
                endVer = c3d.EndPoint.Convert2d(plane);
            }
            else
            {
                pl.AddVertexAt(i, c3d.StartPoint.Convert2d(plane), 0, 0, 0);
                endVer = c3d.EndPoint.Convert2d(plane);
            }

            i++;
        }

        pl.AddVertexAt(i, endVer, 0, 0, 0);
        return pl;
    }

    #endregion CompositeCurve3d

    #region Line3d

    /// <summary>
    /// 将解析类三维构造线转换为实体类构造线
    /// </summary>
    /// <param name="line3d">解析类三维构造线</param>
    /// <returns>实体类构造线</returns>
    public static Xline ToCurve(this Line3d line3d)
    {
        return new Xline
        {
            BasePoint = line3d.PointOnLine,
            SecondPoint = line3d.PointOnLine + line3d.Direction
        };
    }

    /// <summary>
    /// 将三维解析类构造线转换为三维解析类线段
    /// </summary>
    /// <param name="line3d">三维解析类构造线</param>
    /// <param name="fromParameter">起点参数</param>
    /// <param name="toParameter">终点参数</param>
    /// <returns>三维解析类线段</returns>
    public static LineSegment3d ToLineSegment3d(this Line3d line3d, double fromParameter,
        double toParameter)
    {
        return new LineSegment3d(line3d.EvaluatePoint(fromParameter),
            line3d.EvaluatePoint(toParameter));
    }

    #endregion Line3d

    #region LineSegment3d

    /// <summary>
    /// 将三维解析类线段转换为实体类直线
    /// </summary>
    /// <param name="lineSeg3d">三维解析类线段</param>
    /// <returns>实体类直线</returns>
    public static Line ToCurve(this LineSegment3d lineSeg3d)
    {
        return new Line(lineSeg3d.StartPoint, lineSeg3d.EndPoint);
    }

    #endregion LineSegment3d

    #region CircularArc3d

    /// <summary>
    /// 将三维解析类圆/弧转换为实体圆/弧
    /// </summary>
    /// <param name="ca3d">三维解析类圆/弧</param>
    /// <returns>实体圆/弧</returns>
    public static Curve ToCurve(this CircularArc3d ca3d)
    {
        if (ca3d.IsClosed())
        {
            return ToCircle(ca3d);
        }

        return ToArc(ca3d);
    }

    /// <summary>
    /// 将三维解析类圆/弧转换为实体圆
    /// </summary>
    /// <param name="ca3d">三维解析类圆/弧</param>
    /// <returns>实体圆</returns>
    public static Circle ToCircle(this CircularArc3d ca3d) =>
        new(ca3d.Center, ca3d.Normal, ca3d.Radius);

    /// <summary>
    /// 将三维解析类圆/弧转换为实体圆弧
    /// </summary>
    /// <param name="ca3d">三维解析类圆/弧</param>
    /// <returns>实体圆弧</returns>
    public static Arc ToArc(this CircularArc3d ca3d)
    {
        // 必须新建，而不能直接使用GetPlane()获取
        var angle = ca3d.ReferenceVector.AngleOnPlane(new Plane(ca3d.Center, ca3d.Normal));
        return new Arc(ca3d.Center, ca3d.Normal, ca3d.Radius, ca3d.StartAngle + angle,
            ca3d.EndAngle + angle);
    }

    /// <summary>
    /// 将三维解析类圆/弧转换为三维解析类椭圆弧
    /// </summary>
    /// <param name="ca3d">三维解析类圆/弧</param>
    /// <returns>三维解析类椭圆弧</returns>
    public static EllipticalArc3d ToEllipticalArc3d(this CircularArc3d ca3d)
    {
        Vector3d zAxis = ca3d.Normal;
        Vector3d xAxis = ca3d.ReferenceVector;
        Vector3d yAxis = zAxis.CrossProduct(xAxis);

        return new EllipticalArc3d(ca3d.Center, xAxis, yAxis, ca3d.Radius, ca3d.Radius,
            ca3d.StartAngle, ca3d.EndAngle);
    }

    /// <summary>
    /// 将三维解析类圆/弧转换为三维解析类Nurb曲线
    /// </summary>
    /// <param name="ca3d">三维解析类圆/弧</param>
    /// <returns>三维解析类Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this CircularArc3d ca3d)
    {
        EllipticalArc3d ea3d = ToEllipticalArc3d(ca3d);
        NurbCurve3d nc3d = new(ea3d);
        return nc3d;
    }

    #endregion CircularArc3d

    #region EllipticalArc3d

    /// <summary>
    /// 将三维解析类椭圆弧转换为实体类椭圆弧
    /// </summary>
    /// <param name="ea3d">三维解析类椭圆弧</param>
    /// <returns>实体类椭圆弧</returns>
    public static Ellipse ToCurve(this EllipticalArc3d ea3d)
    {
        Ellipse ell = new(ea3d.Center, ea3d.Normal, ea3d.MajorAxis * ea3d.MajorRadius,
            ea3d.MinorRadius / ea3d.MajorRadius, 0, Math.PI * 2);
        // Ge椭圆角度就是Db椭圆的参数
        if (!ea3d.IsClosed())
        {
            ell.StartAngle = ell.GetAngleAtParameter(ea3d.StartAngle);
            ell.EndAngle = ell.GetAngleAtParameter(ea3d.EndAngle);
        }

        return ell;
    }

    #endregion EllipticalArc3d

    #region NurbCurve3d

    /// <summary>
    /// 将三维解析类Nurb曲线转换为实体类样条曲线
    /// </summary>
    /// <param name="nc3d">三维解析类Nurb曲线</param>
    /// <returns>实体类样条曲线</returns>
    public static Spline ToCurve(this NurbCurve3d nc3d)
    {
        Spline spl;
        if (nc3d.HasFitData)
        {
            NurbCurve3dFitData fData = nc3d.FitData;
            if (fData.TangentsExist)
            {
                spl = new Spline(fData.FitPoints, fData.StartTangent, fData.EndTangent, nc3d.Order,
                    fData.FitTolerance.EqualPoint);
            }
            else
            {
                spl = new Spline(fData.FitPoints, nc3d.Order, fData.FitTolerance.EqualPoint);
            }
        }
        else
        {
            DoubleCollection knots = new();
            foreach (double knot in nc3d.Knots)
                knots.Add(knot);

            NurbCurve3dData nurbCurve3dData = nc3d.DefinitionData;

            spl = new Spline(nurbCurve3dData.Degree, nurbCurve3dData.Rational, nc3d.IsClosed(),
                nurbCurve3dData.Periodic, nurbCurve3dData.ControlPoints, knots,
                nurbCurve3dData.Weights, Tolerance.Global.EqualPoint,
                nurbCurve3dData.Knots.Tolerance);
        }

        return spl;
    }

    #endregion NurbCurve3d

    #region PolylineCurve3d

    /// <summary>
    /// 将三维解析类多段线转换为实体类三维多段线
    /// </summary>
    /// <param name="pl3d">三维解析类多段线</param>
    /// <returns>实体类三维多段线</returns>
    public static Polyline3d ToCurve(this PolylineCurve3d pl3d)
    {
        using Point3dCollection pt3dCollection = new();

        for (var i = 0; i < pl3d.NumberOfControlPoints; i++)
            pt3dCollection.Add(pl3d.ControlPointAt(i));

        var closed = false;
        var n = pt3dCollection.Count - 1;
        if (pt3dCollection[0] == pt3dCollection[n])
        {
            pt3dCollection.RemoveAt(n);
            closed = true;
        }

        return new Polyline3d(Poly3dType.SimplePoly, pt3dCollection, closed);
    }

    #endregion PolylineCurve3d
}