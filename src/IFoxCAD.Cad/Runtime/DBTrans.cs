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


    public class DBTrans : IDisposable
    {
        #region 私有字段

        private bool disposedValue;

        private bool _commit;

        #endregion

        public Database Database { get; private set; }

        public Document Document { get; private set; }

        public Editor Editor { get; private set; }

        public Transaction Trans { get; private set; }

        public DBTrans(bool commit = true)
        {
            Document = Application.DocumentManager.MdiActiveDocument;
            Database = Document.Database;
            Editor = Document.Editor;
            Trans = Database.TransactionManager.StartTransaction();
            _commit = commit;
        }

        public T GetObject<T>(ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false, bool forceOpenOnLockedLayer = false) where T : DBObject
        {
            return Trans.GetObject(id, mode, openErased, forceOpenOnLockedLayer) as T;
        }

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

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~DBTrans()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
