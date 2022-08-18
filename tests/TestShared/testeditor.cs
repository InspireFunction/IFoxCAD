namespace Test;

public class Testeditor
{
    [CommandMethod("tested")]
    public void Tested()
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

        Editor ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        var pt = ed.GetPoint("qudiam", new Point3d(0, 0, 0));
        var d = ed.GetDouble("qudoule");
        var i = ed.GetInteger("quint");
        var s = ed.GetString("qustr");
        Env.Editor.WriteMessage("");
    }
    [CommandMethod("testzoom")]
    public void Testzoom()
    {
        using var tr = new DBTrans();
        var res = Env.Editor.GetEntity("\npick ent:");
        if (res.Status == PromptStatus.OK)
            Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>()!);
    }
    [CommandMethod("testzoomextent")]
    public void Testzoomextent()
    {
        //using var tr = new DBTrans();
        //var res = Env.Editor.GetEntity("\npick ent:");
        //if (res.Status == PromptStatus.OK)
        //{
        //    Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>());
        //}

        Env.Editor.ZoomExtents();
    }

    [CommandMethod("testssget")]
    public void Testssget()
    {
        var action_a = () => { Env.Print("this is a"); };
        var action_b = () => { Env.Print("this is b"); };

        var keyword = new Dictionary<string, Action>
        {
            { "A", action_a },
            { "B", action_b }
        };

        var ss = Env.Editor.SSGet( ":S", messages: new string[2] { "get", "del" },
                                         keywords: keyword);
        Env.Print(ss!);
    }
}
