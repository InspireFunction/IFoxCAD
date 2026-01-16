
#if !NET8_0_OR_GREATER
namespace IFoxCAD.Basal;

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
    private static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}
#endif