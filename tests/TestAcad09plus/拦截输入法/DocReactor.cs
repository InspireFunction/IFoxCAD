namespace Gstar_IMEFilter;

public class DocReactor
{
    internal static void IntialReactor()
    {
        try
        {
            var dm = Acap.DocumentManager;
            // 现有文档
            foreach (Document doc in dm)
            {
                if (doc != null && !doc.IsDisposed)
                    doc.CommandWillStart += CommandWillStart;
            }
            // 文档创建事件
            dm.DocumentCreated += DocumentCreated;
        }
        catch (Exception ex)
        {
            Env.Printl("※拦截输入法控制※ 初始化反应器时出错: " + ex.Message);
        }
    }

    internal static void RemoveReactor()
    {
        try
        {
            var dm = Acap.DocumentManager;
            // 现有文档
            foreach (Document doc in dm)
            {
                if (doc != null && !doc.IsDisposed)
                    doc.CommandWillStart -= CommandWillStart;
            }
            // 文档创建事件
            dm.DocumentCreated -= DocumentCreated;
        }
        catch (Exception ex)
        {
            Env.Printl("※拦截输入法控制※ 移除反应器时出错: " + ex.Message);
        }
    }

    static void DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
        try
        {
            if (e.Document != null && !e.Document.IsDisposed)
                e.Document.CommandWillStart += CommandWillStart;
        }
        catch (Exception ex)
        {
            Env.Printl("※拦截输入法控制※ 文档创建事件处理出错: " + ex.Message);
        }
    }

    static void CommandWillStart(object sender, CommandEventArgs e)
    {
        try
        {
            if (sender is not Document doc)
                return;

            if (doc.IsDisposed)
                return;

#if ac2008
            if (Settings.IMEInputSwitch == IMESwitchMode.Disable)
                return;
#else
            if (Settings.IMEInputSwitch == IMESwitchMode.Disable ||
              doc.Editor.IsQuiescentForTransparentCommand)
                return;
#endif
            var gName = e.GlobalCommandName;
            if (gName == "-HATCHEDIT" || gName == "UNDO")
                return;

            // 此函数将焦点设置为视图：
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
        catch (Exception ex)
        {
            Env.Printl("※拦截输入法控制※ 命令开始事件处理出错: " + ex.Message);
        }
    }
}