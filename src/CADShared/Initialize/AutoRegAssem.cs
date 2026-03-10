

namespace IFoxCAD.Cad;

/// <summary>
/// 注册中心
/// <para>
/// 初始化程序集信息写入注册表并反射<see cref="IFoxInitializeAttribute"/>特性和<see cref="IFoxAutoGo"/>接口<br/>
/// 启动cad后的执行顺序为:<br/>
/// 1:<see cref="AutoRegAssem"/>程序集配置中心构造函数<br/>
/// 2:<see cref="IFoxInitializeAttribute"/>特性..(多个)<br/>
/// 3:<see cref="IFoxAutoGo"/>接口..(多个)<br/>
/// </para>
/// </summary>
public abstract class AutoRegAssem : IExtensionApplication
{
    #region 字段

    private readonly AutoReflection _autoRef;

    #endregion

    #region 静态方法

    /// <summary>
    /// 程序集的路径
    /// </summary>
    public static FileInfo Location => new(Assembly.GetCallingAssembly().Location);

    /// <summary>
    /// 程序集的目录
    /// </summary>
    public static DirectoryInfo? CurrDirectory => Location.Directory;

    /// <summary>
    /// 获取程序集的目录
    /// </summary>
    /// <param name="assem">程序集</param>
    /// <returns>路径对象</returns>
    public static DirectoryInfo GetDirectory(Assembly assem)
    {
        ArgumentNullEx.ThrowIfNull(assem);
        return new FileInfo(assem.Location).Directory;
    }

    #endregion


    #region 构造函数

    // 程序集为什么变:
    // 1,直接继承的构造函数,是用户的插件程序集(例如: TestAcad08)
    // 2,二级继承,此时程序集是: IFoxCAD.Acad08
    // 例如此处的this,一旦使用this传递,就会发生 当前程序集 转移,
    // TestAcad08 => IFoxCAD.Acad08
    //protected AutoRegAssem(AutoRegConfig autoRegConfig)
    //    : this(autoRegConfig, null)
    //{
    //}
    // 3,Initialize()方法内,运行时是acadexe目录的程序集

    /// <summary>
    /// 注册中心（推荐使用此重载）
    /// </summary>
    /// <param name="autoRegConfig">配置项目</param>
    protected AutoRegAssem(AutoRegConfig autoRegConfig)
    {
        // 必须注册文档锁,不然无法实现死锁检测
        DocumentLockManager.Init();

        // 必须直接继承,否则此处获取程序集会变.
        var assem = Assembly.GetCallingAssembly();
        var info = new AssemInfo
        {
            Loader = assem.Location,
            Fullname = assem.FullName!,
            Name = assem.GetName().Name!,
            LoadType = AssemLoadType.Starting
        };

        if (autoRegConfig.HasFlag(AutoRegConfig.Regedit))
        {
            if (!AutoReg.SearchForReg(info))
                AutoReg.RegApp(info);
        }
#if acad
        if (autoRegConfig.HasFlag(AutoRegConfig.RemoveEMR))
            AcadEMR.Remove();
#endif
        _autoRef = new AutoReflection(autoRegConfig, info.Name);
    }

    #endregion


    #region RegApp

    /// <summary>
    /// 开启时候执行
    /// </summary>
    public void Initialize()
    {
        _autoRef.Initialize();
    }

    /// <summary>
    /// 关闭时候执行
    /// </summary>
    public void Terminate()
    {
        _autoRef.Terminate();
    }
    #endregion RegApp
}