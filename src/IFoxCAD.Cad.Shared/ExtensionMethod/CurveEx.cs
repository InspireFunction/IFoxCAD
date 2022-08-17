namespace IFoxCAD.Cad;

/// <summary>
/// 实体类曲线扩展类
/// </summary>
public static class CurveEx
{
    /// <summary>
    /// 曲线长度
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>长度</returns>
    public static double GetLength(this Curve curve)
    {
        return curve.GetDistanceAtParameter(curve.EndParam);
    }

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="pars">打断参数表</param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<double> pars)
    {
        if (pars is null)
            throw new ArgumentNullException(nameof(pars));

        return
            curve
            .GetSplitCurves(new DoubleCollection(pars.ToArray()))
            .Cast<Curve>();
    }

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="pars">打断参数表</param>
    /// <param name="isOrder">对参数表是否进行排序
    /// <para>
    /// <see langword="true"/>按参数值升序排序<br/>
    /// <see langword="false"/>不排序,默认值
    /// </para>
    /// </param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<double> pars, bool isOrder = false)
    {
        if (pars is null)
            throw new ArgumentNullException(nameof(pars));
        if (isOrder)
            pars = pars.OrderBy(x => x);

        return
            curve
            .GetSplitCurves(new DoubleCollection(pars.ToArray()))
            .Cast<Curve>();
    }

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="points">打断点表</param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<Point3d> points)
    {
        if (points is null)
            throw new ArgumentNullException(nameof(points));
        return
            curve
            .GetSplitCurves(new Point3dCollection(points.ToArray()))
            .Cast<Curve>();
    }

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="points">打断点表</param>
    /// <param name="isOrder">对点表是否进行排序
    /// <para>
    /// <see langword="true"/>按参数值升序排序<br/>
    /// <see langword="false"/>不排序,默认值
    /// </para>
    /// </param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<Point3d> points, bool isOrder = false)
    {
        if (points is null)
            throw new ArgumentNullException(nameof(points));
        if (isOrder)
            points = points.OrderBy(point => curve.GetParameterAtPoint(
                                             curve.GetClosestPointTo(point, false)));
        return
            curve
            .GetSplitCurves(new Point3dCollection(points.ToArray()))
            .Cast<Curve>();
    }

    /// <summary>
    /// 获取曲线集所围成的封闭区域的曲线集，注意此函数不能处理平行边（两个点及两条线组成的闭合环）
    /// </summary>
    /// <param name="curves">曲线集合</param>
    /// <returns>所有的闭合环的曲线集合</returns>
    public static IEnumerable<Curve> GetAllCycle(this IEnumerable<Curve> curves)
    {
        if (curves is null)
            throw new ArgumentNullException(nameof(curves));

        // 新建图
        var graph = new Graph();
        foreach (var curve in curves)
        {
#if NET35
            graph.AddEdge(curve.ToCurve3d()!);
#else 
            graph.AddEdge(curve.GetGeCurve());
#endif
        }
        //新建 dfs
        var dfs = new DepthFirst();
        // 查询全部的 闭合环
        dfs.FindAll(graph);
        // 遍历闭合环的列表，将每个闭合环转换为实体曲线
        var res = new List<Curve>();
        foreach (var item in dfs.Curve3ds)
        {
            var curve = graph.GetCurves(item.ToList()).ToArray();
            var comcur = new CompositeCurve3d(curve).ToCurve();
            if (comcur is not null)
                res.Add(comcur);
        }
        return res;
    }
    /// <summary>
    /// 曲线打断
    /// </summary>
    /// <param name="curves">曲线列表</param>
    /// <returns>打断后的曲线列表</returns>
    public static List<Curve> BreakCurve(this List<Curve> curves)
    {
        if (curves is null)
            throw new ArgumentNullException(nameof(curves));

        var geCurves = new List<CompositeCurve3d>(); // 存储曲线转换后的复合曲线
        var paramss = new List<List<double>>();      // 存储每个曲线的交点参数值

        for (int i = 0; i < curves.Count; i++)
        {
            var cc3d = curves[i].ToCompositeCurve3d();
            if (cc3d is not null)
            {
                geCurves.Add(cc3d);
                paramss.Add(new List<double>());
            }
        }

        //var oldCurves = new List<Curve>();
        var newCurves = new List<Curve>();
        var cci3d = new CurveCurveIntersector3d();

        for (int i = 0; i < curves.Count; i++)
        {
            var gc1 = geCurves[i];
            var pars1 = paramss[i]; //引用
            for (int j = i; j < curves.Count; j++)
            {
                var gc2 = geCurves[j];
                var pars2 = paramss[j]; // 引用

                cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]); // 引用修改会同步到源对象
                    pars2.Add(pars[1]); // 引用修改会同步到源对象
                }
            }

            if (pars1.Count > 0)
            {
                var c3ds = gc1.GetSplitCurves(pars1);
                if (c3ds.Count > 1)
                {
                    foreach (var c3d in c3ds)
                    {
                        var c3dCur = c3d.ToCurve();
                        if (c3dCur is not null)
                        {
                            c3dCur.SetPropertiesFrom(curves[i]);
                            newCurves.Add(c3dCur);
                        }
                    }
                    //oldCurves.Add(curves[i]);
                }
            }
        }

        return newCurves;
    }

    //转换DBCurve为GeCurved

    #region Curve

    /// <summary>
    /// 将曲线转换为ge曲线，此函数将在未来淘汰，二惊加油
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>ge曲线</returns>
    public static Curve3d? ToCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => ToCurve3d(li),
            Circle ci => ToCurve3d(ci),
            Arc arc => ToCurve3d(arc),
            Ellipse el => ToCurve3d(el),
            Polyline pl => ToCurve3d(pl),
            Polyline2d pl2 => ToCurve3d(pl2),
            Polyline3d pl3 => ToCurve3d(pl3),
            Spline sp => ToCurve3d(sp),
            _ => null
        };
    }

    /// <summary>
    /// 将曲线转换为复合曲线
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>复合曲线</returns>
    public static CompositeCurve3d? ToCompositeCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => new CompositeCurve3d(new Curve3d[] { ToCurve3d(li) }),
            Circle ci => new CompositeCurve3d(new Curve3d[] { ToCurve3d(ci) }),
            Arc arc => new CompositeCurve3d(new Curve3d[] { ToCurve3d(arc) }),
            Ellipse el => new CompositeCurve3d(new Curve3d[] { ToCurve3d(el) }),
            Polyline pl => new CompositeCurve3d(new Curve3d[] { ToCurve3d(pl) }),

            Polyline2d pl2 => new CompositeCurve3d(new Curve3d[] { ToCurve3d(pl2)! }),
            Polyline3d pl3 => new CompositeCurve3d(new Curve3d[] { ToCurve3d(pl3) }),
            Spline sp => new CompositeCurve3d(new Curve3d[] { ToCurve3d(sp) }),
            _ => null
        };
    }

    /// <summary>
    /// 将曲线转换为Nurb曲线
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => ToNurbCurve3d(li),
            Circle ci => ToNurbCurve3d(ci),
            Arc arc => ToNurbCurve3d(arc),
            Ellipse el => ToNurbCurve3d(el),
            Polyline pl => ToNurbCurve3d(pl),
            Polyline2d pl2 => ToNurbCurve3d(pl2),
            Polyline3d pl3 => ToNurbCurve3d(pl3),
            Spline sp => ToNurbCurve3d(sp),
            _ => null
        };
    }

    #region Line

    /// <summary>
    /// 将直线转换为ge直线
    /// </summary>
    /// <param name="line">直线</param>
    /// <returns>ge直线</returns>
    public static LineSegment3d ToCurve3d(this Line line)
    {
        return new LineSegment3d(line.StartPoint, line.EndPoint);
    }

    /// <summary>
    /// 将直线转换为Nurb曲线
    /// </summary>
    /// <param name="line">直线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Line line)
    {
        return new NurbCurve3d(ToCurve3d(line));
    }

    #endregion Line

    #region Circle

    /// <summary>
    /// 将圆转换为ge圆弧曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>ge圆弧曲线</returns>
    public static CircularArc3d ToCurve3d(this Circle cir)
    {
        return
            new CircularArc3d(
                cir.Center,
                cir.Normal,
                cir.Radius);
    }

    /// <summary>
    /// 将圆转换为ge椭圆曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>ge椭圆曲线</returns>
    public static EllipticalArc3d ToEllipticalArc3d(this Circle cir)
    {
        return ToCurve3d(cir).ToEllipticalArc3d();
    }

    /// <summary>
    /// 将圆转换为Nurb曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Circle cir)
    {
        return new NurbCurve3d(ToEllipticalArc3d(cir));
    }

    #endregion Circle

    #region Arc

    /// <summary>
    /// 将圆弧转换为ge圆弧曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>ge圆弧曲线</returns>
    public static CircularArc3d ToCurve3d(this Arc arc)
    {
        Plane plane = new(arc.Center, arc.Normal);

        return
            new CircularArc3d(
                arc.Center,
                arc.Normal,
                plane.GetCoordinateSystem().Xaxis,
                arc.Radius,
                arc.StartAngle,
                arc.EndAngle
                );
    }

    /// <summary>
    /// 将圆弧转换为ge椭圆曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>ge椭圆曲线</returns>
    public static EllipticalArc3d ToEllipticalArc3d(this Arc arc)
    {
        return ToCurve3d(arc).ToEllipticalArc3d();
    }

    /// <summary>
    /// 将圆弧转换为三维Nurb曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Arc arc)
    {
        return new NurbCurve3d(ToEllipticalArc3d(arc));
    }

    #endregion Arc

    #region Ellipse

    /// <summary>
    /// 将椭圆转换为三维ge椭圆曲线
    /// </summary>
    /// <param name="ell">椭圆</param>
    /// <returns>三维ge椭圆曲线</returns>
    public static EllipticalArc3d ToCurve3d(this Ellipse ell)
    {
        return
            new EllipticalArc3d(
                ell.Center,
                ell.MajorAxis.GetNormal(),
                ell.MinorAxis.GetNormal(),
                ell.MajorRadius,
                ell.MinorRadius,
                ell.StartParam,
                ell.EndParam);
    }

    /// <summary>
    /// 将椭圆转换为三维Nurb曲线
    /// </summary>
    /// <param name="ell">椭圆</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Ellipse ell)
    {
        return new NurbCurve3d(ToCurve3d(ell));
    }

    #endregion Ellipse

    #region Spline

    /// <summary>
    /// 将样条曲线转换为三维Nurb曲线
    /// </summary>
    /// <param name="spl">样条曲线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToCurve3d(this Spline spl)
    {
        NurbCurve3d nc3d;
        NurbsData ndata = spl.NurbsData;
        KnotCollection knots = new();
        foreach (Double knot in ndata.GetKnots())
            knots.Add(knot);

        if (ndata.Rational)
        {
            nc3d =
                new NurbCurve3d(
                    ndata.Degree,
                    knots,
                    ndata.GetControlPoints(),
                    ndata.GetWeights(),
                    ndata.Periodic);
        }
        else
        {
            nc3d =
                new NurbCurve3d(
                    ndata.Degree,
                    knots,
                    ndata.GetControlPoints(),
                    ndata.Periodic);
        }

        if (spl.HasFitData)
        {
            var fdata = spl.FitData;
            var vec = new Vector3d();
            if (fdata.TangentsExist && (fdata.StartTangent != vec || fdata.EndTangent != vec))
                nc3d.SetFitData(fdata.GetFitPoints(), fdata.StartTangent, fdata.EndTangent);
        }
        return nc3d;
    }

    #endregion Spline

    #region Polyline2d

    /// <summary>
    /// 将二维多段线转换为三维ge曲线
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>三维ge曲线</returns>
    public static Curve3d? ToCurve3d(this Polyline2d pl2d)
    {
        switch (pl2d.PolyType)
        {
            case Poly2dType.SimplePoly:
            case Poly2dType.FitCurvePoly:
                Polyline pl = new();
                pl.SetDatabaseDefaults();
                pl.ConvertFrom(pl2d, false);
                return ToCurve3d(pl);
            default:
                return ToNurbCurve3d(pl2d);
        }

        //Polyline pl = new Polyline();
        //pl.ConvertFrom(pl2d, false);
        //return ToCurve3d(pl);
    }

    /// <summary>
    /// 将二维多段线转换为三维Nurb曲线
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Polyline2d pl2d)
    {
        switch (pl2d.PolyType)
        {
            case Poly2dType.SimplePoly:
            case Poly2dType.FitCurvePoly:
                Polyline pl = new();
                pl.SetDatabaseDefaults();
                pl.ConvertFrom(pl2d, false);
                return ToNurbCurve3d(pl);

            default:
                return ToCurve3d(pl2d.Spline);
        }
    }

    /// <summary>
    /// 将二维多段线转换为三维ge多段线
    /// </summary>
    /// <param name="pl">二维多段线</param>
    /// <returns>三维ge多段线</returns>
    public static PolylineCurve3d ToPolylineCurve3d(this Polyline2d pl)
    {
        Point3dCollection pnts = new();
        foreach (Vertex2d ver in pl)
        {
            pnts.Add(ver.Position);
        }
        return new PolylineCurve3d(pnts);
    }

    #endregion Polyline2d

    #region Polyline3d

    /// <summary>
    /// 将三维多段线转换为三维曲线
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>三维曲线</returns>
    public static Curve3d ToCurve3d(this Polyline3d pl3d)
    {
        return pl3d.PolyType switch
        {
            Poly3dType.SimplePoly => ToPolylineCurve3d(pl3d),
            _ => ToNurbCurve3d(pl3d),
        };
    }

    /// <summary>
    /// 将三维多段线转换为三维Nurb曲线
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Polyline3d pl3d)
    {
        return ToCurve3d(pl3d.Spline);
    }

    /// <summary>
    /// 将三维多段线转换为三维ge多段线
    /// </summary>
    /// <param name="pl">三维多段线</param>
    /// <returns>三维ge多段线</returns>
    public static PolylineCurve3d ToPolylineCurve3d(this Polyline3d pl)
    {
        Point3dCollection pnts = new();
        foreach (ObjectId id in pl)
        {
            PolylineVertex3d ver = (PolylineVertex3d)id.GetObject(OpenMode.ForRead);
            pnts.Add(ver.Position);
        }
        return new PolylineCurve3d(pnts);
    }

    #endregion Polyline3d

    #region Polyline

    /// <summary>
    /// 多段线转换为复合曲线
    /// </summary>
    /// <param name="pl">多段线对象</param>
    /// <returns>复合曲线对象</returns>
    public static CompositeCurve3d ToCurve3d(this Polyline pl)
    {
        List<Curve3d> c3ds = new();

        for (int i = 0; i < pl.NumberOfVertices; i++)
        {
            switch (pl.GetSegmentType(i))
            {
                case SegmentType.Line:
                    c3ds.Add(pl.GetLineSegmentAt(i));
                    break;

                case SegmentType.Arc:
                    c3ds.Add(pl.GetArcSegmentAt(i));
                    break;

                default:
                    break;
            }
        }
        return new CompositeCurve3d(c3ds.ToArray());
    }

    /// <summary>
    /// 多段线转换为Nurb曲线
    /// </summary>
    /// <param name="pl">多段线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Polyline pl)
    {
        NurbCurve3d? nc3d = null;
        for (int i = 0; i < pl.NumberOfVertices; i++)
        {
            NurbCurve3d? nc3dtemp = null;
            switch (pl.GetSegmentType(i))
            {
                case SegmentType.Line:
                    nc3dtemp = new NurbCurve3d(pl.GetLineSegmentAt(i));
                    break;

                case SegmentType.Arc:
                    nc3dtemp = pl.GetArcSegmentAt(i).ToNurbCurve3d();
                    break;

                default:
                    break;
            }
            if (nc3d is null)
            {
                nc3d = nc3dtemp;
            }
            else if (nc3dtemp is not null)
            {
                nc3d.JoinWith(nc3dtemp);
            }
        }
        return nc3d;
    }

    /// <summary>
    /// 为优化多段线倒角
    /// </summary>
    /// <param name="polyline">优化多段线</param>
    /// <param name="index">顶点索引号</param>
    /// <param name="radius">倒角半径</param>
    /// <param name="isFillet">倒角类型</param>
    public static void ChamferAt(this Polyline polyline, int index, double radius, bool isFillet)
    {
        if (index < 1 || index > polyline.NumberOfVertices - 2)
            throw new System.Exception("错误的索引号");

        if (SegmentType.Line != polyline.GetSegmentType(index - 1) ||
            SegmentType.Line != polyline.GetSegmentType(index))
            throw new System.Exception("非直线段不能倒角");

        //获取当前索引号的前后两段直线,并组合为Ge复合曲线
        Curve3d[] c3ds =
            new Curve3d[]
                {
                        polyline.GetLineSegmentAt(index - 1),
                        polyline.GetLineSegmentAt(index)
                };
        CompositeCurve3d cc3d = new(c3ds);

        //试倒直角
        //子曲线的个数有三种情况:
        //1、=3时倒角方向正确
        //2、=2时倒角方向相反
        //3、=0或为直线时失败
        c3ds =
            cc3d.GetTrimmedOffset
            (
                radius,
                Vector3d.ZAxis,
                OffsetCurveExtensionType.Chamfer
            );

        if (c3ds.Length > 0 && c3ds[0] is CompositeCurve3d)
        {
            var newcc3d = c3ds[0] as CompositeCurve3d;
            c3ds = newcc3d!.GetCurves();
            if (c3ds.Length == 3)
            {
                c3ds = cc3d.GetTrimmedOffset
                        (
                            -radius,
                            Vector3d.ZAxis,
                            OffsetCurveExtensionType.Chamfer
                        );
                if (c3ds.Length == 0 || c3ds[0] is LineSegment3d)
                    throw new System.Exception("倒角半径过大");
            }
            else if (c3ds.Length == 2)
            {
                radius = -radius;
            }
        }
        else
        {
            throw new System.Exception("倒角半径过大");
        }

        //GetTrimmedOffset会生成倒角+偏移，故先反方向倒角,再倒回
        c3ds = cc3d.GetTrimmedOffset
                (
                    -radius,
                    Vector3d.ZAxis,
                    OffsetCurveExtensionType.Extend
                );
        OffsetCurveExtensionType type =
            isFillet ?
            OffsetCurveExtensionType.Fillet : OffsetCurveExtensionType.Chamfer;
        c3ds = c3ds[0].GetTrimmedOffset
                (
                    radius,
                    Vector3d.ZAxis,
                    type
                );

        //将结果Ge曲线转为Db曲线,并将相关的数值反映到原曲线
        var plTemp = c3ds[0].ToCurve() as Polyline;
        if (plTemp is null)
            return;
        polyline.RemoveVertexAt(index);
        polyline.AddVertexAt(index, plTemp.GetPoint2dAt(1), plTemp.GetBulgeAt(1), 0, 0);
        polyline.AddVertexAt(index + 1, plTemp.GetPoint2dAt(2), 0, 0, 0);
    }

    #endregion Polyline

    #endregion
}