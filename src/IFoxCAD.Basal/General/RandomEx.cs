/*
*┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━模块信息━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
*┃ 作   者：YxrWendao
*┃ 创建时间：2022/8/30 22:49:30
*┃ 模块描述：随机数生成器
*┃ 使用范围：通用
*┃ 说   明:本模块中除GetRandom与NextColor方法是IFoxCAD原有的以外，其他方法均通过网络收集整理而来。
*┃ 代码版本：1.0
*┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
*/

namespace IFoxCAD.Basal;

/// <summary>
/// 随机值扩展类
/// </summary>
public static class RandomEx
{
    /// <summary>
    /// 生成一个指定范围的浮点数值<br/>
    /// <a href="https://www.cnblogs.com/qingheshiguang/p/15806915.html">相关链接</a>
    /// </summary>
    /// <param name="ran">一个随机值产生器</param>
    /// <param name="minValue">范围最小浮点数值</param>
    /// <param name="maxValue">范围最大浮点数值</param>
    /// <returns></returns>
    public static double NextDouble(Random ran, double minValue, double maxValue)
    {
        return ran.NextDouble() * (maxValue - minValue) + minValue;
    }
    /// <summary>
    /// 生成一个指定范围的浮点数值
    /// </summary>
    /// <param name="minValue">范围最小浮点数值</param>
    /// <param name="maxValue">范围最大浮点数值</param>
    /// <returns></returns>
    public static double NextDouble(double minValue, double maxValue)
    {
        return NextDouble(GetRandom(), minValue, maxValue);
    }
    /// <summary>
    /// 生成一个布尔随机数
    /// </summary>
    /// <returns></returns>
    public static bool NextBool()
    {
        return NextBool(GetRandom());
    }
    /// <summary>
    /// 生成一个布尔随机数<br/>
    /// </summary>
    /// <returns></returns>
    public static bool NextBool(Random ran)
    {
        bool[] arr = { true, false };
        return arr[ran.Next(2)];
    }
    /// <summary>
    /// 生成一个不连续或指定值的随机值
    /// </summary>
    /// <param name="arr">一个字符串数组</param>
    /// <returns></returns>
    public static string NextString(string[] arr)
    {
        return NextString(GetRandom(), arr);
    }
    /// <summary>
    /// 生成一个不连续或指定值的随机值
    /// </summary>
    /// <param name="ran">一个随机值产生器</param>
    /// <param name="arr">一个字符串数组</param>
    /// <returns></returns>
    public static string NextString(Random ran, string[] arr)
    {
        ran ??= GetRandom();
        int n = ran.Next(arr.Length - 1);
        return arr[n];
    }
    /// <summary>
    /// 生成一个不连续或指定值的随机值
    /// </summary>
    /// <param name="arr">一个双精度值数组</param>
    /// <returns></returns>
    public static double NextDouble(double[] arr)
    {
        return NextDouble(GetRandom(), arr);
    }
    /// <summary>
    /// 生成不连续或指定值的随机值
    /// </summary>
    /// <param name="ran">一个随机值产生器</param>
    /// <param name="arr">一个双精度值数组</param>
    /// <returns></returns>
    public static double NextDouble(Random ran, double[] arr)
    {
        ran ??= GetRandom();
        int n = ran.Next(arr.Length - 1);
        return arr[n];
    }
    /// <summary>
    /// 生成指定范围内的整数
    /// </summary>
    /// <param name="max">范围最大整数值</param>
    /// <returns></returns>
    public static int NextInt(int max)
    {
        return NextInt(GetRandom(), max);
    }
    /// <summary>
    /// 生成指定范围内的整数
    /// </summary>
    /// <param name="ran">一个随机值产生器</param>
    /// <param name="max">范围最大整数值</param>
    /// <returns></returns>
    public static int NextInt(Random ran, int max)
    {
        ran ??= GetRandom();
        return ran.Next(max);
    }
    /// <summary>
    /// 生成指定范围内的整数
    /// </summary>
    /// <param name="min">范围的最小整数</param>
    /// <param name="max">范围的最大整数</param>
    /// <returns>返回一个介于<paramref name="min"/>与<paramref name="max"/>之间的整数</returns>
    public static int NextInt(int min, int max)
    {
        return NextInt(GetRandom(), min, max);
    }
    /// <summary>
    /// 生成指定范围内的整数
    /// </summary>
    /// <param name="ran">一个随机值产生器</param>
    /// <param name="min">范围的最小整数</param>
    /// <param name="max">范围的最大整数</param>
    /// <returns>返回一个介于<paramref name="min"/>与<paramref name="max"/>之间的整数</returns>
    public static int NextInt(Random ran, int min, int max)
    {
        ran ??= GetRandom();
        return ran.Next(min, max);
    }

    /// <summary>
    /// 生成一个随机颜色
    /// </summary>
    /// <returns>返回<see cref="System.Drawing.Color"/></returns>
    public static System.Drawing.Color NextColor()
    {
        return NextColor(GetRandom());
    }
    /// <summary>
    /// 生成一个随机颜色
    /// </summary>
    /// <returns></returns>
    public static System.Drawing.Color NextColor(Random ran)
    {
        ran ??= GetRandom();
        int R = ran.Next(255);
        int G = ran.Next(255);
        int B = ran.Next(255);
        B = (R + G > 400) ? R + G - 400 : B;// 0 : 380 - R - G;
        B = (B > 255) ? 255 : B;
        return System.Drawing.Color.FromArgb(R, G, B);
    }


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
    * Convert.ToString(long.MaxValue & 0xffffffffL, 2)// 去掉高位:"11111111111111111111111111111111" 32个,再强转int
    * 按位与&是保证符号位肯定是1,其他尽可能为0,高位被去掉只是MaxValue&0的原因,强转才是去掉高位..."尽可能"一词带来第一次随机性
    * 0x02:
    * Convert.ToString((long.MaxValue >> 32), 2)      // 去掉低位: "1111111111111111111111111111111" 31个,再强转int
    * 按位或|是尽可能为1..."尽可能"一词带来第二次随机性
    *
    */

    /// <summary>
    /// 带有随机种子的随机数<br/>
    /// <a href="https://bbs.csdn.net/topics/250037962">为什么这样写随机种子呢</a>
    /// </summary>
    /// <returns></returns>
    public static Random GetRandom()
    {
        var tick = DateTime.Now.Ticks;
        var tickSeeds = (int)(tick & 0xffffffffL) | (int)(tick >> 32);
        return new Random(tickSeeds);
    }
}