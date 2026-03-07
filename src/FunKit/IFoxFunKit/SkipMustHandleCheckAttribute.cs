using System;

namespace IFoxFunKit;

/// <summary>
/// 标记方法、属性或类型跳过 MustHandle 分析器的检查
/// 用于 IFoxFunKit 库内部实现，避免自我指涉
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SkipMustHandleCheckAttribute : Attribute
{
}
