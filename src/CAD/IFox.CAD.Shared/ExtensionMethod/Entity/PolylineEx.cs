using System;
using System.Data.SqlTypes;

namespace IFoxCAD.Cad;

/// <summary>
/// 多段线扩展类
/// </summary>
public static class PolylineEx
{
    #region 获取多段线端点
    /// <summary>
    /// 获取二维多段线的端点坐标
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline2d pl2d)
    {
        var tr = DBTrans.GetTopTransaction(pl2d.Database);
        foreach (ObjectId id in pl2d)
        {
            if (tr.GetObject(id) is Vertex2d vertex)
            {
                yield return vertex.Position;
            }
        }
            
    }

    /// <summary>
    /// 获取三维多段线的端点坐标
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline3d pl3d)
    {
        var tr = DBTrans.GetTopTransaction(pl3d.Database);
        foreach (ObjectId id in pl3d)
        {
            if (tr.GetObject(id) is PolylineVertex3d vertex)
                yield return vertex.Position;
        }  
    }

    /// <summary>
    /// 获取多段线的端点坐标
    /// </summary>
    /// <param name="pl">多段线</param>
    /// <returns>端点坐标集合</returns>
    public static List<Point3d> GetPoints(this Polyline pl)
    {
        return
            Enumerable
            .Range(0, pl.NumberOfVertices)
            .Select(pl.GetPoint3dAt)
            .ToList();
    }
    #endregion

    #region 创建多段线
    /// <summary>
    /// 根据点集创建多段线<br/>
    /// 此多段线无默认全局宽度0，无圆弧段
    /// </summary>
    /// <param name="points">点集</param>
    /// <param name="action">多段线属性设置委托</param>
    /// <returns>多段线对象</returns>
    public static Polyline CreatePolyline(this IEnumerable<Point3d> points, Action<Polyline>? action = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        points.ForEach((pt, state, index) => {
            pl.AddVertexAt(index, pt.Point2d(), 0, 0, 0);
        });
        action?.Invoke(pl);
        return pl;
    }

    /// <summary>
    /// 根据点集创建多段线
    /// </summary>
    /// <param name="pts">端点表,利用元组(Point3d pt, double bulge, double startWidth, double endWidth)</param>
    /// <param name="action">轻多段线属性设置委托</param>
    /// <returns>轻多段线对象</returns>
    public static Polyline CreatePolyline(this IEnumerable<(Point3d pt, double bulge, double startWidth, double endWidth)> pts,
                                    Action<Polyline>? action = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();

        pts.ForEach((vertex, state, index) => {
            pl.AddVertexAt(index, vertex.pt.Point2d(), vertex.bulge, vertex.startWidth, vertex.endWidth);
        });
        action?.Invoke(pl);
        return pl;
    }

    #endregion

}