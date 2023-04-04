namespace IFoxCAD.Cad;
/// <summary>
/// 符号表管理类
/// </summary>
/// <typeparam name="TTable">符号表</typeparam>
/// <typeparam name="TRecord">符号表记录</typeparam>
public class SymbolTable<TTable, TRecord> : IEnumerable<ObjectId>
    where TTable : SymbolTable
    where TRecord : SymbolTableRecord, new()
{
    #region 程序集内部属性
    /// <summary>
    /// 事务管理器
    /// </summary>
    internal DBTrans DTrans { get; private set; }
    /// <summary>
    /// 数据库
    /// </summary>
    internal Database Database { get; private set; }

    #endregion

    #region 公开属性
    /// <summary>
    /// 当前符号表
    /// </summary>
    public TTable CurrentSymbolTable { get; private set; }
    #endregion

    #region 构造函数
    /// <summary>
    /// 构造函数，初始化Trans和CurrentSymbolTable属性
    /// </summary>
    /// <param name="tr">事务管理器</param>
    /// <param name="tableId">符号表id</param>
    /// <param name="defaultBehavior">默认行为:例如打开隐藏图层</param>
    internal SymbolTable(DBTrans tr, ObjectId tableId, bool defaultBehavior = true)
    {
        DTrans = tr;
        Database = tr.Database;
        CurrentSymbolTable = DTrans.GetObject<TTable>(tableId)!;

        if (!defaultBehavior)
            return;

        if (CurrentSymbolTable is LayerTable layer)
        {
            // 层表包含隐藏的,全部显示出来
            layer = layer.IncludingHidden;
            if (layer is TTable tt)
                CurrentSymbolTable = tt;
        }
    }

    #endregion

    #region 索引器
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="key">对象名称</param>
    /// <returns>对象的id</returns>
    public ObjectId this[string key]
    {
        get
        {
            if (Has(key))
                return CurrentSymbolTable[key];
            return ObjectId.Null;
        }
    }
    #endregion

    #region Has
    /// <summary>
    /// 判断是否存在符号表记录
    /// </summary>
    /// <param name="key">记录名</param>
    /// <returns>存在返回 <see langword="true"/>, 不存在返回 <see langword="false"/></returns>
    public bool Has(string key)
    {
        return CurrentSymbolTable.Has(key);
    }
    /// <summary>
    /// 判断是否存在符号表记录
    /// </summary>
    /// <param name="objectId">记录id</param>
    /// <returns>存在返回 <see langword="true"/>, 不存在返回 <see langword="false"/></returns>
    public bool Has(ObjectId objectId)
    {
        return CurrentSymbolTable.Has(objectId);
    }
    #endregion

    #region 添加符号表记录
    /// <summary>
    /// 添加符号表记录
    /// </summary>
    /// <param name="record">符号表记录</param>
    /// <returns>对象id</returns>
    private ObjectId Add(TRecord record)
    {
        ObjectId id;
        using (CurrentSymbolTable.ForWrite())
        {
            id = CurrentSymbolTable.Add(record);
            DTrans.Transaction.AddNewlyCreatedDBObject(record, true);
        }
        return id;
    }
    /// <summary>
    /// 添加符号表记录
    /// </summary>
    /// <param name="name">符号表记录名</param>
    /// <param name="action">符号表记录处理函数的无返回值委托</param>
    /// <returns>对象id</returns>
    public ObjectId Add(string name, Action<TRecord>? action = null)
    {
        ObjectId id = this[name];
        if (id.IsNull)
        {
            var record = new TRecord()
            {
                Name = name
            };
            id = Add(record);
            using (record.ForWrite())
                action?.Invoke(record);
        }
        return id;
    }
    #endregion

    #region 删除符号表记录
    /// <summary>
    /// 删除符号表记录
    /// </summary>
    /// <param name="record">符号表记录对象</param>
    private static void Remove(TRecord record)
    {
        using (record.ForWrite())
            record.Erase();
    }

    /// <summary>
    /// 删除符号表记录
    /// </summary>
    /// <param name="name">符号表记录名</param>
    public void Remove(string name)
    {
        var record = GetRecord(name);
        if (record is not null)
            Remove(record);
    }

    /// <summary>
    /// 删除符号表记录
    /// </summary>
    /// <param name="id">符号表记录对象id</param>
    public void Remove(ObjectId id)
    {
        var record = GetRecord(id);
        if (record is not null)
            Remove(record);
    }
    #endregion

    #region 修改符号表记录
    /// <summary>
    /// 修改符号表
    /// </summary>
    /// <param name="name">符号表记录名</param>
    /// <param name="action">修改委托</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void Change(string name, Action<TRecord> action)
    {
        var record = GetRecord(name);
        if (record is not null)
        {
            using (record.ForWrite())
                action.Invoke(record);
        }
    }

    /// <summary>
    /// 修改符号表
    /// </summary>
    /// <param name="id">符号表记录id</param>
    /// <param name="action">修改委托</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void Change(ObjectId id, Action<TRecord> action)
    {
        var record = GetRecord(id);
        if (record is not null)
        {
            using (record.ForWrite())
                action.Invoke(record);
        }
    }
    #endregion

    #region 获取符号表记录
    /// <summary>
    /// 获取符号表记录
    /// </summary>
    /// <param name="id">符号表记录的id</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>符号表记录</returns>
    [System.Diagnostics.DebuggerStepThrough]
    public TRecord? GetRecord(ObjectId id,
                              OpenMode openMode = OpenMode.ForRead,
                              bool openErased = false,
                              bool openLockedLayer = false)
    {
        return DTrans.GetObject<TRecord>(id, openMode, openErased, openLockedLayer);
    }

    /// <summary>
    /// 获取符号表记录
    /// </summary>
    /// <param name="name">符号表记录名</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>符号表记录</returns>
    [System.Diagnostics.DebuggerStepThrough]
    public TRecord? GetRecord(string name,
                              OpenMode openMode = OpenMode.ForRead,
                              bool openErased = false,
                              bool openLockedLayer = false)
    {
        return GetRecord(this[name], openMode, openErased, openLockedLayer);
    }

    /// <summary>
    /// 获取符号表记录
    /// </summary>
    /// <returns>符号表记录集合</returns>
    public IEnumerable<TRecord> GetRecords()
    {
        foreach (var item in this)
        {
            var record = GetRecord(item);
            if (record is not null)
                yield return record;
        }
    }

    /// <summary>
    /// 获取符号表记录的名字集合
    /// </summary>
    /// <returns>记录的名字集合</returns>
    public IEnumerable<string> GetRecordNames() => GetRecords().Select(record => record.Name);

    /// <summary>
    /// 获取符合过滤条件的符号表记录名字集合
    /// </summary>
    /// <param name="filter">过滤器委托</param>
    /// <returns>记录的名字集合</returns>
    public IEnumerable<string> GetRecordNames(Func<TRecord, bool> filter)
    {
        foreach (var item in this)
        {
            var record = GetRecord(item);
            if (record is not null && filter.Invoke(record))
                yield return record.Name;
        }
    }

    /// <summary>
    /// 从源数据库拷贝符号表记录
    /// </summary>
    /// <param name="table">符号表</param>
    /// <param name="name">符号表记录名</param>
    /// <param name="over">是否覆盖，<see langword="true"/> 为覆盖，<see langword="false"/> 为不覆盖</param>
    /// <returns>对象id</returns>
    public ObjectId GetRecordFrom(SymbolTable<TTable, TRecord> table, string name, bool over)
    {
        //if (table is null)
        //    throw new ArgumentNullException(nameof(table), "对象为null");
        table.NotNull(nameof(table));

        ObjectId rid = this[name];
        bool has = rid != ObjectId.Null;
        if ((has && over) || !has)
        {
            ObjectId id = table[name];
            using IdMapping map = new();
            using ObjectIdCollection ids = new() { id };
            table.Database.WblockCloneObjects(
                           ids,
                           CurrentSymbolTable.Id,
                           map,
                           DuplicateRecordCloning.Replace,
                           false);
            rid = map[id].Value;
        }
        return rid;
    }

    /// <summary>
    /// 从文件拷贝符号表记录
    /// </summary>
    /// <param name="tableSelector">符号表过滤器</param>
    /// <param name="fileName">文件名</param>
    /// <param name="name">符号表记录名</param>
    /// <param name="over">是否覆盖，<see langword="true"/> 为覆盖，<see langword="false"/> 为不覆盖</param>
    /// <returns>对象id</returns>
    internal ObjectId GetRecordFrom(Func<DBTrans, SymbolTable<TTable, TRecord>> tableSelector,
                                    string fileName,
                                    string name,
                                    bool over)
    {
        using DBTrans tr = new(fileName);
        return GetRecordFrom(tableSelector(tr), name, over);
    }
    #endregion

    #region 遍历
#line hidden // 调试的时候跳过它
    /// <summary>
    /// 遍历符号表,执行委托
    /// </summary>
    /// <param name="task">要运行的委托</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="checkIdOk">检查id是否删除,默认true</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    public void ForEach(Action<TRecord> task,
                        OpenMode openMode = OpenMode.ForRead,
                        bool checkIdOk = true,
                        bool openErased = false,
                        bool openLockedLayer = false)
    {
        ForEach((a, _, _) => {
            task.Invoke(a);//由于此处是委托,所以 DebuggerStepThrough 特性会进入,改用预处理方式避免
        }, openMode, checkIdOk, openErased, openLockedLayer);
    }

    /// <summary>
    /// 遍历符号表,执行委托(允许循环中断)
    /// </summary>
    /// <param name="task">要执行的委托</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="checkIdOk">检查id是否删除,默认true</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    public void ForEach(Action<TRecord, LoopState> task,
                        OpenMode openMode = OpenMode.ForRead,
                        bool checkIdOk = true,
                        bool openErased = false,
                        bool openLockedLayer = false)
    {
        ForEach((a, b, _) => {
            task.Invoke(a, b);
        }, openMode, checkIdOk, openErased, openLockedLayer);
    }

    /// <summary>
    /// 遍历符号表,执行委托(允许循环中断,输出索引值)
    /// </summary>
    /// <param name="task">要执行的委托</param>
    /// <param name="openMode">打开模式,默认为只读</param>
    /// <param name="checkIdOk">检查id是否删除,默认true</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void ForEach(Action<TRecord, LoopState, int> task,
                        OpenMode openMode = OpenMode.ForRead,
                        bool checkIdOk = true,
                        bool openErased = false,
                        bool openLockedLayer = false)
    {
        //if (task == null)
        //    throw new ArgumentNullException(nameof(task));
        task.NotNull(nameof(task));
        LoopState state = new();/*这种方式比Action改Func更友好*/
        int i = 0;
        foreach (var id in this)
        {
            if (checkIdOk && !id.IsOk())
                continue;
            var record = GetRecord(id, openMode, openErased, openLockedLayer);
            if (record is not null)
                task.Invoke(record, state, i);
            if (!state.IsRun)
                break;
            i++;
        }
    }
#line default

    #endregion

    #region IEnumerable<ObjectId> 成员
    [System.Diagnostics.DebuggerStepThrough]
    public IEnumerator<ObjectId> GetEnumerator()
    {
        foreach (var id in CurrentSymbolTable)
            yield return id;
    }

    [System.Diagnostics.DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}