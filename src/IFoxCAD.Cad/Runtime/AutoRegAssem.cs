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
/// 自动加载程序集的抽象类，继承自 IExtensionApplication 接口
/// </summary>
public abstract class AutoRegAssem : IAutoGo
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
        if (assem is null)
        {
            throw new(nameof(assem));
        }
        return new FileInfo(assem.Location).Directory;
    }

    /// <summary>
    /// 初始化程序集信息
    /// </summary>
    public AutoRegAssem()
    {
        var assem = Assembly.GetCallingAssembly();
        _info.Loader = assem.Location;
        _info.Fullname = assem.FullName;
        _info.Name = assem.GetName().Name;
        _info.LoadType = AssemLoadType.Startting;

        if (!SearchForReg())
            RegApp();
    }

    #region RegApp

    private static RegistryKey GetAcAppKey()
    {
#if ac2009
        string key = HostApplicationServices.Current.RegistryProductRootKey;
#else
        string key = HostApplicationServices.Current.MachineRegistryProductRootKey;
#endif
        var ackey = Registry.CurrentUser.OpenSubKey(key, true);
        return ackey.CreateSubKey("Applications");
    }

    private bool SearchForReg()
    {
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(_info.Name))
        {
            //20220409 bug:文件名相同,路径不同,需要判断路径
            var info = appkey.OpenSubKey(_info.Name);
            return info.GetValue("LOADER")?.ToString().ToLower() == _info.Loader.ToLower();
        }
        return false;
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    public void RegApp()
    {
        var appkey = GetAcAppKey();
        var rk = appkey.CreateSubKey(_info.Name);
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

    public abstract Sequence SequenceId();

    #endregion IExtensionApplication 成员
}
