namespace IFoxCAD.Basal;

public class LinkedHashSet<T> : ICollection<T> where T : IComparable
{
    private readonly IDictionary<T, LoopListNode<T>> m_Dictionary;
    private readonly LoopList<T> m_LinkedList;

    public LinkedHashSet()
    {
        m_Dictionary = new Dictionary<T, LoopListNode<T>>();
        m_LinkedList = new LoopList<T>();
    }

    public LoopListNode<T>? First => m_LinkedList.First;

    public LoopListNode<T>? Last => m_LinkedList.Last;

    public LoopListNode<T>? MinNode { get; set; }

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

    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }


    public void Clear()
    {
        m_LinkedList.Clear();
        m_Dictionary.Clear();
    }

    public bool Remove(T item)
    {
        bool found = m_Dictionary.TryGetValue(item, out LoopListNode<T> node);
        if (!found) return false;
        m_Dictionary.Remove(item);
        m_LinkedList.Remove(node);
        return true;
    }

    public int Count
    {
        get { return m_Dictionary.Count; }
    }

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

    public List<T> ToList()
    {
        return m_LinkedList.ToList();
    }

    [System.Diagnostics.DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        return m_LinkedList.GetEnumerator();
    }

    [System.Diagnostics.DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public bool Contains(T item)
    {
        return m_Dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        // m_LinkedList.CopyTo(array, arrayIndex);
        return;
    }

    public bool SetFirst(LoopListNode<T> node)
    {
        return m_LinkedList.SetFirst(node);
    }

    public LinkedHashSet<T> Clone()
    {
        var newset = new LinkedHashSet<T>();
        foreach (var item in this)
        {
            newset.Add(item);
        }
        return newset;
    }

    public virtual bool IsReadOnly
    {
        get { return m_Dictionary.IsReadOnly; }
    }

    public override string ToString()
    {
        return m_LinkedList.ToString();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    private static Exception GetNotSupportedDueToSimplification()
    {
        return new NotSupportedException("This method is not supported due to simplification of example code.");
    }
}