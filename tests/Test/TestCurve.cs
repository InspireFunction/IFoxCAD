using System.Security.Policy;

namespace Test
{
    public class TestGraph
    {
        [CommandMethod("testpointindict")]
        public void TestPointInDict()
        {
            var pt1 = new Point3d(0.0255, 0.452, 0);
            var pt2 = new Point3d(0.0255001, 0.452003, 0);
            var pt3 = new Point3d(0.0255002, 0.4520001, 0);
            var pt4 = new Point3d(0.0255450, 0.45287893, 0);
            var pt5 = new Point3d(0.02554935, 0.452092375, 0);
            var dict = new Dictionary<Point3d, int>();
            dict.Add(pt1, 1);
            dict.Add(pt2, 2);
            dict.Add(pt3, 3);
            dict.Add(pt4, 4);
            dict.Add(pt5, 5);
            Env.Print(dict[pt1]);
        }

        [CommandMethod("testgraph")]
        public void TestGraph1()
        {
            using var tr = new DBTrans();
            var ents = Env.Editor.SSGet()?.Value?.GetEntities<Curve>();
            if (ents == null)
                return;
            Tools.TestTimes2(1, "new", () => {
                
           
            var res = ents.GetAllCycle();

                //res.ForEach((i, t) => t.ForWrite(e => e.ColorIndex = i + 1));
                Env.Print(res.Count());
            tr.CurrentSpace.AddEntity(res); 
            });

        }

        [CommandMethod("testgraphspeed")]
        public void TestGraphspeed()
        {
            using var tr = new DBTrans();
            var ents = Env.Editor.SSGet()?.Value?.GetEntities<Curve>();
            if (ents == null)
                return;


            var graph = new IFoxCAD.Cad.Graph(); // 为了调试先把图的访问改为internal

            foreach (var curve in ents)
            {

                graph.AddEdge(curve.GetGeCurve());


            }
            //新建 dfs
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
            //res.ForEach((i, t) => t.ForWrite(e => e.ColorIndex = i + 1));

            //tr.CurrentSpace.AddEntity(res);

        }
    }




    public class TestCurve
    {
        [CommandMethod("testbreakcurve")]
        public void TestBreakCurve()
        {
            using var tr = new DBTrans();
            var ents = Env.Editor.SSGet().Value.GetEntities<Curve>();
            var tt = CurveEx.BreakCurve(ents.ToList());
            tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            tr.CurrentSpace.AddEntity(tt);
        }

        [CommandMethod("testCurveCurveIntersector3d")]
        public void TestCurveCurveIntersector3d()
        {
            using var tr = new DBTrans();
            var ents = Env.Editor.SSGet().Value.GetEntities<Curve>()
                .Select(e => e.ToCompositeCurve3d()).ToList();

            var cci3d = new CurveCurveIntersector3d();


            for (int i = 0; i < ents.Count; i++)
            {
                var gc1 = ents[i];
                var int1 = gc1.GetInterval();
                //var pars1 = paramss[i];
                for (int j = i; j < ents.Count; j++)
                {
                    var gc2 = ents[j];
                    //var pars2 = paramss[j];
                    var int2 = gc2.GetInterval();
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
                        //var a = cci3d.GetOverlapRanges(k);
                        //var b = cci3d.IsTangential(k);
                        //var c = cci3d.IsTransversal(k);
                        //var d = cci3d.OverlapCount();
                        //var e = cci3d.OverlapDirection();
                        var pt = cci3d.GetIntersectionParameters(k);
                        var pts = cci3d.GetIntersectionPoint(k);
                        Env.Print(pts);
                    }



                }

            }
            // var tt = CurveEx.Topo(ents.ToList());
            //tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            //tr.CurrentSpace.AddEntity(tt);
        }
    }
}