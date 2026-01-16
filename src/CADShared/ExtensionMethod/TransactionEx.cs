namespace IFoxCAD.Cad;

/// <summary>
/// cad的事务的扩展类
/// </summary>
public static class TransactionEx
{
    /// <summary>
    /// 根据对象id获取对象
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="id">对象id</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>数据库DBObject对象</returns>
    public static DBObject GetObject(this Transaction tr, ObjectId id, OpenMode openMode = OpenMode.ForRead,
        bool openErased = false, bool openLockedLayer = false)
    {
        return tr.GetObject(id, openMode, openErased, openLockedLayer);
    }

    /// <summary>
    /// 根据对象id获取图元对象
    /// </summary>
    /// <typeparam name="T">要获取的图元对象的类型</typeparam>
    /// <param name="tr"></param>
    /// <param name="id">对象id</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>图元对象,类型不匹配时抛异常 </returns>
    public static T? GetObject<T>(this Transaction tr, ObjectId id, OpenMode openMode = OpenMode.ForRead,
        bool openErased = false, bool openLockedLayer = false) where T : DBObject
    {
        return tr.GetObject(id, openMode, openErased, openLockedLayer) as T;
    }
}