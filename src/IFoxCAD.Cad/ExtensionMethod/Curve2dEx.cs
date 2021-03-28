using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 二维解析类曲线转换为二维实体曲线扩展类
    /// </summary>

    public static class Curve2dEx
    {
        #region Curve2d

        /// <summary>
        /// 按矩阵转换Ge2d曲线为Db曲线
        /// </summary>
        /// <param name="curve">Ge2d曲线</param>
        /// <param name="mat">曲线转换矩阵</param>
        /// <returns>Db曲线</returns>
        public static Curve ToCurve(this Curve2d curve, Matrix3d mat)
        {
            return curve switch
            {
                LineSegment2d li => ToCurve(li, mat),
                NurbCurve2d nu => ToCurve(nu, mat),
                EllipticalArc2d el => ToCurve(el, mat),
                CircularArc2d ci => ToCurve(ci, mat),
                PolylineCurve2d po => ToCurve(po, mat),
                Line2d l2 => ToCurve(l2, mat),
                CompositeCurve2d co => ToCurve(co, mat),
                _ => null
            };
        }

        #endregion Curve2d

        #region CircularArc2d

        /// <summary>
        /// 判断点是否位于圆内及圆上
        /// </summary>
        /// <param name="ca2d">二维解析类圆弧对象</param>
        /// <param name="pnt">二维点</param>
        /// <returns>位于圆内及圆上返回 <see langword="true"/>,反之返回 <see langword="false"/></returns>
        public static bool IsIn(this CircularArc2d ca2d, Point2d pnt)
        {
            return ca2d.IsOn(pnt) || ca2d.IsInside(pnt);
        }

        /// <summary>
        /// 将二维解析类圆弧转换为实体圆或者圆弧，然后进行矩阵变换
        /// </summary>
        /// <param name="ca2d">二维解析类圆弧对象</param>
        /// <param name="mat">变换矩阵</param>
        /// <returns>实体圆或者圆弧</returns>
        public static Curve ToCurve(this CircularArc2d ca2d, Matrix3d mat)
        {
            Curve c = ToCurve(ca2d);
            c.TransformBy(mat);
            return c;
        }

        /// <summary>
        /// 将二维解析类圆弧转换为实体圆或者圆弧
        /// </summary>
        /// <param name="ca2d">二维解析类圆弧对象</param>
        /// <returns>实体圆或者圆弧</returns>
        public static Curve ToCurve(this CircularArc2d ca2d)
        {
            if (ca2d.IsClosed())
            {
                return ToCircle(ca2d);
            }
            else
            {
                return ToArc(ca2d);
            }
        }

        /// <summary>
        /// 将二维解析类圆弧转换为实体圆
        /// </summary>
        /// <param name="c2d">二维解析类圆弧对象</param>
        /// <returns>实体圆</returns>
        public static Circle ToCircle(this CircularArc2d c2d)
        {
            return
                new Circle(
                    new Point3d(new Plane(), c2d.Center),
                    new Vector3d(0, 0, 1),
                    c2d.Radius);
        }

        /// <summary>
        /// 将二维解析类圆弧转换为实体圆弧
        /// </summary>
        /// <param name="a2d">二维解析类圆弧对象</param>
        /// <returns>圆弧</returns>
        public static Arc ToArc(this CircularArc2d a2d)
        {
            double startangle, endangle;
            double refangle = a2d.ReferenceVector.Angle;

            if (a2d.IsClockWise)
            {
                startangle = -a2d.EndAngle - refangle;
                endangle = -a2d.StartAngle - refangle;
            }
            else
            {
                startangle = a2d.StartAngle + refangle;
                endangle = a2d.EndAngle + refangle;
            }

            return
                new Arc(
                    new Point3d(new Plane(), a2d.Center),
                    Vector3d.ZAxis,
                    a2d.Radius,
                    startangle,
                    endangle);
        }

        #endregion CircularArc2d

        #region EllipticalArc2d

        //椭圆弧
        /// <summary>
        /// 将二维解析类椭圆弧转换为实体椭圆弧，然后进行矩阵变换
        /// </summary>
        /// <param name="ea2d">二维解析类椭圆弧对象</param>
        /// <param name="mat">变换矩阵</param>
        /// <returns>实体椭圆弧</returns>
        public static Ellipse ToCurve(this EllipticalArc2d ea2d, Matrix3d mat)
        {
            Ellipse e = ToCurve(ea2d);
            e.TransformBy(mat);
            return e;
        }

        /// <summary>
        /// 将二维解析类椭圆弧转换为实体椭圆弧
        /// </summary>
        /// <param name="ea2d">二维解析类椭圆弧对象</param>
        /// <returns>实体椭圆弧</returns>
        public static Ellipse ToCurve(this EllipticalArc2d ea2d)
        {
            Plane plane = new Plane();
            Ellipse ell =
                new Ellipse(
                    new Point3d(plane, ea2d.Center),
                    new Vector3d(0, 0, 1),
                    new Vector3d(plane, ea2d.MajorAxis) * ea2d.MajorRadius,
                    ea2d.MinorRadius / ea2d.MajorRadius,
                    0,
                    Math.PI * 2);
            if (!ea2d.IsClosed())
            {
                if (ea2d.IsClockWise)
                {
                    ell.StartAngle = -ell.GetAngleAtParameter(ea2d.EndAngle);
                    ell.EndAngle = -ell.GetAngleAtParameter(ea2d.StartAngle);
                }
                else
                {
                    ell.StartAngle = ell.GetAngleAtParameter(ea2d.StartAngle);
                    ell.EndAngle = ell.GetAngleAtParameter(ea2d.EndAngle);
                }
            }
            return ell;
        }

        #endregion EllipticalArc2d

        #region Line2d

        /// <summary>
        /// 将二维解析类直线转换为实体类构造线
        /// </summary>
        /// <param name="line2d">二维解析类直线</param>
        /// <returns>实体类构造线</returns>
        public static Xline ToCurve(this Line2d line2d)
        {
            Plane plane = new Plane();
            return
                new Xline
                {
                    BasePoint = new Point3d(plane, line2d.PointOnLine),
                    SecondPoint = new Point3d(plane, line2d.PointOnLine + line2d.Direction)
                };
        }

        /// <summary>
        /// 将二维解析类直线转换为实体类构造线，然后进行矩阵变换
        /// </summary>
        /// <param name="line2d">二维解析类直线</param>
        /// <param name="mat">变换矩阵</param>
        /// <returns>实体类构造线</returns>
        public static Xline ToCurve(this Line2d line2d, Matrix3d mat)
        {
            Xline xl = ToCurve(line2d);
            xl.TransformBy(mat);
            return xl;
        }

        /// <summary>
        /// 将二维解析类构造线转换为二维解析类线段
        /// </summary>
        /// <param name="line2d">二维解析类构造线</param>
        /// <param name="fromParameter">起点参数</param>
        /// <param name="toParameter">终点参数</param>
        /// <returns>二维解析类线段</returns>
        public static LineSegment2d ToLineSegment2d(this Line2d line2d, double fromParameter, double toParameter)
        {
            return
                new LineSegment2d
                (
                    line2d.EvaluatePoint(fromParameter),
                    line2d.EvaluatePoint(toParameter)
                );
        }

        #endregion Line2d

        #region LineSegment2d

        /// <summary>
        /// 将二维解析类线段转换为实体类直线，并进行矩阵变换
        /// </summary>
        /// <param name="ls2d">二维解析类线段</param>
        /// <param name="mat">变换矩阵</param>
        /// <returns>实体类直线</returns>
        public static Line ToCurve(this LineSegment2d ls2d, Matrix3d mat)
        {
            Line l = ToCurve(ls2d);
            l.TransformBy(mat);
            return l;
        }

        /// <summary>
        /// 将二维解析类线段转换为实体类直线
        /// </summary>
        /// <param name="ls2d">二维解析类线段</param>
        /// <returns>实体类直线</returns>
        public static Line ToCurve(this LineSegment2d ls2d)
        {
            Plane plane = new Plane();
            return
                new Line(
                    new Point3d(plane, ls2d.StartPoint),
                    new Point3d(plane, ls2d.EndPoint));

        }

        #endregion LineSegment2d

        #region NurbCurve2d

        /// <summary>
        /// 将二维解析类BURB曲线转换为实体类样条曲线，并进行矩阵变换
        /// </summary>
        /// <param name="nc2d">二维解析类BURB曲线</param>
        /// <param name="mat">变换矩阵</param>
        /// <returns>实体类样条曲线</returns>
        public static Spline ToCurve(this NurbCurve2d nc2d, Matrix3d mat)
        {
            Spline spl = ToCurve(nc2d);
            spl.TransformBy(mat);
            return spl;
        }

        /// <summary>
        /// 将二维解析类BURB曲线转换为实体类样条曲线
        /// </summary>
        /// <param name="nc2d">二维解析类BURB曲线</param>
        /// <returns>实体类样条曲线</returns>
        public static Spline ToCurve(this NurbCurve2d nc2d)
        {
            int i;
            Plane plane = new Plane();
            Point3dCollection ctlpnts = new Point3dCollection();
            for (i = 0; i < nc2d.NumControlPoints; i++)
            {
                ctlpnts.Add(new Point3d(plane, nc2d.GetControlPointAt(i)));
            }

            DoubleCollection knots = new DoubleCollection();
            foreach (double knot in nc2d.Knots)
            {
                knots.Add(knot);
            }

            DoubleCollection weights = new DoubleCollection();
            for (i = 0; i < nc2d.NumWeights; i++)
            {
                weights.Add(nc2d.GetWeightAt(i));
            }

            NurbCurve2dData ncdata = nc2d.DefinitionData;

            return
                new Spline(
                    ncdata.Degree,
                    ncdata.Rational,
                    nc2d.IsClosed(),
                    ncdata.Periodic,
                    ctlpnts,
                    knots,
                    weights,
                    0,
                    nc2d.Knots.Tolerance);
        }

        #endregion NurbCurve2d
    }
}