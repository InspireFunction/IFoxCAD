#if true
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad.ExtensionMethod
{
    public class Bo
    {
        // 根据论文撸出来的
        // https://www.docin.com/p-740208811.html?docfrom=rrela
        [CommandMethod("TestBo")]
        public void TestBo()
        {
            //TODO 步骤01:提取邻接表...没做
            var boNodes = new List<BoNode>();

            if (boNodes.Count == 0)
                return;

            //步骤02:初始化每个节点
            for (int i = 0; i < boNodes.Count; i++)
            {
                boNodes[i].Color = BoColor.白;
                boNodes[i].Distance = int.MaxValue;
                boNodes[i].Parent = null;
            }

            var q = new Queue<BoNode>();

            //步骤12:源点无法涉及的点,也就是独立白色的点进行处理
            for (int ss = 0; ss < boNodes.Count; ss++)
            {
                //步骤03:源点加入队列
                var s = boNodes[ss];
                //循环的时候,如果它不白色,表示它被处理过
                if (s.Color != BoColor.白)
                    continue;

                //步骤04:加入队列,进行此源点的邻近点(链条)染色
                q.Enqueue(s);
                while (q.Count != 0)
                {
                    //步骤05:
                    var u = q.Dequeue();
                    //步骤06 + 步骤10:遍历邻近节点
                    for (int vv = 0; vv < u.Count; vv++)
                    {
                        var v = u[vv];
                        switch (v.Color)
                        {
                            case BoColor.白://步骤07
                                {
                                    v.Color = BoColor.灰;
                                    v.Distance = u.Distance + 1;
                                    v.Parent = u;
                                    q.Enqueue(v);
                                }
                                break;
                            case BoColor.灰://步骤08
                                {
                                    v.Add(u);
                                }
                                break;
                            case BoColor.黑://步骤09
                                break;
                            default:
                                break;
                        }
                    }
                    //步骤11
                    u.Color = BoColor.黑;
                }
            }

            var boss = new LoopList<LoopList<BoNode>>();
            //步骤13 + 步骤18:从所有节点依次取出一个点
            for (int ii = 0; ii < boNodes.Count; ii++)
            {
                var u = boNodes[ii];
                //步骤14:跳过
                if (u.Count == 0)
                    continue;

                //步骤15:取相遇点,并新建封闭集
                var L = new LoopList<BoNode>();
                for (int jj = 0; jj < u.Count; jj++)
                {
                    var v = boNodes[ii][jj];
                    if (u.Distance == v.Distance)
                    {
                        //步骤16:
                        L.Add(u);
                        L.Add(v);
                        步骤20(u, v, L);
                    }
                    else if (u.Distance - v.Distance < 1)
                    {
                        //步骤17:
                        L.Add(u.Parent!);
                        L.Add(u);
                        L.Add(v);
                        步骤20(u.Parent!, v, L);
                    }
                    boss.Add(L);
                }
            }
            //步骤19: 算法结束
        }

        /// <summary>
        /// 这个是排序吗?
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="L"></param>
        void 步骤20(BoNode u, BoNode v, LoopList<BoNode> L)
        {
            //步骤20:
            if (u.Parent == v.Parent)
            {
                L.AddFirst(u.Parent!);
                return;
            }

            //步骤21:
            L.AddFirst(u.Parent!);
            L.AddLast(v.Parent!);
        }
    }

    public enum BoColor
    {
        白,
        灰,
        黑,
    }
    public class BoNode : List<BoNode>// 拥有成员
    {
        /// <summary>
        /// 颜色
        /// </summary>
        public BoColor Color;
        /// <summary>
        /// 从头到此节点的距离
        /// </summary>
        public int Distance;
        /// <summary>
        /// 父节点
        /// </summary>
        public BoNode? Parent;
    }
}

#endif