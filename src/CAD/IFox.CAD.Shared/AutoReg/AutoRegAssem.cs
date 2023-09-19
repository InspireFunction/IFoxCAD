namespace IFoxCAD.Cad;


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
            if (!AutoReg.SearchForReg(_info))
                AutoReg.RegApp(_info);
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