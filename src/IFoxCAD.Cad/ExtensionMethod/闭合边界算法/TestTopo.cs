namespace IFoxCAD.Cad.ExtensionMethod
{
    public class CmdTestTopo
    {
        [CommandMethod("TestTopo")]
        public static void TestTopo()
        {
            using var tr = new DBTrans();
            var ents = Env.Editor.SSGet()?.Value?.GetEntities<Curve>();
            if (ents == null)
                return;
            
            Tools.TestTimes2(1, "bfs", () => {
                var curves = Topo.Create(ents.ToList()!)!;
           
            if (curves == null || !curves.Any())
                return;

            //改颜色,生成图元
            //curves.ForEach((num, cu) => cu.ForWrite(e => e.ColorIndex = num));
            tr.CurrentSpace.AddEntity(curves); 
            });
        }
    }
}