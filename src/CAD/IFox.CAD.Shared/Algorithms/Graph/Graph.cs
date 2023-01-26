namespace IFoxCAD.Cad;
using Exception = System.Exception;

/// <summary>
/// 无权无向图实现
/// IEnumerable 枚举所有顶点;
/// </summary>
public sealed class Graph : IGraph, IEnumerable<IGraphVertex>
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
    /// <summary>
    /// 节点数量
    /// </summary>
    public int VerticesCount => vertices.Count;

    /// <summary>
    /// Returns a reference vertex.
    /// Time complexity: O(1).
    /// </summary>
    private IGraphVertex? ReferenceVertex
    {
        get
        {
            using (var enumerator = vertexs.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.Value;
                }
            }

            return null;
        }
    }
    IGraphVertex? IGraph.ReferenceVertex => ReferenceVertex;
    /// <summary>
    /// 目前点增加点的顺序号,这个点号不随删点而减少的
    /// </summary>
    private int insertCount;
    #endregion

    #region 构造函数
    /// <summary>
    /// 
    /// </summary>
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
        var str = pt.GetHashString();
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
    public void AddEdge(Curve3d curve)
    {
        if (curve == null)
            throw new ArgumentNullException(nameof(curve));

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
        var str = pt.GetHashString();
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
                item.RemoveWhere(x => vertex.Equals(x.TargetVertex));
                // foreach (var edge in item)
                // {
                //    if (vertex.Equals(edge.TargetVertex))
                //        item.Remove(edge);
                // }
            }
            vertexs.Remove(str);
        }
    }

    /// <summary>
    /// 从此图中删除一条边;
    /// </summary>
    /// <param name="curve">曲线</param>
    public void RemoveEdge(Curve3d curve)
    {
        if (curve == null)
            throw new ArgumentNullException(nameof(curve));

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
            if (item.TargetVertex.Equals(dest))
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
        var lastedge = GetEdge(graphVertices[^1], graphVertices[0]);
        if (lastedge is not null)
            curves.Add(lastedge.TargetEdge);

        return curves;
    }
    #endregion

    #region 克隆及接口实现
    /// <summary>
    /// 克隆此图;目测是深克隆
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
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

    /// <summary>
    /// 节点迭代器
    /// </summary>
    /// <returns></returns>
    [System.Diagnostics.DebuggerStepThrough]
    public IEnumerator GetEnumerator()
    {
        return VerticesAsEnumberable.GetEnumerator();
    }

    [System.Diagnostics.DebuggerStepThrough]
    IEnumerator<IGraphVertex>? IEnumerable<IGraphVertex>.GetEnumerator()
    {
        return GetEnumerator() as IEnumerator<IGraphVertex>;
    }
    /// <summary>
    /// 节点迭代器
    /// </summary>
    public IEnumerable<IGraphVertex> VerticesAsEnumberable =>
        vertices.Select(x => x.Key);
    #endregion

    #region 方法
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
public sealed class GraphVertex : IGraphVertex, IEquatable<IGraphVertex>, IComparable, IComparable<IGraphVertex>
{
    #region 属性
    /// <summary>
    /// 数据
    /// </summary>
    public Point3d Data { get; set; }
    /// <summary>
    /// 索引
    /// </summary>
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
    /// <summary>
    /// 是否相等
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(IGraphVertex other)
    {
        return Index == other.Index;
    }
    /// <summary>
    /// 是否相等
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (obj is not IGraphVertex vertex)
            return false;
        else
            return Equals(vertex);
    }
    /// <summary>
    /// 计算hashcode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return Index;
    }
    /// <summary>
    /// 比较大小
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
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
    /// <summary>
    /// 比较大小
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
    /// <summary>
    /// 相等
    /// </summary>
    /// <param name="person1"></param>
    /// <param name="person2"></param>
    /// <returns></returns>
    public static bool operator ==(GraphVertex person1, GraphVertex person2)
    {
        if (person1 is null || person2 is null)
            return Equals(person1, person2);

        return person1.Equals(person2);
    }
    /// <summary>
    /// 不相等
    /// </summary>
    /// <param name="person1"></param>
    /// <param name="person2"></param>
    /// <returns></returns>
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
public sealed class GraphEdge : IEdge, IEquatable<GraphEdge>
{
    #region 属性
    /// <summary>
    /// 顶点
    /// </summary>
    public IGraphVertex TargetVertex { get; set; }
    /// <summary>
    /// 边
    /// </summary>
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
    /// <summary>
    /// 是否相等
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(GraphEdge other)
    {
        if (other is null)
            return false;
        return TargetVertex == other.TargetVertex &&
               TargetEdge == other.TargetEdge;
    }
    /// <summary>
    /// 是否相等
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (obj is not GraphEdge personObj)
            return false;
        else
            return Equals(personObj);
    }
    /// <summary>
    /// 获取hashcode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return (TargetVertex.GetHashCode(), TargetEdge.GetHashCode()).GetHashCode();
    }
    /// <summary>
    /// 相等
    /// </summary>
    /// <param name="person1"></param>
    /// <param name="person2"></param>
    /// <returns></returns>
    public static bool operator ==(GraphEdge person1, GraphEdge person2)
    {
        if (person1 is null || person2 is null)
            return Equals(person1, person2);

        return person1.Equals(person2);
    }
    /// <summary>
    /// 不相等
    /// </summary>
    /// <param name="person1"></param>
    /// <param name="person2"></param>
    /// <returns></returns>
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
public sealed class DepthFirst
{
    #region 公共方法
    /// <summary>
    /// 存储所有的边
    /// </summary>
#if true
    public List<LinkedHashSet<IGraphVertex>> Curve3ds { get; } = new();
#else
    public List<List<IGraphVertex>> Curve3ds { get; } = new();
#endif
    private HashSet<string> Curved { get; } = new();


    /// <summary>
    /// 找出所有的路径
    /// </summary>
    /// <param name="graph">图</param>
    public void FindAll(IGraph graph)
    {
        var total = new HashSet<IGraphVertex>();
        // var graphtmp = graph.Clone();
        foreach (var item in graph.VerticesAsEnumberable)
        {
            Dfs(graph, new LinkedHashSet<IGraphVertex> { item }, total);
            total.Add(item);
        }
    }
    #endregion

    #region 内部方法
    /// <summary>
    /// 递归 DFS;
    /// </summary>
    /// <param name="graph">图</param>
    /// <param name="visited">已经遍历的路径</param>
#if true
    void Dfs(IGraph graph, LinkedHashSet<IGraphVertex> visited, HashSet<IGraphVertex> totalVisited)
    {
        var adjlist = graph.GetAdjacencyList(/*startNode*/ visited.First!.Value); // O(1)
        foreach (var nextNode in adjlist) // O(n)
        {
            if (totalVisited.Contains(nextNode))
            {
                continue;
            }
            // 如果下一个点未遍历过
            if (!visited.Contains(nextNode)) // O(1)
            {
                // 将下一点加入路径集合,并进行下一次递归
                var sub = new LinkedHashSet<IGraphVertex> { nextNode };
                sub.AddRange(visited); // O(n)
                Dfs(graph, sub, totalVisited);
            }
            // 如果下一点遍历过,并且路径大于2,说明已经找到起点
            else if (visited.Count > 2 && nextNode.Equals(visited.Last!.Value))
            {
                // 将重复的路径进行过滤,并把新的路径存入结果
                var curstr = Gethashstring(visited); // O(n)
                if (Isnew(curstr)) // O(1)
                {
                    Curve3ds.Add(visited);
                    Curved.Add(curstr.Item1);
                }
            }
        }
    }




#else

    void Dfs(IGraph graph, List<IGraphVertex> visited)
    {
        var startNode = visited[0];
        IGraphVertex nextNode;
        List<IGraphVertex> sub;

        var adjlist = graph.GetAdjacencyList(startNode).ToList(); // O(n)
        for (int i = 0; i < adjlist.Count; i++) // O(n)
        {
            nextNode = adjlist[i];

            // 如果下一个点未遍历过
            if (!visited.Contains(nextNode)) // O(n)
            {
                // 将下一点加入路径集合,并进行下一次递归
                sub = new List<IGraphVertex> { nextNode };
                sub.AddRange(visited); // O(n)
                Dfs(graph, sub);
            }

            // 如果下一点遍历过,并且路径大于2,说明已经找到起点
            else if (visited.Count > 2 && nextNode.Equals(visited[^1]))
            {
                // 将重复的路径进行过滤,并把新的路径存入结果
                var cur = RotateToSmallest(visited); // O(n)
                var inv = Invert(cur,cur[0]); // O(n)

                var curstr = Gethashstring(cur,inv);
                // Env.Print(curstr);
                if (Isnew(curstr))
                {
                    Curve3ds.Add(cur);
                    Curved.Add(curstr.Item1);
                }
            }
        }
    }
#endif








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
    static List<IGraphVertex> Invert(List<IGraphVertex> lst, IGraphVertex vertex)
    {
        var tmp = lst.ToList();
        tmp.Reverse();
        var index = tmp.IndexOf(vertex);
        return tmp.Skip(index).Concat(lst.Take(index)).ToList();
    }

    static (string, string) Gethashstring(List<IGraphVertex> pathone, List<IGraphVertex> pathtwo)
    {
        var one = new string[pathone.Count];
        var two = new string[pathtwo.Count];
        for (int i = 0; i < pathone.Count; i++)
        {
            one[i] = pathone[i].Index.ToString();
            two[i] = pathtwo[i].Index.ToString();
        }
        return (string.Join("-", one), string.Join("-", two));
    }

    static (string, string) Gethashstring(LinkedHashSet<IGraphVertex> path)
    {
        var one = new string[path.Count];
        var two = new string[path.Count];
        path.For(path.MinNode!, (i, ver1, ver2) => {
            one[i] = ver1.Index.ToString();
            two[i] = ver2.Index.ToString();
        });
        return (string.Join("-", one), string.Join("-", two));
    }


    bool Isnew((string, string) path)
    {
        return !Curved.Contains(path.Item1) && !Curved.Contains(path.Item2);
    }


    #endregion
}