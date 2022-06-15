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

            /*
             *                                                           |             高位64位            |          低位32位             |
             * Convert.ToString(int.MaxValue, 2)输出二进制                                                 "1111111111111111111111111111111" 31个;最高位是符号位,所以少1位
             * Convert.ToString(long.MaxValue,2)输出二进制,刚好长一倍      "11111111111111111111111111111111 1111111111111111111111111111111" 63个;最高位是符号位,所以少1位
             * Convert.ToString(0xffffffffL,  2)int.MaxValue再按位多1                                    "1 1111111111111111111111111111111" 32个;前面的0不会打印出来
             *
             * Convert.ToString(long.MaxValue>>32, 2)相当于平移高位的到低位范围,也就是上面少打印的二进制
             * 验证右移是不是高位保留,答案是
             * var a = Convert.ToInt64("101111111111111111111111111111111111111111111111111111111111111", 2);
             * Convert.ToString(a >> 32,2);
             *
             * (&是尽可能为0) (|是尽可能为1)
             * 32位符号位尽可能为0;再右移高位来低位,使得低位尽可能为1...那它含义何在呢?随机数尽可能大值?
             * 
             */
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