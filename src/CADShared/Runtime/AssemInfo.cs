namespace IFoxCAD.Cad;

/// <summary>
/// 程序集信息
/// </summary>
[Serializable]
public struct AssemInfo
{
    /// <summary>
    /// 注册名
    /// </summary>
    public string Name;

    /// <summary>
    /// 程序集全名
    /// </summary>
    public string Fullname;

    /// <summary>
    /// 程序集路径
    /// </summary>
    public string Loader;

    /// <summary>
    /// 加载方式
    /// </summary>
    public AssemLoadType LoadType;

    /// <summary>
    /// 程序集说明
    /// </summary>
    public string Description;
}


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
/// 注册中心配置信息
/// </summary>
public enum AutoRegConfig
{
    /// <summary>
    /// 不进行任何操作
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// 注册表
    /// </summary>
    Regedit = 1,
    /// <summary>
    /// 反射特性
    /// </summary>
    ReflectionAttribute = 2,
    /// <summary>
    /// 反射接口
    /// </summary>
    ReflectionInterface = 4,
    /// <summary>
    /// 移除教育版
    /// </summary>
    RemoveEMR = 8,
    /// <summary>
    /// 全部
    /// </summary>
    All = Regedit | ReflectionAttribute | ReflectionInterface | RemoveEMR,
}