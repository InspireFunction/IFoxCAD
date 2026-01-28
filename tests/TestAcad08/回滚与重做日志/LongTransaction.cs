using Autodesk.AutoCAD.DatabaseServices;
using System.Windows.Controls;

namespace JoinBoxAcad;


public static class DocumentEx
{
    /// <summary>
    /// 工作集是否含有对象
    /// </summary>
    /// <param name="doc">块所在文档</param>
    /// <param name="entityId">文档中的实体Id</param>
    /// <param name="includingErased">是否包含删除的对象</param>
    /// <returns></returns>
    public static bool WorkSetHas(this Document doc, ObjectId entityId, bool includingErased = false)
    {
#if NET35
        return LongTransaction.WorkSetHas(doc, entityId, includingErased);
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


/// <summary>
/// 在位编辑操作处理器
/// </summary>
public class LongTransaction : IDisposable
{
    private bool _IsDisposed;
    private readonly Document _document;
    private readonly EnhancedActionLogger _logger;
    private bool _refedit_run;


    #region 工作集获取

    // 用来储存在位编辑前获取.
    internal HashSet<ObjectId> _workSet = [];

    internal static Dictionary<Document, LongTransaction> _map = [];

    /// <summary>
    /// 长事务中含有id
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="openErased">是否打开软删除的对象</param>
    /// <returns></returns>
    public bool WorkSetHas(ObjectId id, bool openErased)
    {
        return _workSet.Contains(id);
    }

    internal static void WorkAdd(Document doc, ObjectId id)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        _map[doc]._workSet.Add(id);
    }

    internal static void WorkRemove(Document doc, ObjectId id)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        _map[doc]._workSet.Remove(id);
    }

    public static void WorkSetClear(Document doc)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        _map[doc]._workSet.Clear();
    }

    /// <summary>
    /// 当前文档,在位编辑器记录全图选择集的填充
    /// </summary>
    public static bool WorkSetHas(Document doc, ObjectId id, bool includingErased = false)
    {
        if (doc == null)
            throw new ArgumentNullException("文档不存在");
        return _map[doc].WorkSetHas(id, includingErased);
    }

    public static bool RefeditRun(Document doc)
    {
        return _map[doc]._refedit_run;
    }
    #endregion


    /// <summary>
    /// 过滤器
    /// </summary>
    private static SelectionFilter Filter => new([]);

    public LongTransaction(Document document, EnhancedActionLogger logger)
    {
        _document = document;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 注册命令开始事件
        _document.CommandWillStart += OnCommandWillStart;
        // 注册命令结束事件
        _document.CommandEnded += OnCommandEnded;

        // 数据库
        _document.Database.ObjectAppended += OnObjectAppended;
        _document.Database.ObjectModified += OnObjectModified;
        _document.Database.ObjectErased += OnObjectErased;

        _map[document] = this;
    }

    private void OnObjectErased(object sender, ObjectErasedEventArgs e)
    {
        var entity = e.DBObject as Entity;
        if (entity is null)
            return;
        // 保存命令触发之后需要跳过,否则会剔除全部数据
        if (_refClose_start)
            return;
        if (_refedit_run)
            _workSet.Remove(entity.ObjectId);
    }

    private void OnObjectModified(object sender, ObjectEventArgs e)
    {

    }

    private void OnObjectAppended(object sender, ObjectEventArgs e)
    {
        var entity = e.DBObject as Entity;
        if (entity is null)
            return;

        // 保存命令触发之后需要跳过,否则会剔除全部数据
        if (_refClose_start)
            return;
        if (_refedit_run)
            _workSet.Add(entity.ObjectId);
    }

    #region 事件处理

    // 在位编辑击中的块
    ObjectId[] _refBlock = [];

    private void OnCommandWillStart(object sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandWillStart - {DateTime.Now}");

        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "REFEDIT":
            {
                // TODO #260127a 为了保存时候撤回,这里要备份选择集
                var prompt = _document.Editor.SelectImplied();// 预选
                if (prompt.Status == PromptStatus.OK)
                    _refBlock = prompt.Value.GetObjectIds();

                _workSet.Clear();

                // 触发编辑器打开命令前,扫描全图,命令后再获取一次全图,得到不相交就是多出来的块内元素.
                var prompt2 = Env.Editor.SelectAll(Filter);
                if (prompt2.Status == PromptStatus.OK)
                    _workSet.Add(prompt2.Value.GetObjectIds());
            }
            break;
            case "REFCLOSE":
            {
                _refClose_start = true;
            }
            break;
        }
    }

    bool _refClose_start = false;

    private void OnCommandEnded(object sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandEnded - {DateTime.Now}");

        // 当前状态是回滚/重做任务中.
        if (_logger.IsExecutingUndoRedo)
        {
            // #260126a 使用了异步命令这里需要清理
            HandleAsyncCommandCleanup(e.GlobalCommandName); // todo 撤回时候发送了 refclose _d
            return;
        }

        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "REFEDIT":
            _refedit_run = true;
            HandleRefEdit();
            break;
            case "REFSET":
            HandleRefSet(true);
            break;
            case "REFCLOSE":
            {
                _refedit_run = false;
                // 发生回滚的时候呢?清理的就没了啊 
                // 因此我们需要把 refclose 命令时候 把workset作为动作,这样实现回滚才有数据恢复
                if (_workSet.Count > 0)
                {
                    // 创建在位编辑清空动作
                    var action = new InPlaceSaveAction(_refBlock);
                    _logger.LogAction(action, "REFCLOSE");
                    _workSet.Clear();
                }
                _refClose_start = false;
            }
            break;
        }
    }

    /// <summary>
    /// 处理异步命令清理
    /// </summary>
    private bool HandleAsyncCommandCleanup(string cmd)
    {
        if (!_logger.IsExecutingUndoRedo)
            throw new("不是回滚/重做期间");

        if (!_logger.AsyncCmdsPop(cmd))
            return false;

        // 全部移除就恢复
        if (_logger.AsyncCmdsCount == 0)
            _logger.IsExecutingUndoRedo = false;

        // "减去"-触发了撤回事件-执行了"添加"-工作集添加回来.
        // 同理,"添加"也需要从工作集"减去"
        HandleRefSet(false);

        return true;
    }

    /// <summary>
    /// 处理 REFEDIT 命令
    /// </summary>
    private void HandleRefEdit()
    {
        // 命令后,扫描全图获取,多出来的就是在位编辑块内的
        var prompt = _document.Editor.SelectAll(Filter);
        if (prompt.Status != PromptStatus.OK)
            return;

        // 通过交换,不储存全局的,省点内存
        var all = new HashSet<ObjectId>(_workSet);
        _workSet.Clear();
        var refIds = prompt.Value.GetObjectIds()
            .AsParallel()
            .Where(id => !all.Contains(id))
            .ToList();

        if (refIds.Count == 0)
            return;

        // 创建在位编辑添加动作
        var action = new InPlaceCreateAction(_refBlock);
        _logger.LogAction(action, "REFEDIT");

        // 更新当前ID集合
        _workSet.Add(refIds);
    }

    /// <summary>
    /// 处理 REFSET 命令
    /// </summary>
    private void HandleRefSet(bool addLog)
    {
        // 命令历史的最后一行是:添加/删除
        var lastPrompt = Env.GetVar("lastprompt")?.ToString();
        if (lastPrompt is null)
            return;
        // 完成后必然有上次选择集
        var prompt = _document.Editor.SelectPrevious();
        if (prompt.Status != PromptStatus.OK)
            return;
        var selectedIds = prompt.Value.GetObjectIds();
        HandleRefSetOperation(lastPrompt, selectedIds, addLog);
    }

    /// <summary>
    /// 处理 REFSET 的具体操作（添加/删除）
    /// </summary>
    private void HandleRefSetOperation(string lastPrompt, ObjectId[] selectedIds, bool addLog)
    {
        // 因为无法遍历到在位编辑的块内图元,只能进行布尔运算
        if (IsAddOperation(lastPrompt))
        {
            // 处理边缘条件,过滤获取非工作集的,添加到工作集.
            selectedIds = selectedIds.Where(a => !_workSet.Contains(a)).ToArray();
            _workSet.Add(selectedIds);

            if (addLog)
            {
                // 创建在位编辑添加动作
                var action = new InPlaceAddAction(selectedIds);
                _logger.LogAction(action, "REFSET_ADD");
            }
            return;
        }
        else if (IsRemoveOperation(lastPrompt))
        {
            // 处理边缘条件,过滤获取工作集的,移除出工作集.
            selectedIds = selectedIds.Where(_workSet.Contains).ToArray();
            _workSet.ExceptWith(selectedIds);

            if (addLog)
            {
                // 创建在位编辑移除动作
                var action = new InPlaceRemoveAction(selectedIds);
                _logger.LogAction(action, "REFSET_REMOVE");
            }
            return;
        }

        if (lastPrompt.Contains("已在工作集") || lastPrompt.Contains("不在工作集中"))
            return;

        //throw new System.Exception("不是添加或删除: " + lastPrompt);
    }

    /// <summary>
    /// 判断是否为添加操作
    /// </summary>
    private bool IsAddOperation(string lastPrompt)
    {
        return lastPrompt.Contains("添加") || lastPrompt.Contains("Added");
    }

    /// <summary>
    /// 判断是否为删除操作
    /// </summary>
    private bool IsRemoveOperation(string lastPrompt)
    {
        return lastPrompt.Contains("删除") || lastPrompt.Contains("Removed");
    }
    #endregion

    // 公共 Dispose 方法
    public void Dispose()
    {
        Dispose(true);
        // 阻止垃圾回收器调用析构函数
        GC.SuppressFinalize(this);
    }

    // 受保护的虚拟 Dispose 方法
    protected virtual void Dispose(bool disposing)
    {
        if (_IsDisposed)
            return;
        _IsDisposed = true;

        if (disposing)
        {
            // 释放托管资源
            _document.CommandEnded -= OnCommandEnded;
            _document.CommandWillStart -= OnCommandWillStart;

            _document.Database.ObjectAppended -= OnObjectAppended;
            _document.Database.ObjectModified -= OnObjectModified;
            _document.Database.ObjectErased -= OnObjectErased;
        }
    }
}
