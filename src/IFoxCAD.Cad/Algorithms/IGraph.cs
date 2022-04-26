namespace IFoxCAD.Cad;

/// <summary>
/// 无向图
/// </summary>
public interface IGraph
{
    /// <summary>
    /// 顶点的数量
    /// </summary>
    /// <value></value>
    int VerticesCount { get; }
    
    /// <summary>
    /// 是否存在顶点
    /// </summary>
    /// <param name="key">顶点键</param>
    /// <returns></returns>
    bool ContainsVertex(IGraphVertex key);

    /// <summary>
    /// 顶点的迭代器
    /// </summary>
    /// <value></value>
    IEnumerable<IGraphVertex> VerticesAsEnumberable { get; }

    /// <summary>
    /// 是否有边
    /// </summary>
    /// <param name="source">源顶点</param>
    /// <param name="destination">目的顶点</param>
    /// <returns></returns>
    bool HasEdge(IGraphVertex source, IGraphVertex destination);
    /// <summary>
    /// 图克隆函数
    /// </summary>
    /// <returns></returns>
    IGraph Clone();
    /// <summary>
    /// 获取边
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    IEdge? GetEdge(IGraphVertex source, IGraphVertex dest);
    /// <summary>
    /// 邻接表
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    HashSet<IGraphVertex> GetAdjacencyList(IGraphVertex vertex);
    /// <summary>
    /// 邻接边表
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    HashSet<IEdge> GetAdjacencyEdge(IGraphVertex vertex);
    IGraphVertex? ReferenceVertex { get; }

    void RemoveVertex(Point3d pt);
    void RemoveEdge(Curve3d curve);

}

/// <summary>
/// 无向图顶点
/// </summary>
/// <typeparam name="T">顶点数据类型</typeparam>
public interface IGraphVertex : IComparable
{
    /// <summary>
    /// 顶点的键
    /// </summary>
    /// <value></value>
    int Index { get; set; }

    /// <summary>
    /// 顶点的数据
    /// </summary>
    Point3d Data { get; }
}
/// <summary>
/// 无向图边
/// </summary>
public interface IEdge
{
    /// <summary>
    /// 边
    /// </summary>
    Curve3d TargetEdge { get; } 
    /// <summary>
    /// 目标顶点
    /// </summary>
    IGraphVertex TargetVertex { get; }
}



