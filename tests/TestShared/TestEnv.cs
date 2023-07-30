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
        Env.Print(Env.Dimblk);
        Env.Print(Env.GetDimblkId(Env.DimblkType.Dot));
        Env.Dimblk = Env.DimblkType.Defult;
        Env.Print(Env.Dimblk);
        Env.Print(Env.GetDimblkId(Env.DimblkType.Defult));
        Env.Dimblk = Env.DimblkType.Oblique;
        Env.Print(Env.Dimblk);
        Env.Print(Env.GetDimblkId(Env.DimblkType.Oblique));
        Env.Dimblk = Env.DimblkType.ArchTick;
        Env.Print(Env.Dimblk);
        Env.Print(Env.GetDimblkId(Env.DimblkType.ArchTick));
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


#if !NET40
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

    [CommandMethod(nameof(Test_GetEnv))]
    public static void Test_GetEnv()
    {
        var dir = Env.GetEnv("PrinterConfigDir");
        Env.Printl("pc3打印机位置:" + dir);

        Env.SetEnv("abc", "656");

        var obj = Env.GetEnv("abc");
        Env.Printl("GetEnv:" + obj);

        Env.Printl("GetEnv:" + Env.GetEnv("abc"));
        Env.Printl("GetEnv PATH:" + Env.GetEnv("PATH"));
        
        Env.Printl($"getenv-acad: {Env.GetEnv("ACAD")}");
        Env.Printl($"getvar-acad: {Env.GetVar("TRUSTEDPATHS")}");
        Env.Printl($"getenv-TRUSTEDPATHS: {Env.GetEnv("TRUSTEDPATHS")}");
        Env.Printl($"getenv-osmode: {Env.GetEnv("osmode")}");
        Env.Printl($"getvar-osmode: {Env.GetVar("osmode")}");
    }
    [CommandMethod(nameof(Test_AppendPath))]
    public static void Test_AppendPath()
    {
        Directory.Exists(@"C:\Folder4").Print();
        Env.AppendSupportPath(@"C:\Folder4", @"C:\Folder5", @"C:\Folder6");
        // Env.AppendTrustedPath(@"c:\a\x",@"c:\a\c");
        // AppendSupportPath(@"c:\a\c");
        Env.GetEnv("ACAD").Print();
        // Env.SetEnv("ACAD",  @"C:\Folder1;"+Env.GetEnv("ACAD"));
        Env.GetEnv("ACAD").Contains(@"C:\Folder1").Print();

    }
    
    [CommandMethod(nameof(Test_RemovePath))]
    public static void Test_RemovePath()
    {
        // var acad = Acaop.TryGetSystemVariable("ACAD").ToString();
        // acad.Print();
        // Acaop.SetSystemVariable("ACAD", acad + @";c:\a\x");
        Env.GetEnv("ACAD").Print();
        Env.RemoveSupportPath();
        // Env.RemoveTrustedPath(@"c:\a\x");
        Env.GetEnv("ACAD").Print();
    }
    
    public static void AppendSupportPath(string path)
    {

        string key = HostApplicationServices.Current.UserRegistryProductRootKey;
        // 计算机\HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R24.0\ACAD-4101:804
        var ackey = Registry.CurrentUser.OpenSubKey($@"{key}\Profiles") ?? null;

        if (ackey != null)
        {
            var listkey = ackey.GetSubKeyNames();
            foreach (var item in listkey)
            {
                var acadkey = ackey.OpenSubKey($@"{item}\General", true);
                const string name = "ACAD";
                var str = acadkey?.GetValue(name)?.ToString();
                if (str != null && !str.ToLower().Contains(path.ToLower()))
                {
                    acadkey?.SetValue(name, $@"{str}{path};");
                }
            }
        }

        ackey?.Close();
    }

    
}