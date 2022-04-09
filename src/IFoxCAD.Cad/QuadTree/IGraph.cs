namespace IFoxCAD.Cad
{
    /// <summary>
    /// 无向图
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IGraph<T>
    {
        /// <summary>
        /// 是否为有权图
        /// </summary>
        /// <value></value>
        bool IsWeightedGraph { get; }
        /// <summary>
        /// 节点的数量
        /// </summary>
        /// <value></value>
        int VerticesCount { get; }
        /// <summary>
        /// 搜索的起始节点
        /// </summary>
        /// <value></value>
        IGraphVertex<T> ReferenceVertex { get; }
        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsVertex(T key);
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IGraphVertex<T> GetVertex(T key);
        /// <summary>
        /// 图节点的迭代器
        /// </summary>
        /// <value></value>
        IEnumerable<IGraphVertex<T>> VerticesAsEnumberable { get; }
        /// <summary>
        /// 是否有边
        /// </summary>
        /// <param name="source">源节点</param>
        /// <param name="destination">目的节点</param>
        /// <returns></returns>
        bool HasEdge(T source, T destination);
        /// <summary>
        /// 图克隆函数
        /// </summary>
        /// <returns></returns>
        IGraph<T> Clone();
    }

    /// <summary>
    /// 无向图节点
    /// 顶点的数据结构：
    /// key --- point3d  用于表示点的坐标和作为数据存储的key
    /// edges -- 邻接边表， 每个边的定义为，（边，下一顶点），这样遍历每个顶点的邻接边表时，就可以知道多少个邻接边及对应的邻接点
    /// </summary>
    /// <typeparam name="T">节点数据类型</typeparam>
    public interface IGraphVertex<T>
    {
        /// <summary>
        /// 节点的键
        /// </summary>
        /// <value></value>
        T Key { get; }
        /// <summary>
        /// 节点的邻接边表
        /// </summary>
        /// <value></value>
        IEnumerable<IEdge<T>> Edges { get; }
        /// <summary>
        /// 获取节点的邻接边
        /// </summary>
        /// <param name="targetVertex">目标节点</param>
        /// <returns></returns>
        IEdge<T> GetEdge(IGraphVertex<T> targetVertex);
    }
    /// <summary>
    /// 无向图边
    /// </summary>
    /// <typeparam name="T">边类型</typeparam>
    public interface IEdge<T>
    {
        W Weight<W>() where W : IComparable; // 权重，ifoxcad里应该用不到,计划改为 curve
        /// <summary>
        /// 目标节点的键
        /// </summary>
        /// <value></value>
        T TargetVertexKey { get; }  
        /// <summary>
        /// 目标节点
        /// </summary>
        /// <value></value>
        IGraphVertex<T> TargetVertex { get; }
    }
    /// <summary>
    /// 无向图中边的定义
    /// </summary>
    /// <typeparam name="T">边的类型</typeparam>
    /// <typeparam name="C">权重的类型</typeparam>
    internal class Edge<T, C> : IEdge<T> where C : IComparable
    {
        // 这里的权重是个泛型，所以是否可以用curve类型作为权重？
        // 看来 这里可以用一个曲线作为边的一个值，取消权重的概念，
        // 或者说当C 为int的时候 可以视为权重，当为其他类型的时候 就是一种边的数据类型
        // 所以这里就不能约束C的类型为IComparable
        private object weight;

        internal Edge(IGraphVertex<T> target, C weight)
        {
            this.TargetVertex = target;
            this.weight = weight;
        }

        public T TargetVertexKey => TargetVertex.Key;

        public IGraphVertex<T> TargetVertex { get; private set; }

        public W Weight<W>() where W : IComparable
        {
            return (W)weight;
        }
    }
}