namespace IFoxCAD.Cad;

public class Topo
{
    // 碰撞链集合
    List<CollisionChain> _CollisionChains;
    // 求交类(每次set自动重置,都会有个新的结果)
    static CurveCurveIntersector3d _cci3d = new();
    // cad容差类
    public static Tolerance CadTolerance = new(1e-6, 1e-6);

    public Topo(List<Curve> curves)
    {
        List<CurveInfo> curveList = new();

        //提取包围盒信息
        for (int i = 0; i < curves.Count; i++)
            curveList.Add(new CurveInfo(curves[i]));

        //碰撞检测+消重
        _CollisionChains = new();
        CollisionChain? tmp = null;

        Rect.XCollision(curveList,
            oneRect => {
                tmp = oneRect.CollisionChain;//有碰撞链就直接利用之前的链
                return false;
            },
            (oneRect, twoRect) => {
                //消重:包围盒大小一样+首尾相同+采样点相同
                if (oneRect.Equals(twoRect, 1e-6))
                {
                    var pta1 = oneRect.Edge.GeCurve3d.StartPoint;
                    var pta2 = oneRect.Edge.GeCurve3d.EndPoint;
                    var ptb1 = twoRect.Edge.GeCurve3d.StartPoint;
                    var ptb2 = twoRect.Edge.GeCurve3d.EndPoint;

                    if ((pta1.IsEqualTo(ptb1, CadTolerance) && pta2.IsEqualTo(ptb2, CadTolerance))
                        ||
                        (pta1.IsEqualTo(ptb2, CadTolerance) && pta2.IsEqualTo(ptb1, CadTolerance)))
                        if (oneRect.Edge.SplitPointEquals(twoRect.Edge))
                            return true;//跳过后续步骤
                }

                if (tmp == null)
                {
                    tmp = new();
                    oneRect.CollisionChain = tmp;//碰撞链设置
                    tmp.Add(oneRect);//本体也加入链
                }
                twoRect.CollisionChain = tmp;//碰撞链设置
                tmp.Add(twoRect);
                return false;
            },
            oneRect => {
                if (tmp != null && !_CollisionChains.Contains(tmp))
                {
                    _CollisionChains.Add(tmp);
                    tmp = null;
                }
            });
    }


    /// <summary>
    /// 遍历多条碰撞链
    /// </summary>
    /// <param name="action"></param>
    public void CollisionFor(Action<List<CurveInfo>> action)
    {
        _CollisionChains.ForEach(a => {
            action(a);//输出每条碰撞链
        });
    }

    /// <summary>
    /// 利用交点断分曲线和独立自闭曲线
    /// </summary>
    /// <param name="infos_In">传入每组有碰撞的</param>
    /// <param name="edges_Out">传出不自闭的曲线集</param>
    /// <param name="closedCurves_Out">传出自闭曲线集</param>
    public void GetEdgesAndnewCurves(List<CurveInfo> infos_In, List<Edge> edges_Out, List<CompositeCurve3d> closedCurves_Out)
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

                _cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                //计算两条曲线的交点(多个),分别放入对应的交点参数集
                for (int k = 0; k < _cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = _cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]);//0是第一条曲线的交点参数
                    pars2.Add(pars[1]);//1是第二条曲线的交点参数
                }
            }

            if (gc1.IsClosed())
                closedCurves_Out.Add(gc1);

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
                //惊惊留:(不敢删啊...)
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
    /// <param name="edges_InOut">传入每组有碰撞的;传出闭合边界集(扔掉单交点的)</param>
    /// <param name="closedCurve3d_Out">传出自闭曲线集</param>
    public void AdjacencyList(List<Edge> edges_InOut, List<CompositeCurve3d> closedCurve3d_Out)
    {
        //构建边的邻接表
        //下面是键值对(基于ArrayOfStruct思想,拆开更合适内存布局)
        //knots 是不重复地将所有交点设置为节点(如果是重复就对应 nums++)
        //nums  是记录每个交点被重复了几次
        var knots = new List<Point3d>();
        var nums = new List<int>();
        var closedEdges = new List<Edge>();

        //交点集合 knots 会不断增大,会变得更加慢,因此我在一开始就进行碰撞链分析
        for (int i = 0; i < edges_InOut.Count; i++)
        {
            var edge = edges_InOut[i];
            if (edge.GeCurve3d.IsClosed())
            {
                closedEdges.Add(edge);
                continue;
            }

            var sp = edge.GeCurve3d.StartPoint;
            var ep = edge.GeCurve3d.EndPoint;
        
            if (knots.Contains(edge.GeCurve3d.StartPoint))
            {
                //含有就是其他曲线"共用"此交点,
                //节点所在索引==共用计数索引=>将它++
                edge.StartIndex = knots.IndexOf(edge.GeCurve3d.StartPoint);//给它交点计数的索引,不是次数
                nums[edge.StartIndex]++;//交点计数
            }
            else
            {
                //不含有就加入节点,共用计数也加入,边界设置节点索引
                knots.Add(edge.GeCurve3d.StartPoint);
                nums.Add(1);
                edge.StartIndex = knots.Count - 1;
            }

            if (knots.Contains(edge.GeCurve3d.EndPoint))
            {
                edge.EndIndex = knots.IndexOf(edge.GeCurve3d.EndPoint);
                nums[edge.EndIndex]++;
            }
            else
            {
                knots.Add(edge.GeCurve3d.EndPoint);
                nums.Add(1);
                edge.EndIndex = knots.Count - 1;
            }
        }

        closedCurve3d_Out.AddRange(closedEdges.Select(e => e.GeCurve3d));

        //这里把交点只有一条曲线通过的点过滤掉了,也就是尾巴的图元,
        //剩下的都是闭合的曲线连接了,每个点都至少有两条曲线通过
        var tmp = edges_InOut
                .Except(closedEdges)
                .Where(e => nums[e.StartIndex] > 1 && nums[e.EndIndex] > 1);

        var tmpArr = tmp.ToArray();//Clear导致tmp失效
        edges_InOut.Clear();
        for (int i = 0; i < tmpArr.Length; i++)
            edges_InOut.Add(tmpArr[i]);
    }

    /// <summary>
    /// 获取多个面域
    /// </summary>
    /// <param name="edges">剩下都有两个交点的线</param>
    /// <returns></returns>
    public List<LoopList<EdgeItem>> GetRegions(List<Edge> edges)
    {
        var regions_out = new List<LoopList<EdgeItem>>();

        /* 
         * TODO 这里暴力算法需要优化
         * 狐哥为了处理左拐还是右拐图形bd,用了所有的线做种子线,然后往前后推进
         * 前后推进:利用边界的顺序和逆序获取闭合链条,
         *         会造成一环就有: 123,231,312,321,213,132,再其后再判断他们重复
         *         
         * 优化方案一:邻接表按引用次数排序之后,上面的交点如果只有两个图元,组成链,然后移除出邻接表.
         *           这样我只有遇到多个共点时候,那就只有:多个链头尾(减少了腰身)和邻接表的
         *           实现递减.
         */
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            var edgeItem = new EdgeItem(edge, true);
            edgeItem.FindRegion(edges, regions_out);
            edgeItem = new EdgeItem(edge, false);
            edgeItem.FindRegion(edges, regions_out);
        }

        DeduplicationRegions(regions_out);
        return regions_out;
    }

    /// <summary>
    /// 移除相同面域
    /// </summary>
    /// <param name="regions"></param>
    void DeduplicationRegions(List<LoopList<EdgeItem>> regions)
    {
        Basal.ArrayEx.Deduplication(regions, (first, last) => {
            bool eq = false;//是否存在相同成员
            if (first.Count == last.Count)
            {
                var node1 = first.First;
                var curve1 = node1!.Value.GeCurve3d;

                //两个面域对比,找到相同成员
                var node2 = last.GetNode(e => e.GeCurve3d == curve1);
                if (node2 is not null)
                {
                    eq = true;
                    var f1 = node1.Value.Forward;
                    var f2 = node2.Value.Forward;
                    //链条搜索方向来进行
                    //判断每个节点的成员如果一致就会执行移除
                    for (int k = 1; k < first.Count; k++)
                    {
                        node1 = node1.GetNext(f1);
                        node2 = node2.GetNext(f2);
                        if (node1!.Value.GeCurve3d != node2!.Value.GeCurve3d)
                        {
                            eq = false;
                            break;
                        }
                    }
                }
            }
            return eq;
        });
    }
}