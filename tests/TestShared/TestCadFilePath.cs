
namespace Test;

public class TestCadFilePath
{
    [CommandMethod("TestCadFilePath")]
    public void TestCadFilePathfun()
    {
        string key = HostApplicationServices.Current.UserRegistryProductRootKey;
        // 计算机\HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R24.0\ACAD-4101:804
        RegistryKey ackey = Registry.CurrentUser.OpenSubKey(key);
        var profileskey = ackey.OpenSubKey("Profiles");

        var listkey = profileskey.GetSubKeyNames();
        foreach (var item in listkey)
        {
            var acadkey = profileskey.OpenSubKey($@"{item}\General",true);
            var name = "ACAD";
            var str = acadkey.GetValue(name)?.ToString();
            if (str is not null && !str.Contains("nihao"))
            {
                Env.Print(str);
                acadkey.SetValue(name, $@"{str}\nihao;", RegistryValueKind.String);
            }


        }

    }
}
