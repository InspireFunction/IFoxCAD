using System.Xml.Linq;

namespace IFoxCAD.Cad;


//测试代码(图纸仅绑定,不做其他处理,如清理,关图层,删除嵌套被卸载的参照等)
public class TestCmd_BindXrefs
{
    //后台绑定
    [CommandMethod("MyXBindXrefs")]
    public static void MyXBindXrefs()
    {
        string newfile = @"D:\桌面\测试文件\三梅中学-教学楼-结构施工图 - 副本.dwg";
        using var tr = new DBTrans(newfile, openMode: FileOpenMode.OpenForReadAndAllShare/*后台绑定需要特别注意*/);
        //用于后期删除嵌套被卸载的参照
        Dictionary<ObjectId, string> nested = new();
        XrefEx.XBindXrefs(tr, nested, SymTypes.LayerTable);
        tr.SaveDwgFile();
    }

#if true2
    //后台绑定
    [CommandMethod("MyXBindXrefs")]
    public static void MyXBindXrefs()
    {
        using var tr = new DBTrans(filePath, openMode: FileOpenMode.OpenForReadAndAllShare/*后台绑定注意*/);
        //用于后期删除嵌套被卸载的参照
        Dictionary<ObjectId, string> nested = new();
        tr.XBindXrefs(nested);
        tr.SaveDwgFile();
    } 
#endif

    //前台绑定
    [CommandMethod("MyXBindXrefs1")]
    public static void MyXBindXrefs1()
    {
        using var tr = new DBTrans();
        //用于后期删除嵌套被卸载的参照
        Dictionary<ObjectId, string> nested = new();
        tr.XBindXrefs(nested);
        tr.SaveDwgFile();
    }
}

public enum SymTypes
{
    /// <summary>
    /// 图层表
    /// </summary>
    LayerTable = 1,
    /// <summary>
    /// 文字样式表
    /// </summary>
    TextStyleTable = 2,
    /// <summary>
    /// 标注样式表
    /// </summary>
    DimStyleTable = 4,
    /// <summary>
    /// 线型表
    /// </summary>
    LinetypeTable = 8,
    /// <summary>
    /// 注册应用程序表
    /// </summary>
    RegAppTable = 16,

    All = LayerTable | TextStyleTable | DimStyleTable | LinetypeTable | RegAppTable
}

public static class XrefEx
{
    /// <summary>
    /// <code>
    /// 双重绑定参照
    /// 该方法仅用于"图层绑定"项,其他项有异常eWasOpenForNotify
    /// </code>
    /// <a href="https://www.cnblogs.com/SHUN-ONCET/p/16593360.html">参考链接</a>
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="nested">嵌套参照名称,id</param>
    /// <param name="xNum">需要绑定的符号表</param>
    /// <param name="detachXref">是否拆离参照</param>
    /// <param name="easeNested">是否删除被卸载的嵌套参照</param>
    public static void XBindXrefs(this DBTrans tr,
                                  Dictionary<ObjectId, string> nested,
                                  SymTypes xNum = SymTypes.LayerTable,
                                  bool detachXref = true,
                                  bool easeNested = true)
    {
        DoubleBind(tr, nested, detachXref, (xbindXrefsIds) => {

            // 起初测试是将九大符号表记录均加入的,但经实测不行...(为什么?存疑)
            if ((xNum & SymTypes.LayerTable) == SymTypes.LayerTable)
                tr.LayerTable.ForEach(tabRec => {
                    AddedxbindXrefsIds(tabRec, xbindXrefsIds);
                }, checkIdOk: true);

            if ((xNum & SymTypes.TextStyleTable) == SymTypes.TextStyleTable)
                tr.TextStyleTable.ForEach(tabRec => {
                    AddedxbindXrefsIds(tabRec, xbindXrefsIds);
                }, checkIdOk: true);

            if ((xNum & SymTypes.LinetypeTable) == SymTypes.LinetypeTable)
                tr.LinetypeTable.ForEach(tabRec => {
                    AddedxbindXrefsIds(tabRec, xbindXrefsIds);
                }, checkIdOk: true);

            if ((xNum & SymTypes.DimStyleTable) == SymTypes.DimStyleTable)
                tr.DimStyleTable.ForEach(tabRec => {
                    AddedxbindXrefsIds(tabRec, xbindXrefsIds);
                }, checkIdOk: true);

            if ((xNum & SymTypes.RegAppTable) == SymTypes.RegAppTable)
                tr.RegAppTable.ForEach(tabRec => {
                    AddedxbindXrefsIds(tabRec, xbindXrefsIds);
                }, checkIdOk: true);
        });

        // 内部删除嵌套参照的块操作
        if (easeNested)
        {
            foreach (var item in nested)
            {
                var name = item.Value;
                if (tr.BlockTable.Has(name))
                    tr.GetObject<BlockTableRecord>(tr.BlockTable[name], OpenMode.ForWrite)?
                      .Erase();
            }
        }
    }

    /// <summary>
    /// 符号表记录加入xbind
    /// </summary>
    /// <typeparam name="TTableRecord"></typeparam>
    /// <param name="xbindXrefsIds"></param>
    /// <param name="tabRec"></param>
    static void AddedxbindXrefsIds<TTableRecord>(TTableRecord tabRec, ObjectIdCollection xbindXrefsIds)
        where TTableRecord : SymbolTableRecord
    {
        if (tabRec.IsResolved)
            xbindXrefsIds.Add(tabRec.ObjectId);
    }

    static void DoubleBind(this DBTrans tr,
                           Dictionary<ObjectId, string> nested,
                           bool detachXref,
                           Action<ObjectIdCollection> xbindAction)
    {
        DatabaseEx.DBTextDeviation(tr.Database, () => {

            //解析外部参照:此功能不能锁定文档
            //useThreadEngine==true,线性引擎,会在cad命令历史打印一些AEC信息,并导致绑定慢一点...具体作用不详
            //doNewOnly==true,仅处理 Unresolved_未融入(未解析)的参照
            tr.Database.ResolveXrefs(useThreadEngine: false, doNewOnly: false);

            //有这个 eWrongObjectType 异常,说明还是 xbindXrefsIds 的集合id有问题
            //xbind才是绑其他符号表,/xbind绑块表也会有异常
            var xbindXrefsIds = new ObjectIdCollection();
            //bind只绑块表
            var bindXrefsIds = new ObjectIdCollection();

            tr.BlockTable.ForEach(btr => {
                if (btr.IsFromExternalReference && btr.IsResolved)
                    bindXrefsIds.Add(btr.ObjectId);
            }, checkIdOk: true);

            // 直接拆离的id
            List<ObjectId> detachXrefIds = new();

            // 补充是否被嵌套卸载的处理
            var xg = tr.Database.GetHostDwgXrefGraph(true);//参数:包含僵尸参照
            for (int j = 0; j < xg.NumNodes; j++)
            {
                var xrNode = xg.GetXrefNode(j);
                switch (xrNode.XrefStatus)
                {
                    case XrefStatus.Unresolved://未融入_ResolveXrefs参数2
                        break;
                    case XrefStatus.FileNotFound://未融入(未解析)_未找到文件
                        break;
                    case XrefStatus.Unreferenced://未参照
                        detachXrefIds.Add(xrNode.BlockTableRecordId);
                        break;
                    case XrefStatus.Unloaded://已卸载
                        {
                            var xrId = xrNode.BlockTableRecordId;
                            if (!xrId.IsOk())
                                break;
                            var btr = tr.GetObject<BlockTableRecord>(xrId);
                            if (btr != null && btr.IsFromExternalReference)
                            {
                                if (!xrNode.IsNested)
                                    detachXrefIds.Add(xrId);
                                else if (!nested.ContainsKey(xrId))
                                    nested.Add(xrId, xrNode.Name);//嵌套参照
                            }
                        }
                        break;
                    case XrefStatus.Resolved://已融入_就是可以绑定的
                        break;
                    case XrefStatus.NotAnXref://不是外部参照
                        break;
                    default:
                        break;
                }
            }

            //拆离未参照的文件
            if (detachXref)
            {
                for (int i = 0; i < detachXrefIds.Count; i++)
                    tr.Database.DetachXref(detachXrefIds[i]);
            }

            xbindAction?.Invoke(xbindXrefsIds);

            //若有嵌套参照被卸载,重载
            var keys = nested.Keys.ToArray();
            if (keys.Length > 0)
                tr.Database.ReloadXrefs(new ObjectIdCollection(keys));

            // 切勿交换,若交换秩序,则会绑定无效
            if (xbindXrefsIds.Count > 0)
                tr.Database.XBindXrefs(xbindXrefsIds, true);
            if (bindXrefsIds.Count > 0)
                tr.Database.BindXrefs(bindXrefsIds, true);
        });
    }


#if true2
    /// <summary>
    /// 清理符号7表(块表,标注样式表,图层表,线型表,文字样式表,视口样式表,注册应用表)
    ///注:视图样式表和坐标系表不能这样清理,有异常
    /// </summary>
    /// <param name="db">数据库</param>
    /// <returns>无返回值</returns>
    public static void Purge(Database db)
    {
        ObjectIdCollection ids;
        ObjectIdCollection ids1;
        ObjectIdCollection ids2;
        ObjectIdCollection ids3;
        ObjectIdCollection ids4;
        ObjectIdCollection ids5;
        //ObjectIdCollection ids6;
        //ObjectIdCollection ids7;
        ObjectIdCollection ids8;
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            //块表
            BlockTable blockt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //标注样式表1
            DimStyleTable dimt = tr.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            //图层表2
            LayerTable layert = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            //线型表3
            LinetypeTable linetypet = tr.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
            //文字样式4
            TextStyleTable textstylet = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
            //视口样式表5
            ViewportTable viewport = tr.GetObject(db.ViewportTableId, OpenMode.ForRead) as ViewportTable;
            //注册应用表8
            RegAppTable regappt = tr.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;

    #region 开始清理
            do
            {
                ids = new ObjectIdCollection(blockt.Cast<ObjectId>().ToArray());
                db.Purge(ids);
                if (ids != null)
                {
                    foreach (ObjectId id in ids)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
                ids1 = new ObjectIdCollection(dimt.Cast<ObjectId>().ToArray());
                db.Purge(ids1);
                if (ids1 != null)
                {
                    foreach (ObjectId id in ids1)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
                ids2 = new ObjectIdCollection(layert.Cast<ObjectId>().ToArray());
                db.Purge(ids2);
                if (ids2 != null)
                {
                    foreach (ObjectId id in ids2)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
                ids3 = new ObjectIdCollection(linetypet.Cast<ObjectId>().ToArray());
                db.Purge(ids3);
                if (ids3 != null)
                {
                    foreach (ObjectId id in ids3)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
                ids4 = new ObjectIdCollection(textstylet.Cast<ObjectId>().ToArray());
                db.Purge(ids4);
                if (ids4 != null)
                {
                    foreach (ObjectId id in ids4)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
                ids5 = new ObjectIdCollection(viewport.Cast<ObjectId>().ToArray());
                db.Purge(ids5);
                if (ids5 != null)
                {
                    foreach (ObjectId id in ids5)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
    #region 经测试视图样式表和坐标系表不能这样清理,不然有异常
                ////视图样式表6
                //ViewTable viewt = tr.GetObject(db.ViewTableId, OpenMode.ForRead) as ViewTable;
                //ids6 = new ObjectIdCollection(viewt.Cast<ObjectId>().ToArray());
                //db.Purge(ids6);
                //foreach (ObjectId id in ids6)
                //{
                //    ViewTableRecord btr6 = (ViewTableRecord)tr.GetObject(id, OpenMode.ForWrite);
                //    btr6.Erase();
                //}
                ////坐标系表7
                //UcsTable ucst = tr.GetObject(db.UcsTableId, OpenMode.ForRead) as UcsTable;
                //ids7 = new ObjectIdCollection(ucst.Cast<ObjectId>().ToArray());
                //db.Purge(ids7);
                //foreach (ObjectId id in ids7)
                //{
                //    UcsTableRecord btr7 = (UcsTableRecord)tr.GetObject(id, OpenMode.ForWrite);
                //    btr7.Erase();
                //}
    #endregion
                ids8 = new ObjectIdCollection(regappt.Cast<ObjectId>().ToArray());
                db.Purge(ids8);
                if (ids8 != null)
                {
                    foreach (ObjectId id in ids8)
                    {
                        tr.GetObject(id, OpenMode.ForWrite).Erase();
                    }
                }
            } while (ids.Count > 0 || ids1.Count > 0 || ids2.Count > 0 || ids3.Count > 0 || ids4.Count > 0 || ids5.Count > 0 || ids8.Count > 0);
    #endregion

            tr.Commit();//提交事务
        }
    } 
#endif
}