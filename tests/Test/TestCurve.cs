namespace Test
{
    public class TestCurve
    {
        [CommandMethod("testbreakcurve")]
        public void TestBreakCurve()
        {
            var ents = Env.Editor.SSGet().Value.GetEntities<Curve>();
            var tt =  CurveEx.BreakCurve(ents.ToList());
            using var tr = new DBTrans();
            tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            tr.CurrentSpace.AddEntity(tt);
        }
        [CommandMethod("testtopo")]
        public void TestToPo()
        {
            var ents = Env.Editor.SSGet().Value.GetEntities<Curve>();
            var tt = CurveEx.Topo(ents.ToList());
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
                    cci3d.Set(gc1, gc2, int1,int2, Vector3d.ZAxis);
                    var d = cci3d.OverlapCount();
                    for (int k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                    {
                        //var a = cci3d.GetOverlapRanges(k);
                        var b = cci3d.IsTangential(k);
                        var c = cci3d.IsTransversal(k);
                        //var d = cci3d.OverlapCount();
                        var e = cci3d.OverlapDirection();
                        Env.Print("i");
                    }
                    


                }

            }
            //    var tt = CurveEx.Topo(ents.ToList());
            //using var tr = new DBTrans();
            //tt.ForEach(t => t.ForWrite(e => e.ColorIndex = 1));
            //tr.CurrentSpace.AddEntity(tt);
        }

    }
}
