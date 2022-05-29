using System.Diagnostics;

namespace IFoxCAD.Cad;


/// <summary>
/// 曲线信息
/// </summary>
public class CurveInfo : Rect
{
    #region 属性
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
    #endregion

    #region 构造
    public CurveInfo(Curve curve!!)
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
    #endregion

    #region 方法
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
    #endregion
}

/// <summary>
/// 碰撞链
/// </summary>
public class CollisionChain : List<CurveInfo> { }



/// <summary>
/// 曲线边
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(Edge))]
public class Edge : IEquatable<Edge>, IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    #region 字段
    public CompositeCurve3d GeCurve3d;
    #endregion

    #region 构造
    /// <summary>
    /// 边线(没有包围盒,除非ToCurve)
    /// </summary>
    public Edge(CompositeCurve3d geCurve3d)
    {
        GeCurve3d = geCurve3d;
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

        return a.GeCurve3d == b.GeCurve3d;
    }

    public override int GetHashCode()
    {
        return GeCurve3d.GetHashCode();
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
    #endregion

    #region 方法
#pragma warning disable CA2211 // 非常量字段应当不可见
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // 非常量字段应当不可见

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
    #endregion

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// 转换为字符串_格式化实现
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// 转换为字符串_有参调用
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        if (format is null)
        {
            s.Append(nameof(Edge));
            s.Append("{ ");
            s.Append("(StartPoint=" + GeCurve3d.StartPoint + "; EndPoint=" + GeCurve3d.EndPoint + ")");
            s.Append(" }\r\n");
        }
        return s.ToString();
    }
    #endregion
}

#if true2
/// <summary>
/// 复核曲线(不闭合/无分叉/不自交)
/// </summary>
public class PolyEdge : LoopList<Edge>
{
    /// <summary>
    /// 复核曲线(不闭合/无分叉/不自交)
    /// </summary>
    /// <param name="ps"></param>
    public PolyEdge(params Edge[] ps)
    {
        AddRange(ps);
    }

    // 静态变量可以在高频加入点集的时候保留内存长度
    static List<Point3d> pts = new();

    public Point3d[] Points()
    {
        pts.Clear();

        //因为加入的时候子段是有序的,所以点序也是有序的
        var ge = GetEnumerator();
        while (ge.MoveNext())
        {
            var sp = ge.Current.GeCurve3d.StartPoint;
            if (pts.Count == 0)
                pts.Add(sp);
            else if (pts[pts.Count - 1] != sp)//跳过子段重合点
                pts.Add(sp);

            var ep = ge.Current.GeCurve3d.EndPoint;
            if (pts.Count == 0)
                pts.Add(ep);
            else if (pts[pts.Count - 1] != ep)
                pts.Add(ep);
        }
        return pts.ToArray();
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

#endif



/// <summary>
/// 邻接表节点
/// </summary>
//[DebuggerDisplay("节点 = {Number}; Count = {Count}; Color = {Color}; Distance = {Distance}; 父节点编号 = {Parent?.Number}")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(BoNode))]
public class BoNode : IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    #region 字段
    /// <summary>
    /// 颜色
    /// </summary>
    public BoColor Color;
    /// <summary>
    /// 从头到此步数
    /// </summary>
    public int Steps;
    /// <summary>
    /// 父节点
    /// </summary>
    public BoNode? Parent;
    /// <summary>
    /// 相遇点集合
    /// </summary>
    public List<BoNode>? Meet;
    /// <summary>
    /// 邻近节点
    /// </summary>
    public List<BoNode> Neighbor;


    /// <summary>
    /// 交点
    /// </summary>
    public Point3d Point;
    /// <summary>
    /// 交点的曲线(交点计数就是边数)
    /// </summary>
    public List<Edge> Edges;
    /// <summary>
    /// 节点编号
    /// </summary>
    public int Number;
    #endregion

    #region 构造
    /// <summary>
    /// 邻接表节点
    /// </summary>
    /// <param name="num">节点编号</param>
    /// <param name="point">交点</param>
    /// <param name="edge">边</param>
    public BoNode(int num, Point3d point, Edge edge)
    {
        Number = num;
        Point = point;
        Edges = new();
        Edges.Add(edge);

        Neighbor = new();


        Color = BoColor.白;
        Steps = int.MaxValue;
        Parent = null;
        Meet?.Clear();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 获取封闭边线集合,并排序线序
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns>线序排列,点序未排列</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static LoopList<Edge> GetEdges(LoopList<BoNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new ArgumentNullException(nameof(nodes));

        LoopList<Edge> result = new();
        //1-2 2-3 3-4 4-1
        var lp = nodes.First;
        do
        {
            var edge = GetEdge(lp!.Value, lp.Next!.Value);
            if (edge != null)
                result.Add(edge);
            lp = lp.Next;
        } while (lp != nodes.First);

        return result;
    }

    /// <summary>
    /// 获取a-b节点之间的线
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    static Edge? GetEdge(BoNode node1, BoNode node2)
    {
        if (node1 == null || node2 == null)
            return null;

        for (int i = 0; i < node1.Edges.Count; i++)
        {
            var node1Edge = node1.Edges[i];
            for (int j = 0; j < node2.Edges.Count; j++)
            {
                if (node1Edge == node2.Edges[j])
                    return node1Edge;
            }
        }
        return null;
    }
    #endregion

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// 转换为字符串_格式化实现
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// 转换为字符串_有参调用
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        //s.Append($"Count = {Count};");
        if (format is null)
        {
            s.Append(nameof(BoNode) + $"{Number}");
            s.Append("{ ");

            s.Append("邻点:(");
            for (int i = 0; i < Neighbor.Count; i++)
            {
                s.Append(this.Neighbor[i].Number);
                if (i < Neighbor.Count - 1)
                    s.Append("--");
            }
            s.Append(") ");

            s.Append($"Neighbor.Count={Neighbor.Count}; Color={Color}; Distance={Steps}; 父点编号={Parent?.Number}; ");
            s.Append($"Point={Point};");
            s.Append(" }\r\n");
        }
        return s.ToString();
    }
    #endregion

}

public enum BoColor
{
    白,
    灰,
    黑,
    红,
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(CompositeCurve3ds))]
public class CompositeCurve3ds : List<CompositeCurve3d>, IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

#pragma warning disable CA2211 // 非常量字段应当不可见
    // cad容差类
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // 非常量字段应当不可见

    /// <summary>
    /// 边界点序排列
    /// </summary>
    /// <param name="pl">需要线序排列好的</param>
    /// <returns></returns>
    public static CompositeCurve3ds OrderByPoints(LoopList<Edge> pl)
    {
        CompositeCurve3ds c3ds = new();
        var lp = pl.First;
        do
        {
            //第1个和第2个比较,反向曲线参数
            var a1 = lp!.Value.GeCurve3d;
            var a2 = lp!.Next!.Value.GeCurve3d;

            if (!a1.EndPoint.IsEqualTo(a2.EndPoint, CadTolerance) && //尾巴相同跳过
                !a1.EndPoint.IsEqualTo(a2.StartPoint, CadTolerance))
                a1 = (CompositeCurve3d)a1.GetReverseParameterCurve();

            c3ds.Add(a1);
            lp = lp.Next;
        } while (lp != pl.First);

        return c3ds;
    }


    public CompositeCurve3d ToCompositeCurve3d()
    {
        return new CompositeCurve3d(ToArray());
    }

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// 转换为字符串_格式化实现
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// 转换为字符串_有参调用
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        if (format is null)
        {
            s.Append(nameof(CompositeCurve3ds) + $"{Count}");
            s.Append("{ ");
            for (int i = 0; i < Count; i++)
            {
                s.Append("\r\n(StartPoint=" + this[i].StartPoint + "; EndPoint=" + this[i].EndPoint + ")");
            }
            s.Append(" }\r\n");
        }
        return s.ToString();
    }

    #endregion
}