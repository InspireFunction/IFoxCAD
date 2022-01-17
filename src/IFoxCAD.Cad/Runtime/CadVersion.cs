namespace IFoxCAD.Cad;

public record CadVersion
{
    /// <summary>
    /// 主版本
    /// </summary>
    public int Major;

    /// <summary>
    /// 次版本
    /// </summary>
    public int Minor;

    /// <summary>
    /// 版本号
    /// </summary>
    public double ProgId => double.Parse($"{Major}.{Minor}");

    /// <summary>
    /// 注册表名称
    /// </summary>
    public string ProductName;

    /// <summary>
    /// 注册表位置
    /// </summary>
    public string ProductRootKey;

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns>表示版本号的字符串</returns>
    public override string ToString()
    {
        return $"名称:{ProductName}\n版本号:{ProgId}\n注册表位置:{ProductRootKey}";
    }
}
