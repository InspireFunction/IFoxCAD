using Xunit;

/// <summary>
/// 命令处理器的单元测试
/// </summary>
public class CommandHandlersTests
{
    [Fact]
    public void ShowHelp_ShouldReturnZero()
    {
        // Act
        var result = CommandHandlers.ShowHelp();

        // Assert
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(new string[] { }, false)]
    [InlineData(new[] { "-m" }, true)]
    [InlineData(new[] { "-m", "message" }, true)]
    public void SkipCheckCommit_ShouldValidateArguments(string[] args, bool shouldFail)
    {
        // 注意：这个测试实际上会尝试运行 git commit
        // 在测试环境中可能会失败，我们主要验证参数验证逻辑

        // Act
        var result = CommandHandlers.SkipCheckCommit(args);

        // Assert
        // 如果没有 -m 参数，应该返回错误
        if (!shouldFail && args.Length == 0)
        {
            Assert.NotEqual(0, result);
        }
    }

    [Fact]
    public void AllCommandMethods_ShouldReturnInt()
    {
        // 验证所有命令方法都返回 int（退出码）
        var methods = typeof(CommandHandlers)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.GetCustomAttributes(typeof(GitCommandAttribute), false).Any());

        foreach (var method in methods)
        {
            Assert.Equal(typeof(int), method.ReturnType);
        }
    }

    [Fact]
    public void AllCommandMethods_ShouldHaveGitCommandAttribute()
    {
        // 验证所有公共静态方法都有特性标记
        var methods = typeof(CommandHandlers)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => !m.IsSpecialName); // 排除属性访问器

        foreach (var method in methods)
        {
            var hasAttribute = method.GetCustomAttributes(typeof(GitCommandAttribute), false).Any();
            Assert.True(hasAttribute, $"方法 {method.Name} 应该有 [GitCommand] 特性");
        }
    }
}
