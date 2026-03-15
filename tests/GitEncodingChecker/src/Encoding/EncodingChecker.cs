using System.Text;

/// <summary>
/// 检查文件编码的核心逻辑
/// </summary>
public static class EncodingChecker
{

    /// <summary>
    /// 检测文件编码
    /// </summary>
    public static string DetectEncoding(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            if (bytes.Length == 0) return AppConfig.EncodingNames.Empty;

            // 检查 BOM
            if (bytes.Length >= 3 && bytes[0] == AppConfig.Utf8Bom[0] && bytes[1] == AppConfig.Utf8Bom[1] && bytes[2] == AppConfig.Utf8Bom[2])
                return AppConfig.EncodingNames.Utf8Bom;

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return AppConfig.EncodingNames.Utf16Le;

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return AppConfig.EncodingNames.Utf16Be;

            // 检查是否全是 ASCII
            bool isAscii = bytes.All(b => b < 128);
            if (isAscii) return AppConfig.EncodingNames.Ascii;

            // 尝试 UTF-8 解码
            try
            {
                Encoding.UTF8.GetString(bytes);
                return AppConfig.EncodingNames.Utf8;
            }
            catch { }

            // 检测中文编码
            return DetectChineseEncoding(bytes);
        }
        catch
        {
            return AppConfig.EncodingNames.Error;
        }
    }

    /// <summary>
    /// 检测行尾类型
    /// </summary>
    public static string DetectLineEnding(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            bool hasCrlf = false;
            bool hasLf = false;

            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
                {
                    hasCrlf = true;
                    i++;
                }
                else if (bytes[i] == 0x0A)
                {
                    hasLf = true;
                }
            }

            // 检查最后一个字符
            if (bytes.Length > 0 && bytes[bytes.Length - 1] == 0x0A)
            {
                if (bytes.Length < 2 || bytes[bytes.Length - 2] != 0x0D)
                {
                    hasLf = true;
                }
            }

            if (hasCrlf && hasLf) return AppConfig.LineEndingNames.Mixed;
            if (hasCrlf) return AppConfig.LineEndingNames.Crlf;
            if (hasLf) return AppConfig.LineEndingNames.Lf;
            return AppConfig.LineEndingNames.None;
        }
        catch
        {
            return AppConfig.LineEndingNames.Error;
        }
    }

    /// <summary>
    /// 检查是否有空白行（只包含空格/制表符的行）
    /// </summary>
    public static bool HasBlankLines(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            return lines.Any(line => line.Length > 0 && string.IsNullOrWhiteSpace(line));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 修复文件编码为 UTF-8 无 BOM
    /// </summary>
    public static bool FixEncoding(string filePath, out string message)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            if (bytes.Length == 0)
            {
                message = "文件为空";
                return false;
            }

            string content;
            var encoding = DetectEncoding(filePath);

            switch (encoding)
            {
                case AppConfig.EncodingNames.Utf8Bom:
                    // 去掉 BOM
                    content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    break;

                case AppConfig.EncodingNames.Utf16Le:
                    content = Encoding.Unicode.GetString(bytes);
                    break;

                case AppConfig.EncodingNames.Utf16Be:
                    content = Encoding.BigEndianUnicode.GetString(bytes);
                    break;

                case AppConfig.EncodingNames.Utf8:
                case AppConfig.EncodingNames.Ascii:
                    message = "无需修复";
                    return false;

                default:
                    // 尝试用默认编码读取
                    content = File.ReadAllText(filePath);
                    break;
            }

            // 写入 UTF-8 无 BOM
            File.WriteAllText(filePath, content, new UTF8Encoding(false));
            message = $"已转换为 UTF-8 无 BOM (原编码: {encoding})";
            return true;
        }
        catch (Exception ex)
        {
            message = $"修复失败: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 修复行尾
    /// </summary>
    public static bool FixLineEnding(string filePath, string targetEol, out string message)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var currentEol = DetectLineEnding(filePath);

            if (currentEol == targetEol || currentEol == AppConfig.LineEndingNames.None)
            {
                message = "行尾无需修复";
                return false;
            }

            // 统一换行符
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");

            if (targetEol == AppConfig.LineEndingNames.Crlf)
            {
                content = content.Replace("\n", "\r\n");
            }

            File.WriteAllText(filePath, content, new UTF8Encoding(false));
            message = $"行尾已转换为 {targetEol}";
            return true;
        }
        catch (Exception ex)
        {
            message = $"修复失败: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 移除空白行
    /// </summary>
    public static bool RemoveBlankLines(string filePath, out string message)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            var hasBlank = lines.Any(line => line.Length > 0 && string.IsNullOrWhiteSpace(line));

            if (!hasBlank)
            {
                message = "没有空白行需要移除";
                return false;
            }

            var result = lines.Select(line =>
                line.Length > 0 && string.IsNullOrWhiteSpace(line) ? "" : line
            ).ToList();

            File.WriteAllLines(filePath, result, new UTF8Encoding(false));
            message = "已移除空白行";
            return true;
        }
        catch (Exception ex)
        {
            message = $"修复失败: {ex.Message}";
            return false;
        }
    }

    #region 私有方法

    private static string DetectChineseEncoding(byte[] bytes)
    {
        // GBK 检测
        if (IsGbk(bytes)) return "GBK";

        // GB2312 检测
        if (IsGb2312(bytes)) return "GB2312";

        // Big5 检测
        if (IsBig5(bytes)) return "Big5";

        return "Unknown";
    }

    private static bool IsGbk(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] > 127)
            {
                if (i + 1 >= bytes.Length) return false;
                byte high = bytes[i];
                byte low = bytes[i + 1];
                if (high < 0x81 || high > 0xFE || low < 0x40 || low > 0xFE)
                    return false;
                i++;
            }
        }
        return true;
    }

    private static bool IsGb2312(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] > 127)
            {
                if (i + 1 >= bytes.Length) return false;
                byte high = bytes[i];
                byte low = bytes[i + 1];
                if (high < 0xA1 || high > 0xF7 || low < 0xA1 || low > 0xFE)
                    return false;
                i++;
            }
        }
        return true;
    }

    private static bool IsBig5(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] > 127)
            {
                if (i + 1 >= bytes.Length) return false;
                byte high = bytes[i];
                byte low = bytes[i + 1];
                if (high < 0xA1 || high > 0xF9)
                    return false;
                if ((low < 0x40 || low > 0x7E) && (low < 0xA1 || low > 0xFE))
                    return false;
                i++;
            }
        }
        return true;
    }

    #endregion
}
