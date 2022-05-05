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
             * 经过艰苦卓绝的测试，aop模式由于不能断点调试，所以暂时放弃。
             */

            // 由于大量的函数依赖本属性，强迫用户先开启事务
            if (dBTrans.Count == 0)
                throw new ArgumentNullException("事务栈没有任何事务,请在调用前创建:" + nameof(DBTrans));
            var trans = dBTrans.Peek();
            return trans;
        }
    }

    /// <summary>
    /// 结束栈中所有事务_AOP修复方案
    /// 此方式代表了不允许跨事务循环命令
    /// 若有则需在此命令进行 拒绝注入AOP特性
    /// </summary>
    //internal static void FinishDatabase()
    //{
    //    while (dBTrans.Count != 0)
    //        dBTrans.Peek().Dispose();
    //}

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
            documentLock = Document.LockDocument();

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
#if ac2009
        if (string.IsNullOrEmpty(fileName.Trim()))
            throw new ArgumentNullException(nameof(fileName));
#else

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException(nameof(fileName));
#endif
        if (File.Exists(fileName))
        {
            var doc = Application.DocumentManager
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
                    Database.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndWriteNoShare, true, null);
                Database.CloseInput(true);
            }
        }
        else
        {
            Database = new Database(true, false);
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
        //return Database.GetObjectId(false, hanle, 0);
        return Helper.TryGetObjectId(Database, hanle);
    }

    class Helper
    {
        /*
         * id = db.GetObjectId(false, handle, 0);
         * 参数意义: db.GetObjectId(如果没有找到就创建,句柄号,标记..将来备用)
         * 在vs的输出会一直抛出:
         * 引发的异常:“Autodesk.AutoCAD.Runtime.Exception”(位于 AcdbMgd.dll 中)
         * "eUnknownHandle"
         * 这就是为什么慢的原因,所以直接运行就好了!而Debug还是需要用arx的API替代.
         */

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acdb17.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
           EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QAE?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@_NABVAcDbHandle@@K@Z")]
        extern static int getAcDbObjectId17x32(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acdb17.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
          EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QEAA?AW4ErrorStatus@Acad@@AEAVAcDbObjectId@@_NAEBVAcDbHandle@@K@Z")]
        extern static int getAcDbObjectId17x64(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
           EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QAE?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@_NABVAcDbHandle@@K@Z")]
        extern static int getAcDbObjectId18x32(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
          EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QEAA?AW4ErrorStatus@Acad@@AEAVAcDbObjectId@@_NAEBVAcDbHandle@@K@Z")]
        extern static int getAcDbObjectId18x64(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

        /// <summary>
        /// 句柄转id,NET35(08~12)专用的
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="handle">句柄</param>
        /// <param name="id">返回的id</param>
        /// <param name="createIfNotFound">不存在则创建</param>
        /// <param name="reserved">保留,用于未来</param>
        /// <returns>成功0,其他值都是错误.可以强转ErrorStatus</returns>
        static int GetAcDbObjectId(IntPtr db, Handle handle, out ObjectId id, bool createIfNotFound = false, uint reserved = 0)
        {
            id = ObjectId.Null;
            switch (Application.Version.Major)
            {
                case 17:
                    {
                        if (IntPtr.Size == 4)
                            return getAcDbObjectId17x32(db, out id, createIfNotFound, ref handle, reserved);
                        else
                            return getAcDbObjectId17x64(db, out id, createIfNotFound, ref handle, reserved);
                    }
                case 18:
                    {
                        if (IntPtr.Size == 4)
                            return getAcDbObjectId18x32(db, out id, createIfNotFound, ref handle, reserved);
                        else
                            return getAcDbObjectId18x64(db, out id, createIfNotFound, ref handle, reserved);
                    }
            }
            return -1;
        }

        /// <summary>
        /// 句柄转id
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="handle">句柄</param>
        /// <returns>id</returns>
        public static ObjectId TryGetObjectId(Database db, Handle handle)
        {
#if !NET35
            //高版本直接利用
            var es = db.TryGetObjectId(handle, out ObjectId id);
            //if (!es)
#else
            var es = GetAcDbObjectId(db.UnmanagedObject, handle, out ObjectId id);
            //if (ErrorStatus.OK != (ErrorStatus)es)
#endif
            return id;
        }
    }

    #endregion

    #region idispose接口相关函数
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

    protected virtual void Dispose(bool disposing)
    {
        /* 事务dispose流程：
         * 1. 根据传入的参数确定是否提交，true为提交，false为不提交
         * 2. 根据disposedValue的值确定是否重复dispose，false为首次dispose
         * 3. 如果锁文档就将文档锁dispose
         * 4. 不管是否提交，既然进入dispose，就要将事务栈的当前事务弹出
         *    注意这里的事务栈不是cad的事务管理器，而是dbtrans的事务
         * 5. 清理非托管的字段
         */

        if (disposedValue)
            return;

        // 释放未托管的资源(未托管的对象)并替代终结器
        // 将大型字段设置为 null
        disposedValue = true;

        if (disposing)
        {
            // 释放托管状态(托管对象)
            // 调用cad的事务进行提交
            Transaction.Commit();
        }
        else
        {
            // 否则取消所有的修改
            Transaction.Abort();
        }
        // 调用 cad事务的dispose进行销毁
        if (!Transaction.IsDisposed)
            Transaction.Dispose();

        // 调用文档锁dispose
        documentLock?.Dispose();

        // 将事务栈的当前dbtrans弹栈
        dBTrans.Pop();
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
