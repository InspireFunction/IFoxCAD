using Exception = System.Exception;
namespace IFoxCAD.Cad;


/// <summary>
/// 无权无向图实现
/// IEnumerable 枚举所有顶点;
/// </summary>
internal sealed class Graph : IGraph, IEnumerable<IGraphVertex>
{
    #region 字段及属性
    /// <summary>
    /// 存储所有节点的字典,key为顶点的类型,value为邻接表,类型是hashset,不可重复添加点
    /// </summary>
    /// <value></value>
    readonly Dictionary<IGraphVertex, HashSet<IGraphVertex>> vertices = new();
    /// <summary>
    /// 邻接边表,key为顶点的类型,value为邻接边表,类型是hashset,不可重复添加边
    /// </summary>
    readonly Dictionary<IGraphVertex, HashSet<IEdge>> edges = new();
    /// <summary>
    /// 为加快索引,引入hash检索
    /// </summary>
    readonly Dictionary<string, IGraphVertex> vertexs = new();

    public int VerticesCount => vertices.Count;
    /// <summary>
    /// 目前点增加点的顺序号,这个点号不随删点而减少的
    /// </summary>
    static int insertCount;
    #endregion

    #region 构造函数
    public Graph()
    {
        insertCount = 0; // 每次新建对象就将顶点顺序号归零
    }
    #endregion

    #region 顶点及边_增
    /// <summary>
    /// 向该图添加一个新顶点,但是无边;
    /// </summary>
    /// <param name="pt">点</param>
    /// <returns>创建的顶点</returns>
    public IGraphVertex AddVertex(Point3d pt)
    {
        var str = Graph.GetHashString(pt);
        if (vertexs.ContainsKey(str))
            return vertexs[str];

        var vertex = new GraphVertex(pt, insertCount++);
        vertices.Add(vertex, new HashSet<IGraphVertex>());
        edges.Add(vertex, new HashSet<IEdge>());

        vertexs[str] = vertex;

        return vertex;
    }

    /// <summary>
    /// 向该图添加一个边;
    /// </summary>
    /// <param name="curve"></param>
    public void AddEdge(Curve3d curve!!)
    {
        var start = AddVertex(curve.StartPoint);
        var end = AddVertex(curve.EndPoint);

        // 添加起点的邻接表和邻接边
        vertices[start].Add(end);
        edges[start].Add(new GraphEdge(end, curve));

        // 为了保证点顺序,每个点的邻接边必须按起点-终点,所以添加曲线终点时,将添加一个方向的曲线
        var curtmp = (Curve3d)curve.Clone();
        curtmp = curtmp.GetReverseParameterCurve();

        // 添加终点的邻接表和邻接边
        vertices[end].Add(start);
        edges[end].Add(new GraphEdge(start, curtmp));
    }
    #endregion

    #region 顶点及边_删
    /// <summary>
    /// 从此图中删除现有顶点;
    /// </summary>
    /// <param name="pt">点</param>
    public void RemoveVertex(Point3d pt)
    {
        var str = Graph.GetHashString(pt);
        if (vertexs.ContainsKey(str))
        {
            var vertex = vertexs[str];

            // 删除邻接表里的vertex点,先删除后面的遍历可以少一轮
            vertices.Remove(vertex!);

            // 删除其他顶点的邻接表里的vertex点
            foreach (var item in vertices.Values)
                item.Remove(vertex!);

            // 删除邻接边表里的vertex点,先删除后面的遍历可以少一轮
            edges.Remove(vertex!);

            // 删除其他顶点的邻接边表的指向vertex的边
            foreach (var item in edges.Values)
            {
                foreach (var edge in item)
                {
                    if (vertex.Equals(edge.TargetVertex))
                        item.Remove(edge);
                }
            }
            vertexs.Remove(str);
        }
    }

    /// <summary>
    /// 从此图中删除一条边;
    /// </summary>
    /// <param name="curve">曲线</param>
    public void RemoveEdge(Curve3d curve!!)
    {
        RemoveVertex(curve.StartPoint);
        RemoveVertex(curve.EndPoint);
    }
    #endregion

    #region 顶点和边_查
    /// <summary>
    /// 我们在给定的来源和目的地之间是否有边？
    /// </summary>
    /// <param name="source">起点</param>
    /// <param name="dest">终点</param>
    /// <returns>有边返回 <see langword="true"/>,反之返回 <see langword="false"/></returns>
    public bool HasEdge(IGraphVertex source, IGraphVertex dest)
    {
        if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            throw new ArgumentException("源或目标不在此图中;");

        foreach (var item in edges[source])
        {
            if (item.TargetVertex == dest)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取边
    /// </summary>
    /// <param name="source">起点</param>
    /// <param name="dest">终点</param>
    /// <returns>边</returns>
    /// <exception cref="ArgumentException">传入的点不在图中时抛出参数异常</exception>
    public IEdge? GetEdge(IGraphVertex source, IGraphVertex dest)
    {
        if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            throw new ArgumentException("源或目标不在此图中;");

        foreach (var item in edges[source])
        {
            if (item.TargetVertex == dest)
                return item;
        }
        return null;
    }

    /// <summary>
    /// 是否存在顶点,此函数目前未发现有啥用
    /// </summary>
    /// <param name="value">顶点</param>
    /// <returns>存在顶点返回 <see langword="true"/>,反之返回 <see langword="false"/></returns>
    public bool ContainsVertex(IGraphVertex value)
    {
        return vertices.ContainsKey(value);
    }
    #endregion

    #region 获取邻接表和曲线
    /// <summary>
    /// 获取顶点的邻接表
    /// </summary>
    /// <param name="vertex">顶点</param>
    /// <returns>邻接表</returns>
    public HashSet<IGraphVertex> GetAdjacencyList(IGraphVertex vertex)
    {
        return vertices[vertex];
    }

    /// <summary>
    /// 获取顶点的邻接边表
    /// </summary>
    /// <param name="vertex">顶点</param>
    /// <returns>邻接边表</returns>
    public HashSet<IEdge> GetAdjacencyEdge(IGraphVertex vertex)
    {
        return edges[vertex];
    }

    /// <summary>
    /// 根据顶点表获取曲线集合
    /// </summary>
    /// <param name="graphVertices">顶点表</param>
    /// <returns>曲线表</returns>
    public List<Curve3d> GetCurves(List<IGraphVertex> graphVertices)
    {
        var curves = new List<Curve3d>();
        for (int i = 0; i < graphVertices.Count - 1; i++)
        {
            var cur = graphVertices[i];
            var next = graphVertices[i + 1];
            var edge = GetEdge(cur, next);
            if (edge is not null)
                curves.Add(edge.TargetEdge);
        }
        var lastedge = GetEdge(graphVertices[graphVertices.Count - 1], graphVertices[0]);
        if (lastedge is not null)
            curves.Add(lastedge.TargetEdge);

        return curves;
    }
    #endregion

    #region 克隆及接口实现
    /// <summary>
    /// 克隆此图;目测是深克隆
    /// </summary>
    public Graph Clone()
    {
        var newGraph = new Graph();

        foreach (var vertex in edges.Values)
            foreach (var item in vertex)
                newGraph.AddEdge(item.TargetEdge);

        return newGraph;
    }

    IGraph IGraph.Clone()
    {
        return Clone();
    }

    public IEnumerator GetEnumerator()
    {
        return VerticesAsEnumberable.GetEnumerator();
    }

    IEnumerator<IGraphVertex>? IEnumerable<IGraphVertex>.GetEnumerator()
    {
        return GetEnumerator() as IEnumerator<IGraphVertex>;
    }

    public IEnumerable<IGraphVertex> VerticesAsEnumberable =>
        vertices.Select(x => x.Key);
    #endregion

    #region 方法
    static string GetHashString(Point3d pt)
    {
        return $"{pt.X:n6}{pt.Y:n6}{pt.Z:n6}";
    }

    /// <summary>
    /// 输出点的邻接表的可读字符串
    /// </summary>
    /// <returns></returns>
    public string ToReadable()
    {
        int i = 1;
        string output = string.Empty;
        foreach (var node in vertices)
        {
            var adjacents = string.Empty;

            output = string.Format("{1}\r\n{0}-{2}: [", i, output, node.Key.Data.ToString());

            foreach (var adjacentNode in node.Value)
                adjacents = string.Format("{0}{1},", adjacents, adjacentNode.Data.ToString());

            if (adjacents.Length > 0)
                adjacents = adjacents.TrimEnd(new char[] { ',', ' ' });

            output = string.Format("{0}{1}]", output, adjacents);
            i++;
        }
        return output;
    } 
    #endregion
}


/// <summary>
/// 邻接表图实现的顶点;
/// IEnumerable 枚举所有邻接点;
/// </summary>
internal sealed class GraphVertex : IGraphVertex, IEquatable<IGraphVertex>, IComparable, IComparable<IGraphVertex>
{
    #region 属性
    public Point3d Data { get; set; }
    public int Index { get; set; }
    #endregion

    #region 构造
    /// <summary>
    /// 邻接表图实现的顶点
    /// </summary>
    /// <param name="value">点</param>
    /// <param name="index">所在节点索引</param>
    public GraphVertex(Point3d value, int index)
    {
        Data = value;
        Index = index;
    }
    #endregion

    #region 重载运算符_比较
    public bool Equals(IGraphVertex other)
    {
        return Index == other.Index;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (obj is not IGraphVertex vertex)
            return false;
        else
            return Equals(vertex);
    }

    public override int GetHashCode()
    {
        return Index;
    }

    public int CompareTo(IGraphVertex other)
    {
        if (Equals(other))
            return 0;

        if (Index < other.Index)
            return -1;
        else
            return 1;
    }

    int IComparable<IGraphVertex>.CompareTo(IGraphVertex other)
    {
        return CompareTo(other);
    }

    public int CompareTo(object obj)
    {
        if (obj is null)
            return 1;

        try
        {
            var other = (GraphVertex)obj;
            return CompareTo(other);
        }
        catch (Exception)
        {
            throw new ArgumentException("Object is not a IGraphVertex");
        }
    }

    public static bool operator ==(GraphVertex person1, GraphVertex person2)
    {
        if (person1 is null || person2 is null)
            return Equals(person1, person2);

        return person1.Equals(person2);
    }

    public static bool operator !=(GraphVertex person1, GraphVertex person2)
    {
        if (person1 is null || person2 is null)
            return !Equals(person1, person2);

        return !person1.Equals(person2);
    }
    #endregion
}


/// <summary>
/// 无向图中边的定义
/// </summary>
internal sealed class GraphEdge : IEdge, IEquatable<GraphEdge>
{
    #region 属性
    public IGraphVertex TargetVertex { get; set; }
    public Curve3d TargetEdge { get; set; }
    #endregion

    #region 构造
    /// <summary>
    /// 无向图中边的定义 
    /// </summary>
    /// <param name="target">下一点</param>
    /// <param name="edge">下一点之间的曲线</param>
    public GraphEdge(IGraphVertex target, Curve3d edge)
    {
        TargetVertex = target;
        TargetEdge = edge;
    }
    #endregion

    #region 重载运算符_比较
    public bool Equals(GraphEdge other)
    {
        if (other is null)
            return false;
        return TargetVertex == other.TargetVertex &&
               TargetEdge == other.TargetEdge;
    }
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (obj is not GraphEdge personObj)
            return false;
        else
            return Equals(personObj);
    }

    public override int GetHashCode()
    {
        return (TargetVertex.GetHashCode(), TargetEdge.GetHashCode()).GetHashCode();
    }
    public static bool operator ==(GraphEdge person1, GraphEdge person2)
    {
        if (person1 is null || person2 is null)
            return Equals(person1, person2);

        return person1.Equals(person2);
    }
    public static bool operator !=(GraphEdge person1, GraphEdge person2)
    {
        if (person1 is null || person2 is null)
            return !Equals(person1, person2);

        return !person1.Equals(person2);
    }
    #endregion
}


/// <summary>
/// 深度优先搜索;
/// </summary>
internal sealed class DepthFirst
{
    #region 公共方法
    /// <summary>
    /// 存储所有的边
    /// </summary>
    public List<List<IGraphVertex>> Curve3ds { get; } = new();

    /// <summary>
    /// 找出所有的路径
    /// </summary>
    /// <param name="graph">图</param>
    public void FindAll(IGraph graph)
    {
        var ge = graph.VerticesAsEnumberable.GetEnumerator();
        while (ge.MoveNext())
            Dfs(graph, new List<IGraphVertex> { ge.Current });
    } 
    #endregion

    #region 内部方法
    /// <summary>
    /// 递归 DFS;
    /// </summary>
    /// <param name="graph">图</param>
    /// <param name="visited">已经遍历的路径</param>
    void Dfs(IGraph graph, List<IGraphVertex> visited)
    {
        var startNode = visited[0];
        IGraphVertex nextNode;
        List<IGraphVertex> sub;

        var adjlist = graph.GetAdjacencyList(startNode).ToList();
        for (int i = 0; i < adjlist.Count; i++)
        {
            nextNode = adjlist[i];
            // 如果下一个点未遍历过
            if (!visited.Contains(nextNode))
            {
                // 将下一点加入路径集合,并进行下一次递归
                sub = new List<IGraphVertex> { nextNode };
                sub.AddRange(visited);
                Dfs(graph, sub);
            }
            // 如果下一点遍历过,并且路径大于2,说明已经找到起点
            else if (visited.Count > 2 && nextNode.Equals(visited[^1]))
            {
                // 将重复的路径进行过滤,并把新的路径存入结果
                var cur = DepthFirst.RotateToSmallest(visited);
                var inv = DepthFirst.Invert(cur);
                if (IsNew(cur) && IsNew(inv))
                    Curve3ds.Add(cur);
            }
        }
    }

    /// <summary>
    /// 将列表旋转到最小的值为列表起点
    /// </summary>
    /// <param name="lst"></param>
    /// <returns></returns>
    static List<IGraphVertex> RotateToSmallest(List<IGraphVertex> lst)
    {
        var index = lst.IndexOf(lst.Min());
        return lst.Skip(index).Concat(lst.Take(index)).ToList();
    }

    /// <summary>
    /// 将列表反向,并旋转到起点为最小值
    /// </summary>
    /// <param name="lst"></param>
    /// <returns></returns>
    static List<IGraphVertex> Invert(List<IGraphVertex> lst)
    {
        var tmp = lst.ToList();
        tmp.Reverse();
        return RotateToSmallest(tmp);
    }

    /// <summary>
    /// 比较两个列表是否相等
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    static bool Equals(List<IGraphVertex> a, List<IGraphVertex> b)
    {
        bool ret = (a[0] == b[0]) && (a.Count == b.Count);

        for (int i = 1; ret && (i < a.Count); i++)
            if (!a[i].Equals(b[i]))
                ret = false;

        return ret;
    }

    /// <summary>
    /// 是否已经是存在闭合环
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool IsNew(List<IGraphVertex> path)
    {
        bool ret = true;
        for (int i = 0; i < Curve3ds.Count; i++)
        {
            if (DepthFirst.Equals(Curve3ds[i], path))
            {
                ret = false;
                break;
            }
        }
        return ret;
    } 
    #endregion
}