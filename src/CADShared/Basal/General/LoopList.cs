namespace IFoxCAD.Basal;

#line hidden // 调试的时候跳过它

/// <summary>
/// 环链表节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoopListNode<T>
{
    #region 成员
    /// <summary>
    /// 取值
    /// </summary>
    public T Value;

    /// <summary>
    /// 上一个节点
    /// </summary>
    public LoopListNode<T>? Previous { internal set; get; }

    /// <summary>
    /// 下一个节点
    /// </summary>
    public LoopListNode<T>? Next { internal set; get; }

    /// <summary>
    /// 环链表序列
    /// </summary>
    public LoopList<T>? List { internal set; get; }
    #endregion

    #region 构造
    /// <summary>
    /// 环链表节点构造函数
    /// </summary>
    /// <param name="value">节点值</param>
    /// <param name="ts">环链表</param>
    public LoopListNode(T value, LoopList<T> ts)
    {
        Value = value;
        List = ts;
    }

    /// <summary>
    /// 获取当前节点的临近节点
    /// </summary>
    /// <param name="forward">搜索方向标志,<see langword="true"/>为向前搜索,<see langword="false"/>为向后搜索</param>
    /// <returns></returns>
    public LoopListNode<T>? GetNext(bool forward)
    {
        return forward ? Next : Previous;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 无效化成员
    /// </summary>
    internal void Invalidate()
    {
        List = null;
        Next = null;
        Previous = null;
    }
    #endregion
}

/// <summary>
/// 环链表
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoopList<T> : IEnumerable<T>, IFormattable
{
    #region 成员

    /// <summary>
    /// 节点数
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// 首节点
    /// </summary>
    public LoopListNode<T>? First { get; private set; }

    /// <summary>
    /// 尾节点
    /// </summary>
    public LoopListNode<T>? Last => First?.Previous;




    #endregion

    #region 构造

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public LoopList() { }

    /// <summary>
    /// 环链表构造函数
    /// </summary>
    /// <param name="values">节点迭代器</param>
    [DebuggerStepThrough]
    public LoopList(IEnumerable<T> values)
    {
        using var ge = values.GetEnumerator();
        while (ge.MoveNext())
            Add(ge.Current);
    }

    #endregion

    #region 方法

    /// <summary>
    /// 设置首节点
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns></returns>
    public bool SetFirst(LoopListNode<T> node)
    {
        if (!Contains(node))
            return false;

        First = node;
        return true;
    }

    /// <summary>
    /// 交换两个节点的值
    /// </summary>
    /// <param name="node1">第一个节点</param>
    /// <param name="node2">第二个节点</param>
    public void Swap(LoopListNode<T> node1, LoopListNode<T> node2)
    {
        (node2.Value, node1.Value) = (node1.Value, node2.Value);
    }

    /// <summary>
    /// 链内翻转
    /// </summary>
    public void Reverse()
    {
        var first = First;
        if (first is null)
            return;
        var last = Last;
        for (var i = 0; i < Count / 2; i++)
        {
            Swap(first!, last!);
            first = first!.Next;
            last = last!.Previous;
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        // 移除头部,表示链表再也无法遍历得到
        First = null;
        Count = 0;
    }

    /// <summary>
    /// 从头遍历_非迭代器(此处和通用ForEach冲突,所以内部用)
    /// </summary>
    /// <param name="action"></param>
    void ForEach(Func<LoopListNode<T>, bool> action)
    {
        var node = First;
        if (node is null)
            return;
        for (var i = 0; i < Count; i++)
        {
            if (action(node!))
                break;
            node = node!.Next;
        }
    }

    /// <summary>
    /// 从头遍历_非迭代器(扔出计数)
    /// </summary>
    /// <param name="action"></param>
    public void For(Func<int, LoopListNode<T>, bool> action)
    {
        var node = First;
        if (node is null)
            return;
        for (var i = 0; i < Count; i++)
        {
            if (action(i, node!))
                break;
            node = node!.Next;
        }
    }

    #region Contains

    /// <summary>
    /// 是否包含节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool Contains(LoopListNode<T> node)
    {
        return node.List == this;
    }

    /// <summary>
    /// 是否包含值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(T value)
    {
        var result = false;
        ForEach(node => {
            if (!node.Value!.Equals(value)) return false;
            result = true;
            return true;
        });
        return result;
    }

    /// <summary>
    /// 查找第一个出现的节点
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public LoopListNode<T>? Find(T value)
    {
        // LoopListNode<T> result = null;
        // ForEach(node =>
        // {
        //    if (node.Value.Equals(t2))
        //    {
        //        result = node;
        //        return true;
        //    }
        //    return false;
        // });
        // return result;

        LoopListNode<T>? node = First;
        var c = EqualityComparer<T>.Default;
        if (node is not null)
        {
            if (value is not null)
            {
                do
                {
                    if (c.Equals(node!.Value, value))
                        return node;
                    node = node.Next;
                } while (node != First);
            }
            else
            {
                do
                {
                    if (node!.Value is null)
                        return node;
                    node = node.Next;
                } while (node != First);
            }
        }
        return null;
    }

    /// <summary>
    /// 查找所有出现的节点
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public IEnumerable<LoopListNode<T>>? Finds(T value)
    {
        var node = First;
        if (node is null)
            return null;

        List<LoopListNode<T>> result = [];
        var c = EqualityComparer<T>.Default;
        if (value is not null)
        {
            do
            {
                if (c.Equals(node!.Value, value))
                    result.Add(node);
                node = node.Next;
            } while (node != First);
        }
        else
        {
            do
            {
                if (node!.Value is null)
                    result.Add(node);
                node = node.Next;
            } while (node != First);
        }
        return result;
    }

    /// <summary>
    /// 获取节点
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public LoopListNode<T>? GetNode(Func<T, bool> func)
    {
        LoopListNode<T>? result = null;
        ForEach(node => {
            if (func(node.Value))
            {
                result = node;
                return true;
            }
            return false;
        });
        return result;
    }

    #endregion

    #region Add

    /// <summary>
    /// 在首节点之前插入节点,并设置新节点为首节点
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public LoopListNode<T> AddFirst(T value)
    {
        var node = new LoopListNode<T>(value, this);

        if (Count == 0)
        {
            First = node;
            First.Previous = First.Next = node;
        }
        else
        {
            LoopListNode<T> last = Last!;
            First!.Previous = last.Next = node;
            node.Next = First;
            node.Previous = last;
            First = node;
        }
        Count++;
        return First;
    }

    /// <summary>
    ///  在尾节点之后插入节点,并设置新节点为尾节点
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public LoopListNode<T> Add(T value)
    {
        var node = new LoopListNode<T>(value, this);

        if (Count == 0)
        {
            First = node;
            First.Previous = First.Next = node;
        }
        else
        {
            var last = First!.Previous!;
            First.Previous = last.Next = node;
            node.Next = First;
            node.Previous = last;
        }
        Count++;
        return Last!;
    }

    /// <summary>
    ///  在尾节点之后插入节点,并设置新节点为尾节点_此函数仅为与LinkedList同名方法
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public LoopListNode<T> AddLast(T value)
    {
        return Add(value);
    }

    /// <summary>
    /// 容器内容全部加入到末尾
    /// </summary>
    /// <param name="list"></param>
    [DebuggerStepThrough]
    public void AddRange(IEnumerable<T> list)
    {
        using var ge = list.GetEnumerator();
        while (ge.MoveNext())
            Add(ge.Current);
    }

    /// <summary>
    /// 前面增加节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LoopListNode<T> AddBefore(LoopListNode<T> node, T value)
    {
        if (node == First)
            return AddFirst(value);

        var tNode = new LoopListNode<T>(value, this);
        node.Previous!.Next = tNode;
        tNode.Previous = node.Previous;
        node.Previous = tNode;
        tNode.Next = node;
        Count++;
        return tNode;
    }

    /// <summary>
    /// 后面增加节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LoopListNode<T> AddAfter(LoopListNode<T> node, T value)
    {
        var tNode = new LoopListNode<T>(value, this);
        node.Next!.Previous = tNode;
        tNode.Next = node.Next;
        node.Next = tNode;
        tNode.Previous = node;
        Count++;
        return tNode;
    }

    #endregion

    #region Remove

    /// <summary>
    /// 删除首节点
    /// </summary>
    /// <returns></returns>
    public bool RemoveFirst()
    {
        switch (Count)
        {
            case 0:
            return false;

            case 1:
            First = null;
            break;

            default:
            LoopListNode<T> last = Last!;
            First = First!.Next;
            First!.Previous = last;
            last.Next = First;
            break;
        }
        Count--;
        return true;
    }

    /// <summary>
    /// 删除尾节点
    /// </summary>
    /// <returns></returns>
    public bool RemoveLast()
    {
        switch (Count)
        {
            case 0:
            return false;

            case 1:
            First = null;
            break;

            default:
            LoopListNode<T> last = Last!.Previous!;
            last.Next = First;
            First!.Previous = last;
            break;
        }
        Count--;
        return true;
    }

    /// <summary>
    /// 删除此参数节点(唯一)
    /// </summary>
    /// <param name="node">指定节点</param>
    /// <returns></returns>
    public bool Remove(LoopListNode<T> node)
    {
        if (!Contains(node))
            return false;
        InternalRemove(node);
        return true;
    }

    /// <summary>
    /// 删除含有此参数节点(所有)
    /// </summary>
    /// <param name="value">将移除所有含有此值</param>
    /// <returns></returns>
    public bool Remove(T value)
    {
        var lst = Finds(value);
        if (lst is null)
            return false;

        using var ge = lst.GetEnumerator();
        while (ge.MoveNext())
            if (ge.Current != null)
                InternalRemove(ge.Current);
        return true;
    }

    /// <summary>
    /// 删除节点_内部调用
    /// </summary>
    /// <param name="node">此值肯定存在当前链表</param>
    /// <returns></returns>
    void InternalRemove(LoopListNode<T> node)
    {
        if (Count == 1 || node == First)
        {
            RemoveFirst();// 此处会减数字
        }
        else
        {
            node.Next!.Previous = node.Previous;
            node.Previous!.Next = node.Next;
            Count--;
        }
        node.Invalidate();
    }

    #endregion

    #region LinkTo

    /// <summary>
    /// 链接两节点,并去除这两个节点间的所有节点
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void LinkTo(LoopListNode<T> from, LoopListNode<T> to)
    {
        if (from != to && Contains(from) && Contains(to))
        {
            LoopListNode<T> node = from.Next!;
            var isFirstChanged = false;
            var number = 0;

            while (node != to)
            {
                if (node == First)
                    isFirstChanged = true;

                node = node.Next!;
                number++;
            }

            from.Next = to;
            to.Previous = from;

            if (number > 0 && isFirstChanged)
                First = to;

            Count -= number;
        }
    }

    /// <summary>
    /// 链接两节点,并去除这两个节点间的所有节点
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="number"></param>
    public void LinkTo(LoopListNode<T> from, LoopListNode<T> to, int number)
    {
        if (from != to && Contains(from) && Contains(to))
        {
            from.Next = to;
            to.Previous = from;
            First = to;
            Count -= number;
        }
    }

    /// <summary>
    /// 链接两节点,并去除这两个节点间的所有节点
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="number"></param>
    /// <param name="isFirstChanged"></param>
    public void LinkTo(LoopListNode<T> from, LoopListNode<T> to, int number, bool isFirstChanged)
    {
        if (from != to && Contains(from) && Contains(to))
        {
            from.Next = to;
            to.Previous = from;
            if (isFirstChanged)
                First = to;
            Count -= number;
        }
    }

    #endregion

    #endregion

    #region IEnumerable<T>

    /// <summary>
    /// 获取节点的查询器
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public IEnumerable<LoopListNode<T>> GetNodes(LoopListNode<T> from)
    {
        var node = from;
        for (var i = 0; i < Count; i++)
        {
            yield return node!;
            node = node!.Next;
        }
    }

    /// <summary>
    /// 获取节点的查询器
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    public IEnumerable<LoopListNode<T>> GetNodes()
    {
        LoopListNode<T> node = First!;
        for (var i = 0; i < Count; i++)
        {
            yield return node!;
            node = node.Next!;
        }
    }

    /// <summary>
    /// 获取节点值的查询器
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        LoopListNode<T> node = First!;
        for (var i = 0; i < Count; i++)
        {
            yield return node!.Value;
            node = node.Next!;
        }
    }

    [DebuggerStepThrough]
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    #region IEnumerable 成员

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion IEnumerable 成员

    #endregion

    #region IFormattable
    /// <summary>
    /// 转换为字符串_格式化实现
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// 转换为字符串_无参调用
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ToString(null);
    }

    /// <summary>
    /// 转换为字符串_有参调用
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        s.Append($"Count = {Count};");
        if (format is not null) return s.ToString();
        s.Append("{ ");
        foreach (var value in this)
            s.Append($"{value} ");
        s.Append(" }");
        return s.ToString();
    }
    #endregion

    #region ICloneable
    /* 山人说无法分辨ICloneable接口是深浅克隆,
     * 因此不要在泛型模板实现克隆函数,
     * 让用户自己来 new(xx)实现浅克隆,所以也不提供Clone()了
     *
     * 因此约定了:CopyTo(T,index)是深克隆;MemberwiseClone()是浅克隆;
     * public object Clone()
     * {
     *     var lst = new LoopList<LoopListNode<T>>();
     *     ForEach(node => {
     *         lst.Add(node);
     *         return false;
     *     });
     *     return lst;
     * }
     */
    #endregion
}

#line default