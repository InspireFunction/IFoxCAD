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
    public static DirectoryInfo CurrentDirectory => Location.Directory;

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

        if (!AutoReg.SearchForReg(_info))
        {
            AutoReg.RegApp(_info);
        }

    }

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
