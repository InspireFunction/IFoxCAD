// EncodingChecker - Git 编码检查工具
// 人类友好的命令行工具 - .NET 10 版本

// 使用示例:
//   git ec              # 检查编码
//   git ec-fix          # 修复编码
//   git ecc -m "msg"    # 修复并提交
//   git ec-install      # 交互式安装

using System.Reflection;

// 收集所有带 [GitCommand] 特性的方法
var commandDict = typeof(CommandHandlers)
    .GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Select(m => new { Method = m, Attr = m.GetCustomAttribute<GitCommandAttribute>() })
    .Where(x => x.Attr is not null)
    .ToDictionary(
        x => x.Attr!.Name.ToLowerInvariant(),
        x => new CommandInfo(x.Method, x.Attr!),
        StringComparer.OrdinalIgnoreCase
    );

// 解析命令行参数
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var commandName = cmdArgs.Length > 0 ? cmdArgs[0].ToLowerInvariant() : "--check";
var remainingArgs = cmdArgs.Skip(1).ToArray();

// 显示帮助
if (commandName is "--help" or "-h")
{
    CommandHandlers.ShowHelp();
    Environment.Exit(0);
}

// 执行命令
if (commandDict.TryGetValue(commandName, out var cmdInfo))
{
    try
    {
        var result = cmdInfo.Method.GetParameters() switch
        {
            [] => cmdInfo.Method.Invoke(null, null),
            [{ ParameterType: var pt }] when pt == typeof(string[]) => 
                cmdInfo.Method.Invoke(null, [remainingArgs]),
            _ => throw new InvalidOperationException($"命令 {commandName} 的参数格式不支持")
        };

        Environment.Exit((int)result!);
    }
    catch (TargetInvocationException tie)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ 命令执行失败: {tie.InnerException?.Message ?? tie.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ 未知命令: {commandName}");
    Console.ResetColor();
    Console.WriteLine();
    CommandHandlers.ShowHelp();
    Environment.Exit(1);
}

// 辅助类用于存储命令信息 - 使用 .NET 10 primary constructor
public class CommandInfo(MethodInfo method, GitCommandAttribute attr)
{
    public MethodInfo Method { get; } = method;
    public GitCommandAttribute Attr { get; } = attr;
}
