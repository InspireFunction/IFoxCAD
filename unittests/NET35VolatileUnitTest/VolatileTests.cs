#if NET48
using Xunit;

namespace NET35VolatileUnitTests;

public class VolatileTests
{
    [Fact]
    public void Int_ReadWrite_ReturnsCorrectValue()
    {
        var (success, message) = VolatileTestRunner.TestIntVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void Long_ReadWrite_ReturnsCorrectValue()
    {
        var (success, message) = VolatileTestRunner.TestLongVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void Float_ReadWrite_ReturnsCorrectValue()
    {
        var (success, message) = VolatileTestRunner.TestFloatVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void Double_ReadWrite_ReturnsCorrectValue()
    {
        var (success, message) = VolatileTestRunner.TestDoubleVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void Bool_True_ReadWrite_ReturnsTrue()
    {
        var (success, message) = VolatileTestRunner.TestBoolTrueVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void Bool_False_ReadWrite_ReturnsFalse()
    {
        var (success, message) = VolatileTestRunner.TestBoolFalseVolatile();
        Assert.True(success, message);
    }

    [Fact]
    public void ThreadSafety_ConcurrentReadWrite_WorksCorrectly()
    {
        var (success, message) = VolatileTestRunner.TestThreadSafety();
        Assert.True(success, message);
    }
}
#endif
