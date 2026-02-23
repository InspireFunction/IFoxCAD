using System.Collections.Generic;

namespace TestAcad08.MCP;

/// <summary>
/// CAD命令处理器
/// 包含所有MCP命令的实现
/// 注意: request.Id 已在反射层验证,命令方法内无需再检查
/// </summary>
public class CadCommandHandler
{
    // ========== CAD信息类命令 ==========

    [McpCommand(nameof(Mcp_cad_get_info), "获取CAD进程信息，包括PID、版本、年份等")]
    public string Mcp_cad_get_info(PipeRequest request)
    {
        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_get_info)} 请求");
        var doc = Acap.DocumentManager.MdiActiveDocument;
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var info = new
        {
            pid = process.Id,
            version = Acap.Version.ToString(),
            document_name = doc?.Name ?? "No document",
            serverInfo = new
            {
                name = $"mcp-cad-server-{Acap.Version.Major}",
                version = Acap.Version.ToString()
            }
        };

        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_get_info)} 请求完成");
        return McpPlugin.CreateSuccessResponse(request.Id!, info);
    }

    [McpCommand(nameof(Mcp_cad_send_command), "发送命令到CAD执行。注意：如果命令有-前缀版本，请优先使用-前缀版本以避免交互式提示")]
    public string Mcp_cad_send_command(PipeRequest request,
        [McpParam("CAD命令，例如 '-circle 0,0,0 500'")] string command,
        [McpParam("超时时间（秒）", DefaultValue = 30)] int timeout)
    {
        if (string.IsNullOrEmpty(command))
        {
            return McpPlugin.CreateErrorResponse(request.Id!, "INVALID_COMMAND", "Command is empty");
        }

        var pgpParser = McpPlugin.PgpParser;
        if (pgpParser != null && pgpParser.HasDashVersion(command))
        {
            string dashCmd = "-" + command.Split(' ')[0];
            return McpPlugin.CreateErrorResponse(request.Id!, "DASH_VERSION_AVAILABLE",
                $"Command '{command}' has a dash-prefixed version '{dashCmd}'. " +
                "在使用非交互式执行时，请使用带短横线（-）的版本.");
        }

        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_send_command)} 请求, 命令: {command}, 超时: {timeout}");
        var executor = McpPlugin.CommandExecutor;
        if (executor == null)
        {
            return McpPlugin.CreateErrorResponse(request.Id!, "NOT_INITIALIZED", "CommandExecutor not initialized");
        }
        var result = executor.Execute(command, timeout);
        return McpPlugin.CreateSuccessResponse(request.Id!, result);
    }

    [McpCommand(nameof(Mcp_cad_get_history), "获取CAD命令执行历史")]
    public string Mcp_cad_get_history(
        PipeRequest request,
        [McpParam("文档名称（可选，默认当前文档）", Required = false)] string? doc_name = null)
    {
        var history = HistoryManager.GetHistory(doc_name);
        return McpPlugin.CreateSuccessResponse(request.Id!, new { history });
    }

    [McpCommand(nameof(Mcp_cad_switch_document), "切换到指定的CAD文档")]
    public string Mcp_cad_switch_document(
        PipeRequest request,
        [McpParam("目标文档名称")] string doc_name)
    {
        if (string.IsNullOrEmpty(doc_name))
        {
            return McpPlugin.CreateErrorResponse(request.Id!, "INVALID_DOC_NAME", "Document name is empty");
        }

        bool success = DocumentManager.SwitchDocument(doc_name);
        return McpPlugin.CreateSuccessResponse(request.Id!, new { success });
    }

    [McpCommand(nameof(Mcp_cad_update_pgp), "重新加载PGP命令定义")]
    public string Mcp_cad_update_pgp(PipeRequest request)
    {
        var pgpParser = McpPlugin.PgpParser;
        if (pgpParser == null)
        {
            return McpPlugin.CreateErrorResponse(request.Id!, "NOT_INITIALIZED", "PgpParser not initialized");
        }
        pgpParser.Reload();
        return McpPlugin.CreateSuccessResponse(request.Id!, new { success = true, command_count = pgpParser.CommandCount });
    }

    [McpCommand(nameof(Mcp_cad_say_hello), "测试问候")]
    public string Mcp_cad_say_hello(PipeRequest request)
    {
        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_say_hello)} 请求");
        var greetings = new[]
        {
            "你好啊", "今天天气真不错", "欢迎使用CAD", "新年快乐",
            "工作顺利", "加油", "天气晴朗", "心情美好",
            "一天之计在于晨", "你好啊，朋友"
        };
        var random = new Random();
        var greeting = greetings[random.Next(greetings.Length)];

        Env.Printl($"[{nameof(Mcp_cad_say_hello)}] 随机问候: {greeting}");
        var response = McpPlugin.CreateSuccessResponse(request.Id!, new { message = greeting });
        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_say_hello)} 请求完成");
        return response;
    }

    [McpCommand(nameof(Mcp_cad_list_commands), "列出所有可用命令")]
    public string Mcp_cad_list_commands(PipeRequest request)
    {
        var commands = new List<object>();
        foreach (var name in CommandRegistry.GetCommandNames())
        {
            var desc = CommandRegistry.Descriptions.TryGetValue(name, out var d) ? d : "";
            commands.Add(new { name, description = desc });
        }
        return McpPlugin.CreateSuccessResponse(request.Id!, new { commands });
    }

    [McpCommand(nameof(Mcp_cad_get_tools), "获取MCP tools列表")]
    public string Mcp_cad_get_tools(PipeRequest request)
    {
        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_get_tools)} 请求");

        var tools = ToolRegistry.GetAllTools();
        var toolsWithSchema = tools.ConvertAll(t => new
        {
            name = t.Name,
            description = t.Description,
            inputSchema = t.ToInputSchema()
        });

        var response = McpPlugin.CreateSuccessResponse(request.Id!, new { tools = toolsWithSchema });
        Env.Printl($"[MCP] 处理 {nameof(Mcp_cad_get_tools)} 请求完成，返回 {tools.Count} 个工具");
        return response;
    }
}
