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
        //Point3d ReferenceVertex { get; }
        /// <summary>
        /// 是否存在顶点
        /// </summary>
        /// <param name="key">顶点键</param>
        /// <returns></returns>
        bool ContainsVertex(IGraphVertex key);
        /// <summary>
        /// 获取顶点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //IGraphVertex GetVertex(IGraphVertex key);
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
        IEdge? GetEdge(IGraphVertex source, IGraphVertex dest);
        HashSet<IGraphVertex> GetAdjacencyList(IGraphVertex vertex);
        HashSet<IEdge> GetAdjacencyEdge(IGraphVertex vertex);
    }

    /// <summary>
    /// 无向图顶点
    /// </summary>
    /// <typeparam name="T">顶点数据类型</typeparam>
    public interface IGraphVertex
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

        IGraph Graph { get; }
        /// <summary>
        /// 顶点的邻接边表
        /// </summary>
        /// <value></value>
        //List<IEdge<T>> Edges { get; }
        /// <summary>
        /// 获取顶点的邻接边
        /// </summary>
        /// <param name="targetVertex">目标顶点</param>
        /// <returns></returns>
        //IEdge GetEdge(IGraphVertex targetVertex);
        /// <summary>
        /// 添加邻接边
        /// </summary>
        /// <param name="edge">边类型</param>
        //void AddEdge(IEdge edge);
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
        //Point3d TargetVertexKey { get; }  
        /// <summary>
        /// 目标顶点
        /// </summary>
        /// <value></value>
        IGraphVertex TargetVertex { get; }
    }
    
}


