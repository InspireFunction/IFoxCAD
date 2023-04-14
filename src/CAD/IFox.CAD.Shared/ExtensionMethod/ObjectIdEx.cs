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
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>指定类型对象</returns>
    public static T? GetObject<T>(this ObjectId id,
                                 OpenMode openMode = OpenMode.ForRead,
                                 bool openErased = false,
                                 bool openLockedLayer = false) where T : DBObject
    {
        var tr = DBTrans.GetTopTransaction(id.Database);
        return tr.GetObject<T>(id, openMode, openErased, openLockedLayer);
    }

    /// <summary>
    /// 获取指定类型对象集合
    /// </summary>
    /// <typeparam name="T">指定的泛型</typeparam>
    /// <param name="ids">对象id集合</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>指定类型对象集合</returns>
    [System.Diagnostics.DebuggerStepThrough]
    public static IEnumerable<T> GetObject<T>(this IEnumerable<ObjectId> ids,
                                               OpenMode openMode = OpenMode.ForRead,
                                               bool openErased = false,
                                               bool openLockedLayer = false) where T : DBObject
    {
        return ids.Select(id => 
                        id.GetObject<T>(openMode, openErased, openLockedLayer))
                  .OfType<T>();
    }

    /// <summary>
    /// 返回符合类型的对象id
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="ids">对象id集合</param>
    /// <param name="exactMatch">精确匹配</param>
    /// <returns>对象id集合</returns>
    public static IEnumerable<ObjectId> OfType<T>(this IEnumerable<ObjectId> ids,bool exactMatch =false) where T : DBObject
    {
        var  rxc = RXClass.GetClass(typeof(T));
        if (exactMatch)
        {
            var dxfName = rxc.DxfName;
            return ids.Where(id => id.ObjectClass.DxfName == dxfName);
        }
        else
        {
            return ids.Where(id => id.ObjectClass.IsDerivedFrom(rxc));
        }
    }
    #endregion GetObject

    /// <summary>
    /// 根据对象句柄字符串获取对象Id
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="handleString">句柄字符串</param>
    /// <returns>对象的ObjectId</returns>
    public static ObjectId GetObjectId(this Database db, string handleString)
    {
        if (long.TryParse(handleString, System.Globalization.NumberStyles.HexNumber, null, out long l))
        {
            var hanle = new Handle(l);
            if (db.TryGetObjectId(hanle, out ObjectId id))
            {
                return id;
            }
        }
        return ObjectId.Null;
    }

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
            // Env.Editor.Regen();
        }
    }
}