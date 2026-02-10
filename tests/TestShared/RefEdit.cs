using Autodesk.AutoCAD.DatabaseServices;
using IFoxCAD.Cad;

namespace Test;

// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,无法vote()处理.
// 我们只能够在命令前触发,然后重设选择集,{命令完成之后,又恢复全部图元的选择...没实现,貌似不需要}

public class RefEditInfo
{
    public static Dictionary<Document, RefEditInfo> Map = [];

    public Document Document { get; set; }
    public ObjectId BlockReferenceId { get; set; }
    public HashSet<ObjectId> Workset { get; set; } = [];
    public HashSet<ObjectId> LockedLayers { get; set; } = [];
    public ObjectId CurrentSpaceId { get; internal set; }

    public RefEditInfo(Document document, ObjectId currentSpaceId)
    {
        Document = document;
        CurrentSpaceId = currentSpaceId;
        Map.Add(document, this);
    }
}

public class RefEditCmd
{
    // 收集所有图层ID
    public static HashSet<string> _workcmd = new(StringComparer.OrdinalIgnoreCase);

    [IFoxInitialize]
    public void Init(Document doc)
    {
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
        doc.Database.ObjectAppended += Database_ObjectAppended;
        doc.Database.ObjectErased += Database_ObjectErased;

        doc.ImpliedSelectionChanged += Doc_ImpliedSelectionChanged;

        _workcmd.Add(nameof(REFSET_ADD));
        _workcmd.Add(nameof(REFSET_REMOVE));
    }

    // 防止选择集事件的死循环
    static bool _ssFlag = false;

    // 选择集事件
    private void Doc_ImpliedSelectionChanged(object sender, EventArgs e)
    {
        //if (_ssFlag)
        //{
        //    _ssFlag = false;
        //    return;
        //}

        //var prompt = Env.Editor.SelectImplied();
        //if (prompt.Status != PromptStatus.OK)
        //    return;

        //var ids = prompt.Value.GetObjectIds();
        //if (ids.Length == 0)
        //    return;

        //var doc = Acap.DocumentManager.MdiActiveDocument;
        //if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        //{
        //    var list = ids.Where(xInfo.Workset.Contains).ToArray();
        //    if (list.Length == 0)
        //        return;
        //    _ssFlag = true;
        //    Env.Editor.SetImpliedSelection(list);
        //}
    }

    private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (e.DBObject is not Entity)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        {
            // 即使重复加入也没有关系,因为是HashSet
            xInfo.Workset.Remove(e.DBObject.ObjectId);
        }
    }

    private void Database_ObjectAppended(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is not Entity ent)
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        {
            // TODO 在位编辑要获取相同空间的,不能够跨空间,而且必须要是图元
            if (xInfo.CurrentSpaceId != ent.Database.CurrentSpaceId)
                return;
            // 即使重复加入也没有关系,因为是HashSet
            xInfo.Workset.Add(ent.ObjectId);
        }
    }

    // 文档锁事件(否决命令执行)
    private void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        // 跳过噪音
        if (e.GlobalCommandName == "" || e.GlobalCommandName == "#")
            return;

        if (_workcmd.Contains(e.GlobalCommandName))
            return;

        // TODO 先move再选择,无法跳过...选择集事件,也无法跳过
        // 难道是先锁定全部图层,把workset的改为一个不锁的图层?
        // 貌似还真的可以....

        // 含有就表示正在 在位编辑 过程中
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        {
            var prompt = Env.Editor.SelectImplied();
            if (prompt.Status != PromptStatus.OK)
                return;
            // 获取工作集部分,然后才能执行官方命令/其他Lisp命令
            // 重设选择集
            var list = prompt.Value.GetObjectIds().Where(xInfo.Workset.Contains).ToArray();
            Env.Editor.SetImpliedSelection(list);
        }
    }


    [CommandMethod(nameof(REFSET_ADD), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void REFSET_ADD()
    {
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xinfo))
            return;
        // 即使重复加入也没有关系,因为是HashSet
        xinfo.Workset.Add(prompt.Value.GetObjectIds());

        // 刷新一次
        // 因为添加时候是解锁状态,只需要平移就等于刷新
        using var tr = DBTrans.Create();
        foreach (var id in prompt.Value.GetObjectIds())
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
            ent.Move(Point3d.Origin, Point3d.Origin);
        }

        // 清空选择集
        Env.Editor.SetImpliedSelection([]);
    }


    [CommandMethod(nameof(REFSET_REMOVE), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void REFSET_REMOVE()
    {
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;

        // 即使重复移除也没有关系,因为是HashSet
        xInfo.Workset.Remove(prompt.Value.GetObjectIds());


        // 淡显
        // 锁定全部,再把workset给亮回来
        LockAndUnLock(xInfo);

        using (var tr = DBTrans.Create())
        {
            foreach (var id in xInfo.Workset)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                ent.Move(Point3d.Origin, Point3d.Origin);
            }

            RegenLayers(tr);
        }
    }

    private static void RegenLayers(DBTrans tr)
    {
        var eLayer = tr.LayerTable.Add("Edit-0");
        var lays = new List<ObjectId>
            {
                eLayer
            };
        IFoxUtils.RegenLayers(lays);
    }

    [CommandMethod(nameof(RefClose), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefClose()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;

        // 1,移除原本btr内的图元,是一个块表记录容器,把 workset 设置进去
        using var tr = DBTrans.Create();

        using var brf = (BlockReference)tr.GetObject(xInfo.BlockReferenceId, OpenMode.ForWrite, true, true);
        using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
        var move = Point3d.Origin;
        var moveTo = brf.Position;
        // 移除块表记录中的所有图元
        foreach (ObjectId id in btr)
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
            ent.Erase(true);
        }

        // 将工作集中的图元添加回块表记录
        foreach (var id in xInfo.Workset)
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);

            var ent2 = ent.CloneEx();
            ent2.Move(moveTo, move);
            btr.AppendEntity(ent2);
            tr.AddNewlyCreatedDBObject(ent2, true);

            ent.Erase(true);
        }
        brf.Erase(false);

        // 2,恢复图层锁定的显示
        IFoxUtils.RegenLayers(xInfo.LockedLayers);

        // 3,清空
        RefEditInfo.Map.Remove(doc);

        // TODO 4,此处没有考虑undo的时候怎么恢复?

    }

    // 模拟,实现一个自己的在位编辑器(长事务)
    [CommandMethod(nameof(RefEdit), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefEdit()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;

        // 让用户只能选择块参照
        var pm = new PromptEntityOptions("\n 选择块参照");
        pm.SetRejectMessage("\n 只能选择块参照!");
        pm.AddAllowedClass(typeof(BlockReference), true);
        var per = Env.Editor.GetEntity(pm);
        if (per.Status != PromptStatus.OK)
            return;

        if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        {
            Env.Print("不能重复使用: 在位编辑器");
            return;
        }

        xInfo = new RefEditInfo(doc, doc.Database.CurrentSpaceId);


        // 2,锁定图层,刷新图层状态,让锁定的图层的图元是暗显.
        // 3,解锁全部图层,不刷新,使得全部图元是暗显.
        // 锁定图层
        LockAndUnLock(xInfo);

        using (var tr = DBTrans.Create())
        {
            // 1,删除这个块
            using var bent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForWrite, true, true);
            if (bent is not BlockReference brf)
                return;
            brf.Erase(true);

            // 4,块表记录提取块内图元出来,此时它就是亮显的,因为此时没有锁定图层.
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            using ObjectIdCollection ids = [.. btr];

            // 记录用于保存在位编辑器的工作集
            xInfo.BlockReferenceId = brf.ObjectId;

            // 深度克隆,然后平移到当前目标点位置
            var move = Point3d.Origin;
            var moveTo = brf.Position;
            using IdMapping map = [];
            tr.CurrentSpace.DeepCloneEx(ids, map);
            map.GetValues().ForEach(id => {
                if (!id.IsOk())
                    return;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                ent.Move(move, moveTo);
                xInfo.Workset.Add(id);
            });

            // 这里必须要刷新一个图层,然后使得这些克隆出来的对象是亮显的.
            // 即使这个图层没有发挥任何作用.
            // 我的想法是只刷新这些图元,但是发现同一个事务会触发刷新全部图层,所以要多事务.
            RegenLayers(tr);
        }
    }

    private static void LockAndUnLock(RefEditInfo xInfo)
    {
        using (var tr = DBTrans.Create())
        {
            tr.LayerTable.ForEach((layer, state) => {
                if (!layer.IsLocked)
                {
                    layer.IsLocked = true;
                    xInfo.LockedLayers.Add(layer.ObjectId);
                }
            }, OpenMode.ForWrite);

            // 刷新画面的图层暗显
            IFoxUtils.RegenLayers(xInfo.LockedLayers);

            // 解锁图层
            tr.LayerTable.ForEach((layer, state) => {
                if (xInfo.LockedLayers.Contains(layer.ObjectId))
                    layer.IsLocked = false;
            }, OpenMode.ForWrite);
        }
    }
}





//// kean在位编辑器
//// https://keanw.com/2015/09/launching-autocads-refedit-command-with-an-entity-selected-using-net.html?sharetype=link

//#if !NET35
//namespace Test;

//public static class Extensions
//{
//    ///<summary>
//    /// Get the child entity of the first xref in the nested selection.
//    ///</summary>
//    ///<returns>ObjectId of the top-level object from the outer xref.</returns>
//    public static ObjectId GetFirstXrefChild(this PromptNestedEntityResult res)
//    {
//        var retId = ObjectId.Null;
//        var selId = res.ObjectId;
//        var conts = res.GetContainers();
//        var db = selId.Database;
//        // Use an open-close transaction as we're in a utility function
//        using (var tr = db.TransactionManager.StartOpenCloseTransaction())
//        {
//            // Work backwards through the containers, looking for an xref
//            for (int i = conts.Length - 1; i >= 0; i--)
//            {
//                var br = tr.GetObject(conts[i], OpenMode.ForRead) as BlockReference;
//                if (br != null)
//                {
//                    var btr =
//                      (BlockTableRecord)tr.GetObject(
//                        br.BlockTableRecord, OpenMode.ForRead
//                      );
//                    // If we have an xref, we'll return the next container or the
//                    // selected entity in the case we're at the innermost container
//                    if (btr.IsFromExternalReference)
//                    {
//                        retId = i > 0 ? conts[i - 1] : selId;
//                        break;
//                    }
//                }
//            }
//            tr.Commit();
//        }
//        return retId;
//    }
//}

//public class Commands
//{
//    [CommandMethod("RS", CommandFlags.Redraw)]
//    public void RefeditSelected()
//    {
//        var doc = Application.DocumentManager.MdiActiveDocument;
//        if (doc == null) return;
//        var db = doc.Database;
//        var ed = doc.Editor;
//        // Select an entity within an xref
//        var pner = ed.GetNestedEntity("\nSelect entity on xref");
//        if (pner.Status != PromptStatus.OK)
//            return;
//        // Get the ID of the entity that we want to select in the xref
//        // (this is the first entity contained by an xref)
//        var selId = pner.GetFirstXrefChild();
//        // Only proceed if something is containing it
//        if (selId != ObjectId.Null)
//        {
//            // Define our event handler
//            LongTransactionEventHandler func = (s, e) => {
//                // Get the deepclone translation mapping from the long transaction
//                var map = e.Transaction.ActiveIdMap;
//                // If there's a mapping from our selected object...
//                if (map.Contains(selId))
//                {
//                    // ... add it to the pickfirst selection set
//                    ed.SetImpliedSelection(new ObjectId[] { map[selId].Value });
//                    ed.WriteMessage("\nSelected one entity.");
//                }
//            };
//            // Attach our handler, call REFEDIT and then detach it
//            Application.LongTransactionManager.CheckedOut += func;
//            ed.Command("_.-REFEDIT", pner.PickedPoint, "_O", "_A", "_N");
//            Application.LongTransactionManager.CheckedOut -= func;
//        }
//    }
//}
//#endif

