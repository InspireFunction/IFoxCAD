using System;

namespace IFoxFunKit;

/// <summary>
/// 标记Option/Result的成员方法或属性
/// 表示访问这些成员算作"已正确处理"
/// 供分析器反射读取
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class MustHandleMemberAttribute : Attribute
{
    /// <summary>
    /// 成员类型分类
    /// </summary>
    public MustHandleMemberKind Kind { get; }

    /// <summary>
    /// 标记Option/Result的成员方法或属性
    /// 表示访问这些成员算作"已正确处理"
    /// 供分析器反射读取
    /// </summary>
    /// <param name="kind">分类</param>
    public MustHandleMemberAttribute(MustHandleMemberKind kind = MustHandleMemberKind.General)
    {
        Kind = kind;
    }
}

/// <summary>
/// MustHandle成员类型分类
/// </summary>
public enum MustHandleMemberKind
{
    /// <summary>
    /// 一般处理成员
    /// </summary>
    General,

    /// <summary>
    /// 状态检查属性（如IsSome, IsNone, IsOk, IsErr）
    /// </summary>
    StatusCheck,

    /// <summary>
    /// 值获取属性（如Value, OkValue, ErrValue）
    /// </summary>
    ValueAccess,

    /// <summary>
    /// 转换方法（如Map, Bind, Filter）
    /// </summary>
    Transform,

    /// <summary>
    /// 默认值方法（如UnwrapOr, UnwrapOrElse）
    /// </summary>
    DefaultValue,

    /// <summary>
    /// 尝试获取方法（如TryGetValue, TryGetOk, TryGetErr）
    /// </summary>
    TryGet,

    /// <summary>
    /// 匹配方法（Match）
    /// </summary>
    Match,
}
