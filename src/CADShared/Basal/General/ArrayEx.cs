namespace IFoxCAD.Basal;

/*
 * 由于linq的函数大部分带有状态机,而cad是一个单机程序,
 * 使用状态机会变得缓慢,因此我们设计的时候着重于时间优化,
 * 本工具类在着重于数组遍历时候替代linq
 */
/// <summary>
/// 数组扩展类
/// </summary>
public static class ArrayEx
{
    /// <summary>
    /// 合并数组
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static T[] Combine2<T>(this T[] a, T[] b)
    {
        var c = new T[a.Length + b.Length];
        Array.Copy(a, 0, c, 0, a.Length);
        Array.Copy(b, 0, c, a.Length, b.Length);
        return c;
    }

    /// <summary>
    /// 一维数组按规则消除<br/>
    /// 本例适用于数值类型比较,特定规则比较<br/>
    /// 如果是哈希比较,建议更改为:
    /// <![CDATA[
    ///  HashSet<T> set = new();
    ///  foreach (var item in listInOut)
    ///      set.Add(item);
    /// ]]>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lst">传入有重复成员的数组,原数组修改</param>
    /// <param name="func">
    /// 传出参数1:数组开头<br/>
    /// 传出参数2:数组结尾<br/>
    /// 返回值比较结尾为<see langword="true"/>就移除<br/>
    /// </param>
    [DebuggerStepThrough]
    public static void Deduplication<T>(List<T> lst, Func<T, T, bool> func)
    {
        // 头和尾比较,满足条件移除尾巴
        for (var i = 0; i < lst.Count; i++)
        {
            var first = lst[i];
            for (var j = lst.Count - 1; j > i/*符号是 >= 而且是i*/; j--)
            {
                var last = lst[j];
                if (func(first, last))
                    lst.RemoveAt(j);
            }
        }
    }
}