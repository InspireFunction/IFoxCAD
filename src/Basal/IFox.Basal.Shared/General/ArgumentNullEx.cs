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
        public static void NotNull([NotNull] this object? value, [CallerArgumentExpression(nameof(value))] string valueExpression = "")
        {
            _ = value ?? throw new ArgumentNullException(nameof(value), valueExpression);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 指示参数将为另一个参数传递的表达式捕获为字符串。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute
    {
        /// <summary>
        /// 初始化 CallerArgumentExpressionAttribute 类的新实例。
        /// </summary>
        /// <param name="parameterName"></param>
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
        /// <summary>
        /// 获取其表达式应捕获为字符串的参数的名称。
        /// </summary>
        public string ParameterName { get; }
    }
}