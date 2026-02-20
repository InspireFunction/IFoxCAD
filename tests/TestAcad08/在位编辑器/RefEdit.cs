namespace Test;

// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,无法vote()处理.
// 由于move触发之后再选择,可以选择到workset之外的对象,
// 我们只能够在命令触发前事件锁定图层,这样就选择不了,命令结束之后解锁.

// Git风格的历史记录方案：工作区 + 不可变历史表

// 写undo容易,redo难.不知道什么时候污染了历史,造成redo是0.
// RefEdit-undo-redo 此时重做无法实现...
// OK了,是命令后事件刷新启动了一个大范围的无撤事务导致的.
// 为什么现在在位编辑器内画圆-undo-之后无法redo?
// 发现是命令后事件,因为undo时候使用了刷新图层函数导致的,无解...

public class RefEditCmd
{
    // 本工程命令要作为例外
    static readonly HashSet<string> _workCmd = new(StringComparer.OrdinalIgnoreCase);

    [IFoxInitialize(Sequence.StartDocs)]
    public void StartDocs(Document doc)
    {
        doc.Database.ObjectAppended += Database_ObjectAppended;
        doc.Database.ObjectErased += Database_ObjectErased;
        doc.Database.ObjectUnappended += Database_ObjectUnappended;
        doc.Database.ObjectReappended += Database_ObjectReappended;

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
        doc.Database.ObjectUnappended -= Database_ObjectUnappended;
        doc.Database.ObjectReappended -= Database_ObjectReappended;

        doc.CommandWillStart -= OnCommandWillStart;
        doc.CommandEnded -= OnCommandEnded;
        doc.CommandCancelled -= Doc_CommandCancelled;

        // 关闭图纸时候要清理历史字典.
        RefEditInfo.Destroy(doc);
    }

    /// <summary>
    /// 对象删除事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (e.DBObject is not Entity)
            return;

        // 删除对象 和 refclose 都会触发这里,
        // refclose 需要特殊处理,开事务报错,因此此时是 CtrlState.IsStop
        if (!xInfo.CtrlState.IsRun)
            return;

        // 在位编辑器期间,工作区移除对象,并写入历史,制作回滚点.
        if (e.Erased)
        {
            if (xInfo.Workset.Remove(e.DBObject.ObjectId))
            {
                var tr = e.DBObject.Database.TransactionManager.TopTransaction;
                xInfo.HistoryWrite("Database_ObjectErased", tr);
            }
        }
        else
        {
            // 在位编辑-画圆-删除圆-撤回.
            // 需要把这个圆添加回去
            if (xInfo.Workset.Add(e.DBObject.ObjectId))
            {
                var tr = e.DBObject.Database.TransactionManager.TopTransaction;
                xInfo.HistoryWrite("Database_ObjectErased_undo", tr);
            }
        }
    }

    /// <summary>
    /// 对象添加事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Database_ObjectAppended(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is not Entity ent)
            return;
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (!xInfo.CtrlState.IsRun)
            return;

        // workset不能够跨空间,而且必须要是图元
        if (xInfo.CurrentSpaceId != ent.BlockId)
            return;

        // 在位编辑期间,工作区加入对象,并写入历史,制作回滚点.
        if (xInfo.Workset.Add(ent.ObjectId))
        {
            // 如果不追加历史,在位编辑-画圆-画圆 回滚到1,
            // 相当于git的工作区没有保存,然后直接刷新了,没有清空工作区.
            // 需要每次都追加历史标记

            // 这里不报事务错误,所以不需要发送命令
            // 记录历史会设置回滚点
            xInfo.HistoryWrite("Refedit_ObjectAppended");
        }
    }

    /// <summary>
    /// Redo重做时触发 - 从UndoMarker恢复历史索引
    /// </summary>
    private void Database_ObjectReappended(object sender, ObjectEventArgs e)
    {
        var db = (Database)sender;
        var doc = Acap.DocumentManager.GetDocument(db);
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        // 1,有对象顺序,先触发这个
        if (e.DBObject is DBDictionary markerDict)
        {
            var index = UndoMarker.UndoMarkNodRead(markerDict);
            if (index == -1)
                return;

            var targetNode = xInfo.GetNodeByIndex(index);
            if (targetNode != null)
            {
                xInfo.SetCurrentNode(targetNode);
                // 刷新在命令后事件中处理,但是导致redo问题
            }
            return;
        }

        // 2,再触发这个
        if (e.DBObject is Entity ent)
        {
            // 可能是添加/删除
            if (e.DBObject.IsErased)
            {
                xInfo.Workset.Remove(ent.ObjectId);
            }
            else
            {
                xInfo.Workset.Add(ent.ObjectId);
            }
            return;
        }
    }

    /// <summary>
    /// Undo撤销时触发 - 从UndoMarker恢复历史索引
    /// </summary>
    private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
    {
        var db = (Database)sender;
        var doc = Acap.DocumentManager.GetDocument(db);
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        // 不能判断IsRun,因为refclose回滚需要
        // DebugEx.Printl($"e.DBObject is {e.DBObject.GetType().Name}");

        // 1,有对象顺序,先触发这个
        // 用可撤事务来追踪官方的撤回索引位置
        if (e.DBObject is DBDictionary markerDict)
        {
            var index = UndoMarker.UndoMarkNodRead(markerDict);
            if (index == -1)
                return;

            if (xInfo.CurrentIndex == index)
                return;

            var targetNode = xInfo.GetNodeByIndex(index);
            if (targetNode != null)
            {
                xInfo.SetCurrentNode(targetNode);
                if (!xInfo.CtrlState.IsRun)
                {
                    // 回滚到refedit了
                    // Debugger.Break();
                }
                // 刷新在命令后事件中处理,但是导致redo问题,无解.
            }
            return;
        }

        // 2,再触发这个
        if (e.DBObject is Entity ent)
        {
            // 可能是添加/删除
            if (!e.DBObject.IsErased) // 这里需要取反,已经测试
            {
                xInfo.Workset.Remove(ent.ObjectId);
            }
            else
            {
                xInfo.Workset.Add(ent.ObjectId);
            }
            return;
        }
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

    private void CommandEndedOrCancelled(object sender, CommandEventArgs e)
    {
        var cmd = e.GlobalCommandName;
        if (cmd.StartsWith("GRIP_")) // 操作图元夹点的时候会出现两个命令.
            return;
        if (cmd == "PROPERTIES" || cmd == "SELECT")
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (cmd == "U" || cmd == "MREDO")
        {
            // 如果回滚到在位编辑器外,此时workset是没有图元的,锁图层就会全灰了,
            // 因此此时什么也不干就行了.
            if (xInfo.CtrlState.IsRun)
            {
#if true
                // 动作: 在位编辑器期间-画圆-undo撤回-redo重做,
                // 刷新会导致无法redo重做了.

                // 使用事务也会中断历史.
                //using (var tr = DBTrans.Create(openCloseTrans: true))
                //{
                //}

                // 方案一:无撤标记+事务.
                // xInfo.RefreshDisplay();

                // 方案二:发送命令.
                // doc.SendStringToExecute(nameof(RefreshDisplay) + "\n", false, false, false);

                // 方案三:无撤标记+Open/Close对象.
                // RefreshDisplay2(xInfo);
#endif
            }
            return;
        }

        if (_workCmd.Contains(cmd))
            return;
        if (!xInfo.CtrlState.IsRun)
            return;

        // 在位编辑器期间运行官方命令,所做的操作.
        SetEntityLayerBak(false, xInfo);
    }


    /// <summary>
    /// 无撤方式刷新
    /// </summary>
    /// <param name="xInfo"></param>
    void RefreshDisplay2(RefEditInfo xInfo)
    {
        // 1,锁定图层
        // 2,刷新图层状态,让锁定的图层的图元是暗显.
        // 3,解锁全部图层,不刷新
        HashSet<ObjectId> lockedLayers = [];

        Database db = xInfo.Document.Database;
        db.DisableUndoRecording(true);
        try
        {
            // 打开图层表进行写操作
            // 锁定图层
            ObjectId layerTableId = db.LayerTableId;
#pragma warning disable CS0618 // 类型或成员已过时
            using (var layerTable = (LayerTable)layerTableId.Open(OpenMode.ForWrite, true, true))
            {
                foreach (ObjectId layerId in layerTable)
                {
                    using var layer = (LayerTableRecord)layerId.Open(OpenMode.ForWrite, true, true);
                    if (!layer.IsLocked)
                    {
                        layer.IsLocked = true;
                        lockedLayers.Add(layer.ObjectId);
                    }
                }
            }
#pragma warning restore CS0618 // 类型或成员已过时

#if false
            // 方案三a
            // 一旦使用这个就无法redo了... 

            // 刷新画面的图层暗显
            IFoxUtils.RegenLayers(lockedLayers); 
#endif

#if false
            // 方案三b
            const string str = "LayLockFadectl";
            var value = int.Parse(Acap.GetSystemVariable(str).ToString());
            Acap.SetSystemVariable(str, (value * -1).ToString()); // 这里致命错误

            // 改为遍历当前空间全部图元,无法触发显示更新...妈耶....
            using (var msps = (BlockTableRecord)db.CurrentSpaceId.Open(OpenMode.ForWrite, true, true))
            {
                foreach (var id in msps)
                {
                    if (!id.IsOk())
                        continue;
                    using var ent = (Entity)id.Open(OpenMode.ForWrite, true, true);
                    if (ent.IsDisposed)
                        continue;
                    ent.Draw();
                    ent.RecordGraphicsModified(true);
                }
            }

            // acad2014及以上要加,立即处理队列上面的消息
            System.Windows.Forms.Application.DoEvents();

            Acap.SetSystemVariable(str, (value * -1).ToString()); 
#endif

            // 解锁图层
            foreach (ObjectId layerId in lockedLayers)
            {
#pragma warning disable CS0618 // 类型或成员已过时
                using var layer = (LayerTableRecord)layerId.Open(OpenMode.ForWrite, true, true);
#pragma warning restore CS0618 // 类型或成员已过时
                layer.IsLocked = false;
            }
        }
        finally
        {
            db.DisableUndoRecording(false);
        }
    }


    // 无历史命令刷新显示
    // 还是会导致无法redo
    [CommandMethod(nameof(RefreshDisplay), CommandFlags.NoHistory)]
    public void RefreshDisplay()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        xInfo.RefreshDisplay();
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
        if (cmd == "PROPERTIES" || cmd == "SELECT")
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (cmd == "U" || cmd == "MREDO")
            return;

        if (_workCmd.Contains(cmd))
            return;
        if (!xInfo.CtrlState.IsRun)
            return;

        SetEntityLayerBak(true, xInfo);
    }

    /*
    * 重设选择集方式无法处理 先move再选择 因此非workset的图元锁定图层
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
            if (xInfo.EntityLayerBak.Count > 0) // 防止重入
                return;

            // 备份非 workset 的图元图层,
            // 并修改为 refedito0 再锁定.官方命令就选择不了.
            using var tr = DBTrans.Create(openCloseTrans: true);
            var refLayerId = tr.LayerTable.Add(RefEditInfo.RefEdit0);
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
            // 非 workset 的图元图层,还原
            using var tr = DBTrans.Create(openCloseTrans: true);
            var refLayerId = tr.LayerTable.Add(RefEditInfo.RefEdit0);
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

        xInfo.RefreshDisplay();
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


#if true
    [IFoxInitialize]
    public static void Init(Document _)
    {
        _workCmd.Add(nameof(RefSet));
        _workCmd.Add(nameof(RefClose));
        _workCmd.Add(nameof(RefEdit));
        _workCmd.Add(nameof(RefEditDebug));
    }

    [CommandMethod(nameof(RefSet), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefSet()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        Env.Printl("\n在参照编辑工作集和宿主图形之间传输对象...");

        if (!xInfo.CtrlState.IsRun)
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

            // 保存历史
            xInfo.HistoryWrite(nameof(RefSet) + "_Add");

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

            // 保存历史
            xInfo.HistoryWrite(nameof(RefSet) + "_Remove");

            // 淡显
            // 锁定全部,再把workset给亮回来
            xInfo.RefreshDisplay();
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

        if (!xInfo.CtrlState.IsRun)
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
        var a = xInfo.Workset.Count;

        // 清理 workset
        using (var tr = DBTrans.Create())
        {
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

                // 保存历史
                xInfo.HistoryWrite(nameof(RefClose) + "_Before_Abandon");

                // 删除临时图元,会联动事件移除workset的
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

                // 过滤掉已删除的图元,只保留有效的 ObjectId
                var validIds = new ObjectIdCollection();
                foreach (var id in xInfo.Workset)
                {
                    if (id.IsNull || id.IsEffectivelyErased)
                        continue;
                    var obj = tr.GetObject(id, OpenMode.ForRead, false, true);
                    if (obj != null && !obj.IsDisposed && !obj.IsErased)
                        validIds.Add(id);
                }

                // 深度克隆到块表记录
                using IdMapping map = [];
                var inv = brf.BlockTransform.Inverse();
                btr.DeepCloneEx(validIds, map);
                map.GetValues().ForEach(id => {
                    if (!id.IsOk())
                        return;
                    var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                    ent.TransformBy(inv);
                });

                // 保存历史(防止workset删除时候,触发数据库事件移除)
                xInfo.HistoryWrite(nameof(RefClose) + "_Before_Save");

                // RefEdit-画圆-撤回 为什么内部撤回了一次,会导致保存时候丢失全部图元呢?
                // 原因是没有把画圆添加到历史,在添加数据库上面加上了.
                foreach (var id in xInfo.Workset)
                {
                    if (id.IsNull || id.IsEffectivelyErased)
                        continue;
                    using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                    if (ent.IsDisposed || ent.IsErased)
                        continue;
                    ent.Erase(true);
                }

                Env.Printl("保存参照修改");
            }

            // 恢复原有的块参照
            brf.Erase(false);

            // 恢复 refedit 命令 的淡显
            IFoxUtils.RegenLayers(xInfo.LockedLayers);

            // 删除用来临时锁定的图层
            var refLayerId = tr.LayerTable.Add(RefEditInfo.RefEdit0);
            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.Erase(true);

            a = xInfo.Workset.Count; // 3

            // 此处有删除对象,提交事务之后会触发删除事件,
            // 但是此处引起的删除对象事件不能开事务否则会错误,不能记录历史.
            // 设置一个标记让删除对象事件跳过.
            xInfo.CtrlState.Break();
        }

        a = xInfo.Workset.Count; // 这里变成0了,因为这里删除对象事件导致的

        // 这里会把标志初始化了,需要再次设置成停止.
        xInfo.Reset();
        xInfo.CtrlState.Stop();

        // 再次保存历史
        xInfo.HistoryWrite(nameof(RefClose) + "_After");

        // 恢复窗口标题
        xInfo.RestoreWindowTitle();
    }

    // 模拟,实现一个自己的在位编辑器(长事务)
    [CommandMethod(nameof(RefEdit), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void RefEdit()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (xInfo.CtrlState.IsRun)
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
                using var bent = (Entity)tr.GetObject(idArray[0], OpenMode.ForRead, true, true);
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

        // 初始化记录,通常是0号节点,以及多次运行refedit的节点.
        xInfo.HistoryInit();
        xInfo.HistoryWrite(nameof(RefEdit) + "_Init");

        // 这个空间id确实是对的
        var spaceId = doc.Database.CurrentSpaceId;
        xInfo.CurrentSpaceId = spaceId;

        xInfo.CtrlState.Start();

        // 1,修改数据库
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

        // 2,淡显
        xInfo.RefreshDisplay(xInfo.LockedLayers);

        // 保存初始workset到字典
        xInfo.HistoryWrite(nameof(RefEdit));

        // 3,修改窗口标题为在位编辑器运行中状态
        xInfo.SetRefEditWindowTitle(doc);
    }


    // 清理在位编辑器的历史回滚标记
    [CommandMethod(nameof(RefClear))]
    public void RefClear()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (xInfo.CtrlState.IsRun)
        {
            Env.Print("在位编辑器运行中,不允许清理字典的历史");
            return;
        }

        // 1,清理回滚标记字典,删除字典也是会记录到官方的undo的,只要不重置指针就好了
        UndoMarker.Clear(doc);

        // 2,发送保存命令
        //doc.SendStringToExecute("_QSAVE ", false, false, false);
        //if (File.Exists(doc.Name))
        //{
        //    doc.CloseAndSave(document.Name);
        //}

        // 3,破坏undo,使得用户无法撤回.

        Env.Print("历史记录已清除");
    }


    [CommandMethod(nameof(RefEditDebug))]
    public void RefEditDebug()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        xInfo.PrintHistoryChain();
    }
#endif
}