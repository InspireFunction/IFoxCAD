#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记


#if NET45_OR_GREATER
using System.Runtime.CompilerServices;
#else

//namespace System.Runtime.CompilerServices;
// 以下就做一个假的内联标记,避免每个特性都要预处理.
// 让我们偷点编译器代码.
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/MethodImplAttribute.cs

[Flags]
public enum MethodImplOptions
{
    Unmanaged = 0x0004, // 指定方法使用非托管调用约定
    NoInlining = 0x0008, // 阻止方法内联
    ForwardRef = 0x0010, // 表示方法是一个正向引用
    Synchronized = 0x0020, // 指示方法是同步的
    NoOptimization = 0x0040, // 禁用方法的优化
    PreserveSig = 0x0080, // 指示方法签名在跨语言调用时应保持不变
    AggressiveInlining = 0x0100, // 强制方法尽可能内联
    AggressiveOptimization = 0x0200, // 绕过分层编译的动态PGO优化
    InternalCall = 0x1000 // 表示方法是一个内部调用
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
public sealed class MethodImplAttribute : System.Attribute
{
    // public MethodCodeType MethodCodeType;
    public MethodImplOptions Value { get; }
    public MethodImplAttribute() { }
    public MethodImplAttribute(MethodImplOptions methodImplOptions)
    {
        Value = methodImplOptions;
    }
    public MethodImplAttribute(short value)
    {
        Value = (MethodImplOptions)value;
    }
}
#endif