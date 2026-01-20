using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;

public class SimpleHatchLoopInspector
{
    [CommandMethod("ListHatchLoops")]
    public void ListHatchLoops()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;

        // 选择填充对象
        PromptEntityOptions peo = new PromptEntityOptions("\n选择填充对象: ");
        PromptEntityResult per = ed.GetEntity(peo);

        if (per.Status != PromptStatus.OK) return;

        using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
        {
            using var ent = tr.GetObject(per.ObjectId, OpenMode.ForRead);

            if (ent is Hatch hatch)
            {
                ed.WriteMessage($"\n填充边界数量: {hatch.NumberOfLoops}\n");

                // 遍历所有边界
                for (int i = 0; i < hatch.NumberOfLoops; i++)
                {
                    HatchLoop loop = hatch.GetLoopAt(i);
                    HatchLoopTypes loopType = loop.LoopType;

                    ed.WriteMessage($"\n边界 #{i + 1}:");
                    ed.WriteMessage($" 类型值 = {(int)loopType}");
                    ed.WriteMessage($" ({loopType})");

                    // 判断具体类型
                    if ((loopType & HatchLoopTypes.Outermost) != 0)
                        ed.WriteMessage(" [最外层边界]");
                    if ((loopType & HatchLoopTypes.Default) != 0)
                        ed.WriteMessage(" [默认/岛屿边界]");
                    if ((loopType & HatchLoopTypes.External) != 0)
                        ed.WriteMessage(" [外部边界]");
                    if ((loopType & HatchLoopTypes.Polyline) != 0)
                        ed.WriteMessage(" (多段线)");

                    ed.WriteMessage($"\n  曲线数: {loop.Curves?.Count}");
                }
            }

            tr.Commit();
        }
    }

    [CommandMethod("ShowLoopTypes")]
    public void ShowLoopTypes()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        ed.WriteMessage("\n=== 边界类型常量 ===\n");
        ed.WriteMessage($"Outermost:  {(int)HatchLoopTypes.Outermost} (最外层边界)\n");
        ed.WriteMessage($"Default:    {(int)HatchLoopTypes.Default} (默认/岛屿边界)\n");
        ed.WriteMessage($"External:   {(int)HatchLoopTypes.External} (外部边界)\n");
        ed.WriteMessage($"Polyline:   {(int)HatchLoopTypes.Polyline} (多段线)\n");
        ed.WriteMessage($"Derived:    {(int)HatchLoopTypes.Derived} (派生)\n");
        ed.WriteMessage($"Textbox:    {(int)HatchLoopTypes.Textbox} (文本框)\n");

        ed.WriteMessage("\n组合示例:\n");
        ed.WriteMessage($"Outermost + Polyline: {(int)(HatchLoopTypes.Outermost | HatchLoopTypes.Polyline)}\n");
        ed.WriteMessage($"Default + Polyline:   {(int)(HatchLoopTypes.Default | HatchLoopTypes.Polyline)}\n");
    }
}