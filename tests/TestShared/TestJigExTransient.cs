#if !ac2008
namespace Test;

public partial class Test
{
    [CommandMethod(nameof(TestJigExTransient))]
    public void TestJigExTransient()
    {
        // 先取1点,建2个圆
        var getpt = Env.Editor.GetPoint("\n选择点");
        if (getpt.Status != PromptStatus.OK)
            return;
        var pt = getpt.Value.Ucs2Wcs();

        var c1 = new Circle(pt, Vector3d.ZAxis, 100);
        var c2 = new Circle(pt.Polar(0, 300), Vector3d.ZAxis, 100);

        // 创建瞬态容器
        using JigExTransient jet = new();

        // 将c1以默认模式,c2以亮显模式加到瞬态容器,即在图纸上显示
        jet.Add(c1);
        jet.Add(c2, Acgi.TransientDrawingMode.Highlight);

        // 再取一点,再建一个圆c3
        var r2 = Env.Editor.GetPoint("\n选择下一点");
        if (r2.Status != PromptStatus.OK)
            return;
        var pt2 = r2.Value.Ucs2Wcs();

        // 将c1从瞬态容器中移除,将c2修改颜色,c3加入瞬态容器
        jet.Remove(c1);

        c2.ColorIndex = 1;
        var c3 = new Circle(pt2, Vector3d.ZAxis, 150);
        jet.Add(c3);

        // 由于c2进行了修改,所以需要更新,
        // 可以单个更新或更新整个瞬态容器
        jet.Update(c2);
        // jet.UpdateAll();

        var r4 = Env.Editor.GetPoint("\n此拾取无意义,仅为了暂停查看");

        // 加到图纸中,为测试瞬态容器可以自行dispose消失,所以未全部加入
        using DBTrans tr = new();
        tr.CurrentSpace.AddEntity(c3);

        // 若想将容器中所有图元全部加入提供了Entities属性
        // tr.CurrentSpace.AddEntity(jet.Entities);
    }

    [CommandMethod(nameof(TestJigExTransentDim))]
    public static void TestJigExTransentDim()
    {
        using DBTrans tr = new();
        Editor ed = tr.Editor!;
        PromptPointOptions ppo = new("")
        {
            AppendKeywordsToMessage = false,
        };
        List<Point3d> pts = new();
        for (int i = 0; i < 3; i++)
        {
            ppo.Message = $"\n选择标注点{i + 1}";
            var ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return;
            pts.Add(ppr.Value);
        }

        using RotatedDimension dimension = new(
            rotation: 0,
            line1Point: pts[0],
            line2Point: pts[1],
            dimensionLinePoint: pts[2],
            dimensionText: "<>",
            tr.Database.Dimstyle);

        using JigExTransient jet = new();
        jet.Add(dimension);
        jet.UpdateAll();

        var r4 = Env.Editor.GetPoint("\n此拾取无意义,仅为了暂停查看");

        tr.CurrentSpace.AddEntity(dimension);
    }
}
#endif