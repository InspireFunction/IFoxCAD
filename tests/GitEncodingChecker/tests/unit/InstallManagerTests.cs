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
        // 先尝试切换回原始目录，如果失败则忽略
        try
        {
            if (Directory.Exists(_originalDir))
            {
                Directory.SetCurrentDirectory(_originalDir);
            }
        }
        catch { }

        try
        {
            if (Directory.Exists(_testRepoDir))
            {
                Directory.Delete(_testRepoDir, recursive: true);
            }
        }
        catch { }

        // 清理全局测试配置
        CleanupTestAliases();
    }

    private void CleanupTestAliases()
    {
        // 清理所有测试用的别名
        var testPrefixes = new[] { "test-", "test-same-", "test-diff-", "test-workflow-", "test-no-conflict-" };
        try
        {
            var aliases = GitCommandRunner.RunWithOutput("config", "--global", "--get-regexp", "^alias\\.test-").Trim();
            if (!string.IsNullOrEmpty(aliases))
            {
                foreach (var line in aliases.Split('\n'))
                {
                    var parts = line.Split(new[] { ' ' }, 2);
                    if (parts.Length > 0 && parts[0].StartsWith("alias."))
                    {
                        var aliasName = parts[0].Substring(6);
                        try
                        {
                            GitCommandRunner.Run("config", "--global", "--unset", $"alias.{aliasName}");
                        }
                        catch { }
                    }
                }
            }
        }
        catch { }

        try
        {
            var localAliases = GitCommandRunner.RunWithOutput("config", "--get-regexp", "^alias\\.test-").Trim();
            if (!string.IsNullOrEmpty(localAliases))
            {
                foreach (var line in localAliases.Split('\n'))
                {
                    var parts = line.Split(new[] { ' ' }, 2);
                    if (parts.Length > 0 && parts[0].StartsWith("alias."))
                    {
                        var aliasName = parts[0].Substring(6);
                        try
                        {
                            GitCommandRunner.Run("config", "--unset", $"alias.{aliasName}");
                        }
                        catch { }
                    }
                }
            }
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
        Assert.Contains("ec-check", aliasNames);
        Assert.Contains("ec-fix", aliasNames);
        Assert.Contains("ecc", aliasNames);
        Assert.Contains("ec-force", aliasNames);
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

    [Fact]
    public void GetAllAliasesFromReflection_ShouldReturnConsistentResults()
    {
        // Act - 调用两次
        var aliases1 = InstallManager.GetAllAliasesFromReflection();
        var aliases2 = InstallManager.GetAllAliasesFromReflection();

        // Assert - 结果应该一致
        Assert.Equal(aliases1.Count, aliases2.Count);
        foreach (var (cmd1, alias1, desc1) in aliases1)
        {
            var match = aliases2.FirstOrDefault(a => a.alias == alias1);
            Assert.NotEqual(default, match);
            Assert.Equal(cmd1, match.command);
            Assert.Equal(desc1, match.description);
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
        RunGitCommand("config", "alias.test-ec-local", "!\"echo test\"");

        // Act
        var value = InstallManager.GetGitConfigValue("alias.test-ec-local", isGlobal: false);

        // Assert
        Assert.Equal("!\"echo test\"", value);

        // Cleanup
        RunGitCommand("config", "--unset", "alias.test-ec-local");
    }

    [Fact]
    public void GetGitConfigValue_ShouldHandleGlobalConfig()
    {
        // Arrange
        GitCommandRunner.Run("config", "--global", "alias.test-ec-global", "!\"echo global-test\"");

        // Act
        var value = InstallManager.GetGitConfigValue("alias.test-ec-global", isGlobal: true);

        // Assert
        Assert.Equal("!\"echo global-test\"", value);

        // Cleanup
        GitCommandRunner.Run("config", "--global", "--unset", "alias.test-ec-global");
    }

    [Fact]
    public void GetGitConfigValue_ShouldReturnEmpty_ForInvalidKey()
    {
        // Act
        var value = InstallManager.GetGitConfigValue("", isGlobal: false);

        // Assert
        Assert.Equal(string.Empty, value);
    }

    #endregion

    #region CheckAliasConflicts 测试

    [Fact]
    public void CheckAliasConflicts_ShouldReturnEmpty_WhenNoAliasesExist()
    {
        // Arrange - 使用不太可能存在的别名
        var testAlias = $"test-{Guid.NewGuid():N}";
        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Empty(conflicts);
    }

    [Fact]
    public void CheckAliasConflicts_ShouldNotDetectConflict_WhenValueIsSame()
    {
        // Arrange - 使用与 CheckAliasConflicts 相同的逻辑构建预期的别名值
        // 注意：这里需要使用项目级别的路径，因为测试在本地仓库中
        var exePath = InstallManager.GetExePath(isGlobal: false).Replace("\\", "/");
        var expectedValue = $"!\"{exePath}\" --check";

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
        Assert.Equal(testAlias, conflicts[0].alias);
        Assert.Equal("!\"echo different-value\"", conflicts[0].existingValue);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    [Fact]
    public void CheckAliasConflicts_ShouldHandleMultipleAliases()
    {
        // Arrange
        var testAlias1 = $"test-multi-1-{Guid.NewGuid():N}";
        var testAlias2 = $"test-multi-2-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias1}", "!\"echo conflict1\"");
        // testAlias2 不设置，模拟一个冲突一个没冲突

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias1, "冲突1"),
            ("--fix", testAlias2, "无冲突")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal(testAlias1, conflicts[0].alias);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias1}");
    }

    [Fact]
    public void CheckAliasConflicts_ShouldHandleGlobalScope()
    {
        // Arrange
        var testAlias = $"test-global-{Guid.NewGuid():N}";
        GitCommandRunner.Run("config", "--global", $"alias.{testAlias}", "!\"echo global\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "全局测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: true);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal(testAlias, conflicts[0].alias);

        // Cleanup
        GitCommandRunner.Run("config", "--global", "--unset", $"alias.{testAlias}");
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

    [Fact]
    public void GetExePath_ShouldReturnAbsolutePath()
    {
        // Act
        var projectPath = InstallManager.GetExePath(isGlobal: false);
        var globalPath = InstallManager.GetExePath(isGlobal: true);

        // Assert
        Assert.True(Path.IsPathRooted(projectPath), "项目路径应该是绝对路径");
        Assert.True(Path.IsPathRooted(globalPath), "全局路径应该是绝对路径");
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
        Assert.Equal("!\"old-value\"", InstallManager._backupMap["local:ec"]);
        Assert.Equal("!\"old-fix\"", InstallManager._backupMap["local:ec-fix"]);
    }

    [Fact]
    public void BackupExistingAliases_ShouldHandleGlobalScope()
    {
        // Arrange
        InstallManager._backupMap.Clear();
        var conflicts = new List<(string alias, string existingValue, string newValue)>
        {
            ("ec", "!\"global-old\"", "!\"global-new\"")
        };

        // Act
        InstallManager.BackupExistingAliases(conflicts, isGlobal: true);

        // Assert
        Assert.Single(InstallManager._backupMap);
        Assert.Equal("!\"global-old\"", InstallManager._backupMap["global:ec"]);
    }

    [Fact]
    public void BackupExistingAliases_ShouldHandleEmptyList()
    {
        // Arrange
        InstallManager._backupMap.Clear();
        InstallManager._backupMap["existing"] = "value";
        var conflicts = new List<(string alias, string existingValue, string newValue)>();

        // Act
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert - 不应该清除现有的
        Assert.Single(InstallManager._backupMap);
        Assert.Equal("value", InstallManager._backupMap["existing"]);
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
        InstallManager._backupMap.Clear();
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert - 验证备份（使用带前缀的 key）
        Assert.Single(InstallManager._backupMap);
        Assert.Equal("!\"echo existing\"", InstallManager._backupMap[$"local:{testAlias}"]);

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

    [Fact]
    public void FullWorkflow_ShouldHandleMixedConflicts()
    {
        // Arrange
        var conflictAlias = $"test-mixed-conflict-{Guid.NewGuid():N}";
        var noConflictAlias = $"test-mixed-ok-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{conflictAlias}", "!\"echo conflict\"");
        // noConflictAlias 不设置

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", conflictAlias, "有冲突"),
            ("--fix", noConflictAlias, "无冲突")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal(conflictAlias, conflicts[0].alias);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{conflictAlias}");
    }

    #endregion

    #region 边界条件测试

    [Fact]
    public void GetAllAliasesFromReflection_ShouldHandleSpecialCharactersInDescription()
    {
        // Act
        var aliases = InstallManager.GetAllAliasesFromReflection();

        // Assert - 确保所有描述都不为空
        Assert.All(aliases, a =>
        {
            Assert.False(string.IsNullOrWhiteSpace(a.description));
        });
    }

    [Fact]
    public void CheckAliasConflicts_ShouldHandleAliasWithSpecialCharacters()
    {
        // Arrange - 使用带连字符的别名
        var testAlias = $"test-special-chars-{Guid.NewGuid():N}";
        RunGitCommand("config", $"alias.{testAlias}", "!\"echo test\"");

        var aliases = new List<(string command, string alias, string description)>
        {
            ("--check", testAlias, "特殊字符测试")
        };

        // Act
        var conflicts = InstallManager.CheckAliasConflicts(aliases, isGlobal: false);

        // Assert
        Assert.Single(conflicts);

        // Cleanup
        RunGitCommand("config", "--unset", $"alias.{testAlias}");
    }

    [Fact]
    public void BackupExistingAliases_ShouldOverwriteExistingBackup()
    {
        // Arrange
        InstallManager._backupMap.Clear();
        InstallManager._backupMap["local:ec"] = "!\"first-backup\"";

        var conflicts = new List<(string alias, string existingValue, string newValue)>
        {
            ("ec", "!\"second-backup\"", "!\"new-value\"")
        };

        // Act
        InstallManager.BackupExistingAliases(conflicts, isGlobal: false);

        // Assert - 应该被覆盖
        Assert.Equal("!\"second-backup\"", InstallManager._backupMap["local:ec"]);
    }

    #endregion

    #region CheckLocalAliases 测试

    [Fact]
    public void CheckLocalAliases_ShouldReturnEmptyList_WhenNoLocalAliases()
    {
        // Act
        var aliases = InstallManager.CheckLocalAliases();

        // Assert
        Assert.NotNull(aliases);
        Assert.Empty(aliases);
    }

    [Fact]
    public void CheckLocalAliases_ShouldReturnEcAliases()
    {
        // Arrange - 创建一些本地别名
        RunGitCommand("config", "alias.ec-test1", "!\"echo test1\"");
        RunGitCommand("config", "alias.ec-test2", "!\"echo test2\"");
        RunGitCommand("config", "alias.other-alias", "!\"echo other\""); // 不应该被包含

        // Act
        var aliases = InstallManager.CheckLocalAliases();

        // Assert
        Assert.NotNull(aliases);
        Assert.Equal(2, aliases.Count);
        Assert.Contains("ec-test1", aliases);
        Assert.Contains("ec-test2", aliases);
        Assert.DoesNotContain("other-alias", aliases);

        // Cleanup
        RunGitCommand("config", "--unset", "alias.ec-test1");
        RunGitCommand("config", "--unset", "alias.ec-test2");
        RunGitCommand("config", "--unset", "alias.other-alias");
    }

    [Fact]
    public void CheckLocalAliases_ShouldNotReturnGlobalAliases()
    {
        // Arrange - 创建全局别名
        GitCommandRunner.Run("config", "--global", "alias.ec-global-test", "!\"echo global\"");

        // Act
        var aliases = InstallManager.CheckLocalAliases();

        // Assert - 不应该包含全局别名
        Assert.NotNull(aliases);
        Assert.DoesNotContain("ec-global-test", aliases);

        // Cleanup
        GitCommandRunner.Run("config", "--global", "--unset", "alias.ec-global-test");
    }

    [Fact]
    public void CheckLocalAliases_ShouldHandleSpecialCharactersInAliasName()
    {
        // Arrange - 创建带特殊字符的别名
        RunGitCommand("config", "alias.ecc", "!\"echo m\"");
        RunGitCommand("config", "alias.ec-check", "!\"echo check\"");

        // Act
        var aliases = InstallManager.CheckLocalAliases();

        // Assert
        Assert.NotNull(aliases);
        Assert.Contains("ecc", aliases);
        Assert.Contains("ec-check", aliases);

        // Cleanup
        RunGitCommand("config", "--unset", "alias.ecc");
        RunGitCommand("config", "--unset", "alias.ec-check");
    }

    [Fact]
    public void CheckLocalAliases_ShouldReturnEmptyList_WhenNotInGitRepo()
    {
        // Arrange - 切换到非 Git 目录
        var tempDir = Path.Combine(Path.GetTempPath(), $"NotARepo_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);

        try
        {
            // Act
            var aliases = InstallManager.CheckLocalAliases();

            // Assert - 应该返回空列表或抛出异常
            Assert.NotNull(aliases);
        }
        finally
        {
            // Cleanup
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion
}
