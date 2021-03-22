﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

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


        #region 插入块参照

        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockName">块名</param>
        /// <param name="scale">块插入比例，默认为0</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
                                    string blockName,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default, Transaction trans = null)
        {
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            if (!DBTrans.Top.BlockTable.Has(blockName))
            {
                DBTrans.Top.Editor.WriteMessage($"\n不存在名字为{blockName}的块定义。");
                return ObjectId.Null;
            }
            return blockTableRecord.InsertBlock(position, DBTrans.Top.BlockTable[blockName], scale, rotation, atts, trans);
        }
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockId">块定义id</param>
        /// <param name="scale">块插入比例，默认为0</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
                                    ObjectId blockId,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default, Transaction trans = null)
        {
            if (trans is null)
            {
                trans = DBTrans.Top.Trans;
            }
            if (!DBTrans.Top.BlockTable.Has(blockId))
            {
                DBTrans.Top.Editor.WriteMessage($"\n不存在名字为{DBTrans.Top.GetObject<BlockTableRecord>(blockId).Name}的块定义。");
                return ObjectId.Null;
            }
            using var blockref = new BlockReference(position, blockId)
            {
                ScaleFactors = scale,
                Rotation = rotation
            };
            var objid = blockTableRecord.AddEntity(blockref);
            if (atts != default)
            {
                var btr = DBTrans.Top.GetObject<BlockTableRecord>(blockref.BlockTableRecord);
                if (btr.HasAttributeDefinitions)
                {
                    var attdefs = btr
                        .GetEntities<AttributeDefinition>()
                        .Where(attdef => !(attdef.Constant || attdef.Invisible));
                    foreach (var attdef in attdefs)
                    {
                        using AttributeReference attref = new();
                        attref.SetAttributeFromBlock(attdef, blockref.BlockTransform);
                        attref.Position = attdef.Position.TransformBy(blockref.BlockTransform);
                        attref.AdjustAlignment(DBTrans.Top.Database);
                        if (atts.ContainsKey(attdef.Tag))
                        {
                            attref.TextString = atts[attdef.Tag];
                        }

                        blockref.AttributeCollection.AppendAttribute(attref);
                        trans.AddNewlyCreatedDBObject(attref, true);
                    }
                }
            }
            return objid;
        }

        #endregion
        #endregion



    }
}