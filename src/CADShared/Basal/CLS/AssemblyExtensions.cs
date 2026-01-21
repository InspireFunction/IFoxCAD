#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

using System;
using System.Reflection;

namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        // 在 .NET 3.5 中，Assembly 没有 IsDynamic 属性，需要添加扩展方法
        public static bool IsDynamic(this Assembly assembly)
        {
#if NET35
            // .NET 3.5 不支持动态程序集的概念，所以我们返回 false
            // 但实际上我们可以使用反射来检测是否为动态程序集
            try
            {
                // 在 .NET 3.5 中，动态程序集通常没有 Location
                // 但这也并非绝对准确，所以使用更可靠的方式
                return string.IsNullOrEmpty(assembly.Location);
            }
            catch
            {
                // 如果获取 Location 抛出异常，那很可能是动态程序集
                return true;
            }
#else
            // 对于更高版本，使用内置的 IsDynamic 属性
            return assembly.IsDynamic;
#endif
        }
    }
}