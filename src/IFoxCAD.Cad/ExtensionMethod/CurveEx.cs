using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using IFoxCAD.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IFoxCAD.Cad
{
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
            return
                curve.GetDistanceAtParameter(curve.EndParam);
        }

        /// <summary>
        /// 获取分割曲线集合
        /// </summary>
        /// <param name="curve">曲线</param>
        /// <param name="pars">打断参数表</param>
        /// <returns>打断后曲线的集合</returns>
        public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<double> pars)
        {
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
            return
                curve
                .GetSplitCurves(new Point3dCollection(points.ToArray()))
                .Cast<Curve>();
        }

        private struct EdgeItem : IEquatable<EdgeItem>
        {
            public Edge Edge;
            public bool Forward;

            public EdgeItem(Edge edge, bool forward)
            {
                Edge = edge;
                Forward = forward;
            }

            public CompositeCurve3d GetCurve()
            {
                CompositeCurve3d cc3d = Edge.Curve;
                if (Forward)
                {
                    return cc3d;
                }
                else
                {
                    cc3d = cc3d.Clone() as CompositeCurve3d;
                    return cc3d.GetReverseParameterCurve() as CompositeCurve3d;
                }
            }

            public bool Equals(EdgeItem other)
            {
                return
                   Edge == other.Edge &&
                   Forward == other.Forward;
            }

            public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions)
            {
                var region = new LoopList<EdgeItem>();
                var edgeItem = this;
                region.Add(edgeItem);
                var edgeItem2 = this.GetNext(edges);
                if (edgeItem2.Edge != null)
                {
                    bool hasList = false;
                    foreach (var edgeList2 in regions)
                    {
                        var node = edgeList2.GetNode(e => e.Equals(edgeItem));
                        if (node != null)
                        {
                            if (node.Next.Value.Equals(edgeItem2))
                            {
                                hasList = true;
                                break;
                            }
                        }
                    }
                    if (!hasList)
                    {
                        while (edgeItem2.Edge != null)
                        {
                            if (edgeItem2.Edge == edgeItem.Edge)
                                break;
                            region.Add(edgeItem2);
                            edgeItem2 = edgeItem2.GetNext(edges);
                        }
                        if (edgeItem2.Edge == edgeItem.Edge)
                            regions.Add(region);
                    }
                }
            }

            public EdgeItem GetNext(List<Edge> edges)
            {
                Vector3d vec;
                int next;
                if (Forward)
                {
                    vec = Edge.GetEndVector();
                    next = Edge.EndIndex;
                }
                else
                {
                    vec = Edge.GetStartVector();
                    next = Edge.StartIndex;
                }

                EdgeItem item = new EdgeItem();
                Vector3d vec2, vec3 = new Vector3d();
                double angle = 0;
                bool hasNext = false;
                bool forward = false;
                foreach (var edge in edges)
                {
                    if (edge.IsNext(Edge, next, ref vec3, ref forward))
                    {
                        if (hasNext)
                        {
                            var angle2 = vec.GetAngleTo(vec3, Vector3d.ZAxis);
                            if (angle2 < angle)
                            {
                                vec2 = vec3;
                                angle = angle2;
                                item.Edge = edge;
                                item.Forward = forward;
                            }
                        }
                        else
                        {
                            vec2 = vec3;
                            angle = vec.GetAngleTo(vec2, Vector3d.ZAxis);
                            item.Edge = edge;
                            item.Forward = forward;
                            hasNext = true;
                        }
                    }
                }
                return item;
            }

            public override string ToString()
            {
                return
                    Forward ?
                    string.Format("{0}-{1}", Edge.StartIndex, Edge.EndIndex) :
                    string.Format("{0}-{1}", Edge.EndIndex, Edge.StartIndex);
            }
        }

        private class Edge
        {
            public CompositeCurve3d Curve;
            public int StartIndex;
            public int EndIndex;

            public Vector3d GetStartVector()
            {
                var inter = Curve.GetInterval();
                PointOnCurve3d poc = new PointOnCurve3d(Curve, inter.LowerBound);
                return poc.GetDerivative(1);
            }

            public Vector3d GetEndVector()
            {
                var inter = Curve.GetInterval();
                PointOnCurve3d poc = new PointOnCurve3d(Curve, inter.UpperBound);
                return -poc.GetDerivative(1);
            }

            public bool IsNext(Edge edge, int index, ref Vector3d vec, ref bool forward)
            {
                if (edge != this)
                {
                    if (StartIndex == index)
                    {
                        vec = GetStartVector();
                        forward = true;
                        return true;
                    }
                    else if (EndIndex == index)
                    {
                        vec = GetEndVector();
                        forward = false;
                        return true;
                    }
                }
                return false;
            }
        }

        public static List<Curve> Topo(List<Curve> curves)
        {
            //首先按交点分解为Ge曲线集
            List<CompositeCurve3d> geCurves = new List<CompositeCurve3d>();
            List<List<double>> paramss = new List<List<double>>();

            foreach (var curve in curves)
            {
                var cc3d = curve.ToCompositeCurve3d();
                if (cc3d != null)
                {
                    geCurves.Add(cc3d);
                    paramss.Add(new List<double>());
                }
            }

            List<Edge> edges = new List<Edge>();
            CurveCurveIntersector3d cci3d = new CurveCurveIntersector3d();
            List<Curve> newCurves = new List<Curve>();

            for (int i = 0; i < curves.Count; i++)
            {
                CompositeCurve3d gc1 = geCurves[i];
                List<double> pars1 = paramss[i];
                for (int j = i; j < curves.Count; j++)
                {
                    CompositeCurve3d gc2 = geCurves[j];
                    List<double> pars2 = paramss[j];

                    cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                    for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                    {
                        double[] pars = cci3d.GetIntersectionParameters(k);
                        pars1.Add(pars[0]);
                        pars2.Add(pars[1]);
                    }
                }

                if (pars1.Count > 0)
                {
                    List<CompositeCurve3d> c3ds = gc1.GetSplitCurves(pars1);
                    if (c3ds.Count > 0)
                    {
                        edges.AddRange(
                            c3ds.Select(c => new Edge { Curve = c }));
                    }
                    else if (gc1.IsClosed())
                    {
                        newCurves.Add(gc1.ToCurve());
                    }
                    else
                    {
                        edges.Add(new Edge { Curve = gc1 });
                    }
                }
                else if (gc1.IsClosed())
                {
                    newCurves.Add(gc1.ToCurve());
                }
            }

            //构建边的邻接表
            var knots = new List<Point3d>();
            var nums = new List<int>();
            var closedEdges = new List<Edge>();

            foreach (var edge in edges)
            {
                if (edge.Curve.IsClosed())
                {
                    closedEdges.Add(edge);
                }
                else
                {
                    if (knots.Contains(edge.Curve.StartPoint))
                    {
                        edge.StartIndex =
                            knots.IndexOf(edge.Curve.StartPoint);
                        nums[edge.StartIndex]++;
                    }
                    else
                    {
                        knots.Add(edge.Curve.StartPoint);
                        nums.Add(1);
                        edge.StartIndex = knots.Count - 1;
                    }

                    if (knots.Contains(edge.Curve.EndPoint))
                    {
                        edge.EndIndex =
                            knots.IndexOf(edge.Curve.EndPoint);
                        nums[edge.EndIndex]++;
                    }
                    else
                    {
                        knots.Add(edge.Curve.EndPoint);
                        nums.Add(1);
                        edge.EndIndex = knots.Count - 1;
                    }
                }
            }

            newCurves.AddRange(closedEdges.Select(e => e.Curve.ToCurve()));

            edges =
                edges
                .Except(closedEdges)
                .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1)
                .ToList();

            foreach (var edge in edges.Except(closedEdges))
            {
                if (nums[edge.StartIndex] == 1 || nums[edge.EndIndex] == 1)
                {
                    if (nums[edge.StartIndex] == 1 && nums[edge.EndIndex] == 1)
                    {
                        nums[edge.StartIndex] = 0;
                        nums[edge.EndIndex] = 0;
                    }
                    else
                    {
                        int next = -1;
                        if (nums[edge.StartIndex] == 1)
                        {
                            nums[edge.StartIndex] = 0;
                            nums[next = edge.EndIndex]--;
                        }
                        else
                        {
                            nums[edge.EndIndex] = 0;
                            nums[next = edge.StartIndex]--;
                        }
                    }
                }
            }

            List<LoopList<EdgeItem>> regions = new List<LoopList<EdgeItem>>();
            foreach (var edge in edges)
            {
                var edgeItem = new EdgeItem(edge, true);
                edgeItem.FindRegion(edges, regions);
                edgeItem = new EdgeItem(edge, false);
                edgeItem.FindRegion(edges, regions);
            }

            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = i + 1; j < regions.Count;)
                {
                    bool eq = false;
                    if (regions[i].Count == regions[j].Count)
                    {
                        var node = regions[i].First;
                        var curve = node.Value.Edge.Curve;
                        var node2 = regions[j].GetNode(e => e.Edge.Curve == curve);
                        if (eq = node2 != null)
                        {
                            var b = node.Value.Forward;
                            var b2 = node2.Value.Forward;
                            for (int k = 1; k < regions[i].Count; k++)
                            {
                                node = node.GetNext(b);
                                node2 = node2.GetNext(b2);
                                if (node.Value.Edge.Curve != node2.Value.Edge.Curve)
                                {
                                    eq = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (eq)
                        regions.RemoveAt(j);
                    else
                        j++;
                }
            }

            foreach (var region in regions)
            {
                var cs3ds =
                    region
                    .Select(e => e.GetCurve())
                    .ToArray();
                newCurves.Add(new CompositeCurve3d(cs3ds.ToArray()).ToCurve());
            }

            return newCurves;
        }

        /// <summary>
        /// 曲线打断
        /// </summary>
        /// <param name="curves">曲线列表</param>
        /// <returns>打断后的曲线列表</returns>
        public static List<Curve> BreakCurve(List<Curve> curves)
        {
            List<CompositeCurve3d> geCurves = new List<CompositeCurve3d>();
            List<List<double>> paramss = new List<List<double>>();

            foreach (var curve in curves)
            {
                var cc3d = curve.ToCompositeCurve3d();
                if (cc3d != null)
                {
                    geCurves.Add(cc3d);
                    paramss.Add(new List<double>());
                }
            }

            List<Curve> oldCurves = new List<Curve>();
            List<Curve> newCurves = new List<Curve>();
            CurveCurveIntersector3d cci3d = new CurveCurveIntersector3d();

            for (int i = 0; i < curves.Count; i++)
            {
                CompositeCurve3d gc1 = geCurves[i];
                List<double> pars1 = paramss[i];
                for (int j = i; j < curves.Count; j++)
                {
                    CompositeCurve3d gc2 = geCurves[j];
                    List<double> pars2 = paramss[j];

                    cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                    for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                    {
                        double[] pars = cci3d.GetIntersectionParameters(k);
                        pars1.Add(pars[0]);
                        pars2.Add(pars[1]);
                    }
                }

                if (pars1.Count > 0)
                {
                    List<CompositeCurve3d> c3ds = gc1.GetSplitCurves(pars1);
                    if (c3ds.Count > 1)
                    {
                        foreach (CompositeCurve3d c3d in c3ds)
                        {
                            Curve c = c3d.ToCurve();
                            if (c != null)
                            {
                                c.SetPropertiesFrom(curves[i]);
                                newCurves.Add(c);
                            }
                        }
                        oldCurves.Add(curves[i]);
                    }
                }
            }
            curves = oldCurves;
            return newCurves;
        }

        //转换DBCurve为GeCurved

        #region Curve

        /// <summary>
        /// 将曲线转换为ge曲线，此函数将在未来淘汰，二惊加油
        /// </summary>
        /// <param name="curve">曲线</param>
        /// <returns>ge曲线</returns>
        [Obsolete("请使用Cad自带的 GetGeCurve 函数！")]
        public static Curve3d ToCurve3d(this Curve curve)
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
        public static CompositeCurve3d ToCompositeCurve3d(this Curve curve)
        {
            return curve switch
            {
                Line li => new CompositeCurve3d(new Curve3d[] { ToCurve3d(li) }),
                Circle ci => new CompositeCurve3d(new Curve3d[] { ToCurve3d(ci) }),
                Arc arc => new CompositeCurve3d(new Curve3d[] { ToCurve3d(arc) }),
                Ellipse el => new CompositeCurve3d(new Curve3d[] { ToCurve3d(el) }),
                Polyline pl => new CompositeCurve3d(new Curve3d[] { ToCurve3d(pl) }),

                Polyline2d pl2 => new CompositeCurve3d(new Curve3d[] { ToCurve3d(pl2) }),
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
        public static NurbCurve3d ToNurbCurve3d(this Curve curve)
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
            Plane plane = new Plane(arc.Center, arc.Normal);

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
            KnotCollection knots = new KnotCollection();
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
        public static Curve3d ToCurve3d(this Polyline2d pl2d)
        {
            switch (pl2d.PolyType)
            {
                case Poly2dType.SimplePoly:
                case Poly2dType.FitCurvePoly:
                    Polyline pl = new Polyline();
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
        public static NurbCurve3d ToNurbCurve3d(this Polyline2d pl2d)
        {
            switch (pl2d.PolyType)
            {
                case Poly2dType.SimplePoly:
                case Poly2dType.FitCurvePoly:
                    Polyline pl = new Polyline();
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
            var pnts = new Point3dCollection();
            foreach (Vertex2d ver in pl)
                pnts.Add(ver.Position);
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
            Point3dCollection pnts = new Point3dCollection();
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
            List<Curve3d> c3ds = new List<Curve3d>();

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
        public static NurbCurve3d ToNurbCurve3d(this Polyline pl)
        {
            NurbCurve3d nc3d = null;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                NurbCurve3d nc3dtemp = null;
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
                if (nc3d == null)
                {
                    nc3d = nc3dtemp;
                }
                else if (nc3dtemp != null)
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

            if (polyline.GetSegmentType(index - 1) != SegmentType.Line ||
                polyline.GetSegmentType(index) != SegmentType.Line)
                throw new System.Exception("非直线段不能倒角");

            //获取当前索引号的前后两段直线,并组合为Ge复合曲线
            var c3ds = new Curve3d[]
            {
                polyline.GetLineSegmentAt(index - 1),
                polyline.GetLineSegmentAt(index)
            };
            var cc3d = new CompositeCurve3d(c3ds);

            //试倒直角
            //子曲线的个数有三种情况:
            //1、=3时倒角方向正确
            //2、=2时倒角方向相反
            //3、=0或为直线时失败
            c3ds = cc3d.GetTrimmedOffset
            (
                radius,
                Vector3d.ZAxis,
                OffsetCurveExtensionType.Chamfer
            );

            if (c3ds.Length > 0 && c3ds[0] is CompositeCurve3d newcc3d)
            {
                c3ds = newcc3d.GetCurves();
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
            var type = isFillet ? OffsetCurveExtensionType.Fillet : OffsetCurveExtensionType.Chamfer;
            c3ds = c3ds[0].GetTrimmedOffset
            (
                radius,
                Vector3d.ZAxis,
                type
            );

            //将结果Ge曲线转为Db曲线,并将相关的数值反映到原曲线
            var plTemp = c3ds[0].ToCurve() as Polyline;
            polyline.RemoveVertexAt(index);
            polyline.AddVertexAt(index, plTemp.GetPoint2dAt(1), plTemp.GetBulgeAt(1), 0, 0);
            polyline.AddVertexAt(index + 1, plTemp.GetPoint2dAt(2), 0, 0, 0);
        }

        #endregion Polyline

        #endregion Curve
    }
}
