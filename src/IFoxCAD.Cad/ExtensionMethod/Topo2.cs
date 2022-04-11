namespace IFoxCAD.Cad;

public class Topo2
{
    /// <summary>
    /// 创建邻接表
    /// </summary>
    /// <param name="edges_InOut">传入每组有碰撞的;传出闭合边界集(扔掉单交点的)</param>
    /// <param name="closedCurve3d_Out">传出自闭曲线集</param>
    public void AdjacencyList(List<Edge> edges_InOut, List<CompositeCurve3d> closedCurve3d_Out)
    {
        /*
         * 邻接表:不重复地将共点(交点)作为标记,然后容器加入边
         */
        var knots = new List<Knot>();
        for (int i = 0; i < edges_InOut.Count; i++)
        {
            var edge = edges_InOut[i];
            if (edge.GeCurve3d.IsClosed())
            {
                closedCurve3d_Out.Add(edge.GeCurve3d);
                continue;
            }

            var sp = edge.GeCurve3d.StartPoint;
            var ep = edge.GeCurve3d.EndPoint;

            //含有就是其他曲线"共用"此交点,在节点上面加入边;
            //不含有就加入节点和边;
            var kn1 = Knot.Contains(knots, sp);
            if (kn1 != null)
                kn1.Add(edge);
            else
                knots.Add(new Knot(sp, edge));

            var kn2 = Knot.Contains(knots, ep);
            if (kn2 != null)
                kn2.Add(edge);
            else
                knots.Add(new Knot(ep, edge));
        }

        /*
         * 剪枝:
         * 共点若只有一次使用,代表此图元是尾巴,可以扔掉.
         * 剩下每个点都至少有两条曲线通过(闭合曲线)
         */
        for (int i = knots.Count - 1; i >= 0; i--)
        {
            var kn = knots[i];

            if (kn.Count == 0)
            {
                //此条件不会触发;剔除过程不变0,因为移除了kn;
                //剔除前也不会是0,因为必然初始化并加入;
                knots.RemoveAt(i);
            }
            else if (kn.Count == 1)
            {
                //尾巴图元的尾点只有一个,因此它被孤独容器储存着;
                //尾巴图元的头点可能有多个连接,会储存在其他容器,需要移除;
                var fir = kn.First();
                for (int j = 0; j < knots.Count; j++)
                    knots[j].Remove(fir);

                knots.RemoveAt(i);
            }
        }

        var pk = PureKnots(knots);

        //TODO 直接进入穷举...图....返回参数取消掉
        //还需要优化深度优先算法=>逆时针最短路径
        //如果此方案测试通过,删除所有 DESIGN0409 的字段所涉及的操作
    }

    /// <summary>
    /// 纯化节点
    /// </summary>
    /// <param name="knots"></param>
    List<PolyEdge> PureKnots(List<Knot> knots)
    {
        /*
         * 纯化节点:
         * 把 "日" 变成 "(|)"  6个变成2个节点,但是穷举起来就是12;23;13;21;32;31好多啊,因此还需要优化深度优先算法=>逆时针最短路径
         * 连续的碎线(不闭合的多段线,无分叉,不自交),
         * 在图上的节点就没有必然连接的多段线腰身了,搜索起来更快.
         * 
         * 因为图在深度优先的时候必然会进入重复循环获取下一个节点,(逆序又进行了一次)
         * 而上节点和下节点是必然握手的{pt1,pt2,pt2,pt3},
         * 那么我们可以直接数据纯化数据为{pt1,pt3},组成碎线链 EdgePoly ,减少了腰身每次获取的时间.
         * 
         * (日日)两字一个角点相连,造成有(上中/上下/中下)次数没有共点2,直接每个都成为纯化边界;
         * 但是此操作能够使得有2就加速
         */

        var peListAll = new List<PolyEdge>();
        var peList2 = new List<PolyEdge>();
        for (int i = 0; i < knots.Count; i++)
        {
            var knot = knots[i];
            if (knot.Count == 2)
                peList2.Add(new PolyEdge(knot));
            else
                peListAll.Add(new PolyEdge(knot));
        }

        //共点只有两个图元,它们就是手拉手,没有第三者,然后合并多段线
        MergePolyEdge(peList2);

        peListAll.AddRange(peList2);
        return peListAll;
    }

    /// <summary>
    /// 合并多段线
    /// PolyEdge{{a,b}{b,c}..}=>{a,b,c..}
    /// </summary>
    /// <param name="list"></param>
    void MergePolyEdge(List<PolyEdge> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var plA = list[i];
            for (int j = list.Count - 1; j > i; j--)
            {
                var plB = list[j];

                byte actionNum = 0;//不执行
                if (plA.First!.Value == plB.First!.Value)//{{c,b,a}{c,d}}=>{d,c,b,a}
                    actionNum = 1;
                else if (plA.Last!.Value == plB.First!.Value)//{{a,b,c}{c,d}}=>{a,b,c,d}
                    actionNum = 2;
                else if (plA.First!.Value == plB.Last!.Value)//{{c,b,a}{d,c}}=>{d,c,b,a}
                {
                    actionNum = 1;
                    plB.Reverse();
                }
                else if (plA.Last!.Value == plB.Last!.Value)//{{a,b,c}{d,c}}=>{a,b,c,d}
                {
                    actionNum = 2;
                    plB.Reverse();
                }

                if (actionNum == 1)
                {
                    plB.For((num, node) => {
                        if (num != 0)//跳过共元
                            plA.AddFirst(node.Value);
                        return false;
                    });
                }
                else if (actionNum == 2)
                {
                    plB.For((num, node) => {
                        if (num != 0)//跳过共元
                            plA.AddLast(node.Value);
                        return false;
                    });
                }

                if (actionNum != 0)
                {
                    list.RemoveAt(j);
                    j = list.Count - 1;//指针重拨,一旦加入就从尾开始
                }
            }
        }
    }
}