using Xunit;

/// <summary>
/// CommandTable 集成测试
/// 验证命令表的结构完整性和一致性
/// 所有测试通过 CommandTable API 获取信息，不硬编码具体命令名
/// </summary>
public class CommandTableIntegrationTests
{
    /// <summary>
    /// 验证 CommandTable 不为空且包含命令
    /// </summary>
    [Fact]
    public void CommandTable_ShouldNotBeEmpty()
    {
        // Act & Assert
        Assert.NotEmpty(CommandTable.Map);
        Assert.True(CommandTable.Map.Count > 0, "CommandTable 必须包含至少一个命令");
    }

    /// <summary>
    /// 验证 CommandTable 的别名与 InstallManager 获取的别名一致
    /// 确保两个来源的命令信息完全一致
    /// </summary>
    [Fact]
    public void CommandTable_Aliases_ShouldMatchInstallManager()
    {
        // Arrange
        var fromTable = CommandTable.GetAliasList().OrderBy(a => a.Alias).ToList();
        var fromManager = InstallManager.GetAllAliasesFromReflection().OrderBy(a => a.alias).ToList();

        // Act & Assert
        Assert.Equal(fromManager.Count, fromTable.Count);

        for (int i = 0; i < fromTable.Count; i++)
        {
            Assert.Equal(fromManager[i].alias, fromTable[i].Alias);
            Assert.Equal(fromManager[i].command, fromTable[i].Command);
            Assert.Equal(fromManager[i].description, fromTable[i].Description);
        }
    }

    /// <summary>
    /// 验证 CommandTable 的 Map 是只读的且线程安全（延迟加载）
    /// </summary>
    [Fact]
    public void CommandTable_Map_ShouldBeReadOnly()
    {
        // Arrange
        var map = CommandTable.Map;

        // Act & Assert - 验证是 IReadOnlyDictionary
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, CommandEntry>>(map);

        // 验证不能修改（编译时就会报错，这里验证引用不变）
        var map2 = CommandTable.Map;
        Assert.Same(map, map2);
    }

    /// <summary>
    /// 验证 CommandTable 的所有命令条目包含完整信息
    /// </summary>
    [Fact]
    public void CommandTable_AllEntries_ShouldHaveCompleteInfo()
    {
        // Arrange
        var entries = CommandTable.Map.Values;

        // Act & Assert
        Assert.All(entries, entry =>
        {
            Assert.NotNull(entry);
            Assert.NotNull(entry.Name);
            Assert.NotEmpty(entry.Name);
            Assert.NotNull(entry.MethodInfo);
            Assert.NotNull(entry.Attribute);
            Assert.NotNull(entry.Description);
        });
    }

    /// <summary>
    /// 验证 CommandTable 支持大小写不敏感查找
    /// </summary>
    [Fact]
    public void CommandTable_ShouldBeCaseInsensitive()
    {
        // Arrange - 使用实际的第一个命令来测试
        var firstCommand = CommandTable.Map.Keys.First();
        var upperCase = firstCommand.ToUpperInvariant();
        var mixedCase = string.Join("", firstCommand.Select((c, i) => i % 2 == 0 ? char.ToUpper(c) : char.ToLower(c)));

        // Act & Assert
        Assert.True(CommandTable.ContainsCommand(upperCase), "应支持全大写查找");
        Assert.True(CommandTable.ContainsCommand(mixedCase), "应支持混合大小写查找");
        Assert.True(CommandTable.TryGetCommand(upperCase, out var entry));
        Assert.NotNull(entry);
    }

    /// <summary>
    /// 验证 CommandTable 的别名映射与命令条目一致
    /// </summary>
    [Fact]
    public void CommandTable_Aliases_ShouldBeConsistentWithEntries()
    {
        // Arrange
        var entriesWithAlias = CommandTable.Map.Values
            .Where(v => !string.IsNullOrEmpty(v.GitAlias))
            .ToList();

        // Act & Assert
        Assert.All(entriesWithAlias, entry =>
        {
            Assert.True(
                CommandTable.Aliases.ContainsKey(entry.GitAlias!),
                $"别名 {entry.GitAlias} 应在 Aliases 字典中存在"
            );
            Assert.Equal(entry.Name, CommandTable.Aliases[entry.GitAlias!]);
        });
    }

    /// <summary>
    /// 验证 CommandTable 与实际 exe 的一致性
    /// 通过检查 exe 文件存在且 CommandTable 不为空来验证
    /// </summary>
    [Fact]
    public void CommandTable_ShouldBeConsistentWithExe()
    {
        // Arrange - 获取测试输出目录中的 exe
        var testOutputDir = AppContext.BaseDirectory;
        var exePath = Path.Combine(testOutputDir, "EncodingChecker.exe");

        // Act & Assert
        Assert.True(File.Exists(exePath), "EncodingChecker.exe 必须存在");
        Assert.NotEmpty(CommandTable.Map);
        Assert.True(CommandTable.Map.Count > 0, "CommandTable 必须包含命令");

        Console.WriteLine($"[INFO] 找到 exe: {exePath}");
        Console.WriteLine($"[INFO] CommandTable 包含 {CommandTable.Map.Count} 个命令");
        Console.WriteLine($"[INFO] 命令列表: {string.Join(", ", CommandTable.GetAllCommandNames())}");
    }

    /// <summary>
    /// 验证 CommandTable 的命令名称唯一性
    /// </summary>
    [Fact]
    public void CommandTable_CommandNames_ShouldBeUnique()
    {
        // Arrange
        var names = CommandTable.GetAllCommandNames().ToList();

        // Act & Assert
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    /// <summary>
    /// 验证 CommandTable 的别名唯一性
    /// </summary>
    [Fact]
    public void CommandTable_AliasNames_ShouldBeUnique()
    {
        // Arrange
        var aliases = CommandTable.Aliases.Keys.ToList();

        // Act & Assert
        Assert.Equal(aliases.Count, aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    /// <summary>
    /// 验证 TryGetCommand 对不存在的命令返回 false
    /// </summary>
    [Fact]
    public void CommandTable_TryGetCommand_NonExisting_ShouldReturnFalse()
    {
        // Act
        var result = CommandTable.TryGetCommand("--this-command-does-not-exist", out var entry);

        // Assert
        Assert.False(result);
        Assert.Null(entry);
    }

    /// <summary>
    /// 验证 ContainsCommand 对空字符串和 null 返回 false
    /// </summary>
    [Fact]
    public void CommandTable_ContainsCommand_EmptyOrNull_ShouldReturnFalse()
    {
        // Act & Assert
        Assert.False(CommandTable.ContainsCommand(""));
        Assert.False(CommandTable.ContainsCommand(null!));
    }
}
