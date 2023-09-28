namespace Test;

public class TestGraph
{
    [CommandMethod(nameof(Test_PointInDict))]
    public void Test_PointInDict()
    {
        var pt1 = new Point3d(0.0255, 0.452, 0);
        var pt2 = new Point3d(0.0255001, 0.452003, 0);
        var pt3 = new Point3d(0.0255002, 0.4520001, 0);
        var pt4 = new Point3d(0.0255450, 0.45287893, 0);
        var pt5 = new Point3d(0.02554935, 0.452092375, 0);
        var dict = new Dictionary<Point3d, int>
        {
            { pt1, 1 },
            { pt2, 2 },
            { pt3, 3 },
            { pt4, 4 },
            { pt5, 5 }
        };
        Env.Print(dict[pt1]);
    }

    [CommandMethod(nameof(Test_Graph1))]
    public void Test_Graph1()
    {
        using DBTrans tr = new();
        var ents = Env.Editor.SSGet()?.Value?.GetEntities<Curve>();
        if (ents == null)
            return;
        Tools.TestTimes2(1, "new", () => {
            var res = ents!.GetAllCycle();

            // res.ForEach((i, t) => t.ForWrite(e => e.ColorIndex = i + 1));
            Env.Print(res.Count());
            tr.CurrentSpace.AddEntity(res);
        });
    }

    [CommandMethod(nameof(Test_Graphspeed))]
    public void Test_Graphspeed()
    {
        using DBTrans tr = new();
        var ents = Env.Editor.SSGet()?.Value?.GetEntities<Curve>();
        if (ents == null)
            return;

        var graph = new IFoxCAD.Cad.Graph(); // 为了调试先把图的访问改为internal
        foreach (var curve in ents)
        {
            graph.AddEdge(curve!.GetGeCurve());
        }

        // 新建 dfs
        var dfs = new DepthFirst();
#if true
        Tools.TestTimes2(100, "new", () => {
            // 查询全部的 闭合环
            dfs.FindAll(graph);
        });
        Tools.TestTimes2(1000, "new", () => {
            // 查询全部的 闭合环
            dfs.FindAll(graph);
        });
#else
        Tools.TestTimes2(100, "old", () => {
            // 查询全部的 闭合环
            dfs.FindAll(graph);
        });
        Tools.TestTimes2(1000, "old", () => {
            // 查询全部的 闭合环
            dfs.FindAll(graph);
        });
#endif
        // res.ForEach((i, t) => t.ForWrite(e => e.ColorIndex = i + 1));

        // tr.CurrentSpace.AddEntity(res);
    }
}



public partial class TestCurve
{
    [CommandMethod(nameof(Test_CurveExtend))]
    public void Test_CurveExtend()
    {
        using var tr =  new DBTrans();
        var ent = Env.Editor.GetEntity("pick curve").ObjectId.GetObject<Entity>();
        if (ent is Curve curve)
            curve.ForWrite(e => e.Extend(e.EndParam + 1));

    }


    private Arc ToArc1(CircularArc2d a2d)
    {
        double startangle, endangle;
        double refangle = a2d.ReferenceVector.Angle;

        if (a2d.IsClockWise)
        {
            startangle = -a2d.EndAngle + refangle;
            endangle = -a2d.StartAngle + refangle;
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


    [CommandMethod(nameof(Test_Curve_ToArc))]
    public void Test_Curve_ToArc()
    {
        using var tr = new DBTrans();
        var gearc = new CircularArc2d(new Point2d(0,0),new Point2d(0.5,0.5),new Point2d(1,0));
        var dbarc =  gearc.ToArc();
        var dbarc1 = ToArc1(gearc);
        dbarc.ColorIndex = 1;
        tr.CurrentSpace.AddEntity(dbarc);
        dbarc1.ColorIndex = 2;
        tr.CurrentSpace.AddEntity(dbarc1);

        var gearc3 = new CircularArc3d(new(0,0,0),new(0.5,0.5,0),new Point3d(1,0,0));
        var dbarc3 = (Arc)Curve.CreateFromGeCurve(gearc3);
        dbarc3.ColorIndex = 3;
        tr.CurrentSpace.AddEntity(dbarc3);



        Polyline pl0 = new();//创建有圆弧的多段线

        pl0.AddVertexAt(0, new(-520, 200), -0.74, 0, 0);
        pl0.AddVertexAt(1, new(-100, 140), 0.52, 0, 0);
        pl0.AddVertexAt(2, new(16, -120), -0.27, 0, 0);
        pl0.AddVertexAt(3, new(400, -130), 0.75, 0, 0);
        pl0.AddVertexAt(4, new(450, 200), -0.69, 0, 0);
        tr.CurrentSpace.AddEntity(pl0);


        for (int FFF = 0; FFF < pl0.NumberOfVertices; FFF++)
        {
            if (pl0.GetSegmentType(FFF) == SegmentType.Arc)
            {
                var bulge = pl0.GetBulgeAt(FFF);
                
                //将  CircularArc2d 转为Arc 颜色为红
                CircularArc2d arc2d = pl0.GetArcSegment2dAt(FFF);
                
                Arc arc = arc2d.ToArc();
                if (bulge < 0) arc.ReverseCurve();
                arc.ColorIndex = 1;
                tr.CurrentSpace.AddEntity(arc);
                Env.Printl($"arc的ge:ReferenceVector：{MathEx.ConvertRadToDeg(arc2d.ReferenceVector.Angle)}");
                Env.Printl($"arc的ge:顺时针：{arc2d.IsClockWise}");
                Env.Printl($"arc的ge:起点角度：{MathEx.ConvertRadToDeg(arc2d.StartAngle)},终点角度：{MathEx.ConvertRadToDeg(arc2d.EndAngle)}");
                Env.Printl($"arc的db:起点角度：{MathEx.ConvertRadToDeg(arc.StartAngle)},终点角度：{MathEx.ConvertRadToDeg(arc.EndAngle)}");

                //将  CircularArc2d 转为Arc 颜色为黄
                CircularArc2d arc2d1 = pl0.GetArcSegment2dAt(FFF);
                Arc arc1 = ToArc1(arc2d1);
                if (bulge < 0) arc1.ReverseCurve();
                arc1.ColorIndex = 2;
                tr.CurrentSpace.AddEntity(arc1);
                Env.Printl($"arc1的ge:ReferenceVector：{MathEx.ConvertRadToDeg(arc2d1.ReferenceVector.Angle)}");
                Env.Printl($"arc的ge:顺时针：{arc2d1.IsClockWise}");
                Env.Printl($"arc1的ge:起点角度：{MathEx.ConvertRadToDeg(arc2d1.StartAngle)} ,终点角度： {MathEx.ConvertRadToDeg(arc2d1.EndAngle)}");
                Env.Printl($"arc1的db:起点角度：{MathEx.ConvertRadToDeg(arc1.StartAngle)} ,终点角度： {MathEx.ConvertRadToDeg(arc1.EndAngle)}");
               
                //将 CircularArc3d 转为Arc 颜色为黄色
                CircularArc3d arc3d = pl0.GetArcSegmentAt(FFF);
                Arc arc2 = arc3d.ToArc();
                
                arc2.ColorIndex = 3;
                tr.CurrentSpace.AddEntity(arc2);
                Env.Printl($"arc2的ge:ReferenceVector：{MathEx.ConvertRadToDeg(arc3d.ReferenceVector.AngleOnPlane(new Plane()))}");
                Env.Printl($"arc2的ge:起点角度：{MathEx.ConvertRadToDeg(arc3d.StartAngle)} ,终点角度： {MathEx.ConvertRadToDeg(arc3d.EndAngle)}");
                Env.Printl($"arc2的db:起点角度：{MathEx.ConvertRadToDeg(arc2.StartAngle)}  ,终点角度：  {MathEx.ConvertRadToDeg(arc2.EndAngle)}");

/*


arc的ge: ReferenceVector：154.872779886857
arc的ge: 顺时针：True
arc的ge:起点角度：0,终点角度：146.005764482025
arc的db: 起点角度：334.872779886857,终点角度：120.878544368882
arc1的ge: ReferenceVector：154.872779886857
arc的ge: 顺时针：True
arc1的ge:起点角度：0 ,终点角度： 146.005764482025
arc1的db: 起点角度：334.872779886857 ,终点角度： 120.878544368882
arc2的ge: ReferenceVector：154.872779886857
arc2的ge: 起点角度：0 ,终点角度： 146.005764482025
arc2的db: 起点角度：25.1272201131434  ,终点角度：  171.132984595169


arc的ge: ReferenceVector：149.095360016814
arc的ge: 顺时针：False
arc的ge:起点角度：0,终点角度：109.897726505109
arc的db: 起点角度：149.095360016814,终点角度：258.993086521922
arc1的ge: ReferenceVector：149.095360016814
arc的ge: 顺时针：False
arc1的ge:起点角度：0 ,终点角度： 109.897726505109
arc1的db: 起点角度：149.095360016814 ,终点角度： 258.993086521922
arc2的ge: ReferenceVector：149.095360016814
arc2的ge: 起点角度：0 ,终点角度： 109.897726505109
arc2的db: 起点角度：149.095360016814  ,终点角度：  258.993086521922


arc的ge: ReferenceVector：118.727409809308
arc的ge: 顺时针：True
arc的ge:起点角度：0,终点角度：60.4383004893619
arc的db: 起点角度：298.727409809308,终点角度：359.16571029867
arc1的ge: ReferenceVector：118.727409809308
arc的ge: 顺时针：True
arc1的ge:起点角度：0 ,终点角度： 60.4383004893619
arc1的db: 起点角度：298.727409809308 ,终点角度： 359.16571029867
arc2的ge: ReferenceVector：118.727409809308
arc2的ge: 起点角度：0 ,终点角度： 60.4383004893619
arc2的db: 起点角度：61.2725901906918  ,终点角度：  121.710890680054


arc的ge: ReferenceVector：277.644556524148
arc的ge: 顺时针：False
arc的ge:起点角度：0,终点角度：147.479590583376
arc的db: 起点角度：277.644556524148,终点角度：65.124147107524
arc1的ge: ReferenceVector：277.644556524148
arc的ge: 顺时针：False
arc1的ge:起点角度：0 ,终点角度： 147.479590583376
arc1的db: 起点角度：277.644556524148 ,终点角度： 65.124147107524
arc2的ge: ReferenceVector：277.644556524148
arc2的ge: 起点角度：0 ,终点角度： 147.479590583376
arc2的db: 起点角度：277.644556524148  ,终点角度：  65.124147107524






*/
            }
        }

    }
}


public partial class TestCurve
{
    [CommandMethod(nameof(Test_BreakCurve))]
    public void Test_BreakCurve()
    {
        using DBTrans tr = new();
        var ents = Env.Editor.SSGet()?.Value.GetEntities<Curve>();
        if (ents is null)
            return;
        var tt = CurveEx.BreakCurve(ents.ToList()!);
        tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
        tr.CurrentSpace.AddEntity(tt);
    }

    [CommandMethod(nameof(Test_CurveCurveIntersector3d))]
    public void Test_CurveCurveIntersector3d()
    {
        using DBTrans tr = new();
        var ents = Env.Editor.SSGet()?
                   .Value.GetEntities<Curve>()
                   .Select(e => e?.ToCompositeCurve3d()).ToList();
        if (ents == null)
            return;

        var cci3d = new CurveCurveIntersector3d();
        for (int i = 0; i < ents.Count; i++)
        {
            var gc1 = ents[i];
            var int1 = gc1?.GetInterval();
            // var pars1 = paramss[i];
            for (int j = i; j < ents.Count; j++)
            {
                var gc2 = ents[j];
                // var pars2 = paramss[j];
                var int2 = gc2?.GetInterval();
                cci3d.Set(gc1, gc2, int1, int2, Vector3d.ZAxis);
                var d = cci3d.OverlapCount();
                var a = cci3d.GetIntersectionRanges();
                Env.Print($"{a[0].LowerBound}-{a[0].UpperBound} and {a[1].LowerBound}-{a[1].UpperBound}");
                for (int m = 0; m < d; m++)
                {
                    var b = cci3d.GetOverlapRanges(m);
                    Env.Print($"{b[0].LowerBound}-{b[0].UpperBound} and {b[1].LowerBound}-{b[1].UpperBound}");
                }

                for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    // var a = cci3d.GetOverlapRanges(k);
                    // var b = cci3d.IsTangential(k);
                    // var c = cci3d.IsTransversal(k);
                    // var d = cci3d.OverlapCount();
                    // var e = cci3d.OverlapDirection();
                    var pt = cci3d.GetIntersectionParameters(k);
                    var pts = cci3d.GetIntersectionPoint(k);
                    Env.Print(pts);
                }
            }
        }
        // var tt = CurveEx.Topo(ents.ToList());
        // tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
        // tr.CurrentSpace.AddEntity(tt);
    }
}