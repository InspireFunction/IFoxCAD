namespace IFoxCAD.Cad;


using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// 事务栈
/// <para>隐匿事务在数据库其中担任的角色</para>
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(DBTrans))]
public class DBTrans : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string DebuggerDisplay => ToString(" | ");

    #region 静态函数
    /// <summary>
    /// 获取顶层事务
    /// </summary>
    /// <param name="database">数据库</param>
    /// <returns>事务对象</returns>
    public static DBTrans GetTopTransaction(Database database)
    {
        database.NotNull(nameof(database));
        var trans = database.TransactionManager.TopTransaction;
        trans.NotNull(nameof(trans));

        foreach (var item in _dBTrans)
        {
            if (item.Transaction.UnmanagedObject == trans.UnmanagedObject)
            {
                return item;
            }
        }  // 匹配事务栈内dbtrans的transaction的指针与数据库的顶层事务的指针

        return Top;
    }
    #endregion

    #region 私有字段
    /// <summary>
    /// 事务栈
    /// </summary>
    static readonly Stack<DBTrans> _dBTrans = new();
    /// <summary>
    /// 文档锁
    /// </summary>
    readonly DocumentLock? _documentLock;
    /// <summary>
    /// 是否提交事务
    /// </summary>
    bool _commit;
    /// <summary>
    /// 文件名
    /// </summary>
    readonly string? _fileName;
    #endregion

    #region 公开属性
    /// <summary>
    /// 返回当前事务
    /// </summary>
    public static DBTrans Top
    {
        get
        {
            /*
             * 0x01
             * 事务栈上面有事务,这个事务属于当前文档,
             * 那么直接提交原本事务然后再开一个(一直把栈前面的同数据库提交清空)
             * 那不就发生跨事务读取图元了吗?....否决
             *
             * 0x02
             * 跨文档事务出错 Autodesk.AutoCAD.Runtime.Exception:“eNotFromThisDocument”
             * Curves.GetEntities()会从Top获取事务(Top会new一个),此时会是当前文档;
             * 然后命令文中发生了 using DBTrans tr = new();
             * 当退出命令此事务释放,但是从来不释放Top,
             * 然后我新建了一个文档,再进行命令=>又进入Top,Top返回了前一个文档的事务
             * 因此所以无法清理栈,所以Dispose不触发,导致无法刷新图元和Ctrl+Z出错
             * 所以用AOP方式修复
             *
             * 0x03
             * 经过艰苦卓绝的测试,aop模式由于不能断点调试,所以暂时放弃。
             */

            // 由于大量的函数依赖本属性,强迫用户先开启事务
            if (_dBTrans.Count == 0)
                throw new ArgumentNullException("事务栈没有任何事务,请在调用前创建:" + nameof(DBTrans));
            var trans = _dBTrans.Peek();
            return trans;
        }
    }
    


    /// <summary>
    /// 文档
    /// </summary>
    public Document? Document { get; private set; }
    /// <summary>
    /// 命令行
    /// </summary>
    public Editor? Editor { get; private set; }
    /// <summary>
    /// 事务管理器
    /// </summary>
    public Transaction Transaction { get; private set; }
    /// <summary>
    /// 数据库
    /// </summary>
    public Database Database { get; private set; }
    #endregion

    #region 构造函数
    /// <summary>
    /// 事务栈
    /// <para>默认构造函数,默认为打开当前文档,默认提交事务</para>
    /// </summary>
    /// <param name="doc">要打开的文档</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="doclock">是否锁文档</param>
    public DBTrans(Document? doc = null, bool commit = true, bool doclock = false)
    {
        Document = doc ?? Acap.DocumentManager.MdiActiveDocument;
        Database = Document.Database;
        Editor = Document.Editor;
        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        if (doclock)
            _documentLock = Document.LockDocument();

        _dBTrans.Push(this);
    }

    /// <summary>
    /// 事务栈
    /// <para>打开数据库,默认提交事务</para>
    /// </summary>
    /// <param name="database">要打开的数据库</param>
    /// <param name="commit">事务是否提交</param>
    public DBTrans(Database database, bool commit = true)
    {
        Database = database;
        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        _dBTrans.Push(this);
    }

    /// <summary>
    /// 事务栈
    /// <para>打开文件,默认提交事务</para>
    /// </summary>
    /// <param name="fileName">要打开的文件名</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="fileOpenMode">开图模式</param>
    /// <param name="password">密码</param>
    /// <param name="activeOpen">后台打开false;前台打开true(必须设置CommandFlags.Session)</param>
    public DBTrans(string fileName,
                   bool commit = true,
                   FileOpenMode fileOpenMode = FileOpenMode.OpenForReadAndWriteNoShare,
                   string? password = null,
                   bool activeOpen = false)
    {
        if (fileName == null || string.IsNullOrEmpty(fileName.Trim()))
            throw new ArgumentNullException(nameof(fileName));

        _fileName = fileName.Replace("/", "\\");// doc.Name总是"D:\\JX.dwg"

        // 此处若为失败的文件名,那么保存的时候就会丢失名称,
        // 因此用 _fileName 储存
        if (!File.Exists(_fileName))
        {
            if (activeOpen)
            {
                throw new IOException("错误:事务栈明确为前台开图时,文件不存在");
            }
            else
            {
                // cad08测试:
                // 第2个参数使用false,将导致关闭cad的时候出现致命错误:
                // Unhandled Access Violation Reading Ox113697a0 Exception at 4b4154h
                Database = new Database(true, true);
            }
        }
        else
        {
            var doc = Acap.DocumentManager
                     .Cast<Document>()
                     .FirstOrDefault(doc => !doc.IsDisposed && doc.Name == _fileName);

            if (activeOpen)
            {
                if (doc is null)
                {
                    try
                    {
                        // 设置命令标记: CommandFlags.Session
                        // 若没有设置: Open()之后的会进入中断状态(不会执行,直到切换文档ctrl+tab或者关闭文档)
                        doc = Acap.DocumentManager.Open(fileName, fileOpenMode == FileOpenMode.OpenForReadAndReadShare, password);
                    }
                    catch (Exception e)
                    {
                        throw new IOException($"错误:此文件打开错误:{fileName}\n错误信息:{e.Message}");
                    }
                }
                // 设置命令标记: CommandFlags.Session
                // 若没有设置: doc.IsActive 会异常
                if (!doc.IsActive)
                    Acap.DocumentManager.MdiActiveDocument = doc;

                // Open()是跨文档,所以必须要锁文档
                // 否则 Editor?.Redraw() 的 tm.QueueForGraphicsFlush() 将报错提示文档锁
                _documentLock = doc.LockDocument();

                Database = doc.Database;
                Document = doc;
                Editor = doc.Editor;
            }
            else
            {
                if (doc is null)
                {
                    Database = new Database(false, true);
                    if (Path.GetExtension(_fileName).ToLower().Contains("dxf"))
                    {
                        Database.DxfIn(_fileName, null);
                    }
                    else
                    {
                        Database.ReadDwgFile(_fileName, fileOpenMode, true, password);
                    }
                    Database.CloseInput(true);
                }
                else
                {
                    Database = doc.Database;
                    Document = doc;
                    Editor = doc.Editor;
                }
            }
        }

        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        _dBTrans.Push(this);
    }
    #endregion

    #region 类型转换
    /// <summary>
    /// 隐式转换为Transaction
    /// </summary>
    /// <param name="tr">事务管理器</param>
    /// <returns>事务管理器</returns>
    public static implicit operator Transaction(DBTrans tr)
    {
        return tr.Transaction;
    }
    #endregion

    #region 符号表

    /// <summary>
    /// 块表
    /// </summary>
    public SymbolTable<BlockTable, BlockTableRecord> BlockTable => _BlockTable ??= new(this, Database.BlockTableId);
    SymbolTable<BlockTable, BlockTableRecord>? _BlockTable;
    /// <summary>
    /// 当前绘图空间
    /// </summary>
    public BlockTableRecord CurrentSpace => BlockTable.GetRecord(Database.CurrentSpaceId)!;
    /// <summary>
    /// 模型空间
    /// </summary>
    public BlockTableRecord ModelSpace => BlockTable.GetRecord(BlockTable.CurrentSymbolTable[BlockTableRecord.ModelSpace])!;
    /// <summary>
    /// 图纸空间
    /// </summary>
    public BlockTableRecord PaperSpace => BlockTable.GetRecord(BlockTable.CurrentSymbolTable[BlockTableRecord.PaperSpace])!;
    /// <summary>
    /// 层表
    /// </summary>
    public SymbolTable<LayerTable, LayerTableRecord> LayerTable => _LayerTable ??= new(this, Database.LayerTableId);
    SymbolTable<LayerTable, LayerTableRecord>? _LayerTable;
    /// <summary>
    /// 文字样式表
    /// </summary>
    public SymbolTable<TextStyleTable, TextStyleTableRecord> TextStyleTable => _TextStyleTable ??= new(this, Database.TextStyleTableId);
    SymbolTable<TextStyleTable, TextStyleTableRecord>? _TextStyleTable;
    /// <summary>
    /// 注册应用程序表
    /// </summary>
    public SymbolTable<RegAppTable, RegAppTableRecord> RegAppTable => _RegAppTable ??= new(this, Database.RegAppTableId);
    SymbolTable<RegAppTable, RegAppTableRecord>? _RegAppTable;
    /// <summary>
    /// 标注样式表
    /// </summary>
    public SymbolTable<DimStyleTable, DimStyleTableRecord> DimStyleTable => _DimStyleTable ??= new(this, Database.DimStyleTableId);
    SymbolTable<DimStyleTable, DimStyleTableRecord>? _DimStyleTable;
    /// <summary>
    /// 线型表
    /// </summary>
    public SymbolTable<LinetypeTable, LinetypeTableRecord> LinetypeTable => _LinetypeTable ??= new(this, Database.LinetypeTableId);
    SymbolTable<LinetypeTable, LinetypeTableRecord>? _LinetypeTable;
    /// <summary>
    /// 用户坐标系表
    /// </summary>
    public SymbolTable<UcsTable, UcsTableRecord> UcsTable => _UcsTable ??= new(this, Database.UcsTableId);
    SymbolTable<UcsTable, UcsTableRecord>? _UcsTable;
    /// <summary>
    /// 视图表
    /// </summary>
    public SymbolTable<ViewTable, ViewTableRecord> ViewTable => _ViewTable ??= new(this, Database.ViewTableId);
    SymbolTable<ViewTable, ViewTableRecord>? _ViewTable;
    /// <summary>
    /// 视口表
    /// </summary>
    public SymbolTable<ViewportTable, ViewportTableRecord> ViewportTable => _ViewportTable ??= new(this, Database.ViewportTableId);
    SymbolTable<ViewportTable, ViewportTableRecord>? _ViewportTable;
    #endregion

    #region 字典
    /// <summary>
    /// 命名对象字典
    /// </summary>
    public DBDictionary NamedObjectsDict => GetObject<DBDictionary>(Database.NamedObjectsDictionaryId)!;
    /// <summary>
    /// 组字典
    /// </summary>
    public DBDictionary GroupDict => GetObject<DBDictionary>(Database.GroupDictionaryId)!;
    /// <summary>
    /// 多重引线样式字典
    /// </summary>
    public DBDictionary MLeaderStyleDict => GetObject<DBDictionary>(Database.MLeaderStyleDictionaryId)!;
    /// <summary>
    /// 多线样式字典
    /// </summary>
    public DBDictionary MLStyleDict => GetObject<DBDictionary>(Database.MLStyleDictionaryId)!;
    /// <summary>
    /// 材质字典
    /// </summary>
    public DBDictionary MaterialDict => GetObject<DBDictionary>(Database.MaterialDictionaryId)!;
    /// <summary>
    /// 表格样式字典
    /// </summary>
    public DBDictionary TableStyleDict => GetObject<DBDictionary>(Database.TableStyleDictionaryId)!;
    /// <summary>
    /// 视觉样式字典
    /// </summary>
    public DBDictionary VisualStyleDict => GetObject<DBDictionary>(Database.VisualStyleDictionaryId)!;
    /// <summary>
    /// 颜色字典
    /// </summary>
    public DBDictionary ColorDict => GetObject<DBDictionary>(Database.ColorDictionaryId)!;
    /// <summary>
    /// 打印设置字典
    /// </summary>
    public DBDictionary PlotSettingsDict => GetObject<DBDictionary>(Database.PlotSettingsDictionaryId)!;
    /// <summary>
    /// 打印样式表名字典
    /// </summary>
    public DBDictionary PlotStyleNameDict => GetObject<DBDictionary>(Database.PlotStyleNameDictionaryId)!;
    /// <summary>
    /// 布局字典
    /// </summary>
    public DBDictionary LayoutDict => GetObject<DBDictionary>(Database.LayoutDictionaryId)!;

#if !zcad // 中望官方的问题
    /// <summary>
    /// 数据链接字典
    /// </summary>
    public DBDictionary DataLinkDict => GetObject<DBDictionary>(Database.DataLinkDictionaryId)!;

#if !ac2009
    /// <summary>
    /// 详细视图样式字典
    /// </summary>
    public DBDictionary DetailViewStyleDict => GetObject<DBDictionary>(Database.DetailViewStyleDictionaryId)!;
    /// <summary>
    /// 剖面视图样式字典
    /// </summary>
    public DBDictionary SectionViewStyleDict => GetObject<DBDictionary>(Database.SectionViewStyleDictionaryId)!;
#endif
#endif
    #endregion

    #region 获取对象
    /// <summary>
    /// 根据对象id获取图元对象
    /// </summary>
    /// <typeparam name="T">要获取的图元对象的类型</typeparam>
    /// <param name="id">对象id</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>图元对象,类型不匹配时返回 <see langword="null"/> </returns>
    public T? GetObject<T>(ObjectId id,
                           OpenMode openMode = OpenMode.ForRead,
                           bool openErased = false,
                           bool openLockedLayer = false) where T : DBObject
    {
        return Transaction.GetObject(id, openMode, openErased, openLockedLayer) as T;
    }

    /// <summary>
    /// 根据对象句柄字符串获取对象Id
    /// </summary>
    /// <param name="handleString">句柄字符串</param>
    /// <returns>对象id</returns>
    public ObjectId GetObjectId(string handleString)
    {
        var hanle = new Handle(Convert.ToInt64(handleString, 16));
        // return Database.GetObjectId(false, hanle, 0);
        return DBTransHelper.TryGetObjectId(Database, hanle);
    }
    #endregion

    #region 保存文件
    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="version"></param>
    public void SaveDwgFile(DwgVersion version = DwgVersion.AC1800)
    {
        SaveFile(version);
    }

    /// <summary>
    /// 保存文件<br/>
    /// </summary>
    /// <param name="version">默认2004dwg;若保存dxf则需要在路径输入扩展名</param>
    /// <param name="automatic">为true时候<paramref name="version"/>无效,将变为自动识别环境变量</param>
    /// <param name="saveAsFile">另存为文件,前台将调用时它将无效,将变为弹出面板</param>
    /// <param name="echoes">保存路径失败的提示</param>
    public void SaveFile(DwgVersion version = DwgVersion.AC1800,
                         bool automatic = true,
                         string? saveAsFile = null,
                         bool echoes = true)
    {
        // 遍历当前所有文档,文档必然是前台的
        Document? doc = null;
        foreach (Document docItem in Acap.DocumentManager)
        {
            if (docItem.Database.Filename == this.Database.Filename)
            {
                doc = docItem;
                break;
            }
        }
        // 前台开图,使用命令保存;不需要切换文档
        if (doc != null)
        {
            if (saveAsFile == null)
                doc.SendStringToExecute("_qsave\n", false, true, true);
            else
                /// 无法把 <paramref name="saveAsFile"/>给这个面板
                doc.SendStringToExecute($"_Saveas\n", false, true, true);
            return;
        }

        // 后台开图,用数据库保存
        string? fileMsg;
        bool creatFlag = false;
        saveAsFile = saveAsFile?.Trim();
        if (string.IsNullOrEmpty(saveAsFile))
        {
            fileMsg = _fileName;
            saveAsFile = fileMsg;
            //creatFlag = true;
        }
        else
        {
            fileMsg = saveAsFile;

            // 路径失败也保存到桌面
            var path = Path.GetDirectoryName(saveAsFile);
            if (string.IsNullOrEmpty(path))
            {
                creatFlag = true;
            }
            else if (!Directory.Exists(path))
            {
                try { Directory.CreateDirectory(path); }
                catch { creatFlag = true; }
            }

            // 文件名缺失时
            if (!creatFlag &&
                string.IsNullOrEmpty(Path.GetFileName(saveAsFile).Trim()))
                creatFlag = true;
        }
        if (saveAsFile != null)
        {
            var fileNameWith = Path.GetFileNameWithoutExtension(saveAsFile).Trim();
            if (string.IsNullOrEmpty(fileNameWith))
                creatFlag = true;
        }
        else
        {
            creatFlag = true;
        }

        if (creatFlag)
        {
            var (error, file) = GetOrCreateSaveAsFile();
            if (echoes && error)
                MessageBox.Show($"错误参数:\n{fileMsg}\n\n它将保存:\n{file}", "错误的文件路径");
            saveAsFile = file;
        }

        if (Path.GetExtension(saveAsFile).ToLower().Contains("dxf"))
        {
            // dxf用任何版本号都会报错
#if acad || gcad
            Database.DxfOut(saveAsFile, 7, true);
#endif

#if zcad  // 中望这里没有测试
            Database.DxfOut(saveAsFile, 7, version, true);
#endif
            return;
        }

        if (automatic)
            version = Env.GetDefaultDwgVersion();

        // dwg需要版本号,而dxf不用,dwg用dxf版本号会报错
        // 若扩展名和版本号冲突,按照扩展名为准
        if (version.IsDxfVersion())
            version = DwgVersion.Current;

        Database.SaveAs(saveAsFile, version);
    }


    /// <summary>
    /// 获取文件名,无效的话就制造
    /// </summary>
    /// <returns></returns>
    (bool error, string path) GetOrCreateSaveAsFile()
    {
        var file = Database.Filename;
        if (!string.IsNullOrEmpty(file))
            return (false, file);

        // 为了防止用户输入了错误的路径造成无法保存,
        // 所以此处将进行保存到桌面,
        // 而不是弹出警告就结束
        // 防止前台关闭了所有文档导致没有Editor,所以使用 MessageBox 发送警告
        var fileName = Path.GetFileNameWithoutExtension(_fileName).Trim();
        var fileExt = Path.GetExtension(_fileName);

        if (string.IsNullOrEmpty(fileName))
            fileName = DateTime.Now.ToString("--yyMMdd--hhmmssffff");
        if (string.IsNullOrEmpty(fileExt))
            fileExt = ".dwg";

        // 构造函数(fileName)用了不存在的路径进行后台打开,就会出现此问题
        // 测试命令 FileNotExist
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                + "\\后台保存出错的文件\\";

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        file = dir + fileName + fileExt;
        while (File.Exists(file))
        {
            var time = DateTime.Now.ToString("--yyMMdd--hhmmssffff");
            file = dir + fileName + time + fileExt;
            Thread.Sleep(100);
        }
        return (true, file);
    }

    #endregion

    #region 前台后台任务
    /// <summary>
    /// 前台后台任务分别处理
    /// </summary>
    /// <remarks>
    /// 备注:<br/>
    /// 0x01 文字偏移问题主要出现线性引擎函数<see cref="Database.ResolveXrefs"/>上面,<br/>
    ///      在 参照绑定/深度克隆 的底层共用此函数导致<br/>
    /// 0x02 后台是利用前台当前数据库进行处理的<br/>
    /// 0x03 跨进程通讯暂无测试(可能存在bug)<br/>
    /// </remarks>
    /// <param name="action">委托</param>
    /// <param name="handlingDBTextDeviation">开启单行文字偏移处理</param>
    public void Task(Action action, bool handlingDBTextDeviation = true)
    {
        //if (action == null)
        //    throw new ArgumentNullException(nameof(action));
        action.NotNull(nameof(action));
        // 前台开图 || 后台直接处理
        if (Document != null || !handlingDBTextDeviation)
        {
            action.Invoke();
            return;
        }

        // 后台
        // 这种情况发生在关闭了所有文档之后,进行跨进程通讯
        // 此处要先获取激活的文档,不能直接获取当前数据库否则异常
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        if (doc == null)
        {
            action.Invoke();
            return;
        }
        // 处理单行文字偏移
        // 前台绑定参照的时候不能用它,否则抛出异常:eWasErased
        // 所以本函数自动识别前后台做处理
        var dbBak = doc.Database;
        HostApplicationServices.WorkingDatabase = Database;
        action.Invoke();
        HostApplicationServices.WorkingDatabase = dbBak;
    }
    #endregion

    #region IDisposable接口相关函数
    /// <summary>
    /// 取消事务
    /// </summary>
    public void Abort()
    {
        _commit = false;
        Dispose();
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit()
    {
        _commit = true;
        Dispose();
    }

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
    ~DBTrans()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        /* 事务dispose流程：
         * 1. 根据传入的参数确定是否提交,true为提交,false为不提交
         * 2. 如果锁文档就将文档锁dispose
         * 3. 不管是否提交,既然进入dispose,就要将事务栈的当前事务弹出
         *    注意这里的事务栈不是cad的事务管理器,而是dbtrans的事务
         * 4. 清理非托管的字段
         */

        // 不重复释放,并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;

        // 致命错误时候此处是空,直接跳过
        if (Transaction != null)
        {
            if (_commit)
            {
                // 刷新队列(后台不刷新)
                Editor?.Redraw();
                // 调用cad的事务进行提交,释放托管状态(托管对象)
                Transaction.Commit();
            }
            else
            {
                // 否则取消所有的修改
                Transaction.Abort();
            }

            // 将cad事务进行销毁
            if (!Transaction.IsDisposed)
                Transaction.Dispose();
        }
        // 将文档锁销毁
        _documentLock?.Dispose();

        // 将当前事务栈弹栈
        _dBTrans.Pop();
    }
    #endregion

    #region ToString
    public override string ToString()
    {
        return ToString(null, null);
    }
    public string ToString(IFormatProvider? provider)
    {
        return ToString(null, provider);
    }
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        List<string> lines = new();
        lines.Add($"StackCount = {_dBTrans.Count}");
        lines.Add($"_fileName = \"{_fileName}\"");
        lines.Add($"_commit = {_commit}");
        lines.Add($"_documentLock = {_documentLock != null}");

        lines.Add($"Document = {Document != null}");
        lines.Add($"Editor = {Editor != null}");
        lines.Add($"Transaction = {Transaction != null}");
        lines.Add($"Database = {Database != null}");

        if (!string.IsNullOrEmpty(format))
            return string.Join(format, lines.ToArray());

        return string.Join("\n", lines.ToArray());
    }
    #endregion
}