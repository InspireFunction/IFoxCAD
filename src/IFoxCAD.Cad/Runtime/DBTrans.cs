using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

namespace IFoxCAD.Cad
{
    // TODO 命名的属性

    public class DBTrans : IDisposable
    {
        #region 私有字段

        private bool disposedValue;

        private bool _commit;

        #endregion

        #region 公开属性
        /// <summary>
        /// 数据库
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// 文档
        /// </summary>
        public Document Document { get; private set; }
        /// <summary>
        /// 命令行
        /// </summary>
        public Editor Editor { get; private set; }
        /// <summary>
        /// 事务管理器
        /// </summary>
        public Transaction Trans { get; private set; }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数，默认事务退出时提交全部操作
        /// </summary>
        /// <param name="commit"></param>
        public DBTrans(bool commit = true)
        {
            Document = Application.DocumentManager.MdiActiveDocument;
            Database = Document.Database;
            Editor = Document.Editor;
            Trans = Database.TransactionManager.StartTransaction();
            _commit = commit;
        }

        #endregion

        #region 符号表

        /// <summary>
        /// 块表
        /// </summary>
        public SymbolTable<BlockTable, BlockTableRecord> BlockTable => new(this, Database.BlockTableId);
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

        #region 获取对象
        /// <summary>
        /// 根据对象id获取图元对象
        /// </summary>
        /// <typeparam name="T">要获取的图元对象的类型</typeparam>
        /// <param name="id">对象id</param>
        /// <param name="mode">打开模式，默认为只读</param>
        /// <param name="openErased">是否打开已删除对象，默认为不打开</param>
        /// <param name="forceOpenOnLockedLayer">是否打开锁定图层对象，默认为不打开</param>
        /// <returns></returns>
        public T GetObject<T>(ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false, bool forceOpenOnLockedLayer = false) where T : DBObject
        {
            return Trans.GetObject(id, mode, openErased, forceOpenOnLockedLayer) as T;
        }

        #endregion

        #region idispose接口相关函数

        public void Abort()
        {
            Trans.Abort();
        }

        public void Commit()
        {
            if (_commit)
            {
                Trans.Commit();
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
                    // TODO: 释放托管状态(托管对象)
                    Commit();
                    Trans.Dispose();

                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
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
}
