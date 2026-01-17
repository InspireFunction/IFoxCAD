namespace TestAcad2025;

public static class TestPointOnRegion
{
    [CommandMethod(nameof(TestPointOnRegionCommand))]
    public static void TestPointOnRegionCommand()
    {
        var r1 = Env.Editor.GetEntity("\n选择多段线");
        if (r1.Status != PromptStatus.OK)
            return;
        using var tr = new DBTrans();
        if (tr.GetObject(r1.ObjectId) is not Polyline pl || pl.HasBulges)
            return;
        var stretchPoints = pl.GetStretchPoints();
        while (true)
        {
            var r2 = Env.Editor.GetPoint("\n选择点");
            if (r2.Status != PromptStatus.OK)
                return;
            var pt = r2.Value.Ucs2Wcs();
            stretchPoints.PointOnRegion(pt).Print();
        }
    }
}