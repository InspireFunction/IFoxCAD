using Xunit;

namespace IFoxFunKit.Tests;

public class OptionTests
{
    [Fact]
    public void Some_ShouldCreateOptionWithValue()
    {
        var option = Option<int>.Some(42);

        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public void None_ShouldCreateOptionWithoutValue()
    {
        var option = Option<int>.None;

        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Some_WithNullValue_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void Value_OnNone_ShouldThrowInvalidOperationException()
    {
        var option = Option<int>.None;

        Assert.Throws<InvalidOperationException>(() => option.Value);
    }

    [Fact]
    public void Match_OnSome_ShouldCallSomeFunc()
    {
        var option = Option<int>.Some(42);

        var result = option.Match(
            some: v => $"值是: {v}",
            none: () => "没有值"
        );

        Assert.Equal("值是: 42", result);
    }

    [Fact]
    public void Match_OnNone_ShouldCallNoneFunc()
    {
        var option = Option<int>.None;

        var result = option.Match(
            some: v => $"值是: {v}",
            none: () => "没有值"
        );

        Assert.Equal("没有值", result);
    }

    [Fact]
    public void UnwrapOr_OnSome_ShouldReturnValue()
    {
        var option = Option<int>.Some(42);

        var result = option.UnwrapOr(0);

        Assert.Equal(42, result);
    }

    [Fact]
    public void UnwrapOr_OnNone_ShouldReturnDefault()
    {
        var option = Option<int>.None;

        var result = option.UnwrapOr(0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void TryGetValue_OnSome_ShouldReturnTrueAndOutputValue()
    {
        var option = Option<int>.Some(42);

        var success = option.TryGetValue(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetValue_OnNone_ShouldReturnFalse()
    {
        var option = Option<int>.None;

        var success = option.TryGetValue(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void OkOr_OnSome_ShouldReturnOk()
    {
        var option = Option<int>.Some(42);

        var result = option.OkOr("没有值");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.OkValue);
    }

    [Fact]
    public void OkOr_OnNone_ShouldReturnErr()
    {
        var option = Option<int>.None;

        var result = option.OkOr("没有值");

        Assert.True(result.IsErr);
        Assert.Equal("没有值", result.ErrValue);
    }

    [Fact]
    public void Map_OnSome_ShouldTransformValue()
    {
        var option = Option<int>.Some(42);

        var result = option.Map(x => x * 2);

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Value);
    }

    [Fact]
    public void Map_OnNone_ShouldReturnNone()
    {
        var option = Option<int>.None;

        var result = option.Map(x => x * 2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Bind_OnSome_ShouldApplyFunction()
    {
        var option = Option<int>.Some(42);

        var result = option.Bind(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.Value);
    }

    [Fact]
    public void Bind_OnNone_ShouldReturnNone()
    {
        var option = Option<int>.None;

        var result = option.Bind(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Equals_OnSameValue_ShouldReturnTrue()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(42);

        Assert.Equal(option1, option2);
    }

    [Fact]
    public void Equals_OnDifferentValue_ShouldReturnFalse()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(100);

        Assert.NotEqual(option1, option2);
    }

    [Fact]
    public void Equals_OnNone_ShouldReturnTrue()
    {
        var option1 = Option<int>.None;
        var option2 = Option<int>.None;

        Assert.Equal(option1, option2);
    }
}
