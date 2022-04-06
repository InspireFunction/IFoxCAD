namespace IFoxCAD.Basal
{
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
        /// 单数组消重
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lst"></param>
        public static void Deduplication<T>(List<T> edgesOut, Func<T, T, bool> func)
        {
            for (int i = 0; i < edgesOut.Count; i++)
            {
                var first = edgesOut[i];
                for (int j = edgesOut.Count - 1; j > i; j--)
                {
                    var last = edgesOut[j];
                    if (func(first, last))
                        edgesOut.RemoveAt(j);
                }
            }
        }
    }
}
