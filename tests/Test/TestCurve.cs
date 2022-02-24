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

    }
}
