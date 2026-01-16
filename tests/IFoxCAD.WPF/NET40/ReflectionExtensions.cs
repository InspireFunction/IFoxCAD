#if NET40 || NET35
using System.Reflection;

namespace System
{
    /// <summary>
    /// 为 .NET 4.0 提供反射兼容性扩展
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// 获取属性值（兼容 .NET 4.0）
        /// </summary>
        public static object GetValue(this PropertyInfo propertyInfo, object obj)
        {
            return propertyInfo.GetValue(obj, null);
        }
    }
}
#endif