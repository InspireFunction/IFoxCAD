namespace CadVersionUnitTests;

public class VersionToolTests
{
    [Fact]
    public void IndexToYear_AllValidIndices_ReturnCorrectYear()
    {
        for (int i = 0; i < VersionTool.AcadVers.Length; i++)
        {
            Assert.Equal(2000 + i, VersionTool.IndexToYear(i));
        }
    }

    [Fact]
    public void IndexToYear_InvalidIndex_ReturnsNegativeOne()
    {
        Assert.Equal(-1, VersionTool.IndexToYear(-1));
        Assert.Equal(-1, VersionTool.IndexToYear(VersionTool.AcadVers.Length));
        Assert.Equal(-1, VersionTool.IndexToYear(100));
    }

    [Fact]
    public void YearToIndex_AllValidYears_ReturnCorrectIndex()
    {
        for (int year = 2000; year < 2000 + VersionTool.AcadVers.Length; year++)
        {
            Assert.Equal(year - 2000, VersionTool.YearToIndex(year));
        }
    }

    [Fact]
    public void YearToIndex_InvalidYear_ReturnsNegative()
    {
        Assert.True(VersionTool.YearToIndex(1999) < 0);
        Assert.True(VersionTool.YearToIndex(2100) < 0);
    }

    [Fact]
    public void IndexAndYearConversion_RoundTrip_WorksCorrectly()
    {
        for (int year = 2000; year < 2000 + VersionTool.AcadVers.Length; year++)
        {
            int index = VersionTool.YearToIndex(year);
            Assert.Equal(year, VersionTool.IndexToYear(index));
        }
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
    public void AcadVerOR_FixedPointNumber_HasValidFormat()
    {
        int major = VersionTool.AcadVerOR >> 16;
        int minor = (VersionTool.AcadVerOR & 0xFFFF) / 6553;
        Assert.InRange(major, 15, 25);
        Assert.InRange(minor, 0, 9);
    }

    [Fact]
    public void FixedPoint_AllVersions_EncodeDecodeCorrectly()
    {
        for (int i = 0; i < VersionTool.AcadVers.Length; i++)
        {
            int fixedPoint = VersionTool.AcadVers[i];
            int major = fixedPoint >> 16;
            int minor = (fixedPoint & 0xFFFF) / 6553;
            int reencoded = (int)(((uint)major << 16) | (ushort)(minor * 6553));
            Assert.Equal(fixedPoint, reencoded);
            Assert.InRange(major, 15, 25);
            Assert.InRange(minor, 0, 9);
        }
    }
}
