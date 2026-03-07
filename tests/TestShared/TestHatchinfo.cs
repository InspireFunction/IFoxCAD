namespace Test;

public class TestHatchinfo
{
    [CommandMethod(" TestHatchInfo")]
    public void TestHatchInfo()
    {
        using var tr = DBTrans.Create();
        var sf = new SelectionFilter([new TypedValue(0, "*line,circle,arc")]);
        var ids = Env.Editor.SSGet(null, sf).Value?.GetObjectIds();
        if (ids is null || ids.Count() <= 0)
            return;
        var hf = HatchInfo.Create(ids, false, null, 1, 0).Mode2UserDefined();
        hf.Build(tr.CurrentSpace);
    }
}

