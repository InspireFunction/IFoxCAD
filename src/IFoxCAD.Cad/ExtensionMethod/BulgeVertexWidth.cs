#if !HC2020
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif
using System;

namespace IFoxCAD;

/// <summary>
/// 多段线的顶点,凸度,头宽,尾宽
/// </summary>
[Serializable]
public class BulgeVertexWidth
{
    /// <summary>
    /// 顶点X
    /// </summary>
    public double X;
    /// <summary>
    /// 顶点Y
    /// </summary>
    public double Y;
    /// <summary>
    /// 凸度
    /// </summary>
    public double Bulge;
    /// <summary>
    /// 头宽
    /// </summary>
    public double StartWidth;
    /// <summary>
    /// 尾宽
    /// </summary>
    public double EndWidth;

    public Point2d Vertex => new(X, Y);

    public BulgeVertexWidth() { }

    /// <summary>
    /// 多段线的顶点,凸度,头宽,尾宽
    /// </summary>
    public BulgeVertexWidth(double vertex_X, double vertex_Y,
        double bulge = 0,
        double startWidth = 0,
        double endWidth = 0)
    {
        X = vertex_X;
        Y = vertex_Y;
        Bulge = bulge;
        StartWidth = startWidth;
        EndWidth = endWidth;
    }

    /// <summary>
    /// 多段线的顶点,凸度,头宽,尾宽
    /// </summary>
    public BulgeVertexWidth(Point2d vertex,
        double bulge = 0,
        double startWidth = 0,
        double endWidth = 0)
        : this(vertex.X, vertex.Y, bulge, startWidth, endWidth)
    { }

    /// <summary>
    /// 多段线的顶点,凸度,头宽,尾宽
    /// </summary>
    public BulgeVertexWidth(BulgeVertex bv)
        : this(bv.Vertex.X, bv.Vertex.Y, bv.Bulge)
    { }

    /// <summary>
    /// 多段线的顶点,凸度,头宽,尾宽
    /// </summary>
    /// <param name="pl">多段线</param>
    /// <param name="index">子段编号</param>
    public BulgeVertexWidth(Polyline pl, int index)
    {
        var pt = pl.GetPoint2dAt(index);//这里可以3d
        X = pt.X;
        Y = pt.Y;
        Bulge = pl.GetBulgeAt(index);
        StartWidth = pl.GetStartWidthAt(index);
        EndWidth = pl.GetEndWidthAt(index);
    }

    public BulgeVertex ToBulgeVertex()
    {
        return new BulgeVertex(Vertex, Bulge);
    } 
}
