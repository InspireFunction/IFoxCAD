using System.Diagnostics;

/// <summary>
/// 负责运行 Git 命令的辅助类
/// </summary>
public static class GitCommandRunner
{
    /// <summary>
    /// 运行 Git 命令，不关心输出
    /// </summary>
    public static void Run(params string[] arguments)
    {
        var psi = CreateProcessInfo(arguments);
        using var proc = Process.Start(psi);
        proc?.WaitForExit();
    }

    /// <summary>
    /// 运行 Git 命令并获取输出
    /// </summary>
    public static string RunWithOutput(params string[] arguments)
    {
        var psi = CreateProcessInfo(arguments);
        psi.RedirectStandardOutput = true;

        using var proc = Process.Start(psi);
        if (proc == null) return "";

        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        return output;
    }

    /// <summary>
    /// 运行 Git 命令并逐行处理输出
    /// </summary>
    public static IEnumerable<string> RunWithLines(params string[] arguments)
    {
        var psi = CreateProcessInfo(arguments);
        psi.RedirectStandardOutput = true;

        using var proc = Process.Start(psi);
        if (proc == null) yield break;

        while (!proc.StandardOutput.EndOfStream)
        {
            var line = proc.StandardOutput.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                yield return line;
            }
        }

        proc.WaitForExit();
    }

    private static ProcessStartInfo CreateProcessInfo(string[] arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = GitPathResolver.GetGitPath(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        return psi;
    }
}
