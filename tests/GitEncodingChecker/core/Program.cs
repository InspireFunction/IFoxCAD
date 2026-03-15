// EncodingChecker - Git 编码检查工具
// 人类友好的命令行工具 - .NET 10 版本

// 使用示例:
//   git ec-check        # 检查编码
//   git ec-fix          # 修复编码
//   git ec-m -m "msg"  # 修复并提交
//   git ec-install      # 交互式安装

using System.Reflection;

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

// 先检查命令是否存在，再执行
if (!CommandTable.ContainsCommand(commandName))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ 未知命令: {commandName}");
    Console.ResetColor();
    Console.WriteLine();
    CommandHandlers.ShowHelp();
    Environment.Exit(1);
}

// 执行命令
try
{
    var exitCode = CommandTable.Execute(commandName, remainingArgs);
    Environment.Exit(exitCode);
}
catch (TargetInvocationException tie)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ 命令执行失败: {tie.InnerException?.Message ?? tie.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ 命令执行失败: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
