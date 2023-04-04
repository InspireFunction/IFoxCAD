

using System.Diagnostics.CodeAnalysis;

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
        /// <param name="value">参数</param>
        /// <param name="valueExpression">参数为null时的提示信息</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull([NotNull] this object? value, string valueExpression = "")
        {
            _ = value ?? throw new ArgumentNullException(nameof(value), valueExpression);
        }
    }
}
