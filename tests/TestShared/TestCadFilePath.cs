#if !NET35
namespace Test;

public class TestCadFilePath
{
    [CommandMethod("TestCadFilePath")]
    public void TestCadFilePathfun()
    {
        var key = HostApplicationServices.Current.UserRegistryProductRootKey;
        // 计算机\HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R24.0\ACAD-4101:804
        var ackey = Registry.CurrentUser.OpenSubKey(key);
        var profileskey = ackey?.OpenSubKey("Profiles");

        var listkey = profileskey?.GetSubKeyNames();
        if (listkey == null) return;
        foreach (var item in listkey)
        {
            if (profileskey == null) continue;
            var acadkey = profileskey.OpenSubKey($@"{item}\General", true);
            const string name = "ACAD";
            var str = acadkey?.GetValue(name)?.ToString();
            if (str == null || str.Contains("nihao")) continue;
            Env.Print(str);
            acadkey?.SetValue(name, $@"{str}\nihao;", RegistryValueKind.String);
        }
    }
}

#endif