// # 1. 编译并安装到 .git/hooks
// cd g:\IFox工程\acad_IFoxCAD_35\GitEncodingChecker
// .\发布.cmd

// # 2. 创建一个非 UTF-8 编码的测试文件
// # 比如用记事本创建一个 ANSI 编码的 .cs 文件

// # 3. 暂存并提交，触发 pre-commit hook
// git add .
// git commit -m "test: 测试编码检查"

// # 安装后可以使用 git 别名
// git ec        # 等同于 --check
// git ec-fix    # 等同于 --fix
// git ec-install
// git ec-uninstall
// git ec-clean


using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method)]
public class GitCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public GitCommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

static class Program
{
    private static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

    static readonly string[] DefaultTextExtensions = new[] {
        ".cs", ".slnx", ".csproj", ".json", ".xml", ".config", ".props", ".targets",
        ".md", ".txt", ".yaml", ".yml", ".toml", ".ini", ".cfg", ".conf",
        ".html", ".htm", ".css", ".js", ".ts", ".jsx", ".tsx",
        ".py", ".rb", ".go", ".rs", ".java", ".c", ".cpp", ".h", ".hpp",
        ".sh", ".bash", ".ps1", ".psm1", ".bat", ".cmd"
    };

    static Dictionary<string, string?> ExtensionEolMap = new(StringComparer.OrdinalIgnoreCase);

    static string? _repoRootCache;
    static bool _extensionEolMapInitialized = false;

    static readonly Dictionary<string, (MethodInfo Method, string Description)> GitCommandMap;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(GitCommandAttribute))]
    [DynamicDependency(nameof(CheckEncodingAndPrompt))]
    [DynamicDependency(nameof(InstallGitCommand))]
    [DynamicDependency(nameof(UninstallGitCommand))]
    [DynamicDependency(nameof(CleanGitAliases))]
    [DynamicDependency(nameof(FixEncoding))]
    [DynamicDependency(nameof(EcCommit))]
    static Program()
    {
        GitCommandMap = new Dictionary<string, (MethodInfo, string)>(StringComparer.OrdinalIgnoreCase);
        var methods = typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<GitCommandAttribute>();
            if (attr != null)
            {
                GitCommandMap[attr.Name] = (method, attr.Description);
            }
        }
    }

    static void Main(string[] args)
    {
        var command = args.Length > 0 ? args[0].ToLowerInvariant() : "--check";
        var remainingArgs = args.Skip(1).ToArray();

        // 处理帮助命令
        if (command == "--help" || command == "-h")
        {
            ShowHelp();
            Environment.Exit(0);
        }

        // 检查是否是已注册的Git命令
        if (GitCommandMap.TryGetValue(command, out var cmdInfo))
        {
            // 检查是否需要自动安装（第一次运行时，且不是install/uninstall命令）
            if (command != "--install" && command != "--uninstall" && !IsGitAliasInstalled())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("检测到 Git 命令别名未安装，正在自动安装...");
                Console.ResetColor();
                Console.WriteLine();

                int installResult = InstallGitCommand();
                if (installResult != 0)
                {
                    Environment.Exit(installResult);
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("安装完成！现在可以继续使用 git ec 命令。");
                Console.ResetColor();
                Console.WriteLine();
            }

            try
            {
                object? result;
                var parameters = cmdInfo.Method.GetParameters();
                if (parameters.Length == 0)
                {
                    result = cmdInfo.Method.Invoke(null, null);
                }
                else
                {
                    result = cmdInfo.Method.Invoke(null, new object[] { remainingArgs });
                }
                Environment.Exit((int)result!);
            }
            catch (TargetInvocationException tie)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"命令执行失败: {tie.InnerException?.Message ?? tie.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        // 未知命令
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"未知命令: {command}");
        Console.ResetColor();
        Console.WriteLine();
        ShowHelp();
        Environment.Exit(1);
    }

    static void ShowHelp()
    {
        Console.WriteLine("EncodingChecker - Git 编码检查工具");
        Console.WriteLine();
        Console.WriteLine("用法:");
        Console.WriteLine("  EncodingChecker           检查暂存区文件编码");

        // 动态显示所有注册的命令
        foreach (var kvp in GitCommandMap)
        {
            Console.WriteLine($"  EncodingChecker {kvp.Key,-12} {kvp.Value.Description}");
        }

        Console.WriteLine("  EncodingChecker --help    显示帮助信息");
        Console.WriteLine();
        Console.WriteLine("Git 命令别名:");

        // 动态显示所有git别名
        foreach (var kvp in GitCommandMap)
        {
            var aliasName = kvp.Key.TrimStart('-');
            // install 命令映射到 ec，其他命令映射到 ec-{name}
            var gitAliasName = kvp.Key == "--install" ? "ec" : $"ec-{aliasName}";
            Console.WriteLine($"  git {gitAliasName,-12} {kvp.Value.Description}");
        }
    }

    [GitCommand("--install", "安装 Git 命令别名并配置 UTF-8 编码")]
    static int InstallGitCommand()
    {
        try
        {
            // 使用相对路径（相对于仓库根目录），这样即使仓库移动也能正常工作
            var repoRoot = GetRepoRoot();
            if (string.IsNullOrEmpty(repoRoot))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("错误: 无法确定Git仓库根目录");
                Console.ResetColor();
                return 1;
            }

            // 检查 exe 是否存在
            var exePath = Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe");
            if (!File.Exists(exePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"错误: 找不到 {exePath}");
                Console.WriteLine("请确保 EncodingChecker.exe 位于 .git/hooks/ 目录下");
                Console.ResetColor();
                return 1;
            }

            // 使用相对路径（相对于仓库根目录），这样即使仓库移动也能正常工作
            // Git Bash 直接使用路径，不需要 sh -c
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Git 命令别名已安装:");
            Console.WriteLine($"  (调试: GitCommandMap 包含 {GitCommandMap.Count} 个命令)");

            // 自动注册 GitCommandMap 中的所有命令
            foreach (var kvp in GitCommandMap)
            {
                Console.WriteLine($"  (调试: 处理命令 {kvp.Key})");
                var commandName = kvp.Key;
                var aliasName = commandName.TrimStart('-');
                // install 命令映射到 ec，其他命令映射到 ec-{name}
                var gitAliasName = commandName == "--install" ? "ec" : $"ec-{aliasName}";

                // 构建别名值
                // Git 别名格式: "!path/to/exe" 或 "!path/to/exe --arg"
                // 路径使用正斜杠（Git Bash兼容），空格用双引号包裹
                var exeRelativePath = ".git/hooks/EncodingChecker.exe";
                var aliasValue = commandName == "--install"
                    ? $"!\"{exeRelativePath}\""
                    : $"!\"{exeRelativePath}\" {commandName}";

                // 使用 ArgumentList 避免引号转义问题
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("config");
                psi.ArgumentList.Add("--global");
                psi.ArgumentList.Add($"alias.{gitAliasName}");
                psi.ArgumentList.Add(aliasValue);

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    if (proc.ExitCode != 0)
                    {
                        Console.WriteLine($"  (错误: 设置别名 {gitAliasName} 失败: {error})");
                    }
                }

                Console.WriteLine($"  - git {gitAliasName,-12}: {kvp.Value.Description}");
            }

            var defaultEol = GetDefaultLineEnding(repoRoot);
            var eolValue = defaultEol.ToLowerInvariant();

            RunGitCommand("config", "--global", "core.autocrlf", "false");
            RunGitCommand("config", "--global", "core.eol", eolValue);
            RunGitCommand("config", "--global", "i18n.commitencoding", "utf-8");
            RunGitCommand("config", "--global", "i18n.logoutputencoding", "utf-8");
            RunGitCommand("config", "--global", "core.quotepath", "false");

            Console.WriteLine();
            Console.WriteLine("✓ Git 编码配置已设置为 UTF-8:");
            Console.WriteLine("  - core.autocrlf = false");
            Console.WriteLine($"  - core.eol = {eolValue} (来自配置文件)");
            Console.WriteLine("  - i18n.commitencoding = utf-8");
            Console.WriteLine("  - i18n.logoutputencoding = utf-8");
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"安装失败: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    [GitCommand("--uninstall", "卸载 Git 命令别名")]
    static int UninstallGitCommand()
    {
        try
        {
            // 自动卸载 GitCommandMap 中的所有命令
            foreach (var kvp in GitCommandMap)
            {
                var aliasName = kvp.Key.TrimStart('-');
                // install 命令映射到 ec，其他命令映射到 ec-{name}
                var gitAliasName = kvp.Key == "--install" ? "ec" : $"ec-{aliasName}";
                RunGitCommand("config", "--global", "--unset", $"alias.{gitAliasName}");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Git 命令别名已卸载");
            Console.ResetColor();
            return 0;
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ 别名可能不存在或已被删除");
            Console.ResetColor();
            return 0;
        }
    }

    [GitCommand("--clean", "清理所有 Git 命令别名（包括重复的）")]
    static int CleanGitAliases()
    {
        try
        {
            Console.WriteLine("正在清理所有 EncodingChecker 相关的 Git 别名...");

            // 使用 --unset-all 来删除所有可能的重复别名
            foreach (var kvp in GitCommandMap)
            {
                var aliasName = kvp.Key.TrimStart('-');
                var gitAliasName = kvp.Key == "--install" ? "ec" : $"ec-{aliasName}";

                // 使用 RunGitCommandWithOutput 来捕获输出，忽略错误
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"config --global --unset-all alias.{gitAliasName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 所有 Git 命令别名已清理");
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ 清理过程中出现问题: {ex.Message}");
            Console.ResetColor();
            return 0;
        }
    }

    [GitCommand("--commit", "跳过编码检查强制提交 (用法: git ec-commit -m \"msg\")")]
    static int EcCommit(string[] args)
    {
        if (args.Length == 0 || !args.Contains("-m"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("错误: 缺少提交信息");
            Console.ResetColor();
            Console.WriteLine("用法: git ec-commit -m \"提交信息\"");
            Console.WriteLine("示例: git ec-commit -m \"feat: 添加新功能\"");
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("警告: 跳过编码检查，正在执行强制提交...");
        Console.ResetColor();

        var psi = new ProcessStartInfo
        {
            FileName = "git",
            UseShellExecute = false
        };

        psi.ArgumentList.Add("commit");
        psi.ArgumentList.Add("--no-verify");
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
        return proc?.ExitCode ?? 0;
    }

    [GitCommand("--fix", "修复暂存区文件编码为UTF-8无BOM，行尾根据配置文件")]
    static int FixEncoding()
    {
        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("无法确定Git仓库根目录。");
            return 1;
        }

        var stagedFiles = GetStagedFiles();
        if (stagedFiles.Count == 0)
        {
            Console.WriteLine("没有暂存的文件需要修复。");
            return 0;
        }

        Console.WriteLine($"正在检查 {stagedFiles.Count} 个暂存文件...");
        int encodingFixedCount = 0;
        int lineEndingFixedCount = 0;

        foreach (var file in stagedFiles)
        {
            var fullPath = Path.Combine(repoRoot, file);
            if (!File.Exists(fullPath)) continue;

            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!IsTextExtension(ext)) continue;

            bool needsFix = false;
            var encoding = GetFileEncoding(fullPath);
            var lineEnding = GetLineEndingType(fullPath);
            var targetEol = GetConfiguredLineEnding(repoRoot, file);

            if (encoding == "UTF8-BOM" || encoding == "UTF16-LE" || encoding == "UTF16-BE")
            {
                needsFix = true;
            }

            if ((targetEol == "LF" && (lineEnding == "CRLF" || lineEnding == "Mixed")) ||
                (targetEol == "CRLF" && (lineEnding == "LF" || lineEnding == "Mixed")))
            {
                needsFix = true;
            }

            if (needsFix)
            {
                try
                {
                    var content = File.ReadAllText(fullPath);
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                    if (targetEol == "CRLF")
                    {
                        content = content.Replace("\n", "\r\n");
                    }
                    File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  已修复: {file}");
                    if (encoding != "UTF8" && encoding != "ASCII")
                    {
                        Console.WriteLine($"    编码: {encoding} -> UTF-8");
                        encodingFixedCount++;
                    }
                    if ((targetEol == "LF" && (lineEnding == "CRLF" || lineEnding == "Mixed")) ||
                        (targetEol == "CRLF" && (lineEnding == "LF" || lineEnding == "Mixed")))
                    {
                        Console.WriteLine($"    行尾: {lineEnding} -> {targetEol}");
                        lineEndingFixedCount++;
                    }
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  修复失败: {file} - {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        if (encodingFixedCount > 0 || lineEndingFixedCount > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"共修复 {encodingFixedCount + lineEndingFixedCount} 个问题:");
            if (encodingFixedCount > 0) Console.WriteLine($"  - 编码修复: {encodingFixedCount} 个文件");
            if (lineEndingFixedCount > 0) Console.WriteLine($"  - 行尾修复: {lineEndingFixedCount} 个文件");
            Console.ResetColor();
            Console.WriteLine("请重新执行: git add . && git commit");
        }
        else
        {
            Console.WriteLine("没有需要修复的文件。");
        }
        return 0;
    }

    [GitCommand("--check", "检查暂存区文件编码")]
    static int CheckEncodingAndPrompt()
    {
        var repoRoot = GetRepoRoot();
        // Console.WriteLine($"仓库根目录: {repoRoot}");

        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("无法确定Git仓库根目录。");
            return 0;
        }

        var stagedFiles = GetStagedFiles();
        // Console.WriteLine($"暂存区文件数: {stagedFiles.Count}");

        if (stagedFiles.Count == 0)
        {
            Console.WriteLine("没有暂存的文件需要检查。");
            return 0;
        }

        // Console.WriteLine($"正在检查 {stagedFiles.Count} 个暂存文件的编码...");

        var nonUtf8Files = new List<(string File, string Encoding)>();
        var wrongLineEndingFiles = new List<(string File, string LineEnding, string Expected)>();

        foreach (var file in stagedFiles)
        {
            var fullPath = Path.Combine(repoRoot, file);
            // Console.WriteLine($"  检查: {file} -> {fullPath} (存在:{File.Exists(fullPath)})");

            if (!File.Exists(fullPath)) continue;

            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!IsTextExtension(ext))
            {
                continue;
            }

            var encoding = GetFileEncoding(fullPath);
            // Console.WriteLine($"    编码: {encoding}");

            if (encoding != "UTF8" && encoding != "ASCII")
            {
                nonUtf8Files.Add((file, encoding));
            }

            var lineEnding = GetLineEndingType(fullPath);
            var expectedEol = GetConfiguredLineEnding(repoRoot, file);
            if ((expectedEol == "LF" && (lineEnding == "CRLF" || lineEnding == "Mixed")) ||
                (expectedEol == "CRLF" && (lineEnding == "LF" || lineEnding == "Mixed")))
            {
                wrongLineEndingFiles.Add((file, lineEnding, expectedEol));
            }
        }

        bool hasIssues = nonUtf8Files.Count > 0 || wrongLineEndingFiles.Count > 0;

        if (hasIssues)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("============================================");
            Console.WriteLine("警告: 发现文件问题!");
            Console.WriteLine("============================================");
            Console.ResetColor();
            Console.WriteLine();

            if (nonUtf8Files.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"【编码问题】({nonUtf8Files.Count} 个文件，应为 UTF-8 无BOM):");
                foreach (var (file, encoding) in nonUtf8Files)
                {
                    Console.WriteLine($"  {file} [{encoding}]");
                }
                Console.ResetColor();
                Console.WriteLine();
            }

            if (wrongLineEndingFiles.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"【行尾问题】({wrongLineEndingFiles.Count} 个文件):");
                foreach (var (file, lineEnding, expected) in wrongLineEndingFiles)
                {
                    Console.WriteLine($"  {file} [{lineEnding} -> 应为 {expected}]");
                }
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine("修复命令: git ec-fix");
            Console.WriteLine("强制提交: git ec-commit -m \"msg\"");

            if (!IsGitAliasInstalled())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("检测到 Git 命令别名未安装，正在自动安装...");
                Console.ResetColor();
                InstallGitCommand();
            }

            // 检测是否在交互式环境（标准输入是否是终端）
            if (Console.IsInputRedirected)
            {
                // 非交互式环境（如Git Bash），提示修复命令和强制提交命令
                Console.WriteLine();
                Console.WriteLine("提交已阻止。");
                return 1;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("仍然要提交吗? (y/n): ");
            Console.ResetColor();

            try
            {
                var response = Console.ReadLine();
                if (response?.Trim().ToLower() == "y")
                {
                    Console.WriteLine("继续提交...");
                    return 0;
                }
                else
                {
                    Console.WriteLine("已取消提交。");
                    return 1;
                }
            }
            catch
            {
            }

            Console.WriteLine("已取消提交。");
            return 1;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("所有文件编码检查通过!");
            Console.ResetColor();
            return 0;
        }
    }

    static void RunGitCommand(params string[] arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }
        using var proc = Process.Start(psi);
        proc?.WaitForExit();
    }

    static bool IsGitAliasInstalled()
    {
        try
        {
            // 检查 GitCommandMap 中是否有任意一个命令已安装
            foreach (var kvp in GitCommandMap)
            {
                var aliasName = kvp.Key.TrimStart('-');
                // install 命令映射到 ec，其他命令映射到 ec-{name}
                var gitAliasName = kvp.Key == "--install" ? "ec" : $"ec-{aliasName}";
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"config --global alias.{gitAliasName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) continue;

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                // 如果返回了路径，说明别名已安装
                if (!string.IsNullOrWhiteSpace(output) && output.Contains("EncodingChecker"))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static string GetRepoRoot()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --show-toplevel",
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            var result = proc?.StandardOutput.ReadLine() ?? string.Empty;
            proc?.WaitForExit();
            return result;
        }
        catch
        {
            return Directory.GetCurrentDirectory();
        }
    }

    static List<string> GetStagedFiles()
    {
        var files = new List<string>();
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "diff --cached --name-only --diff-filter=ACM",
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            while (proc != null && !proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    files.Add(line);
                }
            }
            proc?.WaitForExit();
        }
        catch { }
        return files;
    }

    static string GetFileEncoding(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return "UTF8-BOM";
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return "UTF16-LE";
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return "UTF16-BE";
            }

            bool isUtf8 = true;
            int i = 0;
            while (i < bytes.Length && i < 4096)
            {
                byte b = bytes[i];
                if (b < 0x80)
                {
                    i++;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    if (i + 1 >= bytes.Length || (bytes[i + 1] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    if (i + 2 >= bytes.Length || (bytes[i + 1] & 0xC0) != 0x80 || (bytes[i + 2] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0)
                {
                    if (i + 3 >= bytes.Length || (bytes[i + 1] & 0xC0) != 0x80 || (bytes[i + 2] & 0xC0) != 0x80 || (bytes[i + 3] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 4;
                }
                else
                {
                    isUtf8 = false;
                    break;
                }
            }

            if (isUtf8) return "UTF8";

            bool hasHighBytes = bytes.Any(b => b > 127);
            if (hasHighBytes)
            {
                bool isGbk = true;
                for (int g = 0; g < bytes.Length; g++)
                {
                    if (bytes[g] > 127)
                    {
                        if (g + 1 >= bytes.Length || bytes[g] < 0x81 || bytes[g] > 0xFE || bytes[g + 1] < 0x40 || bytes[g + 1] > 0xFE)
                        {
                            isGbk = false;
                            break;
                        }
                        g++;
                    }
                }
                if (isGbk) return "GBK";

                bool isGb2312 = true;
                for (int j = 0; j < bytes.Length; j++)
                {
                    if (bytes[j] > 127)
                    {
                        if (j + 1 >= bytes.Length || bytes[j] < 0xA1 || bytes[j] > 0xF7 || bytes[j + 1] < 0xA1 || bytes[j + 1] > 0xFE)
                        {
                            isGb2312 = false;
                            break;
                        }
                        j++;
                    }
                }
                if (isGb2312) return "GB2312";

                bool isBig5 = true;
                for (int b = 0; b < bytes.Length; b++)
                {
                    if (bytes[b] > 127)
                    {
                        if (b + 1 >= bytes.Length || bytes[b] < 0xA1 || bytes[b] > 0xF9)
                        {
                            isBig5 = false;
                            break;
                        }
                        byte low = bytes[b + 1];
                        if ((low < 0x40 || low > 0x7E) && (low < 0xA1 || low > 0xFE))
                        {
                            isBig5 = false;
                            break;
                        }
                        b++;
                    }
                }
                if (isBig5) return "Big5";
            }

            return "ASCII";
        }
        catch
        {
            return "Error";
        }
    }

    static string GetLineEndingType(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            bool hasCrlf = false;
            bool hasLf = false;

            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
                {
                    hasCrlf = true;
                    i++;
                }
                else if (bytes[i] == 0x0A)
                {
                    hasLf = true;
                }
            }
            if (bytes.Length > 0 && bytes[bytes.Length - 1] == 0x0A)
            {
                if (bytes.Length < 2 || bytes[bytes.Length - 2] != 0x0D)
                {
                    hasLf = true;
                }
            }

            if (hasCrlf && hasLf) return "Mixed";
            if (hasCrlf) return "CRLF";
            if (hasLf) return "LF";
            return "None";
        }
        catch
        {
            return "Error";
        }
    }

    static Dictionary<string, string?> ParseGitattributesEolMap(string repoRoot)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var gitattributesPath = Path.Combine(repoRoot, ".gitattributes");
        if (!File.Exists(gitattributesPath)) return result;

        try
        {
            var lines = File.ReadAllLines(gitattributesPath);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                var pattern = parts[0];
                string? ext = null;

                if (pattern.StartsWith("*.") && pattern.Length > 2)
                {
                    ext = pattern.Substring(1).ToLowerInvariant();
                    if (!ext.StartsWith(".")) ext = "." + ext;
                }

                if (ext == null) continue;

                foreach (var attr in parts.Skip(1))
                {
                    if (attr.StartsWith("eol=", StringComparison.OrdinalIgnoreCase))
                    {
                        var eolValue = attr.Substring(4).ToLowerInvariant();
                        result[ext] = eolValue == "crlf" ? "CRLF" : eolValue == "lf" ? "LF" : null;
                        break;
                    }
                }
            }
        }
        catch { }
        return result;
    }

    static Dictionary<string, string?> ParseEditorconfigEolMap(string repoRoot)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var editorconfigPath = Path.Combine(repoRoot, ".editorconfig");
        if (!File.Exists(editorconfigPath)) return result;

        try
        {
            var lines = File.ReadAllLines(editorconfigPath);
            string? currentSection = null;
            string? currentEol = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    if (currentSection != null && currentEol != null)
                    {
                        var ext = ExtractExtensionFromSection(currentSection);
                        if (ext != null)
                        {
                            result[ext] = currentEol;
                        }
                    }
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    currentEol = null;
                    continue;
                }

                if (currentSection == null) continue;

                if (trimmed.StartsWith("end_of_line", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmed.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        var value = parts[1].Trim().ToLowerInvariant();
                        currentEol = value == "crlf" ? "CRLF" : value == "lf" ? "LF" : null;
                    }
                }
            }

            if (currentSection != null && currentEol != null)
            {
                var ext = ExtractExtensionFromSection(currentSection);
                if (ext != null)
                {
                    result[ext] = currentEol;
                }
            }
        }
        catch { }
        return result;
    }

    static string? ExtractExtensionFromSection(string section)
    {
        if (section == "*") return "*";
        if (section.StartsWith("*.") && section.Length > 2)
        {
            var ext = section.Substring(1).ToLowerInvariant();
            if (!ext.StartsWith(".")) ext = "." + ext;
            return ext;
        }
        return null;
    }

    static void InitExtensionEolMap(string repoRoot)
    {
        if (_extensionEolMapInitialized && _repoRootCache == repoRoot) return;

        _repoRootCache = repoRoot;
        _extensionEolMapInitialized = true;
        ExtensionEolMap.Clear();

        foreach (var ext in DefaultTextExtensions)
        {
            ExtensionEolMap[ext] = null;
        }

        var editorconfigMap = ParseEditorconfigEolMap(repoRoot);
        foreach (var kvp in editorconfigMap)
        {
            ExtensionEolMap[kvp.Key] = kvp.Value;
        }

        var gitattributesMap = ParseGitattributesEolMap(repoRoot);
        foreach (var kvp in gitattributesMap)
        {
            ExtensionEolMap[kvp.Key] = kvp.Value;
        }
    }

    static string GetConfiguredLineEnding(string repoRoot, string filePath)
    {
        InitExtensionEolMap(repoRoot);

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (!ext.StartsWith(".")) ext = "." + ext;

        if (ExtensionEolMap.TryGetValue(ext, out var eol) && eol != null)
        {
            return eol;
        }

        if (ExtensionEolMap.TryGetValue("*", out var defaultEol) && defaultEol != null)
        {
            return defaultEol;
        }

        return "LF";
    }

    static string GetDefaultLineEnding(string repoRoot)
    {
        InitExtensionEolMap(repoRoot);

        if (ExtensionEolMap.TryGetValue("*", out var defaultEol) && defaultEol != null)
        {
            return defaultEol;
        }

        return "LF";
    }

    static bool IsTextExtension(string ext)
    {
        if (string.IsNullOrEmpty(ext)) return false;
        var normalizedExt = ext.ToLowerInvariant();
        if (!normalizedExt.StartsWith(".")) normalizedExt = "." + normalizedExt;
        if (ExtensionEolMap.Count == 0)
        {
            foreach (var defaultExt in DefaultTextExtensions)
            {
                ExtensionEolMap[defaultExt] = null;
            }
        }
        return ExtensionEolMap.ContainsKey(normalizedExt);
    }
}
