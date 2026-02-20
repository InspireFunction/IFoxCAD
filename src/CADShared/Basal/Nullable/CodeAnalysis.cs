#if !NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System;

namespace IFoxCAD.Basal;


using System;

/// <summary>
/// 指定输出参数、字段、属性或返回值不为 null
/// </summary>
[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Field |
    AttributeTargets.Property |
    AttributeTargets.ReturnValue,
    AllowMultiple = false)]
public sealed class NotNullAttribute : Attribute
{
}

/// <summary>
/// 指定参数可为 null
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MaybeNullAttribute : Attribute
{
}

/// <summary>
/// 指定输入参数不为 null，即使对应类型允许为 null
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>
    /// 获取返回值。
    /// </summary>
    public bool ReturnValue { get; }

    /// <summary>
    /// 初始化 <see cref="NotNullWhenAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="returnValue">返回值。</param>
    public NotNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }
}

/// <summary>
/// 表示方法永远不会正常返回（总是抛出异常或终止进程）
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DoesNotReturnAttribute : Attribute { }

/// <summary>
/// 表示当指定参数值为 null 时，方法不会返回
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DoesNotReturnIfAttribute : Attribute
{
    /// <summary>
    /// 获取参数值。
    /// </summary>
    public bool ParameterValue { get; }
    /// <summary>
    /// 初始化 <see cref="DoesNotReturnIfAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="parameterValue">参数值。</param>
    public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;
}

/// <summary>
/// 表示参数/属性设置后，结果不会为 null
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class NotNullIfNotNullAttribute : Attribute
{
    /// <summary>
    /// 获取参数名称。
    /// </summary>
    public string ParameterName { get; }
    /// <summary>
    /// 初始化 <see cref="NotNullIfNotNullAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="parameterName">参数名称。</param>
    public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;
}
#endif