namespace CadVersionUnitTests;

public class VersionToolTests
{
    [Fact]
    public void IndexToYear_ValidIndex_ReturnsCorrectYear()
    {
        Assert.Equal(2000, VersionTool.IndexToYear(0));
        Assert.Equal(2001, VersionTool.IndexToYear(1));
        Assert.Equal(2004, VersionTool.IndexToYear(4));
        Assert.Equal(2007, VersionTool.IndexToYear(7));
        Assert.Equal(2010, VersionTool.IndexToYear(10));
        Assert.Equal(2013, VersionTool.IndexToYear(13));
        Assert.Equal(2018, VersionTool.IndexToYear(18));
        Assert.Equal(2025, VersionTool.IndexToYear(25));
    }

    [Fact]
    public void IndexToYear_InvalidIndex_ReturnsNegativeOne()
    {
        Assert.Equal(-1, VersionTool.IndexToYear(-1));
        Assert.Equal(-1, VersionTool.IndexToYear(100));
    }

    [Fact]
    public void YearToIndex_ValidYear_ReturnsCorrectIndex()
    {
        Assert.Equal(0, VersionTool.YearToIndex(2000));
        Assert.Equal(1, VersionTool.YearToIndex(2001));
        Assert.Equal(4, VersionTool.YearToIndex(2004));
        Assert.Equal(7, VersionTool.YearToIndex(2007));
        Assert.Equal(10, VersionTool.YearToIndex(2010));
        Assert.Equal(13, VersionTool.YearToIndex(2013));
        Assert.Equal(18, VersionTool.YearToIndex(2018));
        Assert.Equal(25, VersionTool.YearToIndex(2025));
    }

    [Fact]
    public void YearToIndex_InvalidYear_ReturnsBitwiseNot()
    {
        Assert.True(VersionTool.YearToIndex(1999) < 0);
        Assert.True(VersionTool.YearToIndex(2100) < 0);
    }

    [Fact]
    public void AcadVersAndDwgVers_ArraysHaveSameLength()
    {
        Assert.Equal(VersionTool.AcadVers.Length, VersionTool.DwgVers.Length);
    }

    [Fact]
    public void AcadVers_ArraysAreSorted()
    {
        for (int i = 1; i < VersionTool.AcadVers.Length; i++)
        {
            Assert.True(VersionTool.AcadVers[i - 1] <= VersionTool.AcadVers[i]);
        }
    }

    [Fact]
    public void DwgVers_KnownVersions_ReturnCorrectValues()
    {
        int index2000 = VersionTool.YearToIndex(2000);
        int index2004 = VersionTool.YearToIndex(2004);
        int index2007 = VersionTool.YearToIndex(2007);
        int index2010 = VersionTool.YearToIndex(2010);
        int index2013 = VersionTool.YearToIndex(2013);

        Assert.Equal(1015, VersionTool.DwgVers[index2000]);
        Assert.Equal(1018, VersionTool.DwgVers[index2004]);
        Assert.Equal(1021, VersionTool.DwgVers[index2007]);
        Assert.Equal(1024, VersionTool.DwgVers[index2010]);
        Assert.Equal(1027, VersionTool.DwgVers[index2013]);
    }

    [Fact]
    public void AcadVerOR_FixedPointNumber_HasValidFormat()
    {
        int major = VersionTool.AcadVerOR >> 16;
        int minor = VersionTool.AcadVerOR & 0xFFFF;

        Assert.True(major >= 15);
        Assert.True(minor >= 0);
    }

    [Fact]
    public void IndexAndYearConversion_RoundTrip_WorksCorrectly()
    {
        for (int year = 2000; year <= 2025; year++)
        {
            int index = VersionTool.YearToIndex(year);
            if (index >= 0)
            {
                int convertedYear = VersionTool.IndexToYear(index);
                Assert.Equal(year, convertedYear);
            }
        }
    }
}
