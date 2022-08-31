namespace IFoxCAD.Cad;

using System.Diagnostics;
using System.IO;

/// <summary>
/// 事务栈
/// <para>隐匿事务在数据库其中担任的角色</para>
/// </summary>
public class DBTrans : IDisposable
{
    #region 私有字段
    /// <summary>
    /// 文档锁
    /// </summary>
    private readonly DocumentLock? documentLock;
    /// <summary>
    /// 是否释放资源
    /// </summary>
    private bool IsDisposed;
    /// <summary>
    /// 是否提交事务
    /// </summary>
    private readonly bool _commit;
    /// <summary>
    /// 事务栈
    /// </summary>
    private static readonly Stack<DBTrans> dBTrans = new();
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
             * 然后命令文中发生了 using var tr = new DBTrans();
             * 当退出命令此事务释放,但是从来不释放Top,
             * 然后我新建了一个文档,再进行命令=>又进入Top,Top返回了前一个文档的事务
             * 因此所以无法清理栈,所以Dispose不触发,导致无法刷新图元和Ctrl+Z出错
             * 所以用AOP方式修复
             * 
             * 0x03
             * 经过艰苦卓绝的测试,aop模式由于不能断点调试,所以暂时放弃。
             */

            // 由于大量的函数依赖本属性,强迫用户先开启事务
            if (dBTrans.Count == 0)
                throw new ArgumentNullException("事务栈没有任何事务,请在调用前创建:" + nameof(DBTrans));
            var trans = dBTrans.Peek();
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
            documentLock = Document.LockDocument();

        dBTrans.Push(this);
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
        dBTrans.Push(this);
    }

    /// <summary>
    /// 事务栈
    /// <para>打开文件,默认提交事务</para>
    /// </summary>
    /// <param name="fileName">要打开的文件名</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="openMode">开图模式</param>
    /// <param name="password">密码</param>
    public DBTrans(string fileName,
                   bool commit = true,
                   FileOpenMode openMode = FileOpenMode.OpenForReadAndWriteNoShare,
                   string? password = null)
    {
        if (fileName == null || string.IsNullOrEmpty(fileName.Trim()))
            throw new ArgumentNullException(nameof(fileName));

        fileName = fileName.Replace("/", "\\");//// doc.Name总是"D:\\JX.dwg"

        if (!File.Exists(fileName))
            Database = new Database(true, false);
        else
        {
            var doc = Acap.DocumentManager
                     .Cast<Document>()
                     .FirstOrDefault(doc => doc.Name == fileName);
            if (doc is not null)
            {
                Database = doc.Database;
                Document = doc;
                Editor = doc.Editor;
            }
            else
            {
                Database = new Database(false, true);
                if (Path.GetExtension(fileName).ToLower().Contains("dxf"))
                    Database.DxfIn(fileName, null);
                else
                {
#if ac2008
                    // FileAccess fileAccess = FileAccess.Read;
                    FileShare fileShare = FileShare.Read;
                    switch (openMode)
                    {
                        case FileOpenMode.OpenTryForReadShare:// 这个是什么状态??
                        // fileAccess = FileAccess.ReadWrite;
                        fileShare = FileShare.ReadWrite;
                        break;
                        case FileOpenMode.OpenForReadAndAllShare:// 完美匹配
                        // fileAccess = FileAccess.ReadWrite;
                        fileShare = FileShare.ReadWrite;
                        break;
                        case FileOpenMode.OpenForReadAndWriteNoShare:// 完美匹配
                        // fileAccess = FileAccess.ReadWrite;
                        fileShare = FileShare.None;
                        break;
                        case FileOpenMode.OpenForReadAndReadShare:// 完美匹配
                        // fileAccess = FileAccess.Read;
                        fileShare = FileShare.Read;
                        break;
                        default:
                        break;
                    }

                    // 这个会致命错误
                    // using FileStream fileStream = new(fileName, FileMode.Open, fileAccess, fileShare);
                    // Database.ReadDwgFile(fileStream.SafeFileHandle.DangerousGetHandle(),
                    //      true/*控制读入一个与系统编码不相同的文件时的转换操作*/,password);

                    Database.ReadDwgFile(fileName, fileShare,
                            true/*控制读入一个与系统编码不相同的文件时的转换操作*/, password);
#else
                    Database.ReadDwgFile(fileName, openMode,
                            true/*控制读入一个与系统编码不相同的文件时的转换操作*/, password);
#endif
                }
                Database.CloseInput(true);
            }
        }

        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        dBTrans.Push(this);
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
    #endregion

    #region 获取对象
    /// <summary>
    /// 根据对象id获取图元对象
    /// </summary>
    /// <typeparam name="T">要获取的图元对象的类型</typeparam>
    /// <param name="id">对象id</param>
    /// <param name="mode">打开模式,默认为只读</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="forceOpenOnLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>图元对象,类型不匹配时返回 <see langword="null"/> </returns>
    public T? GetObject<T>(ObjectId id,
                          OpenMode mode = OpenMode.ForRead,
                          bool openErased = false,
                          bool forceOpenOnLockedLayer = false) where T : DBObject
    {
        return Transaction.GetObject(id, mode, openErased, forceOpenOnLockedLayer) as T;
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
    /// 保存当前数据库的dwg文件,如果前台打开则按dwg默认版本保存,否则按version参数的版本保存
    /// </summary>
    /// <param name="version">dwg版本,默认为2004</param>
    public void SaveDwgFile(DwgVersion version = DwgVersion.AC1800)
    {
        Document? doca = null;
        foreach (Document doc in Acap.DocumentManager)
        {
            if (doc.Database.Filename == this.Database.Filename)
            {
                doca = doc;
                break;
            }
        }
        if (doca == null) 
        {
            // 后台开图,用数据库保存
            if (!string.IsNullOrEmpty(Database.Filename))
            {
                Database.SaveAs(Database.Filename, version);
                return;
            }

            /// 构造函数(fileName)用了不存在的路径进行后台打开,就会出现此问题 
            /// 测试命令 FileNotExist
            Debug.WriteLine("**** 此文件路径不存在,无法保存!将自动保存到桌面中.");
            string dir = Environment.GetFolderPath(
                         Environment.SpecialFolder.DesktopDirectory) + "\\路径不存在进行临时保存\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string file = DateTime.Now.ToString("yyyy-MM-dd--h-mm-ss-ffff");
            Database.SaveAs(dir + file + ".dwg", version);
        }
        else // 前台开图,使用命令保存;不需要切换文档
            doca.SendStringToExecute("_qsave\n", false, true, true);
    }
    #endregion

    #region 前台后台任务
    /// <summary>
    /// 前台后台任务分别处理
    /// </summary>
    /// <remarks>
    /// 备注:<br/>
    /// <code>
    /// 0x01 文字偏移问题主要出现是<see cref="Database.ResolveXrefs"/>这个线性引擎上面,
    ///      在 参照绑定/深度克隆 的底层共用此技术导致问题发生
    /// 0x02 后台是利用前台当前数据库进行处理的
    /// 0x03 跨进程通讯暂无测试(可能存在bug)
    /// </code>
    /// </remarks>
    /// <param name="action">委托</param>
    /// <param name="handlingDBTextDeviation">开启单行文字偏移处理</param>
    public void Task(Action action, bool handlingDBTextDeviation = true)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // 前台开图 || 后台直接处理
        if (Document != null || !handlingDBTextDeviation)
        {
            action.Invoke();
            return;
        }

        // 后台
        // 这种情况发生在关闭了所有文档之后,进行跨进程通讯
        var dbBak = HostApplicationServices.WorkingDatabase;
        if (dbBak == null)
        {
            action.Invoke();
            return;
        }

        // 处理单行文字偏移
        // 前台绑定参照的时候不能用它,否则出现: <see langword="eWasErased"/><br/>
        // 所以本函数自动识别前后台做处理
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
        Dispose(false);
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit()
    {
        Dispose(true);
    }

    /// <summary>
    /// 手动调用释放
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数调用释放
    /// </summary>
    ~DBTrans()
    {
        Dispose(disposing: false);
    }

    protected virtual void Dispose(bool disposing)
    {
        /* 事务dispose流程：
         * 1. 根据传入的参数确定是否提交,true为提交,false为不提交
         * 2. 根据IsDisposed的值确定是否重复dispose,false为首次dispose
         * 3. 如果锁文档就将文档锁dispose
         * 4. 不管是否提交,既然进入dispose,就要将事务栈的当前事务弹出
         *    注意这里的事务栈不是cad的事务管理器,而是dbtrans的事务
         * 5. 清理非托管的字段
         */

        // 不重复释放,并设置已经释放
        if (IsDisposed) return;
        IsDisposed = true;


        if (disposing)
        {
            // 调用cad的事务进行提交,释放托管状态(托管对象)
            Transaction.Commit();
        }
        else
        {
            // 否则取消所有的修改
            Transaction.Abort();
        }

        // 调用cad事务的dispose进行销毁
        if (!Transaction.IsDisposed)
            Transaction.Dispose();

        // 调用文档锁dispose
        documentLock?.Dispose();

        // 将事务栈的当前dbtrans弹栈
        dBTrans.Pop();
    }
    #endregion
}
