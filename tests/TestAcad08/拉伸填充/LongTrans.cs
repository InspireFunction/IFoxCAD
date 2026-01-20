using IFoxCAD.Cad;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace JoinBoxAcad;

public class LongTransManager
{
    /// <summary>
    /// 当前文档,在位编辑器记录全图选择集的填充
    /// </summary>
    public static bool WorkSetHas(ObjectId id)
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        return doc is null ? throw new ArgumentException("居然没有文档") : _map[doc].WorkSetHas(id);
    }

    /// <summary>
    /// 当前文档,在位编辑器运行状态
    /// </summary>
    public static bool RefeditRun()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        return doc is null ? throw new ArgumentException("居然没有文档") : _map[doc].RefeditRun;
    }

    static Dictionary<Document, LongTrans> _map = [];

    [IFoxInitialize]
    public void LongTransManagerInit(Document _)
    {
        // 1,遍历目前所有文档,加载时候可能已经有多个文档打开了
        foreach (Document doc in Acap.DocumentManager)
        {
            _map[doc] = new LongTrans(doc);
        }

        // 2,文档创建和销毁事件,需要同步移除
        Acap.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
        Acap.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
    }

    // 文档创建
    private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        _map[doc] = new LongTrans(doc);
    }

    // 文档关闭
    private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
    {
        var doc = e.Document;
        _map.Remove(doc);
    }


    [CommandMethod(nameof(bbb))]
    public static void bbb()
    {
        using var tr = new DBTrans();
        StringBuilder stringBuilder = new();
        var doc = Acap.DocumentManager.MdiActiveDocument;
        stringBuilder.AppendLine($"WorkSet  id数量: {_map[doc].WorkSet.Count}");
        int i = 0;
        foreach (var id in _map[doc].WorkSet)
        {
            using var ent = tr.GetObject(id);
            stringBuilder.AppendLine($"{i++} {id} {ent.GetType()}");
        }
        Env.Printl(stringBuilder);
    }
}

class LongTrans : IDisposable
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

    public bool WorkSetHas(ObjectId id)
    {
        return WorkSet.Contains(id);
    }

    // TODO 在位编辑期间不知道为什么多了个块表记录??!!
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
    public bool RefeditRun { get => _refeditRun; set => _refeditRun = value; }


    public LongTrans(Document doc)
    {
        _doc = doc;

        // 检查是否在了你在位编辑期间
        var refeditName = Env.GetVar("REFEDITNAME");
        if (!StringHelper.IsNullOrWhiteSpace(refeditName.ToString()))
        {
            Env.Printl("您必须关闭在位编辑器之后运行初始化命令,否则监控长事务功能会失效.");
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
                    WorkSetAdd(prompt.Value.GetObjectIds());
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
                    var ss = new HashSet<ObjectId>(WorkSet);
                    Clear();
                    WorkSetAdd(prompt.Value.GetObjectIds()
                        .AsParallel()
                        .Where(a => !ss.Contains(a)));
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
                    return;
                }
                if (last.Contains("删除") || last.Contains("Removed"))// 中英文cad
                {
                    WorkSetRemove(prompt.Value.GetObjectIds());
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
            case "U": // 撤回
            {
                // 命令历史的最后一行
                var last = Env.GetVar("lastprompt").ToString();
                if (last is null)
                    return;

                // TODO 完蛋啦....这里没有办法???
                var prompt = Env.Editor.SelectPrevious();
                if (prompt.Status != PromptStatus.OK)
                    return;

                // 就是因为无法遍历到在位编辑的块内图元,只能进行布尔运算
                if (last.Contains("从工作集删除"))
                {
                    WorkSetAdd(prompt.Value.GetObjectIds());
                    return;
                }
                if (last.Contains("已添加到工作集"))
                {
                    WorkSetRemove(prompt.Value.GetObjectIds());
                    return;
                }
            }
            break;
        }
    }



    /// <summary>
    /// 数据库加入事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DB_ObjectAppended(object sender, ObjectEventArgs e)
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
        //if (_refeditRun)
        //{
        //    WorkSet.Add(e.DBObject.ObjectId);
        //    DebugEx.Printl($"3,在位编辑撤回时候创建对象: {e.DBObject.ObjectId}");
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
        if (_refeditRun)
        {
            WorkSetAdd(e.DBObject.ObjectId);
            DebugEx.Printl($"4,在位编辑撤回时候创建对象: {e.DBObject.ObjectId}");
        }
    }

    #endregion

    #region IDisposable接口相关函数
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
    ~LongTrans()
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
