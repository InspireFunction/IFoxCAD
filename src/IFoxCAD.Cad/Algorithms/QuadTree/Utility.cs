using System;
 
namespace IFoxCAD.Cad;

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
         * 知识准备:
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
         * 解释代码:
         * 0x01:
         * (int)(long.MaxValue & 0xffffffffL) | (int)(long.MaxValue >> 32);
         * Convert.ToString(long.MaxValue & 0xffffffffL, 2)//去掉高位:"11111111111111111111111111111111" 32个,再强转int
         * 按位与&是保证符号位肯定是1,其他尽可能为0,高位被去掉只是MaxValue&0的原因,强转才是去掉高位..."尽可能"一词带来第一次随机性
         * 0x02:
         * Convert.ToString((long.MaxValue >> 32), 2)      //去掉低位: "1111111111111111111111111111111" 31个,再强转int
         * 按位或|是尽可能为1..."尽可能"一词带来第二次随机性
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