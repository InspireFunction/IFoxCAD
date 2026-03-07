using Xunit;

namespace IFoxFunKit.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulResult()
    {
        var result = Result<int>.Ok(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsErr);
        Assert.Equal(42, result.OkValue);
    }

    [Fact]
    public void Err_ShouldCreateFailedResult()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        Assert.False(result.IsOk);
        Assert.True(result.IsErr);
        Assert.Equal("出错了", result.ErrValue.Message);
    }

    [Fact]
    public void Ok_WithNullValue_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Ok(null!));
    }

    [Fact]
    public void Err_WithNullError_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<int>.Err(null!));
    }

    [Fact]
    public void OkValue_OnErr_ShouldThrowInvalidOperationException()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        Assert.Throws<InvalidOperationException>(() => result.OkValue);
    }

    [Fact]
    public void ErrValue_OnOk_ShouldThrowInvalidOperationException()
    {
        var result = Result<int>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => result.ErrValue);
    }

    [Fact]
    public void Match_OnOk_ShouldCallOkFunc()
    {
        var result = Result<int>.Ok(42);

        var message = result.Match(
            ok: v => $"成功: {v}",
            err: e => $"错误: {e.Message}"
        );

        Assert.Equal("成功: 42", message);
    }

    [Fact]
    public void Match_OnErr_ShouldCallErrFunc()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        var message = result.Match(
            ok: v => $"成功: {v}",
            err: e => $"错误: {e.Message}"
        );

        Assert.Equal("错误: 出错了", message);
    }

    [Fact]
    public void UnwrapOr_OnOk_ShouldReturnOkValue()
    {
        var result = Result<int>.Ok(42);

        var value = result.UnwrapOr(0);

        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOr_OnErr_ShouldReturnDefault()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        var value = result.UnwrapOr(0);

        Assert.Equal(0, value);
    }

    [Fact]
    public void Map_OnOk_ShouldTransformValue()
    {
        var result = Result<int>.Ok(42);

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.OkValue);
    }

    [Fact]
    public void Map_OnErr_ShouldReturnErr()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsErr);
    }

    [Fact]
    public void Bind_OnOk_ShouldApplyFunction()
    {
        var result = Result<int>.Ok(42);

        var bound = result.Bind(x => Result<string>.Ok(x.ToString()));

        Assert.True(bound.IsOk);
        Assert.Equal("42", bound.OkValue);
    }

    [Fact]
    public void Bind_OnErr_ShouldReturnErr()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        var bound = result.Bind(x => Result<string>.Ok(x.ToString()));

        Assert.True(bound.IsErr);
    }

    [Fact]
    public void OkToOption_OnOk_ShouldReturnSome()
    {
        var result = Result<int>.Ok(42);

        var option = result.OkToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public void OkToOption_OnErr_ShouldReturnNone()
    {
        var result = Result<int>.Err(new Exception("出错了"));

        var option = result.OkToOption();

        Assert.True(option.IsNone);
    }
}

public class TypedResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulTypedResult()
    {
        var result = Result<int, string>.Ok(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsErr);
        Assert.Equal(42, result.OkValue);
    }

    [Fact]
    public void Err_ShouldCreateFailedTypedResult()
    {
        var result = Result<int, string>.Err("出错了");

        Assert.False(result.IsOk);
        Assert.True(result.IsErr);
        Assert.Equal("出错了", result.ErrValue);
    }

    [Fact]
    public void Match_OnTypedOk_ShouldCallOkFunc()
    {
        var result = Result<int, string>.Ok(42);

        var message = result.Match(
            ok: v => $"成功: {v}",
            err: e => $"错误: {e}"
        );

        Assert.Equal("成功: 42", message);
    }

    [Fact]
    public void Match_OnTypedErr_ShouldCallErrFunc()
    {
        var result = Result<int, string>.Err("出错了");

        var message = result.Match(
            ok: v => $"成功: {v}",
            err: e => $"错误: {e}"
        );

        Assert.Equal("错误: 出错了", message);
    }

    [Fact]
    public void Map_OnTypedOk_ShouldTransformValue()
    {
        var result = Result<int, string>.Ok(42);

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.OkValue);
    }

    [Fact]
    public void Map_OnTypedErr_ShouldReturnErr()
    {
        var result = Result<int, string>.Err("出错了");

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsErr);
        Assert.Equal("出错了", mapped.ErrValue);
    }

    [Fact]
    public void MapErr_OnTypedErr_ShouldTransformError()
    {
        var result = Result<int, string>.Err("出错了");

        var mapped = result.MapErr(e => e.ToUpper());

        Assert.True(mapped.IsErr);
        Assert.Equal("出错了", mapped.ErrValue);
    }

    [Fact]
    public void Bind_OnTypedOk_ShouldApplyFunction()
    {
        var result = Result<int, string>.Ok(42);

        var bound = result.Bind(x => Result<string, string>.Ok(x.ToString()));

        Assert.True(bound.IsOk);
        Assert.Equal("42", bound.OkValue);
    }

    [Fact]
    public void Bind_OnTypedErr_ShouldReturnErr()
    {
        var result = Result<int, string>.Err("出错了");

        var bound = result.Bind(x => Result<string, string>.Ok(x.ToString()));

        Assert.True(bound.IsErr);
        Assert.Equal("出错了", bound.ErrValue);
    }
}

public class ResultStaticTests
{
    [Fact]
    public void Try_OnSuccessAction_ShouldReturnOk()
    {
        var result = Result.Try(() => { });

        Assert.True(result.IsOk);
    }

    [Fact]
    public void Try_OnSuccessFunc_ShouldReturnOkWithValue()
    {
        var result = Result.Try(() => 42);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.OkValue);
    }

    [Fact]
    public void Try_OnException_ShouldReturnErr()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("测试异常"));

        Assert.True(result.IsErr);
        Assert.Equal("测试异常", result.ErrValue.Message);
    }
}
