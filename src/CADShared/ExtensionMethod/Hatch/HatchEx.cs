// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IFoxCAD.Cad;

/// <summary>
/// 充填扩展类
/// </summary>
public static class HatchEx
{
    /// <summary>
    /// 遍历填充每条边
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="action"></param>
    public static void ForEach(this Hatch hatch, Action<HatchLoop> action)
    {
        for (var i = 0; i < hatch.NumberOfLoops; i++)
            action.Invoke(hatch.GetLoopAt(i));
    }

    /// <summary>
    /// 提取已存在的关联边界(一个边界环里所有的对象 id 组成一个 ObjectIdCollection)
    /// </summary>
    /// <param name="hatch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<ObjectIdCollection>? GetAssociatedBoundaryIds(this Hatch hatch)
    {
        // if (!hatch.Id.IsOk())
        //     throw new ArgumentException("填充未加入或不存在于数据库");

        if (!hatch.Associative)
            return null;

        var listObjIdColl = new List<ObjectIdCollection>();

        for (var i = 0; i < hatch.NumberOfLoops; i++)
        {
            var assIds = hatch.GetAssociatedObjectIdsAt(i);
            if (assIds != null)
            {
                var objIds = new ObjectIdCollection();
                foreach (ObjectId id in assIds)
                {
                    if (id.IsOk())
                        objIds.Add(id);
                }

                if (objIds.Count == 0)
                {
                    throw new ArgumentException("关联的填充边界被删除后没有清理反应器,请调用:" +
                                                "\n hatch.RemoveAssociatedObjectIds()" +
                                                "\n hatch.Associative = false");
                }

                listObjIdColl.Add(objIds);
            }
        }

        return listObjIdColl;
    }

    /// <summary>
    /// 创建边界(仅创建于内存中，未加入数据库)
    /// </summary>
    /// <param name="hatch"></param>
    /// <returns>边界环列表(一个边界环里所有的对象组成一个 DBObjectCollection)</returns>
    public static List<DBObjectCollection> CreateBoundarys(this Hatch hatch)
    {
        // if (!hatch.Id.IsOk())
        //     throw new ArgumentException("填充未加入或不存在于数据库");

        var listDbObjColl = new List<DBObjectCollection>();

        for (var i = 0; i < hatch.NumberOfLoops; i++)
        {
            var objColl = new DBObjectCollection();
            var loop = hatch.GetLoopAt(i);
            var isCurve2d = true;

            if (loop.IsPolyline)
            {
                // 边界是多段线
                HatchLoopIsPolyline(loop, objColl);
                isCurve2d = false;
            }
            else
            {
                // 1是不可能的,大于2的是曲线;
                if (loop.Curves.Count == 2)
                {
                    // 边界是曲线,过滤可能是圆形的情况
                    var circle = TwoArcFormOneCircle(loop, objColl);
                    if (circle is not null)
                    {
                        objColl.Add(circle);
                        isCurve2d = false;
                    }
                }
            }

            // 边界是曲线
            if (isCurve2d)
                HatchLoopIsCurve2d(loop, objColl);

            listDbObjColl.Add(objColl);
        }

        return listDbObjColl;
    }

    /// <summary>
    /// 重新设置边界并计算
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="boundaryIds">边界对象(一个边界环里所有的对象 id 组成一个 ObjectIdCollection)
    /// <br>边界必需是封闭的环, 可以是一条线围合而成也可以是多条线首尾相连围合而成</br>
    /// <br>多个外边界的时候, 建议顺序为(外边界,外边界,外边界,普通边界....), 或(外边界, 普通边界.....外边界, 普通边界....)</br>
    /// </param>
    /// <param name="associative">关联边界(默认保持原样)</param>
    public static void ResetBoundarys(this Hatch hatch, List<ObjectIdCollection> boundaryIds,
        bool? associative = null)
    {
        // if (!hatch.Id.IsOk())
        //     throw new ArgumentException("填充未加入或不存在于数据库");

        boundaryIds.ForEach(ids =>
        {
            foreach (ObjectId id in ids)
            {
                if (!id.IsOk())
                    throw new ArgumentException("边界未加入或不存在于数据库");
            }
        });

        using (hatch.ForWrite())
        {
            while (hatch.NumberOfLoops > 0)
                hatch.RemoveLoopAt(0);

            if (associative != null)
                hatch.Associative = associative == true;

            var isOutermost = true;

            boundaryIds.ForEach(ids =>
            {
                try
                {
                    // 要先添加最外面的边界
                    if (isOutermost)
                    {
                        isOutermost = false;
                        hatch.AppendLoop(HatchLoopTypes.Outermost, ids);
                    }
                    else
                    {
                        // HatchLoopTypes.External 比 HatchLoopTypes.Default 似乎更不容易出问题
                        hatch.AppendLoop(HatchLoopTypes.External, ids);
                    }
                }
                catch (Exception ex)
                {
                    Env.Editor.WriteMessage(Environment.NewLine + "发生错误，传入的边界不符合要求，请核实传入的边界是否为封闭的");
                    throw new Exception(ex.Message);
                }
            });

            hatch.EvaluateHatch(true);
        }
    }

    #region 私有方法

    /// <summary>
    /// 处理边界多段线
    /// </summary>
    /// <param name="loop">填充边界</param>
    /// <param name="objColl">收集边界图元</param>
    private static void HatchLoopIsPolyline(HatchLoop loop, DBObjectCollection objColl)
    {
        // 判断为圆形:
        // 上下两个圆弧,然后填充,就会生成此种填充
        // 顶点数是3,凸度是半圆,两个半圆就是一个圆形
        if (loop.Polyline.Count == 3 && loop.Polyline[0].Bulge == 1 && loop.Polyline[1].Bulge == 1 ||
            loop.Polyline.Count == 3 && loop.Polyline[0].Bulge == -1 && loop.Polyline[1].Bulge == -1)
        {
            var center = loop.Polyline[0].Vertex.GetMidPointTo(loop.Polyline[1].Vertex);
            var radius = loop.Polyline[0].Vertex.GetDistanceTo(loop.Polyline[1].Vertex) * 0.5;
            var circle = new Circle(center.Point3d(), Vector3d.ZAxis, radius);
            objColl.Add(circle);
        }
        else
        {
            // 遍历多段线信息
            var bvc = loop.Polyline;
            var pl = new Polyline();
            pl.SetDatabaseDefaults();
            for (var j = 0; j < bvc.Count; j++)
            {
                var bvw = new BulgeVertexWidth(bvc[j]);
                pl.AddVertexAt(j, bvw.Vertex, bvw.Bulge, bvw.StartWidth, bvw.EndWidth);
            }

            objColl.Add(pl);
        }
    }

    /// <summary>
    /// 两个圆弧组成圆形
    /// </summary>
    /// <param name="loop">填充边界</param>
    /// <param name="objColl">收集边界图元</param>
    // ReSharper disable once UnusedParameter.Local
    private static Circle? TwoArcFormOneCircle(HatchLoop loop, DBObjectCollection objColl)
    {
        if (loop.Curves.Count != 2)
        {
            throw new ArgumentException("边界非多段线,而且点数!=2,点数为:" + nameof(loop.Curves.Count) +
                                        ";两个矩形交集的时候会出现此情况.");
        }

        Circle? circle = null;

        // 判断为圆形:
        // 用一条(不是两条)多段线画出两条圆弧为正圆,就会生成此种填充
        // 边界为曲线,数量为2,可能是两个半圆曲线,如果是,就加入圆形数据中

        // 第一段
        var getCurves1Pts = loop.Curves[0].GetSamplePoints(3); // 曲线取样点分两份(3点)
        var mid1Pt = getCurves1Pts[1]; // 腰点
        var bulge1 = loop.Curves[0].StartPoint.GetArcBulge(mid1Pt, loop.Curves[0].EndPoint);

        // 第二段
        var getCurves2Pts = loop.Curves[1].GetSamplePoints(3);
        var mid2Pt = getCurves2Pts[1];
        var bulge2 = loop.Curves[1].StartPoint.GetArcBulge(mid2Pt, loop.Curves[1].EndPoint);

        // 第一段上弧&&第二段反弧 || 第一段反弧&&第二段上弧
        if (bulge1 == -1 && bulge2 == -1 || bulge1 == 1 && bulge2 == 1)
        {
            var center = loop.Curves[0].StartPoint.GetMidPointTo(loop.Curves[1].StartPoint);
            var radius = loop.Curves[0].StartPoint.GetDistanceTo(loop.Curves[1].StartPoint) * 0.5;
            circle = new Circle(center.Point3d(), Vector3d.ZAxis, radius);
        }

        return circle;
    }

    /// <summary>
    /// 处理边界曲线
    /// </summary>
    /// <param name="loop">填充边界</param>
    /// <param name="objColl">收集边界图元</param>
    private static void HatchLoopIsCurve2d(HatchLoop loop, DBObjectCollection objColl)
    {
        var pLineCount = 0; //记录多段线数量
        var curveIsClosed = 0; // 取每一段曲线,曲线可能是直线来的,但是圆弧会按照顶点来分段
        var newPl = true; // 是否开始新的多段线(一个边界中可能有多条多段线)
        var firstIsPl = false; //遍历边界的第一个子段为多段线(遍历时不一定从多段线的首段开始)
        List<BulgeVertexWidth>? polyLineVertexes = null;
        List<List<BulgeVertexWidth>> polyLineData = [];

        // 遍历边界的多个子段
        foreach (Curve2d curve in loop.Curves)
        {
            // 计数用于实现闭合
            curveIsClosed++;

            if (curve is CircularArc2d or LineSegment2d)
            {
                var pts = curve.GetSamplePoints(3);
                var midPt = pts[1];

                // 判断为多段线圆:
                // 首尾相同,就是圆形
                if (curve.StartPoint.IsEqualTo(curve.EndPoint, new Tolerance(1e-6, 1e-6)))
                {
                    // 获取起点,然后采样三点,中间就是对称点(直径点)
                    var center = curve.StartPoint.GetMidPointTo(midPt);
                    var radius = curve.StartPoint.GetDistanceTo(midPt) * 0.5;
                    var circle = new Circle(center.Point3d(), Vector3d.ZAxis, radius);
                    objColl.Add(circle);
                    // 添加在中部的多段线末尾点
                    if (curveIsClosed > 1 && !newPl)
                        polyLineVertexes?.Add(new BulgeVertexWidth(curve.StartPoint));
                    // 开始新的多段线
                    newPl = true;
                    continue;
                }

                if (curveIsClosed == 1)
                    firstIsPl = true;

                if (newPl)
                {
                    polyLineVertexes = [];
                    polyLineData.Add(polyLineVertexes);
                    newPl = false;
                    pLineCount++;
                }

                // 判断为多段线,圆弧或直线:
                var bulge = curve.StartPoint.GetArcBulge(midPt, curve.EndPoint);
                polyLineVertexes?.Add(new BulgeVertexWidth(curve.StartPoint, bulge));

                // 末尾点,不闭合的情况下就要获取这个
                if (curveIsClosed == loop.Curves.Count)
                {
                    if (firstIsPl && pLineCount > 1)
                    {
                        // 连接首尾多段线
                        polyLineData[0].ForEach(bvw => polyLineData[^1].Add(bvw));
                        polyLineData.RemoveAt(0);
                    }
                    else
                        polyLineVertexes?.Add(new BulgeVertexWidth(curve.EndPoint));
                }

                continue;
            }

            // 判断为样条曲线:
            if (curve is NurbCurve2d spl)
                objColl.Add(spl.ToCurve());

            // 判断为椭圆:
            if (curve is EllipticalArc2d eArc2d)
            {
                var startParam = eArc2d.IsClockWise ? -eArc2d.EndAngle : eArc2d.StartAngle;
                var endParam = eArc2d.IsClockWise ? -eArc2d.StartAngle : eArc2d.EndAngle;
                var ellipse = new Ellipse(eArc2d.Center.Point3d(), Vector3d.ZAxis,
                    eArc2d.MajorAxis.Convert3d() * eArc2d.MajorRadius,
                    eArc2d.MinorRadius / eArc2d.MajorRadius,
                    Math.Atan2(Math.Sin(startParam) * eArc2d.MinorRadius,
                        Math.Cos(startParam) * eArc2d.MajorRadius),
                    Math.Atan2(Math.Sin(endParam) * eArc2d.MinorRadius,
                        Math.Cos(endParam) * eArc2d.MajorRadius));
                objColl.Add(ellipse);
            }

            // 添加在中部的多段线末尾点
            if (curveIsClosed > 1 && !newPl)
                polyLineVertexes?.Add(new BulgeVertexWidth(curve.StartPoint));
            // 开始新的多段线
            newPl = true;
        }

        // 生成多段线
        polyLineData.ForEach(list =>
        {
            if (list.Count == 0) return;
            var pl = new Polyline();
            pl.SetDatabaseDefaults();
            for (var j = 0; j < list.Count; j++)
            {
                pl.AddVertexAt(j, list[j].Vertex, list[j].Bulge, list[j].StartWidth, list[j].EndWidth);
            }

            objColl.Add(pl);
        });
    }

    #endregion
}