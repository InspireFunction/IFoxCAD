namespace Gstar_IMEFilter;

public class DocReactor
{
    internal static void IntialReactor()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        dm.DocumentCreated += DocumentCreated;

        foreach (Document item in dm)
            AddReactor(item);
    }

    internal static void RemoveReactor()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        foreach (Document item in dm)
            RemoveReactor(item);

        dm.DocumentCreated -= DocumentCreated;
    }

    static void DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        AddReactor(e.Document);
    }

    private static void RemoveReactor(Document doc)
    {
        if (doc == null)
            return;

        doc.CommandWillStart -= CommandWillStart;
    }

    private static void AddReactor(Document doc)
    {
        if (doc == null)
            return;

        doc.CommandWillStart += CommandWillStart;
    }

    private static void CommandWillStart(object sender, CommandEventArgs e)
    {
        if (!Settings.Use || ((Document)sender).Editor.IsQuiescentForTransparentCommand())
            return;

        var gName = e.GlobalCommandName;
        if (gName == "-HATCHEDIT" || gName == "UNDO")
            return;

        //此函数将焦点设置为视图：
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
    }
}