namespace IFoxCAD.Cad;

/// <summary>
/// 选择集扩展类
/// </summary>
public static class SelectionSetEx
{
    #region 获取对象id
    /// <summary>
    /// 获取已选择的对象
    /// </summary>
    /// <param name="ss">选择集</param>
    /// <returns>已选择的对象集合</returns>
    public static IEnumerable<SelectedObject> GetSelectedObjects(this SelectionSet ss)
    {
        return ss.Cast<SelectedObject>();
    }

    /// <summary>
    /// 获取已选择的对象
    /// </summary>
    /// <typeparam name="T">已选择的对象泛型</typeparam>
    /// <param name="ss">选择集</param>
    /// <returns>已选择的对象集合</returns>
    public static IEnumerable<T> GetSelectObjects<T>(this SelectionSet ss) where T : SelectedObject
    {
        return ss.Cast<SelectedObject>().OfType<T>();
    }

    /// <summary>
    /// 从选择集中获取对象id
    /// </summary>
    /// <typeparam name="T">图元类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <returns>已选择的对象id集合</returns>
    public static IEnumerable<ObjectId> GetObjectIds<T>(this SelectionSet ss) where T : Entity
    {
        string dxfName = RXClass.GetClass(typeof(T)).DxfName;
        return
            ss
            .GetObjectIds()
            .Where(id => id.ObjectClass.DxfName == dxfName);
    }

    /// <summary>
    /// 将选择集的对象按类型分组
    /// </summary>
    /// <param name="ss">选择集</param>
    /// <returns>分组后的类型/对象id集合</returns>
    public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIdGroup(this SelectionSet ss)
    {
        return
            ss
            .GetObjectIds()
            .GroupBy(id => id.ObjectClass.DxfName);
    }
    #endregion

    #region 获取实体对象

    /// <summary>
    /// 获取指定类型图元
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <param name="tr">事务</param>
    /// <param name="openMode">打开模式</param>
    /// <returns>图元集合</returns>
    public static IEnumerable<T> GetEntities<T>(this SelectionSet ss, OpenMode openMode = OpenMode.ForRead, Transaction tr = default) where T : Entity
    {
        tr ??= DBTrans.Top.Transaction;
        return
            ss
            .GetObjectIds()
            .Select(id => tr.GetObject(id, openMode) as T);
    }

    #endregion

    #region ForEach

    /// <summary>
    /// 遍历选择集
    /// </summary>
    /// <typeparam name="T">指定图元类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <param name="tr">事务</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="action">处理函数</param>
    public static void ForEach<T>(this SelectionSet ss, Action<T> action, OpenMode openMode = OpenMode.ForRead, Transaction tr = default) where T : Entity
    {
        foreach (T ent in ss.GetEntities<T>(openMode, tr))
        {
            action?.Invoke(ent);
        }
    }
    #endregion
}
