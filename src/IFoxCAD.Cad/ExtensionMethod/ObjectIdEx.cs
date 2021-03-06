namespace IFoxCAD.Cad;

/// <summary>
/// 对象id扩展类
/// </summary>
public static class ObjectIdEx
{
    #region GetObject

    /// <summary>
    /// 获取指定类型对象
    /// </summary>
    /// <typeparam name="T">指定的泛型</typeparam>
    /// <param name="id">对象id</param>
    /// <param name="tr">事务</param>
    /// <param name="mode">打开模式</param>
    /// <param name="openErased">打开删除对象</param>
    /// <returns>指定类型对象</returns>
    public static T? GetObject<T>(this ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false, Transaction? tr = default) where T : DBObject
    {
        tr ??= DBTrans.Top.Transaction;
        //tr = Env.GetTrans(tr);
        return tr.GetObject(id, mode, openErased) as T;
    }

    /// <summary>
    /// 获取指定类型对象集合
    /// </summary>
    /// <typeparam name="T">指定的泛型</typeparam>
    /// <param name="ids">对象id集合</param>
    /// <param name="tr">事务</param>
    /// <param name="mode">打开模式</param>
    /// <param name="openErased">打开删除对象</param>
    /// <returns>指定类型对象集合</returns>
    public static IEnumerable<T?> GetObject<T>(this IEnumerable<ObjectId> ids, OpenMode mode = OpenMode.ForRead, bool openErased = false, Transaction? tr = default) where T : DBObject
    {
        return ids.Select(id => id.GetObject<T>(mode, openErased, tr));
    }

    /// <summary>
    /// 返回符合类型的对象id
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="ids">对象id集合</param>
    /// <returns>对象id集合</returns>
    public static IEnumerable<ObjectId> OfType<T>(this IEnumerable<ObjectId> ids) where T : DBObject
    {
        string dxfName = RXClass.GetClass(typeof(T)).DxfName;
        return
            ids
            .Where(id => id.ObjectClass.DxfName == dxfName);
    }
    #endregion GetObject
    /// <summary>
    /// id是否有效,未被删除
    /// </summary>
    /// <param name="id">对象id</param>
    /// <returns>id有效返回 <see langword="true"/>，反之返回 <see langword="false"/></returns>
    public static bool IsOk(this ObjectId id)
    {
        return !id.IsNull && id.IsValid && !id.IsErased && !id.IsEffectivelyErased && id.IsResident;
    }
    /// <summary>
    /// 删除id代表的对象
    /// </summary>
    /// <param name="id">对象id</param>
    public static void Erase(this ObjectId id)
    {
        if (id.IsOk())
        {
            var ent = id.GetObject<DBObject>()!;
            using (ent.ForWrite())
            {
                ent.Erase();
            }// 第一种读写权限自动转换写法
            //Env.Editor.Regen();
        }
    }
}
