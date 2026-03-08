using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IFoxFunKit;

/// <summary>
/// Option类型，表示可能有值的计算结果（类似F#的Option）
/// </summary>
/// <typeparam name="T">值的类型</typeparam>
[SkipMustHandleCheck]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;
    private readonly bool _isSome;

    private Option(T? value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    /// <summary>
    /// 创建一个包含值的Option
    /// </summary>
    public static Option<T> Some(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Some不能包含null值，请使用None表示无值");
        return new Option<T>(value, true);
    }

    /// <summary>
    /// 表示无值的Option
    /// </summary>
    public static Option<T> None => new(default, false);

    /// <summary>
    /// 是否有值
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.StatusCheck)]
    public bool IsSome => _isSome;

    /// <summary>
    /// 是否无值
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.StatusCheck)]
    public bool IsNone => !_isSome;

    /// <summary>
    /// 获取值，如果无值则运行时抛出异常
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.ValueAccess)]
    public T Value => _isSome ? _value! : throw new InvalidOperationException("Option不包含值");

    /// <summary>
    /// 尝试获取值
    /// </summary>
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    [MustHandleMember(MustHandleMemberKind.TryGet)]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
#else
    [MustHandleMember(MustHandleMemberKind.TryGet)]
    public bool TryGetValue(out T value)
#endif
    {
        value = _value!;
        return _isSome;
    }


    /// <summary>
    /// 映射操作
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.Transform)]
    public Option<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return _isSome ? Option<TResult>.Some(mapper(_value!)) : Option<TResult>.None;
    }

    /// <summary>
    /// 绑定操作（扁平映射）
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.Transform)]
    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
    {
        return _isSome ? binder(_value!) : Option<TResult>.None;
    }

    /// <summary>
    /// 提供默认值
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.DefaultValue)]
    public T UnwrapOr(T defaultValue)
    {
        return _isSome ? _value! : defaultValue;
    }

    /// <summary>
    /// 通过工厂函数提供默认值
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.DefaultValue)]
    public T UnwrapOrElse(Func<T> factory)
    {
        return _isSome ? _value! : factory();
    }

    /// <summary>
    /// 过滤操作
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.Transform)]
    public Option<T> Filter(Func<T, bool> predicate)
    {
        return _isSome && predicate(_value!) ? this : None;
    }

    /// <summary>
    /// 转换为Result
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.General)]
    public Result<T, TError> OkOr<TError>(TError error)
    {
        return _isSome ? Result<T, TError>.Ok(_value!) : Result<T, TError>.Err(error);
    }

    /// <summary>
    /// 通过工厂函数转换为Result
    /// </summary>
    [MustHandleMember(MustHandleMemberKind.General)]
    public Result<T, TError> OkOrElse<TError>(Func<TError> errorFactory)
    {
        return _isSome ? Result<T, TError>.Ok(_value!) : Result<T, TError>.Err(errorFactory());
    }

    /// <summary>
    /// 隐式转换从T到Option&lt;T&gt;
    /// </summary>
    public static implicit operator Option<T>(T? value)
    {
        return value is null ? None : Some(value);
    }

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Option<T> other && Equals(other);
    }

    /// <summary>
    /// 判断是否相等
    /// </summary>
    public bool Equals(Option<T> other)
    {
        if (_isSome != other._isSome) return false;
        if (!_isSome) return true;
#pragma warning disable CS8604
        return EqualityComparer<T>.Default.Equals(_value, other._value);
#pragma warning restore CS8604
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode()
    {
        if (!_isSome) return 0;
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(_value);
#else
        return _value?.GetHashCode() ?? 0;
#endif
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        return _isSome ? $"Some({_value})" : "None";
    }

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

/// <summary>
/// Option辅助类
/// </summary>
[SkipMustHandleCheck]
public static class Option
{
    /// <summary>
    /// 创建Some值
    /// </summary>
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);

    /// <summary>
    /// 创建None值
    /// </summary>
    public static Option<T> None<T>() => Option<T>.None;

    /// <summary>
    /// 从可空类型创建Option
    /// </summary>
    public static Option<T> FromNullable<T>(T? value) where T : class
    {
        return value is null ? Option<T>.None : Option<T>.Some(value);
    }

    /// <summary>
    /// 从可空值类型创建Option
    /// </summary>
    public static Option<T> FromNullable<T>(T? value) where T : struct
    {
        return value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None;
    }
}
