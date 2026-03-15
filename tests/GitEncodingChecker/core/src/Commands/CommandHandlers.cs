using System.Diagnostics;
using System.Reflection;

/// <summary>
/// 所有命令处理方法的集合
/// 用 [GitCommand] 特性标记的方法会自动注册
/// </summary>
public static class CommandHandlers
{
    #region 核心检查命令

    [GitCommand("--check", "检查暂存区文件编码", GitAlias = "ec-check")]
    public static int CheckEncoding()
    {
        if (!GitPathResolver.ValidateGitAvailable())
        {
            GitPathResolver.ShowGitNotFoundHelp();
            return 1;
        }

        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("❌ 无法确定 Git 仓库根目录");
            return 1;
        }

        var files = GetStagedFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("ℹ️  没有暂存的文件需要检查");
            return 0;
        }

        Console.WriteLine($"🔍 正在检查 {files.Count} 个暂存文件...\n");

        // 使用 PLINQ 并行处理文件检测 - 简单链式编程
        var results = files
            .AsParallel()                           // 启用并行
            .WithDegreeOfParallelism(Environment.ProcessorCount)  // 使用所有CPU核心
            .Select(file =>
            {
                var fullPath = Path.Combine(repoRoot, file);
                if (!File.Exists(fullPath))
                    return (File: file, Skip: true, SkipReason: "文件不存在", HasError: false,
                            Encoding: "", LineEnding: "", HasBlank: false, EncodingOk: true, LineEndingOk: true);

                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!AppConfig.TextExtensions.Contains(ext))
                    return (File: file, Skip: true, SkipReason: "非文本文件", HasError: false,
                            Encoding: "", LineEnding: "", HasBlank: false, EncodingOk: true, LineEndingOk: true);

                if (AppConfig.SkipExtensions.Contains(ext))
                    return (File: file, Skip: true, SkipReason: AppConfig.SkipReasonPowerShell, HasError: false,
                            Encoding: "", LineEnding: "", HasBlank: false, EncodingOk: true, LineEndingOk: true);

                // 执行检测
                var encoding = EncodingChecker.DetectEncoding(fullPath);
                var lineEnding = EncodingChecker.DetectLineEnding(fullPath);
                var hasBlank = EncodingChecker.HasBlankLines(fullPath);

                bool encodingOk = encoding == "UTF-8" || encoding == "ASCII";
                bool lineEndingOk = lineEnding != "Mixed";
                bool isOk = encodingOk && lineEndingOk && !hasBlank;

                return (File: file, Skip: false, SkipReason: "", HasError: !isOk,
                        Encoding: encoding, LineEnding: lineEnding, HasBlank: hasBlank,
                        EncodingOk: encodingOk, LineEndingOk: lineEndingOk);
            })
            .AsSequential()  // 恢复顺序以便按原始顺序输出
            .ToList();

        // 分类收集结果（线程安全已在ToList后）
        var passedFiles = new List<string>();
        var skippedFiles = new List<(string File, string Reason)>();
        var errorFiles = new List<(string File, string Encoding, string LineEnding, bool HasBlank, bool EncodingOk, bool LineEndingOk)>();

        bool hasError = false;
        foreach (var result in results)
        {
            if (result.Skip)
            {
                skippedFiles.Add((result.File, result.SkipReason));
            }
            else if (result.HasError)
            {
                hasError = true;
                errorFiles.Add((result.File, result.Encoding, result.LineEnding, result.HasBlank, result.EncodingOk, result.LineEndingOk));
            }
            else
            {
                passedFiles.Add(result.File);
            }
        }

        // 按类别输出结果
        // 1. 通过检测的文件
        if (passedFiles.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ 通过检测 ({passedFiles.Count}个):");
            Console.ResetColor();
            foreach (var file in passedFiles)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    {file}");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        // 2. 跳过检测的文件
        if (skippedFiles.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⏭️  跳过检测 ({skippedFiles.Count}个):");
            Console.ResetColor();
            foreach (var (file, reason) in skippedFiles)
            {
                Console.WriteLine($"    {file} ({reason})");
            }
            Console.WriteLine();
        }

        // 3. 检测失败的文件
        if (errorFiles.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ 检测失败 ({errorFiles.Count}个):");
            Console.ResetColor();
            foreach (var (file, encoding, lineEnding, hasBlankFile, encodingOk, lineEndingOk) in errorFiles)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    {file}");
                Console.ResetColor();

                if (!encodingOk)
                    Console.WriteLine($"        编码: {encoding} (应为 UTF-8 无 BOM)");
                if (!lineEndingOk)
                    Console.WriteLine($"        行尾: {lineEnding} (混合换行符)");
                if (hasBlankFile)
                    Console.WriteLine($"        空白: 包含空白行");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
        if (hasError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(AppConfig.MessageCheckFailed);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("💡 修复方法:");
            Console.WriteLine($"  {AppConfig.FixHints.FixCommand}");
            Console.WriteLine($"  {AppConfig.FixHints.CommitCommand}");
            return 1;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(AppConfig.MessageCheckPassed);
            Console.ResetColor();
            return 0;
        }
    }

    [GitCommand("--fix", "修复暂存区文件编码", GitAlias = "ec-fix")]
    public static int FixEncoding()
    {
        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("❌ 无法确定 Git 仓库根目录");
            return 1;
        }

        var files = GetStagedFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("ℹ️  没有暂存的文件需要修复");
            return 0;
        }

        Console.WriteLine($"🔧 正在检查 {files.Count} 个暂存文件...\n");

        int fixedCount = 0;

        foreach (var file in files)
        {
            var fullPath = Path.Combine(repoRoot, file);
            if (!File.Exists(fullPath)) continue;

            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!AppConfig.TextExtensions.Contains(ext)) continue;
            if (AppConfig.SkipExtensions.Contains(ext)) continue;

            // 修复编码
            if (EncodingChecker.FixEncoding(fullPath, out var encMsg))
            {
                Console.WriteLine($"  ✓ {file}: {encMsg}");
                fixedCount++;
            }

            // 修复行尾
            var targetEol = GetTargetLineEnding(repoRoot, file);
            if (EncodingChecker.FixLineEnding(fullPath, targetEol, out var eolMsg))
            {
                Console.WriteLine($"  ✓ {file}: {eolMsg}");
                fixedCount++;
            }

            // 移除空白行
            if (EncodingChecker.RemoveBlankLines(fullPath, out var blankMsg))
            {
                Console.WriteLine($"  ✓ {file}: {blankMsg}");
                fixedCount++;
            }
        }

        Console.WriteLine();
        if (fixedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ 已修复 {fixedCount} 处问题");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("💡 请重新暂存修复后的文件:");
            Console.WriteLine("  git add .");
        }
        else
        {
            Console.WriteLine("ℹ️  没有需要修复的文件");
        }

        return 0;
    }

    #endregion

    #region 安装命令

    [GitCommand("--install", "安装 Git 命令别名（交互式，支持 --force 非交互模式，--target project|global|both 指定目标）", GitAlias = "ec-install")]
    public static int Install(string[]? args = null)
    {
        var force = args?.Contains("--force") ?? false;
        var target = GetTargetFromArgs(args);

        if (force && target != null)
        {
            // 强制模式 + 指定目标
            switch (target)
            {
                case "project":
                    InstallManager.InstallProjectLevel(force: true);
                    break;
                case "global":
                    InstallManager.InstallGlobalLevel(force: true);
                    break;
                case "both":
                    InstallManager.InstallProjectLevel(force: true);
                    InstallManager.InstallGlobalLevel(force: true);
                    break;
            }
        }
        else if (force)
        {
            // 强制模式：默认安装项目级别
            InstallManager.InstallProjectLevel(force: true);
        }
        else
        {
            InstallManager.InteractiveInstall();
        }
        return 0;
    }

    /// <summary>
    /// 从参数中提取 --target 的值
    /// </summary>
    private static string? GetTargetFromArgs(string[]? args)
    {
        if (args == null) return null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--target" && i + 1 < args.Length)
            {
                return args[i + 1].ToLowerInvariant();
            }
        }
        return null;
    }

    [GitCommand("--uninstall", "卸载 Git 命令别名（支持 --target project|global|both 指定目标）", GitAlias = "ec-uninstall")]
    public static int Uninstall(string[]? args = null)
    {
        var target = GetTargetFromArgs(args);

        if (target != null)
        {
            // 指定了目标，直接执行
            switch (target)
            {
                case "project":
                    InstallManager.UninstallProjectLevel();
                    return 0;
                case "global":
                    InstallManager.UninstallGlobalLevel();
                    return 0;
                case "both":
                    InstallManager.UninstallProjectLevel();
                    InstallManager.UninstallGlobalLevel();
                    return 0;
                default:
                    Console.WriteLine($"❌ 无效的目标: {target}");
                    return 1;
            }
        }

        // 交互式模式
        Console.WriteLine();
        Console.WriteLine("请选择要卸载的级别：");
        Console.WriteLine("  [1] 项目级别");
        Console.WriteLine("  [2] 全局级别");
        Console.WriteLine("  [3] 两者都卸载");
        Console.WriteLine("  [Q] 取消");
        Console.WriteLine();

        while (true)
        {
            Console.Write("请输入选项: ");
            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            switch (choice)
            {
                case "1":
                    InstallManager.UninstallProjectLevel();
                    return 0;
                case "2":
                    InstallManager.UninstallGlobalLevel();
                    return 0;
                case "3":
                    InstallManager.UninstallProjectLevel();
                    InstallManager.UninstallGlobalLevel();
                    return 0;
                case "Q":
                    return 0;
                default:
                    Console.WriteLine("❌ 无效选项");
                    break;
            }
        }
    }

    [GitCommand("--install-global", "全局安装（本机所有仓库生效，支持 --force 非交互模式）", GitAlias = "ec-global")]
    public static int InstallGlobal(string[]? args = null)
    {
        var force = args?.Contains("--force") ?? false;
        InstallManager.InstallGlobalLevel(force: force);
        return 0;
    }

    [GitCommand("--uninstall-global", "卸载全局安装", GitAlias = "ec-uninstall-global")]
    public static int UninstallGlobal()
    {
        InstallManager.UninstallGlobalLevel();
        return 0;
    }

    [GitCommand("--clean", "全面清理（项目和全局）", GitAlias = "ec-clean")]
    public static int Clean()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  这将删除所有 EncodingChecker 的安装和配置！");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("确定要继续吗？(y/N): ");

        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (confirm == "y" || confirm == "yes")
        {
            InstallManager.CleanAll();
        }
        else
        {
            Console.WriteLine("已取消清理。");
        }

        return 0;
    }

    #endregion

    #region 提交辅助命令

    [GitCommand("--convert-commit", "修复编码并提交", GitAlias = "ec-m")]
    public static int ConvertAndCommit(string[] args)
    {
        // 格式: git ec-m "msg"
        if (args.Length == 0)
        {
            var lastMsg = GetLastCommitMessage();
            if (!string.IsNullOrEmpty(lastMsg))
            {
                Console.WriteLine($"使用提交信息: \"{lastMsg}\"\n");
                args = new[] { "-m", lastMsg };
            }
            else
            {
                Console.Write("请输入提交信息: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("❌ 提交信息不能为空");
                    return 1;
                }
                args = new[] { "-m", input.Trim() };
            }
        }
        // 将参数转换为 -m 格式供 git commit 使用
        else
        {
            args = new[] { "-m", args[0] };
        }

        // 步骤1: 修复编码
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("步骤 1/2: 修复文件编码...");
        Console.WriteLine("===========================================");
        Console.ResetColor();
        FixEncoding();

        // 步骤2: 重新暂存并提交
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("步骤 2/2: 提交...");
        Console.WriteLine("===========================================");
        Console.ResetColor();

        GitCommandRunner.Run("add", ".");

        var psi = new ProcessStartInfo
        {
            FileName = GitPathResolver.GetGitPath(),
            UseShellExecute = false
        };
        psi.ArgumentList.Add("commit");
        psi.ArgumentList.Add("--no-verify"); // 跳过 pre-commit hook，避免循环调用
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
        return proc?.ExitCode ?? 0;
    }

    [GitCommand("--commit", "跳过检查强制提交", GitAlias = "ec-force")]
    public static int SkipCheckCommit(string[] args)
    {
        // 格式: git ec-force "msg"
        if (args.Length == 0)
        {
            Console.WriteLine("❌ 用法: git ec-force \"提交信息\"");
            return 1;
        }
        // 将参数转换为 -m 格式供 git commit 使用
        args = new[] { "-m", args[0] };

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  跳过编码检查，强制提交...");
        Console.ResetColor();

        var psi = new ProcessStartInfo
        {
            FileName = GitPathResolver.GetGitPath(),
            UseShellExecute = false
        };
        psi.ArgumentList.Add("commit");
        psi.ArgumentList.Add("--no-verify");
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
        return proc?.ExitCode ?? 0;
    }

    #endregion

    #region 诊断命令

    [GitCommand("--check-local-aliases", "检查本地 Git 别名配置", GitAlias = "ec-check-local")]
    public static int CheckLocalAliases()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔍 检查本地 Git 别名配置...");
        Console.ResetColor();
        Console.WriteLine();

        var repoRoot = InstallManager.GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("❌ 当前目录不在 Git 仓库内");
            return 1;
        }

        Console.WriteLine($"仓库路径: {repoRoot}");
        Console.WriteLine();

        var localAliases = InstallManager.CheckLocalAliases();

        if (localAliases.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 未发现本地 Git 别名");
            Console.ResetColor();
            return 0;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️  发现 {localAliases.Count} 个本地 Git 别名:");
        Console.WriteLine();

        foreach (var alias in localAliases)
        {
            try
            {
                var value = GitCommandRunner.RunWithOutput("config", "--local", $"alias.{alias}");
                Console.WriteLine($"  git {alias}");
                Console.WriteLine($"     → {value}");
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine($"  git {alias}");
                Console.WriteLine($"     → (无法读取配置)");
                Console.WriteLine();
            }
        }

        Console.ResetColor();
        Console.WriteLine("💡 提示: 本地别名会覆盖全局别名，可能导致路径错误。");
        Console.WriteLine("   如需清理，请运行: git ec-uninstall --target project");
        Console.WriteLine();

        return 0;
    }

    [GitCommand("--clean-local-aliases", "清理本地 Git 别名", GitAlias = "ec-clean-local")]
    public static int CleanLocalAliases()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🧹 清理本地 Git 别名...");
        Console.ResetColor();
        Console.WriteLine();

        var repoRoot = InstallManager.GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("❌ 当前目录不在 Git 仓库内");
            return 1;
        }

        var localAliases = InstallManager.CheckLocalAliases();

        if (localAliases.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 没有需要清理的本地别名");
            Console.ResetColor();
            return 0;
        }

        Console.WriteLine($"将清理以下 {localAliases.Count} 个本地别名:");
        foreach (var alias in localAliases)
        {
            Console.WriteLine($"  - git {alias}");
        }
        Console.WriteLine();

        Console.Write("确认清理? (y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm == "y" || confirm == "yes")
        {
            InstallManager.UnregisterGitAliases(isGlobal: false);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 本地别名已清理");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("已取消清理。");
        }

        return 0;
    }

    #endregion

    #region 帮助命令

    [GitCommand("--help", "显示帮助信息", GitAlias = "ec-help")]
    public static int ShowHelp()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           EncodingChecker - Git 编码检查工具                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("📋 可用命令:\n");

        // 动态从 GitCommandAttribute 获取命令信息
        var commands = typeof(CommandHandlers)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(m => new { Method = m, Attr = m.GetCustomAttribute<GitCommandAttribute>() })
            .Where(x => x.Attr is not null)
            .Select(x => new
            {
                Cmd = x.Attr!.Name,
                Alias = x.Attr!.GitAlias != null ? $"git {x.Attr.GitAlias}" : x.Attr.Name,
                Desc = x.Attr!.Description,
                Order = GetCommandOrder(x.Attr.Name)
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Cmd)
            .ToList();

        // 按类别分组显示
        var coreCommands = commands.Where(c =>
            c.Cmd is "--check" or "--fix" or "--convert-commit" or "--commit").ToList();
        var installCommands = commands.Where(c =>
            c.Cmd is "--install" or "--install-global" or "--uninstall" or "--uninstall-global").ToList();
        var otherCommands = commands.Where(c =>
            c.Cmd == "--help").ToList();

        // 显示核心命令
        foreach (var cmd in coreCommands)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {cmd.Alias,-20}");
            Console.ResetColor();
            Console.WriteLine(cmd.Desc);
        }

        Console.WriteLine();

        // 显示安装命令
        foreach (var cmd in installCommands)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {cmd.Alias,-20}");
            Console.ResetColor();
            Console.WriteLine(cmd.Desc);
        }

        Console.WriteLine();

        // 显示其他命令
        foreach (var cmd in otherCommands)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {cmd.Alias,-20}");
            Console.ResetColor();
            Console.WriteLine(cmd.Desc);
        }

        Console.WriteLine();
        Console.WriteLine("💡 示例:\n");
        Console.WriteLine("  git add .");
        Console.WriteLine("  git ec-check              # 检查编码");
        Console.WriteLine("  git ec-fix                # 修复问题");
        Console.WriteLine("  git add .");
        Console.WriteLine("  git ec-m \"feat: xxx\"     # 修复并提交");
        Console.WriteLine();

        return 0;
    }

    /// <summary>
    /// 获取命令显示顺序
    /// </summary>
    private static int GetCommandOrder(string cmd) => cmd switch
    {
        "--check" => 1,
        "--fix" => 2,
        "--convert-commit" => 3,
        "--commit" => 4,
        "--install" => 10,
        "--install-global" => 11,
        "--uninstall" => 12,
        "--uninstall-global" => 13,
        _ => 99
    };

    #endregion

    #region 私有辅助方法

    private static string GetRepoRoot()
    {
        try
        {
            return GitCommandRunner.RunWithOutput("rev-parse", "--show-toplevel").Trim();
        }
        catch
        {
            return "";
        }
    }

    private static List<string> GetStagedFiles()
    {
        return GitCommandRunner.RunWithLines("diff", "--cached", "--name-only", "--diff-filter=ACM").ToList();
    }

    private static string? GetLastCommitMessage()
    {
        try
        {
            var repoRoot = GetRepoRoot();
            if (string.IsNullOrEmpty(repoRoot)) return null;

            var path = Path.Combine(repoRoot, ".git", "COMMIT_EDITMSG");
            if (!File.Exists(path)) return null;

            var content = File.ReadAllText(path);
            var lines = content.Split('\n')
                .Where(l => !l.TrimStart().StartsWith("#") && !string.IsNullOrWhiteSpace(l))
                .ToList();

            return lines.Count > 0 ? string.Join("\n", lines).Trim() : null;
        }
        catch
        {
            return null;
        }
    }



    private static string GetTargetLineEnding(string repoRoot, string filePath)
    {
        // 简化版：从 .editorconfig 读取
        try
        {
            var configPath = Path.Combine(repoRoot, AppConfig.EditorConfigFileName);
            if (File.Exists(configPath))
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith(AppConfig.EditorConfigEndOfLineKey))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            var eol = parts[1].Trim().ToUpperInvariant();
                            return eol == AppConfig.LineEndingNames.Crlf ? AppConfig.LineEndingNames.Crlf : AppConfig.LineEndingNames.Lf;
                        }
                    }
                }
            }
        }
        catch { }

        return AppConfig.DefaultLineEnding;
    }

    #endregion
}
