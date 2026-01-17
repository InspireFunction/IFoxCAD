namespace Test;

public class Testeditor
{
    [CommandMethod(nameof(Test_Editor))]
    public void Test_Editor()
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
    [CommandMethod(nameof(Test_Zoom))]
    public void Test_Zoom()
    {
        using DBTrans tr = new();
        var res = Env.Editor.GetEntity("\npick ent:");
        if (res.Status == PromptStatus.OK)
            Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>()!);
    }
    [CommandMethod(nameof(Test_ZoomExtents))]
    public void Test_ZoomExtents()
    {
        // using DBTrans tr = new();
        // var res = Env.Editor.GetEntity("\npick ent:");
        // if (res.Status == PromptStatus.OK)
        // {
        //    Env.Editor.ZoomObject(res.ObjectId.GetObject<Entity>());
        // }

        Env.Editor.ZoomExtents();
    }

    [CommandMethod(nameof(Test_Zoom_1))]
    public void Test_Zoom_1()
    {
        Env.Editor.Zoom(new(0, 0, 0),200,200);
    }
    [CommandMethod(nameof(Test_Zoom_2))]
    public void Test_Zoom_2()
    {
        Env.Editor.ZoomWindow(new Point3d(-100,-100,0),new(100,100,0));
    }

    [CommandMethod(nameof(Test_Ssget))]
    public void Test_Ssget()
    {

        var keyword = new Dictionary<string, (string, Action)>
        {
            { "D", ("你好",  () => { Env.Print("this is c"); }) },
            { "B", ("hello", () => { Env.Print("this is b"); }) }
        };

        var ss = Env.Editor.SSGet(/*":S", */ messages: ("get", "del" ),
                                         keywords: keyword);
        Env.Print(ss!);
    }

    [CommandMethod(nameof(Test_ExportWMF), CommandFlags.Modal | CommandFlags.UsePickSet)]
    public void Test_ExportWMF()
    {
        var psr = Env.Editor.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
            psr = Env.Editor.GetSelection();// 手选
        if (psr.Status != PromptStatus.OK)
            return;

        var ids = psr.Value.GetObjectIds();
        // acad21(acad08没有)先选择再执行..会让你再选择一次
        // 而且只发生在启动cad之后第一次执行.
        Env.Editor.ComExportWMF(@"C:\Users\vic\Desktop\aaa.dwg", ids);
    }
}