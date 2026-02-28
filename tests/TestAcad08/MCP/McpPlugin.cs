namespace TestAcad08.MCP;

/// <summary>
/// MCP插件主类
/// 使用IFox的IFoxInitialize特性进行初始化
/// </summary>
public static class McpPlugin
{
    private static bool _isInitialized = false;
    private static NamedPipeServer? _pipeServer;

    /// <summary>
    /// 约定的管道名称
    /// </summary>
    public const string DefaultPipeName = "MCP_CAD_PIPE_{pid}";

    /// <summary>
    /// 获取当前活动的命令处理器
    /// </summary>
    public static CadCommandHandler? CommandHandler { get; private set; }

    /// <summary>
    /// 获取命令执行器
    /// </summary>
    public static CommandExecutor? CommandExecutor { get; private set; }

    /// <summary>
    /// 获取PGP解析器
    /// </summary>
    public static PgpParser? PgpParser { get; private set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static string CreateSuccessResponse(object id, object payload)
    {
        var response = new JsonRpcResponse { id = id, result = payload };
        return IFoxCAD.Cad.MyJson.SerializeObject(response, JsonRpcProtocol.JsonSettings);
    }

    /// <summary>
    /// 创建错误响应
    /// </summary>
    public static string CreateErrorResponse(object id, string code, string message)
    {
        JsonRpcErrorResponse errorResponse;
        if (!int.TryParse(code, out int codeNum))
        {
            codeNum = -32000;
            message = $"{code}: {message}";
        }
        errorResponse = new JsonRpcErrorResponse { id = id, error = new JsonRpcError { code = codeNum, message = message } };
        return IFoxCAD.Cad.MyJson.SerializeObject(errorResponse, JsonRpcProtocol.JsonSettings);
    }

    /// <summary>
    /// 初始化MCP插件
    /// 在首次文档开启后执行
    /// </summary>
    [IFoxInitialize(Sequence.StartOnce)]
    public static void Initialize(Document doc)
    {
        try
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            // 初始化公共依赖
            CommandExecutor = new CommandExecutor();
            PgpParser = new PgpParser();

            // 初始化命令处理器
            CommandHandler = new CadCommandHandler();

            // 注册所有命令
            CommandRegistry.Initialize();

            string pipeName = DefaultPipeName.Replace("{pid}", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
            Env.Printl($"[MCP] 准备创建管道: {pipeName}");

            _pipeServer = new NamedPipeServer(pipeName);
            Env.Printl("[MCP] NamedPipeServer已创建，准备启动...");

            _pipeServer.Start();
            Env.Printl($"[MCP] 插件初始化完成, 管道: {pipeName}, PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
        }
        catch (Exception ex)
        {
            Env.Printl($"[MCP] 初始化错误: {ex.GetType().Name}: {ex.Message}");
            foreach (var line in (ex.StackTrace ?? "").Split('\n').Take(3))
                Env.Printl($"  {line.Trim()}");
        }
    }

    /// <summary>
    /// 终止MCP插件
    /// </summary>
    [IFoxInitialize(Sequence.ProcessLast)]
    public static void Terminate()
    {
        try
        {
            _pipeServer?.Dispose();
            _pipeServer = null;

            // 修复说明：清理注册表状态，解决以下问题：
            // 1. CommandHandler 被置空后，注册表仍持有对旧实例的引用
            // 2. ToolRegistry 缓存的工具定义与实际不一致
            // 3. 重新加载插件时旧数据残留导致潜在问题
            CommandRegistry.Reset();
            ToolRegistry.Reset();

            CommandHandler = null;
            CommandExecutor = null;
            PgpParser = null;
            _isInitialized = false;
            Env.Printl("[MCP] 插件已终止");
        }
        catch { }
    }
}
