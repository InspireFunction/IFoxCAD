

namespace IFox.Basal
{
    /// <summary>
    /// 参数null检查类
    /// </summary>
    public static class ArgumentNullEx
    {
        /// <summary>
        /// 检查参数是否为 null
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="argument">参数</param>
        /// <param name="argumentExpression">参数为null时的提示信息</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull<T>(this T argument, string? argumentExpression = null) where T : class
        {
            if (argument == null) throw new ArgumentNullException(paramName: argumentExpression);
        }
    }
}
