using System.Diagnostics;

namespace IFoxCAD.Cad;

public class Topo
{
    #region 成员
    // 求交类(每次set自动重置,都会有个新的结果)
    static CurveCurveIntersector3d _Cci3d = new();
    // cad容差类
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
    #endregion

    public List<CurveInfo> curveList = new();

    #region 构造
    /// <summary>
    /// 获取封闭曲线
    /// </summary>
    /// <param name="curves"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Topo(List<Curve> curves)
    {
        if (curves == null || curves.Count == 0)
            throw new ArgumentNullException(nameof(curves));

        curveList = new();
        //提取包围盒信息
        for (int i = 0; i < curves.Count; i++)
            curveList.Add(new CurveInfo(curves[i]));
    }

    /// <summary>
    /// 获取曲线集所围成的封闭区域的曲线集
    /// </summary>
    /// <param name="curves">曲线集</param>
    /// <returns>闭合的曲线集</returns>
    public static IEnumerable<Curve>? Create(List<Curve> curves)
    {
        //闭合的曲线集合
        List<CompositeCurve3d> closedCurve3d = new();

        //零散的边界
        List<Edge> gs = new();
        Dictionary<string, BoNode> boNodes = new();

        var topo = new Topo(curves);

        var infos = topo.curveList;

        topo.GetEdgesAndnewCurves(infos, gs, closedCurve3d);
        topo.AdjacencyList(gs, closedCurve3d, boNodes);
        var bos = boNodes.Select(a => a.Value).ToArray();

        //TODO Topo校验数据
        var regions = topo.BreadthFirstSearch(bos);
        if (regions == null || regions.Count == 0)
            return null;

        //零碎的曲线在此处生成封闭曲线
        for (int regNum = 0; regNum < regions.Count; regNum++)
        {
            var pl = BoNode.GetEdges(regions[regNum]);
            if (pl.Count == 0)
                continue;
            var c3ds = CompositeCurve3ds.OrderByPoints(pl);
            closedCurve3d.Add(c3ds.ToCompositeCurve3d());
        }

        //因为生成可能导致遗忘释放,所以这里统一生成
        return closedCurve3d.Select(e => e.ToCurve()!).Where(e => e != null).ToList();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 利用交点断分曲线和独立自闭曲线
    /// </summary>
    /// <param name="infos_In">传入每组有碰撞的</param>
    /// <param name="edges_Out">传出不自闭的曲线集</param>
    /// <param name="closed_Out">传出自闭曲线集</param>
    public void GetEdgesAndnewCurves(List<CurveInfo> infos_In, List<Edge> edges_Out, List<CompositeCurve3d> closed_Out)
    {
        //此处是O(n²)
        //曲线a和其他曲线n根据 交点 切割子线(第一次是自交对比)
        for (int a = 0; a < infos_In.Count; a++)
        {
            var curve1 = infos_In[a];
            var gc1 = curve1.Edge.GeCurve3d;
            var pars1 = curve1.Paramss;

            for (int n = a; n < infos_In.Count; n++)
            {
                var curve2 = infos_In[n];

                //包围盒没有碰撞就直接结束
                if (!curve1.IntersectsWith(curve2))
                    continue;

                var gc2 = curve2.Edge.GeCurve3d;
                var pars2 = curve2.Paramss;

                _Cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                //计算两条曲线的交点(多个),分别放入对应的交点参数集
                for (int k = 0; k < _Cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = _Cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]);//0是第一条曲线的交点参数
                    pars2.Add(pars[1]);//1是第二条曲线的交点参数
                }
            }

            if (gc1.IsClosed())
                closed_Out.Add(gc1);

            if (pars1.Count == 0)
                continue;

            //有交点参数
            //根据交点参数断分曲线,然后获取边界
            var c3ds = curve1.Split(pars1);
            if (c3ds.Count > 0)
            {
                edges_Out.AddRange(c3ds);
            }
            else
            {
                //狐哥写的这里出现的条件是:有曲线参数,但是切分不出来曲线...没懂为什么...
                //是这些参数?{参数0位置?头参/尾参/参数不在曲线上?}
                edges_Out.Add(curve1.Edge);
            }

            //有交点的才消重,无交点必然不重复.
            Edge.Distinct(edges_Out);
        }
    }

    /// <summary>
    /// 创建邻接表
    /// </summary>
    /// <param name="edges_In">传入每组有碰撞的</param>
    /// <param name="closed_Out">传出自闭曲线集</param>
    /// <param name="boNodes_Out">节点集合返回(交点坐标字符串,节点)</param>
    public void AdjacencyList(List<Edge> edges_In, List<CompositeCurve3d> closed_Out, Dictionary<string, BoNode> boNodes_Out)
    {
        int boNumber = 0;
        /*
         * 邻接表:不重复地将共点(交点)作为标记,然后容器加入边
         */
        for (int i = 0; i < edges_In.Count; i++)
        {
            var edge = edges_In[i];
            //曲线闭合直接提供出去
            if (edge.GeCurve3d.IsClosed())
            {
                closed_Out.Add(edge.GeCurve3d);
                continue;
            }

            //点转字符串作为key
            var sp = edge.GeCurve3d.StartPoint.GetHashString(2);
            var ep = edge.GeCurve3d.EndPoint.GetHashString(2);

            //词典key是共用此交点
            BoNode spNode;
            BoNode epNode;
            if (boNodes_Out.ContainsKey(sp))
                spNode = boNodes_Out[sp];
            else
                spNode = new BoNode(boNumber++, edge.GeCurve3d.StartPoint, edge);

            if (boNodes_Out.ContainsKey(ep))
                epNode = boNodes_Out[ep];
            else
                epNode = new BoNode(boNumber++, edge.GeCurve3d.EndPoint, edge);

            //加入边图元
            if (!spNode.Edges.Contains(edge))
                spNode.Edges.Add(edge);
            if (!epNode.Edges.Contains(edge))
                epNode.Edges.Add(edge);

            //加入邻居节点
            if (!spNode.Neighbor.Contains(epNode))
                spNode.Neighbor.Add(epNode);
            if (!epNode.Neighbor.Contains(spNode))
                epNode.Neighbor.Add(spNode);

            //加入词典
            if (!boNodes_Out.ContainsKey(sp))
                boNodes_Out.Add(sp, spNode);
            if (!boNodes_Out.ContainsKey(ep))
                boNodes_Out.Add(ep, epNode);
        }

        // TODO 如果有效率问题,尝试把剪枝去掉,
        // 因为后续的分析边界也同样存在剪枝,可以说可有可无
#if true2
        /*
         * 剪枝:
         * 共点若只有一次使用,代表此图元是尾巴,可以扔掉.
         * 剩下每个点都至少有两条曲线通过(闭合曲线)
         */

        // 尾巴的编号集合
        var reNums = boNodes_Out.Select(a => a.Value)
                            .Where(b => b.Neighbor.Count == 1)//只有一个点就是尾巴
                            .Select(c => c.Number)
                            .ToList();

        for (int ii = boNodes_Out.Count - 1; ii >= 0; ii--)
        {
            //剔除子元素含有尾巴
            var item = boNodes_Out.ElementAt(ii);
            var node = item.Value;
            for (int jj = node.Neighbor.Count - 1; jj >= 0; jj--)
            {
                if (reNums.Contains(node.Neighbor[jj].Number))
                    node.Neighbor.RemoveAt(jj);
            }

            //剔除词典含有尾巴
            if (reNums.Contains(node.Number))
                boNodes_Out.Remove(item.Key);
        }
#endif
    }
    #endregion




    #region 广度
    /// <summary>
    /// 广度优先算法
    /// <a href="https://www.docin.com/p-740208811.html?docfrom=rrela">论文链接</a>
    /// </summary>
    /// <param name="boNodes">邻接表</param>
    /// <returns>多个面域</returns>
    /// <exception cref="ArgumentNullException"></exception>
    List<LoopList<BoNode>> BreadthFirstSearch(BoNode[] boNodes)
    {
        if (boNodes == null || boNodes.Length == 0)
            throw new ArgumentNullException(nameof(boNodes));

        Topo.Init(boNodes);  //O(n)

        //同一代节点进入队列
        var queue = new Queue<BoNode>();

        //步骤12:源点无法涉及的点,也就是独立白色的点进行处理
        for (int ssNum = 0; ssNum < boNodes.Length; ssNum++) //O(n)
        {
            //步骤03:源点
            var s = boNodes[ssNum];
            //步骤12:循环的时候,如果它不白色,表示它被处理过
            if (s.Color != BoColor.白)
                continue;

            //步骤04:源点加入队列,进行此源点的邻近点(邻居/儿子们/同一代的点)涂色
            queue.Enqueue(s);
            while (queue.Count != 0) //O(n)
            {
                //步骤05:
                var 我u = queue.Dequeue();

                //步骤06 + 步骤10:
                //邻近节点(同一代的点)已经在邻接表找到了,这里遍历它们
                我u.Neighbor.ForEach(邻v => {   //O(n)
                    switch (邻v.Color)
                    {
                        case BoColor.白://步骤07:
                            {
                                //把邻近点都涂灰加入队列,
                                //下次就是=>步骤04 循环,下一代再进入=>步骤08
                                邻v.Color = BoColor.灰;
                                邻v.Steps = 我u.Steps + 1;
                                邻v.Parent = 我u;
                                queue.Enqueue(邻v);
                            }
                            break;
                        case BoColor.灰://步骤08
                            {
                                if (邻v.Meet == null)
                                    邻v.Meet = new();
                                邻v.Meet.Add(我u);
                            }
                            break;
                        case BoColor.黑://步骤09
                            {
                                //论文前面说了曲线它不处理,需要提前获取
                                //这里遇到黑色,其实就是腰身闭环(曲线唇形杀回马枪),直接记录环就好了.
                                //由于有步数,所以不会跨两个的
                                //LoopList<BoNode> lst = new();
                                //lst.Add(我u);
                                //lst.Add(子v);
                                //boss.Add(lst);
                            }
                            break;
                        default:
                            break;
                    }
                });
                //步骤11
                我u.Color = BoColor.黑;
            }
        }
        return Topo.MeetGetRegions(boNodes); //O(n2)
    }


    /// <summary>
    /// 在相遇链中提取封闭的区域
    /// </summary>
    /// <param name="boNodes"></param>
    static List<LoopList<BoNode>> MeetGetRegions(BoNode[] boNodes)
    {
        List<LoopList<BoNode>> regions = new();

        //步骤13 + 步骤18:从所有节点依次取出一个点
        for (int boNums = 0; boNums < boNodes.Length; boNums++)
        {
            var 含有相遇v0 = boNodes[boNums];
            //步骤14:跳过
            if (含有相遇v0.Meet == null)
                continue;

            //这里就是论文<2 封闭区域分离算法>
            //步骤15:新建闭合集
            for (int i = 0; i < 含有相遇v0.Meet.Count; i++)
            {
                LoopList<BoNode> region = new();

                //存在多个相遇链,论文的图4a和图4b描述这事,分别是左链和右链分享中间
                var 相遇v1 = 含有相遇v0.Meet[i];
                if (含有相遇v0.Steps == 相遇v1.Steps) //步骤16:步数相同,就是同一代
                {
                    region.Add(含有相遇v0);
                    region.Add(相遇v1);
                }
                else if (相遇v1.Steps + 1 == 含有相遇v0.Steps) //步骤17:步数差1
                {
                    region.Add(含有相遇v0.Parent!);
                    region.Add(含有相遇v0);
                    region.Add(相遇v1);
                }
                else
                {
                    Debugger.Break();//这里会出现意外吗?
                }
                GetLink(region); //O(n)
                regions.Add(Topo.OrderByRegionLines(region)); //O(n2)
            }
        }
        return regions;
    }

    /// <summary>
    /// 调整线序
    /// </summary>
    /// <param name="L"></param>
    static LoopList<BoNode> OrderByRegionLines(LoopList<BoNode> L)
    {
        if (L == null || L.Count == 0)
            throw new ArgumentNullException(nameof(L));

        LoopList<BoNode> list = new();
        var boNode = L.First!.Value;
        for (int i = 0; i < L.Count; i++)//约束循环找顺序次数
        {
            list.Add(boNode);
            L.For((v, item) => {
                //循环每个节点,跳过已经是L2的
                //邻居节点作为目标进入循环
                var boNode2 = item.Value;
                if (boNode2 != boNode
                    && !list.Contains(boNode2)
                    && boNode.Neighbor.Contains(boNode2))
                {
                    boNode = boNode2;//进入循环
                    return true;
                }
                return false;
            });
        }
        return list;
    }


    /// <summary>
    /// 从相遇点开始往上寻找父节点并加入链中
    /// </summary>
    /// <param name="L"></param>
    /// <exception cref="ArgumentNullException"></exception>
    static void GetLink(LoopList<BoNode> L)
    {
        if (L == null || L.Count == 0)
            throw new ArgumentNullException(nameof(L));

        var uM = L.First;
        var vM = L.Last;
        if (uM == vM)
            return;

        var u = uM!.Value;
        var v = vM!.Value;

        while (u.Parent != null && v.Parent != null)
        {
            if (u.Parent == v.Parent)
            {
                //步骤20:
                L.AddLast(u.Parent);
                break;
            }
            else
            {
                //步骤21:
                L.AddFirst(u.Parent);
                L.AddLast(v.Parent);
                u = u.Parent;
                v = v.Parent;
            }
        }
    }


    /// <summary>
    /// 初始化每个节点
    /// </summary>
    /// <param name="boNodes"></param>
    static void Init(BoNode[] boNodes)
    {
        //步骤02:初始化每个节点
        for (int i = 0; i < boNodes.Length; i++)
        {
            boNodes[i].Color = BoColor.白;
            boNodes[i].Steps = int.MaxValue;
            boNodes[i].Parent = null;
            boNodes[i].Meet?.Clear();
        }
    }
    #endregion

}