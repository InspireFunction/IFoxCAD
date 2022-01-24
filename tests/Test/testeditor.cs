namespace test;

public class testeditor
{
    [CommandMethod("tested")]
    public void tested()
    {
        var pts = new List<Point2d>
        {
            new Point2d(0,0),
            new Point2d(0,1),
            new Point2d(1,1),
            new Point2d(1,0)
        };
        var res = EditorEx.GetLines(pts, false);
        var res1 = EditorEx.GetLines(pts, true);
        var res2 = pts.Select(pt => new TypedValue((int)LispDataType.Point2d, pt)).ToList();

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        var pt = ed.GetPoint("qudiam", new Point3d(0, 0, 0));
        var d = ed.GetDouble("qudoule");
        var i = ed.GetInteger("quint");
        var s = ed.GetString("qustr");
        Env.Editor.WriteMessage("");
    }
    [CommandMethod("testzoom")]
    public void testzoom()
    {
        using var tr = new DBTrans();
        var res = Env.Editor.GetEntity("\npick ent:");
        if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
        {
            Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>());
        }


    }
    [CommandMethod("testzoomextent")]
    public void testzoomextent()
    {
        //using var tr = new DBTrans();
        //var res = Env.Editor.GetEntity("\npick ent:");
        //if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
        //{
        //    Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>());
        //}

        Env.Editor.ZoomExtents();
    }
}
