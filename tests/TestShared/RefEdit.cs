using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using IFoxCAD.Cad;
using System.Collections.Generic;

namespace Test;

// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,无法vote()处理.
// 由于move触发之后再选择,可以选择到workset之外的对象,
// 我们只能够在命令触发前事件锁定图层,这样就选择不了,命令结束之后解锁.

public class RefEditInfo
{
    public static Dictionary<Document, RefEditInfo> Map = [];

    public Document Document { get; set; }
    public ObjectId BlockReferenceId { get; set; }
    public HashSet<ObjectId> Workset { get; set; } = [];
    public HashSet<ObjectId> LockedLayers { get; set; } = [];
    public ObjectId CurrentSpaceId { get; internal set; }

    // 备份,map[图元id,原有图层id]
    public Dictionary<ObjectId, ObjectId> ActionEntityLayerMap = [];

    public RefEditInfo(Document document, ObjectId currentSpaceId)
    {
        Document = document;
        CurrentSpaceId = currentSpaceId;
        Map.Add(document, this);
    }
}

public class RefEditCmd
{
    // 本工程命令要作为例外
    static HashSet<string> _workCmd = new(StringComparer.OrdinalIgnoreCase);
    const string RefEdit0 = "RefEdit-0";

    [IFoxInitialize]
    public void Init(Document doc)
    {
        doc.Database.ObjectAppended += Database_ObjectAppended;
        doc.Database.ObjectErased += Database_ObjectErased;

        doc.CommandWillStart += OnCommandWillStart;
        doc.CommandEnded += OnCommandEnded;
        doc.CommandCancelled += Doc_CommandCancelled;

        _workCmd.Add(nameof(RefSet));
        _workCmd.Add(nameof(RefClose));
        _workCmd.Add(nameof(RefEdit));
    }

    private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (e.DBObject is not Entity)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;
        // 即使重复移除也没有关系,因为是HashSet
        xInfo.Workset.Remove(e.DBObject.ObjectId);
    }

    private void Database_ObjectAppended(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is not Entity ent)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;
        // workset不能够跨空间,而且必须要是图元
        if (xInfo.CurrentSpaceId != ent.BlockId)
            return;
        // 即使重复加入也没有关系,因为是HashSet
        xInfo.Workset.Add(ent.ObjectId);
    }



    // 命令取消
    private void Doc_CommandCancelled(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        CommandEndedOrCancelled(cmd);
    }

    // 命令结束
    private void OnCommandEnded(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        CommandEndedOrCancelled(cmd);
    }

    private void CommandEndedOrCancelled(string cmd)
    {
        if (_workCmd.Contains(cmd))
            return;

        // 含有就表示正在 在位编辑 过程中
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;

        ChangeLayer(false, xInfo);
    }

    // 命令开始
    // 感觉每个命令都需要处理一次,实在有点太过分耶...
    // TODO 是不是应该做一些批量处理的操作?例如是sendCommand或者Lisp期间就不执行?
    private void OnCommandWillStart(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        if (cmd.StartsWith("GRIP_")) // 操作图元夹点的时候会出现两个命令.
            return;

        if (_workCmd.Contains(cmd))
            return;

        // 含有就表示正在 在位编辑 过程中
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;

        // 这种方式无法处理 先move再选择
        //var prompt = Env.Editor.SelectImplied();
        //if (prompt.Status == PromptStatus.OK)
        //{
        //    // 获取工作集部分,然后才能执行官方命令/其他Lisp命令
        //    // 重设选择集
        //    var list = prompt.Value.GetObjectIds().Where(xInfo.Workset.Contains).ToArray();
        //    Env.Editor.SetImpliedSelection(list);
        //}

        if (xInfo.ActionEntityLayerMap.Count > 0) // 防止重入
            return;

        ChangeLayer(true, xInfo);
    }

    /*
    * 官方命令:先命令move再选择,依然会选择到workset以外的.
    * 方案一: 好
    * 1,workset 以外的,备份图元图层,修改为 refedito0,并锁定.官方命令就选择不了.
    * 2,workset 以内的,就原有图层名,因为用户命令可能需要判断图层名称,并且不是锁定的
    * 
    * 方案二: 不好
    * 修改图层名加前缀,然后锁定,会发现用户想要修改workset内的图元图层时候就会存在set图层名是不对的.
    */
    void ChangeLayer(bool isStart, RefEditInfo xInfo)
    {
        if (isStart)
        {
            using var tr = DBTrans.Create();
            var refLayerId = tr.LayerTable.Add(RefEdit0);
            foreach (var id in tr.CurrentSpace)
            {
                if (xInfo.Workset.Contains(id))
                    continue;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                xInfo.ActionEntityLayerMap[id] = ent.LayerId;
                ent.LayerId = refLayerId;
            }

            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.IsLocked = true;
        }
        else
        {
            using var tr = DBTrans.Create();
            var refLayerId = tr.LayerTable.Add(RefEdit0);
            foreach (var id in tr.CurrentSpace)
            {
                if (xInfo.Workset.Contains(id))
                    continue;
                if (!xInfo.ActionEntityLayerMap.TryGetValue(id, out var oldLayer))
                    continue;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                if (refLayerId == ent.LayerId)
                    ent.LayerId = oldLayer;
            }

            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.IsLocked = false;

            xInfo.ActionEntityLayerMap.Clear();
        }

        // 重新淡显全部-再亮显workset
        using (var tr = DBTrans.Create())
        {
            Fade(tr, xInfo);
        }
        using (var tr = DBTrans.Create())
        {
            RegenLayers(tr, xInfo);
        }
    }


    [CommandMethod(nameof(RefSet), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefSet()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!RefEditInfo.Map.TryGetValue(doc, out var xInfo))
            return;
        Env.Printl("\n在参照编辑工作集和宿主图形之间传输对象...");


        // 输入选项 [添加(A)/删除(R)] <添加>: *取消*
        var pko = new PromptKeywordOptions("\n输入选项 ");
        pko.Keywords.Add("A", "A", "添加(A)");
        pko.Keywords.Add("R", "R", "删除(R)");
        pko.Keywords.Default = "A";

        var result = Env.Editor.GetKeywords(pko);
        if (result.Status != PromptStatus.OK)
        {
            Env.Print("*取消*");
            return;
        }

        var psr = Env.Editor.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
        {
            // 选择集有官方自带默认关键字,如果想要键入一样的关键字,需要键盘Hook
            var pso = new PromptSelectionOptions
            {
                MessageForAdding = "\n 选择对象:",
                RejectObjectsFromNonCurrentSpace = true, // 不允许跨空间选择
                RejectObjectsOnLockedLayers = true, // 不选择锁定图层对象
                AllowDuplicates = true, // 不允许重复选择
            };
            psr = Env.Editor.GetSelection(pso);// 手选
            if (psr.Status != PromptStatus.OK)
            {
                Env.Printl("*取消*");
                return;
            }
        }
        var idArray = psr.Value.GetObjectIds();

        bool isAdd = true;
        isAdd = result.StringResult == "A";
        if (isAdd)
        {
            xInfo.Workset.Add(idArray);

            // 刷新一次
            // 因为添加时候是解锁状态,只需要平移就等于刷新
            using var tr = DBTrans.Create();
            foreach (var id in idArray)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Move(Point3d.Origin, Point3d.Origin);
            }
        }
        else
        {
            xInfo.Workset.Remove(idArray);

            // 淡显
            // 锁定全部,再把workset给亮回来
            using (var tr = DBTrans.Create())
            {
                Fade(tr, xInfo);
            }

            using (var tr = DBTrans.Create())
            {
                RegenLayers(tr, xInfo);
            }
        }

        // 清空选择集
        Env.Editor.SetImpliedSelection([]);
    }



    /// <summary>
    /// 局部刷新图元
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xInfo"></param>
    private static void RegenLayers(DBTrans tr, RefEditInfo? xInfo = null)
    {
        if (xInfo is not null)
        {
            // 此时已经解锁全部图层,但是没有使用 IFoxUtils.RegenLayers 刷新,
            // 然后我们使用平移图元就会亮显这一部分的图元.
            foreach (var id in xInfo.Workset)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                ent.Move(Point3d.Origin, Point3d.Origin);
            }
        }
        // 刷新这个图层,
        // 即使这个图层没有任何图元,也会触发刷新修改过的图元而不是整个图层
        var eLayer = tr.LayerTable.Add(RefEdit0);
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
        {
            Env.Print("当前没有使用:在位编辑器");
            return;
        }

        // 命令: refclose
        // 输入选项 [保存参照修改(S)/放弃参照修改(D)] <保存参照修改>:

        var pko = new PromptKeywordOptions("\n输入选项 ");
        pko.Keywords.Add("S", "S", "保存参照修改(S)");
        pko.Keywords.Add("D", "D", "放弃参照修改(D)");
        pko.Keywords.Default = "S";

        var result = Env.Editor.GetKeywords(pko);
        if (result.Status != PromptStatus.OK)
        {
            Env.Print("*取消*");
            return;
        }

        // 清理 workset
        using var tr = DBTrans.Create();
        using var brf = (BlockReference)tr.GetObject(xInfo.BlockReferenceId, OpenMode.ForWrite, true, true);

        if (result.StringResult == "D")
        {
            Env.Printl("放弃参照修改");
        }
        else if (result.StringResult == "S")
        {
            // 1,移除原本btr内的图元,是一个块表记录容器,把 workset 设置进去
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            foreach (ObjectId id in btr)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(true);
            }

            // 深度克隆 
            using ObjectIdCollection ids = [.. xInfo.Workset];
            using IdMapping map = [];
            var inv = brf.BlockTransform.Inverse();
            btr.DeepCloneEx(ids, map);
            map.GetValues().ForEach(id => {
                if (!id.IsOk())
                    return;
                var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                ent.TransformBy(inv);
            });

            Env.Printl("保存参照修改");
        }

        // 删除临时图元
        foreach (var id in xInfo.Workset)
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
            if (ent.IsDisposed)
                continue;
            ent.Erase(true);
        }

        // 恢复原有的块参照
        brf.Erase(false);

        // 2,恢复图层锁定的显示
        IFoxUtils.RegenLayers(xInfo.LockedLayers);

        // 3,清空
        RefEditInfo.Map.Remove(doc);

        // TODO 4,此处没有考虑undo的时候怎么恢复?

        // 删除用来临时锁定的图层
        var eLayer = tr.LayerTable.Add(RefEdit0);
        using var ll = (LayerTableRecord)tr.GetObject(eLayer, OpenMode.ForWrite, true, true);
        ll.Erase(true);
    }


    // 模拟,实现一个自己的在位编辑器(长事务)
    [CommandMethod(nameof(RefEdit), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefEdit()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (RefEditInfo.Map.TryGetValue(doc, out var xInfo))
        {
            Env.Print("不能重复使用:在位编辑器");
            return;
        }
        Env.Print($"\n用 {nameof(RefClose)} 或“参照编辑”工具栏来结束参照编辑任务。");

        ObjectId ooid = ObjectId.Null;
        var psr = Env.Editor.SelectImplied();// 预选
        if (psr.Status == PromptStatus.OK)
        {
            var idArray = psr.Value.GetObjectIds();
            if (idArray.Length == 1)
            {
                using var tr = DBTrans.Create();
                using var bent = (Entity)tr.GetObject(idArray[0], OpenMode.ForWrite, true, true);
                if (bent is BlockReference)
                    ooid = idArray[0];
            }
        }

        if (ooid == ObjectId.Null)
        {
            // 让用户只能选择块参照
            var pm = new PromptEntityOptions("\n在位编辑器:选择块参照");
            pm.SetRejectMessage("\n只能选择块参照!");
            pm.AddAllowedClass(typeof(BlockReference), true);
            var per = Env.Editor.GetEntity(pm);
            if (per.Status != PromptStatus.OK)
                return;
            ooid = per.ObjectId;
        }

        xInfo = new RefEditInfo(doc, doc.Database.CurrentSpaceId);

        // 淡显图元
        using (var tr = DBTrans.Create())
        {
            Fade(tr, xInfo);
        }

        using (var tr = DBTrans.Create())
        {
            // 1,删除这个块参照
            using var bent = (Entity)tr.GetObject(ooid, OpenMode.ForWrite, true, true);
            if (bent is not BlockReference brf)
                return;
            brf.Erase(true);

            // 2,提取块内图元出来
            // 此时没有锁定图层,再平移之后(触发修改),它就是亮显的.
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            xInfo.BlockReferenceId = brf.ObjectId;
            // 深度克隆
            using ObjectIdCollection ids = [.. btr];
            using IdMapping map = [];
            tr.CurrentSpace.DeepCloneEx(ids, map);
            map.GetValues().ForEach(id => {
                if (!id.IsOk())
                    return;
                var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                ent.TransformBy(brf.BlockTransform);
                xInfo.Workset.Add(id);
            });

            // 3,触发图元,要平行事务.
            RegenLayers(tr);
        }
    }

    /// <summary>
    /// 淡显全部图元
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xInfo"></param>
    private static void Fade(DBTrans tr, RefEditInfo xInfo)
    {
        // 1,锁定图层
        // 2,刷新图层状态,让锁定的图层的图元是暗显.
        // 3,解锁全部图层,不刷新
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