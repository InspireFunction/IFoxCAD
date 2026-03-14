using System.Diagnostics;

/// <summary>
/// 负责找到本机的 Git 可执行文件
/// 人类不喜欢配置，所以要智能一点
/// </summary>
public static class GitPathResolver
{
    private static string? _cachedPath;

    /// <summary>
    /// 常见的 Git 安装位置，按优先级排序
    /// </summary>
    private static readonly string[] CommonGitPaths = new[]
    {
        // 标准安装路径（64位）
        @"C:\Program Files\Git\bin\git.exe",
        @"C:\Program Files\Git\cmd\git.exe",

        // 标准安装路径（32位）
        @"C:\Program Files (x86)\Git\bin\git.exe",
        @"C:\Program Files (x86)\Git\cmd\git.exe",

        // 用户级安装（通过官网安装程序选择"仅为我安装"）
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Git\bin\git.exe"),

        // 便携版常见位置
        @"C:\git\bin\git.exe",
        @"C:\tools\git\bin\git.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"git\bin\git.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"tools\git\bin\git.exe"),

        // 包管理器安装路径
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"scoop\shims\git.exe"),
        @"C:\ProgramData\chocolatey\bin\git.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".cargo\bin\git.exe"),
    };

    /// <summary>
    /// 获取 Git 可执行文件路径
    /// 查找顺序：环境变量 -> PATH -> 常见路径 -> where 命令
    /// </summary>
    public static string GetGitPath()
    {
        if (_cachedPath != null)
            return _cachedPath;

        // 1. 检查环境变量（给用户最高控制权）
        var envPath = Environment.GetEnvironmentVariable("GIT_PATH");
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
        {
            _cachedPath = envPath;
            return _cachedPath;
        }

        // 2. 尝试从 PATH 中直接调用
        if (TryGetGitFromPath(out var pathFromPath))
        {
            _cachedPath = pathFromPath;
            return _cachedPath;
        }

        // 3. 扫描常见安装路径
        foreach (var candidate in CommonGitPaths)
        {
            if (File.Exists(candidate))
            {
                _cachedPath = candidate;
                return _cachedPath;
            }
        }

        // 4. 尝试使用 where 命令查找
        if (TryGetGitFromWhereCommand(out var pathFromWhere))
        {
            _cachedPath = pathFromWhere;
            return _cachedPath;
        }

        // 5. 实在找不到，回退到 "git" 让系统尝试
        // 使用时如果失败会显示友好的错误提示
        _cachedPath = "git";
        return _cachedPath;
    }

    /// <summary>
    /// 验证 Git 是否真的可用
    /// </summary>
    public static bool ValidateGitAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetGitPath(),
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc != null)
            {
                proc.WaitForExit(5000);
                return proc.ExitCode == 0;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// 显示找不到 Git 时的帮助信息
    /// </summary>
    public static void ShowGitNotFoundHelp()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ 找不到 Git 可执行文件！");
        Console.WriteLine();
        Console.ResetColor();

        Console.WriteLine("别担心，试试以下方法之一：");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("方法 1 - 设置环境变量（推荐）:");
        Console.ResetColor();
        Console.WriteLine("  setx GIT_PATH \"C:\\Program Files\\Git\\bin\\git.exe\"");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("方法 2 - 将 Git 添加到 PATH:");
        Console.ResetColor();
        Console.WriteLine("  把 Git 的 bin 目录添加到系统 PATH 环境变量");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("方法 3 - 安装 Git:");
        Console.ResetColor();
        Console.WriteLine("  https://git-scm.com/download/win");
    }

    private static bool TryGetGitFromPath(out string path)
    {
        path = "git";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc != null)
            {
                proc.WaitForExit(5000);
                return proc.ExitCode == 0;
            }
        }
        catch { }

        return false;
    }

    private static bool TryGetGitFromWhereCommand(out string path)
    {
        path = "git";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "git",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc != null)
            {
                string output = proc.StandardOutput.ReadLine() ?? "";
                proc.WaitForExit(5000);

                if (!string.IsNullOrEmpty(output) && File.Exists(output))
                {
                    path = output;
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// 清除缓存，用于测试
    /// </summary>
    public static void ClearCache()
    {
        _cachedPath = null;
    }
}
