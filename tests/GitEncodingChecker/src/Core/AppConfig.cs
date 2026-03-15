/// <summary>
/// 应用程序配置类 - 集中管理所有硬编码的常量
/// 后续可扩展为支持配置文件、环境变量等
/// </summary>
public static class AppConfig
{
    #region 文件扩展名配置

    /// <summary>
    /// 需要检查的文本文件扩展名列表
    /// </summary>
    public static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".slnx", ".csproj", ".json", ".xml", ".config",
        ".props", ".targets", ".md", ".txt", ".yaml", ".yml",
        ".toml", ".ini", ".cfg", ".conf", ".html", ".htm",
        ".css", ".js", ".ts", ".jsx", ".tsx", ".py", ".rb",
        ".go", ".rs", ".java", ".c", ".cpp", ".h", ".hpp",
        ".sh", ".bash", ".bat", ".cmd"
    };

    /// <summary>
    /// 需要跳过的文件扩展名列表（如PowerShell脚本）
    /// </summary>
    public static readonly HashSet<string> SkipExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ps1", ".psm1"
    };

    /// <summary>
    /// 跳过文件的提示原因
    /// </summary>
    public const string SkipReasonPowerShell = "PowerShell 脚本";

    #endregion

    #region 编码检测配置

    /// <summary>
    /// UTF-8 BOM 标记字节
    /// </summary>
    public static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

    /// <summary>
    /// 编码名称常量
    /// </summary>
    public static class EncodingNames
    {
        public const string Utf8Bom = "UTF8-BOM";
        public const string Utf8 = "UTF-8";
        public const string Ascii = "ASCII";
        public const string Utf16Le = "UTF16-LE";
        public const string Utf16Be = "UTF16-BE";
        public const string Empty = "Empty";
        public const string Error = "Error";
    }

    /// <summary>
    /// 行尾类型常量
    /// </summary>
    public static class LineEndingNames
    {
        public const string Crlf = "CRLF";
        public const string Lf = "LF";
        public const string Mixed = "Mixed";
        public const string None = "None";
        public const string Error = "Error";
    }

    /// <summary>
    /// 目标行尾类型（默认）
    /// </summary>
    public const string DefaultLineEnding = "LF";

    #endregion

    #region Git 路径配置

    /// <summary>
    /// 常见的 Git 安装位置，按优先级排序
    /// </summary>
    public static readonly string[] CommonGitPaths = new[]
    {
        // 标准安装路径（64位）
        @"C:\Program Files\Git\bin\git.exe",
        @"C:\Program Files\Git\cmd\git.exe",

        // 标准安装路径（32位）
        @"C:\Program Files (x86)\Git\bin\git.exe",
        @"C:\Program Files (x86)\Git\cmd\git.exe",

        // 用户级安装（通过官网安装程序选择"仅为我安装"）
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Git\bin\git.exe"),

        // 便携版常见位置
        @"C:\git\bin\git.exe",
        @"C:\tools\git\bin\git.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"git\bin\git.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"tools\git\bin\git.exe"),

        // 包管理器安装路径
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"scoop\shims\git.exe"),
        @"C:\ProgramData\chocolatey\bin\git.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".cargo\bin\git.exe"),
    };

    /// <summary>
    /// Git 环境变量名称
    /// </summary>
    public const string GitPathEnvironmentVariable = "GIT_PATH";

    /// <summary>
    /// Git 可执行文件默认名称
    /// </summary>
    public const string GitExecutableName = "git";

    #endregion

    #region 编辑器配置

    /// <summary>
    /// EditorConfig 文件名
    /// </summary>
    public const string EditorConfigFileName = ".editorconfig";

    /// <summary>
    /// EditorConfig 行尾配置键
    /// </summary>
    public const string EditorConfigEndOfLineKey = "end_of_line";

    #endregion

    #region 消息提示配置

    /// <summary>
    /// 检测通过的消息
    /// </summary>
    public const string MessageCheckPassed = "✓ 所有文件编码检查通过！";

    /// <summary>
    /// 检测失败的消息
    /// </summary>
    public const string MessageCheckFailed = "❌ 编码检查未通过！";

    /// <summary>
    /// 修复提示信息
    /// </summary>
    public static class FixHints
    {
        public const string FixCommand = "git ec-fix     # 自动修复编码问题";
        public const string CommitCommand = "git ecc -m \"msg\"  # 修复并提交";
    }

    #endregion
}
