using System;

namespace IFoxFunKit;

/// <summary>
/// Option扩展方法
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// 匹配Option的所有分支（必须处理Some和None）
    /// </summary>
    /// <typeparam name="T">Option中的值类型</typeparam>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="option">Option实例</param>
    /// <param name="some">处理Some分支的函数</param>
    /// <param name="none">处理None分支的函数</param>
    /// <returns>匹配结果</returns>
    [MustHandleMember(MustHandleMemberKind.Match)]
    public static TResult Match<T, TResult>(
        this Option<T> option,
        Func<T, TResult> some,
        Func<TResult> none)
    {
        return option.IsSome ? some(option.Value) : none();
    }

    /// <summary>
    /// 匹配Option的所有分支（无返回值版本）
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.Match)]
    public static void Match<T>(
        this Option<T> option,
        Action<T> some,
        Action none)
    {
        if (option.IsSome)
            some(option.Value);
        else
            none();
    }

    /// <summary>
    /// 如果Some则执行操作，返回自身以便链式调用
    /// </summary>
    public static Option<T> IfSome<T>(this Option<T> option, Action<T> action)
    {
        if (option.IsSome)
            action(option.Value);
        return option;
    }

    /// <summary>
    /// 如果None则执行操作，返回自身以便链式调用
    /// </summary>
    public static Option<T> IfNone<T>(this Option<T> option, Action action)
    {
        if (option.IsNone)
            action();
        return option;
    }

    /// <summary>
    /// 将Option转换为可空类型
    /// </summary>
    public static T? ToNullable<T>(this Option<T> option) where T : struct
    {
        return option.IsSome ? option.Value : null;
    }

    /// <summary>
    /// 将Option转换为引用类型（可能为null）
    /// </summary>
    public static T? ToReference<T>(this Option<T> option) where T : class
    {
        return option.IsSome ? option.Value : null;
    }
}

/// <summary>
/// Result扩展方法
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// 匹配Result的所有分支（必须处理Ok和Err）
    /// </summary>
    /// <typeparam name="TOk">成功值类型</typeparam>
    /// <typeparam name="TErr">错误值类型</typeparam>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="result">Result实例</param>
    /// <param name="ok">处理Ok分支的函数</param>
    /// <param name="err">处理Err分支的函数</param>
    /// <returns>匹配结果</returns>
    [MustHandleMember(MustHandleMemberKind.Match)]
    public static TResult Match<TOk, TErr, TResult>(
        this Result<TOk, TErr> result,
        Func<TOk, TResult> ok,
        Func<TErr, TResult> err)
    {
        return result.IsOk ? ok(result.OkValue) : err(result.ErrValue);
    }

    /// <summary>
    /// 匹配Result的所有分支（无返回值版本）
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.Match)]
    public static void Match<TOk, TErr>(
        this Result<TOk, TErr> result,
        Action<TOk> ok,
        Action<TErr> err)
    {
        if (result.IsOk)
            ok(result.OkValue);
        else
            err(result.ErrValue);
    }

    /// <summary>
    /// 匹配Result&lt;T&gt;的所有分支
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> ok,
        Func<Exception, TResult> err)
    {
        return result.IsOk ? ok(result.OkValue) : err(result.ErrValue);
    }

    /// <summary>
    /// 匹配Result&lt;T&gt;的所有分支（无返回值版本）
    /// </summary>
    public static void Match<T>(
        this Result<T> result,
        Action<T> ok,
        Action<Exception> err)
    {
        if (result.IsOk)
            ok(result.OkValue);
        else
            err(result.ErrValue);
    }

    /// <summary>
    /// 如果Ok则执行操作，返回自身以便链式调用
    /// </summary>
    public static Result<TOk, TErr> IfOk<TOk, TErr>(this Result<TOk, TErr> result, Action<TOk> action)
    {
        if (result.IsOk)
            action(result.OkValue);
        return result;
    }

    /// <summary>
    /// 如果Err则执行操作，返回自身以便链式调用
    /// </summary>
    public static Result<TOk, TErr> IfErr<TOk, TErr>(this Result<TOk, TErr> result, Action<TErr> action)
    {
        if (result.IsErr)
            action(result.ErrValue);
        return result;
    }

    /// <summary>
    /// 如果Ok则执行操作（简化版）
    /// </summary>
    public static Result<T> IfOk<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsOk)
            action(result.OkValue);
        return result;
    }

    /// <summary>
    /// 如果Err则执行操作（简化版）
    /// </summary>
    public static Result<T> IfErr<T>(this Result<T> result, Action<Exception> action)
    {
        if (result.IsErr)
            action(result.ErrValue);
        return result;
    }

    /// <summary>
    /// 将Result转换为可空类型
    /// </summary>
    public static TOk? ToNullable<TOk, TErr>(this Result<TOk, TErr> result) where TOk : struct
    {
        return result.IsOk ? result.OkValue : null;
    }

    /// <summary>
    /// 将Result转换为可空类型（简化版）
    /// </summary>
    public static T? ToNullable<T>(this Result<T> result) where T : struct
    {
        return result.IsOk ? result.OkValue : null;
    }
}
