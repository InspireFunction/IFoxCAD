using Xunit;
using System.Text;

/// <summary>
/// 编码检查功能的单元测试
/// </summary>
public class EncodingCheckerTests : IDisposable
{
    private readonly string _testDir;

    public EncodingCheckerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"EncodingCheckerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    private string CreateTestFile(string name, byte[] content)
    {
        var path = Path.Combine(_testDir, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    private string CreateTestFile(string name, string content, Encoding encoding)
    {
        var path = Path.Combine(_testDir, name);
        File.WriteAllText(path, content, encoding);
        return path;
    }

    [Fact]
    public void DetectEncoding_ShouldReturnUtf8Bom_WhenFileHasBom()
    {
        // Arrange
        var content = new byte[] { 0xEF, 0xBB, 0xBF, (byte)'h', (byte)'i' };
        var path = CreateTestFile("utf8bom.txt", content);

        // Act
        var encoding = EncodingChecker.DetectEncoding(path);

        // Assert
        Assert.Equal("UTF8-BOM", encoding);
    }

    [Fact]
    public void DetectEncoding_ShouldReturnUtf8_WhenFileIsUtf8WithoutBom()
    {
        // Arrange
        var path = CreateTestFile("utf8.txt", "Hello 世界", new UTF8Encoding(false));

        // Act
        var encoding = EncodingChecker.DetectEncoding(path);

        // Assert
        Assert.Equal("UTF-8", encoding);
    }

    [Fact]
    public void DetectEncoding_ShouldReturnAscii_WhenFileIsAscii()
    {
        // Arrange
        var path = CreateTestFile("ascii.txt", "Hello World", Encoding.ASCII);

        // Act
        var encoding = EncodingChecker.DetectEncoding(path);

        // Assert
        Assert.Equal("ASCII", encoding);
    }

    [Fact]
    public void DetectEncoding_ShouldReturnUtf16Le_WhenFileIsUtf16Le()
    {
        // Arrange
        var content = new byte[] { 0xFF, 0xFE, (byte)'A', 0x00 };
        var path = CreateTestFile("utf16le.txt", content);

        // Act
        var encoding = EncodingChecker.DetectEncoding(path);

        // Assert
        Assert.Equal("UTF16-LE", encoding);
    }

    [Fact]
    public void DetectEncoding_ShouldReturnEmpty_WhenFileIsEmpty()
    {
        // Arrange
        var path = CreateTestFile("empty.txt", Array.Empty<byte>());

        // Act
        var encoding = EncodingChecker.DetectEncoding(path);

        // Assert
        Assert.Equal("Empty", encoding);
    }

    [Fact]
    public void DetectLineEnding_ShouldReturnCrlf_WhenFileUsesCrlf()
    {
        // Arrange
        var path = CreateTestFile("crlf.txt", "line1\r\nline2\r\n", Encoding.UTF8);

        // Act
        var lineEnding = EncodingChecker.DetectLineEnding(path);

        // Assert
        Assert.Equal("CRLF", lineEnding);
    }

    [Fact]
    public void DetectLineEnding_ShouldReturnLf_WhenFileUsesLf()
    {
        // Arrange
        var path = CreateTestFile("lf.txt", "line1\nline2\n", Encoding.UTF8);

        // Act
        var lineEnding = EncodingChecker.DetectLineEnding(path);

        // Assert
        Assert.Equal("LF", lineEnding);
    }

    [Fact]
    public void DetectLineEnding_ShouldReturnMixed_WhenFileUsesBoth()
    {
        // Arrange
        var path = CreateTestFile("mixed.txt", "line1\r\nline2\nline3\r\n", Encoding.UTF8);

        // Act
        var lineEnding = EncodingChecker.DetectLineEnding(path);

        // Assert
        Assert.Equal("Mixed", lineEnding);
    }

    [Fact]
    public void HasBlankLines_ShouldReturnTrue_WhenFileHasBlankLines()
    {
        // Arrange
        var path = CreateTestFile("blank.txt", "line1\n   \nline3\n", Encoding.UTF8);

        // Act
        var hasBlank = EncodingChecker.HasBlankLines(path);

        // Assert
        Assert.True(hasBlank);
    }

    [Fact]
    public void HasBlankLines_ShouldReturnFalse_WhenFileHasNoBlankLines()
    {
        // Arrange
        var path = CreateTestFile("noblank.txt", "line1\n\nline3\n", Encoding.UTF8);

        // Act
        var hasBlank = EncodingChecker.HasBlankLines(path);

        // Assert
        Assert.False(hasBlank); // 空行不算空白行
    }

    [Fact]
    public void FixEncoding_ShouldRemoveBom_WhenFileHasBom()
    {
        // Arrange
        var content = new byte[] { 0xEF, 0xBB, 0xBF, (byte)'h', (byte)'i' };
        var path = CreateTestFile("fixbom.txt", content);

        // Act
        var result = EncodingChecker.FixEncoding(path, out var message);

        // Assert
        Assert.True(result);
        Assert.Contains("UTF-8", message);

        // 验证 BOM 已被移除
        var bytes = File.ReadAllBytes(path);
        Assert.DoesNotContain((byte)0xEF, bytes);
    }

    [Fact]
    public void FixEncoding_ShouldReturnFalse_WhenNoFixNeeded()
    {
        // Arrange
        var path = CreateTestFile("nofix.txt", "Hello", new UTF8Encoding(false));

        // Act
        var result = EncodingChecker.FixEncoding(path, out var message);

        // Assert
        Assert.False(result);
        Assert.Contains("无需修复", message);
    }

    [Fact]
    public void FixLineEnding_ShouldConvertToLf()
    {
        // Arrange
        var path = CreateTestFile("fixeol.txt", "line1\r\nline2\r\n", Encoding.UTF8);

        // Act
        var result = EncodingChecker.FixLineEnding(path, "LF", out var message);

        // Assert
        Assert.True(result);

        var content = File.ReadAllText(path);
        Assert.DoesNotContain("\r\n", content);
        Assert.Contains("\n", content);
    }

    [Fact]
    public void RemoveBlankLines_ShouldRemoveWhitespaceLines()
    {
        // Arrange
        var path = CreateTestFile("removeblank.txt", "line1\n   \nline3\n", new UTF8Encoding(false));

        // Act
        var result = EncodingChecker.RemoveBlankLines(path, out var message);

        // Assert
        Assert.True(result);

        var lines = File.ReadAllLines(path);
        Assert.DoesNotContain(lines, line => line.Length > 0 && string.IsNullOrWhiteSpace(line));
    }
}
