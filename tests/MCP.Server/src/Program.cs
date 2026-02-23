using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private static ConfigManager? _configManager;
    private static CancellationTokenSource? _cts;

    static async Task Main(string[] args)
    {
        Console.Error.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.Error.WriteLine("║              MCP CAD Server v1.0                           ║");
        Console.Error.WriteLine("║              AutoCAD MCP 集成服务                           ║");
        Console.Error.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.Error.WriteLine();

        _cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) => {
            e.Cancel = true;
            _cts.Cancel();
        };

        try
        {
            _configManager = new ConfigManager();
            if (!_configManager.Load())
            {
                Console.Error.WriteLine("[错误] 加载配置失败");
                Console.Error.WriteLine("请编辑 config.json 设置正确的 CAD 路径");
                return;
            }

            var config = _configManager.Config;

            Console.Error.WriteLine("[信息] MCP 服务已启动");
            Console.Error.WriteLine("[信息] 等待 AutoCAD 启动...");
            Console.Error.WriteLine();

            await WaitForCadAndConnectAsync(config);
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
            _cts?.Dispose();
        }
    }

    /// <summary>
    /// 等待CAD进程启动并连接
    /// </summary>
    private static async Task WaitForCadAndConnectAsync(Config? config)
    {
        if (_cts is null)
        {
            return;
        }

        while (!_cts.Token.IsCancellationRequested)
        {
            var cad = config?.Cad;
            if (cad is null)
                return;

            var exePath = cad.ExePath;
            var cadProcess = exePath != null ? FindCadProcess(exePath) : null;
            if (cadProcess != null)
            {
                Console.Error.WriteLine($"[信息] 检测到 CAD 进程 (PID: {cadProcess.Id})");

                var pipeName = cad.PipeName?.Replace("{pid}", cadProcess.Id.ToString());
                if (string.IsNullOrEmpty(pipeName))
                {
                    Console.Error.WriteLine("[错误] PipeName 配置为空");
                    continue;
                }
                var pipeClient = new NamedPipeClient(pipeName);

                Console.Error.WriteLine("[信息] 尝试连接 CAD 进程...");

                bool connected = await pipeClient.ConnectAsync(30, _cts.Token);
                if (connected && config != null)
                {
                    Console.Error.WriteLine("[信息] 已连接到 CAD 进程");
                    _mcpServer = new McpServer(pipeClient, config);
                    await _mcpServer.StartAsync(_cts.Token);
                    Console.Error.WriteLine("[信息] 连接已断开，等待 CAD 重新启动...");
                }
                else
                {
                    Console.Error.WriteLine("[错误] 无法连接到 CAD 进程");
                    Console.Error.WriteLine("请检查 AutoCAD 是否已加载 MCP CAD 插件");
                }
            }

            await Task.Delay(2000, _cts.Token);
        }
    }

    /// <summary>
    /// 查找CAD进程
    /// </summary>
    private static Process? FindCadProcess(string exePath)
    {
        try
        {
            var processes = Process.GetProcesses();
            var cadProcesses = processes.Where(p => {
                try
                {
                    return p.MainModule != null &&
                           !string.IsNullOrEmpty(p.MainModule.FileName) &&
                           p.MainModule.FileName.Equals(exePath, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }).ToList();

            return cadProcesses.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[警告] 查找 CAD 进程出错: {ex.Message}");
            return null;
        }
    }
}
