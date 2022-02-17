namespace IFoxCAD.Basal;

public static class LinkedListEx
{
    /// <summary>
    /// 获取节点
    /// </summary>
    /// <param name="func">节点数据比较委托</param>
    /// <returns>节点</returns>
    public static LinkedListNode<T>? GetNode<T>(this LinkedList<T> linkedList, Func<T, bool> func)
    {
        var node = linkedList.First;
        if (node != null)
        {
            for (int i = 0; i < linkedList.Count; i++)
            {
                if (func(node.Value))
                {
                    return node;
                }
                node = node.Next;
            }
            return null;
        }

        return null;
    }

    public static LinkedListNode<T> Add<T>(this LinkedList<T> linkedList, T value)
    {
        return linkedList.AddLast(value);
    }

    /// <summary>
    /// 获取节点的查询器
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<LinkedListNode<T>> GetNodes<T>(this LinkedList<T> linkedList)
    {
        var node = linkedList.First;
        for (int i = 0; i < linkedList.Count; i++)
        {
            yield return node;
            node = node.Next;
        }
    }


    /// <summary>
    /// 获取当前节点的临近节点
    /// </summary>
    /// <param name="forward">搜索方向标志，<see langword="true"/>为向前搜索，<see langword="false"/>为向后搜索</param>
    /// <returns>节点</returns>
    public static LinkedListNode<T> GetNext<T>(this LinkedListNode<T> node, bool forward)
    {
        return forward ? node.Next : node.Previous;
    }
}

