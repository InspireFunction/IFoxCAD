namespace BasalUnitTests;

public class TupleAndIndexTests
{
    [Fact]
    public void ValueTuple_Create_ReturnsCorrectValues()
    {
        var tuple = (1, 2);

        Assert.Equal(1, tuple.Item1);
        Assert.Equal(2, tuple.Item2);
    }

    [Fact]
    public void ValueTuple_ToString_ReturnsFormattedString()
    {
        var tuple = (1, 2);
        var result = tuple.ToString();

        Assert.Contains("1", result);
        Assert.Contains("2", result);
    }

    [Fact]
    public void ValueTuple_WithNames_AccessByName()
    {
        var tuple = (Id: 1, Name: "Test");

        Assert.Equal(1, tuple.Id);
        Assert.Equal("Test", tuple.Name);
    }

    [Fact]
    public void Array_GetLastElement_UsingLength()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var lastElement = array[array.Length - 1];

        Assert.Equal(5, lastElement);
    }

    [Fact]
    public void Array_GetThirdFromEnd_UsingLength()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var element = array[array.Length - 3];

        Assert.Equal(3, element);
    }

    [Fact]
    public void Array_SubArray_UsingLinq()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var range = array.Skip(1).Take(2).ToArray();

        Assert.Equal(2, range.Length);
        Assert.Equal(2, range[0]);
        Assert.Equal(3, range[1]);
    }

    [Fact]
    public void Array_SubArrayFromStart_UsingLinq()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var range = array.Take(3).ToArray();

        Assert.Equal(3, range.Length);
        Assert.Equal(1, range[0]);
        Assert.Equal(2, range[1]);
        Assert.Equal(3, range[2]);
    }

    [Fact]
    public void Array_SubArrayToEnd_UsingLinq()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var range = array.Skip(2).ToArray();

        Assert.Equal(3, range.Length);
        Assert.Equal(3, range[0]);
        Assert.Equal(4, range[1]);
        Assert.Equal(5, range[2]);
    }

    [Fact]
    public void Array_GetRange_UsingLinq()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var subArray = array.Skip(1).Take(array.Length - 2).ToArray();

        Assert.Equal(3, subArray.Length);
        Assert.Equal(2, subArray[0]);
        Assert.Equal(3, subArray[1]);
        Assert.Equal(4, subArray[2]);
    }
}
