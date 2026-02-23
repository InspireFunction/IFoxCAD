using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.Server
{
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

                var cadProcess = await StartCadProcessAsync(config.Cad.ExePath, config.Cad.StartupArgs);
                if (cadProcess == null)
                {
                    Console.Error.WriteLine("[错误] 启动 CAD 进程失败");
                    return;
                }

                Console.Error.WriteLine($"[信息] CAD 进程已启动 (PID: {cadProcess.Id})");

                await Task.Delay(3000, _cts.Token);

                string pipeName = config.Cad.PipeName.Replace("{pid}", cadProcess.Id.ToString());

                var pipeClient = new NamedPipeClient(pipeName);

                bool connected = await pipeClient.ConnectAsync(30, _cts.Token);
                if (!connected)
                {
                    Console.Error.WriteLine("[错误] 无法连接到 CAD 进程");
                    Console.Error.WriteLine("请检查 AutoCAD 是否已加载 MCP CAD 插件");
                    return;
                }

                Console.Error.WriteLine("[信息] 已连接到 CAD 进程");

                _mcpServer = new McpServer(pipeClient, config);
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
                _cts?.Dispose();
            }
        }

        /// <summary>
        /// 启动CAD进程
        /// </summary>
        private static async Task<Process?> StartCadProcessAsync(string exePath, string args)
        {
            try
            {
                if (!File.Exists(exePath))
                {
                    Console.Error.WriteLine($"[错误] CAD 可执行文件不存在: {exePath}");
                    return null;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                var process = Process.Start(psi);
                if (process == null)
                {
                    Console.Error.WriteLine("[错误] 启动 CAD 进程失败");
                    return null;
                }

                await Task.Delay(1000);

                return process;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[错误] 启动 CAD 进程出错: {ex.Message}");
                return null;
            }
        }
    }
}
