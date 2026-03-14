// 人类友好的命令注册方式
// 用这个特性标记方法，就能自动注册为 Git 别名

[AttributeUsage(AttributeTargets.Method)]
public class GitCommandAttribute(string name, string description) : Attribute
{
    /// <summary>命令行参数，如 "--install"</summary>
    public string Name { get; } = name;

    /// <summary>命令描述，显示在帮助中</summary>
    public string Description { get; } = description;

    /// <summary>对应的 Git 别名，如 "ec"</summary>
    public string? GitAlias { get; set; }

    /// <summary>是否需要在安装时注册到 Git 别名</summary>
    public bool RegisterAlias { get; set; } = true;
}

/// <summary>
/// 标记安装相关的命令
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class InstallCommandAttribute : Attribute;

/// <summary>
/// 标记全局安装相关的命令
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class GlobalInstallCommandAttribute : Attribute;
