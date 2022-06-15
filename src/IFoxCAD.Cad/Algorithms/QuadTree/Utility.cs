using System;
 
namespace IFoxCAD.Cad
{
    public static class Utility
    {
        /// <summary>
        /// 带有随机种子的随机数
        /// <a href="https://bbs.csdn.net/topics/250037962">为什么这样写随机种子呢</a>
        /// </summary>
        /// <returns></returns>
        public static Random GetRandom()
        {
            var tick = DateTime.Now.Ticks;
            // Convert.ToString(int.MaxValue, 2)输出二进制
            // Convert.ToString(long.MaxValue, 2)输出二进制,刚好长一倍
            // Convert.ToString(0xffffffffL, 2)就是int.MaxValue再按位多1;

            // Convert.ToString(0xffffffffL>>2, 2)就是截断了0xffffffffL的位范围,63-32=31位数量;
            // 64位少最高位(符号)和最低位>>到32位范围
            // (&是尽可能为0) (|是尽可能为1)

            //64位的1位尽可能为0;这31位就保持不变;再右移高位31位过来低位,尽可能保持低位为1
            var tickSeeds = (int)(tick & 0xffffffffL) | (int)(tick >> 32);
            return new Random(tickSeeds);
        }

        /// <summary>
        /// 随机颜色
        /// </summary>
        /// <returns></returns>
        public static System.Drawing.Color RandomColor
        {
            get
            {
                var ran = GetRandom();
                int R = ran.Next(255);
                int G = ran.Next(255);
                int B = ran.Next(255);
                B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
                B = (B > 255) ? 255 : B;
                return System.Drawing.Color.FromArgb(R, G, B);
            }
        }
    }
}