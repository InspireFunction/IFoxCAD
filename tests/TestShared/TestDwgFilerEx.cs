#if false
namespace Test;

using DxfFiler = IFoxCAD.Cad.DxfFiler;

public class CmdTestDwgFilerEx
{
    [CommandMethod(nameof(CmdTest_DwgFilerEx))]
    public static void CmdTest_DwgFilerEx()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        ed.WriteMessage("\n****测试,序列化图元");

        var ssPsr = ed.SelectImplied();// 预选
        if (ssPsr.Status != PromptStatus.OK)
        {
            ssPsr = ed.GetSelection();// 手选  这里输入al会变成all,无法删除ssget的all关键字
            if (ssPsr.Status != PromptStatus.OK)
                return;
        }

        using DBTrans tr = new();
        var ids = ssPsr.Value.GetObjectIds();
        foreach (var id in ids)
        {
            if (!id.IsOk())
                continue;
            var ent = tr.GetObject<Entity>(id, OpenMode.ForRead);
            if (ent is null)
                continue;
            var dwgFilerEx = new DwgFilerEx(ent);
            ed.WriteMessage(Environment.NewLine + dwgFilerEx.ToString());
        }
    }

    [CommandMethod(nameof(CmdTest_EntDxfout))]
    public static void CmdTest_EntDxfout()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        // 定义选择集选项
        var pso = new PromptSelectionOptions
        {
            RejectObjectsOnLockedLayers = true, // 不选择锁定图层对象
            AllowDuplicates = true, // 不允许重复选择
        };
        var ssPsr = ed.GetSelection(pso);// 手选  这里输入al会变成all,无法删除ssget的all关键字
        if (ssPsr.Status != PromptStatus.OK)
            return;

        using DBTrans tr = new();
        var ids = ssPsr.Value.GetObjectIds();
        foreach (var id in ids)
        {
            if (!id.IsOk())
                continue;
            var ent = tr.GetObject<Entity>(id, OpenMode.ForRead);
            if (ent is null)
                continue;
            // ResultBuffer rbDxf = new();
            var filer = new DxfFiler(ent.UnmanagedObject, true);/// 这里有问题
            ent.DxfOut(filer);
        }
    }


    [CommandMethod(nameof(CmdTest_TextOut))]
    public static void CmdTest_TextOut()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

#if true
        var peo1 = new PromptEntityOptions(Environment.NewLine + "点选源TCH_WIREDIM2:")
        {
            AllowObjectOnLockedLayer = false,
            AllowNone = false
        };
        var gt1 = ed.GetEntity(peo1);
        if (gt1.Status != PromptStatus.OK)
            return;
#else
        var peo2 = new PromptEntityOptions(Environment.NewLine + "点选目标TCH_WIREDIM2:")
        {
            AllowObjectOnLockedLayer = false,
            AllowNone = false
        };
        var gt2 = ed.GetEntity(peo2);
        if (gt2.Status != PromptStatus.OK)
            return;
#endif

        using DBTrans tr = new();
        var dwgFilerEx = new DwgFilerEx();
        var bText = tr.GetObject<DBText>(gt1.ObjectId, OpenMode.ForRead);
        if (bText is null)
            return;

        // DwgFilerEx.StringList[0] = "1@2@3@4@5@6@7@";
        // 复制 TCH_WIREDIM2 不行,TEXT 也不行,直接崩溃。line等线就没事
        bText.DwgOut(dwgFilerEx.DwgFiler);

        int testNum = 1 | 2 | 4 | 8;

        if ((testNum & 1) == 1)
        {
            // 错误,原地克隆也是不行的,它会生成在了模型中.
            var sIds = new List<ObjectId>
            {
                bText.ObjectId
            };
            // 克隆到目标块表内
            using ObjectIdCollection bindIds = new(sIds.ToArray());
            using IdMapping map = new();

            tr.CurrentSpace.DeepCloneEx(bindIds, map);
            var newTexts = map.GetValues().GetObject<DBText>();
            newTexts.ForEach(nText => {
                if (nText == null)
                    return;
                // 通过上面的克隆就已经在块表上面了.所以下面的设置也跟设置到已有图元上一样报错.
                nText.UpgradeOpen();
                nText.DwgIn(dwgFilerEx);
                tr.CurrentSpace.AddEntity(nText);
                nText.DowngradeOpen();
            });
        }
        if ((testNum & 2) == 2)
        {
            // 出错
            // 直接设置
            bText.DwgIn(dwgFilerEx);
        }
        if ((testNum & 4) == 4)
        {
            // 出错
            // 此时是内存中对象....
            var nText = (DBText)bText.Clone();
            nText.DwgIn(dwgFilerEx);
            tr.CurrentSpace.AddEntity(nText);
        }
        if ((testNum & 8) == 8)
        {
            // 新对象相当于克隆,是ok的
            DBText nText = new();
            nText.SetDatabaseDefaults();
            nText.DwgIn(dwgFilerEx);
            tr.CurrentSpace.AddEntity(nText);
        }
    }
}

#endif