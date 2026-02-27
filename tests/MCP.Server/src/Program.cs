using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.Server;

class Program
{
    private static McpServer? _mcpServer;
    private static CadConnectionManager? _connectionManager;
    private static CancellationTokenSource? _cts;

    static async Task Main(string[] args)
    {
        Console.Error.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.Error.WriteLine("║              MCP CAD Server v2.1                           ║");
        Console.Error.WriteLine("║              AutoCAD MCP 集成服务 (多实例支持)              ║");
        Console.Error.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.Error.WriteLine();

        _cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) => {
            e.Cancel = true;
            _cts.Cancel();
        };

        try
        {
            _connectionManager = new CadConnectionManager();

            Console.Error.WriteLine("[信息] MCP 服务已启动");
            Console.Error.WriteLine("[信息] 正在检测 AutoCAD 进程...");
            Console.Error.WriteLine();

            // 启动后台连接检测任务（持续运行）
            var connectionTask = _connectionManager.StartBackgroundDetectionAsync(_cts.Token);

            // 等待至少一个CAD连接
            await WaitForFirstConnectionAsync(_cts.Token);

            // 启动MCP服务器
            _mcpServer = new McpServer(_connectionManager);
            await _mcpServer.StartAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("\n[信息] 正在关闭...");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] {ex.Message}");
            Console.Error.WriteLine($"[错误] {ex.StackTrace}");
        }
        finally
        {
            _mcpServer?.Dispose();
            _connectionManager?.Dispose();
            _cts?.Dispose();
        }
    }

    private static async Task WaitForFirstConnectionAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var connections = _connectionManager!.GetAllConnections();
            if (connections.Count > 0)
            {
                Console.Error.WriteLine($"[信息] 已连接 {connections.Count} 个CAD实例");
                return;
            }

            Console.Error.WriteLine("[信息] 等待CAD连接...");
            await Task.Delay(2000, ct);
        }
    }
}
