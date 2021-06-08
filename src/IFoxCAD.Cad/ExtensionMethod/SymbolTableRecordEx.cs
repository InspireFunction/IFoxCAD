using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 符号表记录扩展类
    /// </summary>
    public static class SymbolTableRecordEx
    {


        #region 块表记录

        #region 添加实体
        public static ObjectId AddEntity(this BlockTableRecord btr, Entity entity, Transaction trans = null)
        {
            ObjectId id;
            trans ??= DBTrans.Top.Trans;
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
            trans ??= DBTrans.Top.Trans;
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
        /// 添加多个实体
        /// </summary>
        /// <param name="btr"></param>
        /// <param name="ents"></param>
        /// <returns></returns>
        public static List<ObjectId> AddEntity(this BlockTableRecord btr, params Entity[] ents)
        {
            return btr.AddEntity(ents, null);
        }
        #endregion

        #region 添加图元
        /// <summary>
        /// 在指定绘图空间添加图元
        /// </summary>
        /// <typeparam name="T">图元类型</typeparam>
        /// <param name="btr">绘图空间</param>
        /// <param name="ent">图元对象</param>
        /// <param name="action">图元属性设置委托</param>
        /// <param name="trans">事务管理器</param>
        /// <returns>图元id</returns>
        private static ObjectId AddEnt<T>(this BlockTableRecord btr, T ent, Action<T> action, Transaction trans) where T : Entity
        {
            trans ??= DBTrans.Top.Trans;
            action?.Invoke(ent);
            return btr.AddEntity(ent, trans);
        }

        /// <summary>
        /// 在指定绘图空间添加直线
        /// </summary>
        /// <param name="trans">事务管理器</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="btr">绘图空间</param>
        /// <param name="action">直线属性设置委托</param>
        /// <returns>直线的id</returns>
        public static ObjectId AddLine(this BlockTableRecord btr, Point3d start, Point3d end, Action<Line> action = default, Transaction trans = default)
        {
            var line = new Line(start, end);
            return btr.AddEnt(line, action, trans);
        }
        /// <summary>
        /// 在指定绘图空间X-Y平面添加圆
        /// </summary>
        /// <param name="btr">绘图空间</param>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="action">圆属性设置委托</param>
        /// <param name="trans">事务管理器</param>
        /// <returns>圆的id</returns>
        public static ObjectId AddCircle(this BlockTableRecord btr, Point3d center, double radius, Action<Circle> action = default, Transaction trans = default)
        {
            var circle = new Circle(center, Vector3d.ZAxis, radius);
            return btr.AddEnt(circle, action, trans);
        }

        /// <summary>
        /// 在指定绘图空间X-Y平面3点画外接圆
        /// </summary>
        /// <param name="btr">绘图空间</param>
        /// <param name="p0">第一点</param>
        /// <param name="p1">第二点</param>
        /// <param name="p2">第三点</param>
        /// <param name="action">圆属性设置委托</param>
        /// <param name="trans">事务管理器</param>
        /// <returns>三点有外接圆则返回圆的id，否则返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this BlockTableRecord btr, Point3d p0, Point3d p1, Point3d p2, Action<Circle> action = default, Transaction trans = default)
        {
            var dx1 = p1.X - p0.X;
            var dy1 = p1.Y - p0.Y;
            var dx2 = p2.X - p0.X;
            var dy2 = p2.Y - p0.Y;

            var d = dx1 * dy2 - dx2 * dy1;

            if (d != 0.0)
            {
                var d2 = d * 2;
                var c1 = (p0.X + p1.X) * dx1 + (p0.Y + p1.Y) * dy1;
                var c2 = (p0.X + p2.X) * dx2 + (p0.Y + p2.Y) * dy2;
                var ce = new Point3d((c1 * dy2 - c2 * dy1) / d2, (c2 * dx1 - c1 * dx2) / d2, 0);
                var circle = new Circle(ce, Vector3d.ZAxis, p0.DistanceTo(ce));
                return btr.AddEnt(circle, action, trans);
            }
            return ObjectId.Null;
        }
        /// <summary>
        /// 在指定的绘图空间添加轻多段线
        /// </summary>
        /// <param name="btr">绘图空间</param>
        /// <param name="pts">端点表</param>
        /// <param name="bulges">凸度表</param>
        /// <param name="startWidths">端点的起始宽度</param>
        /// <param name="endWidths">端点的终止宽度</param>
        /// <param name="action">轻多段线属性设置委托</param>
        /// <param name="trans">事务管理器</param>
        /// <returns>轻多段线</returns>
        public static ObjectId AddPline(this BlockTableRecord btr, List<Point3d> pts, List<double> bulges = default, List<double> startWidths = default, List<double> endWidths = default, Action<Polyline> action = default, Transaction trans = default)
        {
            bulges ??= new List<double>(new double[pts.Count]);
            startWidths ??= new List<double>(new double[pts.Count]);
            endWidths ??= new List<double>(new double[pts.Count]);
            Polyline pl = new();
            for (int i = 0; i < pts.Count; i++)
            {
                pl.AddVertexAt(i, pts[i].Point2d(), bulges[i], startWidths[i], endWidths[i]);
            }
            return btr.AddEnt(pl, action, trans);
        }

        /// <summary>
        /// 在指定绘图空间X-Y平面3点画圆弧
        /// </summary>
        /// <param name="btr">绘图空间</param>
        /// <param name="startPoint">圆弧起点</param>
        /// <param name="pointOnArc">圆弧上的点</param>
        /// <param name="endPoint">圆弧终点</param>
        /// <param name="action">圆弧属性设置委托</param>
        /// <param name="trans">事务管理器</param>
        /// <returns>圆弧id</returns>
        public static ObjectId AddArc(this BlockTableRecord btr, Point3d startPoint, Point3d pointOnArc, Point3d endPoint, Action<Arc> action = default, Transaction trans = default)
        {

            var arc = new CircularArc3d(startPoint, pointOnArc, endPoint);
#if ac2009
            return btr.AddEnt(arc.ToArc(), action, trans);
#elif ac2013
            return btr.AddEnt(Curve.CreateFromGeCurve(arc) as Arc, action, trans);
#endif           
        }
        #endregion

        #region 获取实体/实体id
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
            trans ??= DBTrans.Top.Trans;
            return
                btr
                .Cast<ObjectId>()
                .Select(id => trans.GetObject(id, mode))
                .OfType<T>();
        }

        /// <summary>
        /// 按类型获取实体Id,AutoCad2010以上版本支持
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <returns>实体Id集合</returns>
        public static IEnumerable<ObjectId> GetObjectIds<T>(this BlockTableRecord btr) where T : Entity
        {
            string dxfName = RXClass.GetClass(typeof(T)).DxfName;
            return btr.Cast<ObjectId>()
                .Where(id => id.ObjectClass.DxfName == dxfName);
        }

        /// <summary>
        /// 按类型获取实体Id的分组
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <returns>实体Id分组</returns>
        public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIds(this BlockTableRecord btr)
        {
            return
                btr
                .Cast<ObjectId>()
                .GroupBy(id => id.ObjectClass.DxfName);
        }

        /// <summary>
        /// 获取绘制顺序表
        /// </summary>
        /// <param name="btr">块表</param>
        /// <param name="tr">事务</param>
        /// <returns>绘制顺序表</returns>
        public static DrawOrderTable GetDrawOrderTable(this BlockTableRecord btr, Transaction tr = null)
        {
            tr ??= DBTrans.Top.Trans;
            return tr.GetObject(btr.DrawOrderTableId, OpenMode.ForRead) as DrawOrderTable;
        }

        #endregion

        #region 插入块参照

        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockName">块名</param>
        /// <param name="scale">块插入比例，默认为1</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
                                    string blockName,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default, Transaction trans = null)
        {
            trans ??= DBTrans.Top.Trans;
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
        /// <param name="scale">块插入比例，默认为1</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
                                    ObjectId blockId,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default, Transaction trans = null)
        {
            trans ??= DBTrans.Top.Trans;
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
