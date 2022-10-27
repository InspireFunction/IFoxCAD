namespace Gstar_IMEFilter;

public class DocReactor
{
    internal static void IntialReactor()
    {
        var dm = Acap.DocumentManager;
        // 现有文档
        foreach (Document doc in dm)
            doc.CommandWillStart += CommandWillStart;
        // 文档创建事件
        dm.DocumentCreated += DocumentCreated;
    }

    internal static void RemoveReactor()
    {
        var dm = Acap.DocumentManager;
        // 现有文档
        foreach (Document doc in dm)
            doc.CommandWillStart -= CommandWillStart;
        // 文档创建事件
        dm.DocumentCreated -= DocumentCreated;
    }

    static void DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        e.Document.CommandWillStart += CommandWillStart;
    }

    static void CommandWillStart(object sender, CommandEventArgs e)
    {
        if (!Settings.Use || ((Document)sender).Editor.IsQuiescentForTransparentCommand())
            return;

        var gName = e.GlobalCommandName;
        if (gName == "-HATCHEDIT" || gName == "UNDO")
            return;

        // 此函数将焦点设置为视图：
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
    }
}