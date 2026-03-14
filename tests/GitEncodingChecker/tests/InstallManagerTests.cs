using Xunit;
using System.Reflection;

/// <summary>
/// InstallManager 的单元测试
/// 测试冲突检测、备份等功能
/// </summary>
public class InstallManagerTests : IDisposable
{
    private readonly string _testRepoDir;
    private readonly string _originalDir;

    public InstallManagerTests()
    {
        _originalDir = Directory.GetCurrentDirectory();
        _testRepoDir = Path.Combine(Path.GetTempPath(), $"InstallManagerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoDir);

        // 初始化 Git 仓库
        RunGitCommand("init");
        RunGitCommand("config", "user.email", "test@test.com");
        RunGitCommand("config", "user.name", "Test User");

        // 切换到测试目录
        Directory.SetCurrentDirectory(_testRepoDir);

        // 清空备份映射
        InstallManager._backupMap.Clear();
        InstallManager._customPrefix = null;
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

        // 清理全局测试配置
        try
        {
            GitCommandRunner.Run("config", "--global", "--unset", "alias.test-ec");
        }
        catch { }
    }

    private void RunGitCommand(params string[] args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
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

        using var proc = System.Diagnostics.Process.Start(psi);
        proc?.WaitForExit();
    }

    #region GetAllAliasesFromReflection 测试

    [Fact]
    public void GetAllAliasesFromReflection_ShouldReturnAllGitCommandAliases()
    {
        // Act
        var aliases = InstallManager.GetAllAliasesFromReflection();

        // Assert
        Assert.NotNull(aliases);
        Assert.NotEmpty(aliases);

        // 验证包含预期的别名
        var aliasNames = aliases.Select(a => a.alias).ToList();
        Assert.Contains("ec", aliasNames);
        Assert.Contains("ec-fix", aliasNames);
        Assert.Contains("ecc", aliasNames);
        Assert.Contains("ec-commit", aliasNames);
        Assert.Contains("ec-install", aliasNames);
        Assert.Contains("ec-uninstall", aliasNames);
        Assert.Contains("ec-global", aliasNames);
        Assert.Contains("ec-uninstall-global", aliasNames);
    }

    [Fact]
    public void GetAllAliasesFromReflection_ShouldHaveValidCommandNames()
    {
        // Act
        var aliases = InstallManager.GetAllAliasesFromReflection();

        // Assert
        foreach (var (command, alias, description) in aliases)
        {
            Assert.False(string.IsNullOrEmpty(command), "命令名称不应为空");
            Assert.StartsWith("--", command);
            Assert.False(string.IsNullOrEmpty(alias), "别名不应为空");
            Assert.False(string.IsNullOrEmpty(description), "描述不应为空");
        }
    }

    #endregion

    #region GetGitConfigValue 测试

    [Fact]
    public void GetGitConfigValue_ShouldReturnEmpty_WhenKeyNotExists()
    {
        // Act
        var value = InstallManager.GetGitConfigValue("alias.nonexistent-key-12345", isGlobal: false);

        // Assert
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void GetGitConfigValue_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        RunGitCommand("config", "alias.test-ec", "!\"echo test\"");

        // Act
        var value = InstallManager.GetGitConfigValue("alias.test-ec", isGlobal: false);

        // Assert
        Assert.Equal("!\"echo test\"", value);

        // Cleanup
        RunGitCommand("config", "--unset", "alias.test-ec");
    }

    [Fact]
    public void GetGitConfigValue_ShouldHandleGlobalConfig()
    {
        // Arrange
        GitCommandRunner.Run("config", "--global", "alias.test-ec", "!\"echo global-test\"");

        // Act
        var value = InstallManager.GetGitConfigValue("alias.test-ec", isGlobal: true);

        // Assert
        Assert.Equal("!\"echo global-test\"", value);

        // Cleanup
        GitCommandRunner.Run("config", "--global", "--unset", "alias.test-ec");
    }

    #endregion

    #region CheckAliasConflicts 测试 - 无冲突场景

    [Fact]
    public void CheckAliasConflicts_ShouldReturnEmpty_WhenNoAliasesExist()
    {
        // Arrange
        var aliases = new List<(string command, string alias, string description)>
        {
            ("--test", "test-alias-1", "测试别名1"),
            ("--test2", "test-alias-2", "测试别名2")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Empty(conflicts);
    }

    #endregion

    #region CheckAliasConflicts 测试 - 有冲突场景

    [Fact]
    public void CheckAliasConflicts_ShouldDetectConflicts_WhenAliasesExist()
    {
        // Arrange
        var testAlias = $"test-conflict-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias}", "!\"echo existing\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "测试冲突")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal(testAlias, conflicts[0].alias);
        Assert.Equal("!\"echo existing\"", conflicts[0].existingValue);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    [Fact]
    public void CheckAliasConflicts_ShouldDetectMultipleConflicts()
    {
        // Arrange
        var testAlias1 = $"test-multi-1-{Guid.NewGuid():N}";
        var testAlias2 = $"test-multi-2-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias1}", "!\"echo existing1\"");
        RunGitCommand("config", $"alias.{testAlias2}", "!\"echo existing2\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias1, "测试冲突1"),
            ("--fix", testAlias2, "测试冲突2"),
            ("--new", $"test-multi-3-{Guid.NewGuid():N}", "无冲突")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Equal(2, conflicts.Count);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias1}");
        RunGitCommand("config", "--unset", $"alias.{testAlias2}");
    }

    #endregion

    #region CheckAliasConflicts 测试 - 值相同不算冲突

    [Fact]
    public void CheckAliasConflicts_ShouldNotDetectConflict_WhenValueIsSame()
    {
        // Arrange - 使用与 CheckAliasConflicts 相同的逻辑构建预期的别名值
        // 注意：GetExePath 使用 GetRepoRoot() 获取当前仓库路径
        var exePath = InstallManager.GetExePath(isGlobal: false).Replace("\\", "/");
        var expectedValue = $"!\"{exePath}\"";

        var testAlias = $"test-same-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias}", expectedValue);

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "相同值测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert - 值相同，不算冲突
        Assert.Empty(conflicts);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    [Fact]
    public void CheckAliasConflicts_ShouldDetectConflict_WhenValueIsDifferent()
    {
        // Arrange
        var testAlias = $"test-diff-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias}", "!\"echo different-value\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "不同值测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert - 值不同，算冲突
        Assert.Single(conflicts);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    #endregion

    #region GetExePath 测试

    [Fact]
    public void GetExePath_ShouldReturnProjectPath_WhenIsGlobalIsFalse()
    {
        // Act
        var path = InstallManager.GetExePath(isGlobal: false);

        // Assert
        Assert.Contains(".git\\hooks\\EncodingChecker.exe", path);
        Assert.DoesNotContain(".git-hooks", path);
    }

    [Fact]
    public void GetExePath_ShouldReturnGlobalPath_WhenIsGlobalIsTrue()
    {
        // Act
        var path = InstallManager.GetExePath(isGlobal: true);

        // Assert
        Assert.Contains(".git-hooks\\EncodingChecker.exe", path);
        Assert.DoesNotContain(".git\\hooks", path);
    }

    #endregion

    #region BackupExistingAliases 测试

    [Fact]
    public void BackupExistingAliases_ShouldPopulateBackupMap()
    {
        // Arrange
        InstallManager._backupMap.Clear();
        var conflicts = new List<(string alias, string existingValue, string newValue)>
        {
            ("ec", "!\"old-value\"", "!\"new-value\""),
            ("ec-fix", "!\"old-fix\"", "!\"new-fix\"")
        };

        // Act
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert
        Assert.Equal(2, InstallManager._backupMap.Count);
        Assert.Equal("!\"old-value\"", InstallManager._backupMap["ec"]);
        Assert.Equal("!\"old-fix\"", InstallManager._backupMap["ec-fix"]);
    }

    [Fact]
    public void BackupExistingAliases_ShouldClearExistingBackupMap()
    {
        // Arrange - 先放入一些旧数据
        InstallManager._backupMap["old-key"] = "old-value";

        var conflicts = new List<(string alias, string existingValue, string newValue)>
        {
            ("ec", "!\"new-backup\"", "!\"new-value\"")
        };

        // Act
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert - 旧数据应该被清除
        Assert.Single(InstallManager._backupMap);
        Assert.False(InstallManager._backupMap.ContainsKey("old-key"));
        Assert.Equal("!\"new-backup\"", InstallManager._backupMap["ec"]);
    }

    #endregion

    #region 集成测试

    [Fact]
    public void FullWorkflow_ShouldDetectAndHandleConflicts()
    {
        // Arrange - 设置一个已存在的别名
        var testAlias = $"test-workflow-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias}", "!\"echo existing\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "工作流测试")
        };

        // Act - 检查冲突
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal(testAlias, conflicts[0].alias);
        Assert.Equal("!\"echo existing\"", conflicts[0].existingValue);

        // Act - 备份
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert - 验证备份
        Assert.Single(InstallManager._backupMap);
        Assert.Equal("!\"echo existing\"", InstallManager._backupMap[testAlias]);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    [Fact]
    public void FullWorkflow_ShouldPass_WhenNoConflicts()
    {
        // Arrange - 使用一个不太可能存在的别名
        var testAlias = $"test-no-conflict-{Guid.NewGuid():N}";

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "无冲突测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Empty(conflicts);
    }

    #endregion
}
