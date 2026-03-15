using System.Diagnostics;
using System.Reflection;

/// <summary>
/// 管理安装过程 - 使用 core.hooksPath 方案
/// 不再使用 Git 别名，而是通过设置全局/项目 hooks 路径实现自动触发
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
    /// 项目级别安装 - 在当前仓库的 .git/hooks/ 目录安装
    /// </summary>
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
            Console.WriteLine("请确保当前目录在 Git 仓库内。");
            Console.ResetColor();
            return false;
        }

        var hooksDir = Path.Combine(repoRoot, ".git", "hooks");
        var exePath = Path.Combine(hooksDir, "EncodingChecker.exe");
        var hookPath = Path.Combine(hooksDir, "pre-commit");

        // 确保 exe 存在
        if (!File.Exists(exePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 错误: 找不到 {exePath}");
            Console.WriteLine("请先运行发布脚本: pwsh -File 发布.ps1");
            Console.ResetColor();
            return false;
        }

        // 检查是否已存在 pre-commit hook（非我们安装的）
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

        // 创建 pre-commit hook（支持链式调用）
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

        // 设置编码配置
        ConfigureGitEncoding(isGlobal: false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 项目级别安装完成！");
        Console.WriteLine($"  Hook 位置: {hookPath}");
        Console.ResetColor();
        return true;
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

        var globalHooksDir = GlobalHooksDir;
        Directory.CreateDirectory(globalHooksDir);

        var destExe = Path.Combine(globalHooksDir, "EncodingChecker.exe");
        var destHook = Path.Combine(globalHooksDir, "pre-commit");

        // 复制 exe
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

        // 创建全局 pre-commit hook
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

        // 设置全局 hooks 路径
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

        // 设置编码配置
        ConfigureGitEncoding(isGlobal: true);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 全局级别安装完成！");
        Console.WriteLine($"  程序位置: {globalHooksDir}");
        Console.WriteLine($"  作用范围: 本机所有 Git 仓库");
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

        var repoRoot = GetRepoRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("当前目录不在 Git 仓库内，跳过项目级别卸载。");
            return;
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
               $"    # 使用 cmd 调用，避免 MSYS 路径转换问题\n" +
               $"    MSYS_NO_PATHCONV=1 cmd /c \"\"\"$EXE_PATH\"\"\"\n" +
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
               $"REPO_HOOK=\"\"\"$(git rev-parse --git-dir)/hooks/pre-commit\"\"\"\n" +
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
               $"    MSYS_NO_PATHCONV=1 cmd /c \"\"\"$EXE_PATH\"\"\"\n" +
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
    private static string? GetRepoRoot()
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
}
