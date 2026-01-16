#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

namespace IFoxCAD.Cad;

using System;
using System.Runtime.InteropServices;


public struct TransStatus
{
    [System.Flags]
    private enum TrStatus : byte
    {
        IsCommit = 1 << 0,    // 提交标记 (00000001)
        IsDisposed = 1 << 1   // 释放标记 (00000010)
    }

    // 私有状态字段
    private TrStatus _status;

    // 公有属性
    public bool IsCommit => _status.HasFlag(TrStatus.IsCommit);
    public bool IsAbort => !_status.HasFlag(TrStatus.IsCommit); // 未提交即为取消
    public bool IsDisposed => _status.HasFlag(TrStatus.IsDisposed);

    // 提交事务
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Commit() => _status |= TrStatus.IsCommit;

    // 取消事务
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Abort() => _status &= ~TrStatus.IsCommit;

    // 释放事务
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _status |= TrStatus.IsDisposed;

    // 隐式类型转换
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(TransStatus status)
        => (byte)status._status;

    public override string ToString()
        => $"提交标记: {IsCommit}, 释放标记: {IsDisposed}";
}



[Flags]
public enum Echo
{
    None = 0, // 不应用任何回声
    ActiveDocument = 1 << 0, // 应用前台文档的回声
    Errors = 1 << 1, // 应用报错的回声
    FatalErrors = 1 << 2, // 应用致命错误的回声
    All = ActiveDocument | Errors | FatalErrors // 启用所有回声功能
}

#if NET35
public static class EnumExtensions
{
    /// <summary>
    /// 检查枚举是否包含指定的标志
    /// .NET 3.5 兼容版本
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T 必须是枚举类型");
        }

        long longValue = value.ToInt64(null);
        long longFlag = flag.ToInt64(null);

        return (longValue & longFlag) == longFlag;
    }
}
#endif