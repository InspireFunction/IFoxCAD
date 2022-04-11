namespace IFoxCAD.Cad;

/// <summary>
/// 边
/// </summary>
public class Edge : IEquatable<Edge>
{
    #region 字段
    public CompositeCurve3d GeCurve3d;
    public int StartIndex;//DESIGN0409 这个东西只是用在邻接表过滤就没用了.
    public int EndIndex;  //DESIGN0409 这个东西只是用在邻接表过滤就没用了.
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
    /// <summary>
    /// 开始向量
    /// </summary>
    /// <returns></returns>
    public Vector3d GetStartVector()
    {
        //获取曲线参数
        var inter = GeCurve3d.GetInterval();
        var poc = new PointOnCurve3d(GeCurve3d, inter.LowerBound);
        //方向导数
        return poc.GetDerivative(1);
    }

    /// <summary>
    /// 结束向量
    /// </summary>
    /// <returns></returns>
    public Vector3d GetEndVector()
    {
        var inter = GeCurve3d.GetInterval();
        var poc = new PointOnCurve3d(GeCurve3d, inter.UpperBound);
        return -poc.GetDerivative(1);
    }

    /// <summary>
    /// 判断节点位置
    /// </summary>
    /// <param name="edge">边界</param>
    /// <param name="startOrEndIndex">边界是否位于此处</param>
    /// <param name="vec">方向向量</param>
    /// <param name="forward">搜索方向</param>
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
    /// <param name="forward">搜索方向标志,<see langword="true"/>为向前搜索,<see langword="false"/>为向后搜索</param>
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
            return cc3d;

        //反向曲线参数
        cc3d = cc3d.Clone() as CompositeCurve3d;
        return cc3d?.GetReverseParameterCurve() as CompositeCurve3d;
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
            if (getEdgeItem == edgeItem)
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
        return Forward ?
               $"{StartIndex}-{EndIndex}" :
               $"{EndIndex}-{StartIndex}";
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

/// <summary>
/// 碰撞链
/// </summary>
public class CollisionChain : List<CurveInfo>
{
}


/// <summary>
/// 储存多段线(不闭合/无分叉/不自交)
/// </summary>
public class PolyEdge : LoopList<Edge>
{
    public PolyEdge(params Edge[] ps)
    {
        for (int i = 0; i < ps.Length; i++)
            Add(ps[i]);
    }

    public PolyEdge(Knot kn)
    {
        AddRange(kn);
    }


    /// <summary>
    /// 含有子段返回多段线
    /// </summary>
    /// <param name="lst">多段线集合(纯化后,提供给深度遍历)</param>
    /// <param name="find">查找的子段</param>
    /// <returns>返回多段线</returns>
    public static PolyEdge? Contains(IEnumerable<PolyEdge> lst, Edge find)
    {
        //不太晓得这里效率
        //Parallel.ForEach(this, item => {
        //    Parallel.ForEach(item, item2 => {
        //        if (item2 == find)
        //            return;
        //    });
        //});

        //多段线集合=>多段线=>子段
        var ge1 = lst.GetEnumerator();
        while (ge1.MoveNext())
        {
            var ge2 = ge1.Current.GetEnumerator();
            while (ge2.MoveNext())
            {
                //子段相同:返回多段线,所在计数
                if (ge2.Current == find)
                    return ge1.Current;
            }
        }
        return null;
    }
}


/// <summary>
/// 一个共点储存分支边
/// </summary>
public class Knot : PolyEdge//共点的边集合(交点计数就是边数)
{
    public Point3d Point; //交点

    public Knot(Point3d point, Edge edge)
    {
        Point = point;
        Add(edge);
    }

    /// <summary>
    /// 含有就返回成员
    /// </summary>
    /// <param name="knots">节点集合</param>
    /// <param name="findPoint">查找点</param>
    /// <returns></returns>
    public static Knot? Contains(IEnumerable<Knot> knots, Point3d findPoint)
    {
        var ge = knots.GetEnumerator();
        while (ge.MoveNext())
            if (ge.Current.Point == findPoint)
                return ge.Current;
        return null;
    }
}