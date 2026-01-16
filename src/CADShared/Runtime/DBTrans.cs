namespace IFoxCAD.Cad;

using System.Diagnostics;
using System.IO;
using Exception = System.Exception;
using Acaop = Application;
using IFoxCAD.Com;


/// <summary>
/// 事务栈
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(DBTrans))]
public sealed class DBTrans : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    #region 公共静态函数

    /// <summary>
    /// 获取原生事务栈顶
    /// </summary>
    /// <param name="db">数据库</param>
    /// <returns>原生事务</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Transaction? GetTopTransaction(Database db)
    {
        var tr = db?.TransactionManager.TopTransaction;
        return tr is null ? throw new ArgumentNullException(nameof(DBTrans), $"此数据库{db}没有原生事务") : tr;
    }

    /// <summary>
    /// 获取栈顶事务
    /// </summary>
    /// <param name="db">数据库,默认是工作数据库</param>
    /// <returns>事务对象</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DBTrans GetTop(Database? db = null)
    {
        // 工作数据库和当前文档数据库是不一样的.
        // 例如布局管理器是经过工作数据库取值的,我们事务栈也是如此.
        // 而默认构造函数则以当前文档创建.
        // 工作文档是空的场景: 全部文档关闭,后台打开图纸,
        // 要WorkingDatabase=后台db,并且提交事务前恢复原本.
        db ??= HostApplicationServices.WorkingDatabase;
        if (db is null)
            throw new ArgumentNullException(nameof(DBTrans), $"工作数据库为空,后台调用需先设置或调用Task函数");
        if (_dBTrans.Count == 0)
            throw new ArgumentNullException(nameof(DBTrans), $"调用前必须创建事务栈");
        if (!_dBTrans.TryGetValue(db, out var trStack))
            throw new ArgumentNullException(nameof(DBTrans), $"此数据库{db}没有加入事务栈");
        return trStack.Peek();
    }

    /// <summary>
    /// 获取栈顶事务
    /// </summary>
    public static DBTrans Top => GetTop();

    // 提供一个查询不报异常的,用于后台常开的dbMap检索用
    /// <summary>
    /// 尝试获取栈顶事务
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetTop(Database db, out DBTrans dBTrans)
    {
        if (_dBTrans.TryGetValue(db, out var trStack))
        {
            dBTrans = trStack.Peek();
            return true;
        }
        dBTrans = null!;
        return false;
    }

    /// <summary>
    /// 事务栈是否已经包含此数据库
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(Database db)
    {
        return _dBTrans.ContainsKey(db);
    }

    /// <summary>
    /// 设置工作数据库
    /// </summary>
    public static void SetWorking(Database db)
    {
        if (db is null)
            throw new ArgumentNullException(nameof(db));
        HostApplicationServices.WorkingDatabase = db;
    }

    /// <summary>
    /// 隐式转换为原生事务
    /// </summary>
    /// <param name="tr">事务栈</param>
    /// <returns>原生事务</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Transaction(DBTrans tr) => tr._transaction;

    #endregion

    #region 静态资源

    // 静态资源是不算到类大小的.
    /// <summary>
    /// 事务栈
    /// </summary>
    private static readonly Dictionary<Database, Stack<DBTrans>> _dBTrans = [];

    /// <summary>
    /// 文档锁 map[文档名,文档锁]
    /// </summary>
    private readonly static Dictionary<Database, (Document Document, DocumentLock DocumentLock)> _docAndLockMap = [];
    #endregion

    #region 本类字段
    // 既然可以通过隐式转换,那么就私有它
    // 例如: Transaction tr = DBTrans.Top;
    /// <summary>
    /// 原生事务
    /// </summary>
    private readonly Transaction _transaction;

    /// <summary>
    /// 数据库
    /// </summary>
    private readonly Database _database;

    /// <summary>
    /// 提交事务和释放标记
    /// </summary>
    private TransStatus _transStatus = new();

    #endregion

    #region 公开属性

    /// <summary>
    /// 文档
    /// </summary>
    public Document? Document
    {
        get
        {
            if (_docAndLockMap.TryGetValue(_database, out var dx))
                return dx.Document;
            return null;
            //// Acad2014找不到会报错,2024则不会
            //try { return Acaop.DocumentManager.GetDocument(_database); }
            //catch { return null; }
        }
    }

    /// <summary>
    /// 命令行
    /// </summary>
    public Editor? Editor => Document?.Editor;

    /// <summary>
    /// 数据库
    /// </summary>
    public Database Database => _database;

    #endregion

    #region 构造函数


    /// <summary>
    /// 事务栈
    /// <para>默认构造函数,默认为打开当前文档,默认提交事务</para>
    /// </summary>
    /// <param name="doc">要打开的文档</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="docLock">是否锁文档</param>
    /// <param name="openCloseTrans">无撤事务</param>
    public DBTrans(Document? doc = null, bool commit = true,
     bool docLock = false, bool openCloseTrans = false)
    {
        doc ??= Acaop.DocumentManager.MdiActiveDocument;

        // 如果已经锁了就不再锁
        //#if !NET35
        //        // 用这个可以避免多个插件进行锁
        //        if (docLock && doc.LockMode(false) == DocumentLockMode.NotLocked)
        //            _documentLock = doc.LockDocument();
        //#endif

        // 用这个只能大家都用IFoxCAD才能避免多次锁,除非把它做成共享内存.
        if (docLock)
        {
            if (_docAndLockMap.ContainsKey(doc.Database))
            {
                throw new ArgumentNullException("文档已经锁定,切勿重复加锁");
            }
            _docAndLockMap[doc.Database] = (doc, doc.LockDocument());
        }

        _database = doc.Database;
        CheckDatabaseError();
        var tm = _database.TransactionManager;
#if !NET35
        if (openCloseTrans)
            _transaction = tm.StartOpenCloseTransaction();
        else
            _transaction = tm.StartTransaction();
#else
        _transaction = tm.StartTransaction();
#endif
        if (commit) _transStatus.Commit();
        if (!_dBTrans.TryGetValue(_database, out var trStack))
        {
            trStack = new();
            _dBTrans.Add(_database, trStack);
        }
        trStack.Push(this);
    }

    /// <summary>
    /// 事务栈
    /// <para>打开数据库,默认提交事务</para>
    /// </summary>
    /// <param name="db">要打开的数据库</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="openCloseTrans">无撤事务</param>
    public DBTrans(Database db, bool commit = true, bool openCloseTrans = false)
    {
        _database = db;
        CheckDatabaseError();
        var tm = _database.TransactionManager;
#if !NET35
        if (openCloseTrans)
            _transaction = tm.StartOpenCloseTransaction();
        else
            _transaction = tm.StartTransaction();
#else
        _transaction = tm.StartTransaction();
#endif
        if (commit) _transStatus.Commit();
        if (!_dBTrans.TryGetValue(_database, out var trStack))
        {
            trStack = new();
            _dBTrans.Add(_database, trStack);
        }
        trStack.Push(this);
    }

#if !NET35
    // 嵌套此事务是可以替换句柄的
    // https://www.cnblogs.com/JJBox/p/12489648.html
    /// <summary>
    /// 无撤事务
    /// <para>此处提交事务,外层可以进行回滚</para>
    /// </summary>
    /// <param name="action">无撤事务</param>
    /// <param name="commit">是否提交</param>
    public void StartTransaction(Action<OpenCloseTransaction> action, bool commit = true)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        using var tr = _database.TransactionManager.StartOpenCloseTransaction();
        action.Invoke(tr);
        if (commit) tr.Commit();
    }
#endif

    /*
    0x01,前台开图创建文档,记录doc的.
    前台打开会被文档持有,是无法释放db的,并且需要发送命令保存和关闭.

    后台开图或者后台创建数据库,不记录doc的,
    所以doc is null也能保证是后台开的,用于释放后台打开的db,
    所以我删掉file字段了,
    并且移除了保存方式到外部,它就更加没有存在价值了.

    0x02,原本参数有file的构造函数存在问题

    21,有同名file已经加入事务栈,表示已经开过,
    但是在此无论如何都会再次开图,因为构造函数必然是构造有效对象.
    所以构造函数无法避免此类情况,
    调用者想要避免此场景,要自己去构造 fdbMap<file, db>,

    22,文件已经在前台打开(文档集合已经持有),但是通过file参数要求后台打开.
    出现后台只读场景,功能上是可行的,但是从业务上来说是矛盾的,
    你可以直接用前台文档进行呀,因此我删掉了此构造函数,
    分解成 后台打开 和 前台打开 两个静态函数,
    让调用者自行规避或明知而为.

    0x03,当前进程前台已经打开,通过文档集合判断.

    0x04,当前进程后台已经打开呢?
    你无法通过遍历文档集合得到,它压根不加入文档集合,
    因此和21规避同名file一样,自行构造后台 fdbMap<file, db>,
    再通过DBTrans.GetTop(db)得到事务,两个O(1)检索就得到了.

    41,通常后台处理完就关闭了,
    所以改为静态读取文件后加入事务栈,完成就提交释放,
    为了便利性,期间会一直持有事务直到提交后自动关闭数据库.

    42,后台常开去拷贝数据呢?多次提交事务呢?是可能的.
    所以提供静态创建数据库函数,不加入事务栈.
    由调用者自行持有.

    0x05,前台开图必须设置: CommandFlags.Session 标记
    前台开图如果用命令,不设置标记的话就会卡死.

    如何判断调用的函数位置是Session呢?
    目前是让调用者自己肉眼保证,没有报错机制.
    感觉不可行的方案:
    拦截运行时的命令,获取输入中的命令,然后反射全部命令,
    因为事件也是Session环境,也可以调用事务栈,但是此时没有命令.
    并且还不一定用桌子的命令特性定义命令.
    */

    /// <summary>
    /// 后台打开文件并加入事务栈
    /// <para>默认提交事务</para>
    /// <para>后台打开后执行任务需要用Task函数保证工作数据库正确</para>
    /// </summary>
    /// <param name="file">要打开的文件</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="fileOpenMode">开图模式</param>
    /// <param name="password">密码</param>
    /// <param name="openCloseTrans">无撤事务</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static DBTrans OpenPushToBackend(string file, bool commit = true,
        FileOpenMode fileOpenMode = FileOpenMode.OpenForReadAndWriteNoShare,
        string? password = null, bool openCloseTrans = false)
    {
        if (StringHelper.IsNullOrWhiteSpace(file))
            throw new FileNotFoundException(nameof(OpenPushToBackend), "文件后缀不是dxf/dwg");
        Database? db = null;

        // 排除只读,因为磁盘文件可能已经释放可以可写打开.
        // doc.Name: "D:\\JX.dwg" 比较时候需要一致
        file = file.Replace("/", "\\");
        var fdocMap = FileDocumentMap();
        if (fdocMap.TryGetValue(file, out var doc))
        {
            db = doc.Database;
        }
        // 此处创建的数据库是肯定获取不到文档的.
        db ??= OpenFileForBackend(file, fileOpenMode, password);
        return new DBTrans(db, commit, openCloseTrans: openCloseTrans);
    }

    /// <summary>
    /// 前台打开文件并加入事务栈
    /// <para>默认提交事务,需要设置CommandFlags.Session</para>
    /// </summary>
    /// <param name="file">要打开的文件</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="fileOpenMode">开图模式</param>
    /// <param name="password">密码</param>
    /// <param name="openCloseTrans">无撤事务</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static DBTrans OpenPushToFrontend(string file, bool commit = true,
        FileOpenMode fileOpenMode = FileOpenMode.OpenForReadAndWriteNoShare,
        string? password = null, bool openCloseTrans = false)
    {
        if (StringHelper.IsNullOrWhiteSpace(file))
            throw new FileNotFoundException(nameof(OpenPushToFrontend), "文件后缀不是dxf/dwg");
        if (!File.Exists(file))
            throw new FileNotFoundException(nameof(OpenPushToFrontend), "文件必须存在");

        // 排除只读,因为磁盘文件可能已经释放可以可写打开.
        // doc.Name: "D:\\JX.dwg" 比较时候需要一致
        file = file.Replace("/", "\\");
        var fdocMap = FileDocumentMap();
        if (!fdocMap.TryGetValue(file, out var doc))
        {
            doc ??= OpenFileForFrontend(file, fileOpenMode, password);
        }

        // 前台进行同步激活文档和工作数据库
        // 后台为什么不设置呢?因为后台需要还原,
        // 因此不在打开时候修改工作数据库,而是制作Task函数进行.
        // 而前台由文档持有,并通过它自己的机制保证关闭时候切换.
        var dm = Acaop.DocumentManager;
        if (!doc.IsActive)
        {
            dm.MdiActiveDocument = doc;
            HostApplicationServices.WorkingDatabase = doc.Database;
        }

        // file是当前文档就不用加锁,但是命令有Session就要加文档锁.
        // 非当前文档要激活切换,此时必然是跨文档,也要加文档锁.
        // 因此需要统一命令加上Session并且加文档锁.
        // 否则 Editor?.Redraw() tm.QueueForGraphicsFlush() 将报错提示文档锁
        return new DBTrans(doc, commit, true);
    }


    // 若命令没有设置 CommandFlags.Session
    // Open会卡死进入中断状态不会执行打开,
    // 直到切换文档ctrl+tab或者关闭文档,
    // 并且doc.IsActive会异常
    // 若是构造函数出错,导致Dispose函数出现需要判断本类资源为空场景.
    /// <summary>
    /// 前台开图
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="fileOpenMode">模式</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Document OpenFileForFrontend(string file, FileOpenMode fileOpenMode, string? password)
    {
        try
        {
            var dm = Acaop.DocumentManager;
            return dm.Open(file,
                fileOpenMode == FileOpenMode.OpenForReadAndReadShare,
                password);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /*
            using var db = new Database(false, true);
            db.ReadDwgFile(file, FileShare.ReadWrite, true, null);
            var tr = db.TransactionManager.StartTransaction(); 
            var count = db.GetHostDwgXrefGraph(false).NumNodes
    */

    // 文件不存在,创建数据库,允许之后保存.
    // 文件存在,前台文档集合没找到,后台开图.
    // 文件存在,前台文档集合找到,后台只读打开,诡异的业务场景,会致命错误吗?
    // Acad08测试: new Database()第2个参数使用false时,
    // 将导致关闭cad的时候出现致命错误:
    // Unhandled Access Violation Reading Ox113697a0 Exception at 4b4154h
    // 报错: ePermanentlyErased 就是new Database()参数写错了.
    // 报错: eFileSharingViolation 他人在打开此图.
    // 报错: eWrongObjectType 表示你没有用Task函数包裹.

    /// <summary>
    /// 后台打开图纸
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="fileOpenMode">模式</param>
    /// <param name="password">密码</param>
    /// <returns>数据库</returns>
    /// <exception cref="FileNotFoundException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Database OpenFileForBackend(string file, FileOpenMode fileOpenMode, string? password)
    {
        if (!File.Exists(file))
            return new Database(true, true);

        var db = new Database(false, true);
        var ext = Path.GetExtension(file);
        if (string.Equals(ext, ".dwg", StringComparison.OrdinalIgnoreCase))
        {
#if ac2008
            db.ReadDwgFile(file, FileOpenModeHelper.GetFileShare(fileOpenMode), true, password);
#else
            db.ReadDwgFile(file, fileOpenMode, true, password);
#endif
        }
        else if (string.Equals(ext, ".dxf", StringComparison.OrdinalIgnoreCase))
            db.DxfIn(file, null);
        else throw new FileNotFoundException(nameof(OpenFileForBackend), $"文件后缀不是dxf/dwg: {ext}");
        // 读取文件就断开连接,释放磁盘占用,避免其他不能读取和删除.
        db.CloseInput(true);
        return db;
    }

    // 为了方便内联,这个函数还是写轻松点,大不了其他人重写一份
    /// <summary>
    /// 前台文件路径和文档映射表
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<string, Document> FileDocumentMap()
    {
        var dm = Acaop.DocumentManager;
        var map = new Dictionary<string, Document>();
        foreach (Document doc in dm)
        {
            if (doc.IsDisposed || doc.IsReadOnly)
                continue;
            map[doc.Name] = doc;
        }
        return map;
    }

    // 后台可以之后加入
    /// <summary>
    /// 前台文件路径和数据库映射表
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<string, Database> FileDatabaseMap()
    {
        var dm = Acaop.DocumentManager;
        var map = new Dictionary<string, Database>();
        foreach (Document doc in dm)
        {
            if (doc.IsDisposed || doc.IsReadOnly) continue;
            if (!map.ContainsKey(doc.Name))
                map.Add(doc.Name, doc.Database);
        }
        return map;
    }

    #endregion

    #region 符号表
    /// <summary>
    /// 块表
    /// </summary>
    public SymbolTable<BlockTable, BlockTableRecord> BlockTable =>
        _blockTable ??= new(this, _database.BlockTableId);
    private SymbolTable<BlockTable, BlockTableRecord>? _blockTable;

    /// <summary>
    /// 层表
    /// </summary>
    public SymbolTable<LayerTable, LayerTableRecord> LayerTable =>
        _layerTable ??= new(this, _database.LayerTableId);
    private SymbolTable<LayerTable, LayerTableRecord>? _layerTable;

    /// <summary>
    /// 文字样式表
    /// </summary>
    public SymbolTable<TextStyleTable, TextStyleTableRecord> TextStyleTable =>
        _textStyleTable ??= new(this, _database.TextStyleTableId);
    private SymbolTable<TextStyleTable, TextStyleTableRecord>? _textStyleTable;

    /// <summary>
    /// 注册应用程序表
    /// </summary>
    public SymbolTable<RegAppTable, RegAppTableRecord> RegAppTable =>
        _regAppTable ??= new(this, _database.RegAppTableId);
    private SymbolTable<RegAppTable, RegAppTableRecord>? _regAppTable;

    /// <summary>
    /// 标注样式表
    /// </summary>
    public SymbolTable<DimStyleTable, DimStyleTableRecord> DimStyleTable =>
        _dimStyleTable ??= new(this, _database.DimStyleTableId);
    private SymbolTable<DimStyleTable, DimStyleTableRecord>? _dimStyleTable;

    /// <summary>
    /// 线型表
    /// </summary>
    public SymbolTable<LinetypeTable, LinetypeTableRecord> LinetypeTable =>
        _linetypeTable ??= new(this, _database.LinetypeTableId);
    private SymbolTable<LinetypeTable, LinetypeTableRecord>? _linetypeTable;

    /// <summary>
    /// 用户坐标系表
    /// </summary>
    public SymbolTable<UcsTable, UcsTableRecord> UcsTable =>
        _ucsTable ??= new(this, _database.UcsTableId);
    private SymbolTable<UcsTable, UcsTableRecord>? _ucsTable;

    /// <summary>
    /// 视图表
    /// </summary>
    public SymbolTable<ViewTable, ViewTableRecord> ViewTable =>
        _viewTable ??= new(this, _database.ViewTableId);
    private SymbolTable<ViewTable, ViewTableRecord>? _viewTable;

    /// <summary>
    /// 视口表
    /// </summary>
    public SymbolTable<ViewportTable, ViewportTableRecord> ViewportTable =>
        _viewportTable ??= new(this, _database.ViewportTableId);
    private SymbolTable<ViewportTable, ViewportTableRecord>? _viewportTable;

    #endregion

    #region 表记录

    private readonly Dictionary<ObjectId, DBObject> _objectCache = new();

    private T GetCache<T>(ObjectId objectId) where T : DBObject
    {
        if (_objectCache.TryGetValue(objectId, out var obj)
            && obj is T result)
        {
            return result;
        }
        result = (T)GetObject(objectId);
        _objectCache.Add(objectId, result);
        return result;
    }

    /// <summary>
    /// 当前绘图空间(可能是不同布局的)
    /// </summary>
    public BlockTableRecord CurrentSpace =>
        GetCache<BlockTableRecord>(_database.CurrentSpaceId);

    /// <summary>
    /// 模型空间
    /// </summary>
    public BlockTableRecord ModelSpace =>
        GetCache<BlockTableRecord>(this.BlockTable[BlockTableRecord.ModelSpace]);

    /// <summary>
    /// 图纸空间
    /// </summary>
    public BlockTableRecord PaperSpace =>
        GetCache<BlockTableRecord>(this.BlockTable[BlockTableRecord.PaperSpace]);

    /// <summary>
    /// 命名对象字典
    /// </summary>
    public DBDictionary NamedObjectsDict =>
        GetCache<DBDictionary>(_database.NamedObjectsDictionaryId);

    /// <summary>
    /// 组字典
    /// </summary>
    public DBDictionary GroupDict =>
        GetCache<DBDictionary>(_database.GroupDictionaryId);

    /// <summary>
    /// 多重引线样式字典
    /// </summary>
    public DBDictionary MLeaderStyleDict =>
        GetCache<DBDictionary>(_database.MLeaderStyleDictionaryId);

    /// <summary>
    /// 多线样式字典
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public DBDictionary MLStyleDict =>
        GetCache<DBDictionary>(_database.MLStyleDictionaryId);

    /// <summary>
    /// 材质字典
    /// </summary>
    public DBDictionary MaterialDict =>
        GetCache<DBDictionary>(_database.MaterialDictionaryId);

    /// <summary>
    /// 表格样式字典
    /// </summary>
    public DBDictionary TableStyleDict =>
        GetCache<DBDictionary>(_database.TableStyleDictionaryId);

    /// <summary>
    /// 视觉样式字典
    /// </summary>
    public DBDictionary VisualStyleDict =>
        GetCache<DBDictionary>(_database.VisualStyleDictionaryId);

    /// <summary>
    /// 颜色字典
    /// </summary>
    public DBDictionary ColorDict =>
        GetCache<DBDictionary>(_database.ColorDictionaryId);

    /// <summary>
    /// 打印设置字典
    /// </summary>
    public DBDictionary PlotSettingsDict =>
        GetCache<DBDictionary>(_database.PlotSettingsDictionaryId);

    /// <summary>
    /// 打印样式表名字典
    /// </summary>
    public DBDictionary PlotStyleNameDict =>
        GetCache<DBDictionary>(_database.PlotStyleNameDictionaryId);

    /// <summary>
    /// 布局字典
    /// </summary>
    public DBDictionary LayoutDict =>
        GetCache<DBDictionary>(_database.LayoutDictionaryId);

#if NET40_OR_GREATER
    /// <summary>
    /// 数据链接字典
    /// </summary>
    public DBDictionary DataLinkDict =>
        GetCache<DBDictionary>(_database.DataLinkDictionaryId);

    /// <summary>
    /// 详细视图样式字典
    /// </summary>
    public DBDictionary DetailViewStyleDict =>
        GetCache<DBDictionary>(_database.DetailViewStyleDictionaryId);

    /// <summary>
    /// 剖面视图样式字典
    /// </summary>
    public DBDictionary SectionViewStyleDict =>
        GetCache<DBDictionary>(_database.SectionViewStyleDictionaryId);
#endif
    #endregion

    #region 通用方法

    /// <summary>
    /// 根据对象id获取对象
    /// </summary>
    /// <param name="id">对象id</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>数据库DBObject对象</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DBObject GetObject(ObjectId id, OpenMode openMode = OpenMode.ForRead,
        bool openErased = false, bool openLockedLayer = false)
    {
        // 由于锁图层+读模式+提权是失败的,所以要直接用写模式.
        if (openLockedLayer)
            openMode = OpenMode.ForWrite;
        return _transaction.GetObject(id, openMode, openErased, openLockedLayer);
    }

    /*
    返回值T还是T?
    若返回值不可空,但是这函数又强转类,所以强转失败会为null.
    若返回值可空,你就必须每次要调用之后判断空,
    但是很多时候是明确类型的,会多了判断语句.
    还记得那句话吗:可空类型标记不是让你写if,而是让你尽可能不写if.
    是is还是as呢?这就是我们得放弃这个API.
    */
    /// <summary>
    /// 根据对象id获取图元对象
    /// </summary>
    /// <typeparam name="T">要获取的图元对象的类型</typeparam>
    /// <param name="id">对象id</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>图元对象</returns>
    //[Obsolete("可空类型标记出现后,建议使用非泛型标记的", false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetObject<T>(ObjectId id, OpenMode openMode = OpenMode.ForRead,
        bool openErased = false, bool openLockedLayer = false) where T : DBObject
    {
        return _transaction.GetObject(id, openMode, openErased, openLockedLayer) as T;
    }

    /// <summary>
    /// id有效,未被删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOk(ObjectId id)
        => !id.IsNull && id.IsValid && !id.IsErased && !id.IsEffectivelyErased && id.IsResident;


    /// <summary>
    /// 根据句柄获取对象Id
    /// </summary>
    /// <param name="handleString">句柄字符串</param>
    /// <returns>对象Id</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectId GetObjectId(string handleString)
    {
        Handle handle;
        if (IntPtr.Size == 4)
            handle = new Handle(Convert.ToInt32(handleString, 16));
        else
            handle = new Handle(Convert.ToInt64(handleString, 16));
        return _database.TryGetObjectId(handle, out ObjectId id) ? id : ObjectId.Null;
    }

    /// <summary>
    /// 根据句柄获取对象Id
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <returns>对象Id</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectId GetObjectId(Handle handle)
    {
        return _database.TryGetObjectId(handle, out ObjectId id) ? id : ObjectId.Null;
    }

    /// <summary>
    /// 将新创建对象加入或移出事务
    /// <para>不是新创建对象移出会异常:eNotNewlyCreated</para>
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="add">true是加入事务,false是移出事务</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNewlyCreatedDBObject(DBObject obj, bool add = true)
    {
        _transaction.AddNewlyCreatedDBObject(obj, add);
    }


    /// <summary>
    /// 事务栈任务自动处理前台后台
    /// </summary>
    /// <param name="action">委托</param>
    /// <param name="echo">回声,可以选择就是可以排除</param>
    public DBTrans Task(Action action, Echo echo = Echo.All)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (CheckDatabaseError(echo.HasFlag(Echo.FatalErrors)))
        {
            return this;
        }

        Database? dbBak = HostApplicationServices.WorkingDatabase;
        Document? docBak = Acaop.DocumentManager.MdiActiveDocument;
        var doc = Document;

        // 直接执行: 当前文档前台开图 或 当前就是工作数据库 
        if ((docBak is not null && docBak == doc && docBak.Database == dbBak)
           || (dbBak is not null && dbBak == _database))
        {
            try
            {
                action.Invoke();
                return this;
            }
            catch (Exception ex)
            {
                if (!echo.HasFlag(Echo.Errors))
                    throw; // 不,加ex表示由我的栈帧抛出.
                System.Diagnostics.Trace.WriteLine(
                    $"捕获到异常:\n" +
                    $"错误时间: {DateTime.Now}\n" +
                    $"错误信息: {ex.Message}\n" +
                    $"堆栈跟踪: {ex.StackTrace}");
                return this;
            }
        }

        // GetTop()允许
        // 1,当前文档就是工作数据库.
        // 2,工作数据库不是当前文档,例如克隆后台数据到前台.
        // GetTop()阻止
        // 3,工作数据库为空表示:关闭全部文档并且后台开图.
        // 设置 工作数据库=后台db,之后进入任务,完成任务后还原.

        // 设置工作数据库还顺便处理了深度克隆导致单行文字偏移.
        // 布局管理器取值和设置是经过工作数据库.

        // 前台绑定参照用此方法会抛出异常:eWasErased
        // 是不是之前测试有问题呢?
        // 可能是原本只有一个栈的错误弹栈导致的.
        // 可能是绑定要调用其他引擎,此时需要Session呢?
        // 可能是用完工作数据库之后,提交事务前忘记切换.(现在才知道有这回事)
        // 可能是已经位于前台文档集合中,但是没有激活为当前文档,
        // 激活岂不是要设置Session?用事件处理?发送异步命令?
        // 发送异步命令激活文档,但是委托怎么传入进去呢?
        // 又或许这种切换是可以的,只是要文档锁?
        // 切换会不会刷新工作数据库呢?影响了这两行位置
        // 让用户自己去保证吧.
        if (doc is not null && !doc.IsDisposed && docBak != doc)
        {
            if (echo.HasFlag(Echo.ActiveDocument))
            {
                System.Diagnostics.Trace.WriteLine(
                     $"前台文档,非激活状态执行任务\n" +
                     $"规避错误需要设置CommandFlags.Session+切换文档+文档锁");
            }
            // 调用者需要外部设置和恢复
            // Acaop.DocumentManager.MdiActiveDocument = doc;
        }

        HostApplicationServices.WorkingDatabase = _database;
        try
        {
            action.Invoke();
            return this;
        }
        catch (Exception ex)
        {
            if (!echo.HasFlag(Echo.Errors))
                throw; // 不,加ex表示由我的栈帧抛出.
            System.Diagnostics.Trace.WriteLine(
                $"捕获到异常:\n" +
                $"错误时间: {DateTime.Now}\n" +
                $"错误信息: {ex.Message}\n" +
                $"堆栈跟踪: {ex.StackTrace}");
            return this;
        }
        finally
        {
            // 工作数据库=后台db,使用完后还原应该允许设置null的.
            HostApplicationServices.WorkingDatabase = dbBak;

            // if (docBak is not null && !docBak.IsDisposed)
            //    Acaop.DocumentManager.MdiActiveDocument = docBak;
        }
    }
    #endregion

    #region IDisposable接口相关函数

    /// <summary>
    /// 取消事务
    /// </summary>
    public void Abort()
    {
        _transStatus.Abort();
        Dispose();
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit()
    {
        _transStatus.Commit();
        Dispose();
    }

    /// <summary>
    /// 释放标记
    /// </summary>
    public bool IsDisposed => _transStatus.IsDisposed;

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
    ~DBTrans() => Dispose(false);

    /* 
    事务释放流程：
    1,根据传入的_transStatus参数确定是否提交,
    2,不管是否提交,既然进入dispose,就将当前事务弹出.
    3,将文档锁释放
    4,静态字段也就是全局变量,生命周期和进程一样,由GC释放.
    5,通过事务栈打开读取的文件,需要释放数据库,否则遗忘释放了,
    并且在此之后需要保证 工作数据库 是其他数据库,
    工作数据库如果存放 释放了的数据库 ,这会产生错误.
    为了简化这个流程,本类写了一个Task函数,使得后台用完工作数据库就切换.
    */
    /// <summary>
    /// 释放函数
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (_transStatus.IsDisposed) return;
        _transStatus.Dispose();

        // 释放本类资源
        if (disposing)
        {
            // 构造函数引发致命错误时,
            // 本类资源会被清空,此处会连锁报错,
            // 因此去掉打开文件的构造函数,避免构造出错,
            // 构造都是有效对象,此处也就不需要判断为空了
            if (!_transaction.IsDisposed) // 防止隐式转换提交绕过
            {
                if (_transStatus.IsCommit)
                {
                    CheckWorkingDatabaseSync();
                    _transaction.Commit();
                }
                else
                {
                    // 如果是前台文档集合的,但是不是当前文档呢?
                    // 和Task一样,由用户自己保证.
                    // 防止事务回滚造成的视图回滚
                    using var vtr = Editor?.GetCurrentView();
                    _transaction.Abort();
                    if (vtr is not null) Editor?.SetCurrentView(vtr);
                }
                _transaction.Dispose();
            }

            if (_docAndLockMap.TryGetValue(_database, out var dx))
            {
                dx.Document.Dispose();
                _docAndLockMap.Remove(_database);
            }

            // 表记录释放
            foreach (var pair in _objectCache)
            {
                pair.Value.Dispose();
            }
            _objectCache.Clear();

            // 符号表释放
            //_blockTable?.Dispose();
            //_layerTable?.Dispose();
            //_textStyleTable?.Dispose();
            //_regAppTable?.Dispose();
            //_dimStyleTable?.Dispose();
            //_linetypeTable?.Dispose();
            //_ucsTable?.Dispose();
            //_viewTable?.Dispose();
            //_viewportTable?.Dispose();
        }

        // 释放全局资源,将当前事务栈弹栈
        if (_dBTrans.TryGetValue(_database, out var trStack))
        {
            trStack.Pop();
            if (trStack.Count == 0)
            {
                _dBTrans.Remove(_database);
                // 释放读取文件创建的数据库
                if (Document is null)
                    _database.Dispose();
            }
        }

        _blockTable = null!;
        _layerTable = null!;
        _textStyleTable = null!;
        _regAppTable = null!;
        _dimStyleTable = null!;
        _linetypeTable = null!;
        _ucsTable = null!;
        _viewTable = null!;
        _viewportTable = null!;
    }

    // 提交事务前如果工作数据库=后台图纸(还没有释放)
    // 不报错也会直接致命错误,并且没有任何信息,不知其意义.
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CheckWorkingDatabaseSync()
    {
        Database? wdb = HostApplicationServices.WorkingDatabase;
        if (wdb is null) return;
        Document? doc = Acaop.DocumentManager.GetDocument(wdb);
        if (doc is not null) return;
        throw new Exception($"致命错误,工作数据库是后台数据库");
    }

    // _database没有可空标记,
    // 被用户意外释放.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool CheckDatabaseError(bool echo = true)
    {
        if (_database.IsDisposed)
        {
            if (!echo) return true;
            throw new Exception("致命错误,数据库被错误释放");
        }
        return false;
    }

    #endregion

    #region ToString
    /// <summary>
    /// 输出事务栈信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("事务栈信息:");
        int i = 0;
        foreach (var pair in _dBTrans)
        {
            sb.AppendLine($"序号{++i}: 数据库路径: \"{pair.Key.Filename}\" - 事务栈数: {pair.Value.Count}");
        }
        sb.AppendLine("当前事务信息:");
        sb.AppendLine($"_database = \"{_database.Filename}\"");
        sb.AppendLine($"Document = {Document != null}");
        sb.AppendLine($"Editor = {Editor != null}");
        sb.AppendLine($"_transStatus = {_transStatus.ToString()}");
        sb.AppendLine($"_transaction = {_transaction.UnmanagedObject}");
        return sb.ToString();
    }

    #endregion
}
