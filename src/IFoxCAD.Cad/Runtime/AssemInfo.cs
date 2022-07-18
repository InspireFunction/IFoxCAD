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