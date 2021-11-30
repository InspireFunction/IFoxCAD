using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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
                trans = DBTrans.Top.Transaction;
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

        #region 多段线端点坐标
        /// <summary>
        /// 获取二维多段线的端点坐标
        /// </summary>
        /// <param name="pl2d">二维多段线</param>
        /// <param name="tr">事务</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline2d pl2d, Transaction tr = null)
        {
            tr ??= DBTrans.Top.Transaction;
            foreach (ObjectId id in pl2d)
            {
                yield return ((Vertex2d)tr.GetObject(id, OpenMode.ForRead)).Position;
            }
        }

        /// <summary>
        /// 获取三维多段线的端点坐标
        /// </summary>
        /// <param name="pl3d">三维多段线</param>
        /// <param name="tr">事务</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline3d pl3d, Transaction tr = null)
        {
            tr ??= DBTrans.Top.Transaction;
            foreach (ObjectId id in pl3d)
            {
                yield return ((PolylineVertex3d)tr.GetObject(id, OpenMode.ForRead)).Position;
            }
        }

        /// <summary>
        /// 获取多段线的端点坐标
        /// </summary>
        /// <param name="pl">多段线</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline pl)
        {
            return
                Enumerable
                .Range(0, pl.NumberOfVertices)
                .Select(i => pl.GetPoint3dAt(i));
        }
        #endregion

        #region TransformBy

        /// <summary>
        /// 移动实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="from">基点</param>
        /// <param name="to">目标点</param>
        public static void Move(this Entity ent, Point3d from, Point3d to)
        {
            ent.TransformBy(Matrix3d.Displacement(to - from));
        }

        /// <summary>
        /// 缩放实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">缩放基点坐标</param>
        /// <param name="scaleValue">缩放比例</param>
        public static void Scale(this Entity ent, Point3d center, double scaleValue)
        {
            ent.TransformBy(Matrix3d.Scaling(scaleValue, center));
        }

        /// <summary>
        /// 旋转实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angle">转角</param>
        /// <param name="normal">旋转平面的法向矢量</param>
        public static void Rotation(this Entity ent, Point3d center, double angle, Vector3d normal)
        {
            ent.TransformBy(Matrix3d.Rotation(angle, normal, center));
        }

        /// <summary>
        /// 在XY平面内旋转实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angle">转角</param>
        public static void Rotation(this Entity ent, Point3d center, double angle)
        {
            ent.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis.TransformBy(ent.Ecs), center));
        }

        /// <summary>
        /// 按对称轴镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="startPoint">对称轴起点</param>
        /// <param name="endPoint">对称轴终点</param>
        public static void Mirror(this Entity ent, Point3d startPoint, Point3d endPoint)
        {
            ent.TransformBy(Matrix3d.Mirroring(new Line3d(startPoint, endPoint)));
        }

        /// <summary>
        /// 按对称面镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="plane">对称平面</param>
        public static void Mirror(this Entity ent, Plane plane)
        {
            ent.TransformBy(Matrix3d.Mirroring(plane));
        }

        /// <summary>
        /// 按对称点镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="basePoint">对称点</param>
        public static void Mirror(this Entity ent, Point3d basePoint)
        {
            ent.TransformBy(Matrix3d.Mirroring(basePoint));
        }

        #endregion

        #region 实体范围
        /// <summary>
        /// 获取实体集合的范围
        /// </summary>
        /// <param name="ents">实体迭代器</param>
        /// <returns>实体集合的范围</returns>
        public static Extents3d GetExtents(this IEnumerable<Entity> ents)
        {
            var it = ents.GetEnumerator();
            var ext = it.Current.GeometricExtents;
            while (it.MoveNext())
                ext.AddExtents(it.Current.GeometricExtents);
            return ext;
        }
        #endregion

        #region 单行文字

        /// <summary>
        /// 更正单行文字的镜像属性
        /// </summary>
        /// <param name="txt">单行文字</param>
        public static void ValidateMirror(this DBText txt)
        {
            if (!txt.Database.Mirrtext)
            {
                txt.IsMirroredInX = false;
                txt.IsMirroredInY = false;
            }
        }
        #endregion

        #region 多行文字

        /// <summary>
        /// 炸散多行文字
        /// </summary>
        /// <typeparam name="T">存储多行文字炸散之后的对象的类型</typeparam>
        /// <param name="mt">多行文字</param>
        /// <param name="obj">存储对象变量</param>
        /// <param name="mTextFragmentCallback">回调函数，用于处理炸散之后的对象
        /// <para>MTextFragment -- 多行文字炸散后的对象</para>
        /// <para>MTextFragmentCallbackStatus -- 回调函数处理的结果</para>
        /// </param>
        public static void ExplodeFragments<T>(this MText mt, T obj, Func<MTextFragment, T, MTextFragmentCallbackStatus> mTextFragmentCallback)
        {
            mt.ExplodeFragments((f, o) => mTextFragmentCallback(f, (T)o), obj);
        }

        /// <summary>
        /// 获取多行文字的无格式文本
        /// </summary>
        /// <param name="mt">多行文字</param>
        /// <returns>文本</returns>
        public static string GetUnFormatString(this MText mt)
        {
            List<string> strs = new();
            mt.ExplodeFragments(
                strs,
                (f, o) => {
                    o.Add(f.Text);
                    return MTextFragmentCallbackStatus.Continue;
                });
            return string.Join("", strs.ToArray());
        }
        #endregion

        #region 圆弧

        /// <summary>
        /// 根据圆心、起点和终点来创建圆弧(二维)
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="centerPoint">圆心</param>
        /// <param name="endPoint">终点</param>
        /// <returns>圆弧</returns>
        public static Arc CreateArcSCE(Point3d startPoint, Point3d centerPoint, Point3d endPoint)
        {
            Arc arc = new();
            arc.Center = centerPoint;
            arc.Radius = centerPoint.DistanceTo(startPoint);
            Vector2d startVector = new(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            Vector2d endVector = new(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
            //计算起始和终止角度
            arc.StartAngle = startVector.Angle;
            arc.EndAngle = endVector.Angle;
            return arc;
        }
        /// <summary>
        /// 三点法创建圆弧(二维)
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="pointOnArc">圆弧上的点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>圆弧</returns>
        public static Arc CreateArc(Point3d startPoint, Point3d pointOnArc, Point3d endPoint)
        {
            //创建一个几何类的圆弧对象
            CircularArc3d geArc = new(startPoint, pointOnArc, endPoint);
            //将几何类圆弧对象的圆心和半径赋值给圆弧
#if ac2009
            return geArc.ToArc();
#elif ac2013
            return (Arc)Curve.CreateFromGeCurve(geArc);
#endif
        }

        /// <summary>
        /// 根据起点、圆心和圆弧角度创建圆弧(二维)
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="centerPoint">圆心</param>
        /// <param name="angle">圆弧角度</param>
        /// <returns>圆弧</returns>
        public static Arc CreateArc(Point3d startPoint, Point3d centerPoint, double angle)
        {
            Arc arc = new();
            arc.Center = centerPoint;
            arc.Radius = centerPoint.DistanceTo(startPoint);
            Vector2d startVector = new(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            arc.StartAngle = startVector.Angle;
            arc.EndAngle = startVector.Angle + angle;
            return arc;
        }

        #endregion

        #region 圆

        /// <summary>
        /// 两点创建圆(两点中点为圆心)
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>圆</returns>
        public static Circle CreateCircle(Point3d startPoint, Point3d endPoint)
        {
            Circle circle = new();
            circle.Center = startPoint.GetMidPointTo(endPoint);
            circle.Radius = startPoint.DistanceTo(endPoint) * 0.5;
            return circle;
        }

        /// <summary>
        /// 三点法创建圆(失败则返回Null)
        /// </summary>
        /// <param name="pt1">第一点</param>
        /// <param name="PointV">第二点</param>
        /// <param name="pt3">第三点</param>
        /// <returns>圆</returns>
        public static Circle CreateCircle(Point3d pt1, Point3d PointV, Point3d pt3)
        {
            //先判断三点是否共线,得到pt1点指向PointV、PointV点的矢量
            Vector3d va = pt1.GetVectorTo(PointV);
            Vector3d vb = pt1.GetVectorTo(pt3);
            //如两矢量夹角为0或180度（π弧度),则三点共线.
            if (va.GetAngleTo(vb) == 0 | va.GetAngleTo(vb) == Math.PI)
            {
                return null;
            }
            else
            {
                //创建一个几何类的圆弧对象
                CircularArc3d geArc = new(pt1, PointV, pt3);
                geArc.ToCircle();
                return geArc.ToCircle();
            }
        }

        #endregion

        #region 块参照

        #region 裁剪块参照

        private const string filterDictName = "ACAD_FILTER";
        private const string spatialName = "SPATIAL";

        /// <summary>
        /// 裁剪块参照
        /// </summary>
        /// <param name="bref">块参照</param>
        /// <param name="pt3ds">裁剪多边形点表</param>
        public static void ClipBlockRef(this BlockReference bref, IEnumerable<Point3d> pt3ds)
        {
            if (bref == null)
            {
                throw new ArgumentNullException(nameof(bref));
            }
            if (pt3ds == null)
            {
                throw new ArgumentNullException(nameof(pt3ds));
            }
            Matrix3d mat = bref.BlockTransform.Inverse();
            var pts =
                pt3ds
                .Select(p => p.TransformBy(mat))
                .Select(p => new Point2d(p.X, p.Y))
                .ToCollection();

            SpatialFilterDefinition sfd = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
            using SpatialFilter sf = new() { Definition = sfd };
            var dict = bref.GetXDictionary().GetSubDictionary(true, new string[] { filterDictName });
            dict.SetAt<SpatialFilter>(spatialName, sf);
            //SetToDictionary(dict, spatialName, sf);
        }

        /// <summary>
        /// 裁剪块参照
        /// </summary>
        /// <param name="bref">块参照</param>
        /// <param name="pt1">第一角点</param>
        /// <param name="PointV">第二角点</param>
        public static void ClipBlockRef(this BlockReference bref, Point3d pt1, Point3d PointV)
        {
            if (bref == null)
            {
                throw new ArgumentNullException(nameof(bref));
            }
            Matrix3d mat = bref.BlockTransform.Inverse();
            pt1 = pt1.TransformBy(mat);
            PointV = PointV.TransformBy(mat);
            Point2dCollection pts = new()
            {
                new Point2d(Math.Min(pt1.X, PointV.X), Math.Min(pt1.Y, PointV.Y)),
                new Point2d(Math.Max(pt1.X, PointV.X), Math.Max(pt1.Y, PointV.Y))
            };

            SpatialFilterDefinition sfd = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
            using SpatialFilter sf = new() { Definition = sfd };
            var dict = bref.GetXDictionary().GetSubDictionary(true, new string[] { filterDictName });
            dict.SetAt<SpatialFilter>(spatialName, sf);
            //SetToDictionary(dict, spatialName, sf);
        }
        #endregion
        #endregion
    }
}
