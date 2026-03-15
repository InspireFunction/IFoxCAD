using System.Diagnostics;
using System.Reflection;
using System.Text;

/// <summary>
/// 管理安装过程 - 使用 core.hooksPath 方案
/// 支持 Git Hook 自动触发 + Git 别名手动命令
/// </summary>
public static class InstallManager
{
    /// <summary>
    /// 全局 hooks 目录（在用户主目录下）
    /// </summary>
    public static string GlobalHooksDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".git-hooks"
    );

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
        Console.WriteLine("  [1] 项目级别 - 仅当前仓库生效（推荐团队协作）");
        Console.ResetColor();
        Console.WriteLine("      • 在 .git/hooks/ 目录安装 pre-commit hook");
        Console.WriteLine("      • 自动在 commit 前检查编码");
        Console.WriteLine("      • 不影响其他仓库");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [2] 全局级别 - 本机所有仓库生效（推荐个人使用）");
        Console.ResetColor();
        Console.WriteLine("      • 设置 core.hooksPath 指向 ~/.git-hooks/");
        Console.WriteLine("      • 所有仓库自动触发编码检查");
        Console.WriteLine("      • 支持链式调用（不破坏仓库自己的 hooks）");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  [3] 两者都安装");
        Console.ResetColor();
        Console.WriteLine("      • 项目级别优先于全局级别");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("  [4] 仅注册 Git 别名（用于手动命令）");
        Console.ResetColor();
        Console.WriteLine("      • git ec-check - 检查编码");
        Console.WriteLine("      • git ec-fix - 修复编码");
        Console.WriteLine("      • git ec-m \"msg\" - 修复并提交");
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

                case "4":
                    // 询问是全局还是项目级别别名
                    Console.WriteLine();
                    Console.WriteLine("请选择别名注册范围：");
                    Console.WriteLine("  [1] 全局 - 所有仓库可用（推荐）");
                    Console.WriteLine("  [2] 项目 - 仅当前仓库可用");
                    Console.WriteLine();
                    Console.Write("请输入选项 (1/2): ");
                    var scopeChoice = Console.ReadLine()?.Trim();
                    var isGlobalAlias = scopeChoice != "2";
                    RegisterGitAliases(isGlobalAlias);
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
    /// 项目级别安装 - 在当前仓库的 .git/hooks/ 目录安装
    /// </summary>
    public static bool InstallProjectLevel(bool force = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📦 正在安装到项目级别...");
        Console.ResetColor();

        // 差异化：获取仓库根目录
        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ 错误: 无法确定 Git 仓库根目录");
            Console.WriteLine("请确保当前目录在 Git 仓库内。");
            Console.ResetColor();
            return false;
        }

        var hooksDir = Path.Combine(repoRoot, ".git", "hooks");
        var exePath = Path.Combine(hooksDir, "EncodingChecker.exe");
        var hookPath = Path.Combine(hooksDir, "pre-commit");

        // 差异化：确保 exe 存在（项目级别要求已存在）
        if (!File.Exists(exePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 错误: 找不到 {exePath}");
            Console.WriteLine("请先运行发布脚本: pwsh -File 发布.ps1");
            Console.ResetColor();
            return false;
        }

        // 差异化：检查并备份现有 hook
        if (!HandleExistingHook(hookPath, force))
            return false;

        // 差异化：创建 hook
        try
        {
            CreateHookFile(hookPath, exePath, isGlobal: false);
            Console.WriteLine("  ✓ 已创建 pre-commit hook");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 创建 hook 失败: {ex.Message}");
            Console.ResetColor();
            return false;
        }

        // 统一逻辑：完成安装（传递 force 参数）
        return FinishInstallation(isGlobal: false, hooksDir, hookPath, force);
    }

    /// <summary>
    /// 全局级别安装 - 设置 core.hooksPath
    /// </summary>
    public static bool InstallGlobalLevel(bool force = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🌍 正在安装到全局级别...");
        Console.ResetColor();

        // 差异化：获取全局 hooks 目录
        var globalHooksDir = GlobalHooksDir;
        Directory.CreateDirectory(globalHooksDir);

        var destExe = Path.Combine(globalHooksDir, "EncodingChecker.exe");
        var destHook = Path.Combine(globalHooksDir, "pre-commit");

        // 差异化：复制 exe（全局级别需要从源位置复制）
        if (!force || !File.Exists(destExe))
        {
            var sourceExe = FindSourceExe();
            if (sourceExe == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ 错误: 找不到 EncodingChecker.exe 源文件");
                Console.ResetColor();
                return false;
            }

            try
            {
                File.Copy(sourceExe, destExe, overwrite: true);
                Console.WriteLine($"  ✓ 已复制: {destExe}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 复制文件失败: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 差异化：创建 hook
        try
        {
            CreateHookFile(destHook, destExe, isGlobal: true);
            Console.WriteLine("  ✓ 已创建全局 pre-commit hook");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 创建 hook 失败: {ex.Message}");
            Console.ResetColor();
            return false;
        }

        // 差异化：设置全局 hooks 路径
        try
        {
            var hooksPathForGit = globalHooksDir.Replace("\\", "/");
            GitCommandRunner.Run("config", "--global", "core.hooksPath", hooksPathForGit);
            Console.WriteLine($"  ✓ 已设置 core.hooksPath: {hooksPathForGit}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 设置 core.hooksPath 失败: {ex.Message}");
            Console.ResetColor();
            return false;
        }

        // 统一逻辑：完成安装（传递 force 参数）
        return FinishInstallation(isGlobal: true, globalHooksDir, destHook, force);
    }

    /// <summary>
    /// 统一逻辑：完成安装（配置编码、注册别名、显示完成信息）
    /// </summary>
    private static bool FinishInstallation(bool isGlobal, string hooksDir, string hookPath, bool force = false)
    {
        // 设置编码配置
        ConfigureGitEncoding(isGlobal);

        // 注册 Git 别名
        RegisterGitAliases(isGlobal, force);

        // 显示完成信息
        Console.ForegroundColor = ConsoleColor.Green;
        if (isGlobal)
        {
            Console.WriteLine("✓ 全局级别安装完成！");
            Console.WriteLine($"  程序位置: {hooksDir}");
            Console.WriteLine($"  作用范围: 本机所有 Git 仓库");
        }
        else
        {
            Console.WriteLine("✓ 项目级别安装完成！");
            Console.WriteLine($"  Hook 位置: {hookPath}");
        }
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// 差异化：检查并备份现有的 pre-commit hook
    /// </summary>
    private static bool HandleExistingHook(string hookPath, bool force)
    {
        if (File.Exists(hookPath) && !IsOurHook(hookPath))
        {
            if (!force)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  检测到已存在 pre-commit hook");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  当前 hook 将被备份为 pre-commit.backup");
                Console.Write("是否继续? (Y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirm == "n" || confirm == "no")
                {
                    Console.WriteLine("已取消安装。");
                    return false;
                }
            }

            // 备份现有 hook
            var backupPath = hookPath + ".backup";
            try
            {
                File.Copy(hookPath, backupPath, overwrite: true);
                Console.WriteLine($"  ✓ 已备份现有 hook: pre-commit.backup");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️  备份失败: {ex.Message}");
                Console.ResetColor();
            }
        }
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

        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("当前目录不在 Git 仓库内，跳过项目级别卸载。");
            return;
        }

        // 检查本地别名
        var localAliases = CheckLocalAliases();
        if (localAliases.Count > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  检测到 {localAliases.Count} 个本地 Git 别名:");
            foreach (var alias in localAliases)
            {
                Console.WriteLine($"   - git {alias}");
            }
            Console.WriteLine();
            Console.WriteLine("这些别名指向的路径可能不正确，建议清理。");
            Console.ResetColor();
            Console.Write("是否清理本地别名? (Y/n): ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (confirm != "n" && confirm != "no")
            {
                UnregisterGitAliases(isGlobal: false);
            }
        }

        var hooksDir = Path.Combine(repoRoot, ".git", "hooks");
        var hookPath = Path.Combine(hooksDir, "pre-commit");
        var backupPath = hookPath + ".backup";

        // 删除我们的 hook
        if (File.Exists(hookPath) && IsOurHook(hookPath))
        {
            try
            {
                File.Delete(hookPath);
                Console.WriteLine("  ✓ 已删除 pre-commit hook");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  删除失败: {ex.Message}");
            }
        }

        // 恢复备份的 hook
        if (File.Exists(backupPath))
        {
            try
            {
                File.Move(backupPath, hookPath);
                Console.WriteLine("  ✓ 已恢复备份的 hook");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  恢复备份失败: {ex.Message}");
            }
        }

        // 删除 exe
        var exePath = Path.Combine(hooksDir, "EncodingChecker.exe");
        if (File.Exists(exePath))
        {
            try
            {
                File.Delete(exePath);
                Console.WriteLine("  ✓ 已删除 EncodingChecker.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  删除 exe 失败: {ex.Message}");
            }
        }

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

        // 清理全局别名
        UnregisterGitAliases(isGlobal: true);

        // 清理 core.hooksPath 配置
        try
        {
            GitCommandRunner.Run("config", "--global", "--unset", "core.hooksPath");
            Console.WriteLine("  ✓ 已清理 core.hooksPath");
        }
        catch
        {
            Console.WriteLine("  ℹ️  core.hooksPath 未设置或已清理");
        }

        // 删除全局 exe 和 hook
        var globalHooksDir = GlobalHooksDir;
        var exePath = Path.Combine(globalHooksDir, "EncodingChecker.exe");
        var hookPath = Path.Combine(globalHooksDir, "pre-commit");

        if (File.Exists(exePath))
        {
            try
            {
                File.Delete(exePath);
                Console.WriteLine("  ✓ 已删除全局 exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  删除 exe 失败: {ex.Message}");
            }
        }

        if (File.Exists(hookPath))
        {
            try
            {
                File.Delete(hookPath);
                Console.WriteLine("  ✓ 已删除全局 hook");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  删除 hook 失败: {ex.Message}");
            }
        }

        // 尝试删除空目录
        if (Directory.Exists(globalHooksDir) && Directory.GetFiles(globalHooksDir).Length == 0)
        {
            try
            {
                Directory.Delete(globalHooksDir);
                Console.WriteLine("  ✓ 已删除空目录");
            }
            catch { }
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

        UninstallProjectLevel();
        Console.WriteLine();
        UninstallGlobalLevel();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 全面清理完成");
        Console.ResetColor();
    }

    #region 私有辅助方法

    /// <summary>
    /// 创建 hook 文件（支持链式调用）
    /// </summary>
    private static void CreateHookFile(string hookPath, string exePath, bool isGlobal)
    {
        var exeDir = Path.GetDirectoryName(exePath)!;
        var hookContent = isGlobal
            ? GetGlobalHookContent(exePath)
            : GetProjectHookContent(exePath);

        File.WriteAllText(hookPath, hookContent);
    }

    /// <summary>
    /// 全局 hook 内容 - 支持链式调用仓库自己的 hook
    /// </summary>
    private static string GetGlobalHookContent(string exePath)
    {
        var exeDir = Path.GetDirectoryName(exePath)!.Replace("\\", "/");
        return $"#!/bin/sh\n" +
               $"# EncodingChecker Global Hook\n" +
               $"# 由 GitEncodingChecker 自动生成\n" +
               $"\n" +
               $"SCRIPT_DIR=\"{exeDir}\"\n" +
               $"EXE_PATH=\"$SCRIPT_DIR/EncodingChecker.exe\"\n" +
               $"\n" +
               $"# 1. 运行 EncodingChecker 检查\n" +
               $"if [ -f \"$EXE_PATH\" ]; then\n" +
               $"    # 使用多种方式尝试执行，兼容不同环境\n" +
               $"    if command -v cygpath >/dev/null 2>&1; then\n" +
               $"        # Cygwin/MSYS 环境：转换路径后执行\n" +
               $"        WIN_PATH=$(cygpath -w \"$EXE_PATH\" 2>/dev/null || echo \"$EXE_PATH\")\n" +
               $"        \"$WIN_PATH\"\n" +
               $"    elif [ -n \"$MSYSTEM\" ] || [ -n \"$MINGW_PREFIX\" ]; then\n" +
               $"        # MSYS2/MinGW 环境：直接使用 Windows 路径格式\n" +
               $"        WIN_PATH=$(echo \"$EXE_PATH\" | sed 's|^/c/|C:/|i; s|^/d/|D:/|i; s|^/e/|E:/|i; s|^/f/|F:/|i; s|^/g/|G:/|i; s|^/h/|H:/|i')\n" +
               $"        cmd //c \"$WIN_PATH\" 2>/dev/null || \"$EXE_PATH\"\n" +
               $"    else\n" +
               $"        # 其他环境：直接执行\n" +
               $"        \"$EXE_PATH\"\n" +
               $"    fi\n" +
               $"    RESULT=$?\n" +
               $"    if [ $RESULT -ne 0 ]; then\n" +
               $"        echo \"编码检查失败，提交已取消\"\n" +
               $"        exit $RESULT\n" +
               $"    fi\n" +
               $"else\n" +
               $"    echo \"警告: 找不到 EncodingChecker.exe\"\n" +
               $"fi\n" +
               $"\n" +
               $"# 2. 链式调用仓库自己的 pre-commit（如果存在且不是当前文件）\n" +
               $"REPO_HOOK=\"$(git rev-parse --git-dir)/hooks/pre-commit\"\n" +
               $"if [ -f \"$REPO_HOOK\" ] && [ \"$REPO_HOOK\" != \"$0\" ]; then\n" +
               $"    \"$REPO_HOOK\"\n" +
               $"    exit $?\n" +
               $"fi\n" +
               $"\n" +
               $"exit 0\n";
    }

    /// <summary>
    /// 项目级 hook 内容
    /// </summary>
    private static string GetProjectHookContent(string exePath)
    {
        var exeDir = Path.GetDirectoryName(exePath)!.Replace("\\", "/");
        return $"#!/bin/sh\n" +
               $"# EncodingChecker Project Hook\n" +
               $"# 由 GitEncodingChecker 自动生成\n" +
               $"\n" +
               $"SCRIPT_DIR=\"{exeDir}\"\n" +
               $"EXE_PATH=\"$SCRIPT_DIR/EncodingChecker.exe\"\n" +
               $"\n" +
               $"# 运行 EncodingChecker 检查\n" +
               $"if [ -f \"$EXE_PATH\" ]; then\n" +
               $"    # 使用多种方式尝试执行，兼容不同环境\n" +
               $"    if command -v cygpath >/dev/null 2>&1; then\n" +
               $"        # Cygwin/MSYS 环境：转换路径后执行\n" +
               $"        WIN_PATH=$(cygpath -w \"$EXE_PATH\" 2>/dev/null || echo \"$EXE_PATH\")\n" +
               $"        \"$WIN_PATH\"\n" +
               $"    elif [ -n \"$MSYSTEM\" ] || [ -n \"$MINGW_PREFIX\" ]; then\n" +
               $"        # MSYS2/MinGW 环境：直接使用 Windows 路径格式\n" +
               $"        WIN_PATH=$(echo \"$EXE_PATH\" | sed 's|^/c/|C:/|i; s|^/d/|D:/|i; s|^/e/|E:/|i; s|^/f/|F:/|i; s|^/g/|G:/|i; s|^/h/|H:/|i')\n" +
               $"        cmd //c \"$WIN_PATH\" 2>/dev/null || \"$EXE_PATH\"\n" +
               $"    else\n" +
               $"        # 其他环境：直接执行\n" +
               $"        \"$EXE_PATH\"\n" +
               $"    fi\n" +
               $"    exit $?\n" +
               $"else\n" +
               $"    echo \"错误: 找不到 EncodingChecker.exe\"\n" +
               $"    exit 1\n" +
               $"fi\n";
    }

    /// <summary>
    /// 检查是否是我们安装的 hook
    /// </summary>
    private static bool IsOurHook(string hookPath)
    {
        try
        {
            var content = File.ReadAllText(hookPath);
            return content.Contains("EncodingChecker") || content.Contains("由 GitEncodingChecker 自动生成");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 配置 Git 编码设置
    /// </summary>
    private static void ConfigureGitEncoding(bool isGlobal)
    {
        var args = new List<string>();
        if (isGlobal) args.Add("--global");

        try
        {
            GitCommandRunner.Run(args.Concat(new[] { "config", "core.autocrlf", "false" }).ToArray());
            GitCommandRunner.Run(args.Concat(new[] { "config", "i18n.commitencoding", "utf-8" }).ToArray());
            GitCommandRunner.Run(args.Concat(new[] { "config", "i18n.logoutputencoding", "utf-8" }).ToArray());
            GitCommandRunner.Run(args.Concat(new[] { "config", "core.quotepath", "false" }).ToArray());

            if (!isGlobal)
            {
                // 项目级别设置行尾
                var eol = DetectLineEndingFromEditorConfig();
                GitCommandRunner.Run("config", "core.eol", eol.ToLowerInvariant());
            }
        }
        catch { }
    }

    /// <summary>
    /// 从 .editorconfig 检测行尾设置
    /// </summary>
    private static string DetectLineEndingFromEditorConfig()
    {
        try
        {
            var repoRoot = GetRepoRoot();
            if (string.IsNullOrEmpty(repoRoot)) return "lf";

            var editorConfigPath = Path.Combine(repoRoot, ".editorconfig");
            if (!File.Exists(editorConfigPath)) return "lf";

            var content = File.ReadAllText(editorConfigPath);
            if (content.Contains("end_of_line = crlf")) return "crlf";
            if (content.Contains("end_of_line = lf")) return "lf";
        }
        catch { }

        return "lf";
    }

    /// <summary>
    /// 获取 Git 仓库根目录
    /// </summary>
    public static string? GetRepoRoot()
    {
        try
        {
            var output = GitCommandRunner.RunWithOutput("rev-parse", "--show-toplevel");
            return output.Trim();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 查找源 exe 文件
    /// </summary>
    private static string? FindSourceExe()
    {
        // 可能的源位置
        var possiblePaths = new[]
        {
            // 当前目录（发布目录）
            Path.Combine(Environment.CurrentDirectory, "EncodingChecker.exe"),
            // 项目 hooks 目录
            Path.Combine(GetRepoRoot() ?? "", ".git", "hooks", "EncodingChecker.exe"),
            // 发布输出目录
            Path.Combine(Environment.CurrentDirectory, "bin", "Release", "win-x64", "publish", "EncodingChecker.exe"),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    /// <summary>
    /// 获取 EncodingChecker.exe 的路径（internal 用于测试）
    /// </summary>
    internal static string GetExePath(bool isGlobal)
    {
        if (isGlobal)
        {
            return Path.Combine(GlobalHooksDir, "EncodingChecker.exe");
        }
        else
        {
            var repoRoot = GetRepoRoot();
            if (string.IsNullOrEmpty(repoRoot))
            {
                throw new InvalidOperationException("当前目录不在 Git 仓库内");
            }
            return Path.Combine(repoRoot, ".git", "hooks", "EncodingChecker.exe");
        }
    }

    /// <summary>
    /// 检查现有安装状态
    /// </summary>
    private static (bool projectInstalled, bool globalInstalled) CheckExistingInstallation()
    {
        bool projectInstalled = false;
        bool globalInstalled = false;

        // 检查项目级别
        var repoRoot = GetRepoRoot();
        if (!string.IsNullOrEmpty(repoRoot))
        {
            var projectHook = Path.Combine(repoRoot, ".git", "hooks", "pre-commit");
            if (File.Exists(projectHook) && IsOurHook(projectHook))
                projectInstalled = true;
        }

        // 检查全局级别
        var globalHooksDir = GlobalHooksDir;
        if (Directory.Exists(globalHooksDir))
        {
            var globalHook = Path.Combine(globalHooksDir, "pre-commit");
            if (File.Exists(globalHook) && IsOurHook(globalHook))
                globalInstalled = true;
        }

        return (projectInstalled, globalInstalled);
    }

    /// <summary>
    /// 显示安装后提示
    /// </summary>
    private static void ShowPostInstallHints(bool isGlobal)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("💡 使用提示:");
        Console.ResetColor();
        Console.WriteLine();

        if (isGlobal)
        {
            Console.WriteLine("  • 所有 Git 仓库在 commit 时会自动检查编码");
            Console.WriteLine("  • 如果检查失败，commit 会被阻止");
            Console.WriteLine("  • 如需跳过检查: git commit --no-verify");
        }
        else
        {
            Console.WriteLine("  • 当前仓库在 commit 时会自动检查编码");
            Console.WriteLine("  • 如果检查失败，commit 会被阻止");
            Console.WriteLine("  • 如需跳过检查: git commit --no-verify");
        }

        Console.WriteLine();
        Console.WriteLine("  手动检查命令:");
        Console.WriteLine($"    {GlobalHooksDir.Replace("\\", "/")}/EncodingChecker.exe");
        Console.WriteLine();
    }

    #endregion

    #region Git 别名注册

    /// <summary>
    /// 从 CommandTable 获取所有 Git 命令别名
    /// 使用集中式的命令表，确保exe和单元测试看到一致的命令列表
    /// </summary>
    public static List<(string command, string alias, string description)> GetAllAliasesFromReflection()
    {
        // 使用 CommandTable 获取别名列表，确保与运行时一致
        return CommandTable.GetAliasList().ToList();
    }

    /// <summary>
    /// 注册所有 Git 别名
    /// 已正确配置的别名会自动跳过，避免不必要的 Git 配置修改
    /// </summary>
    public static bool RegisterGitAliases(bool isGlobal = true, bool force = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔧 正在注册 Git 别名...");
        Console.ResetColor();

        var aliases = GetAllAliasesFromReflection();
        var exePath = FindSourceExe() ?? Path.Combine(GlobalHooksDir, "EncodingChecker.exe");

        if (!File.Exists(exePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 错误: 找不到 EncodingChecker.exe: {exePath}");
            Console.ResetColor();
            return false;
        }

        // 分类处理别名：已正确配置的、需要更新的、真正冲突的
        var (alreadyConfigured, needsUpdate, realConflicts) = CategorizeAliases(aliases, exePath, isGlobal);

        // 显示已正确配置的别名（仅 verbose 模式或数量较少时）
        if (alreadyConfigured.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ℹ️  {alreadyConfigured.Count} 个别名已正确配置，跳过:");
            foreach (var alias in alreadyConfigured)
            {
                Console.WriteLine($"     - git {alias}");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        // 处理需要更新的别名（指向同一程序但参数不同）
        if (needsUpdate.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  检测到 {needsUpdate.Count} 个别名需要更新:");
            foreach (var (alias, existing, newValue) in needsUpdate)
            {
                Console.WriteLine($"   - {alias}: {existing}");
            }
            Console.ResetColor();
            Console.WriteLine();

            if (!force)
            {
                Console.Write("是否更新? (Y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirm == "n" || confirm == "no")
                {
                    // 将需要更新的移到冲突列表，不处理
                    realConflicts.AddRange(needsUpdate);
                    needsUpdate.Clear();
                }
            }
        }

        // 处理真正的冲突（指向不同程序）
        if (realConflicts.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  检测到 {realConflicts.Count} 个别名冲突（指向其他程序）:");
            foreach (var (alias, existing, _) in realConflicts)
            {
                Console.WriteLine($"   - {alias}: {existing}");
            }
            Console.ResetColor();
            Console.WriteLine();

            if (force)
            {
                Console.WriteLine("强制模式: 自动覆盖冲突别名");
            }
            else
            {
                Console.Write("是否覆盖? (Y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirm == "n" || confirm == "no")
                {
                    Console.WriteLine("已取消别名注册。");
                    return false;
                }
            }
        }

        // 备份需要更新和冲突的别名
        BackupExistingAliases([.. needsUpdate, .. realConflicts], isGlobal);

        // 只注册需要处理的别名（排除已正确配置的）
        var aliasesToRegister = aliases
            .Where(a => !alreadyConfigured.Contains(a.alias))
            .ToList();

        int successCount = 0;
        int skipCount = alreadyConfigured.Count;

        foreach (var (command, alias, _) in aliasesToRegister)
        {
            try
            {
                var aliasValue = $"!\"{exePath}\" {command}";
                if (isGlobal)
                {
                    GitCommandRunner.Run("config", "--global", $"alias.{alias}", aliasValue);
                }
                else
                {
                    GitCommandRunner.Run("config", $"alias.{alias}", aliasValue);
                }
                Console.WriteLine($"  ✓ git {alias}");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ git {alias}: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        if (skipCount > 0)
        {
            Console.WriteLine($"✓ 成功注册 {successCount}/{aliases.Count} 个别名（跳过 {skipCount} 个已配置）");
        }
        else
        {
            Console.WriteLine($"✓ 成功注册 {successCount}/{aliases.Count} 个别名");
        }
        Console.ResetColor();

        // 格式化配置文件，统一缩进风格（只有当实际有修改时）
        if (successCount > 0)
        {
            FormatGitConfigFile(isGlobal);
        }

        return successCount + skipCount == aliases.Count;
    }

    /// <summary>
    /// 格式化 Git 配置文件，统一使用 tab 缩进
    /// 解决原文件格式混乱（混用 tab 和空格）的问题
    /// </summary>
    public static void FormatGitConfigFile(bool isGlobal)
    {
        try
        {
            string? configPath = GetGitConfigPath(isGlobal);
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                return;

            string content = File.ReadAllText(configPath);
            if (string.IsNullOrWhiteSpace(content))
                return;

            // 解析并重新格式化配置
            var formattedLines = new List<string>();
            string? currentSection = null;

            foreach (var rawLine in content.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                string trimmedLine = line.Trim();

                // 跳过空行，但保留一个空行作为段落分隔
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    if (formattedLines.Count > 0 && !string.IsNullOrWhiteSpace(formattedLines[^1]))
                    {
                        formattedLines.Add("");
                    }
                    continue;
                }

                // 处理注释行
                if (trimmedLine.StartsWith('#') || trimmedLine.StartsWith(';'))
                {
                    formattedLines.Add(trimmedLine);
                    continue;
                }

                // 处理节头 [section] 或 [section "subsection"]
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
                {
                    currentSection = trimmedLine;
                    // 节前面加空行（除了文件开头）
                    if (formattedLines.Count > 0 && !string.IsNullOrWhiteSpace(formattedLines[^1]))
                    {
                        formattedLines.Add("");
                    }
                    formattedLines.Add(trimmedLine);
                    continue;
                }

                // 处理配置项 key = value
                int equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = trimmedLine[..equalIndex].Trim();
                    string value = trimmedLine[(equalIndex + 1)..].Trim();

                    // 使用 tab 缩进
                    formattedLines.Add($"\t{key} = {value}");
                }
                else
                {
                    // 不符合 key=value 格式的行，原样保留
                    formattedLines.Add(trimmedLine);
                }
            }

            // 移除末尾的空行
            while (formattedLines.Count > 0 && string.IsNullOrWhiteSpace(formattedLines[^1]))
            {
                formattedLines.RemoveAt(formattedLines.Count - 1);
            }

            // 确保文件末尾有换行符
            formattedLines.Add("");

            // 写回文件，使用 UTF-8 无 BOM
            var newContent = string.Join("\n", formattedLines);
            File.WriteAllText(configPath, newContent, new UTF8Encoding(false));
        }
        catch
        {
            // 格式化失败不影响主要功能，静默处理
        }
    }

    /// <summary>
    /// 获取 Git 配置文件路径
    /// </summary>
    private static string? GetGitConfigPath(bool isGlobal)
    {
        try
        {
            if (isGlobal)
            {
                // 全局配置通常在用户主目录
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".gitconfig"
                );
            }
            else
            {
                // 本地配置在仓库的 .git/config
                var repoRoot = GetRepoRoot();
                if (!string.IsNullOrEmpty(repoRoot))
                {
                    return Path.Combine(repoRoot, ".git", "config");
                }
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// 将别名分类为：已正确配置的、需要更新的、真正冲突的
    /// </summary>
    private static (
        List<string> alreadyConfigured,
        List<(string alias, string existingValue, string newValue)> needsUpdate,
        List<(string alias, string existingValue, string newValue)> realConflicts
    ) CategorizeAliases(
        List<(string command, string alias, string description)> aliases,
        string exePath,
        bool isGlobal)
    {
        var alreadyConfigured = new List<string>();
        var needsUpdate = new List<(string alias, string existingValue, string newValue)>();
        var realConflicts = new List<(string alias, string existingValue, string newValue)>();

        foreach (var (command, alias, _) in aliases)
        {
            var existingValue = GetGitConfigValue($"alias.{alias}", isGlobal);
            if (string.IsNullOrEmpty(existingValue))
            {
                // 别名不存在，需要注册
                continue;
            }

            var newValue = $"!\"{exePath}\" {command}";

            // 标准化路径后比较
            var normalizedExisting = NormalizeAliasValue(existingValue);
            var normalizedNew = NormalizeAliasValue(newValue);

            if (normalizedExisting.Equals(normalizedNew, StringComparison.OrdinalIgnoreCase))
            {
                // 已正确配置，完全匹配
                alreadyConfigured.Add(alias);
            }
            else if (IsSameExecutable(normalizedExisting, normalizedNew))
            {
                // 指向同一程序但参数不同（可能是命令更新）
                needsUpdate.Add((alias, existingValue, newValue));
            }
            else
            {
                // 指向不同程序，真正冲突
                realConflicts.Add((alias, existingValue, newValue));
            }
        }

        return (alreadyConfigured, needsUpdate, realConflicts);
    }

    /// <summary>
    /// 标准化别名值以便比较
    /// </summary>
    private static string NormalizeAliasValue(string value)
    {
        return value.Replace("\\", "/").Replace("\"", "").Trim();
    }

    /// <summary>
    /// 检查两个别名是否指向同一可执行文件
    /// </summary>
    private static bool IsSameExecutable(string normalizedValue1, string normalizedValue2)
    {
        // 提取可执行文件路径（去掉开头的 ! 和参数）
        var exe1 = ExtractExecutablePath(normalizedValue1);
        var exe2 = ExtractExecutablePath(normalizedValue2);

        return exe1.Equals(exe2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 从别名值中提取可执行文件路径
    /// </summary>
    private static string ExtractExecutablePath(string normalizedValue)
    {
        // 去掉开头的 !
        var value = normalizedValue.StartsWith('!') ? normalizedValue[1..] : normalizedValue;

        // 找到第一个空格前的部分（可执行文件路径）
        var spaceIndex = value.IndexOf(' ');
        if (spaceIndex > 0)
        {
            value = value[..spaceIndex];
        }

        return value.Trim();
    }

    /// <summary>
    /// 检查别名冲突（保留用于向后兼容）
    /// </summary>
    [Obsolete("使用 CategorizeAliases 方法替代")]
    public static List<(string alias, string existingValue, string newValue)> CheckAliasConflicts(
        List<(string command, string alias, string description)> aliases, bool isGlobal)
    {
        var exePath = FindSourceExe() ?? Path.Combine(GlobalHooksDir, "EncodingChecker.exe");
        var (_, needsUpdate, realConflicts) = CategorizeAliases(aliases, exePath, isGlobal);
        return [.. needsUpdate, .. realConflicts];
    }

    /// <summary>
    /// 备份现有别名（internal 用于测试访问）
    /// </summary>
    internal static readonly Dictionary<string, string> _backupMap = new();
    internal static string? _customPrefix;

    public static void BackupExistingAliases(
        List<(string alias, string existingValue, string newValue)> conflicts, bool isGlobal)
    {
        var prefix = isGlobal ? "global" : "local";
        foreach (var (alias, existingValue, _) in conflicts)
        {
            var backupKey = $"{prefix}:{alias}";
            _backupMap[backupKey] = existingValue;
        }
    }

    /// <summary>
    /// 获取 Git 配置值
    /// </summary>
    public static string GetGitConfigValue(string key, bool isGlobal)
    {
        try
        {
            var args = isGlobal
                ? new[] { "config", "--global", key }
                : new[] { "config", key };
            return GitCommandRunner.RunWithOutput(args).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 检查本地是否有 ec 前缀的别名
    /// </summary>
    public static List<string> CheckLocalAliases()
    {
        var localAliases = new List<string>();
        try
        {
            var result = GitCommandRunner.RunWithOutput("config", "--local", "--get-regexp", "^alias\\.ec");
            if (!string.IsNullOrEmpty(result))
            {
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(' ', 2);
                    if (parts.Length > 0 && parts[0].StartsWith("alias."))
                    {
                        localAliases.Add(parts[0].Substring(6)); // 移除 "alias." 前缀
                    }
                }
            }
        }
        catch
        {
            // 可能没有本地别名，忽略错误
        }
        return localAliases;
    }

    /// <summary>
    /// 卸载 Git 别名
    /// </summary>
    public static void UnregisterGitAliases(bool isGlobal = true)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🗑️  正在卸载 Git 别名...");
        Console.ResetColor();

        var aliases = GetAllAliasesFromReflection();
        int successCount = 0;

        foreach (var (_, alias, _) in aliases)
        {
            try
            {
                if (isGlobal)
                {
                    GitCommandRunner.Run("config", "--global", "--unset", $"alias.{alias}");
                }
                else
                {
                    GitCommandRunner.Run("config", "--unset", $"alias.{alias}");
                }
                Console.WriteLine($"  ✓ 已移除 git {alias}");
                successCount++;
            }
            catch
            {
                // 别名可能不存在，忽略错误
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ 已清理 {successCount} 个别名");
        Console.ResetColor();
    }

    #endregion
}
