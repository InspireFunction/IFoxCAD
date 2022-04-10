using System;

using Exception = System.Exception;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 无权无向图实现
    /// IEnumerable 枚举所有顶点。
    /// </summary>
    public class Graph : IGraph, IEnumerable<Point3d>
    {
        /// <summary>
        /// 存储所有节点的字典，key为顶点的类型，value为邻接表
        /// </summary>
        /// <value></value>
        private Dictionary<Point3d, HashSet<Point3d>> vertices = new ();
        /// <summary>
        /// 邻接边表
        /// </summary>
        private Dictionary<Point3d, HashSet<IEdge>> edges = new ();

        public int VerticesCount => vertices.Count;

        public Point3d ReferenceVertex => vertices.FirstOrDefault().Key;

        public Graph()
        {
            //vertices = new Dictionary<T, GraphVertex<T>>();
        }

        /// <summary>
        /// 向该图添加一个边。
        /// 时间复杂度: O(1).
        /// </summary>
        public void AddEdge(Curve3d value!!)
        {
            var start = value.StartPoint;
            var end = value.EndPoint;

            if (vertices.ContainsKey(start)) // 如果曲线的起点在邻接表字典里
            {
                var nextVertex = new GraphVertex(end);
                var edge = new GraphEdge(nextVertex, value);
                vertices[start].Add(end); // 邻接表加曲线的终点
                edges[start].Add(edge); // 邻接边表加曲线
            }
            else if (vertices.ContainsKey(end)) // 如果曲线的终点在邻接表字典里
            {
                var nextVertex = new GraphVertex(start);
                var edge = new GraphEdge(nextVertex, value);
                vertices[end].Add(start); // 邻接表加曲线的起点
                edges[end].Add(edge); // 邻接边表加曲线
            }
            else
            {
                // 添加起点
                vertices.Add(start,new HashSet<Point3d>());
                vertices[start].Add(end);
                edges.Add(start, new HashSet<IEdge>());
                edges[start].Add(new GraphEdge(new GraphVertex(end), value));
                // 添加终点
                vertices.Add(end,new HashSet<Point3d>());
                vertices[end].Add(start);
                edges.Add(end, new HashSet<IEdge>());
                edges[end].Add(new GraphEdge(new GraphVertex(start),value));
            }

        }

        /// <summary>
        /// 从此图中删除一条边。
        /// 时间复杂度: O(2V*E) 其中 V 是顶点数,E是边数。
        /// </summary>
        public void RemoveEdge(Curve3d curve!!)
        {
            var start = curve.StartPoint;
            var end = curve.EndPoint;

            if (!vertices.ContainsKey(start) || !vertices.ContainsKey(end))
            {
                throw new Exception("源或目标顶点不在此图中。");
            }

            if (!edges[start].Contains(new GraphEdge(new GraphVertex(end),curve))
                || !edges[end].Contains(new GraphEdge(new GraphVertex(start),curve)))
            {
                throw new Exception("边不存在。");
            }

            RemoveVertex(start);
            RemoveVertex(end);
        }


        /// <summary>
        /// 从此图中删除现有顶点。
        /// 时间复杂度: O(V*E) 其中 V 是顶点数,E是边数。
        /// </summary>
        public void RemoveVertex(Point3d vertex)
        {
            if (!vertices.ContainsKey(vertex))
            {
                throw new Exception("顶点不在此图中。");
            }
            // 删除邻接表里的vertex点,先删除后面的遍历可以少一轮
            vertices.Remove(vertex);
            // 删除其他顶点的邻接表里的vertex点
            foreach (var item in vertices.Values)
            {
                item.Remove(vertex);
            }

            // 删除邻接边表里的vertex点,先删除后面的遍历可以少一轮
            edges.Remove(vertex);
            // 删除其他顶点的邻接边表的指向vertex的边
            foreach (var item in edges.Values)
            {
                foreach (var edge in item)
                {
                    if (edge.TargetVertexKey == vertex)
                    {
                        item.Remove(edge);
                    }
                }
            }
            

        }

        /// <summary>
        /// 向该图添加一个新顶点，但是无边。
        /// 时间复杂度: O(E). E是边数
        /// </summary>
        public void AddVertex(GraphVertex vertex!!)
        {
            if (vertices.ContainsKey(vertex.Key))
            {
                throw new Exception("顶点已经存在。");
            }

            vertices.Add(vertex.Key, new HashSet<Point3d>());
            edges.Add(vertex.Key, new HashSet<IEdge>());

            foreach (var item in vertex.Edges)
            {
                // 根据顶点的邻接边表构建图的邻接表
                vertices[vertex.Key].Add(item.Key);
                // 根据顶点的邻接边表构建图的邻接边表
                edges[vertex.Key].Add(item.Value);
            }
        }

        

        /// <summary>
        /// 我们在给定的来源和目的地之间是否有边？
        /// 时间复杂度: O(E).E 是边
        /// </summary>
        public bool HasEdge(Point3d source, Point3d dest)
        {
            if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            {
                throw new ArgumentException("源或目标不在此图中。");
            }
            foreach (var item in edges[source])
            {
                if (item.TargetVertexKey == dest)
                {
                    return true;
                }
            }
            return false;
            
        }
        

        public bool ContainsVertex(Point3d value)
        {
            return vertices.ContainsKey(value);
        }
        public IGraphVertex GetVertex(Point3d key)
        {
            
            var vertex = new GraphVertex(key);
            foreach (var item in edges[key])
            {
                vertex.AddEdge(item);
            }
            return vertex;
            
        }

        public HashSet<Point3d> GetAdjacencyList(Point3d vertex)
        {
            return vertices[vertex];
        }

        public HashSet<IEdge> GetAdjacencyEdge(Point3d vertex)
        {
            return edges[vertex];
        }


        /// <summary>
        /// 克隆此图。目测是深克隆
        /// </summary>
        public Graph Clone()
        {
            var newGraph = new Graph();

            foreach (var vertex in edges.Values)
            {
                foreach (var item in vertex)
                {
                    newGraph.AddEdge(item.TargetEdge);
                }
            }
            return newGraph;
        }

        public IEnumerator GetEnumerator()
        {
            return vertices.Select(x => x.Key).GetEnumerator();
        }

        IEnumerator<Point3d>? IEnumerable<Point3d>.GetEnumerator()
        {
            return GetEnumerator() as IEnumerator<Point3d>;
        }

        IGraph IGraph.Clone()
        {
            return Clone();
        }

        

        public IEnumerable<IGraphVertex> VerticesAsEnumberable => (IEnumerable<IGraphVertex>)vertices.Select(x => x.Value);
        public IEnumerable<Point3d> Point3dAsEnumberable => vertices.Select(x => x.Key);

    }

    /// <summary>
    /// 邻接表图实现的顶点。
    /// IEnumerable 枚举所有邻接点。
    /// </summary>
    public class GraphVertex : IEnumerable<Point3d>, IGraphVertex
    {
        public Point3d Key { get; private set; }

        /// <summary>
        /// 邻接边表
        /// 这个类的定义有问题：
        /// todo:邻接边表和邻接表的名字是一样的，造成误解
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        public Dictionary<Point3d, IEdge> Edges => new ();

        public GraphVertex(Point3d value)
        {
            Key = value;
        }

        public void AddEdge(IEdge edge)
        {
            Edges.Add(edge.TargetVertexKey,edge);
        }
        public IEdge GetEdge(IGraphVertex targetVertex)
        {
            return Edges[targetVertex.Key];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Point3d> GetEnumerator()
        {
            return Edges.Select(x => x.Key).GetEnumerator();
        }

        
    }


    /// <summary>
    /// 深度优先搜索。
    /// </summary>
    public class DepthFirst
    {
        // 存储所有的边
        public List<LoopList<Curve3d>> Curve3ds { get; } = new List<LoopList<Curve3d>> ();
        /// <summary>
        /// 如果项目存在，则返回 true。
        /// 这个函数需要改写，在ifoxcad里返回的应该是所有的环
        /// </summary>
        public void FindAll(IGraph graph)
        {
            foreach (var item in graph.Point3dAsEnumberable)
            {
                var curves = new LoopList<Curve3d>();
                var visited = new List<Point3d>();
                if (dfs(graph,item,visited,item))
                {
                    for (int i = 0; i < visited.Count - 1; i++)
                    {
                        var cur = visited[i];
                        var next = visited[i + 1];
                        var curedge = graph.GetAdjacencyEdge(cur);
                        foreach (var edge in curedge)
                        {
                            if (edge.TargetVertexKey == next)
                            {
                                curves.Add(edge.TargetEdge);
                            }
                        }

                    }
                }
                Curve3ds.Add(curves);
            }
            
            
        }

        /// <summary>
        /// 递归 DFS。
        /// 这个函数需要改写，在ifoxcad里返回的应该是所有的环
        /// </summary>
        private bool dfs(IGraph graph, Point3d current, List<Point3d> visited, Point3d search)
        {
          
            visited.Add(current);
            if (current == search && visited.Count >= 2)
            {
                return true;
            }

           // 改造这个搜索函数，当搜索闭合的时候，将闭合链存入结果列表
            foreach (var edge in graph.GetAdjacencyList(current))
            {
                if (visited.Contains(edge))
                {
                    continue; 
                }
                if (dfs(graph,edge,visited,search))
                {
                    return true;
                }
            }
            return false;
        }
    }

    

    // ===========另一个dfs实现，感觉比较好一点在于可以用委托干点其他的事情，和前一个可以结合一下

    /*
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
    */
}
