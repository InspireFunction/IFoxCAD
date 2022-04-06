using Autodesk.AutoCAD.BoundaryRepresentation;

namespace Test
{
    public class TestCurve
    {
        [CommandMethod("testbreakcurve")]
        public void TestBreakCurve()
        {
            var ents = Env.Editor.SSGet().Value.GetEntities<Curve>();
            var tt = CurveEx.BreakCurve(ents.ToList());
            using var tr = new DBTrans();
            tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            tr.CurrentSpace.AddEntity(tt);
        }

        [CommandMethod("testCurveCurveIntersector3d")]
        public void TestCurveCurveIntersector3d()
        {
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
            //    var tt = CurveEx.Topo(ents.ToList());
            //using var tr = new DBTrans();
            //tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            //tr.CurrentSpace.AddEntity(tt);
        }




        [CommandMethod("testtopo")]
        public void TestToPo()
        {
            var ents = Env.Editor.SSGet().Value?.GetEntities<Curve>();
            if (ents == null)
                return;
            var tt = CurveEx.Topo(ents.ToList());
            using var tr = new DBTrans();
            tt.ForEach((i,t) => t.ForWrite(e => e.ColorIndex = i));
            tr.CurrentSpace.AddEntity(tt);
        }

        [CommandMethod("testGetEdgesAndnewCurves")]
        public void TestGetEdgesAndnewCurves()
        {
            var curves = Env.Editor.SSGet().Value?.GetEntities<Curve>().ToList();
            if (curves == null)
                return;
            var edges = new List<IFoxCAD.Cad.Edge>();
            var closedCurve3d = new List<CompositeCurve3d>();
            var topo = new Topo(curves);
            topo.GetEdgesAndnewCurves(edges, closedCurve3d);

            using var tr = new DBTrans();

            for (int i = 0; i < edges.Count; i++)
            {
                var ent = edges[i].GeCurve3d.ToCurve();
                ent.ColorIndex = i;
                tr.CurrentSpace.AddEntity(ent);
            }

            //Env.Print("");
        }
    }
}