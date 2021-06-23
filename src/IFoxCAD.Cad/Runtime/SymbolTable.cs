using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using Autodesk.AutoCAD.DatabaseServices;
namespace IFoxCAD.Cad
{
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
        internal SymbolTable(DBTrans tr, ObjectId tableId)
        {
            DTrans = tr;
            CurrentSymbolTable = DTrans.GetObject<TTable>(tableId);
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
                {
                    return CurrentSymbolTable[key];
                }
                return ObjectId.Null;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="id">对象id</param>
        /// <returns>对象的id</returns>
        public ObjectId this[ObjectId id]
        {
            // TODO: 需论证是否需要这个索引器
            get 
            { 
                /* return the specified index here */
                if (Has(id))
                {
                    return id;
                }
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
        public ObjectId Add(string name, Action<TRecord> action = null)
        {
            ObjectId id = this[name];
            if (id.IsNull)
            {
                TRecord record = new()
                {
                    Name = name
                };
                id = Add(record);
                using (record.ForWrite())
                {
                    action?.Invoke(record);
                }
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
            {
                record.Erase();
            }
        }
        /// <summary>
        /// 删除符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        public void Remove(string name)
        {
            TRecord record = GetRecord(name);
            if (record != null)
            {
                Remove(record);
            }
            
        }
        /// <summary>
        /// 删除符号表记录
        /// </summary>
        /// <param name="id">符号表记录对象id</param>
        public void Remove(ObjectId id)
        {
            TRecord record = GetRecord(id);
            if (record != null)
            {
                Remove(record);
            }
        }


        #endregion

        #region 修改符号表记录
        /// <summary>
        /// 修改符号表
        /// </summary>
        /// <param name="record">符号表记录</param>
        /// <param name="action">修改委托</param>
        private static void Change(TRecord record, Action<TRecord> action)
        {
            using (record.ForWrite())
            {
                action?.Invoke(record);
            }
        }
        /// <summary>
        /// 修改符号表
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <param name="action">修改委托</param>
        public void Change(string name, Action<TRecord> action)
        {
            var record = GetRecord(name);
            if (record != null)
            {
                Change(record, action);
            }
        }
        /// <summary>
        /// 修改符号表
        /// </summary>
        /// <param name="id">符号表记录id</param>
        /// <param name="action">修改委托</param>
        public void Change(ObjectId id, Action<TRecord> action)
        {
            var record = GetRecord(id);
            if (record != null)
            {
                Change(record, action);
            }
        }
        #endregion

        #region 获取符号表记录
        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="id">符号表记录的id</param>
        /// <param name="openMode">打开模式，默认为只读</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(ObjectId id, OpenMode openMode = OpenMode.ForRead) => id.IsNull ? null : DTrans.GetObject<TRecord>(id, openMode);

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <param name="openMode">打开模式，默认为只读</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(string name, OpenMode openMode = OpenMode.ForRead) => GetRecord(this[name], openMode);

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <returns>符号表记录迭代器</returns>
        public IEnumerable<TRecord> GetRecords()
        {
            return this.Select(id => GetRecord(id));
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
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            ObjectId rid = this[name];
            bool has = rid != ObjectId.Null;
            if ((has && over) || !has)
            {
                ObjectId id = table[name];
                using IdMapping idm = new();
                using (ObjectIdCollection ids = new()
                { id })
                {
                    table.Database.WblockCloneObjects(ids, CurrentSymbolTable.Id, idm, DuplicateRecordCloning.Replace, false);
                }
                rid = idm[id].Value;
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
        internal ObjectId GetRecordFrom(Func<DBTrans, SymbolTable<TTable, TRecord>> tableSelector, string fileName, string name, bool over)
        {
            using var tr = new DBTrans(fileName);
            return GetRecordFrom(tableSelector(tr), name, over);
        }
        #endregion

        #region IEnumerable<ObjectId> 成员

        public IEnumerator<ObjectId> GetEnumerator()
        {

            foreach (var id in CurrentSymbolTable)
            {
                yield return id;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
