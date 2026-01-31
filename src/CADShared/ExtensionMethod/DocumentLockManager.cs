namespace IFoxCAD.Cad;

/// <summary>
/// 文档锁管理器，用于管理文档的锁定和解锁。
/// </summary>
public static class DocumentLockManager
{
    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
        Acap.DocumentManager.DocumentLockModeChangeVetoed += DocumentManager_DocumentLockModeChangeVetoed; ;
        Acap.DocumentManager.DocumentLockModeWillChange += DocumentManager_DocumentLockModeWillChange;
    }

    private static void DocumentManager_DocumentLockModeChangeVetoed(object sender, DocumentLockModeChangeVetoedEventArgs e)
    {
    }

    private static void DocumentManager_DocumentLockModeWillChange(object sender, DocumentLockModeWillChangeEventArgs e)
    {
#if true2
        var doc = e.Document;

        // 三个事件,这里也是不可以加锁的
        //var l = doc.LockDocument();


        // 进入锁定
        if (e.CurrentMode == DocumentLockMode.NotLocked && e.MyCurrentMode == DocumentLockMode.Write)
        {
            // 1.0 文档刚刚被锁定（开始写操作）

            // 这里无法拿到文档锁对象,但是此处属于已经锁定的,
            // 因此我们可以在容器内添加这个文档,而没有解锁措施.
            if (!_docAndLockMap.ContainsKey(doc.Database))
            {
                _docAndLockMap[doc.Database] = (doc, null);
                DebugEx.Printl("1.0 文档刚刚被锁定");
            }
        }
        else if (e.CurrentMode == DocumentLockMode.Write && e.MyCurrentMode == DocumentLockMode.NotLocked)
        {
            // 2.0 文档刚刚被解锁（写操作完成）
            _docAndLockMap.Remove(doc.Database);
            DebugEx.Printl("2.0 文档刚刚被解锁");
        }
        else if (e.CurrentMode == DocumentLockMode.Write && e.MyCurrentMode == DocumentLockMode.Write)
        {
            // 3.0 嵌套锁?通过计数实现吗?但是感觉没释放啊?
            DebugEx.Printl("3.0 嵌套锁");
        }
        else if (e.CurrentMode == DocumentLockMode.NotLocked && e.MyCurrentMode == DocumentLockMode.NotLocked)
        {
            DebugEx.Printl("4.0 没有锁");
        }
        else
        {
            throw new System.Exception("这是啥状态?");
        }

#endif
        //DebugEx.Printl($"11: {e.CurrentMode}, {e.MyCurrentMode}, {e.MyNewMode}");
    }


    /// <summary>
    /// 触发条件是加锁之后,用来补充锁
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="System.Exception"></exception>
    private static void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        // 可以阻止特定命令
        // e.Veto()
        // DebugEx.Printl($"22: {e.CurrentMode}, {e.MyCurrentMode}, {e.MyPreviousMode}");
    }


    /// <summary>
    /// 文档锁容器
    /// 为什么用数据库作为key,因为数据库是前后台都有的.
    /// 有key就表示有锁定中,锁定中也锁对象也可能为null
    /// </summary>
    private static readonly Dictionary<Database, (Document Document, DocumentLock? DocumentLock)> _docAndLockMap = [];

    /// <summary>
    /// 添加文档锁
    /// </summary>
    public static DocumentLockPlus LockDocument(Document doc)
    {
#if !NET35
        // 这是高版本才有的,但是我实现了死锁检测耶,直接屏蔽算了.
        // 如果文档未锁定，则尝试锁定文档，否则不创建锁实例。
        if (doc.LockMode(false) != DocumentLockMode.NotLocked)
            throw new Exception($"该文档已经锁定,重复加锁导致死锁,请修改逻辑: {doc.Name}");
#endif

        if (_docAndLockMap.TryGetValue(doc.Database, out var dlock))
            throw new Exception($"该文档已经锁定,重复加锁导致死锁,请修改逻辑: {dlock.Document.Name}");

        var docker = doc.LockDocument();
        _docAndLockMap[doc.Database] = (doc, docker);
        return new DocumentLockPlus(doc);
    }

    /// <summary>
    /// 释放并移除文档锁
    /// </summary>
    /// <param name="doc"></param>
    public static bool RemoveLock(Document doc)
    {
        if (_docAndLockMap.TryGetValue(doc.Database, out var dlock))
        {
            dlock.DocumentLock?.Dispose();
            _docAndLockMap.Remove(doc.Database);
            return true;
        }
        return false;
    }
}