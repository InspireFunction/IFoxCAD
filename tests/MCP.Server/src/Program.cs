using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.Server;

/// <summary>
/// MCP Server 入口程序
/// </summary>
class Program
{
    private static McpServer? _mcpServer;
    private static CadConnectionManager? _connectionManager;
    private static CancellationTokenSource? _cts;

    static async Task Main(string[] args)
    {
        Console.Error.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.Error.WriteLine("║              MCP CAD Server v2.0                           ║");
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

            await WaitForCadAndConnectAsync(_cts.Token);
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

    /// <summary>
    /// 等待CAD进程启动并连接
    /// </summary>
    private static async Task WaitForCadAndConnectAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _connectionManager!.DetectAndConnectAllAsync(ct);

            var connections = _connectionManager.GetAllConnections();
            if (connections.Count > 0)
            {
                Console.Error.WriteLine($"[信息] 已连接 {connections.Count} 个CAD实例");

                _mcpServer = new McpServer(_connectionManager);
                await _mcpServer.StartAsync(ct);

                Console.Error.WriteLine("[信息] 连接已断开，等待 CAD 重新启动...");
            }

            await Task.Delay(2000, ct);
        }
    }
}
