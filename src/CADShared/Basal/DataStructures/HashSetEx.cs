using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 容器扩展
/// </summary>
public static class HashSetExtensions
{
    /// <summary>
    /// 添加容器元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="hashSet">目标HashSet</param>
    /// <param name="collection">容器</param>
    /// <returns>添加元素数量</returns>
    public static int Add<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        if (hashSet == null)
            throw new System.ArgumentNullException(nameof(hashSet));

        if (collection == null)
            throw new System.ArgumentNullException(nameof(collection));

        int countBefore = hashSet.Count;

        foreach (T item in collection)
        {
            hashSet.Add(item);
        }

        return hashSet.Count - countBefore;
    }

    /// <summary>
    /// 移除容器中的元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="hashSet">目标HashSet</param>
    /// <param name="collection">容器</param>
    /// <returns>移除元素数量</returns>
    public static int Remove<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        if (hashSet == null)
            throw new System.ArgumentNullException(nameof(hashSet));

        if (collection == null)
            throw new System.ArgumentNullException(nameof(collection));

        int countBefore = hashSet.Count;

        foreach (T item in collection)
        {
            hashSet.Remove(item);
        }

        return countBefore - hashSet.Count;
    }
}