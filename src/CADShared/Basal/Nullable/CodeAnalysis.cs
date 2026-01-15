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
    public bool ReturnValue { get; }

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
/// 表示当方法返回指定值时，参数不会为 null
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DoesNotReturnIfAttribute : Attribute
{
    public bool ParameterValue { get; }
    public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;
}

/// <summary>
/// 表示方法/属性调用后，参数不会为 null
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class NotNullIfNotNullAttribute : Attribute
{
    public string ParameterName { get; }
    public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;
}
#endif