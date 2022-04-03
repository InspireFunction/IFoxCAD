namespace IFoxCAD.Cad;

using IFoxCAD.Basal;
using System.Diagnostics;

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
    /// <summary>
    /// 边节点
    /// </summary>
    private struct EdgeItem : IEquatable<EdgeItem>
    {
        public Edge Edge;
        public bool Forward;

        public EdgeItem(Edge edge, bool forward)
        {
            Edge = edge;
            Forward = forward;
        }

        public CompositeCurve3d? GetCurve()
        {
            var cc3d = Edge.Curve;
            if (Forward)
            {
                return cc3d;
            }
            else
            {
                cc3d = cc3d.Clone() as CompositeCurve3d;
                return cc3d?.GetReverseParameterCurve() as CompositeCurve3d;
            }
        }

        public bool Equals(EdgeItem other)
        {
            return Edge == other.Edge && Forward == other.Forward;
        }

        /// <summary>
        /// 查找区域
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="regions"></param>
        public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions)
        {
            var region = new LoopList<EdgeItem>();
            var edgeItem = this;
            region.Add(edgeItem);
            var edgeItem2 = this.GetNext(edges);
            if (edgeItem2.Edge is not null)
            {
                bool hasList = false;
                foreach (var edgeList2 in regions)
                {
                    var node = edgeList2.GetNode(e => e.Equals(edgeItem));
                    //var node = edgeList2.Find(edgeItem);
                    if (node is not null && node != edgeList2.Last)
                    {
                        if (node.Next!.Value.Equals(edgeItem2))
                        {
                            hasList = true;
                            break;
                        }
                    }
                }
                if (!hasList)
                {
                    while (edgeItem2.Edge is not null)
                    {
                        if (edgeItem2.Edge == edgeItem.Edge)
                            break;
                        region.Add(edgeItem2); //上一条语句判断失误，导致不停的将相同的值加入region,不能退出循环
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

            EdgeItem item = new();
            Vector3d vec2, vec3 = new();
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

        public override bool Equals(object obj)
        {
            return obj is EdgeItem item && Equals(item);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
    /// <summary>
    /// 边
    /// </summary>
    private class Edge
    {
        public CompositeCurve3d Curve;
        public int StartIndex;
        public int EndIndex;
        public Edge(CompositeCurve3d curve)
        {
            Curve = curve;
        }
        public Vector3d GetStartVector()
        {
            var inter = Curve.GetInterval();
            PointOnCurve3d poc = new(Curve, inter.LowerBound);
            return poc.GetDerivative(1);
        }

        public Vector3d GetEndVector()
        {
            var inter = Curve.GetInterval();
            PointOnCurve3d poc = new(Curve, inter.UpperBound);
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
    /// <summary>
    /// 获取曲线集所围成的封闭区域的曲线集
    /// </summary>
    /// <param name="curves">曲线集</param>
    /// <returns>闭合的曲线集</returns>
    public static List<Curve> Topo(List<Curve> curves)
    {
        var edges = new List<Edge>();
        var newCurves = new List<Curve>();
        GetEdgesAndnewCurves(curves, edges, newCurves);

        //构建边的邻接表
        //knots 和 nums 实际上是一个键值对(基于ArrayOfStruct思想,拆开更合适内存布局)
        //knots 是不重复地将所有交点设置为节点,如果是重复,就对应 nums++
        //nums 是记录每个节点坐标被重复了几次
        var knots = new List<Point3d>();
        var nums = new List<int>();
        var closedEdges = new List<Edge>();

        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (edge.Curve.IsClosed())
            {
                closedEdges.Add(edge);
                continue;
            }

            if (knots.Contains(edge.Curve.StartPoint))
            {
                //含有就是其他曲线"共用"此交点,
                //节点所在索引==共用计数索引=>将它++
                edge.StartIndex = knots.IndexOf(edge.Curve.StartPoint);
                nums[edge.StartIndex]++;
            }
            else
            {
                //不含有就加入节点,共用计数也加入,边界加入节点索引
                knots.Add(edge.Curve.StartPoint);
                nums.Add(1);
                edge.StartIndex = knots.Count - 1;
            }

            if (knots.Contains(edge.Curve.EndPoint))
            {
                edge.EndIndex = knots.IndexOf(edge.Curve.EndPoint);
                nums[edge.EndIndex]++;
            }
            else
            {
                knots.Add(edge.Curve.EndPoint);
                nums.Add(1);
                edge.EndIndex = knots.Count - 1;
            }
        }

        newCurves.AddRange(closedEdges.Select(e => e.Curve.ToCurve())!);

        //这里把交点只有一条曲线通过的点过滤掉了,也就是尾巴的图元,
        //剩下的都是闭合的曲线连接了,每个点都至少有两条曲线通过
        edges =
            edges
            .Except(closedEdges)
            .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1)
            .ToList();

        //这一大坨 不用看了 注释掉也没影响貌似,而且后续也没有用 nums
        // foreach (var edge in edges.Except(closedEdges))
        // {
        //     if (nums[edge.StartIndex] == 1 || nums[edge.EndIndex] == 1)
        //     {
        //         if (nums[edge.StartIndex] == 1 && nums[edge.EndIndex] == 1)
        //         {
        //             nums[edge.StartIndex] = 0;
        //             nums[edge.EndIndex] = 0;
        //         }
        //         else
        //         {
        //             int next = -1;
        //             if (nums[edge.StartIndex] == 1)
        //             {
        //                 nums[edge.StartIndex] = 0;
        //                 nums[next = edge.EndIndex]--;
        //             }
        //             else
        //             {
        //                 nums[edge.EndIndex] = 0;
        //                 nums[next = edge.StartIndex]--;
        //             }
        //         }
        //     }
        // }

        var regions = new List<LoopList<EdgeItem>>();
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            //TODO 这里有bug,两个内接的矩形会卡死
            var edgeItem = new EdgeItem(edge, true);
            edgeItem.FindRegion(edges, regions); // 经测试是这里卡住了
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
                    var curve = node!.Value.Edge.Curve;
                    var node2 = regions[j].GetNode(e => e.Edge.Curve == curve);
                    //var node2 = regions[j].Find(node.Value);
                    if (node2 is not null)
                    {
                        eq = true;
                        var b = node.Value.Forward;
                        var b2 = node2.Value.Forward;
                        for (int k = 1; k < regions[i].Count; k++)
                        {
                            node = node.GetNext(b);
                            node2 = node2.GetNext(b2);
                            if (node!.Value.Edge.Curve != node2!.Value.Edge.Curve)
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

        for (int i = 0; i < regions.Count; i++)
        {
            var cs3ds =
                regions[i]
                .Select(e => e.GetCurve())
                .ToArray();
            newCurves.Add(new CompositeCurve3d(cs3ds).ToCurve()!);
        }

        return newCurves;
    }

    /// <summary>
    /// 从曲线集合分离边界(交点断分曲线的)和独立的曲线
    /// </summary>
    /// <param name="curves">传入判断的曲线集</param>
    /// <param name="edgesOut">边界(可能仍然存在自闭,因为样条曲线允许打个鱼形圈,尾巴又交叉在其他曲线)</param>
    /// <param name="closedCurvesOut">自闭的曲线</param>
    static void GetEdgesAndnewCurves(List<Curve> curves,
        List<Edge> edgesOut,
        List<Curve> closedCurvesOut)
    {
        //首先按交点分解为Ge曲线集
        var geCurves = new List<CompositeCurve3d>();
        var paramss = new List<List<double>>();

        for (int i = 0; i < curves.Count; i++)
        {
            var cc3d = curves[i].ToCompositeCurve3d();
            if (cc3d is not null)
            {
                geCurves.Add(cc3d);
                paramss.Add(new List<double>());
            }
        }

        var cci3d = new CurveCurveIntersector3d();

        //遍历所有曲线,然后获取交点...此处是O(n²)
        for (int i = 0; i < curves.Count; i++)
        {
            var gc1 = geCurves[i];
            var pars1 = paramss[i];
            //曲线a,和曲线b/c/d/e...分别交点,组成交点数组
            //第一次是 aa对比,所以会怎么样呢?(交点无限个)
            for (int j = i; j < curves.Count; j++)
            {
                var gc2 = geCurves[j];
                var pars2 = paramss[j];

                //求交类此方法内部会重置,不需要清空,每次set都会有个新的结果 
                cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                //计算两条曲线的交点(多个),分别放入对应的交点参数集
                for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]);//0是第一条曲线的交点参数
                    pars2.Add(pars[1]);//1是第二条曲线的交点参数
                }
            }

            //有交点参数
            if (pars1.Count > 0)
            {
                //根据交点参数断分曲线,然后获取边界
                var c3ds = gc1.GetSplitCurves(pars1);
                if (c3ds.Count > 0)
                {
                    edgesOut.AddRange(c3ds.Select(c => new Edge(c)));
                }
                //惊惊留:(不敢删啊...)
                //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                //猜测:曲线参数在头或者尾,那么交点就是直接碰头碰尾,
                //也就是 aa对比 同一条曲线自己和自己产生的?
                //参数大于0{是这些参数? 头参/尾参/参数不在曲线上?}
                else if (gc1.IsClosed())
                {
                    closedCurvesOut.Add(gc1.ToCurve()!);
                }
                else
                {
                    edgesOut.Add(new Edge(gc1));
                }
            }
            else if (gc1.IsClosed())
            {
                closedCurvesOut.Add(gc1.ToCurve()!);
            }
        }
    }

    /// <summary>
    /// 曲线打断
    /// </summary>
    /// <param name="curves">曲线列表</param>
    /// <returns>打断后的曲线列表</returns>
    public static List<Curve> BreakCurve(this List<Curve> curves)
    {
        var geCurves = new List<CompositeCurve3d>(); // 存储曲线转换后的复合曲线
        var paramss = new List<List<double>>(); // 存储每个曲线的交点参数值

        foreach (var curve in curves)
        {
            var cc3d = curve.ToCompositeCurve3d();
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
                    foreach (CompositeCurve3d c3d in c3ds)
                    {
                        var c = c3d.ToCurve();
                        if (c is not null)
                        {
                            c.SetPropertiesFrom(curves[i]);
                            newCurves.Add(c);
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
    [Obsolete("请使用Cad自带的 GetGeCurve 函数！")]
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

        if (polyline.GetSegmentType(index - 1) != SegmentType.Line || polyline.GetSegmentType(index) != SegmentType.Line)
            throw new System.Exception("非直线段不能倒角");

        //获取当前索引号的前后两段直线,并组合为Ge复合曲线
        Curve3d[] c3ds =
            new Curve3d[]
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
                c3ds =
                    cc3d.GetTrimmedOffset
                    (
                        -radius,
                        Vector3d.ZAxis,
                        OffsetCurveExtensionType.Chamfer
                    );
                if (c3ds.Length == 0 || c3ds[0] is LineSegment3d)
                {
                    throw new System.Exception("倒角半径过大");
                }
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
        c3ds =
            cc3d.GetTrimmedOffset
            (
                -radius,
                Vector3d.ZAxis,
                OffsetCurveExtensionType.Extend
            );
        OffsetCurveExtensionType type =
            isFillet ?
            OffsetCurveExtensionType.Fillet : OffsetCurveExtensionType.Chamfer;
        c3ds =
            c3ds[0].GetTrimmedOffset
            (
                radius,
                Vector3d.ZAxis,
                type
            );

        //将结果Ge曲线转为Db曲线,并将相关的数值反映到原曲线
        Polyline? plTemp = c3ds[0].ToCurve() as Polyline;
        polyline.RemoveVertexAt(index);
        polyline.AddVertexAt(index, plTemp!.GetPoint2dAt(1), plTemp.GetBulgeAt(1), 0, 0);
        polyline.AddVertexAt(index + 1, plTemp.GetPoint2dAt(2), 0, 0, 0);
    }

    #endregion Polyline

    #endregion Curve
}
