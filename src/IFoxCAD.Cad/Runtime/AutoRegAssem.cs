namespace IFoxCAD.Cad;
using Registry = Microsoft.Win32.Registry;
using RegistryKey = Microsoft.Win32.RegistryKey;

/// <summary>
/// 程序集加载类型
/// </summary>
public enum AssemLoadType
{
    /// <summary>
    /// 启动
    /// </summary>
    Startting = 2,

    /// <summary>
    /// 随命令
    /// </summary>
    ByCommand = 12,

    /// <summary>
    /// 无效
    /// </summary>
    Disabled = 20
}

/// <summary>
///  初始化程序集信息,并写入注册表
/// </summary>
public abstract class AutoRegAssem : IExtensionApplication
{
    private readonly AutoClass ac;
    AssemInfo Info;

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
        if (assem is null)
            throw new(nameof(assem));

        return new FileInfo(assem.Location).Directory;
    }

    /// <summary>
    /// 初始化程序集信息,并写入注册表
    /// </summary>
    public AutoRegAssem()
    {
        var assem = Assembly.GetCallingAssembly();
        Info = new()
        {
            Loader = assem.Location,
            Fullname = assem.FullName,
            Name = assem.GetName().Name,
            LoadType = AssemLoadType.Startting
        };

        if (!SearchForReg())
            RegApp();

        //实例化了 AutoClass 之后会自动执行 IFoxAutoGo 接口下面的类,
        //以及自动执行特性 [IFoxInitialize]
        //类库用户不在此处进行其他代码,而是实现特性
        ac = new AutoClass(Info.Name);
        ac.Initialize();
    }

    #region RegApp

    static RegistryKey GetAcAppKey()
    {
#if NET35
        string key = HostApplicationServices.Current.RegistryProductRootKey;
#else
        string key = HostApplicationServices.Current.MachineRegistryProductRootKey;
#endif
        var ackey = Registry.CurrentUser.OpenSubKey(key, true);
        return ackey.CreateSubKey("Applications");
    }

    bool SearchForReg()
    {
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(Info.Name))
        {
            //20220409 bug:文件名相同,路径不同,需要判断路径
            var info = appkey.OpenSubKey(Info.Name);
            return info.GetValue("LOADER")?.ToString().ToLower() == Info.Loader.ToLower();
        }
        return false;
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    void RegApp()
    {
        var appkey = GetAcAppKey();
        var rk = appkey.CreateSubKey(Info.Name);
        rk.SetValue("DESCRIPTION", Info.Fullname, RegistryValueKind.String);
        rk.SetValue("LOADCTRLS", Info.LoadType, RegistryValueKind.DWord);
        rk.SetValue("LOADER", Info.Loader, RegistryValueKind.String);
        rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);
        appkey.Close();
    }

    //这里的是不会自动执行的
    public void Initialize() { }
    public void Terminate() { }

    ~AutoRegAssem()
    {
        ac.Terminate();
    }
    #endregion RegApp
}
