using Exception = System.Exception;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 无权无向图实现
    /// IEnumerable 枚举所有顶点。
    /// </summary>
    public class Graph<T> : IGraph<T>, IEnumerable<T>
    {
        /// <summary>
        /// 存储所有节点的字典，key为节点的类型，value为节点类型
        /// </summary>
        /// <value></value>
        private Dictionary<T, GraphVertex<T>> vertices = new Dictionary<T, GraphVertex<T>>();

        public int VerticesCount => vertices.Count;
        public bool IsWeightedGraph => false;

        public Graph()
        {
            //vertices = new Dictionary<T, GraphVertex<T>>();
        }

        /// <summary>
        /// 返回一个参考顶点。
        /// 时间复杂度: O(1).
        /// </summary>
        private GraphVertex<T> referenceVertex
        {
            get
            {
                using (var enumerator = vertices.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current.Value;
                    }
                }

                return null;
            }
        }

        IGraphVertex<T> IGraph<T>.ReferenceVertex => referenceVertex;


        /// <summary>
        /// 向该图添加一个新顶点。
        /// 时间复杂度: O(1).
        /// </summary>
        public void AddVertex(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            var newVertex = new GraphVertex<T>(value);

            vertices.Add(value, newVertex);
        }

        /// <summary>
        /// 从此图中删除现有顶点。
        /// 时间复杂度: O(V) 其中 V 是顶点数。
        /// </summary>
        public void RemoveVertex(T vertex)
        {
            if (vertex == null)
            {
                throw new ArgumentNullException();
            }

            if (!vertices.ContainsKey(vertex))
            {
                throw new Exception("顶点不在此图中。");
            }

            foreach (var v in vertices[vertex].Edges)
            {
                v.Edges.Remove(vertices[vertex]);
            }

            vertices.Remove(vertex);
        }

        /// <summary>
        /// 向该图添加一条边。
        /// 时间复杂度: O(1).
        /// </summary>
        public void AddEdge(T source, T dest)
        {
            if (source == null || dest == null)
            {
                throw new ArgumentException();
            }

            if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            {
                throw new Exception("源或目标顶点不在此图中。");
            }

            if (vertices[source].Edges.Contains(vertices[dest])
                || vertices[dest].Edges.Contains(vertices[source]))
            {
                throw new Exception("边已经存在。");
            }

            vertices[source].Edges.Add(vertices[dest]);
            vertices[dest].Edges.Add(vertices[source]);
        }

        /// <summary>
        /// 从此图中删除一条边。
        /// 时间复杂度: O(1).
        /// </summary>
        public void RemoveEdge(T source, T dest)
        {

            if (source == null || dest == null)
            {
                throw new ArgumentException();
            }

            if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            {
                throw new Exception("源或目标顶点不在此图中。");
            }

            if (!vertices[source].Edges.Contains(vertices[dest])
                || !vertices[dest].Edges.Contains(vertices[source]))
            {
                throw new Exception("边不存在。");
            }

            vertices[source].Edges.Remove(vertices[dest]);
            vertices[dest].Edges.Remove(vertices[source]);
        }

        /// <summary>
        /// 我们在给定的来源和目的地之间是否有边？
        /// 时间复杂度: O(1).
        /// </summary>
        public bool HasEdge(T source, T dest)
        {
            if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            {
                throw new ArgumentException("源或目标不在此图中。");
            }

            return vertices[source].Edges.Contains(vertices[dest])
                && vertices[dest].Edges.Contains(vertices[source]);
        }
        /// <summary>
        /// 节点的邻接表
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public IEnumerable<T> Edges(T vertex)
        {
            if (!vertices.ContainsKey(vertex))
            {
                throw new ArgumentException("顶点不在此图中。");
            }

            return vertices[vertex].Edges.Select(x => x.Key);
        }

        public bool ContainsVertex(T value)
        {
            return vertices.ContainsKey(value);
        }

        public IGraphVertex<T> GetVertex(T value)
        {
            return vertices[value];
        }

        /// <summary>
        /// 克隆此图。目测是深克隆
        /// </summary>
        public Graph<T> Clone()
        {
            var newGraph = new Graph<T>();

            foreach (var vertex in vertices)
            {
                newGraph.AddVertex(vertex.Key);
            }

            foreach (var vertex in vertices)
            {
                foreach (var edge in vertex.Value.Edges)
                {
                    newGraph.AddEdge(vertex.Value.Key, edge.Key);
                }
            }

            return newGraph;
        }

        public IEnumerator GetEnumerator()
        {
            return vertices.Select(x => x.Key).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator() as IEnumerator<T>;
        }

        IGraph<T> IGraph<T>.Clone()
        {
            return Clone();
        }

        public IEnumerable<IGraphVertex<T>> VerticesAsEnumberable => (IEnumerable<IGraphVertex<T>>)vertices.Select(x => x.Value);

       
    }

    /// <summary>
    /// 邻接表图实现的顶点。
    /// IEnumerable 枚举所有出边目标顶点。
    /// </summary>
    public class GraphVertex<T> : IEnumerable<T>, IGraphVertex<T>
    {
        public T Key { get; set; }
        /// <summary>
        /// 邻接表
        /// todo:这里应该改为adjacencyList 来表示邻接点表
        /// </summary>
        /// <value></value>
        public HashSet<GraphVertex<T>> Edges { get; }
        /// <summary>
        /// 邻接边表
        /// 这个类的定义有问题：
        /// todo:邻接边表和邻接表的名字是一样的，造成误解
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        IEnumerable<IEdge<T>> IGraphVertex<T>.Edges => (IEnumerable<IEdge<T>>)Edges.Select(x => new Edge<T, int>(x, 1));

        public GraphVertex(T value)
        {
            Key = value;
            Edges = new HashSet<GraphVertex<T>>();
        }

        public IEdge<T> GetEdge(IGraphVertex<T> targetVertex)
        {
            return new Edge<T, int>(targetVertex, 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Edges.Select(x => x.Key).GetEnumerator();
        }
    }


    /// <summary>
    /// 深度优先搜索。
    /// </summary>
    public class DepthFirst<T>
    {
        /// <summary>
        /// 如果项目存在，则返回 true。
        /// 这个函数需要改写，在ifoxcad里返回的应该是所有的环
        /// </summary>
        public bool Find(IGraph<T> graph, T vertex)
        {
            return dfs(graph.ReferenceVertex, new HashSet<T>(), vertex);
        }

        /// <summary>
        /// 递归 DFS。
        /// 这个函数需要改写，在ifoxcad里返回的应该是所有的环
        /// </summary>
        private bool dfs(IGraphVertex<T> current,
            HashSet<T> visited, T searchVetex)
        {
            visited.Add(current.Key);

            if (current.Key.Equals(searchVetex))
            {
                return true;
            }

            foreach (var edge in current.Edges)
            {
                if (visited.Contains(edge.TargetVertexKey))
                {
                    continue; // 改造这个搜索函数，当搜索闭合的时候，将闭合链存入结果列表
                }

                if (dfs(edge.TargetVertex, visited, searchVetex))
                {
                    return true;
                }
            }

            return false;
        }


    }

// ===========另一个dfs实现，感觉比较好一点在于可以用委托干点其他的事情，和前一个可以结合一下

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

    /// <summary>
    /// Implementation of graph vertex.
    /// </summary>
    /// <typeparam name="T">Generic Type.</typeparam>
    public class Vertex<T>
    {
        /// <summary>
        ///     Gets vertex data.
        /// </summary>
        public T Data { get; }

        /// <summary>
        ///     Gets an index of the vertex in graph adjacency matrix.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Gets reference to the graph this vertex belongs to.
        /// </summary>
        public DirectedWeightedGraph<T>? Graph { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex{T}"/> class.
        /// </summary>
        /// <param name="data">Vertex data. Generic type.</param>
        /// <param name="index">Index of the vertex in graph adjacency matrix.</param>
        /// <param name="graph">Graph this vertex belongs to.</param>
        public Vertex(T data, int index, DirectedWeightedGraph<T>? graph)
        {
            Data = data;
            Index = index;
            Graph = graph;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex{T}"/> class.
        /// </summary>
        /// <param name="data">Vertex data. Generic type.</param>
        /// <param name="index">Index of the vertex in graph adjacency matrix.</param>
        public Vertex(T data, int index)
        {
            Data = data;
            Index = index;
        }

        /// <summary>
        ///     Sets graph reference to the null. This method called when vertex removed from the graph.
        /// </summary>
        public void SetGraphNull() => Graph = null;

        /// <summary>
        ///     Override of base ToString method. Prints vertex data and index in graph adjacency matrix.
        /// </summary>
        /// <returns>String which contains vertex data and index in graph adjacency matrix..</returns>
        public override string ToString() => $"Vertex Data: {Data}, Index: {Index}";
    }

    /// <summary>
    ///     Implementation of the directed weighted graph via adjacency matrix.
    /// </summary>
    /// <typeparam name="T">Generic Type.</typeparam>
    public class DirectedWeightedGraph<T> : IDirectedWeightedGraph<T>
    {
        /// <summary>
        ///     Capacity of the graph, indicates the maximum amount of vertices.
        /// </summary>
        private readonly int capacity;

        /// <summary>
        ///     Adjacency matrix which reflects the edges between vertices and their weight.
        ///     Zero value indicates no edge between two vertices.
        /// </summary>
        private readonly double[,] adjacencyMatrix;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectedWeightedGraph{T}"/> class.
        /// </summary>
        /// <param name="capacity">Capacity of the graph, indicates the maximum amount of vertices.</param>
        public DirectedWeightedGraph(int capacity)
        {
            ThrowIfNegativeCapacity(capacity);

            this.capacity = capacity;
            Vertices = new Vertex<T>[capacity];
            adjacencyMatrix = new double[capacity, capacity];
            Count = 0;
        }

        /// <summary>
        ///     Gets list of vertices of the graph.
        /// </summary>
        public Vertex<T>?[] Vertices { get; private set; }

        /// <summary>
        ///     Gets current amount of vertices in the graph.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Adds new vertex to the graph.
        /// </summary>
        /// <param name="data">Data of the vertex.</param>
        /// <returns>Reference to created vertex.</returns>
        public Vertex<T> AddVertex(T data)
        {
            ThrowIfOverflow();
            var vertex = new Vertex<T>(data, Count, this);
            Vertices[Count] = vertex;
            Count++;
            return vertex;
        }

        /// <summary>
        ///     Creates an edge between two vertices of the graph.
        /// </summary>
        /// <param name="startVertex">Vertex, edge starts at.</param>
        /// <param name="endVertex">Vertex, edge ends at.</param>
        /// <param name="weight">Double weight of an edge.</param>
        public void AddEdge(Vertex<T> startVertex, Vertex<T> endVertex, double weight)
        {
            ThrowIfVertexNotInGraph(startVertex);
            ThrowIfVertexNotInGraph(endVertex);

            ThrowIfWeightZero(weight);

            var currentEdgeWeight = adjacencyMatrix[startVertex.Index, endVertex.Index];

            ThrowIfEdgeExists(currentEdgeWeight);

            adjacencyMatrix[startVertex.Index, endVertex.Index] = weight;
        }

        /// <summary>
        ///     Removes vertex from the graph.
        /// </summary>
        /// <param name="vertex">Vertex to be removed.</param>
        public void RemoveVertex(Vertex<T> vertex)
        {
            ThrowIfVertexNotInGraph(vertex);

            Vertices[vertex.Index] = null;
            vertex.SetGraphNull();

            for (var i = 0; i < Count; i++)
            {
                adjacencyMatrix[i, vertex.Index] = 0;
                adjacencyMatrix[vertex.Index, i] = 0;
            }

            Count--;
        }

        /// <summary>
        ///     Removes edge between two vertices.
        /// </summary>
        /// <param name="startVertex">Vertex, edge starts at.</param>
        /// <param name="endVertex">Vertex, edge ends at.</param>
        public void RemoveEdge(Vertex<T> startVertex, Vertex<T> endVertex)
        {
            ThrowIfVertexNotInGraph(startVertex);
            ThrowIfVertexNotInGraph(endVertex);
            adjacencyMatrix[startVertex.Index, endVertex.Index] = 0;
        }

        /// <summary>
        ///     Gets a neighbors of particular vertex.
        /// </summary>
        /// <param name="vertex">Vertex, method gets list of neighbors for.</param>
        /// <returns>Collection of the neighbors of particular vertex.</returns>
        public IEnumerable<Vertex<T>?> GetNeighbors(Vertex<T> vertex)
        {
            ThrowIfVertexNotInGraph(vertex);

            for (var i = 0; i < Count; i++)
            {
                if (adjacencyMatrix[vertex.Index, i] != 0)
                {
                    yield return Vertices[i];
                }
            }
        }

        /// <summary>
        ///     Returns true, if there is an edge between two vertices.
        /// </summary>
        /// <param name="startVertex">Vertex, edge starts at.</param>
        /// <param name="endVertex">Vertex, edge ends at.</param>
        /// <returns>True if edge exists, otherwise false.</returns>
        public bool AreAdjacent(Vertex<T> startVertex, Vertex<T> endVertex)
        {
            ThrowIfVertexNotInGraph(startVertex);
            ThrowIfVertexNotInGraph(endVertex);

            return adjacencyMatrix[startVertex.Index, endVertex.Index] != 0;
        }

        /// <summary>
        /// Return the distance between two vertices in the graph.
        /// </summary>
        /// <param name="startVertex">first vertex in edge.</param>
        /// <param name="endVertex">secnod vertex in edge.</param>
        /// <returns>distance between the two.</returns>
        public double AdjacentDistance(Vertex<T> startVertex, Vertex<T> endVertex)
        {
            if (AreAdjacent(startVertex, endVertex))
            {
                return adjacencyMatrix[startVertex.Index, endVertex.Index];
            }

            return 0;
        }

        private static void ThrowIfNegativeCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new InvalidOperationException("Graph capacity should always be a non-negative integer.");
            }
        }

        private static void ThrowIfWeightZero(double weight)
        {
            if (weight.Equals(0.0d))
            {
                throw new InvalidOperationException("Edge weight cannot be zero.");
            }
        }

        private static void ThrowIfEdgeExists(double currentEdgeWeight)
        {
            if (!currentEdgeWeight.Equals(0.0d))
            {
                throw new InvalidOperationException($"Vertex already exists: {currentEdgeWeight}");
            }
        }

        private void ThrowIfOverflow()
        {
            if (Count == capacity)
            {
                throw new InvalidOperationException("Graph overflow.");
            }
        }

        private void ThrowIfVertexNotInGraph(Vertex<T> vertex)
        {
            if (vertex.Graph != this)
            {
                throw new InvalidOperationException($"Vertex does not belong to graph: {vertex}.");
            }
        }
    }
    /// <summary>
    /// 深度优先搜索 -遍历图的算法。
    /// 算法从用户选择的根节点开始。
    /// 算法在回溯之前尽可能沿着每个分支探索。
    /// </summary>
    /// <typeparam name="T">顶点数据类型。</typeparam>
    public class DepthFirstSearch<T> : IGraphSearch<T> where T : IComparable<T>
    {
        /// <summary>
        /// 从起始顶点遍历图。
        /// </summary>
        ///<param name="graph">图实例。</param>
        ///<param name="startVertex">搜索开始的顶点。</param>
        ///<param name="action">每个图顶点需要执行的动作</param>
        public void VisitAll(IDirectedWeightedGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action = default)
        {
            Dfs(graph, startVertex, action, new HashSet<Vertex<T>>());
        }

        /// <summary>
        /// 从起始顶点遍历图。
        /// </summary>
        ///<param name="graph">图实例。</param>
        ///<param name="startVertex">搜索开始的顶点。</param>
        ///<param name="action">每个图顶点需要执行的动作</param>
        ///<param name="visited">具有访问顶点的哈希集。</param>
        private void Dfs(IDirectedWeightedGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action, HashSet<Vertex<T>> visited)
        {
            action?.Invoke(startVertex);

            visited.Add(startVertex);

            foreach (var vertex in graph.GetNeighbors(startVertex))
            {
                if (vertex == null || visited.Contains(vertex))
                {
                    continue;
                }

                Dfs(graph, vertex!, action, visited);
            }
        }
    }

}
