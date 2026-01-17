namespace Test;

public class TestHatchinfo
{
    [CommandMethod(" TestHatchInfo")]
    public void TestHatchInfo()
    {
        using var tr = new DBTrans();
        var sf = new SelectionFilter(new TypedValue[] { new TypedValue(0, "*line,circle,arc") });
        var ids = Env.Editor.SSGet(null, sf).Value?.GetObjectIds();
        if (ids == null || ids.Count() <= 0) return;
        var hf = new HatchInfo(ids!, false, null, 1, 0).Mode2UserDefined();
        hf.Build(tr.CurrentSpace);
    }
}

