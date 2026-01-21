using System.Threading;

namespace JoinBoxAcad;

public static class DocumentEx
{
    /// <summary>
    /// 判断对象是否在工作集中(判断在位编辑块时块内外图元)
    /// </summary>
    /// <param name="doc">块所在文档</param>
    /// <param name="entityId">文档中的实体Id</param>
    /// <param name="includingErased">是否包含删除的对象</param>
    /// <returns></returns>
    public static bool WorkSetHas(this Document doc, ObjectId entityId, bool includingErased = false)
    {
#if NET35
        return LongTransactionManager.WorkSetHas(doc, entityId, includingErased);
#else
        var id = Acap.LongTransactionManager.CurrentLongTransactionFor(doc);
        // 当前长事务不存在
        if (id.IsNull) return false;
        var tr = DBTrans.GetTop(doc.Database);
        using var longtr = (LongTransaction)tr.GetObject(id, openErased: includingErased);
        return longtr.WorkSetHas(entityId, includingErased);
#endif
    }
}


#if NET35
public class LongTransactionManager
{
    /// <summary>
    /// 当前文档,在位编辑器记录全图选择集的填充
    /// </summary>
    public static bool WorkSetHas(Document doc, ObjectId id, bool includingErased = false)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        return _map[doc].WorkSetHas(id, includingErased);
    }

    /// <summary>
    /// 当前文档,在位编辑器运行状态
    /// </summary>
    public static bool RefeditRun(Document doc)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        return _map[doc].RefeditRun;
    }

    static Dictionary<Document, LongTransaction> _map = [];

    [IFoxInitialize]
    public void LongTransManagerInit(Document _)
    {
        // 1,遍历目前所有文档,加载时候可能已经有多个文档打开了
        foreach (Document doc in Acap.DocumentManager)
        {
            _map[doc] = new(doc);
        }

        // 2,文档创建和销毁事件,需要同步移除
        Acap.DocumentManager.DocumentCreated += DM_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed += DM_DocumentToBeDestroyed;
    }

    // 文档创建
    private void DM_DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        _map[doc] = new LongTransaction(doc);
    }

    // 文档关闭
    private void DM_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        _map.Remove(doc);
    }


    [CommandMethod(nameof(WorkSet))]
    public static void WorkSet()
    {
        using var tr = new DBTrans();
        StringBuilder stringBuilder = new();
        var doc = Acap.DocumentManager.MdiActiveDocument;
        stringBuilder.AppendLine($"WorkSet  id数量: {_map[doc].WorkSet.Count}");
        int i = 0;
        foreach (var id in _map[doc].WorkSet)
        {
            using var ent = tr.GetObject(id, openErased: true);
            stringBuilder.AppendLine($"{i++} {id} {ent.GetType()}");
        }
        Env.Printl(stringBuilder);
    }


    // Acad08 为什么没有用呢?其他版本呢?
    [CommandMethod(nameof(CmdTransperentcy), CommandFlags.Redraw)]
    public void CmdTransperentcy()
    {
        using DBTrans tr = new();
        var ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        var x = EditorEx.GetInteger(ed, "输入透明度", 255);
        var ts = new Transparency((byte)x.Value);
        foreach (var id in tr.BlockTable)
        {
            if (!id.IsOk())
                continue;
            using var obj = tr.GetObject(id);
            if (obj is not BlockTableRecord btr)
                continue;
            foreach (var entid in btr)
            {
                using var ent = (Entity)tr.GetObject(entid, OpenMode.ForWrite, openLockedLayer: true);
                ent.Transparency = ts;
            }
        }
    }

}

/// <summary>
/// 长事务
/// </summary>
public class LongTransaction : IDisposable
{
    #region 静态成员
    /// <summary>
    /// 选择集过滤器
    /// </summary>
    //public static SelectionFilter FilterForHatch = new([new((int)DxfCode.Start, "HATCH")]);
    public static SelectionFilter FilterForHatch = new([]);
    #endregion

    #region 动态成员
    readonly Document _doc;
    bool _refeditRun;

    #region WorkSet
    /// <summary>
    /// 在位编辑器记录全图选择集的填充
    /// </summary>
    internal HashSet<ObjectId> WorkSet = [];

    /// <summary>
    /// 长事务中含有id
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="openErased">是否打开软删除的对象</param>
    /// <returns></returns>
    public bool WorkSetHas(ObjectId id, bool openErased)
    {
        return WorkSet.Contains(id);
    }

    // TODO 在位编辑期间不知道为什么多了个块表记录??!!
    // 而且是已经删除的,怀疑是acad08的删除不清理bug
    bool WorkSetAdd(ObjectId id)
    {
        if (WorkSet.Add(id))
        {
            Env.Printl($"加入了 {id}  ");
            return true;
        }
        return false;
    }

    bool WorkSetRemove(ObjectId id)
    {
        if (WorkSet.Remove(id))
        {
            Env.Printl($"移除了 {id}  ");
            return true;
        }
        return false;
    }

    void WorkSetAdd(IEnumerable<ObjectId> ids)
    {
        foreach (var item in ids)
        {
            WorkSetAdd(item);
        }
    }

    void WorkSetRemove(IEnumerable<ObjectId> ids)
    {
        foreach (var item in ids)
        {
            WorkSetRemove(item);
        }
    }
    void Clear()
    {
        Env.Printl($"移除了全部");
        WorkSet.Clear();
    }


    #endregion

    /// <summary>
    /// 在位编辑器运行状态
    /// </summary>
    public bool RefeditRun
    {
        get
        {
            var refeditName = Env.GetVar("REFEDITNAME");
            return !StringHelper.IsNullOrWhiteSpace(refeditName.ToString());
        }
    }

    /// <summary>
    /// 长事务
    /// </summary>
    /// <param name="doc">文档</param>
    public LongTransaction(Document doc)
    {
        _doc = doc;

        // 检查是否在了你在位编辑期间
        if (RefeditRun)
        {
            Env.Printl($"长事务监控功能失效,该文档已经打开在位编辑器,重启文档才能进入监控: {doc.Name}");
            return;
        }
        LoadHelper(true);
    }

    void LoadHelper(bool isLoad)
    {
        if (isLoad)
        {
            _doc.CommandWillStart += Md_CommandWillStart;
            _doc.CommandEnded += Md_CommandEnded;
            _doc.Database.ObjectAppended += DB_ObjectAppended;
            _doc.Database.ObjectErased += DB_ObjectErased;
            _doc.Database.ObjectModified += DB_ObjectModified;
        }
        else
        {
            _doc.CommandWillStart -= Md_CommandWillStart;
            _doc.CommandEnded -= Md_CommandEnded;
            _doc.Database.ObjectAppended -= DB_ObjectAppended;
            _doc.Database.ObjectErased -= DB_ObjectErased;
            _doc.Database.ObjectModified -= DB_ObjectModified;
        }
    }
    #endregion

    #region 事件
    /// <summary>
    /// 快照:执行前记录全局图元(因为存在撤回问题,所以必须记录)
    /// </summary>
    readonly HashSet<ObjectId> _currentIds = [];

    /// <summary>
    /// 反应器->command命令执行前
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_CommandWillStart(object sender, CommandEventArgs e)
    {
        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "REFEDIT":
            {
                // 触发编辑器打开命令前,扫描全图
                var prompt = Env.Editor.SelectAll(FilterForHatch);
                if (prompt.Status == PromptStatus.OK)
                {
                    _currentIds.Add(prompt.Value.GetObjectIds());
                }
            }
            break;
        }
    }

    /// <summary>
    /// 反应器->command命令完成后
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_CommandEnded(object sender, CommandEventArgs e)
    {
        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "REFEDIT":
            {
                _refeditRun = true;

                // 命令后,扫描全图获取,多出来的就是在位编辑块内的
                var prompt = Env.Editor.SelectAll(FilterForHatch);
                if (prompt.Status == PromptStatus.OK)
                {
                    Clear();
                    WorkSetAdd(prompt.Value.GetObjectIds()
                        .AsParallel()
                        .Where(a => !_currentIds.Contains(a)));
                }
            }
            break;
            case "REFSET": // 加减在位编辑图元
            {
                // 命令历史的最后一行是:添加/删除
                var last = Env.GetVar("lastprompt").ToString();
                if (last is null)
                    return;
                // 完成后必然有上次选择集
                var prompt = Env.Editor.SelectPrevious();
                if (prompt.Status != PromptStatus.OK)
                    return;

                // 就是因为无法遍历到在位编辑的块内图元,只能进行布尔运算
                if (last.Contains("添加") || last.Contains("Added"))// 中英文cad
                {
                    WorkSetAdd(prompt.Value.GetObjectIds());
                    _currentIds.Remove(prompt.Value.GetObjectIds());
                    return;
                }
                if (last.Contains("删除") || last.Contains("Removed"))// 中英文cad
                {
                    WorkSetRemove(prompt.Value.GetObjectIds());
                    _currentIds.Add(prompt.Value.GetObjectIds());
                    return;
                }
            }
            break;
            case "REFCLOSE":// 保存块,清空集合
            {
                _refeditRun = false;
                Clear();
            }
            break;
            case "MREDO": // 重做 ctrl+y
            {
            }
            break;
            case "U": // 撤回 ctrl+z
            {
                if (!_refeditRun)
                    return;

                // TODO 撤回对象跟踪问题
                // 由于 acad arx 存在一个部分撤回功能,也就是撤回时候不通过事件.
                // 局部撤回技术 https://www.codeleading.com/article/18306112823/
                // 造成我们存在无法跟踪撤回了哪些图元致命问题.

                // 具体测试:
                // 画rect和填充 组块,在位编辑,只选中填充减去,执行u,会发现除了命令事件,没有触发数据库事件.
                // 在位编辑这个功能使用了局部撤回技术.
                // 你只能捕捉u命令,无法捕捉哪个对象被撤回了.
                // 此时就无法区分 在为编辑 图元在内部还是外部.

                // 通过快照再进行一次过滤?? 不行.
                // 对象是编辑期间减出去,运行u命令,此时快照是原本的,就不对了.
                // 因此我们要 删除和添加 时候更新原本的快照.
                // 但是多次撤回呢? 由于无法跟踪对象,导致它目前代码也不对.

                // 我来从头做一个数据日志? redolog?
                // 每次画一个东西就记录,
                // 发生 ctrl+z 找到撤回点,
                // 由我进行删除对象,把期间加入的图元抛到自定义事件中...
                // 这似乎很恐怖啊...
                // 还是那句话,我要怎么找到cad原生命令的撤回点呢?
                // 似乎不需要找了,因为撤回点是我的,而且是记录事务和undoMark(多命令撤回)

                //var prompt = Env.Editor.SelectAll(FilterForHatch);
                //if (prompt.Status == PromptStatus.OK)
                //{
                //    Clear();
                //    WorkSetAdd(prompt.Value.GetObjectIds()
                //        .AsParallel()
                //        .Where(a => !_currentIds.Contains(a)));
                //}

                var tr = new DBTrans();

            }
            break;
        }
    }

    /// <summary>
    /// 数据库加入事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DB_ObjectAppended(object sender, ObjectEventArgs e)
    {
        if (_refeditRun)
        {
            WorkSetAdd(e.DBObject.ObjectId);
            DebugEx.Printl($"1,在位编辑创建对象: {e.DBObject.ObjectId}");
        }
    }

    /// <summary>
    /// 撤回事件(获取删除对象)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DB_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        // object erased.
        DebugEx.Printl($"{nameof(DB_ObjectErased)}: {e.DBObject} , {e.DBObject.IsUndoing} , {e.DBObject.IsErased}");

        if (e.Erased)
        {
            if (_refeditRun)
            {
                WorkSetRemove(e.DBObject.ObjectId);
                DebugEx.Printl($"2,在位编辑删除对象: {e.DBObject.ObjectId}");
            }
            return;
        }

        // UNDO
        DebugEx.Printl($"3,UNDO对象: {e.DBObject.ObjectId}");

        //if (_refeditRun)
        //{
        //    WorkSet.Add(e.DBObject.ObjectId);
        //    DebugEx.Printl($"4,在位编辑撤回时候创建对象: {e.DBObject.ObjectId}");
        //}
    }

    /// <summary>
    /// 撤回事件(更改时触发)
    /// 它会获取有修改步骤的图元id
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DB_ObjectModified(object sender, ObjectEventArgs e)
    {
        DebugEx.Printl($"{nameof(DB_ObjectModified)}: {e.DBObject} , {e.DBObject.IsUndoing} , {e.DBObject.IsErased}");

        //if (_refeditRun)
        //{
        //    WorkSetAdd(e.DBObject.ObjectId);
        //    DebugEx.Printl($"6,在位编辑撤回时候创建对象: {e.DBObject.ObjectId}");
        //}
    }

    #endregion

    #region IDisposable接口相关函数
    /// <summary>
    /// 释放标记
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 手动调用释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数调用释放
    /// </summary>
    ~LongTransaction()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放
        if (IsDisposed) return;
        IsDisposed = true;

        if (_doc.IsDisposed)
            return;
        LoadHelper(false);
    }
    #endregion
}

#endif