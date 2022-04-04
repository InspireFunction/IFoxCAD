using IFoxCAD.Basal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 边节点
    /// </summary>
    public struct EdgeItem : IEquatable<EdgeItem>
    {
        #region 字段
        /// <summary>
        /// 边界
        /// </summary>
        public Edge Edge;
        /// <summary>
        /// 用来判断搜索方向(向前还是向后)
        /// </summary>
        public bool Forward;
        #endregion

        #region 构造
        public EdgeItem(Edge edge, bool forward)
        {
            Edge = edge;
            Forward = forward;
        }
        #endregion

        #region 方法
        public CompositeCurve3d? GetCurve()
        {
            var cc3d = Edge.Curve;
            if (Forward)
            {
                return cc3d;
            }
            else
            {
                //反向曲线参数
                cc3d = cc3d.Clone() as CompositeCurve3d;
                return cc3d?.GetReverseParameterCurve() as CompositeCurve3d;
            }
        }

        /// <summary>
        /// 查找面域
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="regions"></param>
        public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions)
        {
            var region = new LoopList<EdgeItem>();
            var edgeItem = this;
            region.Add(edgeItem);
            var edgeItem2 = this.GetNext(edges);
            if (edgeItem2.Edge is null)
                return;

            bool hasList = false;

            for (int i = 0; i < regions.Count; i++)
            {
                var edgeList2 = regions[i];
                var node = edgeList2.GetNode(e => e.Equals(edgeItem));
                if (node is not null && node != edgeList2.Last)
                {
                    if (node.Next!.Value.Equals(edgeItem2))
                    {
                        hasList = true;
                        break;
                    }
                }
            }
            if (!hasList)
            {
                while (edgeItem2.Edge is not null)
                {
                    if (edgeItem2.Edge == edgeItem.Edge)
                        break;
                    region.Add(edgeItem2); //TODO 此处死循环,上一条语句判断失误,导致不停的将相同的值加入region
                    edgeItem2 = edgeItem2.GetNext(edges);
                }
                if (edgeItem2.Edge == edgeItem.Edge)
                    regions.Add(region);
            }
        }

        /// <summary>
        /// 获取下一个
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public EdgeItem GetNext(List<Edge> edges)
        {
            Vector3d vec;
            int next;
            if (Forward)
            {
                vec = Edge.GetEndVector();
                next = Edge.EndIndex;
            }
            else
            {
                vec = Edge.GetStartVector();
                next = Edge.StartIndex;
            }

            EdgeItem item = new();
            Vector3d vec2, vec3 = new();
            double angle = 0;
            bool hasNext = false;
            bool forward = false;
            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                if (edge.IsNext(Edge, next, ref vec3, ref forward))
                {
                    if (hasNext)
                    {
                        var angle2 = vec.GetAngleTo(vec3, Vector3d.ZAxis);
                        if (angle2 < angle)
                        {
                            vec2 = vec3;
                            angle = angle2;
                            item.Edge = edge;
                            item.Forward = forward;
                        }
                    }
                    else
                    {
                        vec2 = vec3;
                        angle = vec.GetAngleTo(vec2, Vector3d.ZAxis);
                        item.Edge = edge;
                        item.Forward = forward;
                        hasNext = true;
                    }
                }
            }
            return item;
        }
        #endregion

        #region 类型转换
        public override string ToString()
        {
            return
                Forward ?
                string.Format("{0}-{1}", Edge.StartIndex, Edge.EndIndex) :
                string.Format("{0}-{1}", Edge.EndIndex, Edge.StartIndex);
        }
        #endregion

        #region 重载运算符_比较
        public override bool Equals(object obj)
        {
            return this == (EdgeItem)obj;
        }
        public bool Equals(EdgeItem b)
        {
            return this == b;
        }
        public static bool operator !=(EdgeItem a, EdgeItem b)
        {
            return !(a == b);
        }
        public static bool operator ==(EdgeItem a, EdgeItem b)
        {
            return a.Edge == b.Edge && a.Forward == b.Forward;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
            //return this.ToString().GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// 边
    /// </summary>
    public class Edge : IEquatable<Edge>
    {
        #region 字段
        public CompositeCurve3d Curve;
        public int StartIndex;
        public int EndIndex;
        #endregion

        #region 构造
        public Edge(CompositeCurve3d curve)
        {
            Curve = curve;
        }
        #endregion

        #region 方法
        public Vector3d GetStartVector()
        {
            var inter = Curve.GetInterval();
            PointOnCurve3d poc = new(Curve, inter.LowerBound);
            return poc.GetDerivative(1);
        }

        public Vector3d GetEndVector()
        {
            var inter = Curve.GetInterval();
            PointOnCurve3d poc = new(Curve, inter.UpperBound);
            return -poc.GetDerivative(1);
        }

        /// <summary>
        /// 判断节点位置
        /// </summary>
        /// <param name="edge">边界</param>
        /// <param name="startOrEndIndex">边界是否位于此处</param>
        /// <param name="vec"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public bool IsNext(Edge edge, int startOrEndIndex, ref Vector3d vec, ref bool forward)
        {
            if (edge != this)
            {
                if (StartIndex == startOrEndIndex)
                {
                    vec = GetStartVector();
                    forward = true;
                    return true;
                }
                else if (EndIndex == startOrEndIndex)
                {
                    vec = GetEndVector();
                    forward = false;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 重载运算符_比较
        public override bool Equals(object? obj)
        {
            return this == obj as Edge;
        }
        public bool Equals(Edge? b)
        {
            return this == b;
        }
        public static bool operator !=(Edge? a, Edge? b)
        {
            return !(a == b);
        }
        public static bool operator ==(Edge? a, Edge? b)
        {
            //此处地方不允许使用==null,因为此处是定义
            if (b is null)
                return a is null;
            else if (a is null)
                return false;
            if (ReferenceEquals(a, b))//同一对象
                return true;

            return a.Curve == b.Curve &&
                   a.StartIndex == b.StartIndex &&
                   a.EndIndex == b.EndIndex;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    public class Topo
    {
        /// <summary>
        /// 用于切割的曲线集
        /// </summary>
        List<Curve> _curves;
      
        public Topo(List<Curve> curves)
        {
            _curves = curves;
        }

        /// <summary>
        /// 从曲线集合分离边界(交点断分曲线的)和独立的曲线
        /// </summary>
        /// <param name="edges">边界(可能仍然存在自闭,因为样条曲线允许打个鱼形圈,尾巴又交叉在其他曲线)</param>
        /// <param name="closedCurvesOut">自闭的曲线</param>
        public void GetEdgesAndnewCurves(List<Edge> edges, List<Curve> closedCurvesOut)
        {
            //首先按交点分解为Ge曲线集
            var geCurves = new List<CompositeCurve3d>();
            var paramss = new List<List<double>>();

            for (int i = 0; i < _curves.Count; i++)
            {
                var cc3d = _curves[i].ToCompositeCurve3d();
                if (cc3d is not null)
                {
                    geCurves.Add(cc3d);
                    paramss.Add(new List<double>());
                }
            }

            var cci3d = new CurveCurveIntersector3d();

            //遍历所有曲线,然后获取交点...此处是O(n²)
            for (int i = 0; i < _curves.Count; i++)
            {
                var gc1 = geCurves[i];
                var pars1 = paramss[i];
                //曲线a,和曲线b/c/d/e...分别交点,组成交点数组
                //第一次是 aa对比,所以会怎么样呢?(交点无限个)
                for (int j = i; j < _curves.Count; j++)
                {
                    var gc2 = geCurves[j];
                    var pars2 = paramss[j];

                    //求交类此方法内部会重置,不需要清空,每次set都会有个新的结果
                    cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                    //计算两条曲线的交点(多个),分别放入对应的交点参数集
                    for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                    {
                        var pars = cci3d.GetIntersectionParameters(k);
                        pars1.Add(pars[0]);//0是第一条曲线的交点参数
                        pars2.Add(pars[1]);//1是第二条曲线的交点参数
                    }
                }

                if (gc1.IsClosed())
                {
                    closedCurvesOut.Add(gc1.ToCurve()!);
                }
                //有交点参数
                if (pars1.Count > 0)
                {
                    //根据交点参数断分曲线,然后获取边界
                    var c3ds = gc1.GetSplitCurves(pars1);
                    if (c3ds.Count > 0)
                    {
                        edges.AddRange(c3ds.Select(c => new Edge(c)));
                    }
                    //惊惊留:(不敢删啊...)
                    //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                    //猜测:曲线参数在头或者尾,那么交点就是直接碰头碰尾,
                    //也就是 aa对比 同一条曲线自己和自己产生的?
                    //参数大于0{是这些参数? 头参/尾参/参数不在曲线上?}
                    //else if (gc1.IsClosed())
                    //{
                    //    closedCurvesOut.Add(gc1.ToCurve()!);
                    //}
                    else
                    {
                        edges.Add(new Edge(gc1));
                    }
                }
                //else if (gc1.IsClosed())
                //{
                //    closedCurvesOut.Add(gc1.ToCurve()!);
                //}
            }
            edges = edges.Distinct(new EdgeComparer()).ToList();
        }


        private class EdgeComparer : IEqualityComparer<Edge>
        {

            public bool Equals(Edge x, Edge y)
            {
#if ac2009
                var pts = x.Curve.GetSamplePoints(100);
                return pts.All(pt => y.Curve.IsOn(pt));
#elif ac2013 || ac2015
                var pts = x.Curve.GetSamplePoints(100);
                return pts.All(pt => y.Curve.IsOn(pt.Point));
#endif
                //return x.Curve.IsEqualTo(y.Curve);
            }

            // If Equals() returns true for a pair of objects
            // then GetHashCode() must return the same value for these objects.
            public int GetHashCode(Edge product)
            {
                return product.Curve.GetHashCode();
            }
        }

#if true2
        /// <summary>
        /// 从曲线集合分离边界(交点断分曲线的)和独立的曲线
        /// </summary>
        /// <param name="_edges">边界(可能仍然存在自闭,因为样条曲线允许打个鱼形圈,尾巴又交叉在其他曲线)</param>
        /// <param name="closedCurvesOut">自闭的曲线</param>
        public void GetEdgesAndnewCurves2(List<Curve> closedCurvesOut)
        {
            //首先按交点分解为Ge曲线集
            var geCurves = new List<CompositeCurve3d>();
            var paramss = new List<List<double>>();

            for (int i = 0; i < _curves.Count; i++)
            {
                var cc3d = _curves[i].ToCompositeCurve3d();
                if (cc3d is not null)
                {
                    geCurves.Add(cc3d);
                    paramss.Add(new List<double>());
                }
            }

            var cci3d = new CurveCurveIntersector3d();

            //遍历所有曲线,然后获取交点...此处是O(n²)
            for (int i = 0; i < geCurves.Count; i++)
            {
                var gc1 = geCurves[i];
                var pars1 = paramss[i];
                //曲线a,和曲线b/c/d/e...分别交点,组成交点数组
                //第一次是 aa对比,所以会怎么样呢?(交点无限个 cci3d.NumberOfIntersectionPoints == 0)
                for (int j = i; j < geCurves.Count; j++)
                {
                    var gc2 = geCurves[j];
                    var pars2 = paramss[j];

                    //求交类此方法内部会重置,不需要清空,每次set都会有个新的结果
                    cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                    //没有交点,可能为同方向重叠的
                    //完全重合多段线的时候,此处也不是 ==0
                    if (cci3d.NumberOfIntersectionPoints == 0)
                    {
                        //TODO 两个内接矩形共边(重叠部分边界)会导致处理曲线错误
                        var gc1s = gc1.GetCurves();
                        var gc2s = gc2.GetCurves();
                        if (gc1s.Length == gc2s.Length && gc1s.Length == 0)
                            continue;

                        //点在它们之间
                        //if (OnLine(gc1.StartPoint, gc1.EndPoint, gc2.StartPoint))
                        //{
                        //    var line1 = new Line(gc1.StartPoint, gc2.StartPoint).ToCompositeCurve3d();
                        //    var line2 = new Line(gc2.StartPoint, gc1.EndPoint).ToCompositeCurve3d();
                        //    _edges.Add(new Edge(line1!));
                        //    _edges.Add(new Edge(line2!));
                        //}
                        //if (OnLine(gc1.StartPoint, gc1.EndPoint, gc2.EndPoint))
                        //{
                        //    var line1 = new Line(gc1.StartPoint, gc2.EndPoint).ToCompositeCurve3d();
                        //    var line2 = new Line(gc2.EndPoint, gc1.EndPoint).ToCompositeCurve3d();
                        //    _edges.Add(new Edge(line1!));
                        //    _edges.Add(new Edge(line2!));
                        //}
                    }
                    else
                    {
                        //计算两条曲线的交点(多个),分别放入对应的交点参数集
                        for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                        {
                            var pars = cci3d.GetIntersectionParameters(k);
                            pars1.Add(pars[0]);//0是第一条曲线的交点参数
                            pars2.Add(pars[1]);//1是第二条曲线的交点参数
                        }
                    }
                }

                //有交点参数
                if (pars1.Count > 0)
                {
                    //根据交点参数断分曲线,然后获取边界
                    var c3ds = gc1.GetSplitCurves(pars1);
                    if (c3ds.Count > 0)
                    {
                        _edges.AddRange(c3ds.Select(c => new Edge(c)));
                    }
                    //惊惊留:(不敢删啊...)
                    //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                    //猜测:曲线参数在头或者尾,那么交点就是直接碰头碰尾,
                    //也就是 aa对比 同一条曲线自己和自己产生的?
                    //参数大于0{是这些参数? 头参/尾参/参数不在曲线上?}
                    else if (gc1.IsClosed())
                    {
                        closedCurvesOut.Add(gc1.ToCurve()!);
                    }
                    else
                    {
                        _edges.Add(new Edge(gc1));
                    }
                }
                else if (gc1.IsClosed())
                {
                    closedCurvesOut.Add(gc1.ToCurve()!);
                }
            }
        } 
#endif

        /// <summary>
        /// 判断点是否在线段上
        /// <a href="https://blog.csdn.net/liangzhaoyang1/article/details/51088475">原文链接</a>
        /// </summary>
        /// <param name="sp">线段点头</param>
        /// <param name="ep">线段点尾</param>
        /// <returns>this是否线段ab内</returns>
        public bool OnLine(Point3d sp, Point3d ep, Point3d op)
        {
            //叉乘是保证面积一致
            //叉乘:依次用手指盖住每列,交叉相乘再相减
            var cross = (sp.X - op.X) * (ep.Y - op.Y) - (ep.X - op.X) * (sp.Y - op.Y) < 1e-10;

            return cross
                   && Math.Min(sp.X, ep.X) <= op.X
                   && op.X <= Math.Max(sp.X, ep.X)
                   && Math.Min(sp.Y, ep.Y) <= op.Y
                   && op.Y <= Math.Max(sp.Y, ep.Y);
        }

        /// <summary>
        /// 创建邻接表
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="closedCurvesOut"></param>
        public void AdjacencyList(List<Edge> edges, List<Curve> closedCurvesOut)
        {
            //构建边的邻接表
            //knots 和 nums 实际上是一个键值对(基于ArrayOfStruct思想,拆开更合适内存布局)
            //knots 是不重复地将所有交点设置为节点,如果是重复,就对应 nums++
            //nums 是记录每个节点坐标被重复了几次
            var knots = new List<Point3d>();
            var nums = new List<int>();
            var closedEdges = new List<Edge>();

            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                if (edge.Curve.IsClosed())
                {
                    closedEdges.Add(edge);
                    continue;
                }

                if (knots.Contains(edge.Curve.StartPoint))
                {
                    //含有就是其他曲线"共用"此交点,
                    //节点所在索引==共用计数索引=>将它++
                    edge.StartIndex = knots.IndexOf(edge.Curve.StartPoint);
                    nums[edge.StartIndex]++;
                }
                else
                {
                    //不含有就加入节点,共用计数也加入,边界设置节点索引
                    knots.Add(edge.Curve.StartPoint);
                    nums.Add(1);
                    edge.StartIndex = knots.Count - 1;
                }

                if (knots.Contains(edge.Curve.EndPoint))
                {
                    edge.EndIndex = knots.IndexOf(edge.Curve.EndPoint);
                    nums[edge.EndIndex]++;
                }
                else
                {
                    knots.Add(edge.Curve.EndPoint);
                    nums.Add(1);
                    edge.EndIndex = knots.Count - 1;
                }
            }

            closedCurvesOut.AddRange(closedEdges.Select(e => e.Curve.ToCurve())!);

            //这里把交点只有一条曲线通过的点过滤掉了,也就是尾巴的图元,
            //剩下的都是闭合的曲线连接了,每个点都至少有两条曲线通过
            var tmp = edges
                    .Except(closedEdges)
                    .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1);
            edges.Clear();
            var ge = tmp.GetEnumerator();
            while (ge.MoveNext())
            {
                edges.Add(ge.Current);
            }

            //这一大坨不用看了 注释掉也没影响貌似,而且后续也没有用 nums
            // foreach (var edge in edges.Except(closedEdges))
            // {
            //     if (nums[edge.StartIndex] == 1 || nums[edge.EndIndex] == 1)
            //     {
            //         if (nums[edge.StartIndex] == 1 && nums[edge.EndIndex] == 1)
            //         {
            //             nums[edge.StartIndex] = 0;
            //             nums[edge.EndIndex] = 0;
            //         }
            //         else
            //         {
            //             int next = -1;
            //             if (nums[edge.StartIndex] == 1)
            //             {
            //                 nums[edge.StartIndex] = 0;
            //                 nums[next = edge.EndIndex]--;
            //             }
            //             else
            //             {
            //                 nums[edge.EndIndex] = 0;
            //                 nums[next = edge.StartIndex]--;
            //             }
            //         }
            //     }
            // }
        }

        /// <summary>
        /// 获取多个面域
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public List<LoopList<EdgeItem>> GetRegions(List<Edge> edges)
        {
            //利用边界的顺序和逆序获取闭合链条
            var regions = new List<LoopList<EdgeItem>>();
            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                //TODO 这里有bug,两个内接的矩形会卡死
                var edgeItem = new EdgeItem(edge, true);
                edgeItem.FindRegion(edges, regions); // 经测试是这里卡住了 testTopo
                edgeItem = new EdgeItem(edge, false);
                edgeItem.FindRegion(edges, regions);
            }
            return regions;
        }

        /// <summary>
        /// 这是做什么的
        /// </summary>
        /// <param name="regions"></param>
        public void RegionsInfo(List<LoopList<EdgeItem>> regions)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = i + 1; j < regions.Count;)
                {
                    bool eq = false;
                    if (regions[i].Count == regions[j].Count)
                    {
                        var node = regions[i].First;
                        var curve = node!.Value.Edge.Curve;
                        var node2 = regions[j].GetNode(e => e.Edge.Curve == curve);
                        //var node2 = regions[j].Find(node.Value);
                        if (node2 is not null)
                        {
                            eq = true;
                            var b = node.Value.Forward;
                            var b2 = node2.Value.Forward;
                            for (int k = 1; k < regions[i].Count; k++)
                            {
                                node = node.GetNext(b);
                                node2 = node2.GetNext(b2);
                                if (node!.Value.Edge.Curve != node2!.Value.Edge.Curve)
                                {
                                    eq = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (eq)
                        regions.RemoveAt(j);
                    else
                        j++;
                }
            }
        }



    }
}
