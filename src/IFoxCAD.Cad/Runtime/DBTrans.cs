namespace IFoxCAD.Cad;

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
    private bool disposedValue;
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
            DBTrans trans;
            try
            {
                trans = dBTrans.Peek();
            }
            catch (System.Exception)
            {
                trans = new DBTrans();
            }
            return trans;
        }
    }

    /// <summary>
    /// 数据库
    /// </summary>
    public Database Database { get; private set; }
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

    #endregion

    #region 构造函数
    /// <summary>
    /// 默认构造函数，默认为打开当前文档，默认提交事务
    /// </summary>
    /// <param name="doc">要打开的文档</param>
    /// <param name="commit">事务是否提交</param>
    /// <param name="doclock">是否锁文档</param>
    public DBTrans(Document? doc = null, bool commit = true, bool doclock = false)
    {
        Document = doc ?? Application.DocumentManager.MdiActiveDocument;
        Database = Document.Database;
        Editor = Document.Editor;
        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        if (doclock)
        {
            documentLock = Document.LockDocument();
        }
        dBTrans.Push(this);
    }

    /// <summary>
    /// 构造函数，打开数据库，默认提交事务
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
    /// 构造函数，打开文件，默认提交事务
    /// </summary>
    /// <param name="fileName">要打开的文件名</param>
    /// <param name="commit">事务是否提交</param>
    public DBTrans(string fileName, bool commit = true)
    {
        Database = CreateDatabase(fileName);
        Transaction = Database.TransactionManager.StartTransaction();
        _commit = commit;
        dBTrans.Push(this);
    }

    private static Database CreateDatabase(string fileName)
    {
        var db = new Database(false, true);
        if (Path.GetExtension(fileName).ToLower().Contains("dxf"))
        {
            db.DxfIn(fileName, null);
        }
        else
        {
            db.ReadDwgFile(fileName, FileShare.Read, true, null);
        }
        db.CloseInput(true);
        return db;
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
    public SymbolTable<BlockTable, BlockTableRecord> BlockTable => new(this, Database.BlockTableId);
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
    public SymbolTable<LayerTable, LayerTableRecord> LayerTable => new(this, Database.LayerTableId);
    /// <summary>
    /// 文字样式表
    /// </summary>
    public SymbolTable<TextStyleTable, TextStyleTableRecord> TextStyleTable => new(this, Database.TextStyleTableId);

    /// <summary>
    /// 注册应用程序表
    /// </summary>
    public SymbolTable<RegAppTable, RegAppTableRecord> RegAppTable => new(this, Database.RegAppTableId);

    /// <summary>
    /// 标注样式表
    /// </summary>
    public SymbolTable<DimStyleTable, DimStyleTableRecord> DimStyleTable => new(this, Database.DimStyleTableId);

    /// <summary>
    /// 线型表
    /// </summary>
    public SymbolTable<LinetypeTable, LinetypeTableRecord> LinetypeTable => new(this, Database.LinetypeTableId);

    /// <summary>
    /// 用户坐标系表
    /// </summary>
    public SymbolTable<UcsTable, UcsTableRecord> UcsTable => new(this, Database.UcsTableId);

    /// <summary>
    /// 视图表
    /// </summary>
    public SymbolTable<ViewTable, ViewTableRecord> ViewTable => new(this, Database.ViewTableId);

    /// <summary>
    /// 视口表
    /// </summary>
    public SymbolTable<ViewportTable, ViewportTableRecord> ViewportTable => new(this, Database.ViewportTableId);
    #endregion

    #region 字典
    //TODO: 补充关于扩展字典，命名对象字典，组字典，多线样式字典等对象字典的属性
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
    /// <summary>
    /// 详细视图样式字典
    /// </summary>
    public DBDictionary DetailViewStyleDict => GetObject<DBDictionary>(Database.DetailViewStyleDictionaryId)!;
    /// <summary>
    /// 剖面视图样式字典
    /// </summary>
    public DBDictionary SectionViewStyleDict => GetObject<DBDictionary>(Database.SectionViewStyleDictionaryId)!;

    #endregion

    #region 获取对象
    /// <summary>
    /// 根据对象id获取图元对象
    /// </summary>
    /// <typeparam name="T">要获取的图元对象的类型</typeparam>
    /// <param name="id">对象id</param>
    /// <param name="mode">打开模式，默认为只读</param>
    /// <param name="openErased">是否打开已删除对象，默认为不打开</param>
    /// <param name="forceOpenOnLockedLayer">是否打开锁定图层对象，默认为不打开</param>
    /// <returns>图元对象，类型不匹配时返回 <see langword="null"/> </returns>
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
        return Database.GetObjectId(false, hanle, 0);
    }



    #endregion

    #region idispose接口相关函数

    public void Abort()
    {
        Transaction.Abort();
        Dispose(false);
    }

    public void Commit()
    {
        if (_commit)
        {
            Transaction.Commit();
        }
        else
        {
            Abort();
        }

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                Commit();
                dBTrans.Pop();
                if (!Transaction.IsDisposed)
                {
                    if (Document?.IsActive==true)
                        Transaction.TransactionManager.QueueForGraphicsFlush();
                    Transaction.Dispose();
                }
                documentLock?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并替代终结器
            // 将大型字段设置为 null
            disposedValue = true;
        }
        //Transaction.TransactionManager.QueueForGraphicsFlush();
    }

    // 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    ~DBTrans()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
