//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace IFoxCAD.Cad.QuadTree
//{
//    // =====================
//    // 另一个实现的
    
//    public interface IGraph<T>
//    {
//        int Count { get; }

//        Vertex<T>?[] Vertices { get; }

//        void AddEdge(Vertex<T> startVertex, Vertex<T> endVertex, double weight);

//        Vertex<T> AddVertex(T data);

//        bool AreAdjacent(Vertex<T> startVertex, Vertex<T> endVertex);

//        double AdjacentDistance(Vertex<T> startVertex, Vertex<T> endVertex);

//        IEnumerable<Vertex<T>?> GetNeighbors(Vertex<T> vertex);

//        void RemoveEdge(Vertex<T> startVertex, Vertex<T> endVertex);

//        void RemoveVertex(Vertex<T> vertex);
//    }





//    public interface IGraphSearch<T>
//    {
//        /// <summary>
//        /// 从起始顶点遍历图。
//        /// </summary>
//        ///<param name="graph">图实例。</param>
//        ///<param name="startVertex">搜索开始的顶点。</param>
//        ///<param name="action">每个图顶点需要执行的动作</param>
//        void VisitAll(IGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action = null);
//    }

//    // ===========另一个dfs实现，感觉比较好一点在于可以用委托干点其他的事情，和前一个可以结合一下

    
//    /// <summary>
//    /// Implementation of graph vertex.
//    /// </summary>
//    /// <typeparam name="T">Generic Type.</typeparam>
//    public class Vertex<T>
//    {
//        /// <summary>
//        /// Gets vertex data.
//        /// </summary>
//        public T Data { get; }

//        /// <summary>
//        /// Gets an index of the vertex in graph adjacency matrix.
//        /// </summary>
//        public int Index { get; }

        

        
//        /// <summary>
//        /// Initializes a new instance of the <see cref="Vertex{T}"/> class.
//        /// </summary>
//        /// <param name="data">Vertex data. Generic type.</param>
//        /// <param name="index">Index of the vertex in graph adjacency matrix.</param>
//        public Vertex(T data, int index)
//        {
//            Data = data;
//            Index = index;
//        }

        
//    }

//    /// <summary>
//    ///     Implementation of the directed weighted graph via adjacency matrix.
//    /// </summary>
//    /// <typeparam name="T">Generic Type.</typeparam>
//    public class Graph<T> : IGraph<T>
//    {
//        /// <summary>
//        ///     Capacity of the graph, indicates the maximum amount of vertices.
//        /// </summary>
//        private readonly int capacity;

//        /// <summary>
//        ///     Adjacency matrix which reflects the edges between vertices and their weight.
//        ///     Zero value indicates no edge between two vertices.
//        /// </summary>
//        private readonly double[,] adjacencyMatrix;

//        /// <summary>
//        ///     Initializes a new instance of the <see cref="Graph{T}"/> class.
//        /// </summary>
//        /// <param name="capacity">Capacity of the graph, indicates the maximum amount of vertices.</param>
//        public Graph(int capacity)
//        {
//            ThrowIfNegativeCapacity(capacity);

//            this.capacity = capacity;
//            Vertices = new Vertex<T>[capacity];
//            adjacencyMatrix = new double[capacity, capacity];
//            Count = 0;
//        }

//        /// <summary>
//        ///     Gets list of vertices of the graph.
//        /// </summary>
//        public Vertex<T>?[] Vertices { get; private set; }

//        /// <summary>
//        ///     Gets current amount of vertices in the graph.
//        /// </summary>
//        public int Count { get; private set; }

//        /// <summary>
//        ///     Adds new vertex to the graph.
//        /// </summary>
//        /// <param name="data">Data of the vertex.</param>
//        /// <returns>Reference to created vertex.</returns>
//        public Vertex<T> AddVertex(T data)
//        {
//            ThrowIfOverflow();
//            var vertex = new Vertex<T>(data, Count, this);
//            Vertices[Count] = vertex;
//            Count++;
//            return vertex;
//        }

//        /// <summary>
//        ///     Creates an edge between two vertices of the graph.
//        /// </summary>
//        /// <param name="startVertex">Vertex, edge starts at.</param>
//        /// <param name="endVertex">Vertex, edge ends at.</param>
//        /// <param name="weight">Double weight of an edge.</param>
//        public void AddEdge(Vertex<T> startVertex, Vertex<T> endVertex, double weight)
//        {
//            ThrowIfVertexNotInGraph(startVertex);
//            ThrowIfVertexNotInGraph(endVertex);

//            ThrowIfWeightZero(weight);

//            var currentEdgeWeight = adjacencyMatrix[startVertex.Index, endVertex.Index];

//            ThrowIfEdgeExists(currentEdgeWeight);

//            adjacencyMatrix[startVertex.Index, endVertex.Index] = weight;
//        }

//        /// <summary>
//        ///     Removes vertex from the graph.
//        /// </summary>
//        /// <param name="vertex">Vertex to be removed.</param>
//        public void RemoveVertex(Vertex<T> vertex)
//        {
//            ThrowIfVertexNotInGraph(vertex);

//            Vertices[vertex.Index] = null;
//            vertex.SetGraphNull();

//            for (var i = 0; i < Count; i++)
//            {
//                adjacencyMatrix[i, vertex.Index] = 0;
//                adjacencyMatrix[vertex.Index, i] = 0;
//            }

//            Count--;
//        }

//        /// <summary>
//        ///     Removes edge between two vertices.
//        /// </summary>
//        /// <param name="startVertex">Vertex, edge starts at.</param>
//        /// <param name="endVertex">Vertex, edge ends at.</param>
//        public void RemoveEdge(Vertex<T> startVertex, Vertex<T> endVertex)
//        {
//            ThrowIfVertexNotInGraph(startVertex);
//            ThrowIfVertexNotInGraph(endVertex);
//            adjacencyMatrix[startVertex.Index, endVertex.Index] = 0;
//        }

//        /// <summary>
//        ///     Gets a neighbors of particular vertex.
//        /// </summary>
//        /// <param name="vertex">Vertex, method gets list of neighbors for.</param>
//        /// <returns>Collection of the neighbors of particular vertex.</returns>
//        public IEnumerable<Vertex<T>?> GetNeighbors(Vertex<T> vertex)
//        {
//            ThrowIfVertexNotInGraph(vertex);

//            for (var i = 0; i < Count; i++)
//            {
//                if (adjacencyMatrix[vertex.Index, i] != 0)
//                {
//                    yield return Vertices[i];
//                }
//            }
//        }

//        /// <summary>
//        ///     Returns true, if there is an edge between two vertices.
//        /// </summary>
//        /// <param name="startVertex">Vertex, edge starts at.</param>
//        /// <param name="endVertex">Vertex, edge ends at.</param>
//        /// <returns>True if edge exists, otherwise false.</returns>
//        public bool AreAdjacent(Vertex<T> startVertex, Vertex<T> endVertex)
//        {
//            ThrowIfVertexNotInGraph(startVertex);
//            ThrowIfVertexNotInGraph(endVertex);

//            return adjacencyMatrix[startVertex.Index, endVertex.Index] != 0;
//        }

//        /// <summary>
//        /// Return the distance between two vertices in the graph.
//        /// </summary>
//        /// <param name="startVertex">first vertex in edge.</param>
//        /// <param name="endVertex">secnod vertex in edge.</param>
//        /// <returns>distance between the two.</returns>
//        public double AdjacentDistance(Vertex<T> startVertex, Vertex<T> endVertex)
//        {
//            if (AreAdjacent(startVertex, endVertex))
//            {
//                return adjacencyMatrix[startVertex.Index, endVertex.Index];
//            }

//            return 0;
//        }

//        private static void ThrowIfNegativeCapacity(int capacity)
//        {
//            if (capacity < 0)
//            {
//                throw new InvalidOperationException("Graph capacity should always be a non-negative integer.");
//            }
//        }

//        private static void ThrowIfWeightZero(double weight)
//        {
//            if (weight.Equals(0.0d))
//            {
//                throw new InvalidOperationException("Edge weight cannot be zero.");
//            }
//        }

//        private static void ThrowIfEdgeExists(double currentEdgeWeight)
//        {
//            if (!currentEdgeWeight.Equals(0.0d))
//            {
//                throw new InvalidOperationException($"Vertex already exists: {currentEdgeWeight}");
//            }
//        }

//        private void ThrowIfOverflow()
//        {
//            if (Count == capacity)
//            {
//                throw new InvalidOperationException("Graph overflow.");
//            }
//        }

//        private void ThrowIfVertexNotInGraph(Vertex<T> vertex)
//        {
//            if (vertex.Graph != this)
//            {
//                throw new InvalidOperationException($"Vertex does not belong to graph: {vertex}.");
//            }
//        }
//    }
//    /// <summary>
//    /// 深度优先搜索 -遍历图的算法。
//    /// 算法从用户选择的根节点开始。
//    /// 算法在回溯之前尽可能沿着每个分支探索。
//    /// </summary>
//    /// <typeparam name="T">顶点数据类型。</typeparam>
//    public class DepthFirstSearch<T> : IGraphSearch<T> where T : IComparable<T>
//    {
//        /// <summary>
//        /// 从起始顶点遍历图。
//        /// </summary>
//        ///<param name="graph">图实例。</param>
//        ///<param name="startVertex">搜索开始的顶点。</param>
//        ///<param name="action">每个图顶点需要执行的动作</param>
//        public void VisitAll(IGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action = default)
//        {
//            Dfs(graph, startVertex, action, new HashSet<Vertex<T>>());
//        }

//        /// <summary>
//        /// 从起始顶点遍历图。
//        /// </summary>
//        ///<param name="graph">图实例。</param>
//        ///<param name="startVertex">搜索开始的顶点。</param>
//        ///<param name="action">每个图顶点需要执行的动作</param>
//        ///<param name="visited">具有访问顶点的哈希集。</param>
//        private void Dfs(IGraph<T> graph, Vertex<T> startVertex, Action<Vertex<T>>? action, HashSet<Vertex<T>> visited)
//        {
//            action?.Invoke(startVertex);

//            visited.Add(startVertex);

//            foreach (var vertex in graph.GetNeighbors(startVertex))
//            {
//                if (vertex == null || visited.Contains(vertex))
//                {
//                    continue;
//                }

//                Dfs(graph, vertex!, action, visited);
//            }
//        }
//    }
    

//}
