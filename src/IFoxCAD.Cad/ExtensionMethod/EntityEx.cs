using Autodesk.AutoCAD.DatabaseServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 实体图元类扩展函数
    /// </summary>
    public static class EntityEx
    {
        #region 实体刷新
        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="entity">实体对象</param>
        public static void Flush(this Entity entity, Transaction trans = null)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            entity.RecordGraphicsModified(true);
            trans.TransactionManager.QueueForGraphicsFlush();
        }

        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="id">实体id</param>
        public static void Flush(this ObjectId id) => Flush(DBTrans.Top.GetObject<Entity>(id));
        #endregion

    }
}
