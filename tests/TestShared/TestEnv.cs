namespace Test;

public class Testenv
{
    [CommandMethod(nameof(Test_Enum))]
    public void Test_Enum()
    {
        Env.CmdEcho = true;
    }
    [CommandMethod(nameof(Test_Enum1))]
    public void Test_Enum1()
    {
        Env.CmdEcho = false;
    }

    [CommandMethod(nameof(Test_Dimblk))]
    public void Test_Dimblk()
    {
        Env.Dimblk = Env.DimblkType.Dot;
        Env.Dimblk = Env.DimblkType.Defult;
        Env.Dimblk = Env.DimblkType.Oblique;
    }
    [CommandMethod(nameof(Test_Dimblk1))]
    public void Test_Dimblk1()
    {
        var dim = Env.Dimblk;
        Env.Editor.WriteMessage(dim.ToString());
    }

    [CommandMethod(nameof(Test_Osmode))]
    public void Test_Osmode()
    {
        // 设置osmode变量，多个值用逻辑或
        Env.OSMode = Env.OSModeType.End | Env.OSModeType.Middle;
        // 也可以直接写数值，进行强转
        Env.OSMode = (Env.OSModeType)5179;
        // 追加模式
        Env.OSMode |= Env.OSModeType.Center;
        // 检查是否有某个模式
        var os = Env.OSMode.Include(Env.OSModeType.Center);
        // 取消某个模式
        Env.OSMode ^= Env.OSModeType.Center;
        Env.Editor.WriteMessage(Env.OSMode.ToString());
    }
    [CommandMethod(nameof(Test_Osmode1))]
    public void Test_Osmode1()
    {
        var dim = Env.OSMode;
        Env.Editor.WriteMessage(dim.ToString());
    }

    [CommandMethod(nameof(Test_Cadver))]
    public void Test_Cadver()
    {
        // Env.Print(AcadVersion.Versions);
        AcadVersion.Versions.ForEach(v => Env.Print(v));
        AcadVersion.FromApp(Acap.AcadApplication)?.Print();
        1.Print();
        "1".Print();
    }

    [CommandMethod(nameof(Test_GetVar))]
    public void Test_GetVar()
    {
        // test getvar
        var a = Env.GetVar("dbmod");
        a.Print();
        Env.SetVar("dbmod1", 1);
    }

    //[CommandMethod(nameof(Test_DwgVersion))]
    //public void TestDwgVersion()
    //{
    //    string filename = @"C:\Users\vic\Desktop\test.dwg";
    //    var a = Helper.GetCadFileVersion(filename);
    //    a.Print();
    //    ((DwgVersion)a).Print();
    //}


#if !NET35 && !NET40
    // 通过此功能获取全部变量,尚不清楚此处如何设置,没有通过测试
    [CommandMethod(nameof(Test_GetvarAll))]
    public static void Test_GetvarAll()
    {
        GetvarAll();
    }

    public static Dictionary<string, object> GetvarAll()
    {
        var dict = new Dictionary<string, object>();
        var en = new SystemVariableEnumerator();
        while (en.MoveNext())
        {
            Console.WriteLine(en.Current.Name + "-----" + en.Current.Value);// Value会出现异常
            dict.Add(en.Current.Name, en.Current.Value);
        }
        return dict;
    }
#endif
}