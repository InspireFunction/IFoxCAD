namespace IFoxCAD.Basal;

/// <summary>
/// 表示一个保持插入顺序的哈希集合。
/// </summary>
/// <typeparam name="T">集合中的元素类型。</typeparam>
public class LinkedHashSet<T> : ICollection<T> where T : IComparable
{
    private readonly IDictionary<T, LoopListNode<T>> m_Dictionary;
    private readonly LoopList<T> m_LinkedList;

    /// <summary>
    /// 初始化 <see cref="LinkedHashSet{T}"/> 类的新实例。
    /// </summary>
    public LinkedHashSet()
    {
        m_Dictionary = new Dictionary<T, LoopListNode<T>>();
        m_LinkedList = new LoopList<T>();
    }

    /// <summary>
    /// 获取集合中的第一个节点。
    /// </summary>
    public LoopListNode<T>? First => m_LinkedList.First;

    /// <summary>
    /// 获取集合中的最后一个节点。
    /// </summary>
    public LoopListNode<T>? Last => m_LinkedList.Last;

    /// <summary>
    /// 获取或设置最小节点。
    /// </summary>
    public LoopListNode<T>? MinNode { get; set; }

    /// <summary>
    /// 将指定项添加到集合中。
    /// </summary>
    /// <param name="item">要添加到集合的对象。</param>
    /// <returns>如果项已添加到集合中，则为 true；否则为 false。</returns>
    public bool Add(T item)
    {
        if (m_Dictionary.ContainsKey(item))
            return false;
        var node = m_LinkedList.AddLast(item);
        m_Dictionary.Add(item, node);

        if (MinNode is null)
        {
            MinNode = node;
        }
        else
        {
            if (item.CompareTo(MinNode.Value) < 0)
            {
                MinNode = node;
            }
        }



        return true;
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    /// <summary>
    /// 在集合开头添加指定值。
    /// </summary>
    /// <param name="value">要添加的值。</param>
    /// <returns>新添加的节点。</returns>
    public LoopListNode<T> AddFirst(T value)
    {
        if (m_Dictionary.ContainsKey(value))
        {
            return m_Dictionary[value];
        }
        var node = m_LinkedList.AddFirst(value);
        m_Dictionary.Add(value, node);
        if (MinNode is null)
        {
            MinNode = node;
        }
        else
        {
            if (value.CompareTo(MinNode.Value) < 0)
            {
                MinNode = node;
            }
        }
        return node;
    }

    /// <summary>
    /// 将指定集合中的所有项添加到集合中。
    /// </summary>
    /// <param name="collection">要添加的项的集合。</param>
    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }


    /// <summary>
    /// 从集合中移除所有项。
    /// </summary>
    public void Clear()
    {
        m_LinkedList.Clear();
        m_Dictionary.Clear();
    }

    /// <summary>
    /// 从集合中移除指定项。
    /// </summary>
    /// <param name="item">要移除的项。</param>
    /// <returns>如果成功移除项，则为 true；否则为 false。</returns>
    public bool Remove(T item)
    {
        bool found = m_Dictionary.TryGetValue(item, out LoopListNode<T> node);
        if (!found) return false;
        m_Dictionary.Remove(item);
        m_LinkedList.Remove(node);
        return true;
    }

    /// <summary>
    /// 获取集合中包含的元素数量。
    /// </summary>
    public int Count
    {
        get { return m_Dictionary.Count; }
    }

    /// <summary>
    /// 对集合中的节点执行指定操作。
    /// </summary>
    /// <param name="from">起始节点。</param>
    /// <param name="action">要对每个节点执行的操作。</param>
    public void For(LoopListNode<T> from, Action<int, T, T> action)
    {
        var first = from;
        var last = from;
        if (first is null) return;

        for (int i = 0; i < Count; i++)
        {
            action.Invoke(i, first!.Value, last!.Value);
            first = first.Next;
            last = last.Previous;
        }
    }

    /// <summary>
    /// 将集合中的元素复制到新列表中。
    /// </summary>
    /// <returns>包含集合元素的新列表。</returns>
    public List<T> ToList()
    {
        return m_LinkedList.ToList();
    }

    /// <summary>
    /// 返回循环访问集合的枚举器。
    /// </summary>
    /// <returns>用于循环访问集合的枚举器。</returns>
    [System.Diagnostics.DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        return m_LinkedList.GetEnumerator();
    }

    /// <summary>
    /// 返回循环访问集合的枚举器。
    /// </summary>
    /// <returns>用于循环访问集合的枚举器。</returns>
    [System.Diagnostics.DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    /// <summary>
    /// 确定集合是否包含指定项。
    /// </summary>
    /// <param name="item">要在集合中查找的项。</param>
    /// <returns>如果集合包含指定项，则为 true；否则为 false。</returns>
    public bool Contains(T item)
    {
        return m_Dictionary.ContainsKey(item);
    }

    /// <summary>
    /// 将集合中的元素复制到指定的数组中。
    /// </summary>
    /// <param name="array">复制元素的目标数组。</param>
    /// <param name="arrayIndex">复制开始处的索引。</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        // m_LinkedList.CopyTo(array, arrayIndex);
        return;
    }

    /// <summary>
    /// 将指定节点设置为集合的第一个节点。
    /// </summary>
    /// <param name="node">要设置为第一个节点的节点。</param>
    /// <returns>如果操作成功，则为 true；否则为 false。</returns>
    public bool SetFirst(LoopListNode<T> node)
    {
        return m_LinkedList.SetFirst(node);
    }

    /// <summary>
    /// 创建集合的浅拷贝副本。
    /// </summary>
    /// <returns>集合的浅拷贝。</returns>
    public LinkedHashSet<T> Clone()
    {
        var newset = new LinkedHashSet<T>();
        foreach (var item in this)
        {
            newset.Add(item);
        }
        return newset;
    }

    /// <summary>
    /// 获取一个值，该值指示集合是否为只读。
    /// </summary>
    public virtual bool IsReadOnly
    {
        get { return m_Dictionary.IsReadOnly; }
    }

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
        return m_LinkedList.ToString();
    }

    /// <summary>
    /// 将当前集合与指定集合取并集。
    /// </summary>
    /// <param name="other">要与其他集合取并集的集合。</param>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public void UnionWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 仅保留当前集合中也在指定集合中的元素。
    /// </summary>
    /// <param name="other">要与当前集合取交集的集合。</param>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public void IntersectWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 从当前集合中移除指定集合中的所有元素。
    /// </summary>
    /// <param name="other">要从中移除的元素的集合。</param>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public void ExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合是否为指定集合的子集。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合是指定集合的子集，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 从当前集合中移除与指定集合的交集。
    /// </summary>
    /// <param name="other">要从中移除的元素的集合。</param>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合是否为指定集合的超集。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合是指定集合的超集，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合是否为指定集合的真超集。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合是指定集合的真超集，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合是否为指定集合的真子集。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合是指定集合的真子集，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合与指定集合是否共享任何元素。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合与指定集合共享任何元素，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool Overlaps(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    /// <summary>
    /// 确定当前集合与指定集合是否包含相同的元素。
    /// </summary>
    /// <param name="other">要与当前集合进行比较的集合。</param>
    /// <returns>如果当前集合与指定集合包含相同的元素，则为 true；否则为 false。</returns>
    /// <exception cref="NotSupportedException">此方法不支持。</exception>
    public bool SetEquals(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    private static Exception GetNotSupportedDueToSimplification()
    {
        return new NotSupportedException("This method is not supported due to simplification of example code.");
    }
}