using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IFoxFunKit;

/// <summary>
/// Result类型，表示可能失败的计算结果（类似Rust的Result）
/// </summary>
/// <typeparam name="TOk">成功值的类型</typeparam>
/// <typeparam name="TErr">错误值的类型</typeparam>
[SkipMustHandleCheck]
public readonly struct Result<TOk, TErr> : IEquatable<Result<TOk, TErr>>
{
    private readonly TOk? _okValue;
    private readonly TErr? _errValue;
    private readonly bool _isOk;

    private Result(TOk? okValue, TErr? errValue, bool isOk)
    {
        _okValue = okValue;
        _errValue = errValue;
        _isOk = isOk;
    }

    /// <summary>
    /// 创建成功的Result
    /// </summary>
    public static Result<TOk, TErr> Ok(TOk value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Ok值不能为null");
        return new Result<TOk, TErr>(value, default, true);
    }

    /// <summary>
    /// 创建失败的Result
    /// </summary>
    public static Result<TOk, TErr> Err(TErr error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error), "Err值不能为null");
        return new Result<TOk, TErr>(default, error, false);
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsOk => _isOk;

    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsErr => !_isOk;

    /// <summary>
    /// 获取成功值，如果失败则抛出异常
    /// </summary>
    public TOk OkValue => _isOk ? _okValue! : throw new InvalidOperationException("Result不包含成功值");

    /// <summary>
    /// 获取错误值，如果成功则抛出异常
    /// </summary>
    public TErr ErrValue => !_isOk ? _errValue! : throw new InvalidOperationException("Result不包含错误值");

    /// <summary>
    /// 尝试获取成功值
    /// </summary>
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public bool TryGetOk([MaybeNullWhen(false)] out TOk value)
#else
    public bool TryGetOk(out TOk? value)
#endif
    {
        value = _okValue!;
        return _isOk;
    }

    /// <summary>
    /// 尝试获取错误值
    /// </summary>
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public bool TryGetErr([MaybeNullWhen(false)] out TErr error)
#else
    public bool TryGetErr(out TErr? error)
#endif
    {
        error = _errValue!;
        return !_isOk;
    }

    /// <summary>
    /// 映射成功值
    /// </summary>
    public Result<TResult, TErr> Map<TResult>(Func<TOk, TResult> mapper)
    {
        return _isOk ? Result<TResult, TErr>.Ok(mapper(_okValue!)) : Result<TResult, TErr>.Err(_errValue!);
    }

    /// <summary>
    /// 映射错误值
    /// </summary>
    public Result<TOk, TResult> MapErr<TResult>(Func<TErr, TResult> mapper)
    {
        return _isOk ? Result<TOk, TResult>.Ok(_okValue!) : Result<TOk, TResult>.Err(mapper(_errValue!));
    }

    /// <summary>
    /// 绑定操作（扁平映射）
    /// </summary>
    public Result<TResult, TErr> Bind<TResult>(Func<TOk, Result<TResult, TErr>> binder)
    {
        return _isOk ? binder(_okValue!) : Result<TResult, TErr>.Err(_errValue!);
    }

    /// <summary>
    /// 提供默认值
    /// </summary>
    public TOk UnwrapOr(TOk defaultValue)
    {
        return _isOk ? _okValue! : defaultValue;
    }

    /// <summary>
    /// 通过工厂函数提供默认值
    /// </summary>
    public TOk UnwrapOrElse(Func<TErr, TOk> factory)
    {
        return _isOk ? _okValue! : factory(_errValue!);
    }

    /// <summary>
    /// 获取成功值或抛出异常
    /// </summary>
    public TOk Expect(string message)
    {
        return _isOk ? _okValue! : throw new InvalidOperationException(message);
    }

    /// <summary>
    /// 获取错误值或抛出异常
    /// </summary>
    public TErr ExpectErr(string message)
    {
        return !_isOk ? _errValue! : throw new InvalidOperationException(message);
    }

    /// <summary>
    /// 转换为Option
    /// </summary>
    public Option<TOk> OkToOption()
    {
        return _isOk ? Option<TOk>.Some(_okValue!) : Option<TOk>.None;
    }

    /// <summary>
    /// 将错误转换为Option
    /// </summary>
    public Option<TErr> ErrToOption()
    {
        return !_isOk ? Option<TErr>.Some(_errValue!) : Option<TErr>.None;
    }

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Result<TOk, TErr> other && Equals(other);
    }

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public bool Equals(Result<TOk, TErr> other)
    {
        if (_isOk != other._isOk) return false;
#pragma warning disable CS8604
        if (_isOk)
            return EqualityComparer<TOk>.Default.Equals(_okValue, other._okValue);
        return EqualityComparer<TErr>.Default.Equals(_errValue, other._errValue);
#pragma warning restore CS8604
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode()
    {
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return _isOk ? HashCode.Combine(_okValue) : HashCode.Combine(_errValue);
#else
        return _isOk ? (_okValue?.GetHashCode() ?? 0) : (_errValue?.GetHashCode() ?? 0);
#endif
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        return _isOk ? $"Ok({_okValue})" : $"Err({_errValue})";
    }

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(Result<TOk, TErr> left, Result<TOk, TErr> right) => left.Equals(right);

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(Result<TOk, TErr> left, Result<TOk, TErr> right) => !left.Equals(right);
}

/// <summary>
/// 错误类型为Exception的Result简化版
/// </summary>
[SkipMustHandleCheck]
public readonly struct Result<T> : IEquatable<Result<T>>
{
    private readonly Result<T, Exception> _inner;

    private Result(Result<T, Exception> inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// 创建成功的Result
    /// </summary>
    public static Result<T> Ok(T value)
    {
        return new Result<T>(Result<T, Exception>.Ok(value));
    }

    /// <summary>
    /// 创建失败的Result
    /// </summary>
    public static Result<T> Err(Exception error)
    {
        return new Result<T>(Result<T, Exception>.Err(error));
    }

    /// <summary>
    /// 从可能抛出异常的操作创建Result
    /// </summary>
    public static Result<T> Try(Func<T> func)
    {
        try
        {
            return Ok(func());
        }
        catch (Exception ex)
        {
            return Err(ex);
        }
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsOk => _inner.IsOk;

    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsErr => _inner.IsErr;

    /// <summary>
    /// 获取成功值，如果失败则抛出异常
    /// </summary>
    public T OkValue => _inner.OkValue;

    /// <summary>
    /// 获取错误值，如果成功则抛出异常
    /// </summary>
    public Exception ErrValue => _inner.ErrValue;

    /// <summary>
    /// 尝试获取成功值
    /// </summary>
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public bool TryGetOk([MaybeNullWhen(false)] out T value) => _inner.TryGetOk(out value);
#else
    public bool TryGetOk(out T? value) => _inner.TryGetOk(out value);
#endif

    /// <summary>
    /// 尝试获取错误值
    /// </summary>
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public bool TryGetErr([MaybeNullWhen(false)] out Exception error) => _inner.TryGetErr(out error);
#else
    public bool TryGetErr(out Exception? error) => _inner.TryGetErr(out error);
#endif

    /// <summary>
    /// 映射成功值
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) => new(_inner.Map(mapper));

    /// <summary>
    /// 映射错误值
    /// </summary>
    public Result<T> MapErr(Func<Exception, Exception> mapper) => new(_inner.MapErr(mapper));

    /// <summary>
    /// 绑定操作（扁平映射）
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) => new(_inner.Bind(x => binder(x)._inner));

    /// <summary>
    /// 提供默认值
    /// </summary>
    public T UnwrapOr(T defaultValue) => _inner.UnwrapOr(defaultValue);

    /// <summary>
    /// 通过工厂函数提供默认值
    /// </summary>
    public T UnwrapOrElse(Func<Exception, T> factory) => _inner.UnwrapOrElse(factory);

    /// <summary>
    /// 获取成功值或抛出异常
    /// </summary>
    public T Expect(string message) => _inner.Expect(message);

    /// <summary>
    /// 获取错误值或抛出异常
    /// </summary>
    public Exception ExpectErr(string message) => _inner.ExpectErr(message);

    /// <summary>
    /// 转换为Option
    /// </summary>
    public Option<T> OkToOption() => _inner.OkToOption();

    /// <summary>
    /// 将错误转换为Option
    /// </summary>
    public Option<Exception> ErrToOption() => _inner.ErrToOption();

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public bool Equals(Result<T> other) => _inner.Equals(other._inner);

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode() => _inner.GetHashCode();

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString() => _inner.ToString();

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
}

/// <summary>
/// Result辅助类
/// </summary>
public static class Result
{
    /// <summary>
    /// 创建成功的Result
    /// </summary>
    public static Result<TOk, TErr> Ok<TOk, TErr>(TOk value) => Result<TOk, TErr>.Ok(value);

    /// <summary>
    /// 创建失败的Result
    /// </summary>
    public static Result<TOk, TErr> Err<TOk, TErr>(TErr error) => Result<TOk, TErr>.Err(error);

    /// <summary>
    /// 创建成功的Result（简化版）
    /// </summary>
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    /// <summary>
    /// 创建失败的Result（简化版）
    /// </summary>
    public static Result<T> Err<T>(Exception error) => Result<T>.Err(error);

    /// <summary>
    /// 从可能抛出异常的操作创建Result
    /// </summary>
    public static Result<T> Try<T>(Func<T> func) => Result<T>.Try(func);

    /// <summary>
    /// 从可能抛出异常的操作创建Result（无返回值）
    /// </summary>
    public static Result<Unit> Try(Action action)
    {
        try
        {
            action();
            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Err(ex);
        }
    }
}

/// <summary>
/// 表示无返回值的类型
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Unit的唯一值
    /// </summary>
    public static Unit Value => default;

    /// <summary>
    /// 判断是否相等（Unit总是相等）
    /// </summary>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// 获取哈希码（总是0）
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// 相等运算符（总是相等）
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// 不等运算符（总是不等）
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
