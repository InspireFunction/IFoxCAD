using System.Diagnostics;
using System.Reflection;

/// <summary>
/// 管理安装过程 - 项目级别或全局级别
/// </summary>
public static class InstallManager
{
    /// <summary>
    /// 存储备份的别名配置 (alias -> originalValue)
    /// </summary>
    internal static readonly Dictionary<string, string> _backupMap = new();

    /// <summary>
    /// 自定义前缀（用户选择新前缀选项时使用）
    /// </summary>
    internal static string? _customPrefix;

    /// <summary>
    /// 反射获取所有需要注册的Git别名
    /// </summary>
    internal static List<(string command, string alias, string description)> GetAllAliasesFromReflection()
    {
        var aliases = new List<(string command, string alias, string description)>();
        var assembly = Assembly.GetExecutingAssembly();

        // 获取所有类型中的方法
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<GitCommandAttribute>();
                if (attr != null && attr.RegisterAlias && !string.IsNullOrEmpty(attr.GitAlias))
                {
                    aliases.Add((attr.Name, attr.GitAlias, attr.Description));
                }
            }
        }

        return aliases;
    }

    /// <summary>
    /// 检查别名冲突，返回冲突列表
    /// </summary>
    internal static List<(string alias, string existingValue, string newValue)> CheckAliasConflicts(
        List<(string command, string alias, string description)> aliases, 
        bool isGlobal)
    {
        var conflicts = new List<(string alias, string existingValue, string newValue)>();

        foreach (var (command, alias, description) in aliases)
        {
            var existingValue = GetGitConfigValue($"alias.{alias}", isGlobal);
            if (!string.IsNullOrEmpty(existingValue))
            {
                // 构建新的别名值
                var exePath = GetExePath(isGlobal);
                var exePathForGit = exePath.Replace("\\", "/");
                var newValue = command == "--check"
                    ? $"!\"{exePathForGit}\""
                    : $"!\"{exePathForGit}\" {command}";

                // 如果值完全相同，说明是我们自己之前设置的，不算冲突
                if (!existingValue.Equals(newValue, StringComparison.Ordinal))
                {
                    conflicts.Add((alias, existingValue, newValue));
                }
            }
        }

        return conflicts;
    }

    /// <summary>
    /// 获取Git配置值
    /// </summary>
    internal static string GetGitConfigValue(string key, bool isGlobal)
    {
        try
        {
            var args = new List<string> { "config" };
            if (isGlobal) args.Add("--global");
            args.Add("--get");
            args.Add(key);

            var output = GitCommandRunner.RunWithOutput(args.ToArray()).Trim();
            return output;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 获取可执行文件路径
    /// </summary>
    internal static string GetExePath(bool isGlobal)
    {
        if (isGlobal)
        {
            var globalHooksDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".git-hooks"
            );
            return Path.Combine(globalHooksDir, "EncodingChecker.exe");
        }
        else
        {
            var repoRoot = GetRepoRoot();
            return Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe");
        }
    }

    /// <summary>
    /// 备份现有别名配置
    /// </summary>
    internal static void BackupExistingAliases(List<(string alias, string existingValue, string newValue)> conflicts, bool isGlobal)
    {
        _backupMap.Clear();

        foreach (var (alias, existingValue, _) in conflicts)
        {
            _backupMap[alias] = existingValue;

            // 将原有配置注释保留到Git配置中（使用特殊的注释键）
            var backupKey = $"alias.backup-{alias}-{DateTime.Now:yyyyMMdd-HHmmss}";
            var args = new List<string> { "config" };
            if (isGlobal) args.Add("--global");
            args.Add(backupKey);
            args.Add($"# 原别名 {alias} 的备份: {existingValue}");

            try
            {
                GitCommandRunner.Run(args.ToArray());
            }
            catch { }
        }
    }

    /// <summary>
    /// 交互式处理冲突
    /// </summary>
    /// <returns>true表示继续安装，false表示取消</returns>
    private static bool HandleConflictsInteractively(List<(string alias, string existingValue, string newValue)> conflicts, bool isGlobal)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️  检测到 {conflicts.Count} 个Git别名冲突:");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var (alias, existingValue, newValue) in conflicts)
        {
            Console.WriteLine($"  别名: git {alias}");
            Console.WriteLine($"  现有: {existingValue}");
            Console.WriteLine($"  新值: {newValue}");
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("请选择处理方式:");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  [O] 覆盖   - 使用新别名，原有配置将注释保留");
        Console.WriteLine("  [S] 跳过   - 保留现有别名，不安装冲突的别名");
        Console.WriteLine("  [P] 新前缀 - 为所有别名添加自定义前缀");
        Console.WriteLine("  [C] 取消   - 取消安装");
        Console.WriteLine();

        while (true)
        {
            Console.Write("请输入选项 (O/S/P/C): ");
            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            switch (choice)
            {
                case "O":
                case "覆盖":
                    BackupExistingAliases(conflicts, isGlobal);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ 已备份 {conflicts.Count} 个原有配置");
                    Console.ResetColor();
                    return true;

                case "S":
                case "跳过":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ 将跳过冲突的别名");
                    Console.ResetColor();
                    // 将冲突的别名标记为跳过
                    foreach (var (alias, _, _) in conflicts)
                    {
                        _backupMap[alias] = "__SKIP__";
                    }
                    return true;

                case "P":
                case "新前缀":
                case "前缀":
                    Console.Write("请输入新的前缀 (例如: 'my' 将生成 git my-ec): ");
                    var prefix = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        // 使用新前缀重新生成别名列表
                        return HandlePrefixOption(conflicts, prefix, isGlobal);
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ 前缀不能为空");
                    Console.ResetColor();
                    break;

                case "C":
                case "取消":
                case "Q":
                    Console.WriteLine("已取消安装。");
                    return false;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ 无效选项");
                    Console.ResetColor();
                    break;
            }
        }
    }

    /// <summary>
    /// 处理前缀选项
    /// </summary>
    private static bool HandlePrefixOption(List<(string alias, string existingValue, string newValue)> conflicts, string prefix, bool isGlobal)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"将使用前缀 '{prefix}' 生成新别名:");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var (alias, _, _) in conflicts)
        {
            var newAlias = $"{prefix}-{alias}";
            Console.WriteLine($"  git {alias} -> git {newAlias}");
        }

        Console.WriteLine();
        Console.Write("确认使用此前缀? (Y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm == "n" || confirm == "no")
        {
            return HandleConflictsInteractively(conflicts, isGlobal);
        }

        // 存储前缀供后续使用
        _customPrefix = prefix;
        return true;
    }

    /// <summary>
    /// 交互式询问用户安装级别
    /// </summary>
    public static void InteractiveInstall()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🚀 准备安装 EncodingChecker");
        Console.ResetColor();
        Console.WriteLine();

        // 检查是否已经在某处安装
        var (projectInstalled, globalInstalled) = CheckExistingInstallation();

        if (projectInstalled && globalInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️ 检测到项目级别和全局级别都已安装");
            Console.ResetColor();
            Console.WriteLine();
        }
        else if (projectInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 已检测到项目级别安装");
            Console.ResetColor();
            Console.WriteLine();
        }
        else if (globalInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ 已检测到全局级别安装");
            Console.ResetColor();
            Console.WriteLine();
        }

        // 显示选项
        Console.WriteLine("请选择安装方式：");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  [1] 项目级别 - 仅当前仓库生效（推荐）");
        Console.ResetColor();
        Console.WriteLine("      • Git 别名: git ec, git ec-fix, git ecc ...");
        Console.WriteLine("      • 配置文件: 本仓库的 .git/config");
        Console.WriteLine("      • 适用场景: 团队协作，每个仓库独立配置");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [2] 全局级别 - 本机所有仓库生效");
        Console.ResetColor();
        Console.WriteLine("      • Git 别名: git ec, git ec-fix, git ecc ...");
        Console.WriteLine("      • 配置文件: 用户目录的 .gitconfig");
        Console.WriteLine("      • 适用场景: 个人开发，统一管理");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  [3] 两者都安装");
        Console.ResetColor();
        Console.WriteLine("      • 项目级别优先于全局级别");
        Console.WriteLine("      • 最灵活但可能让人困惑");
        Console.WriteLine();

        Console.WriteLine("  [Q] 取消安装");
        Console.WriteLine();

        // 获取用户输入
        while (true)
        {
            Console.Write("请输入选项 (1/2/3/Q): ");
            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            switch (choice)
            {
                case "1":
                    if (InstallProjectLevel())
                        ShowPostInstallHints(isGlobal: false);
                    return;

                case "2":
                    if (InstallGlobalLevel())
                        ShowPostInstallHints(isGlobal: true);
                    return;

                case "3":
                    var projectSuccess = InstallProjectLevel();
                    var globalSuccess = InstallGlobalLevel();
                    if (projectSuccess || globalSuccess)
                        ShowPostInstallHints(isGlobal: true);
                    return;

                case "Q":
                case "":
                    Console.WriteLine();
                    Console.WriteLine("已取消安装。");
                    Console.WriteLine("如需稍后安装，请运行: .\\发布.ps1");
                    return;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ 无效选项，请重新输入");
                    Console.ResetColor();
                    break;
            }
        }
    }

    /// <summary>
    /// 项目级别安装
    /// </summary>
    /// <param name="force">是否强制安装（非交互模式）</param>
    /// <returns>true表示安装成功</returns>
    public static bool InstallProjectLevel(bool force = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📦 正在安装到项目级别...");
        Console.ResetColor();

        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ 错误: 无法确定 Git 仓库根目录");
            Console.ResetColor();
            return false;
        }

        var hooksDir = Path.Combine(repoRoot, ".git", "hooks");
        var exePath = Path.Combine(hooksDir, "EncodingChecker.exe");

        // 确保 exe 存在
        if (!File.Exists(exePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 错误: 找不到 {exePath}");
            Console.WriteLine("请先运行发布脚本: pwsh -File 发布.ps1");
            Console.ResetColor();
            return false;
        }

        // 获取所有别名并检查冲突
        var aliases = GetAllAliasesFromReflection();
        var conflicts = CheckAliasConflicts(aliases, isGlobal: false);

        // 如果有冲突，处理冲突
        if (conflicts.Count > 0)
        {
            if (force)
            {
                // 强制模式：自动备份并覆盖
                Console.WriteLine($"检测到 {conflicts.Count} 个冲突，强制模式将自动备份并覆盖...");
                BackupExistingAliases(conflicts, isGlobal: false);
            }
            else
            {
                // 交互式处理
                if (!HandleConflictsInteractively(conflicts, isGlobal: false))
                {
                    return false;
                }
            }
        }

        // 注册别名
        RegisterAliases(repoRoot, isGlobal: false, aliases);

        // 设置编码配置
        ConfigureGitEncoding(isGlobal: false);

        // 获取 Git 配置文件路径
        var gitConfigPath = GetGitConfigFilePath(isGlobal: false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 项目级别安装完成！");
        Console.WriteLine($"  Git 配置: {gitConfigPath}");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// 全局级别安装
    /// </summary>
    /// <param name="force">是否强制安装（非交互模式，此时假设文件已由外部脚本复制）</param>
    /// <returns>true表示安装成功</returns>
    public static bool InstallGlobalLevel(bool force = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🌍 正在安装到全局级别...");
        Console.ResetColor();

        // 获取全局 hooks 目录
        var globalHooksDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".git-hooks"
        );

        Directory.CreateDirectory(globalHooksDir);

        var destExe = Path.Combine(globalHooksDir, "EncodingChecker.exe");

        // 如果不是强制模式，需要自己复制文件
        if (!force)
        {
            // 复制 exe
            var sourceExe = FindSourceExe();
            if (sourceExe == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ 错误: 找不到 EncodingChecker.exe 源文件");
                Console.ResetColor();
                return false;
            }

            // 检查目标文件是否被占用
            if (File.Exists(destExe))
            {
                try
                {
                    using var stream = File.Open(destExe, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (IOException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ 错误: 目标文件被占用，无法覆盖: {destExe}");
                    Console.WriteLine("请关闭占用该文件的程序后重试。");
                    Console.ResetColor();
                    return false;
                }
            }

            try
            {
                File.Copy(sourceExe, destExe, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 复制文件失败: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
        else
        {
            // 强制模式：假设文件已由外部脚本复制，只检查文件是否存在
            if (!File.Exists(destExe))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 错误: 找不到 EncodingChecker.exe: {destExe}");
                Console.WriteLine("请确保文件已正确复制到全局目录。");
                Console.ResetColor();
                return false;
            }
        }

        // 获取所有别名并检查冲突
        var aliases = GetAllAliasesFromReflection();
        var conflicts = CheckAliasConflicts(aliases, isGlobal: true);

        // 如果有冲突，处理冲突
        if (conflicts.Count > 0)
        {
            if (force)
            {
                // 强制模式：自动备份并覆盖
                Console.WriteLine($"检测到 {conflicts.Count} 个冲突，强制模式将自动备份并覆盖...");
                BackupExistingAliases(conflicts, isGlobal: true);
            }
            else
            {
                // 交互式处理
                if (!HandleConflictsInteractively(conflicts, isGlobal: true))
                {
                    return false;
                }
            }
        }

        // 注册别名
        RegisterAliases(globalHooksDir, isGlobal: true, aliases);

        // 设置编码配置
        ConfigureGitEncoding(isGlobal: true);

        // 获取 Git 配置文件路径
        var gitConfigPath = GetGitConfigFilePath(isGlobal: true);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ 全局级别安装完成！");
        Console.WriteLine($"  程序位置: {destExe}");
        Console.WriteLine($"  Git 配置: {gitConfigPath}");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// 卸载项目级别
    /// </summary>
    public static void UninstallProjectLevel()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🗑️  正在卸载项目级别...");
        Console.ResetColor();

        UnregisterAliases(isGlobal: false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 项目级别已卸载");
        Console.ResetColor();
    }

    /// <summary>
    /// 卸载全局级别
    /// </summary>
    public static void UninstallGlobalLevel()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🗑️  正在卸载全局级别...");
        Console.ResetColor();

        UnregisterAliases(isGlobal: true);

        // 清理 core.hooksPath 配置
        try
        {
            GitCommandRunner.Run("config", "--global", "--unset", "core.hooksPath");
            Console.WriteLine("  已清理全局 hooks 路径配置");
        }
        catch { }

        // 删除全局 exe
        var globalHooksDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".git-hooks"
        );
        var exePath = Path.Combine(globalHooksDir, "EncodingChecker.exe");

        if (File.Exists(exePath))
        {
            File.Delete(exePath);
            Console.WriteLine($"  已删除: {exePath}");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 全局级别已卸载");
        Console.ResetColor();
    }

    /// <summary>
    /// 清理所有安装（项目和全局）
    /// </summary>
    public static void CleanAll()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🧹 开始全面清理...");
        Console.ResetColor();
        Console.WriteLine();

        // 清理项目级别
        var repoRoot = GetRepoRoot();
        if (!string.IsNullOrEmpty(repoRoot))
        {
            var projectExe = Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe");
            if (File.Exists(projectExe))
            {
                try
                {
                    File.Delete(projectExe);
                    Console.WriteLine($"  ✓ 删除项目 exe: {projectExe}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ 无法删除项目 exe: {ex.Message}");
                }
            }
        }

        // 清理全局级别
        var globalHooksDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".git-hooks"
        );
        var globalExe = Path.Combine(globalHooksDir, "EncodingChecker.exe");
        if (File.Exists(globalExe))
        {
            try
            {
                File.Delete(globalExe);
                Console.WriteLine($"  ✓ 删除全局 exe: {globalExe}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ 无法删除全局 exe: {ex.Message}");
            }
        }

        // 清理空目录
        if (Directory.Exists(globalHooksDir) && Directory.GetFiles(globalHooksDir).Length == 0)
        {
            try
            {
                Directory.Delete(globalHooksDir);
                Console.WriteLine($"  ✓ 删除空目录: {globalHooksDir}");
            }
            catch { }
        }

        // 清理 Git 别名（项目和全局）
        var aliases = GetAllAliasesFromReflection().Select(a => a.alias).ToList();
        // 添加可能的前缀版本
        var allAliases = new List<string>(aliases);
        allAliases.AddRange(aliases.Select(a => $"my-{a}"));
        allAliases.AddRange(aliases.Select(a => $"custom-{a}"));

        // 去重
        allAliases = allAliases.Distinct().ToList();

        // 清理项目级别别名
        if (!string.IsNullOrEmpty(repoRoot))
        {
            foreach (var alias in allAliases)
            {
                try
                {
                    GitCommandRunner.Run("config", "--unset", $"alias.{alias}");
                }
                catch { }
            }
            Console.WriteLine("  ✓ 清理项目级别 Git 别名");
        }

        // 清理全局级别别名
        foreach (var alias in allAliases)
        {
            try
            {
                GitCommandRunner.Run("config", "--global", "--unset", $"alias.{alias}");
            }
            catch { }
        }
        Console.WriteLine("  ✓ 清理全局级别 Git 别名");

        // 清理备份的别名配置
        if (!string.IsNullOrEmpty(repoRoot))
        {
            try
            {
                var output = GitCommandRunner.RunWithOutput("config", "--local", "--list");
                var backupKeys = output.Split('\n')
                    .Where(line => line.Contains("alias.backup-"))
                    .Select(line => line.Split('=')[0])
                    .ToList();

                foreach (var key in backupKeys)
                {
                    try
                    {
                        GitCommandRunner.Run("config", "--unset", key);
                    }
                    catch { }
                }
            }
            catch { }
        }

        try
        {
            var output = GitCommandRunner.RunWithOutput("config", "--global", "--list");
            var backupKeys = output.Split('\n')
                .Where(line => line.Contains("alias.backup-"))
                .Select(line => line.Split('=')[0])
                .ToList();

            foreach (var key in backupKeys)
            {
                try
                {
                    GitCommandRunner.Run("config", "--global", "--unset", key);
                }
                catch { }
            }
        }
        catch { }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ 全面清理完成！");
        Console.ResetColor();
    }

    #region 私有方法

    private static (bool projectInstalled, bool globalInstalled) CheckExistingInstallation()
    {
        bool project = false, global = false;

        // 检查项目级别
        var repoRoot = GetRepoRoot();
        if (!string.IsNullOrEmpty(repoRoot))
        {
            var projectExe = Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe");
            project = File.Exists(projectExe);
        }

        // 检查全局级别
        var globalExe = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".git-hooks", "EncodingChecker.exe"
        );
        global = File.Exists(globalExe);

        return (project, global);
    }

    private static void RegisterAliases(string exeBasePath, bool isGlobal, List<(string command, string alias, string description)>? aliases = null)
    {
        var exePath = Path.Combine(exeBasePath, isGlobal ? "EncodingChecker.exe" : ".git/hooks/EncodingChecker.exe");
        var exePathForGit = exePath.Replace("\\", "/");

        // 如果没有提供别名列表，使用反射获取
        aliases ??= GetAllAliasesFromReflection();

        foreach (var (command, alias, desc) in aliases)
        {
            // 如果用户选择了自定义前缀，应用前缀
            var finalAlias = !string.IsNullOrEmpty(_customPrefix) ? $"{_customPrefix}-{alias}" : alias;

            // 检查这个别名是否应该被跳过（在冲突处理时用户选择跳过）
            if (_backupMap.ContainsKey(alias) && _backupMap[alias] == "__SKIP__")
            {
                Console.WriteLine($"  ⏭️  跳过别名: git {finalAlias}");
                continue;
            }

            var aliasValue = command == "--check"
                ? $"!\"{exePathForGit}\""
                : $"!\"{exePathForGit}\" {command}";

            var args = new List<string> { "config" };
            if (isGlobal) args.Add("--global");
            args.Add($"alias.{finalAlias}");
            args.Add(aliasValue);

            GitCommandRunner.Run(args.ToArray());
            Console.WriteLine($"  ✓ 注册别名: git {finalAlias}");
        }
    }

    private static void UnregisterAliases(bool isGlobal)
    {
        var aliases = GetAllAliasesFromReflection().Select(a => a.alias).ToList();

        foreach (var alias in aliases)
        {
            var args = new List<string> { "config" };
            if (isGlobal) args.Add("--global");
            args.Add("--unset");
            args.Add($"alias.{alias}");

            // 忽略错误（可能本来就不存在）
            try
            {
                GitCommandRunner.Run(args.ToArray());
            }
            catch { }
        }
    }

    private static void ConfigureGitEncoding(bool isGlobal)
    {
        var args = new List<string>();
        if (isGlobal) args.Add("--global");

        GitCommandRunner.Run(args.Concat(new[] { "config", "core.autocrlf", "false" }).ToArray());
        GitCommandRunner.Run(args.Concat(new[] { "config", "i18n.commitencoding", "utf-8" }).ToArray());
        GitCommandRunner.Run(args.Concat(new[] { "config", "i18n.logoutputencoding", "utf-8" }).ToArray());
        GitCommandRunner.Run(args.Concat(new[] { "config", "core.quotepath", "false" }).ToArray());

        if (isGlobal)
        {
            // 全局安装：设置全局 hooks 路径，使 ~/.git-hooks 目录的 hooks 生效
            var globalHooksDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".git-hooks"
            );
            var hooksPathForGit = globalHooksDir.Replace("\\", "/");
            GitCommandRunner.Run("config", "--global", "core.hooksPath", hooksPathForGit);
        }
        else
        {
            // 项目级别设置行尾（从 .editorconfig 读取）
            var eol = DetectLineEndingFromEditorConfig();
            GitCommandRunner.Run("config", "core.eol", eol.ToLowerInvariant());
        }
    }

    private static void ShowPostInstallHints(bool isGlobal)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("💡 使用提示:");
        Console.ResetColor();
        Console.WriteLine();

        // 根据是否有自定义前缀显示不同的提示
        var prefix = !string.IsNullOrEmpty(_customPrefix) ? $"{_customPrefix}-" : "";

        Console.WriteLine($"  git {prefix}ec              # 检查暂存区文件编码");
        Console.WriteLine($"  git {prefix}ec-fix          # 修复编码问题");
        Console.WriteLine($"  git {prefix}ecc -m \"msg\"   # 修复编码并提交");
        Console.WriteLine($"  git {prefix}ec-commit -m \"msg\"  # 跳过检查强制提交");
        Console.WriteLine();

        if (isGlobal)
        {
            Console.WriteLine("  全局安装后，所有仓库都可以使用上述命令");
        }
        else
        {
            Console.WriteLine("  项目安装后，仅当前仓库可以使用上述命令");
        }
    }

    private static string? FindSourceExe()
    {
        // 尝试多个可能的源位置
        var candidates = new List<string>();

        var repoRoot = GetRepoRoot();
        if (!string.IsNullOrEmpty(repoRoot))
        {
            candidates.Add(Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe"));
            candidates.Add(Path.Combine(repoRoot, "tests", "GitEncodingChecker", "bin", "Release", "win-x64", "publish", "EncodingChecker.exe"));
            candidates.Add(Path.Combine(repoRoot, "tests", "GitEncodingChecker", "bin", "Debug", "EncodingChecker.exe"));
        }

        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "EncodingChecker.exe"));

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static string GetRepoRoot()
    {
        try
        {
            var output = GitCommandRunner.RunWithOutput("rev-parse", "--show-toplevel");
            return output.Trim();
        }
        catch
        {
            return "";
        }
    }

    private static string DetectLineEndingFromEditorConfig()
    {
        try
        {
            var repoRoot = GetRepoRoot();
            if (string.IsNullOrEmpty(repoRoot)) return "lf";

            var editorconfigPath = Path.Combine(repoRoot, ".editorconfig");
            if (!File.Exists(editorconfigPath)) return "lf";

            var lines = File.ReadAllLines(editorconfigPath);
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("end_of_line"))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim().ToLowerInvariant();
                    }
                }
            }
        }
        catch { }

        return "lf";
    }

    /// <summary>
    /// 获取 Git 配置文件路径
    /// </summary>
    private static string GetGitConfigFilePath(bool isGlobal)
    {
        try
        {
            if (isGlobal)
            {
                // 获取全局配置文件路径
                var output = GitCommandRunner.RunWithOutput("config", "--global", "--show-origin", "--list");
                // 输出格式: file:C:/Users/xxx/.gitconfig	core.autocrlf=false
                var firstLine = output.Split('\n').FirstOrDefault();
                if (!string.IsNullOrEmpty(firstLine) && firstLine.StartsWith("file:"))
                {
                    var path = firstLine.Substring(5).Split('\t')[0];
                    return path;
                }
                // 默认路径
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".gitconfig");
            }
            else
            {
                // 项目级别配置文件
                var repoRoot = GetRepoRoot();
                if (!string.IsNullOrEmpty(repoRoot))
                {
                    return Path.Combine(repoRoot, ".git", "config");
                }
                return ".git/config";
            }
        }
        catch
        {
            return isGlobal ? "~/.gitconfig" : ".git/config";
        }
    }

    #endregion
}
