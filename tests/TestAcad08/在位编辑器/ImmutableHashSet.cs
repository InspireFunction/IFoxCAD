namespace Test;

/// <summary>
/// 只读包装器，提供不可变集合接口
/// </summary>
public class ImmutableHashSet<T> : IEnumerable<T>
{
    private readonly HashSet<T> _set;

    public ImmutableHashSet(HashSet<T> set)
    {
        if (set == null)
            throw new ArgumentNullException(nameof(set));
        _set = [.. set];
    }

    public ImmutableHashSet(IEnumerable<T> collection)
    {
        _set = [.. collection];
    }

    public int Count => _set.Count;

    public bool Contains(T item) => _set.Contains(item);

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

    /// <summary>
    /// 创建可变的副本
    /// </summary>
    public HashSet<T> ToMutable() => [.. _set];

    /// <summary>
    /// 添加元素，返回新的不可变集合
    /// </summary>
    public ImmutableHashSet<T> Add(T item)
    {
        var newSet = new HashSet<T>(_set);
        newSet.Add(item);
        return new ImmutableHashSet<T>(newSet);
    }

    /// <summary>
    /// 移除元素，返回新的不可变集合
    /// </summary>
    public ImmutableHashSet<T> Remove(T item)
    {
        var newSet = new HashSet<T>(_set);
        newSet.Remove(item);
        return new ImmutableHashSet<T>(newSet);
    }

    /// <summary>
    /// 批量添加元素，返回新的不可变集合
    /// </summary>
    public ImmutableHashSet<T> AddRange(IEnumerable<T> items)
    {
        var newSet = new HashSet<T>(_set);
        foreach (var item in items)
            newSet.Add(item);
        return new ImmutableHashSet<T>(newSet);
    }

    /// <summary>
    /// 批量移除元素，返回新的不可变集合
    /// </summary>
    public ImmutableHashSet<T> RemoveRange(IEnumerable<T> items)
    {
        var newSet = new HashSet<T>(_set);
        foreach (var item in items)
            newSet.Remove(item);
        return new ImmutableHashSet<T>(newSet);
    }

    public static ImmutableHashSet<T> Empty => new([]);
}
