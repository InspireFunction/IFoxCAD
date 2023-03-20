﻿namespace IFoxCAD.Cad;


/// <summary>
/// 注册中心
/// <para>
/// 初始化程序集信息写入注册表并反射<see cref="IFoxInitialize"/>特性和<see cref="IFoxAutoGo"/>接口<br/>
/// 启动cad后的执行顺序为:<br/>
/// 1:<see cref="AutoRegAssem"/>程序集配置中心构造函数<br/>
/// 2:<see cref="IFoxInitialize"/>特性..(多个)<br/>
/// 3:<see cref="IFoxAutoGo"/>接口..(多个)<br/>
/// </para>
/// </summary>
public abstract class AutoRegAssem : IExtensionApplication
{
    #region 字段
    readonly AssemInfo _info;
    readonly AutoReflection? _autoRef;
    #endregion

    #region 静态方法
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
    public static DirectoryInfo GetDirectory(Assembly? assem)
    {
        if (assem is null)
            throw new(nameof(assem));
        return new FileInfo(assem.Location).Directory;
    }
    #endregion

    #region 构造函数
    /// <summary>
    /// 注册中心
    /// </summary>
    /// <param name="autoRegConfig">配置项目</param>
    public AutoRegAssem(AutoRegConfig autoRegConfig)
    {
        var assem = Assembly.GetCallingAssembly();
        _info = new()
        {
            Loader = assem.Location,
            Fullname = assem.FullName,
            Name = assem.GetName().Name,
            LoadType = AssemLoadType.Startting
        };

        if ((autoRegConfig & AutoRegConfig.Regedit) == AutoRegConfig.Regedit)
        {
            if (!SearchForReg())
                RegApp();
        }

        if ((autoRegConfig & AutoRegConfig.RemoveEMR) == AutoRegConfig.RemoveEMR)
            AcadEMR.Remove();

        // 实例化了 AutoClass 之后会自动执行 IFoxAutoGo 接口下面的类,
        // 以及自动执行特性 [IFoxInitialize]
        // 类库用户不在此处进行其他代码,而是实现特性
        if ((autoRegConfig & AutoRegConfig.ReflectionInterface) == AutoRegConfig.ReflectionInterface ||
            (autoRegConfig & AutoRegConfig.ReflectionAttribute) == AutoRegConfig.ReflectionAttribute)
        {
            _autoRef = new AutoReflection(_info.Name, autoRegConfig);
            _autoRef.Initialize();
        }
    }
    #endregion

    #region RegApp

    /// <summary>
    /// 获取当前cad注册表位置
    /// </summary>
    /// <param name="writable">打开权限</param>
    /// <returns></returns>
    public static RegistryKey GetAcAppKey(bool writable = true)
    {
        RegistryKey? ackey = null;
        var hc = HostApplicationServices.Current;
#if acad || zcad // 中望此处缺乏测试
        string key = hc.UserRegistryProductRootKey;
        ackey = Registry.CurrentUser.OpenSubKey(key, writable);
#endif

#if gcad
        // gcad 此处是否仍然有bug? 等待其他人测试反馈
        var key = hc.RegistryProductRootKey; // 浩辰2020读出来是""
        string regedit = "";
        if (Env.GetAcadVersion() == 2019)
            regedit = @"Software\Gstarsoft\GstarCAD\R" + 19 + @"\zh-CN";
        if (Env.GetAcadVersion() == 2020)
            regedit = @"Software\Gstarsoft\GstarCAD\B" + 20 + @"\zh-CN";
        ackey = Registry.CurrentUser.OpenSubKey(regedit, writable);
#endif

        return ackey.CreateSubKey("Applications");
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    public bool UnRegApp()
    {
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(_info.Name))
        {
            appkey.DeleteSubKey(_info.Name, false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否已经存在注册表
    /// </summary>
    /// <returns></returns>
    bool SearchForReg()
    {
        // 在使用netloadx的时候,此处注册表是失效的,具体原因要进行netloadx测试
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(_info.Name))
        {
            // 20220409 bug:文件名相同,路径不同,需要判断路径
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

    // 这里的是不会自动执行的
    public void Initialize()
    {
    }
    public void Terminate()
    {
    }

    ~AutoRegAssem()
    {
        _autoRef?.Terminate();
    }
    #endregion RegApp
}