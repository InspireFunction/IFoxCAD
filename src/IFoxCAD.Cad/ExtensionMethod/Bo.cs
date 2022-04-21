#if true2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad.ExtensionMethod
{
    public class Bo
    {
        [CommandMethod("TestBo")]
        public void TestBo()
        {
            //TODO 提取邻接表...没做
            var boNodes = new List<BoNode>();

            if (boNodes.Count == 0)
                return;

            //初始化每个节点
            for (int i = 0; i < boNodes.Count; i++)
            {
                boNodes[i].Color = BoColor.白;
                boNodes[i].Distance = double.MaxValue;
                boNodes[i].Parent = null;
            }

            //源点加入队列
            var s = boNodes[0];
            var q = new Queue<BoNode>();
            q.Enqueue(s);
            if (q.Count != 0)
            {
                var u = q.Dequeue();
                for (int i = 0; i < u.Count; i++)
                {
                    var v = u[i];
                    bool cont = false;
                    switch (v.Color)
                    {
                        case BoColor.白:
                            {
                                v.Color = BoColor.灰;
                                v.Distance = u.Distance + 1;
                                v.Parent = u;
                                q.Enqueue(v);
                            }
                            break;
                        case BoColor.灰:
                            {

                            }
                            break;
                        case BoColor.黑:
                            cont = true;
                            break;
                        default:
                            break;
                    }
                    if (cont)
                        continue;


                }
            }


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
        public double Distance;
        /// <summary>
        /// 父节点
        /// </summary>
        public BoNode? Parent;
    }
}

#endif