namespace IFoxCAD.Basal;

/// <summary>
/// linq 扩展类
/// </summary>
public static class LinqEx
{
    #region FindByMax

    /// <summary>
    /// 按转换函数找出序列中最大键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="func">转换函数</param>
    /// <returns>最大键值的对应值</returns>
    public static TValue FindByMax<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;
        var key = func(value);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(key) <= 0) continue;
            key = tKey;
            value = itr.Current;
        }
        return value;
    }

    /// <summary>
    /// 按转换函数找出序列中最大键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="maxResult">对应的最大键值</param>
    /// <param name="func">转换函数</param>
    /// <returns>最大键值的对应值</returns>
    public static TValue FindByMax<TValue, TKey>(this IEnumerable<TValue> source, out TKey maxResult, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;
        var key = func(value);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(key) <= 0) continue;
            key = tKey;
            value = itr.Current;
        }
        maxResult = key;
        return value;
    }

    /// <summary>
    /// 按比较器找出序列中最大键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <param name="source">序列</param>
    /// <param name="comparison">比较器</param>
    /// <returns>最大键值的对应值</returns>
    public static TValue FindByMax<TValue>(this IEnumerable<TValue> source, Comparison<TValue> comparison)
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;

        while (itr.MoveNext())
        {
            if (comparison(itr.Current, value) > 0)
                value = itr.Current;
        }
        return value;
    }

    #endregion FindByMax

    #region FindByMin

    /// <summary>
    /// 按转换函数找出序列中最小键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="minKey">对应的最小键值</param>
    /// <param name="func">转换函数</param>
    /// <returns>最小键值的对应值</returns>
    public static TValue FindByMin<TValue, TKey>(this IEnumerable<TValue> source, out TKey minKey, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;
        var key = func(value);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(key) >= 0) continue;
            key = tKey;
            value = itr.Current;
        }
        minKey = key;
        return value;
    }

    /// <summary>
    /// 按转换函数找出序列中最小键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="func">转换函数</param>
    /// <returns>最小键值的对应值</returns>
    public static TValue FindByMin<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;
        var key = func(value);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(key) >= 0) continue;
            key = tKey;
            value = itr.Current;
        }
        return value;
    }

    /// <summary>
    /// 按比较器找出序列中最小键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <param name="source">序列</param>
    /// <param name="comparison">比较器</param>
    /// <returns>最小键值的对应值</returns>
    public static TValue FindByMin<TValue>(this IEnumerable<TValue> source, Comparison<TValue> comparison)
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var value = itr.Current;

        while (itr.MoveNext())
        {
            if (comparison(itr.Current, value) < 0)
                value = itr.Current;
        }
        return value;
    }

    #endregion FindByMin

    #region FindByExt

    /// <summary>
    /// 按转换函数找出序列中最(小/大)键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="func">转换函数</param>
    /// <returns>最(小/大)键值的对应值</returns>
    public static TValue[] FindByExt<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var values = new TValue[2];
        values[0] = values[1] = itr.Current;

        var keys = new TKey[2];
        keys[0] = keys[1] = func(itr.Current);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(keys[0]) < 0)
            {
                keys[0] = tKey;
                values[0] = itr.Current;
            }
            else if (tKey.CompareTo(keys[1]) > 0)
            {
                keys[1] = tKey;
                values[1] = itr.Current;
            }
        }
        return values;
    }

    /// <summary>
    /// 按比较器找出序列中最(小/大)键值的对应值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <param name="source">序列</param>
    /// <param name="comparison">比较器</param>
    /// <returns>最(小/大)键值的对应值</returns>
    public static TValue[] FindByExt<TValue>(this IEnumerable<TValue> source, Comparison<TValue> comparison)
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var values = new TValue[2];
        values[0] = values[1] = itr.Current;

        while (itr.MoveNext())
        {
            if (comparison(itr.Current, values[0]) < 0)
                values[0] = itr.Current;
            else if (comparison(itr.Current, values[1]) > 0)
                values[1] = itr.Current;
        }
        return values;
    }

    /// <summary>
    /// 按转换函数找出序列中最(小/大)键值的对应键值
    /// </summary>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TKey">键</typeparam>
    /// <param name="source">序列</param>
    /// <param name="func">转换函数</param>
    /// <returns>最(小/大)键值</returns>
    public static TKey[] FindExt<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> func)
        where TKey : IComparable<TKey>
    {
        using var itr = source.GetEnumerator();
        if (!itr.MoveNext())
            throw new ArgumentNullException(nameof(source), "对象为 null");

        var keys = new TKey[2];
        keys[0] = keys[1] = func(itr.Current);

        while (itr.MoveNext())
        {
            var tKey = func(itr.Current);
            if (tKey.CompareTo(keys[0]) < 0)
                keys[0] = tKey;
            else if (tKey.CompareTo(keys[1]) > 0)
                keys[1] = tKey;
        }
        return keys;
    }

    #endregion FindByExt

    #region Order

    /// <summary>
    /// 自定义的比较泛型类
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    private class SpecComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comp;

        internal SpecComparer(Comparison<T> comp)
        {
            _comp = comp;
        }

        #region IComparer<T> 成员
        
        public int Compare(T? x, T? y)
        {
            if (x is not null && y is not null)
                return _comp(x, y);
            return 0;
        }
        #endregion IComparer<T> 成员
    }

    /// <summary>
    /// 使用指定的比较器将序列按升序排序
    /// </summary>
    /// <typeparam name="T">输入泛型</typeparam>
    /// <typeparam name="TKey">输出泛型</typeparam>
    /// <param name="source">序列</param>
    /// <param name="keySelector">用于从元素中提取键的函数</param>
    /// <param name="comparison">比较器</param>
    /// <returns>排序的序列</returns>
    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, Comparison<TKey> comparison)
    {
        return source.OrderBy(keySelector, new SpecComparer<TKey>(comparison));
    }

    /// <summary>
    /// 使用指定的比较器将其后的序列按升序排序
    /// </summary>
    /// <typeparam name="T">输入泛型</typeparam>
    /// <typeparam name="TKey">输出泛型</typeparam>
    /// <param name="source">序列</param>
    /// <param name="keySelector">用于从元素中提取键的函数</param>
    /// <param name="comparison">比较器</param>
    /// <returns>排序的序列</returns>
    public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> keySelector, Comparison<TKey> comparison)
    {
        return source.ThenBy(keySelector, new SpecComparer<TKey>(comparison));
    }

    #endregion Order
}
