using IFoxCAD.Cad;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Test;


// 此处的淡显已经成功.
// 三个数据库事件,新增/删除/修改,无法vote()处理.
// 由于move触发之后再选择,可以选择到workset之外的对象,
// 我们只能够在命令触发前事件锁定图层,这样就选择不了,命令结束之后解锁.

// 要用无撤事务在数据库字典上面储存这些历史,否则我无法跟随acad的undo和redo
// 要用可撤事务(普通事务)记录一个撤回标记,否则undo时候不知道触发到哪里了,我们要通过索引值重新载入,并且刷新.

public class RefEditInfo
{
    /// <summary>
    /// 主字典常量名称
    /// </summary>
    public const string MainNameWorkset = "RefEdit_Workset";
    public const string MainNameUndo = "RefEdit_Undo";

    /// <summary>
    /// 临时图层常量名称
    /// </summary>
    public const string RefEdit0 = "RefEdit-0";

    /// <summary>
    /// 全局储存每个文档的在位编辑器状态,包括当前的工作集和历史快照链表等.
    /// </summary>
    public static readonly Dictionary<Document, RefEditInfo> RefeditMap = [];

    public Document Document { get; }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    [XrecordProperty(DxfCode.ExtendedDataInteger16, SkipNull = false)]
    public bool IsRun { get; set; } = false;

    /// <summary>
    /// 块参照ID
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public ObjectId BlockReferenceId { get; set; }

    /// <summary>
    /// 当前空间ID
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public ObjectId CurrentSpaceId { get; set; }

    /// <summary>
    /// 工作集
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public HashSet<ObjectId> Workset { get; internal set; } = [];

    /// <summary>
    /// 锁定的图层
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public HashSet<ObjectId> LockedLayers { get; internal set; } = [];

    /// <summary>
    /// 参照集添加的ID
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public HashSet<ObjectId> RefsetAddIds { get; internal set; } = [];

    /// <summary>
    /// 参照集移除的ID
    /// </summary>
    [XrecordProperty(DxfCode.SoftPointerId)]
    public HashSet<ObjectId> RefsetRemoveIds { get; internal set; } = [];

    /// <summary>
    /// 触发历史记录的命令名称
    /// </summary>
    [XrecordProperty(DxfCode.ExtendedDataAsciiString)]
    public string CommandName { get; set; } = string.Empty;

    // 命令事件前锁定图层用,不需要加入历史
    // map[图元id,原有图层id]
    public Dictionary<ObjectId, ObjectId> EntityLayerBak = [];

    // 存储 workset 历史到数据库
    private ObjectId _worksetNod = ObjectId.Null;
    // 回滚字典
    private ObjectId _undoMarkNod = ObjectId.Null;

    // 当前在历史链条中的索引
    private int _currentIndex = -1;

    /// <summary>
    /// 当前历史索引（用于调试）
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// 历史记录总数（用于调试）
    /// </summary>
    public int HistoryCount
    {
        get
        {
            if (_worksetNod.IsNull)
                return 0;
            try
            {
                using var tr = DBTrans.Create(openCloseTrans: true);
                var worksetDict = (DBDictionary)tr.GetObject(_worksetNod, OpenMode.ForRead);
                return GetExistingIndices(worksetDict).Count;
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// 打印当前历史链信息（用于调试）
    /// </summary>
    public void PrintHistoryChain()
    {
        if (_worksetNod.IsNull)
        {
            Env.Printl("历史字典ID为空，无历史记录");
            return;
        }

        Env.Printl("");
        Env.Print("========== RefEdit 历史链调试信息 ==========");
        Env.Print($"当前索引: {_currentIndex}");
        Env.Print($"历史记录总数: {HistoryCount}");
        Env.Print($"IsRun: {IsRun}");
        Env.Print($"BlockReferenceId: {BlockReferenceId}");
        Env.Print($"CurrentSpaceId: {CurrentSpaceId}");
        Env.Print($"Workset 数量: {Workset.Count}");
        Env.Print($"LockedLayers 数量: {LockedLayers.Count}");
        Env.Print($"RefsetAddIds 数量: {RefsetAddIds.Count}");
        Env.Print($"RefsetRemoveIds 数量: {RefsetRemoveIds.Count}");

        try
        {
            using var tr = DBTrans.Create(openCloseTrans: true);
            var worksetDict = (DBDictionary)tr.GetObject(_worksetNod, OpenMode.ForRead);
            var indices = GetExistingIndices(worksetDict);
            indices.Sort();

            Env.Print("----- 历史链详情 -----");
            foreach (var index in indices)
            {
                string key = $"Workset_{index}";
                string marker = index == _currentIndex ? " <-- 当前" : "";

                string cmdName = "";
                if (worksetDict.Contains(key))
                {
                    var xrec = (Xrecord)tr.GetObject(worksetDict.GetAt(key), OpenMode.ForRead);
                    var propValues = XrecordSerializer.DeserializeToDictionary(xrec.Data);

                    // 获取命令名称
                    if (propValues.TryGetValue("CommandName", out var cmdValue) && cmdValue is string cmd)
                    {
                        cmdName = string.IsNullOrEmpty(cmd) ? "" : $" [{cmd}]";
                    }

                    Env.Print($"[{key}]{cmdName}{marker}");

                    foreach (var kvp in propValues)
                    {
                        string propName = kvp.Key;
                        var value = kvp.Value;

                        // 跳过已显示的CommandName
                        if (propName == "CommandName")
                            continue;

                        if (value is int count)
                            Env.Print($"  {propName}: {count} 个对象");
                        else if (value is bool boolVal)
                            Env.Print($"  {propName}: {boolVal}");
                        else if (value is ObjectId oid)
                            Env.Print($"  {propName}: {oid}");
                        else if (value == null)
                            Env.Print($"  {propName}: (空)");
                        else
                            Env.Print($"  {propName}: {value}");
                    }
                }
                else
                {
                    Env.Print($"[{key}]{marker}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Env.Print($"读取历史链出错: {ex.Message}");
        }

        Env.Printl("============================================");
    }

    RefEditInfo(Document document)
    {
        Document = document;
    }

    public static RefEditInfo Create(Document document)
    {
        var info = new RefEditInfo(document);
        RefeditMap.Add(document, info);
        return info;
    }

    public static void Destroy(Document document)
    {
        if (RefeditMap.TryGetValue(document, out var info))
        {
            // 清理数据库中的历史记录
            info.ClearWorksetStorage();
            RefeditMap.Remove(document);
        }
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <returns>当前对象的深拷贝</returns>
    //public RefEditInfo Clone()
    //{
    //    var clone = new RefEditInfo(Document)
    //    {
    //        RefsetAddIds = [.. RefsetAddIds],
    //        RefsetRemoveIds = [.. RefsetRemoveIds],
    //        Workset = [.. Workset],
    //        LockedLayers = [.. LockedLayers],
    //        BlockReferenceId = BlockReferenceId,
    //        CurrentSpaceId = CurrentSpaceId,
    //        IsRun = IsRun,
    //        // EntityLayerBak 不需要加入历史,因为它只是命令事件前的临时状态,不需要保存.
    //    };
    //    return clone;
    //}


    /// <summary>
    /// 写入历史,数据库主字典
    /// </summary>
    /// <param name="commandName">触发历史记录的命令名称</param>
    public void HistoryWrite(string commandName = "")
    {
        if (_worksetNod.IsNull)
            throw new System.Exception("历史 _worksetDictId.IsNull 造成无法回滚");

        // 设置命令名称
        CommandName = commandName;

        // 无撤事务
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            var worksetDictObj = (DBDictionary)tr.GetObject(_worksetNod, OpenMode.ForWrite);

            List<int> existingIndices = [];
            foreach (DictionaryEntry entry in worksetDictObj)
            {
                string key = entry.Key.ToString();
                if (key.StartsWith("Workset_") && int.TryParse(key[8..], out int index))
                    existingIndices.Add(index);
            }

            int maxIndex = existingIndices.Count > 0 ? existingIndices.Max() : -1;

            foreach (var index in existingIndices)
            {
                if (index > _currentIndex)
                {
                    string key = $"Workset_{index}";
                    if (worksetDictObj.Contains(key))
                        worksetDictObj.Remove(key);
                }
            }

            int newIndex = maxIndex + 1;
            var xrec = new Xrecord();
            worksetDictObj.SetAt($"Workset_{newIndex}", xrec);
            tr.AddNewlyCreatedDBObject(xrec, true);

            xrec.Data = XrecordSerializer.Serialize(this);
            _currentIndex = newIndex;
        }

        // 可撤事务
        // 撤销时会触发 ObjectUnappended 事件,我们就能捕获到,然后从而知道当前撤销到了哪个索引了.
        using (var tr = DBTrans.Create())
        {
            var worksetDictObj = (DBDictionary)tr.GetObject(_undoMarkNod, OpenMode.ForWrite);

            // 可撤事务,字典套字典存储索引值
            // undo 时会触发 ObjectUnappended 事件，我们就能捕获到
            var indexDict = new DBDictionary();
            // XData需要以1001开头的扩展数据，注册应用名
            indexDict.XData = new ResultBuffer(
                new TypedValue(1001, MainNameUndo),
                new TypedValue((int)DxfCode.ExtendedDataInteger32, _currentIndex)
            );
            worksetDictObj.SetAt("Index", indexDict);
            tr.AddNewlyCreatedDBObject(indexDict, true);
        }
    }


    /// <summary>
    /// 从数据库读取 workset 状态
    /// </summary>
    /// <param name="isUndo">true 为撤销（读取上一个状态），false 为重做（读取下一个状态）</param>
    /// <returns>是否成功读取</returns>
    public bool LoadWorksetFromDatabase(bool isUndo)
    {
        if (_worksetNod.IsNull)
            return false;

        using var tr = DBTrans.Create(openCloseTrans: true);
        var worksetDict = (DBDictionary)tr.GetObject(_worksetNod, OpenMode.ForRead);

        var existingIndices = GetExistingIndices(worksetDict);
        if (existingIndices.Count == 0)
            return false;

        existingIndices.Sort();

        int currentPosition = existingIndices.IndexOf(_currentIndex);
        int targetIndex = isUndo
            ? (currentPosition > 0 ? existingIndices[currentPosition - 1] : -1)
            : (currentPosition >= 0 && currentPosition < existingIndices.Count - 1 ? existingIndices[currentPosition + 1] : -1);

        if (targetIndex < 0)
            return false;

        string targetKey = $"Workset_{targetIndex}";
        if (!worksetDict.Contains(targetKey))
            return false;

        var xrec = (Xrecord)tr.GetObject(worksetDict.GetAt(targetKey), OpenMode.ForRead);
        Clear();
        XrecordSerializer.Deserialize(xrec.Data, this);

        _currentIndex = targetIndex;
        return true;
    }

    /// <summary>
    /// 获取字典中所有现有的 Workset 索引
    /// </summary>
    private static List<int> GetExistingIndices(DBDictionary worksetDict)
    {
        var indices = new List<int>();
        foreach (DictionaryEntry entry in worksetDict)
        {
            string key = entry.Key.ToString();
            if (key.StartsWith("Workset_") && int.TryParse(key[8..], out int index))
                indices.Add(index);
        }
        return indices;
    }

    /// <summary>
    /// 设置当前历史索引
    /// </summary>
    /// <param name="index">要设置的索引值</param>
    public void SetCurrentIndex(int index)
    {
        _currentIndex = index;
    }

    /// <summary>
    /// 根据指定索引从数据库加载 workset 状态
    /// </summary>
    /// <param name="targetIndex">目标索引</param>
    /// <returns>是否成功加载</returns>
    public bool LoadWorksetFromDatabaseByIndex(int targetIndex)
    {
        if (_worksetNod.IsNull)
            return false;

        if (targetIndex < 0)
            return false;

        using var tr = DBTrans.Create(openCloseTrans: true);
        var worksetDict = (DBDictionary)tr.GetObject(_worksetNod, OpenMode.ForRead);

        string targetKey = $"Workset_{targetIndex}";
        if (!worksetDict.Contains(targetKey))
            return false;

        var xrec = (Xrecord)tr.GetObject(worksetDict.GetAt(targetKey), OpenMode.ForRead);
        Clear();
        XrecordSerializer.Deserialize(xrec.Data, this);

        _currentIndex = targetIndex;
        return true;
    }

    /// <summary>
    /// 清理内存容器并重置id
    /// </summary>
    public void Clear()
    {
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

    /// <summary>
    /// 清理数据库字典历史
    /// </summary>
    private void ClearWorksetStorage()
    {
        if (_worksetNod.IsNull)
            return;
        try
        {
            using var tr = DBTrans.Create(openCloseTrans: true);
            var nod = (DBDictionary)tr.GetObject(Document.Database.NamedObjectsDictionaryId, OpenMode.ForWrite);

            // 查找并删除我们的存储字典
            foreach (DictionaryEntry entry in nod)
            {
                if ((ObjectId)entry.Value == _worksetNod)
                {
                    nod.Remove(entry.Key.ToString());
                    break;
                }
            }
            _worksetNod = ObjectId.Null;
        }
        catch
        {
            // 忽略清理错误
        }
    }

    /// <summary>
    /// 初始化主字典，用于存储workset历史记录
    /// </summary>
    public void HistoryInit()
    {
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            var db = Document.Database;
            var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

            // 注册扩展数据应用程序名
            var regAppTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!regAppTable.Has(MainNameUndo))
            {
                regAppTable.UpgradeOpen();
                var regAppRecord = new RegAppTableRecord();
                regAppRecord.Name = MainNameUndo;
                regAppTable.Add(regAppRecord);
                tr.AddNewlyCreatedDBObject(regAppRecord, true);
            }

            // 检查是否已存在主字典
            if (nod.Contains(MainNameWorkset))
            {
                _worksetNod = nod.GetAt(MainNameWorkset);
            }
            else
            {
                var d = new DBDictionary();
                nod.SetAt(MainNameWorkset, d);
                tr.AddNewlyCreatedDBObject(d, true);
                _worksetNod = d.ObjectId;
            }

            if (nod.Contains(MainNameUndo))
            {
                _undoMarkNod = nod.GetAt(MainNameUndo);
            }
            else
            {
                var d = new DBDictionary();
                nod.SetAt(MainNameUndo, d);
                tr.AddNewlyCreatedDBObject(d, true);
                _undoMarkNod = d.ObjectId;
            }
        }
    }

    /// <summary>
    /// 读取历史,数据库主字典,并决定是否刷新
    /// </summary>
    public void HistoryRead(bool isUndo)
    {
        // TODO 不能通过undo/redo命令进入啊,一定要通过撤回的对象来判断当前所在undoLog的节点位置
        // 否则不是在位编辑的期间岂不是也会undo命令?
        if (!LoadWorksetFromDatabase(isUndo))
        {
            return;
        }

        // TODO 这是锁定再活动,如果是退出refedit,就不需要锁定了.
        // 刷新显示
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            HashSet<ObjectId> lockedLayers = [];
            Fade(tr, lockedLayers);
        }
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            RegenLayers(tr);
        }
        Env.Printl($"已{(isUndo ? "撤销" : "重做")}到上一次状态");
    }

    /// <summary>
    /// 淡显全部图元
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="lockedLayers"></param>
    public void Fade(DBTrans tr, HashSet<ObjectId> lockedLayers)
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
    public void RegenLayers(DBTrans tr)
    {
        // 此时已经解锁全部图层,但是没有使用 IFoxUtils.RegenLayers 刷新,
        // 然后我们使用平移图元就会亮显这一部分的图元.
        foreach (var id in Workset)
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
            ent.Move(Point3d.Origin, Point3d.Origin);
        }

        // 刷新这个图层,
        // 即使这个图层没有任何图元,也会触发刷新修改过的图元而不是整个图层
        var refLayerId = tr.LayerTable.Add(RefEdit0);
        var lays = new List<ObjectId> { refLayerId };
        IFoxUtils.RegenLayers(lays);
    }
}

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

        // 没有了这三个确实可以在位编辑器期间redo了,但是为什么图元不显示呢?
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

        RefEditInfo.Destroy(doc);
    }


    private void Database_ObjectModified(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is not Xrecord xrec)
            return;

        var db = (Database)sender;
        var doc = Acap.DocumentManager.GetDocument(db);
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        using var tr = DBTrans.Create(openCloseTrans: true);
        try
        {
            var data = xrec.Data;
            bool isOurRecord = false;
            foreach (var tv in data)
            {
                if (tv.TypeCode == (int)DxfCode.ExtendedDataAsciiString &&
                    tv.Value as string == "__XRECORD_HEADER__")
                {
                    isOurRecord = true;
                    break;
                }
            }

            if (isOurRecord)
            {
                XrecordSerializer.Deserialize(data, xInfo);

                HashSet<ObjectId> lockedLayers = [];
                xInfo.Fade(tr, lockedLayers);
                xInfo.RegenLayers(tr);
            }
        }
        catch
        {
        }
    }

    private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        if (e.DBObject is Xrecord xrec)
        {
            var data = xrec.Data;
            if (data != null)
            {
                foreach (var tv in data)
                {
                    if (tv.TypeCode == (int)DxfCode.ExtendedDataAsciiString &&
                        tv.Value as string == "__XRECORD_HEADER__")
                    {
                        XrecordSerializer.Deserialize(data, xInfo);

                        using var tr = DBTrans.Create(openCloseTrans: true);
                        HashSet<ObjectId> lockedLayers = [];
                        xInfo.Fade(tr, lockedLayers);
                        xInfo.RegenLayers(tr);
                        return;
                    }
                }
            }
            return;
        }

        if (e.DBObject is not Entity)
            return;
        if (!xInfo.IsRun)
            return;
        if (xInfo.Workset.Remove(e.DBObject.ObjectId))
        {
            // 这里开无撤事务会错误,看报错是上下文不可用
            // 所以我要发送命令,使用无撤事务,记录历史,而不设置undoMarker
            // doc.SendStringToExecute(nameof(Refedit_ObjectErased) + " ", false, false, false);

            // 我发现,撤回的删除也会发生记录,又再次记录了历史,污染了历史.
            // 但是这样岂不是不能redo删除的对象?
        }
    }

    //[CommandMethod(nameof(Refedit_ObjectErased), CommandFlags.NoHistory)]
    //public void Refedit_ObjectErased()
    //{
    //    var doc = Acap.DocumentManager.MdiActiveDocument;
    //    if (!TryGetRefEditInfo(doc, out var xInfo))
    //        return;
    //    xInfo.HistoryWrite(nameof(Refedit_ObjectErased), true);
    //}


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

    //[CommandMethod(nameof(Refedit_ObjectAppended), CommandFlags.NoHistory)]
    //public void Refedit_ObjectAppended()
    //{
    //    var doc = Acap.DocumentManager.MdiActiveDocument;
    //    if (!TryGetRefEditInfo(doc, out var xInfo))
    //        return;
    //    xInfo.HistoryWrite(nameof(Refedit_ObjectAppended));
    //}

    // redo重做时候加入对象
    private void Database_ObjectReappended(object sender, ObjectEventArgs e)
    {
        var db = (Database)sender;
        var doc = Acap.DocumentManager.GetDocument(db);
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        // 处理 DBDictionary 类型的 UndoMarker（redo 时恢复）
        if (e.DBObject is not DBDictionary markerDict)
            return;

        using var tr = DBTrans.Create(openCloseTrans: true);
        var arr = markerDict.XData.AsArray();
        int index = -1;
        if (arr.Length >= 2 &&
            arr[0].TypeCode == 1001 &&
            arr[0].Value.ToString() == RefEditInfo.MainNameUndo &&
            arr[1].TypeCode == (int)DxfCode.ExtendedDataInteger32)
        {
            index = (int)arr[1].Value;
        }

        if (index == -1)
            return;

        // Redo 时，UndoMarker 存储的索引就是要回到的索引
        xInfo.SetCurrentIndex(index);
        // 无法刷新 LoadHistoryAndRefresh(xInfo, isUndo: false);
        // 通过命令后事件处理刷新
    }

    // undo撤回时候移除对象
    // 这里撤回的对象是一个一个触发的,
    // 但是一个命令可能有n个图元.我们历史链表也是一个命令记录一批
    // 所以我们必须要用一个标记表示我们我们回到了哪个命令
    // 不能通过undo/redo命令判断进入,而是通过撤回了什么对象,
    // 因为非在位编辑的期间也会undo/redo命令,
    // TODO 所以我们要在refedit/refset/refclose都制作对象进行撤回检测(并且必须可撤事务保存)
    private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
    {
        var db = (Database)sender;
        var doc = Acap.DocumentManager.GetDocument(db);
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;

        // 不能判断IsRun,因为refclose回滚需要

        // ObjectUnappended 事件触发时，markerDict 已经被移除,
        // 所以这里会分配一个新的 ObjectId 给这个 DBDictionary 对象,
        // 所以我们用Xdata来获取编号
        if (e.DBObject is not DBDictionary markerDict)
            return;

        using var tr = DBTrans.Create(openCloseTrans: true);
        var arr = markerDict.XData.AsArray();
        int index = -1;
        if (arr.Length >= 2 &&
            arr[0].TypeCode == 1001 &&
            arr[0].Value.ToString() == RefEditInfo.MainNameUndo &&
            arr[1].TypeCode == (int)DxfCode.ExtendedDataInteger32)
        {
            index = (int)arr[1].Value;
        }

        if (index == -1)
            return;

        // 这个事件会令刷新无效,命令后事件实现刷新.
        // refedit 一次回滚了两步,毕竟我们备份了 IsRun==false 状态.
        // refclose 保存-撤销 为什么没有刷新?.
        xInfo.SetCurrentIndex(index);
        // 无法刷新 LoadHistoryAndRefresh(xInfo, isUndo: true);
    }


    /// <summary>
    /// 加载历史记录并刷新显示
    /// </summary>
    /// <param name="xInfo">RefEdit 信息对象</param>
    /// <param name="isUndo">是否为撤销操作</param>
    private static void LoadHistoryAndRefresh(RefEditInfo xInfo, bool isUndo)
    {
        if (!xInfo.LoadWorksetFromDatabaseByIndex(xInfo.CurrentIndex))
            return;

        if (!xInfo.IsRun)
        {
            // 回滚到在位编辑器外,此时workset是没有图元的,如果锁图层就会全灰了,
            // 因此此时什么也不干就行了.
        }
        else
        {
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                HashSet<ObjectId> lockedLayers = [];
                xInfo.Fade(tr, lockedLayers);
            }
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                xInfo.RegenLayers(tr);
            }

            Env.Printl($"已{(isUndo ? "撤销" : "重做")}到历史记录索引: {xInfo.CurrentIndex}");
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

        if (cmd == "U")
        {
            using var tr = DBTrans.Create(openCloseTrans: true);
            LoadHistoryAndRefresh(xInfo, isUndo: true);
            return;
        }

        if (cmd == "MREDO")
        {
            // IsRun==true 期间 这里如果失效,也会打印消息
            using var tr = DBTrans.Create(openCloseTrans: true);
            LoadHistoryAndRefresh(xInfo, isUndo: false);
            return;
        }

        if (_workCmd.Contains(cmd))
            return;
        if (!xInfo.IsRun)
            return;

        // 在位编辑器期间运行官方命令,所做的操作.
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
        if (cmd == "PROPERTIES" || cmd == "SELECT")
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (!TryGetRefEditInfo(doc, out var xInfo))
            return;
        if (cmd == "U" || cmd == "MREDO")
            return;

        if (_workCmd.Contains(cmd))
            return;
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

        // 重新淡显全部-再亮显workset
        HashSet<ObjectId> lockedLayers = [];
        using (var tr2 = DBTrans.Create(openCloseTrans: true))
        {
            xInfo.Fade(tr2, lockedLayers);
        }
        using (var tr2 = DBTrans.Create(openCloseTrans: true))
        {
            xInfo.RegenLayers(tr2);
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

            // 保存到字典
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

            // 保存到字典
            xInfo.HistoryWrite(nameof(RefSet) + "_Remove");

            // 淡显
            // 锁定全部,再把workset给亮回来
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                xInfo.Fade(tr, xInfo.LockedLayers);
            }
            using (var tr = DBTrans.Create(openCloseTrans: true))
            {
                xInfo.RegenLayers(tr);
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

                // 过滤掉已删除的图元，只保留有效的 ObjectId
                var validIds = new ObjectIdCollection();
                foreach (var id in xInfo.Workset)
                {
                    if (id.IsNull || id.IsEffectivelyErased)
                        continue;
                    try
                    {
                        var obj = tr.GetObject(id, OpenMode.ForRead, false, true);
                        if (obj != null && !obj.IsDisposed && !obj.IsErased)
                            validIds.Add(id);
                    }
                    catch
                    {
                        // 无效的 ObjectId，跳过
                    }
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

                // 保存历史(防止workset删除时候移除,此处记录)
                xInfo.HistoryWrite(nameof(RefClose) + "_Before_Save");

                // RefEdit-画圆-撤回 为什么内部撤回了一次,会导致保存时候丢失全部图元呢?
                // 原因: 撤销操作使图元从数据库中删除,造成id是重置的,而workset记录的是删除后的id?
                // 不对啊,我还只是撤回了一个删除的圆啊,理论上只会导致一个id被更改.
                // 所以保存时候要剔除这些删除的id,否则异常
                // --删除临时图元,会联动事件移除workset的
                // 需要过滤已删除的图元，因为撤销后 Workset 可能包含已删除的对象
                foreach (var id in xInfo.Workset)
                {
                    if (id.IsNull || id.IsEffectivelyErased)
                        continue;
                    try
                    {
                        using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
                        if (ent.IsDisposed || ent.IsErased)
                            continue;
                        ent.Erase(true);
                    }
                    catch
                    {
                        // 无效的 ObjectId，跳过
                    }
                }

                Env.Printl("保存参照修改");
            }

            // 恢复原有的块参照
            brf.Erase(false);

            // 恢复图层锁定的显示
            IFoxUtils.RegenLayers(xInfo.LockedLayers);

            // 删除用来临时锁定的图层
            var refLayerId = tr.LayerTable.Add(RefEditInfo.RefEdit0);
            using var refLayer = (LayerTableRecord)tr.GetObject(refLayerId, OpenMode.ForWrite, true, true);
            refLayer.Erase(true);

            a = xInfo.Workset.Count; // 3 
        }

        a = xInfo.Workset.Count; // 这里变成0了,因为这里删除对象事件导致的

        xInfo.Clear();
        xInfo.IsRun = false;

        // 再次保存历史
        xInfo.HistoryWrite(nameof(RefClose) + "_After");
    }

    // TODO 现在redo都是有问题的.
    // RefEdit-undo-redo 此时重做无法实现.
    // 写undo容易,redo难.不知道什么时候污染了历史,造成redo是0.

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
        xInfo.IsRun = true;

        // 1,淡显图元
        using (var tr = DBTrans.Create())
        {
            xInfo.Fade(tr, xInfo.LockedLayers);
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
            xInfo.RegenLayers(tr);
        }

        // 保存初始workset到字典
        xInfo.HistoryWrite(nameof(RefEdit));
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