using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 符号表记录扩展类
    /// </summary>
    public static class SymbolTableRecordEx
    {
        

        #region 块表记录

        public static ObjectId AddEntity(this BlockTableRecord btr, Entity entity, Transaction trans = null)
        {
            ObjectId id;
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            
            using (btr.ForWrite())
            {
                id = btr.AppendEntity(entity);
                trans.AddNewlyCreatedDBObject(entity, true);
            }

            return id;
        }




        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="ents">实体集合</param>
        /// <returns>对象 id 列表</returns>
        public static List<ObjectId> AddEntity<T>(this BlockTableRecord btr, IEnumerable<T> ents, Transaction trans = null) where T : Entity
        {
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            using (btr.ForWrite())
            {
                return ents
                    .Select(ent =>
                    {
                        ObjectId id = btr.AppendEntity(ent);
                        trans.AddNewlyCreatedDBObject(ent, true);
                        return id;
                    })
                    .ToList();
            }
        }


        /// <summary>
        /// 获取块表记录内的指定类型的实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>实体集合</returns>
        public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr, OpenMode mode = OpenMode.ForRead, Transaction trans = null) where T : Entity
        {
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            return
                btr
                .Cast<ObjectId>()
                .Select(id => trans.GetObject(id, mode))
                .OfType<T>();
        }
        #endregion



    }
}
