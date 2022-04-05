namespace IFoxCAD.Cad;

using IFoxCAD.Basal;


public class Edge : IEquatable<Edge>
{
    #region 字段
    public CompositeCurve3d GeCurve3d;
    public int StartIndex;
    public int EndIndex;
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
    #endregion

    #region 构造
    /// <summary>
    /// 边线(没有包围盒,除非ToCurve)
    /// </summary>
    public Edge(CompositeCurve3d geCurve3d)
    {
        GeCurve3d = geCurve3d;
    }
    public Edge(Edge edge) : this(edge.GeCurve3d)
    {
        StartIndex = edge.StartIndex;
        EndIndex = edge.EndIndex;
    }
    #endregion

    #region 方法
    public Vector3d GetStartVector()
    {
        var inter = GeCurve3d.GetInterval();
        PointOnCurve3d poc = new(GeCurve3d, inter.LowerBound);
        return poc.GetDerivative(1);
    }

    public Vector3d GetEndVector()
    {
        var inter = GeCurve3d.GetInterval();
        PointOnCurve3d poc = new(GeCurve3d, inter.UpperBound);
        return -poc.GetDerivative(1);
    }

    /// <summary>
    /// 判断节点位置
    /// </summary>
    /// <param name="edge">边界</param>
    /// <param name="startOrEndIndex">边界是否位于此处</param>
    /// <param name="vec"></param>
    /// <param name="forward"></param>
    /// <returns></returns>
    public bool IsNext(Edge edge, int startOrEndIndex, ref Vector3d vec, ref bool forward)
    {
        if (edge == this)
            return false;

        if (StartIndex == startOrEndIndex)
        {
            vec = GetStartVector();
            forward = true;
            return true;
        }
        else if (EndIndex == startOrEndIndex)
        {
            vec = GetEndVector();
            forward = false;
            return true;
        }
        return false;
    }
    #endregion

    #region 重载运算符_比较
    public override bool Equals(object? obj)
    {
        return this == obj as Edge;
    }
    public bool Equals(Edge? b)
    {
        return this == b;
    }
    public static bool operator !=(Edge? a, Edge? b)
    {
        return !(a == b);
    }
    public static bool operator ==(Edge? a, Edge? b)
    {
        //此处地方不允许使用==null,因为此处是定义
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))//同一对象
            return true;

        return a.GeCurve3d == b.GeCurve3d
            && a.StartIndex == b.StartIndex
            && a.EndIndex == b.EndIndex;
    }

    /// <summary>
    /// 采样点比较(会排序,无视顺序逆序)
    /// </summary>
    /// <param name="b"></param>
    /// <param name="splitNum">切割曲线份数</param>
    /// <returns></returns>
    public bool SplitPointEquals(Edge? b, int splitNum = 4)
    {
        if (b is null)
            return this is null;
        else if (this is null)
            return false;
        if (ReferenceEquals(this, b))//同一对象
            return true;

        //这里获取曲线长度会经过很多次平方根,
        //所以不要这样做,直接采样点之后判断就完事

        //曲线采样分割点也一样,才是一样
        Point3d[] sp1;
        Point3d[] sp2;

#if NET35
        sp1 = GeCurve3d.GetSamplePoints(splitNum);
        sp2 = b.GeCurve3d.GetSamplePoints(splitNum);
        if (sp1.Length != sp2.Length)
            return false;
#else
        var tmp1 = GeCurve3d.GetSamplePoints(splitNum);
        var tmp2 = b.GeCurve3d.GetSamplePoints(splitNum);
        if (tmp1.Length != tmp2.Length)
            return false;

        sp1 = tmp1.Select(a => a.Point).ToArray();
        sp2 = tmp2.Select(a => a.Point).ToArray();
#endif

        //因为两条曲线可能是逆向的,所以采样之后需要点排序
        sp1 = sp1.OrderBy(a => a.X).ThenBy(a => a.Y).ThenBy(a => a.Z).ToArray();
        sp2 = sp2.OrderBy(a => a.X).ThenBy(a => a.Y).ThenBy(a => a.Z).ToArray();

        for (int i = 0; i < sp1.Length; i++)
        {
            if (!sp1[i].IsEqualTo(sp2[i], CadTolerance))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 消重顺序逆序
    /// </summary>
    /// <param name="edgesOut"></param>
    public static void Distinct(List<Edge> edgesOut)
    {
        if (edgesOut.Count == 0)
            return;

        //Edge没有包围盒,无法快速判断
        //曲线a和其他曲线n根据交点切割子线,会造成重复部分,例如多段线逆向相同
        for (int i = edgesOut.Count - 1; i >= 0; i--)
        {
            var aa = edgesOut[i];
            for (int j = i - 1; j >= 0; j--)
            {
                var bb = edgesOut[j];
                //顺序 || 逆序
                if ((aa.GeCurve3d.StartPoint.IsEqualTo(bb.GeCurve3d.StartPoint, CadTolerance) &&
                     aa.GeCurve3d.EndPoint.IsEqualTo(bb.GeCurve3d.EndPoint, CadTolerance))
                    ||
                    (aa.GeCurve3d.StartPoint.IsEqualTo(bb.GeCurve3d.EndPoint, CadTolerance) &&
                     aa.GeCurve3d.EndPoint.IsEqualTo(bb.GeCurve3d.StartPoint, CadTolerance)))
                {
                    if (aa.SplitPointEquals(bb, 5))
                        edgesOut.RemoveAt(i);
                }
            }
        }
    }

    //    private class EdgeComparer : IEqualityComparer<Edge>
    //    {
    //        public bool Equals(Edge x, Edge y)
    //        {
    //#if ac2009
    //            var pts = x.Curve.GetSamplePoints(100);
    //            return pts.All(pt => y.Curve.IsOn(pt));
    //#elif ac2013 || ac2015
    //            var pts = x.Curve.GetSamplePoints(100);
    //            return pts.All(pt => y.Curve.IsOn(pt.Point));
    //#endif
    //            //return x.Curve.IsEqualTo(y.Curve);
    //        }

    //        // If Equals() returns true for a pair of objects
    //        // then GetHashCode() must return the same value for these objects.
    //        public int GetHashCode(Edge product)
    //        {
    //            return product.Curve.GetHashCode();
    //        }
    //    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion
}


public class EdgeItem : Edge, IEquatable<EdgeItem>
{
    #region 字段
    /// <summary>
    /// 用来判断搜索方向(向前还是向后)
    /// </summary>
    public bool Forward;
    #endregion

    #region 构造
    /// <summary>
    /// 边节点
    /// </summary>
    public EdgeItem(Edge edge, bool forward) : base(edge)
    {
        Forward = forward;
    }
    #endregion

    #region 方法
    public CompositeCurve3d? GetCurve()
    {
        var cc3d = GeCurve3d;
        if (Forward)
        {
            return cc3d;
        }
        else
        {
            //反向曲线参数
            cc3d = cc3d.Clone() as CompositeCurve3d;
            return cc3d?.GetReverseParameterCurve() as CompositeCurve3d;
        }
    }

    /// <summary>
    /// 查找面域
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="regions"></param>
    public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions)
    {
        var region = new LoopList<EdgeItem>();
        var edgeItem = this;
        region.Add(edgeItem);
        var edgeItem2 = this.GetNext(edges);
        if (edgeItem2 is null)
            return;

        bool hasList = false;

        for (int i = 0; i < regions.Count; i++)
        {
            var edgeList2 = regions[i];
            var node = edgeList2.GetNode(e => e.Equals(edgeItem));
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
            while (edgeItem2 is not null)
            {
                if (edgeItem2 == edgeItem)
                    break;
                region.Add(edgeItem2); //TODO 此处死循环,上一条语句判断失误,导致不停的将相同的值加入region
                edgeItem2 = edgeItem2.GetNext(edges);
            }
            if (edgeItem2 == edgeItem)
                regions.Add(region);
        }
    }

    /// <summary>
    /// 获取下一个
    /// </summary>
    /// <param name="edges"></param>
    /// <returns></returns>
    public EdgeItem? GetNext(List<Edge> edges)
    {
        Vector3d vec;
        int next;
        if (Forward)
        {
            vec = GetEndVector();
            next = EndIndex;
        }
        else
        {
            vec = GetStartVector();
            next = StartIndex;
        }

        EdgeItem? edgeItem = null;
        Vector3d vec2, vec3 = new();
        double angle = 0;
        bool hasNext = false;
        bool forward = false;
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (this.IsNext(edge, next, ref vec3, ref forward))
            {
                if (hasNext)
                {
                    var angle2 = vec.GetAngleTo(vec3, Vector3d.ZAxis);
                    if (angle2 < angle)
                    {
                        vec2 = vec3;
                        angle = angle2;
                        edgeItem = new EdgeItem(edge, forward);
                    }
                }
                else
                {
                    hasNext = true;
                    vec2 = vec3;
                    angle = vec.GetAngleTo(vec2, Vector3d.ZAxis);
                    edgeItem = new EdgeItem(edge, forward);
                }
            }
        }
        return edgeItem;
    }
    #endregion

    #region 类型转换
    public override string ToString()
    {
        return
            Forward ?
            string.Format("{0}-{1}", StartIndex, EndIndex) :
            string.Format("{0}-{1}", EndIndex, StartIndex);
    }
    #endregion

    #region 重载运算符_比较
    public override bool Equals(object? obj)
    {
        return this == obj as EdgeItem;
    }
    public bool Equals(EdgeItem? b)
    {
        return this == b;
    }
    public static bool operator !=(EdgeItem? a, EdgeItem? b)
    {
        return !(a == b);
    }
    public static bool operator ==(EdgeItem? a, EdgeItem? b)
    {
        //此处地方不允许使用==null,因为此处是定义
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))//同一对象
            return true;

        return a.Forward == b.Forward && (a == b as Edge);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion
}


public class CurveInfo : Rect
{
    /// <summary>
    /// 曲线图元(母线)
    /// </summary>
    public Curve Curve;
    /// <summary>
    /// 曲线参数
    /// </summary>
    public List<double> Paramss;

    Edge? _Edge;
    /// <summary>
    /// 边界
    /// </summary>
    public Edge Edge
    {
        get
        {
            if (_Edge == null)
                _Edge = new Edge(Curve.ToCompositeCurve3d()!);
            return _Edge;
        }
    }

    public CurveInfo(Curve curve)
    {
        Curve = curve;
        Paramss = new List<double>();

        //TODO 此处存在bug:射线之类的没有过滤
        var box = Curve.GeometricExtents;
        _X = box.MinPoint.X;
        _Y = box.MinPoint.Y;
        _Right = box.MaxPoint.X;
        _Top = box.MaxPoint.Y;
    }

    /// <summary>
    /// 分割曲线,返回子线
    /// </summary>
    /// <param name="pars1">曲线参数</param>
    /// <returns></returns>
    public List<Edge> Split(List<double> pars1)
    {
        var edges = new List<Edge>();
        var c3ds = Edge.GeCurve3d.GetSplitCurves(pars1);
        if (c3ds.Count > 0)
            edges.AddRange(c3ds.Select(c => new Edge(c)));
        return edges;
    }
}


public class Topo
{
    /// <summary>
    /// 用于切割的曲线集
    /// </summary>
    List<CurveInfo> _curves;

    /// <summary>
    /// 求交类(每次set自动重置,都会有个新的结果)
    /// </summary>
    static CurveCurveIntersector3d _cci3d = new();
    public static Tolerance CadTolerance = new(1e-6, 1e-6);

    public Topo(List<Curve> curves)
    {
        _curves = new();
        for (int i = 0; i < curves.Count; i++)
            _curves.Add(new CurveInfo(curves[i]));

        //TODO 这里需要补充 列扫碰撞检测(碰撞算法)
        //将包围盒碰撞的放入一个集合a,这个集合a又被_curves储存起来,
        //集合a再运行 GetEdgesAndnewCurves
    }

    /// <summary>
    /// 利用交点断分曲线和独立曲线
    /// </summary>
    /// <param name="edgesOut">边界(可能仍然存在自闭,因为样条曲线允许打个鱼形圈,尾巴又交叉在其他曲线)</param>
    /// <param name="closedCurvesOut">自闭的曲线</param>
    public void GetEdgesAndnewCurves(List<Edge> edgesOut, List<Curve> closedCurvesOut)
    {
        //此处是O(n²)
        //曲线a和其他曲线n根据 交点 切割子线(第一次是自交对比)
        for (int a = 0; a < _curves.Count; a++)
        {
            var curve1 = _curves[a];
            var gc1 = curve1.Edge.GeCurve3d;
            var pars1 = curve1.Paramss;

            for (int n = a; n < _curves.Count; n++)
            {
                var curve2 = _curves[n];

                //包围盒没有碰撞就直接结束
                if (!curve1.IntersectsWith(curve2))
                    continue;

                var gc2 = curve2.Edge.GeCurve3d;
                var pars2 = curve2.Paramss;

                _cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                //计算两条曲线的交点(多个),分别放入对应的交点参数集
                for (int k = 0; k < _cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = _cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]);//0是第一条曲线的交点参数
                    pars2.Add(pars[1]);//1是第二条曲线的交点参数
                }
            }

            if (gc1.IsClosed())
                closedCurvesOut.Add(gc1.ToCurve()!);

            if (pars1.Count == 0)
                continue;

            //有交点参数
            //根据交点参数断分曲线,然后获取边界
            var c3ds = curve1.Split(pars1);
            if (c3ds.Count > 0)
            {
                edgesOut.AddRange(c3ds);
            }
            else
            {
                //惊惊留:(不敢删啊...)
                //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                //是这些参数?{参数0位置?头参/尾参/参数不在曲线上?}
                edgesOut.Add(curve1.Edge);
            }

            //有交点的才消重,无交点必然不重复.
            Edge.Distinct(edgesOut);
        }
    }

    /// <summary>
    /// 创建邻接表
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="closedCurvesOut"></param>
    public void AdjacencyList(List<Edge> edges, List<Curve> closedCurvesOut)
    {
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
            if (edge.GeCurve3d.IsClosed())
            {
                closedEdges.Add(edge);
                continue;
            }

            if (knots.Contains(edge.GeCurve3d.StartPoint))
            {
                //含有就是其他曲线"共用"此交点,
                //节点所在索引==共用计数索引=>将它++
                edge.StartIndex = knots.IndexOf(edge.GeCurve3d.StartPoint);
                nums[edge.StartIndex]++;
            }
            else
            {
                //不含有就加入节点,共用计数也加入,边界设置节点索引
                knots.Add(edge.GeCurve3d.StartPoint);
                nums.Add(1);
                edge.StartIndex = knots.Count - 1;
            }

            if (knots.Contains(edge.GeCurve3d.EndPoint))
            {
                edge.EndIndex = knots.IndexOf(edge.GeCurve3d.EndPoint);
                nums[edge.EndIndex]++;
            }
            else
            {
                knots.Add(edge.GeCurve3d.EndPoint);
                nums.Add(1);
                edge.EndIndex = knots.Count - 1;
            }
        }

        closedCurvesOut.AddRange(closedEdges.Select(e => e.GeCurve3d.ToCurve())!);

        //这里把交点只有一条曲线通过的点过滤掉了,也就是尾巴的图元,
        //剩下的都是闭合的曲线连接了,每个点都至少有两条曲线通过
        var tmp = edges
                .Except(closedEdges)
                .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1);
        edges.Clear();
        var ge = tmp.GetEnumerator();
        while (ge.MoveNext())
        {
            edges.Add(ge.Current);
        }

        //这一大坨不用看了 注释掉也没影响貌似,而且后续也没有用 nums
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
    }

    /// <summary>
    /// 获取多个面域
    /// </summary>
    /// <param name="edges"></param>
    /// <returns></returns>
    public List<LoopList<EdgeItem>> GetRegions(List<Edge> edges)
    {
        //利用边界的顺序和逆序获取闭合链条
        var regions = new List<LoopList<EdgeItem>>();
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            //TODO 这里有bug,两个内接的矩形会卡死
            var edgeItem = new EdgeItem(edge, true);
            edgeItem.FindRegion(edges, regions); // 经测试是这里卡住了 testTopo
            edgeItem = new EdgeItem(edge, false);
            edgeItem.FindRegion(edges, regions);
        }
        return regions;
    }

    /// <summary>
    /// 广度优先算法?
    /// </summary>
    /// <param name="regions"></param>
    public void RegionsInfo(List<LoopList<EdgeItem>> regions)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            for (int j = i + 1; j < regions.Count;)
            {
                bool eq = false;
                if (regions[i].Count == regions[j].Count)
                {
                    var node = regions[i].First;
                    var curve = node!.Value.GeCurve3d;
                    var node2 = regions[j].GetNode(e => e.GeCurve3d == curve);
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
                            if (node!.Value.GeCurve3d != node2!.Value.GeCurve3d)
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
    }
}