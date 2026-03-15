using Xunit;
using System.Text;
using System.Diagnostics;

/// <summary>
/// 集成测试 - 模拟真实用户使用场景
/// 测试流程：发布.ps1 安装 → 使用 git 命令
/// 
/// 使用 CommandTable 验证命令存在性，确保测试的命令与实际代码一致
/// </summary>
public class IntegrationTests : IDisposable
{
    private readonly string _testRepoDir;
    private readonly string _originalDir;
    private readonly string _exePath;
    private readonly string _tempGlobalHooksDir;

    public IntegrationTests()
    {
        _originalDir = Directory.GetCurrentDirectory();

        // 创建临时测试目录
        _testRepoDir = Path.Combine(Path.GetTempPath(), $"IntegrationTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRepoDir);

        // 创建临时全局 hooks 目录（避免污染用户环境）
        _tempGlobalHooksDir = Path.Combine(Path.GetTempPath(), $"GlobalHooks_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempGlobalHooksDir);

        // 切换到测试目录（在运行Git命令之前）
        Directory.SetCurrentDirectory(_testRepoDir);

        // 初始化 Git 仓库
        RunGitCommand("init");
        RunGitCommand("config", "user.email", "test@test.com");
        RunGitCommand("config", "user.name", "Test User");

        // 获取编译后的 exe 路径（使用测试输出目录中的可执行文件）
        // 由于项目引用，EncodingChecker.exe 会自动复制到测试输出目录
        var testOutputDir = AppContext.BaseDirectory;
        _exePath = Path.Combine(testOutputDir, "EncodingChecker.exe");

        // 记录调试信息
        Console.WriteLine($"[DEBUG] 测试输出目录: {testOutputDir}");
        Console.WriteLine($"[DEBUG] 查找可执行文件: {_exePath}");
        Console.WriteLine($"[DEBUG] 文件存在: {File.Exists(_exePath)}");

        // 如果测试输出目录中没有，尝试从主项目的发布目录获取
        if (!File.Exists(_exePath))
        {
            var coreProjectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "core"));
            var possiblePaths = new[]
            {
                Path.Combine(coreProjectDir, "bin", "Release", "native", "EncodingChecker.exe"),
                Path.Combine(coreProjectDir, "bin", "Release", "net10.0", "win-x64", "EncodingChecker.exe"),
                Path.Combine(coreProjectDir, "bin", "Debug", "net10.0", "win-x64", "EncodingChecker.exe")
            };

            _exePath = possiblePaths.FirstOrDefault(File.Exists) ?? _exePath;

            Console.WriteLine($"[DEBUG] 尝试从主项目目录查找: {coreProjectDir}");
            Console.WriteLine($"[DEBUG] 最终路径: {_exePath}");
            Console.WriteLine($"[DEBUG] 最终文件存在: {File.Exists(_exePath)}");
        }

        // 确保 exe 存在
        if (!File.Exists(_exePath))
        {
            throw new FileNotFoundException($"找不到 EncodingChecker.exe，请先运行发布脚本。搜索路径: {_exePath}");
        }

        // 复制 exe 及其依赖到 .git/hooks/（模拟发布.ps1 的项目安装）
        // 注意：使用非AOT版本进行测试，避免AOT运行时问题
        var hooksDir = Path.Combine(_testRepoDir, ".git", "hooks");
        Directory.CreateDirectory(hooksDir);

        // 使用已定义的 testOutputDir 变量
        var testOutputExe = Path.Combine(testOutputDir, "EncodingChecker.exe");

        string sourceDir;
        if (File.Exists(testOutputExe))
        {
            Console.WriteLine($"[DEBUG] 使用测试输出目录中的非AOT版本: {testOutputExe}");
            sourceDir = testOutputDir;
        }
        else if (File.Exists(_exePath))
        {
            Console.WriteLine($"[DEBUG] 使用主项目路径: {_exePath}");
            sourceDir = Path.GetDirectoryName(_exePath)!;
        }
        else
        {
            throw new FileNotFoundException($"找不到 EncodingChecker.exe 用于测试");
        }

        // 复制所有必要文件到 hooks 目录
        CopyDirectory(sourceDir, hooksDir);
        Console.WriteLine($"[DEBUG] 已复制所有依赖文件到: {hooksDir}");

        // 安装项目级别别名（模拟用户运行发布.ps1 后）
        InstallProjectAliases();
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);

        try
        {
            if (Directory.Exists(_testRepoDir))
            {
                Directory.Delete(_testRepoDir, recursive: true);
            }
        }
        catch { }

        try
        {
            if (Directory.Exists(_tempGlobalHooksDir))
            {
                Directory.Delete(_tempGlobalHooksDir, recursive: true);
            }
        }
        catch { }
    }

    #region 辅助方法

    private void CopyDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir);
        }
    }

    private void RunGitCommand(params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = _testRepoDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
    }

    private string RunGitCommandWithOutput(params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = _testRepoDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = Process.Start(psi);
        var output = proc?.StandardOutput.ReadToEnd() ?? "";
        proc?.WaitForExit();
        return output;
    }

    private int RunExeCommand(params string[] args)
    {
        var exePath = Path.Combine(_testRepoDir, ".git", "hooks", "EncodingChecker.exe");
        Console.WriteLine($"[DEBUG] 运行命令: {exePath} {string.Join(" ", args)}");

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = _testRepoDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            Console.WriteLine($"[ERROR] 无法启动进程: {exePath}");
            return -1;
        }

        // 读取输出和错误
        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();

        proc.WaitForExit();

        Console.WriteLine($"[DEBUG] ExitCode: {proc.ExitCode}");
        if (!string.IsNullOrEmpty(output))
            Console.WriteLine($"[DEBUG] Output: {output}");
        if (!string.IsNullOrEmpty(error))
            Console.WriteLine($"[DEBUG] Error: {error}");

        return proc.ExitCode;
    }

    private void InstallProjectAliases()
    {
        // 使用 CommandTable 验证命令存在性，确保安装的别名与代码一致
        ValidateCommandsExist();

        // 注册项目级别的 Git 别名（模拟发布.ps1 的效果）
        var exePath = Path.Combine(_testRepoDir, ".git", "hooks", "EncodingChecker.exe").Replace("\\", "/");

        // 从 CommandTable 获取别名列表，确保与代码一致
        var aliasList = CommandTable.GetAliasList()
            .Where(a => a.Alias is "ec-check" or "ec-fix" or "ec-m" or "ec-force")
            .ToList();

        foreach (var (command, alias, _) in aliasList)
        {
            RunGitCommand("config", $"alias.{alias}", $"!\"{exePath}\" {command}");
            Console.WriteLine($"[DEBUG] 注册别名: git {alias} -> {command}");
        }
    }

    /// <summary>
    /// 验证测试所需的命令在 CommandTable 中存在
    /// 使用别名从 CommandTable 查找对应的命令
    /// </summary>
    private void ValidateCommandsExist()
    {
        // 使用别名从 CommandTable 获取实际命令名，避免硬编码
        var requiredAliases = new[] { "ec-check", "ec-fix", "ec-m", "ec-force" };

        foreach (var alias in requiredAliases)
        {
            Assert.True(
                CommandTable.Aliases.ContainsKey(alias),
                $"别名 {alias} 必须在 CommandTable.Aliases 中存在。可用别名: {string.Join(", ", CommandTable.Aliases.Keys)}"
            );

            var commandName = CommandTable.Aliases[alias];
            Assert.True(
                CommandTable.ContainsCommand(commandName),
                $"命令 {commandName} (别名: {alias}) 必须在 CommandTable 中存在"
            );
        }

        Console.WriteLine($"[DEBUG] CommandTable 验证通过，包含 {CommandTable.Map.Count} 个命令");
        Console.WriteLine($"[DEBUG] 可用命令: {string.Join(", ", CommandTable.GetAllCommandNames())}");
    }

    private string CreateTestFile(string name, byte[] content)
    {
        var path = Path.Combine(_testRepoDir, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    private string CreateTestFile(string name, string content, Encoding encoding)
    {
        var path = Path.Combine(_testRepoDir, name);
        File.WriteAllText(path, content, encoding);
        return path;
    }

    #endregion

    #region 场景1: 用户工作流 - 检查通过 → 提交

    [Fact]
    public void Scenario1_CleanFile_ShouldPassCheckAndAllowCommit()
    {
        // Arrange: 创建符合规范的文件（UTF-8 无 BOM，统一换行符）
        var content = "Hello World\nThis is a test file\n";
        CreateTestFile("clean.txt", content, new UTF8Encoding(false));

        // Act: git add
        RunGitCommand("add", "clean.txt");

        // Act: git ec-check（用户检查编码）
        var result = RunExeCommand("--check");

        // Assert: 应该通过检查
        Assert.Equal(0, result);

        // Act: 提交（模拟 ec-commit 的前半部分）
        RunGitCommand("commit", "-m", "feat: add clean file");

        // Assert: 提交成功
        var log = RunGitCommandWithOutput("log", "--oneline");
        Assert.Contains("feat: add clean file", log);
    }

    #endregion

    #region 场景2: 用户工作流 - 检查失败 → 修复 → 重新检查 → 提交

    [Fact]
    public void Scenario2_FileWithBom_ShouldDetectFixAndCommit()
    {
        // Arrange: 创建带 BOM 的文件
        var bomContent = new byte[] { 0xEF, 0xBB, 0xBF, (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)'\n' };
        CreateTestFile("withbom.txt", bomContent);

        // Act: git add
        RunGitCommand("add", "withbom.txt");

        // Act: 第一次检查（应该失败）
        var firstCheck = RunExeCommand("--check");
        Assert.NotEqual(0, firstCheck); // 应该检测到 BOM 问题

        // Act: 用户运行修复
        var fixResult = RunExeCommand("--fix");
        Assert.Equal(0, fixResult);

        // Act: 重新暂存修复后的文件
        RunGitCommand("add", "withbom.txt");

        // Act: 第二次检查（应该通过）
        var secondCheck = RunExeCommand("--check");
        Assert.Equal(0, secondCheck);

        // Act: 提交
        RunGitCommand("commit", "-m", "fix: remove BOM");

        // Assert: 提交成功
        var log = RunGitCommandWithOutput("log", "--oneline");
        Assert.Contains("fix: remove BOM", log);

        // Assert: 文件确实被修复了（无 BOM）
        var bytes = File.ReadAllBytes(Path.Combine(_testRepoDir, "withbom.txt"));
        Assert.NotEqual(0xEF, bytes[0]);
    }

    #endregion

    #region 场景3: 用户工作流 - 混合换行符检测

    [Fact]
    public void Scenario3_MixedLineEndings_ShouldDetectAndFix()
    {
        // Arrange: 创建混合换行符的文件
        var content = "line1\r\nline2\nline3\r\n";  // CRLF 和 LF 混合
        CreateTestFile("mixed.txt", content, new UTF8Encoding(false));

        // Act: git add
        RunGitCommand("add", "mixed.txt");

        // Act: 检查
        var checkResult = RunExeCommand("--check");
        Assert.NotEqual(0, checkResult); // 应该检测到混合换行符

        // Act: 修复
        var fixResult = RunExeCommand("--fix");
        Assert.Equal(0, fixResult);

        // Act: 重新暂存和检查
        RunGitCommand("add", "mixed.txt");
        var secondCheck = RunExeCommand("--check");
        Assert.Equal(0, secondCheck);
    }

    #endregion

    #region 场景4: 用户工作流 - 空白行检测

    [Fact]
    public void Scenario4_BlankLines_ShouldDetectAndFix()
    {
        // Arrange: 创建包含空白行的文件（空格和制表符组成的行）
        var content = "line1\n   \nline3\n";  // 第二行是空白行
        CreateTestFile("blank.txt", content, new UTF8Encoding(false));

        // Act: git add
        RunGitCommand("add", "blank.txt");

        // Act: 检查
        var checkResult = RunExeCommand("--check");
        Assert.NotEqual(0, checkResult); // 应该检测到空白行

        // Act: 修复
        var fixResult = RunExeCommand("--fix");
        Assert.Equal(0, fixResult);

        // Act: 重新暂存和检查
        RunGitCommand("add", "blank.txt");
        var secondCheck = RunExeCommand("--check");
        Assert.Equal(0, secondCheck);

        // Assert: 空白行已被移除
        var fixedContent = File.ReadAllText(Path.Combine(_testRepoDir, "blank.txt"));
        Assert.DoesNotContain("   \n", fixedContent);
    }

    #endregion

    #region 场景5: 用户工作流 - 多个文件批量处理

    [Fact]
    public void Scenario5_MultipleFiles_ShouldCheckAll()
    {
        // Arrange: 创建多个文件，有的有问题，有的没问题
        CreateTestFile("good.txt", "good content\n", new UTF8Encoding(false));
        var bomContent = new byte[] { 0xEF, 0xBB, 0xBF, (byte)'b', (byte)'a', (byte)'d', (byte)'\n' };
        CreateTestFile("bad.txt", bomContent);

        // Act: git add 所有文件
        RunGitCommand("add", ".");

        // Act: 检查
        var checkResult = RunExeCommand("--check");
        Assert.NotEqual(0, checkResult); // 应该因为 bad.txt 失败

        // Act: 修复
        RunExeCommand("--fix");

        // Act: 重新暂存
        RunGitCommand("add", ".");

        // Act: 再次检查
        var secondCheck = RunExeCommand("--check");
        Assert.Equal(0, secondCheck);
    }

    #endregion

    #region 场景6: 用户工作流 - 空暂存区

    [Fact]
    public void Scenario6_NoStagedFiles_ShouldReturnSuccess()
    {
        // Act: 在没有暂存文件的情况下运行检查
        var result = RunExeCommand("--check");

        // Assert: 应该返回 0（没有错误）
        Assert.Equal(0, result);
    }

    #endregion

    #region 场景7: 用户工作流 - 非文本文件跳过

    [Fact]
    public void Scenario7_BinaryFiles_ShouldBeSkipped()
    {
        // Arrange: 创建二进制文件
        var binaryContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        CreateTestFile("image.png", binaryContent);

        // Act: git add
        RunGitCommand("add", "image.png");

        // Act: 检查
        var result = RunExeCommand("--check");

        // Assert: 应该通过（二进制文件被跳过）
        Assert.Equal(0, result);
    }

    #endregion

    #region 场景8: 用户工作流 - ec-m 完整流程

    [Fact]
    public void Scenario8_EcCommit_ShouldFixAndCommit()
    {
        // Arrange: 创建有问题的文件
        var bomContent = new byte[] { 0xEF, 0xBB, 0xBF, (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)'\n' };
        CreateTestFile("commit.txt", bomContent);
        RunGitCommand("add", "commit.txt");

        // Act: 使用 ec-m 修复并提交
        // 注意：ec-m 会交互式询问提交信息，这里直接调用底层命令
        RunExeCommand("--fix");
        RunGitCommand("add", "commit.txt");
        RunGitCommand("commit", "-m", "test: ec-m workflow");

        // Assert: 提交成功
        var log = RunGitCommandWithOutput("log", "--oneline");
        Assert.Contains("test: ec-m workflow", log);

        // Assert: 文件已被修复
        var bytes = File.ReadAllBytes(Path.Combine(_testRepoDir, "commit.txt"));
        Assert.NotEqual(0xEF, bytes[0]);
    }

    #endregion

    #region 场景9: 用户工作流 - 安装/卸载钩子

    [Fact]
    public void Scenario9_InstallUninstallHooks_ShouldWork()
    {
        // Arrange: 确保已安装
        RunExeCommand("--install", "--force", "--target", "project");

        // Assert: 确认 exe 存在
        var exePath = Path.Combine(_testRepoDir, ".git", "hooks", "EncodingChecker.exe");
        Assert.True(File.Exists(exePath), "EncodingChecker.exe 应该存在");

        // Act: 卸载项目级别
        var uninstallResult = RunExeCommand("--uninstall", "--target", "project");

        // Assert: 卸载应该成功
        Assert.Equal(0, uninstallResult);

        // Assert: exe 应该被删除（或 hook 被删除）
        var hookPath = Path.Combine(_testRepoDir, ".git", "hooks", "pre-commit");
        Assert.False(File.Exists(hookPath) && IsOurHook(hookPath), "pre-commit hook 应该被移除");

        // Act: 重新安装
        var installResult = RunExeCommand("--install", "--force", "--target", "project");

        // Assert: 安装应该成功
        Assert.Equal(0, installResult);

        // Assert: exe 应该存在
        Assert.True(File.Exists(exePath), "EncodingChecker.exe 应该重新存在");
    }

    private bool IsOurHook(string hookPath)
    {
        if (!File.Exists(hookPath)) return false;
        var content = File.ReadAllText(hookPath);
        return content.Contains("EncodingChecker") || content.Contains("ec-check");
    }

    #endregion

    #region 场景10: 用户工作流 - 帮助命令

    [Fact]
    public void Scenario10_HelpCommand_ShouldReturnZero()
    {
        // Act: 运行帮助命令
        var result = RunExeCommand("--help");

        // Assert: 应该返回 0
        Assert.Equal(0, result);
    }

    #endregion
}
