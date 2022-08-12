namespace IFoxCAD.Basal
{
    /* 
     * 由于linq的函数大部分带有状态机,而cad是一个单机程序,
     * 使用状态机会变得缓慢,因此我们设计的时候着重于时间优化,
     * 本工具类在着重于数组遍历时候替代linq
     */
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
        /// 一维数组消重,此函数建议更改为:
        /// <![CDATA[
        ///  HashSet<T> set = new();
        ///  foreach (var item in listInOut)
        ///      set.Add(item);
        /// ]]>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listInOut">传入有重复成员的数组,传出没有重复的</param>
        /// <param name="func">传出参数1:数组开头;传出参数2:数组结尾;返回值比较结尾为<see langword="true"/>就移除</param>
        [Obsolete]
        public static void Deduplication<T>(List<T> listInOut, Func<T, T, bool> func)
        {
            for (int i = 0; i < listInOut.Count; i++)
            {
                var first = listInOut[i];
                for (int j = listInOut.Count - 1; j > i; j--)
                {
                    var last = listInOut[j];
                    if (func(first, last))
                        listInOut.RemoveAt(j);
                }
            }
        }

    }
}
