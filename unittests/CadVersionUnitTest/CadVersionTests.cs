namespace CadVersionUnitTests;

public class CadVersionTests
{
    [Fact]
    public void CadVersion_Properties_SetAndGet_WorkCorrectly()
    {
        var version = new CadVersion
        {
            Major = 24,
            Minor = 1,
            ProductName = "AutoCAD 2024",
            ProductRootKey = @"SOFTWARE\Autodesk\AutoCAD\R24.1"
        };

        Assert.Equal(24, version.Major);
        Assert.Equal(1, version.Minor);
        Assert.Equal("AutoCAD 2024", version.ProductName);
        Assert.Equal(@"SOFTWARE\Autodesk\AutoCAD\R24.1", version.ProductRootKey);
    }

    [Fact]
    public void CadVersion_ProgId_ReturnsCorrectValue()
    {
        var version1 = new CadVersion { Major = 24, Minor = 1 };
        var version2 = new CadVersion { Major = 15, Minor = 0 };
        var version3 = new CadVersion { Major = 25, Minor = 0 };

        Assert.Equal(24.1, version1.ProgId, 1);
        Assert.Equal(15.0, version2.ProgId, 1);
        Assert.Equal(25.0, version3.ProgId, 1);
    }

    [Fact]
    public void CadVersion_ToString_ReturnsCorrectFormat()
    {
        var version = new CadVersion
        {
            Major = 24,
            Minor = 1,
            ProductName = "AutoCAD 2024",
            ProductRootKey = @"SOFTWARE\Autodesk\AutoCAD\R24.1"
        };

        string result = version.ToString();

        Assert.Contains("名称:AutoCAD 2024", result);
        Assert.Contains("版本号:24.1", result);
        Assert.Contains("注册表位置:SOFTWARE\\Autodesk\\AutoCAD\\R24.1", result);
    }

    [Fact]
    public void CadVersion_DefaultConstructor_PropertiesAreDefault()
    {
        var version = new CadVersion();

        Assert.Equal(0, version.Major);
        Assert.Equal(0, version.Minor);
        Assert.Null(version.ProductName);
        Assert.Null(version.ProductRootKey);
    }
}
