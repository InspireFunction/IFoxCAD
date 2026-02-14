using IFoxCAD.Cad;
using System.Windows.Controls;
namespace Test;

// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,无法vote()处理.
// 由于move触发之后再选择,可以选择到workset之外的对象,
// 我们只能够在命令触发前事件锁定图层,这样就选择不了,命令结束之后解锁.

public class RefEditInfo
{
    /// <summary>
    /// 全局储存每个文档的在位编辑器状态,包括当前的工作集和历史快照链表等.
    /// </summary>
    public static readonly Dictionary<Document, RefEditInfo> RefeditMap = [];
    /// <summary>
    /// 全局储存每个文档的在位编辑器快照链表历史
    /// </summary>
    public static readonly Dictionary<Document, LinkedList<RefEditInfo>> HistoryMap = [];

    public Document Document { get; }
    public bool IsRun { get; set; } = false;
    public ObjectId BlockReferenceId { get; set; }
    public ObjectId CurrentSpaceId { get; set; }

    public HashSet<ObjectId> Workset { get; internal set; } = [];
    public HashSet<ObjectId> LockedLayers { get; internal set; } = [];
    public HashSet<ObjectId> RefsetAddIds { get; internal set; } = [];
    public HashSet<ObjectId> RefsetRemoveIds { get; internal set; } = [];

    // 命令事件前锁定图层用,不需要加入历史
    // map[图元id,原有图层id]
    public Dictionary<ObjectId, ObjectId> EntityLayerBak = [];

    RefEditInfo(Document document)
    {
        Document = document;
    }

    public static RefEditInfo Create(Document document)
    {
        var info = new RefEditInfo(document);
        RefeditMap.Add(document, info);
        HistoryMap.Add(document, new());
        return info;
    }

    public static void Destroy(Document document)
    {
        RefeditMap.Remove(document);
        HistoryMap.Remove(document);
    }

    /// <summary>
    /// 克隆当前对象，创建深拷贝
    /// </summary>
    /// <returns>当前对象的深拷贝</returns>
    public RefEditInfo Clone()
    {
        var clone = new RefEditInfo(Document)
        {
            RefsetAddIds = [.. RefsetAddIds],
            RefsetRemoveIds = [.. RefsetRemoveIds],
            Workset = [.. Workset],
            LockedLayers = [.. LockedLayers],
            BlockReferenceId = BlockReferenceId,
            CurrentSpaceId = CurrentSpaceId,
            IsRun = IsRun,
            // EntityLayerBak 不需要加入历史,因为它只是命令事件前的临时状态,不需要保存.
        };
        return clone;
    }

    /// <summary>
    /// 重新初始化,需要把原本的储存到一个链表中
    /// </summary>
    internal void AsSaved()
    {
        if (!HistoryMap.TryGetValue(Document, out var hist))
        {
            throw new Exception("该文档没有初始化");
        }

        // 克隆当前对象并添加到链表头部
        var clone = Clone();
        hist.AddFirst(clone);

        // 清空当前状态
        RefsetAddIds.Clear();
        RefsetRemoveIds.Clear();
        Workset.Clear();
        LockedLayers.Clear();
        BlockReferenceId = ObjectId.Null;
        CurrentSpaceId = ObjectId.Null;

        // 不需要加入历史
        EntityLayerBak.Clear();
    }
}

public class RefEditCmd
{
    // 本工程命令要作为例外
    static readonly HashSet<string> _workCmd = new(StringComparer.OrdinalIgnoreCase);
    const string RefEdit0 = "RefEdit-0";

    [IFoxInitialize(Sequence.StartDocs)]
    public void StartDocs(Document doc)
    {
        doc.Database.ObjectAppended += Database_ObjectAppended;
        doc.Database.ObjectErased += Database_ObjectErased;

        doc.CommandWillStart += OnCommandWillStart;
        doc.CommandEnded += OnCommandEnded;
        doc.CommandCancelled += Doc_CommandCancelled;

        RefEditInfo.Create(doc);
    }

    [IFoxInitialize(Sequence.EndDocs)]
    public void EndDocs(Document doc)
    {
        doc.Database.ObjectAppended -= Database_ObjectAppended;
        doc.Database.ObjectErased -= Database_ObjectErased;

        doc.CommandWillStart -= OnCommandWillStart;
        doc.CommandEnded -= OnCommandEnded;
        doc.CommandCancelled -= Doc_CommandCancelled;

        RefEditInfo.Destroy(doc);
    }


    private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (e.DBObject is not Entity)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (!xInfo.IsRun)
            return;
        // 即使重复移除也没有关系,因为是HashSet
        xInfo.Workset.Remove(e.DBObject.ObjectId);
    }

    private void Database_ObjectAppended(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is not Entity ent)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (!xInfo.IsRun)
            return;

        // workset不能够跨空间,而且必须要是图元
        if (xInfo.CurrentSpaceId != ent.BlockId)
            return;
        // 即使重复加入也没有关系,因为是HashSet
        xInfo.Workset.Add(ent.ObjectId);
    }

    // redo重做时候加入对象
    private void Database_ObjectReappended(object sender, ObjectEventArgs e)
    {
        _undoredo.Add(e.DBObject);
    }

    // undo撤回时候移除对象
    // TODO 这里撤回的对象是一个一个触发的,
    // 但是一个命令可能有n个图元.我们历史链表也是一个命令记录一批
    private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
    {
        _undoredo.Add(e.DBObject);
    }

    // 命令取消
    private void Doc_CommandCancelled(object sender, CommandEventArgs e)
    {
        CommandEndedOrCancelled(sender, e);
    }

    // 命令结束
    private void OnCommandEnded(object sender, CommandEventArgs e)
    {
        CommandEndedOrCancelled(sender, e);
    }

    HashSet<DBObject> _undoredo = [];


    // TODO 撤回步数不对
    // 如果遇到refedit就要结束
    // 如果遇到refset就要撤回到上一次refset的状态,并且刷新
    // 如果遇到refclose就要重新进入refedit状态
    void UndoPop()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        // 检查是否有历史记录可以恢复
        if (!RefEditInfo.HistoryMap.TryGetValue(doc, out var hist) || hist.Count == 0)
            return;

        // 从历史链表中恢复最近的状态
        var lastState = hist.First?.Value;
        if (lastState == null)
            return;

        // 恢复 workset 状态
        xInfo.Workset = [.. lastState.Workset];
        xInfo.LockedLayers = [.. lastState.LockedLayers];
        xInfo.RefsetAddIds = [.. lastState.RefsetAddIds];
        xInfo.RefsetRemoveIds = [.. lastState.RefsetRemoveIds];
        xInfo.BlockReferenceId = lastState.BlockReferenceId;
        xInfo.CurrentSpaceId = lastState.CurrentSpaceId;
        xInfo.IsRun = lastState.IsRun;

        // 移除已恢复的历史记录
        // TODO 这里也是不对的,毕竟redo还需要这个历史
        hist.RemoveFirst();

        // 刷新显示
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            HashSet<ObjectId> lockedLayers = [];
            Fade(tr, xInfo, lockedLayers);
        }
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            RegenLayers(tr, xInfo);
        }

        Env.Printl("已撤销到上一次 RefClose 状态");
    }


    private void CommandEndedOrCancelled(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        if (cmd.StartsWith("GRIP_")) // 操作图元夹点的时候会出现两个命令.
            return;

        if (cmd == "U")
        {
            var doc2 = (Document)sender;
            doc2.Database.ObjectUnappended -= Database_ObjectUnappended;
            // TODO 撤回的步数不对...如何跟随官方的????
            // 选择块,refedit,撤回
            UndoPop();
            _undoredo.Clear();
            return;
        }
        if (cmd == "MREDO")
        {
            var doc2 = (Document)sender;
            doc2.Database.ObjectReappended -= Database_ObjectReappended;
            _undoredo.Clear();
            return;
        }

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (_workCmd.Contains(cmd))
        {
            // 记录历史
            // TODO 应该命令前记录一次,命令后记录一次?
            xInfo.AsSaved();
            return;
        }

        // 在位编辑器期间运行官方命令,所做的操作.
        if (!xInfo.IsRun)
            return;
        SetEntityLayerBak(false, xInfo);
    }

    // 命令开始
    // 感觉每个命令都需要处理一次,实在有点太过分耶.
    // 是不是应该做一些批量处理的操作?
    // 例如是sendCommand或者Lisp期间就不执行?
    private void OnCommandWillStart(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        if (cmd.StartsWith("GRIP_")) // 操作图元夹点的时候会出现两个命令.
            return;

        if (cmd == "U")
        {
            var doc2 = (Document)sender;
            _undoredo.Clear();
            doc2.Database.ObjectUnappended += Database_ObjectUnappended;
            return;
        }
        if (cmd == "MREDO")
        {
            var doc2 = (Document)sender;
            _undoredo.Clear();
            doc2.Database.ObjectReappended += Database_ObjectReappended;
            return;
        }

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (_workCmd.Contains(cmd))
        {
            // 记录历史
            // TODO 应该命令前记录一次,命令后记录一次?
            //xInfo.AsSaved();
            return;
        }

        if (!xInfo.IsRun)
            return;

        // 重设选择集这种方式无法处理 先move再选择,因此非workset的图元锁定图层
        //var prompt = Env.Editor.SelectImplied();
        //if (prompt.Status == PromptStatus.OK)
        //{
        //    // 获取工作集部分,然后才能执行官方命令/其他Lisp命令
        //    // 重设选择集
        //    var list = prompt.Value.GetObjectIds().Where(xInfo.Workset.Contains).ToArray();
        //    Env.Editor.SetImpliedSelection(list);
        //}

        if (xInfo.EntityLayerBak.Count > 0) // 防止重入
            return;
        SetEntityLayerBak(true, xInfo);
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
    /// <summary>
    /// 设置临时图元到容器
    /// </summary>
    /// <param name="isStart"></param>
    /// <param name="xInfo"></param>
    private void SetEntityLayerBak(bool isStart, RefEditInfo xInfo)
    {
        if (isStart)
        {
            // 备份非workset的图元图层,
            // 并修改为 refedito0 再锁定.官方命令就选择不了.
            using var tr = DBTrans.Create(openCloseTrans: true);
            var refLayerId = tr.LayerTable.Add(RefEdit0);
            foreach (var id in tr.CurrentSpace)
            {
                if (xInfo.Workset.Contains(id))
                    continue;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                xInfo.EntityLayerBak[id] = ent.LayerId;
                ent.LayerId = refLayerId;
            }

            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.IsLocked = true;
        }
        else
        {
            // 非workset的图元图层,还原
            using var tr = DBTrans.Create(openCloseTrans: true);
            var refLayerId = tr.LayerTable.Add(RefEdit0);
            foreach (var id in tr.CurrentSpace)
            {
                if (xInfo.Workset.Contains(id))
                    continue;
                if (!xInfo.EntityLayerBak.TryGetValue(id, out var oldLayer))
                    continue;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                if (refLayerId == ent.LayerId)
                    ent.LayerId = oldLayer;
            }

            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.IsLocked = false;

            xInfo.EntityLayerBak.Clear();
        }

        // 重新淡显全部-再亮显workset
        HashSet<ObjectId> lockedLayers = [];
        using (var tr = DBTrans.Create(openCloseTrans: true)) // 使用无撤事务
        {
            Fade(tr, xInfo, lockedLayers);
        }
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            RegenLayers(tr, xInfo);
        }
    }

    private static bool TryGetRefEditInfo(Document doc, out RefEditInfo xInfo)
    {
        if (!RefEditInfo.RefeditMap.TryGetValue(doc, out xInfo))
        {
            Env.Print("没有初始化此文档的在位编辑器");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 淡显全部图元
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xInfo"></param>
    private static void Fade(DBTrans tr, RefEditInfo xInfo, HashSet<ObjectId> lockedLayers)
    {
        // 1,锁定图层
        // 2,刷新图层状态,让锁定的图层的图元是暗显.
        // 3,解锁全部图层,不刷新
        tr.LayerTable.ForEach((layer, state) => {
            if (!layer.IsLocked)
            {
                layer.IsLocked = true;
                lockedLayers.Add(layer.ObjectId);
            }
        }, OpenMode.ForWrite);

        // 刷新画面的图层暗显
        IFoxUtils.RegenLayers(lockedLayers);

        // 解锁图层
        tr.LayerTable.ForEach((layer, state) => {
            if (lockedLayers.Contains(layer.ObjectId))
                layer.IsLocked = false;
        }, OpenMode.ForWrite);
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
        var refLayerId = tr.LayerTable.Add(RefEdit0);
        var lays = new List<ObjectId> { refLayerId };
        IFoxUtils.RegenLayers(lays);
    }


#if true
    [IFoxInitialize]
    public static void Init(Document _)
    {
        _workCmd.Add(nameof(RefSet));
        _workCmd.Add(nameof(RefClose));
        _workCmd.Add(nameof(RefEdit));
    }

    [CommandMethod(nameof(RefSet), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefSet()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        Env.Printl("\n在参照编辑工作集和宿主图形之间传输对象...");

        if (!xInfo.IsRun)
        {
            Env.Print("当前没有使用:在位编辑器");
            return;
        }

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
                MessageForAdding = "\n选择对象: ",
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
        if (result.StringResult == "A")
        {
            var sets = new HashSet<ObjectId>();
            foreach (var item in idArray)
            {
                if (xInfo.Workset.Add(item))
                    sets.Add(item);
            }

            // 成功的部分放入
            xInfo.RefsetAddIds.Add(sets);

            // 刷新一次
            // 因为添加时候是解锁状态,只需要平移就等于刷新
            using var tr = DBTrans.Create(openCloseTrans: true);
            foreach (var id in sets)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Move(Point3d.Origin, Point3d.Origin);
            }
        }
        else
        {
            var sets = new HashSet<ObjectId>();
            foreach (var item in idArray)
            {
                if (xInfo.Workset.Remove(item))
                    sets.Add(item);
            }

            // 成功的部分放入
            xInfo.RefsetRemoveIds.Add(sets);

            // 淡显
            // 锁定全部,再把workset给亮回来
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                Fade(tr, xInfo, xInfo.LockedLayers);
            }
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                RegenLayers(tr, xInfo);
            }
        }

        // 清空选择集
        Env.Editor.SetImpliedSelection([]);
    }

    [CommandMethod(nameof(RefClose), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefClose()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (!xInfo.IsRun)
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
            // 放弃修改时候
            // 通过 refset 移除出去的图元要删掉啊
            foreach (var id in xInfo.RefsetRemoveIds)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(true);
            }

            // 通过 refset 添加进来的图元要恢复啊
            foreach (var id in xInfo.RefsetAddIds)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(false);

                // 这里不要剔除任何容器内容,否则造成 快照 记录错误.
                // xInfo.Workset.Remove(id);
            }

            // 删除临时图元
            foreach (var id in xInfo.Workset)
            {
                // 只能这里跳过
                if (xInfo.RefsetAddIds.Contains(id))
                    continue;
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(true);
            }

            Env.Printl("放弃参照修改");
        }
        else if (result.StringResult == "S")
        {
            // 移除原本块表记录内的图元
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            foreach (ObjectId id in btr)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(true);
            }

            // 深度克隆到块表记录
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

            // 删除临时图元
            foreach (var id in xInfo.Workset)
            {
                using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                if (ent.IsDisposed)
                    continue;
                ent.Erase(true);
            }

            Env.Printl("保存参照修改");
        }

        // 恢复原有的块参照
        brf.Erase(false);

        // 恢复图层锁定的显示
        IFoxUtils.RegenLayers(xInfo.LockedLayers);

        // 删除用来临时锁定的图层
        var refLayerId = tr.LayerTable.Add(RefEdit0);
        using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
        refLayer.Erase(true);

        xInfo.IsRun = false;
    }

    // 模拟,实现一个自己的在位编辑器(长事务)
    [CommandMethod(nameof(RefEdit), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefEdit()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (xInfo.IsRun)
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

        // 前台应该用这个啊,为什么这个是错误的呢?难道是它的块表记录?先不管了
        //var layoutId = LayoutManager.Current.GetLayoutId(LayoutManager.Current.CurrentLayout);

        // 这个确实是对的
        var a = doc.Database.CurrentSpaceId;
        xInfo.CurrentSpaceId = a;
        xInfo.IsRun = true;

        // 1,淡显图元
        using (var tr = DBTrans.Create())
        {
            Fade(tr, xInfo, xInfo.LockedLayers);
        }

        // 2,修改数据库
        using (var tr = DBTrans.Create())
        {
            // 删除这个块参照
            using var bent = (Entity)tr.GetObject(ooid, OpenMode.ForWrite, true, true);
            if (bent is not BlockReference brf)
                return;
            brf.Erase(true);

            // 深度克隆提取块内图元出来
            // 此时没有锁定图层,再平移之后(触发修改),它就是亮显的.
            using var btr = (BlockTableRecord)tr.GetObject(brf.BlockTableRecord, OpenMode.ForWrite, true, true);
            xInfo.BlockReferenceId = brf.ObjectId;
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
        }

        // 3,触发刷新图层就会局部亮显
        using (var tr = DBTrans.Create())
        {
            RegenLayers(tr, xInfo);
        }
    }
#endif
}