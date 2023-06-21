namespace IFoxCAD.Basal
{
    /// <summary>
    /// 参数null检查类
    /// </summary>
    public static class ArgumentNullEx
    {
        /// <summary>
        /// 检查参数是否为 null
        /// </summary>
        /// <param name="argument">参数</param>
        /// <param name="paramName">参数名字</param>
        public static void ThrowIfNull([NotNull] object? argument, 
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }

        [DoesNotReturn]
        private static void Throw(string? paramName) => throw new ArgumentNullException(paramName);



        /// <summary>
        /// 检查参数是否为 null
        /// </summary>
        /// <param name="value">参数</param>
        /// <param name="valueExpression">参数为null时的提示信息</param>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete("请使用 ArgumentNullEx.ThrowIfNull(value);")]
        public static void NotNull([NotNull] this object? value, [CallerArgumentExpression(nameof(value))] string valueExpression = "")
        {
            _ = value ?? throw new ArgumentNullException(nameof(value), valueExpression);
        }
    }
}