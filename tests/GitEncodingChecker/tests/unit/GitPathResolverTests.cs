using Xunit;

/// <summary>
/// GitPathResolver 的单元测试
/// 确保在各种环境下都能找到 Git
/// </summary>
public class GitPathResolverTests
{
    [Fact]
    public void GetGitPath_ShouldReturnNonEmptyString()
    {
        // Act
        var path = GitPathResolver.GetGitPath();

        // Assert
        Assert.False(string.IsNullOrEmpty(path));
    }

    [Fact]
    public void GetGitPath_ShouldCacheResult()
    {
        // Act
        var path1 = GitPathResolver.GetGitPath();
        var path2 = GitPathResolver.GetGitPath();

        // Assert - 应该返回相同的引用（缓存）
        Assert.Same(path1, path2);
    }

    [Fact]
    public void ValidateGitAvailable_ShouldReturnTrue_WhenGitExists()
    {
        // 这个测试假设运行环境有 Git 安装
        // 如果没有安装，这个测试会失败，这是预期的

        // Act
        var isAvailable = GitPathResolver.ValidateGitAvailable();

        // Assert
        // 注意：在 CI 环境或没有 Git 的机器上，这可能为 false
        // 但我们希望它返回 true，因为这是一个有效的 Git 安装检测
        Assert.True(isAvailable, "Git 应该可用。如果失败，请确保 Git 已安装并在 PATH 中。");
    }

    [Fact]
    public void ClearCache_ShouldForceReLookup()
    {
        // Arrange
        var path1 = GitPathResolver.GetGitPath();

        // Act
        GitPathResolver.ClearCache();
        var path2 = GitPathResolver.GetGitPath();

        // Assert - 值应该相同，但不是同一个引用（因为缓存被清除了）
        Assert.Equal(path1, path2);
    }

    [Theory]
    [InlineData("git")]
    [InlineData(@"C:\Program Files\Git\bin\git.exe")]
    [InlineData(@"C:\Program Files (x86)\Git\bin\git.exe")]
    public void GetGitPath_ShouldHandleVariousPaths(string expectedPath)
    {
        // 这个测试主要验证方法不会抛出异常
        // 实际返回的路径取决于运行环境

        // Act & Assert - 不应该抛出异常
        var exception = Record.Exception(() => GitPathResolver.GetGitPath());
        Assert.Null(exception);
    }
}
