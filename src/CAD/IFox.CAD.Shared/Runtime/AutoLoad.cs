namespace IFoxCAD.Cad;

/// <summary>
/// 自动加载和初始化抽象类
/// </summary>
public abstract class AutoLoad : IExtensionApplication
{
    private AssemInfo _info = new();

    /// <summary>
    /// 程序集的路径
    /// </summary>
    public static FileInfo Location => new(Assembly.GetCallingAssembly().Location);

    /// <summary>
    /// 程序集的目录
    /// </summary>
    public static DirectoryInfo CurrDirectory => Location.Directory;

    /// <summary>
    /// 获取程序集的目录
    /// </summary>
    /// <param name="assem">程序集</param>
    /// <returns>路径对象</returns>
    public static DirectoryInfo GetDirectory(Assembly assem)
    {
        if (assem == null)
        {
            throw new(nameof(assem));
        }
        return new FileInfo(assem.Location).Directory;
    }

    /// <summary>
    /// 初始化程序集信息
    /// </summary>
    public AutoLoad()
    {
        Assembly assem = Assembly.GetCallingAssembly();
        _info.Loader = assem.Location;
        _info.Fullname = assem.FullName;
        _info.Name = assem.GetName().Name;
        _info.LoadType = AssemLoadType.Startting;

        if (!SearchForReg())
        {
            RegApp();
            AppendSupportPath(CurrDirectory.FullName);
        }

    }

    #region RegApp

    private static RegistryKey GetAcAppKey()
    {
#if NET35
        string key = HostApplicationServices.Current.RegistryProductRootKey;
#else
        string key = HostApplicationServices.Current.UserRegistryProductRootKey;
#endif
        RegistryKey ackey = Registry.CurrentUser.OpenSubKey(key, true);
        return ackey.CreateSubKey("Applications");
    }

    private void AppendSupportPath(string path)
    {
#if NET35
        string key = HostApplicationServices.Current.RegistryProductRootKey;
#else
        string key = HostApplicationServices.Current.UserRegistryProductRootKey;
#endif
        // 计算机\HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R24.0\ACAD-4101:804
        RegistryKey ackey = Registry.CurrentUser.OpenSubKey($@"{key}\Profiles");

        var listkey = ackey.GetSubKeyNames();
        foreach (var item in listkey)
        {
            var acadkey = ackey.OpenSubKey($@"{item}\General", true);
            var name = "ACAD";
            var str = acadkey.GetValue(name)?.ToString();
            if (str is not null && !str.Contains(path))
            {
                acadkey.SetValue(name, $@"{str}{path};");
            }
            
        }
        
        ackey.Close();
    }

    private bool SearchForReg()
    {
        RegistryKey appkey = GetAcAppKey();
        var regApps = appkey.GetSubKeyNames();
        return regApps.Contains(_info.Name);
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    public void RegApp()
    {
        RegistryKey appkey = GetAcAppKey();
        RegistryKey rk = appkey.CreateSubKey(_info.Name);
        rk.SetValue("DESCRIPTION", _info.Fullname, RegistryValueKind.String);
        rk.SetValue("LOADCTRLS", _info.LoadType, RegistryValueKind.DWord);
        rk.SetValue("LOADER", _info.Loader, RegistryValueKind.String);
        rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);
        appkey.Close();
        
    }

#endregion RegApp

#region IExtensionApplication 成员

    /// <summary>
    /// 初始化函数
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// 结束函数
    /// </summary>
    public abstract void Terminate();

#endregion IExtensionApplication 成员
}
