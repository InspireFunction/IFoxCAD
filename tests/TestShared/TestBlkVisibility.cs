using System.Windows;

namespace TestAcad2025;

public static class TestBlkVisibility
{
    [CommandMethod(nameof(TestBlkVisibility))]
    public static void Main()
    {
        var r1 = Env.Editor.GetEntity("\n选择块参照");
        if (r1.Status != PromptStatus.OK)
            return;
        using var tr = DBTrans.Create();
        if (tr.GetObject(r1.ObjectId) is not BlockReference { IsDynamicBlock: true } brf)
            return;
        var info = brf.GetVisibilityInfo();
        if (info is null)
        {
            return;
        }
        MessageBox.Show(
            $"块{brf.Name}的可见性名字：{info.PropertyName}，参数：{string.Join(", ", info.AllowedValues.ToArray())}。是否可见？{info.Has}");
    }
}