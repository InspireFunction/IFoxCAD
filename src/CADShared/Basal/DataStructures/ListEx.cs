namespace IFoxCAD.Basal;


/// <summary>
/// 提供用于列表操作的扩展方法。
/// </summary>
public static class ListEx
{
    /// <summary>
    /// 比较两个列表是否相等。
    /// </summary>
    /// <typeparam name="T">列表中的元素类型。</typeparam>
    /// <param name="a">要比较的第一个列表。</param>
    /// <param name="b">要比较的第二个列表。</param>
    /// <returns>如果两个列表相等，则为 true；否则为 false。</returns>
    public static bool EqualsAll<T>(this IList<T> a, IList<T> b)
    {
        return EqualsAll(a, b, null);
        // there is a slight performance gain in passing null here.
        // It is how it is done in other parts of the framework.
    }

    /// <summary>
    /// 使用指定的比较器比较两个列表是否相等。
    /// </summary>
    /// <typeparam name="T">列表中的元素类型。</typeparam>
    /// <param name="a">要比较的第一个列表。</param>
    /// <param name="b">要比较的第二个列表。</param>
    /// <param name="comparer">用于比较元素的比较器。</param>
    /// <returns>如果两个列表相等，则为 true；否则为 false。</returns>
    public static bool EqualsAll<T>(this IList<T> a, IList<T> b, IEqualityComparer<T>? comparer)
    {
        if (a is null)
            return b is null;
        else if (b is null)
            return false;

        if (a.Count != b.Count)
            return false;

        comparer ??= EqualityComparer<T>.Default;

        for (int i = 0; i < a.Count; i++)
            if (!comparer.Equals(a[i], b[i]))
                return false;
        return true;
    }
}
