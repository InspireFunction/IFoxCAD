namespace Test;

public class TestCmd_BindXrefs
{
    // 后台绑定
    [CommandMethod(nameof(Test_Bind1))]
    public static void Test_Bind1()
    {
        string fileName = @"D:\Test.dwg";
        using var tr = DBTrans.OpenPushToBackend(fileName);
        tr.XrefFactory(XrefModes.Bind);
        DatabaseEx.SaveDwgFile(tr.Database);
    }

    // 前台绑定
    [CommandMethod(nameof(Test_Bind2))]
    public static void Test_Bind2()
    {
        using var tr = DBTrans.Create();
        tr.XrefFactory(XrefModes.Bind);
        DatabaseEx.SaveDwgFile(tr.Database);
    }
}