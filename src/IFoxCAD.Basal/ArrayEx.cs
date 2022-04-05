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

    }
}
