namespace Test;

public class Testenv
{
    [CommandMethod("testenum")]
    public void Testenum()
    {
       
        Env.CmdEcho = true;
        
    }
    [CommandMethod("testenum1")]
    public void Testenum1()
    {
        
        Env.CmdEcho = false;
       
    }

    [CommandMethod("testdimblk")]
    public void Testdimblk()
    {

        Env.Dimblk = Env.DimblkType.Dot;
        Env.Dimblk = Env.DimblkType.Defult;
        Env.Dimblk = Env.DimblkType.Oblique;

    }
    [CommandMethod("testdimblk1")]
    public void Testdimblk1()
    {
        var dim = Env.Dimblk;
        Env.Editor.WriteMessage(dim.ToString());

    }

    [CommandMethod("testosmode")]
    public void Testosmode()
    {
        // 设置osmode变量，多个值用逻辑或
        Env.OSMode = Env.OSModeType.End | Env.OSModeType.Middle;
        // 也可以直接写数值，进行强转
        Env.OSMode = (Env.OSModeType)5179;
        // 追加模式
        Env.OSMode |= Env.OSModeType.Center;
        //检查是否有某个模式
        var os = Env.OSMode.Include(Env.OSModeType.Center);
        // 取消某个模式
        Env.OSMode ^= Env.OSModeType.Center;
        Env.Editor.WriteMessage(Env.OSMode.ToString());
    }
    [CommandMethod("testosmode1")]
    public void Testosmode1()
    {
        var dim = Env.OSMode;
        Env.Editor.WriteMessage(dim.ToString());

    }

    [CommandMethod("testcadver")]
    public void Testcadver()
    {
        //Env.Print(AcadVersion.Versions);
        AcadVersion.Versions.ForEach(v => Env.Print(v));
        AcadVersion.FromApp(Application.AcadApplication).Print();
        1.Print();
        "1".Print();

    }

    [CommandMethod("TestGetVar")]
    public void TestGetVar()
    {
        // test getvar
        var a = Env.GetVar("dbmod");
        a.Print();
        Env.SetVar("dbmod1", 1);
    }

    [CommandMethod("TestDwgVersion")]
    public void TestDwgVersion()
    {
        //
        //string filename = @"C:\Users\vic\Desktop\test.dwg";
        //var a = Helper.GetCadFileVersion(filename);
        //a.Print();
        //((DwgVersion)a).Print();
    }
}
