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
            if (kn.Count == 0)//此条件不会触发;剔除过程不变0,因为移除了kn;剔除前也不会是0,因为必然初始化并加入;
                knots.RemoveAt(i);
            else if (kn.Count == 1)//孤独点容器必然储存着尾巴图元
            {
                //尾巴图元的头点可能有多个连接,因此尾巴图元会在其他集合中,需要移除其他集合的,
                //而尾点只有一个
                var fir = kn.First();
                for (int j = 0; j < knots.Count; j++)
                    knots[j].Remove(fir);

                //剔除点,减少再次进入剔除尾巴
                knots.RemoveAt(i);
            }
        }

        var pk = PureKnots(knots);

        //TODO 直接进入穷举...图....返回参数取消掉
        //还需要优化深度优先算法=>逆时针最短路径
        //如果次方案测试通过,删除所有 DESIGN0409 的字段所涉及的操作
    }

    /// <summary>
    /// 纯化节点
    /// </summary>
    /// <param name="knots"></param>
    private static List<PolyEdge> PureKnots(List<Knot> knots)
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

        var peList = new List<PolyEdge>();
        for (int i = 0; i < knots.Count; i++)
        {
            var knot = knots[i];
            if (knot.Count == 2)//共点只有两个图元,它们就是手拉手,没有第三者
            {
                //{{a,b}{b,c}{c,d}}=>{a,b,c,d}
                //其中一个已经加入,就找到指定多段线,插入子段
                var a = knot.First!.Value;
                var b = knot.Last!.Value;

                //多段线集合中查询子段,返回多段线
                var pe = PolyEdge.Contains(peList, a);
                if (pe == null)
                {
                    pe = PolyEdge.Contains(peList, b);
                    if (pe != null)
                        pe.AddAfter(pe.Find(b)!, a);
                    else
                    {
                        //新建多段线加入
                        //遍历顺序可能导致:先有{a,b}来找{d,e}找不到,会生成一条{d,e},
                        //所以需要生成完,再找一次首尾.
                        peList.Add(new PolyEdge(a, b));
                    }
                }
                else
                {
                    pe.AddAfter(pe.Find(a)!, b);
                }
            }
            else
            {
                peList.Add(new PolyEdge(knot));
            }
        }

        //遍历顺序可能导致:先有{a,b}来找{c,d}找不到,会生成一条{c,d};但是完成纯化之后会有{{a,b,c}{c,d}}
        //所以需要生成完,再找一次首尾.
        Basal.ArrayEx.Deduplication(peList, (first, last) => {
            if (first.First!.Value == last.First!.Value)//{{c,b,a}{c,d}}=>{d,c,b,a}
            {
                last.ForEach(a => first.AddFirst(a));
                return true;
            }
            else if (first.Last!.Value == last.First!.Value)//{{a,b,c}{c,d}}=>{a,b,c,d}
            {
                last.ForEach(a => first.AddLast(a));
                return true;
            }
            return false;
        });

        return peList;
    }
}