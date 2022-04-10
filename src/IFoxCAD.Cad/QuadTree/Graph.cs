

using Exception = System.Exception;

namespace IFoxCAD.Cad.FirstGraph
{
    /// <summary>
    /// 无权无向图实现
    /// IEnumerable 枚举所有顶点。
    /// </summary>
    public class Graph : IGraph, IEnumerable<IGraphVertex>
    {
        /// <summary>
        /// 存储所有节点的字典，key为顶点的类型，value为邻接表
        /// </summary>
        /// <value></value>
        private Dictionary<IGraphVertex, HashSet<IGraphVertex>> vertices = new ();
        /// <summary>
        /// 邻接边表
        /// </summary>
        private Dictionary<IGraphVertex, HashSet<IEdge>> edges = new ();
        public int VerticesCount => vertices.Count;


        public Graph()
        {
            //vertices = new Dictionary<T, GraphVertex<T>>();
            
        }

        /// <summary>
        /// 向该图添加一个新顶点，但是无边。
        /// 时间复杂度: O(E). E是边数
        /// </summary>
        public void AddVertex(Point3d pt)
        {
            var vertex = new GraphVertex(pt);
            if (vertices.ContainsKey(vertex))
            {
                throw new Exception("顶点已经存在。");
            }

            vertices.Add(vertex, new HashSet<IGraphVertex>());
            edges.Add(vertex, new HashSet<IEdge>());

        }

        /// <summary>
        /// 从此图中删除现有顶点。
        /// 时间复杂度: O(V*E) 其中 V 是顶点数,E是边数。
        /// </summary>
        public void RemoveVertex(Point3d pt)
        {
            var vertex = new GraphVertex(pt);
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
                    if (vertex.Equals(edge.TargetVertex))
                    {
                        item.Remove(edge);
                    }
                }
            }
        }


        /// <summary>
        /// 向该图添加一个边。
        /// 时间复杂度: O(1).
        /// </summary>
        public void AddEdge(Curve3d value!!)
        {
            // 函数有问题,有一个端点在图里时，另一个点应该新增个顶点，
            var start = new GraphVertex(value.StartPoint);
            var end = new GraphVertex(value.EndPoint);


            if (vertices.ContainsKey(start) && !vertices.ContainsKey(end)) // 如果曲线的起点在邻接表字典里,终点不在
            {
                var edge = new GraphEdge(end, value);
                vertices[start].Add(end); // 邻接表加曲线的终点
                edges[start].Add(edge); // 邻接边表加曲线

                // 邻接表字典添加终点
                vertices.Add(end, new HashSet<IGraphVertex>());
                // 邻接表加入起点
                vertices[end].Add(start);
                // 邻接边表字典添加终点
                edges.Add(end, new HashSet<IEdge>());
                // 邻接边表加入起点
                edges[end].Add(new GraphEdge(start, value));

            }
            else if (vertices.ContainsKey(end) && !vertices.ContainsKey(start)) // 如果曲线的终点在邻接表字典里,起点不在
            {
                
                var edge = new GraphEdge(start, value);
                vertices[end].Add(start); // 邻接表加曲线的起点
                edges[end].Add(edge); // 邻接边表加曲线

                // 邻接表字典添加起点
                vertices.Add(start, new HashSet<IGraphVertex>());
                // 邻接表加入终点
                vertices[start].Add(end);
                // 邻接边表字典添加起点
                edges.Add(start, new HashSet<IEdge>());
                // 邻接边表加入终点
                edges[start].Add(new GraphEdge(end, value));
            }
            else if (vertices.ContainsKey(start) && vertices.ContainsKey(end)) // 起点和终点同时在
            {
                var edge = new GraphEdge(end, value);
                vertices[start].Add(end); // 邻接表加曲线的终点
                edges[start].Add(edge); // 邻接边表加曲线

                var edge1 = new GraphEdge(start, value);
                vertices[end].Add(start); // 邻接表加曲线的起点
                edges[end].Add(edge1); // 邻接边表加曲线
            }
            else
            {
                // 邻接表字典添加起点
                vertices.Add(start,new HashSet<IGraphVertex>());
                // 邻接表加入终点
                vertices[start].Add(end);
                // 邻接边表字典添加起点
                edges.Add(start, new HashSet<IEdge>());
                // 邻接边表加入终点
                edges[start].Add(new GraphEdge(end, value));

                // 邻接表字典添加终点
                vertices.Add(end,new HashSet<IGraphVertex>());
                // 邻接表加入起点
                vertices[end].Add(start);
                // 邻接边表字典添加终点
                edges.Add(end, new HashSet<IEdge>());
                // 邻接边表加入起点
                edges[end].Add(new GraphEdge(start,value));
            }

        }

        /// <summary>
        /// 从此图中删除一条边。
        /// 时间复杂度: O(2V*E) 其中 V 是顶点数,E是边数。
        /// </summary>
        public void RemoveEdge(Curve3d curve!!)
        {
            var start = new GraphVertex(curve.StartPoint);
            var end = new GraphVertex(curve.EndPoint);

            if (!vertices.ContainsKey(start) || !vertices.ContainsKey(end))
            {
                throw new Exception("源或目标顶点不在此图中。");
            }

            if (!edges[start].Contains(new GraphEdge(end,curve))
                || !edges[end].Contains(new GraphEdge(start,curve)))
            {
                throw new Exception("边不存在。");
            }
            // 曲线的起点邻接表里删除终点
            vertices[start].Remove(end);
            // 曲线的终点邻接表里删除起点
            vertices[end].Remove(start);
            // 曲线的起点邻接边表里删除终点邻接边
            edges[start].Remove(new GraphEdge(end,curve));
            // 曲线的终点邻接边表里删除起点邻接边
            edges[end].Remove(new GraphEdge(start,curve));

            // 如果 邻接表的长度为0，说明为孤立的顶点就删除
            if (vertices[start].Count == 0)
            {
                vertices.Remove(start);
                edges.Remove(start);
            }
            if (vertices[end].Count == 0)
            {
                vertices.Remove(end);
                edges.Remove(end);

            }

            
        }


        

        

        

        /// <summary>
        /// 我们在给定的来源和目的地之间是否有边？
        /// 时间复杂度: O(E).E 是边
        /// </summary>
        public bool HasEdge(IGraphVertex source, IGraphVertex dest)
        {
            if (!vertices.ContainsKey(source) || !vertices.ContainsKey(dest))
            {
                throw new ArgumentException("源或目标不在此图中。");
            }
            foreach (var item in edges[source])
            {
                if (item.TargetVertex == dest)
                {
                    return true;
                }
            }
            return false;
            
        }
        

        public bool ContainsVertex(IGraphVertex value)
        {
            return vertices.ContainsKey(value);
        }
        

        public HashSet<IGraphVertex> GetAdjacencyList(IGraphVertex vertex)
        {
            return vertices[vertex];
        }

        public HashSet<IEdge> GetAdjacencyEdge(IGraphVertex vertex)
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

        IEnumerator<IGraphVertex>? IEnumerable<IGraphVertex>.GetEnumerator()
        {
            return GetEnumerator() as IEnumerator<IGraphVertex>;
        }

        IGraph IGraph.Clone()
        {
            return Clone();
        }

        

        public IEnumerable<IGraphVertex> VerticesAsEnumberable => 
            vertices.Select(x => x.Key);

        public virtual string ToReadable()
        {
            int i = 1;
            string output = string.Empty;
            
            foreach (var node in vertices)
            {
                var adjacents = string.Empty;

                output = String.Format("{1}\r\n{0}-{2}: [",i, output, node.Key.Data.ToString());

                foreach (var adjacentNode in node.Value)
                    adjacents = String.Format("{0}{1},", adjacents, adjacentNode.Data.ToString());

                if (adjacents.Length > 0)
                    adjacents = adjacents.TrimEnd(new char[] { ',', ' ' });

                output = String.Format("{0}{1}]", output, adjacents);
                i++;
            }

            return output;
        }
    }

    /// <summary>
    /// 邻接表图实现的顶点。
    /// IEnumerable 枚举所有邻接点。
    /// </summary>
    public struct GraphVertex : IGraphVertex, IEquatable<GraphVertex>
    {
        public Point3d Data { get; private set; }
      

        public GraphVertex(Point3d value)
        {
            Data = value;
        }

        public bool Equals(GraphVertex other)
        {
            return Data.IsEqualTo(other.Data, new Tolerance(1e-6,1e-6));
        }
        public override bool Equals(Object obj)
        {
            if (obj is null)
                return false;
            if (obj is not GraphVertex personObj)
                return false;
            else
                return Equals(personObj);
        }

        public override int GetHashCode()
        {
            // 原来的代码 不起作用，那么就转字符串算
            //return (Data.X., Data.Y, Data.Z).GetHashCode();

            return (Data.X.ToString("n6"), Data.Y.ToString("n6"), Data.Z.ToString("n6")).GetHashCode();
        }
        public static bool operator ==(GraphVertex person1, GraphVertex person2)
        {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }
        public static bool operator !=(GraphVertex person1, GraphVertex person2)
        {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }

    }

    /// <summary>
    /// 无向图中边的定义
    /// </summary>
    /// <typeparam name="T">边的类型</typeparam>
    /// <typeparam name="C">权重的类型</typeparam>
    public class GraphEdge : IEdge, IEquatable<GraphEdge>
    {
        // 这里的传入的两个参数分别为下一点和下一点之间的曲线
        internal GraphEdge(IGraphVertex target, Curve3d edge)
        {
            this.TargetVertex = target;
            this.TargetEdge = edge;
        }

        public IGraphVertex TargetVertex { get; private set; }

        public Curve3d TargetEdge { get; private set; }

        public bool Equals(GraphEdge other)
        {
            if (other is null)
            {
                return false;
            }
            return TargetVertex == other.TargetVertex && TargetEdge == other.TargetEdge;
        }
        public override bool Equals(Object obj)
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
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }
        public static bool operator !=(GraphEdge person1, GraphEdge person2)
        {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
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
            foreach (var item in graph.VerticesAsEnumberable)
            {
                var curves = new LoopList<Curve3d>();
                var visited = new List<IGraphVertex>();
                if (dfs(graph,item,visited,item))
                {
                    for (int i = 0; i < visited.Count - 1; i++)
                    {
                        var cur = visited[i];
                        var next = visited[i + 1];
                        var curedge = graph.GetAdjacencyEdge(cur);
                        foreach (var edge in curedge)
                        {
                            if (edge.TargetVertex == next)
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
        private bool dfs(IGraph graph, IGraphVertex current, List<IGraphVertex> visited, IGraphVertex search)
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

    

    
}
