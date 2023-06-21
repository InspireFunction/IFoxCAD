namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 指示参数将为另一个参数传递的表达式捕获为字符串。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
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