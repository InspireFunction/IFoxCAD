#if NET40 || NET35
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 兼容 .NET 4.0 的 CallerMemberNameAttribute 实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute
    {
        public CallerMemberNameAttribute()
        {
        }
    }
}
#endif