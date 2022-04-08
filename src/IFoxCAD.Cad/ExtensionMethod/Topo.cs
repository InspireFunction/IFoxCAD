namespace IFoxCAD.Cad;

/// <summary>
/// 边
/// </summary>
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
    /// <param name="splitNum">
    /// 切割曲线份数(采样3点令样条和圆弧重叠发生失效,因此从4开始)
    ///  <a href="..\..\..\docs\Topo命令说明\曲线采样点数说明.png">图例说明在将本工程docs内</a>
    /// </param>
    /// <returns>采样点重叠true</returns>
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
#else
        var tmp1 = GeCurve3d.GetSamplePoints(splitNum);
        var tmp2 = b.GeCurve3d.GetSamplePoints(splitNum);
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
        Basal.ArrayEx.Deduplication(edgesOut, (first, last) => {
            var pta1 = first.GeCurve3d.StartPoint;
            var pta2 = first.GeCurve3d.EndPoint;
            var ptb1 = last.GeCurve3d.StartPoint;
            var ptb2 = last.GeCurve3d.EndPoint;
            //顺序 || 逆序
            if ((pta1.IsEqualTo(ptb1, CadTolerance) && pta2.IsEqualTo(ptb2, CadTolerance))
                ||
                (pta1.IsEqualTo(ptb2, CadTolerance) && pta2.IsEqualTo(ptb1, CadTolerance)))
            {
                return first.SplitPointEquals(last);
            }
            return false;
        });
    }
    public override int GetHashCode()
    {
        return GeCurve3d.GetHashCode() ^ StartIndex ^ EndIndex;
    }
    #endregion
}

/// <summary>
/// 边节点
/// </summary>
public class EdgeItem : Edge, IEquatable<EdgeItem>
{
    #region 字段
    /// <summary>
    /// <param name="forward">搜索方向标志，<see langword="true"/>为向前搜索，<see langword="false"/>为向后搜索</param>
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
    /// <param name="edges">用来查询位置的</param>
    /// <param name="regions_out">返回的面域</param>
    public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions_out)
    {
        var result = new LoopList<EdgeItem>();//新的面域
        var edgeItem = this;
        result.Add(edgeItem);
        var getEdgeItem = this.GetNext(edges);
        if (getEdgeItem is null)
            return;

        bool hasList = false;

        for (int i = 0; i < regions_out.Count; i++)
        {
            var edgeList2 = regions_out[i];
            var node = edgeList2.GetNode(e => e.Equals(edgeItem));
            if (node is not null && node != edgeList2.Last)
            {
                if (node.Next!.Value.Equals(getEdgeItem))
                {
                    hasList = true;
                    break;
                }
            }
        }
        if (!hasList)
        {
            //之前这里有个死循环,造成的原因是8字走b了(腰身闭环),然后下面闭环永远找不到开头
            while (getEdgeItem is not null)
            {
                if (result.Contains(getEdgeItem))
                {
                    //腰身闭环,从前头开始剔除到这个重复节点
                    while (result.First?.Value != getEdgeItem)
                        result.RemoveFirst();
                    break;
                }
                result.Add(getEdgeItem);
                getEdgeItem = getEdgeItem.GetNext(edges);
            }
            if (getEdgeItem == edgeItem)//TODO 这里必须存在可以环形排序方式,令它不会重复加入一样的链条.
                regions_out.Add(result);
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
        return base.GetHashCode() ^ (Forward ? 0 : 1);
    }
    #endregion
}

/// <summary>
/// 曲线信息
/// </summary>
public class CurveInfo : Rect
{
    /// <summary>
    /// 曲线图元
    /// </summary>
    public Curve Curve;
    /// <summary>
    /// 曲线分割点的参数
    /// </summary>
    public List<double> Paramss;
    /// <summary>
    /// 碰撞链
    /// </summary>
    public CollisionChain? CollisionChain;

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
    /// <param name="pars1">曲线分割点的参数</param>
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

public class CollisionChain
{
    /// <summary>
    /// 碰撞链
    /// </summary>
    public List<CurveInfo> Collision;

    public CollisionChain()
    {
        Collision = new List<CurveInfo>();
    }
}

public class Topo
{
    // 碰撞链集合
    List<CollisionChain> _CollisionChains;
    // 求交类(每次set自动重置,都会有个新的结果)
    static CurveCurveIntersector3d _cci3d = new();
    // cad容差类
    public static Tolerance CadTolerance = new(1e-6, 1e-6);


    public Topo(List<Curve> curves)
    {
        List<CurveInfo> curveList = new();

        //提取包围盒信息
        for (int i = 0; i < curves.Count; i++)
            curveList.Add(new CurveInfo(curves[i]));

        //碰撞检测+消重
        _CollisionChains = new();
        CollisionChain? tmp = null;

        Rect.XCollision(curveList,
            oneRect => {
                tmp = oneRect.CollisionChain;//有碰撞链就直接利用之前的链
                return false;
            },
            (oneRect, twoRect) => {
                //消重:包围盒大小一样+首尾相同+采样点相同
                if (oneRect.Equals(twoRect, 1e-6))
                {
                    var pta1 = oneRect.Edge.GeCurve3d.StartPoint;
                    var pta2 = oneRect.Edge.GeCurve3d.EndPoint;
                    var ptb1 = twoRect.Edge.GeCurve3d.StartPoint;
                    var ptb2 = twoRect.Edge.GeCurve3d.EndPoint;

                    if ((pta1.IsEqualTo(ptb1, CadTolerance) && pta2.IsEqualTo(ptb2, CadTolerance))
                        ||
                        (pta1.IsEqualTo(ptb2, CadTolerance) && pta2.IsEqualTo(ptb1, CadTolerance)))
                        if (oneRect.Edge.SplitPointEquals(twoRect.Edge))
                            return true;//跳过后续步骤
                }

                if (tmp == null)
                {
                    tmp = new();
                    oneRect.CollisionChain = tmp;//碰撞链设置
                    tmp.Collision.Add(oneRect);//本体也加入链
                }
                twoRect.CollisionChain = tmp;//碰撞链设置
                tmp.Collision.Add(twoRect);
                return false;
            },
            oneRect => {
                if (tmp != null && !_CollisionChains.Contains(tmp))
                {
                    _CollisionChains.Add(tmp);
                    tmp = null;
                }
            });
    }


    /// <summary>
    /// 遍历多条碰撞链
    /// </summary>
    /// <param name="action"></param>
    public void CollisionFor(Action<List<CurveInfo>> action)
    {
        _CollisionChains.ForEach(a => {
            action(a.Collision);//输出每条碰撞链
        });
    }

    /// <summary>
    /// 利用交点断分曲线和独立自闭曲线
    /// </summary>
    /// <param name="infos_In">传入每组有碰撞的</param>
    /// <param name="edges_Out">传出不自闭的曲线集</param>
    /// <param name="closedCurves_Out">传出自闭曲线集</param>
    public void GetEdgesAndnewCurves(List<CurveInfo> infos_In, List<Edge> edges_Out, List<CompositeCurve3d> closedCurves_Out)
    {
        //此处是O(n²)
        //曲线a和其他曲线n根据 交点 切割子线(第一次是自交对比)
        for (int a = 0; a < infos_In.Count; a++)
        {
            var curve1 = infos_In[a];
            var gc1 = curve1.Edge.GeCurve3d;
            var pars1 = curve1.Paramss;

            for (int n = a; n < infos_In.Count; n++)
            {
                var curve2 = infos_In[n];

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
                closedCurves_Out.Add(gc1);

            if (pars1.Count == 0)
                continue;

            //有交点参数
            //根据交点参数断分曲线,然后获取边界
            var c3ds = curve1.Split(pars1);
            if (c3ds.Count > 0)
            {
                edges_Out.AddRange(c3ds);
            }
            else
            {
                //惊惊留:(不敢删啊...)
                //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                //是这些参数?{参数0位置?头参/尾参/参数不在曲线上?}
                edges_Out.Add(curve1.Edge);
            }

            //有交点的才消重,无交点必然不重复.
            Edge.Distinct(edges_Out);
        }
    }

    /// <summary>
    /// 创建邻接表
    /// </summary>
    /// <param name="edges_InOut">传入每组有碰撞的;传出闭合边界集(扔掉单交点的)</param>
    /// <param name="closedCurve3d_Out">传出自闭曲线集</param>
    public void AdjacencyList(List<Edge> edges_InOut, List<CompositeCurve3d> closedCurve3d_Out)
    {
        //构建边的邻接表
        //下面是键值对(基于ArrayOfStruct思想,拆开更合适内存布局)
        //knots 是不重复地将所有交点设置为节点(如果是重复就对应 nums++)
        //nums  是记录每个交点被重复了几次
        var knots = new List<Point3d>();
        var nums = new List<int>();
        var closedEdges = new List<Edge>();

        //交点集合 knots 会不断增大,会变得更加慢,因此我在一开始就进行碰撞链分析
        for (int i = 0; i < edges_InOut.Count; i++)
        {
            var edge = edges_InOut[i];
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

        closedCurve3d_Out.AddRange(closedEdges.Select(e => e.GeCurve3d));

        //这里把交点只有一条曲线通过的点过滤掉了,也就是尾巴的图元,
        //剩下的都是闭合的曲线连接了,每个点都至少有两条曲线通过
        var tmp = edges_InOut
                .Except(closedEdges)
                .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1)
                .ToArray();//要ToArray克隆,否则下面会清空掉这个容器

        edges_InOut.Clear();
        for (int i = 0; i < tmp.Length; i++)
            edges_InOut.Add(tmp[i]);
    }

    /// <summary>
    /// 获取多个面域
    /// </summary>
    /// <param name="edges">剩下都有两个交点的线</param>
    /// <returns></returns>
    public List<LoopList<EdgeItem>> GetRegions(List<Edge> edges)
    {
        var regions_out = new List<LoopList<EdgeItem>>();

        /* 
         * TODO 这里暴力算法需要优化
         * 狐哥为了处理左拐还是右拐图形bd,用了所有的线做种子线,然后往前后推进
         * 前后推进:利用边界的顺序和逆序获取闭合链条,
         *         会造成一环就有: 123,231,312,321,213,132,再其后再判断他们重复
         *         
         * 优化方案一:邻接表按引用次数排序之后,上面的交点如果只有两个图元,组成链,然后移除出邻接表.
         *           这样我只有遇到多个共点时候,那就只有:多个链头尾(减少了腰身)和邻接表的
         *           实现递减.
         */
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            var edgeItem = new EdgeItem(edge, true);
            edgeItem.FindRegion(edges, regions_out);
            edgeItem = new EdgeItem(edge, false);
            edgeItem.FindRegion(edges, regions_out);
        }

        DeduplicationRegions(regions_out);
        return regions_out;
    }

    /// <summary>
    /// 移除相同面域
    /// </summary>
    /// <param name="regions"></param>
    void DeduplicationRegions(List<LoopList<EdgeItem>> regions)
    {
        Basal.ArrayEx.Deduplication(regions, (first, last) => {
            bool eq = false;//是否存在相同成员
            if (first.Count == last.Count)
            {
                var node1 = first.First;
                var curve1 = node1!.Value.GeCurve3d;

                //两个面域对比,找到相同成员
                var node2 = last.GetNode(e => e.GeCurve3d == curve1);
                if (node2 is not null)
                {
                    eq = true;
                    var f1 = node1.Value.Forward;
                    var f2 = node2.Value.Forward;
                    //链条搜索方向来进行
                    //判断每个节点的成员如果一致就会执行移除
                    for (int k = 1; k < first.Count; k++)
                    {
                        node1 = node1.GetNext(f1);
                        node2 = node2.GetNext(f2);
                        if (node1!.Value.GeCurve3d != node2!.Value.GeCurve3d)
                        {
                            eq = false;
                            break;
                        }
                    }
                }
            }
            return eq;
        });
    }
}