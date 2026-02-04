namespace Test;


// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,如果发生 _workset 包含就执行,否则跳过处理.
// 问题是为什么这些数据库已经准备入库了,无法vote()处理啊.
// 官方是怎么做的呢?

public class RefEditCmd
{
    // 收集所有图层ID
    HashSet<ObjectId> _lockedLayers = [];
    public static HashSet<ObjectId> _workset = [];

    // refedit
    // 模拟,实现一个自己的在位编辑器(长事务)
    [CommandMethod(nameof(RefEdit), CommandFlags.Redraw)]
    public void RefEdit()
    {
        // 让用户只能选择块参照
        var ed = Application.DocumentManager.MdiActiveDocument.Editor;
        var pm = new PromptEntityOptions("\n 选择块参照");
        pm.SetRejectMessage("\n 只能选择块参照!");
        pm.AddAllowedClass(typeof(BlockReference), true);
        var per = ed.GetEntity(pm);
        if (per.Status != PromptStatus.OK)
            return;

        using (var tr = DBTrans.Create())
        {
            // 2,锁定图层,刷新图层状态,让锁定的图层的图元是暗显.
            // 3,解锁全部图层,不刷新,使得全部图元是暗显.
            // 锁定图层
            tr.LayerTable.ForEach((layer, state) => {
                if (!layer.IsLocked)
                {
                    layer.IsLocked = true;
                    _lockedLayers.Add(layer.ObjectId);
                }
            }, OpenMode.ForWrite);

            // 刷新画面的图层暗显
            IFoxUtils.RegenLayers(_lockedLayers);

            // 解锁图层
            tr.LayerTable.ForEach((layer, state) => {
                if (_lockedLayers.Contains(layer.ObjectId))
                    layer.IsLocked = false;
            }, OpenMode.ForWrite);
        }

        using (var tr = DBTrans.Create())
        {
            // 1,删除这个块
            using var bent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForWrite, true, true);
            if (bent is not BlockReference brf)
                return;
            brf.Erase(true);

            // 5,新建一个无锁的Edit-0图层.
            var eLayer = tr.LayerTable.Add("Edit-0");

            // 4,块表记录提取块内图元出来,此时它就是亮显的,因为此时没有锁定图层.
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            using ObjectIdCollection ids = [.. btr];

            // 深度克隆,然后平移到当前目标点位置
            var move = Point3d.Origin;
            var moveTo = brf.Position;
            using IdMapping map = [];
            tr.CurrentSpace.DeepCloneEx(ids, map);
            map.GetValues().ForEach(id => {
                if (!id.IsOk())
                    return;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                //ent.LayerId = eLayer;
                ent.Move(move, moveTo);
                _workset.Add(id);
            });

            // 我的想法是只刷新这些图元,但是发现同一个事务会触发刷新全部图层,所以要多事务.
            var lays = new List<ObjectId>
            {
                eLayer
            };
            IFoxUtils.RegenLayers(lays);
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

