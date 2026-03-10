
#if !NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics.CodeAnalysis;

namespace IFoxFunKit;

/// <summary>
/// 参数null检查类
/// </summary>
public static class ArgumentNullEx
{
    /// <summary>
    /// 检查参数是否为 null
    /// </summary>
    /// <param name="argument">参数</param>
    /// <param name="paramName">参数名字</param>
    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    private static void Throw(string? paramName) =>
        throw new System.ArgumentNullException(paramName);
}
#else
// 此处应用高版本自带的
// .NET 8.0+ 使用内置的 ArgumentNullException.ThrowIfNull

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace IFoxFunKit;

/// <summary>
/// 参数null检查类 (.NET 8.0+ 使用内置API)
/// </summary>
public static class ArgumentNullEx
{
    /// <summary>
    /// 检查参数是否为 null
    /// </summary>
    /// <param name="argument">参数</param>
    /// <param name="paramName">参数名字</param>
    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        System.ArgumentNullException.ThrowIfNull(argument, paramName);
    }

    /// <summary>
    /// 检查参数是否为 null，并允许自定义异常消息
    /// </summary>
    /// <param name="argument">参数</param>
    /// <param name="paramName">参数名字</param>
    /// <param name="message">异常消息</param>
    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null,
        string? message = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName, message);
        }
    }

    /// <summary>
    /// 检查参数是否为 null（泛型版本，支持值类型和引用类型）
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="argument">参数</param>
    /// <param name="paramName">参数名字</param>
    public static void ThrowIfNull<T>([NotNull] T? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        System.ArgumentNullException.ThrowIfNull(argument, paramName);
    }

    /// <summary>
    /// 检查参数是否为 null（泛型版本，支持值类型和引用类型），并允许自定义异常消息
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="argument">参数</param>
    /// <param name="paramName">参数名字</param>
    /// <param name="message">异常消息</param>
    public static void ThrowIfNull<T>([NotNull] T? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null,
        string? message = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName, message);
        }
    }
}
#endif
