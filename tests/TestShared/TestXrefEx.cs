namespace Test;

public class TestCmd_BindXrefs
{
    //后台绑定
    [CommandMethod(nameof(Test_Bind1))]
    public static void Test_Bind1()
    {
        string fileName = @"D:\Test.dwg";
        using var tr = new DBTrans(fileName,
            fileOpenMode: FileOpenMode.OpenForReadAndAllShare/*后台绑定特别注意*/);
        tr.XrefFactory(XrefModes.Bind);
        tr.Database.SaveDwgFile();
    }

    //前台绑定
    [CommandMethod(nameof(Test_Bind2))]
    public static void Test_Bind2()
    {
        using var tr = new DBTrans();
        tr.XrefFactory(XrefModes.Bind);
        tr.Database.SaveDwgFile();
    }
}