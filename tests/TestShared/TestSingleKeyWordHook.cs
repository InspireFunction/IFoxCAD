namespace TestShared;
public static class TestSingleKeyWordHook
{
    [CommandMethod(nameof(TestSingleKeyWordHookDemo))]
    public static void TestSingleKeyWordHookDemo()
    {
        var line1 = new Line(Point3d.Origin, new Point3d(100, 100, 0));
        line1.SetDatabaseDefaults();
        using var j1 = new JigEx((mpw, _) => {
            line1.Move(line1.StartPoint, mpw);
        });
        j1.DatabaseEntityDraw(wd => wd.Geometry.Draw(line1));
        var jppo = j1.SetOptions("\n选择位置或");
        jppo.Keywords.Add("A", "A", "旋转90°(A)");
        jppo.Keywords.Add("D", "D", "旋转45°(D)");
        // 创建关键字钩子
        using var skwh = new SingleKeyWordHook();
        // 添加关键字
        skwh.AddKeys(jppo.Keywords);
        while (true)
        {
            // 循环开始时复位
            skwh.Reset();
            var r1 = Env.Editor.Drag(j1);
            if (skwh.IsResponsed || r1.Status == PromptStatus.Keyword)
            {
                // 此钩子完整保留了原关键字的鼠标点击功能，所以要同时支持两种情况，也方便在已经写好的关键字功能上扩展
                // 如果响应了关键字
                switch (skwh.IsResponsed ? skwh.StringResult : r1.StringResult.ToUpper())
                {
                    case "A":
                    line1.Rotation(line1.StartPoint, Math.PI * 0.5, Vector3d.ZAxis);
                    break;
                    case "D":
                    line1.Rotation(line1.StartPoint, Math.PI * 0.25, Vector3d.ZAxis);
                    break;
                }
                continue;
            }
            if (r1.Status == PromptStatus.OK)
            {
                using var tr = new DBTrans();
                tr.CurrentSpace.AddEntity(line1);
            }
            return;
        }
    }
}
