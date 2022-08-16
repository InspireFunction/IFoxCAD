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
            foreach (var rootkey in copys)
            {
                if (!Regex.IsMatch(rootkey, _pattern))
                    continue;

                var gs = Regex.Match(rootkey, _pattern).Groups;
                var ver = new CadVersion
                {
                    ProductRootKey = rootkey,
                    ProductName = Registry.LocalMachine
                                .OpenSubKey("SOFTWARE")
                                .OpenSubKey(rootkey)
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
        if (app == null)
            throw new ArgumentNullException(nameof(app));

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
        foreach (var ver in Versions)
            if (ver.Major == major && ver.Minor == minor)
                return ver;
        return null;
    }
}
