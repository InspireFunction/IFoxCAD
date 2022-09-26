#define lack_test

namespace IFoxCAD.Cad;

#if lack_test
public static class DBTransEx
{
    /*
     * 0x01
     * db.Purge(ids)是获取未硬引用(无引用?)的对象,也就可以删除的.
     * 0x02
     * 如果一个图元引用一个图层,
     * 假设这个图元是可以删除的(实际上它可能来自于词典记录的id) => 那么它被 db.Purge(ids) 识别,
     * 但是这个图层因为有硬引用,所以不被 db.Purge(ids) 识别,
     * 只能删除图元之后,循环第二次再通过 db.Purge(ids) 获取图层id.
     * 0x03
     * 因为删除之后,符号表内的引用可能被修改,因此需要重复遍历符号表.
     * 0x04
     * 试试循环事务
     * 0x05
     * 无法过滤外部参照图层,使得全部图层打开了
     */

    /// <summary>
    /// 清理符号表
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="sym"></param>
    /// <param name="excludeXref">排除外部参照:默认true,为false时候会令图层全部显示再清理,包括冻结</param>
    public static void Purge(this DBTrans tr, SymModes sym = SymModes.All, bool excludeXref = true)
    {
        using ObjectIdCollection ids = new();
        var db = tr.Database;

        if ((sym & SymModes.BlockTable) == SymModes.BlockTable)
        {
            if (!excludeXref)
                GetAllIds(tr, tr.BlockTable, ids, excludeXref);
            else
                tr.BlockTable.ForEach(tabRec => {
                    if (!tabRec.IsFromExternalReference)
                        ids.Add(tabRec.Id);
                });
        }
        if ((sym & SymModes.DimStyleTable) == SymModes.DimStyleTable)
            GetAllIds(tr, tr.DimStyleTable, ids, excludeXref);
        if ((sym & SymModes.LayerTable) == SymModes.LayerTable)
            GetAllIds(tr, tr.LayerTable, ids, excludeXref);
        if ((sym & SymModes.LinetypeTable) == SymModes.LinetypeTable)
            GetAllIds(tr, tr.LinetypeTable, ids, excludeXref);
        if ((sym & SymModes.TextStyleTable) == SymModes.TextStyleTable)
            GetAllIds(tr, tr.TextStyleTable, ids, excludeXref);
        if ((sym & SymModes.ViewportTable) == SymModes.ViewportTable)
            GetAllIds(tr, tr.ViewportTable, ids, excludeXref);
        if ((sym & SymModes.RegAppTable) == SymModes.RegAppTable)
            GetAllIds(tr, tr.RegAppTable, ids, excludeXref);

        // SHUN007 说这两个表可能有错误
        if ((sym & SymModes.ViewTable) == SymModes.ViewTable)
            GetAllIds(tr, tr.ViewTable, ids, excludeXref);
        if ((sym & SymModes.UcsTable) == SymModes.UcsTable)
            GetAllIds(tr, tr.UcsTable, ids, excludeXref);

        // Purge是查询能够清理的对象
        db.Purge(ids);
        foreach (ObjectId id in ids)
            id.Erase();
    }

    static void GetAllIds<TTable, TRecord>(DBTrans tr,
                       SymbolTable<TTable, TRecord> symbolTable,
                       ObjectIdCollection ids,
                       bool excludeXref = true)
                       where TTable : SymbolTable
                       where TRecord : SymbolTableRecord, new()
    {
        if (!excludeXref)
            symbolTable.ForEach(id => ids.Add(id));
        else
        {
            symbolTable.ForEach(id => {
                var tabRec = tr.GetObject<TRecord>(id);
                if (tabRec == null)
                    return;
                if (!tabRec.Name.Contains("|"))
                    ids.Add(tabRec.Id);
            });
        }
    }
}
#endif