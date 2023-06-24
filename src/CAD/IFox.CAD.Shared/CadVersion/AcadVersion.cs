

namespace IFoxCAD.Cad;

/// <summary>
/// cad版本号类
/// </summary>
public static class AcadVersion
{
    private static readonly string _pattern = @"Autodesk\\AutoCAD\\R(\d+)\.(\d+)\\.*?";

    /// <summary>
    /// 所有安装的cad的版本号
    /// </summary>
    public static List<CadVersion> Versions
    {
        get
        {
            string[] copys = Registry.LocalMachine
                            .OpenSubKey(@"SOFTWARE\Autodesk\Hardcopy")
                            .GetValueNames();

            var _versions = new List<CadVersion>();
            for (int i = 0; i < copys.Length; i++)
            {
                if (!Regex.IsMatch(copys[i], _pattern))
                    continue;

                var gs = Regex.Match(copys[i], _pattern).Groups;
                var ver = new CadVersion
                {
                    ProductRootKey = copys[i],
                    ProductName = Registry.LocalMachine
                                .OpenSubKey("SOFTWARE")
                                .OpenSubKey(copys[i])
                                .GetValue("ProductName")
                                .ToString(),

                    Major = int.Parse(gs[1].Value),
                    Minor = int.Parse(gs[2].Value),
                };
                _versions.Add(ver);
            }
            return _versions;
        }
    }

    /// <summary>已打开的cad的版本号</summary>
    /// <param name="app">已打开cad的application对象</param>
    /// <returns>cad版本号对象</returns>
    public static CadVersion? FromApp(object app)
    {
        ArgumentNullEx.ThrowIfNull(app);

        string acver = app.GetType()
                        .InvokeMember(
                            "Version",
                            BindingFlags.GetProperty,
                            null,
                            app,
                            new object[0]).ToString();

        var gs = Regex.Match(acver, @"(\d+)\.(\d+).*?").Groups;
        int major = int.Parse(gs[1].Value);
        int minor = int.Parse(gs[2].Value);
        for (int i = 0; i < Versions.Count; i++)
            if (Versions[i].Major == major && Versions[i].Minor == minor)
                return Versions[i];
        return null;
    }
}