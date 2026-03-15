using System.Reflection;

/// <summary>
/// 命令表 - 集中管理所有通过 [GitCommand] 特性标记的命令
/// 提供静态访问方式，供exe和单元测试共享使用
/// </summary>
public static class CommandTable
{
    // 延迟加载的命令映射表
    private static readonly Lazy<Dictionary<string, CommandEntry>> _lazyMap = new(BuildCommandMap);

    /// <summary>
    /// 命令映射表 - Key: 命令名称(如 "--check")，Value: 命令条目
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Map => _lazyMap.Value;

    /// <summary>
    /// 获取所有别名映射 - Key: Git别名(如 "ec-check")，Value: 命令名称
    /// </summary>
    public static IReadOnlyDictionary<string, string> Aliases =>
        Map.Values
            .Where(v => !string.IsNullOrEmpty(v.GitAlias))
            .ToDictionary(v => v.GitAlias!, v => v.Name);

    /// <summary>
    /// 检查命令是否存在
    /// </summary>
    public static bool ContainsCommand(string commandName)
    {
        if (string.IsNullOrEmpty(commandName)) return false;
        return Map.ContainsKey(commandName.ToLowerInvariant());
    }

    /// <summary>
    /// 尝试获取命令条目
    /// </summary>
    public static bool TryGetCommand(string commandName, out CommandEntry? entry)
    {
        entry = null;
        if (string.IsNullOrEmpty(commandName)) return false;
        return Map.TryGetValue(commandName.ToLowerInvariant(), out entry);
    }

    /// <summary>
    /// 执行指定命令
    /// </summary>
    /// <param name="commandName">命令名称</param>
    /// <param name="args">参数数组</param>
    /// <returns>命令执行结果码</returns>
    /// <exception cref="KeyNotFoundException">命令不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">命令参数格式不支持时抛出</exception>
    public static int Execute(string commandName, string[]? args = null)
    {
        if (!TryGetCommand(commandName, out var entry) || entry == null)
        {
            throw new KeyNotFoundException($"未知命令: {commandName}");
        }

        args ??= Array.Empty<string>();

        object? result = entry.MethodInfo.GetParameters() switch
        {
            [] => entry.MethodInfo.Invoke(null, null),
            [{ ParameterType: var pt }] when pt == typeof(string[]) =>
                entry.MethodInfo.Invoke(null, [args]),
            _ => throw new InvalidOperationException($"命令 {commandName} 的参数格式不支持")
        };

        return (int)result!;
    }

    /// <summary>
    /// 尝试执行命令，如果命令不存在返回false
    /// </summary>
    public static bool TryExecute(string commandName, string[]? args, out int exitCode)
    {
        exitCode = 1;
        if (!TryGetCommand(commandName, out var entry) || entry == null)
        {
            return false;
        }

        try
        {
            exitCode = Execute(commandName, args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取所有命令名称列表
    /// </summary>
    public static IEnumerable<string> GetAllCommandNames() => Map.Keys;

    /// <summary>
    /// 获取所有带Git别名的命令信息
    /// </summary>
    public static IEnumerable<(string Command, string Alias, string Description)> GetAliasList()
    {
        return Map.Values
            .Where(v => !string.IsNullOrEmpty(v.GitAlias) && v.RegisterAlias)
            .Select(v => (v.Name, v.GitAlias!, v.Description));
    }

    /// <summary>
    /// 构建命令映射表
    /// </summary>
    private static Dictionary<string, CommandEntry> BuildCommandMap()
    {
        return typeof(CommandHandlers)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(m => new { Method = m, Attr = m.GetCustomAttribute<GitCommandAttribute>() })
            .Where(x => x.Attr is not null)
            .ToDictionary(
                x => x.Attr!.Name.ToLowerInvariant(),
                x => new CommandEntry(x.Method, x.Attr!),
                StringComparer.OrdinalIgnoreCase
            );
    }
}

/// <summary>
/// 命令条目 - 包含命令的完整信息
/// </summary>
public sealed class CommandEntry
{
    /// <summary>
    /// 命令方法信息
    /// </summary>
    public MethodInfo MethodInfo { get; }

    /// <summary>
    /// 命令特性
    /// </summary>
    public GitCommandAttribute Attribute { get; }

    /// <summary>
    /// 命令名称（如 "--check"）
    /// </summary>
    public string Name => Attribute.Name;

    /// <summary>
    /// 命令描述
    /// </summary>
    public string Description => Attribute.Description;

    /// <summary>
    /// Git别名（如 "ec-check"）
    /// </summary>
    public string? GitAlias => Attribute.GitAlias;

    /// <summary>
    /// 是否注册别名
    /// </summary>
    public bool RegisterAlias => Attribute.RegisterAlias;

    public CommandEntry(MethodInfo methodInfo, GitCommandAttribute attribute)
    {
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
    }

    /// <summary>
    /// 执行命令（无参数）
    /// </summary>
    public int Execute() => CommandTable.Execute(Name, null);

    /// <summary>
    /// 执行命令（带参数）
    /// </summary>
    public int Execute(string[] args) => CommandTable.Execute(Name, args);
}
