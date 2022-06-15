using System;
 
namespace IFoxCAD.Cad
{
    public static class Utility
    {
        public static Random GetRandom()
        {
            var tick = DateTime.Now.Ticks;
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