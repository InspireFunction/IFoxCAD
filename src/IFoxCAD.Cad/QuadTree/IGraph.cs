using IFoxCAD.Cad;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 无向图
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IGraph
    {
        ///// <summary>
        ///// 是否为有权图
        ///// </summary>
        ///// <value></value>
        //bool IsWeightedGraph { get; }
        /// <summary>
        /// 顶点的数量
        /// </summary>
        /// <value></value>
        int VerticesCount { get; }
        /// <summary>
        /// 搜索的起始顶点
        /// </summary>
        /// <value></value>
        Point3d ReferenceVertex { get; }
        /// <summary>
        /// 是否存在顶点
        /// </summary>
        /// <param name="key">顶点键</param>
        /// <returns></returns>
        bool ContainsVertex(Point3d key);
        /// <summary>
        /// 获取顶点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IGraphVertex GetVertex(Point3d key);
        /// <summary>
        /// 顶点的迭代器
        /// </summary>
        /// <value></value>
        IEnumerable<IGraphVertex> VerticesAsEnumberable { get; }
        IEnumerable<Point3d> Point3dAsEnumberable { get; }

        /// <summary>
        /// 是否有边
        /// </summary>
        /// <param name="source">源顶点</param>
        /// <param name="destination">目的顶点</param>
        /// <returns></returns>
        bool HasEdge(Point3d source, Point3d destination);
        /// <summary>
        /// 图克隆函数
        /// </summary>
        /// <returns></returns>
        IGraph Clone();

        HashSet<Point3d> GetAdjacencyList(Point3d vertex);
        HashSet<IEdge> GetAdjacencyEdge(Point3d vertex);
    }

    /// <summary>
    /// 无向图顶点
    /// 顶点的数据结构：
    /// key --- point3d  用于表示点的坐标和作为数据存储的key
    /// edges -- 邻接边表， 每个边的定义为，（边，下一顶点），这样遍历每个顶点的邻接边表时，就可以知道多少个邻接边及对应的邻接点
    /// </summary>
    /// <typeparam name="T">顶点数据类型</typeparam>
    public interface IGraphVertex
    {
        /// <summary>
        /// 顶点的键
        /// </summary>
        /// <value></value>
        Point3d Key { get; }
        ///// <summary>
        ///// 顶点的邻接边表
        ///// </summary>
        ///// <value></value>
        //List<IEdge<T>> Edges { get; }
        /// <summary>
        /// 获取顶点的邻接边
        /// </summary>
        /// <param name="targetVertex">目标顶点</param>
        /// <returns></returns>
        IEdge GetEdge(IGraphVertex targetVertex);
        /// <summary>
        /// 添加邻接边
        /// </summary>
        /// <param name="edge">边类型</param>
        void AddEdge(IEdge edge);
    }
    /// <summary>
    /// 无向图边
    /// </summary>
    /// <typeparam name="T">顶点类型</typeparam>
    /// <typeparam name="U">边的类型</typeparam>
    public interface IEdge
    {
        /// <summary>
        /// 边
        /// </summary>
        Curve3d TargetEdge { get; } 
        /// <summary>
        /// 目标顶点的键
        /// </summary>
        /// <value></value>
        Point3d TargetVertexKey { get; }  
        /// <summary>
        /// 目标顶点
        /// </summary>
        /// <value></value>
        IGraphVertex TargetVertex { get; }
    }
    /// <summary>
    /// 无向图中边的定义
    /// </summary>
    /// <typeparam name="T">边的类型</typeparam>
    /// <typeparam name="C">权重的类型</typeparam>
    internal class GraphEdge : IEdge
    {
        // 这里的传入的两个参数分别为下一点和下一点之间的曲线
        internal GraphEdge(IGraphVertex target, Curve3d edge)
        {
            this.TargetVertex = target;
            this.TargetEdge = edge;
        }

        public Point3d TargetVertexKey => TargetVertex.Key;

        public IGraphVertex TargetVertex { get; private set; }

        public Curve3d TargetEdge { get; private set; }
    }
}


// =====================
// 另一个实现的
/*
public interface IDirectedWeightedGraph<T>
{
    int Count { get; }

    Vertex<T>?[] Vertices { get; }

    void AddEdge(Vertex<T> startVertex, Vertex<T> endVertex, double weight);

    Vertex<T> AddVertex(T data);

    bool AreAdjacent(Vertex<T> startVertex, Vertex<T> endVertex);

    double AdjacentDistance(Vertex<T> startVertex, Vertex<T> endVertex);

    IEnumerable<Vertex<T>?> GetNeighbors(Vertex<T> vertex);

    void RemoveEdge(Vertex<T> startVertex, Vertex<T> endVertex);

    void RemoveVertex(Vertex<T> vertex);
}
public interface IGraphSearch<T>
{
    /// <summary>
    /// 从起始顶点遍历图。
    /// </summary>
    ///<param name="graph">图实例。</param>
    ///<param name="startVertex">搜索开始的顶点。</param>
    ///<param name="action">每个图顶点需要执行的动作</param>
    void VisitAll(IDirectedWeightedGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action = null);
}
*/