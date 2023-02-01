﻿namespace IFoxCAD.Cad;

/// <summary>
/// 多段线扩展类
/// </summary>
public static class PolylineEx
{
    /// <summary>
    /// 获取二维多段线的端点坐标
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <param name="trans">事务</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline2d pl2d, DBTrans? trans = null)
    {
        trans ??= DBTrans.Top;
        foreach (ObjectId id in pl2d)
        {
            var vertex = trans.GetObject<Vertex2d>(id);
            if (vertex != null)
                yield return vertex.Position;
        }
            
    }

    /// <summary>
    /// 获取三维多段线的端点坐标
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <param name="trans">事务</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline3d pl3d, DBTrans? trans = null)
    {
        trans ??= DBTrans.Top;
        foreach (ObjectId id in pl3d)
        {
            var vertex = trans.GetObject<PolylineVertex3d>(id);
            if (vertex != null)
                yield return vertex.Position;
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
            .Select(pl.GetPoint3dAt);
    }
}