using Xunit;

/// <summary>
/// CommandTable 的单元测试
/// 测试静态命令表的各项功能
/// 所有测试通过 CommandTable API 获取信息，不硬编码具体命令名
/// </summary>
public class CommandTableTests
{
    #region Map 基础功能测试

    [Fact]
    public void Map_ShouldNotBeNull()
    {
        Assert.NotNull(CommandTable.Map);
    }

    [Fact]
    public void Map_ShouldNotBeEmpty()
    {
        Assert.NotEmpty(CommandTable.Map);
        Assert.True(CommandTable.Map.Count > 0, "应包含至少一个命令");
    }

    [Fact]
    public void Map_ShouldBeCaseInsensitive()
    {
        // 使用第一个命令来测试大小写不敏感
        var firstCommand = CommandTable.Map.Keys.First();
        var upperCase = firstCommand.ToUpperInvariant();

        // 验证大小写不敏感
        Assert.True(CommandTable.Map.ContainsKey(upperCase), "应支持全大写查找");
    }

    [Fact]
    public void Map_ShouldContainCommandEntryWithCorrectInfo()
    {
        // 使用第一个命令来测试
        var firstCommand = CommandTable.Map.Keys.First();
        var entry = CommandTable.Map[firstCommand];

        Assert.NotNull(entry);
        Assert.Equal(firstCommand, entry.Name);
        Assert.NotNull(entry.MethodInfo);
        Assert.NotNull(entry.Attribute);
    }

    #endregion

    #region ContainsCommand 测试

    [Theory]
    [InlineData("--unknown", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ContainsCommand_InvalidInput_ShouldReturnFalse(string? commandName, bool expected)
    {
        var result = CommandTable.ContainsCommand(commandName!);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ContainsCommand_ExistingCommand_ShouldReturnTrue()
    {
        // 使用第一个命令来测试
        var firstCommand = CommandTable.Map.Keys.First();

        var result = CommandTable.ContainsCommand(firstCommand);
        Assert.True(result);
    }

    #endregion

    #region TryGetCommand 测试

    [Fact]
    public void TryGetCommand_ExistingCommand_ShouldReturnTrueAndEntry()
    {
        // 使用第一个命令来测试
        var firstCommand = CommandTable.Map.Keys.First();

        var result = CommandTable.TryGetCommand(firstCommand, out var entry);

        Assert.True(result);
        Assert.NotNull(entry);
        Assert.Equal(firstCommand, entry!.Name);
    }

    [Fact]
    public void TryGetCommand_NonExistingCommand_ShouldReturnFalseAndNull()
    {
        var result = CommandTable.TryGetCommand("--nonexistent", out var entry);

        Assert.False(result);
        Assert.Null(entry);
    }

    [Fact]
    public void TryGetCommand_NullOrEmpty_ShouldReturnFalse()
    {
        Assert.False(CommandTable.TryGetCommand(null!, out _));
        Assert.False(CommandTable.TryGetCommand("", out _));
    }

    #endregion

    #region Aliases 测试

    [Fact]
    public void Aliases_ShouldNotBeNull()
    {
        Assert.NotNull(CommandTable.Aliases);
    }

    [Fact]
    public void Aliases_ShouldBeConsistentWithMap()
    {
        // 验证 Aliases 中的每个别名都能在 Map 中找到对应的命令
        foreach (var alias in CommandTable.Aliases)
        {
            Assert.True(
                CommandTable.Map.ContainsKey(alias.Value),
                $"别名 {alias.Key} 对应的命令 {alias.Value} 应在 Map 中存在"
            );
        }
    }

    #endregion

    #region GetAllCommandNames 测试

    [Fact]
    public void GetAllCommandNames_ShouldReturnAllCommands()
    {
        var commands = CommandTable.GetAllCommandNames().ToList();

        Assert.NotEmpty(commands);
        Assert.Equal(CommandTable.Map.Count, commands.Count);

        // 验证每个命令名都在 Map 中
        foreach (var cmd in commands)
        {
            Assert.True(CommandTable.Map.ContainsKey(cmd));
        }
    }

    #endregion

    #region GetAliasList 测试

    [Fact]
    public void GetAliasList_ShouldReturnAllAliasInfo()
    {
        var aliases = CommandTable.GetAliasList().ToList();

        // 验证别名数量与 Aliases 字典一致
        Assert.Equal(CommandTable.Aliases.Count, aliases.Count);

        // 验证每个别名信息完整
        Assert.All(aliases, alias =>
        {
            Assert.NotNull(alias.Alias);
            Assert.NotNull(alias.Command);
            Assert.NotNull(alias.Description);
        });
    }

    [Fact]
    public void GetAliasList_ShouldMatchInstallManager_GetAllAliasesFromReflection()
    {
        // 验证 CommandTable.GetAliasList 与 InstallManager.GetAllAliasesFromReflection 返回一致
        var fromTable = CommandTable.GetAliasList().OrderBy(a => a.Alias).ToList();
        var fromManager = InstallManager.GetAllAliasesFromReflection().OrderBy(a => a.alias).ToList();

        Assert.Equal(fromManager.Count, fromTable.Count);

        for (int i = 0; i < fromTable.Count; i++)
        {
            Assert.Equal(fromManager[i].alias, fromTable[i].Alias);
            Assert.Equal(fromManager[i].command, fromTable[i].Command);
            Assert.Equal(fromManager[i].description, fromTable[i].Description);
        }
    }

    #endregion

    #region Execute 测试

    [Fact]
    public void Execute_NonExistingCommand_ShouldThrowKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            CommandTable.Execute("--nonexistent"));
    }

    [Fact]
    public void TryExecute_NonExistingCommand_ShouldReturnFalse()
    {
        var result = CommandTable.TryExecute("--nonexistent", null, out var exitCode);

        Assert.False(result);
        Assert.Equal(1, exitCode);
    }

    #endregion

    #region CommandEntry 测试

    [Fact]
    public void CommandEntry_ShouldExposeCorrectProperties()
    {
        // 使用第一个带别名的命令来测试
        var entry = CommandTable.Map.Values.FirstOrDefault(e => !string.IsNullOrEmpty(e.GitAlias));

        Assert.NotNull(entry);
        Assert.NotNull(entry!.MethodInfo);
        Assert.NotNull(entry.Attribute);
        Assert.NotNull(entry.Name);
        Assert.NotNull(entry.GitAlias);
        Assert.NotNull(entry.Description);
    }

    #endregion

    #region 一致性测试

    [Fact]
    public void CommandTable_ShouldBeConsistentAcrossMultipleAccesses()
    {
        // 多次访问应该返回相同的命令列表
        var map1 = CommandTable.Map;
        var map2 = CommandTable.Map;

        Assert.Equal(map1.Count, map2.Count);

        foreach (var key in map1.Keys)
        {
            Assert.True(map2.ContainsKey(key));
            Assert.Equal(map1[key].Name, map2[key].Name);
        }
    }

    [Fact]
    public void CommandTable_MapAndAliases_ShouldBeConsistent()
    {
        // Map 中的别名信息应该与 Aliases 字典一致
        var mapAliases = CommandTable.Map.Values
            .Where(v => !string.IsNullOrEmpty(v.GitAlias))
            .ToDictionary(v => v.GitAlias!, v => v.Name);

        Assert.Equal(mapAliases.Count, CommandTable.Aliases.Count);

        foreach (var alias in mapAliases.Keys)
        {
            Assert.True(CommandTable.Aliases.ContainsKey(alias));
            Assert.Equal(mapAliases[alias], CommandTable.Aliases[alias]);
        }
    }

    /// <summary>
    /// 验证命令名称唯一性（大小写不敏感）
    /// </summary>
    [Fact]
    public void CommandTable_CommandNames_ShouldBeUnique()
    {
        var names = CommandTable.GetAllCommandNames().ToList();
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    /// <summary>
    /// 验证别名唯一性（大小写不敏感）
    /// </summary>
    [Fact]
    public void CommandTable_AliasNames_ShouldBeUnique()
    {
        var aliases = CommandTable.Aliases.Keys.ToList();
        Assert.Equal(aliases.Count, aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    #endregion
}
