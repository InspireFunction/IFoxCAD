using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.Server;

/// <summary>
/// CAD连接信息
/// </summary>
public class CadConnectionInfo
{
    /// <summary>
    /// CAD实例编号（从1开始）
    /// </summary>
    public int InstanceId { get; set; }

    /// <summary>
    /// 进程ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// 进程名称
    /// </summary>
    public string ProcessName { get; set; } = "";

    /// <summary>
    /// 可执行文件路径
    /// </summary>
    public string? ExePath { get; set; }

    /// <summary>
    /// 管道客户端
    /// </summary>
    public NamedPipeClient? PipeClient { get; set; }

    /// <summary>
    /// 看门狗
    /// </summary>
    public Watchdog? Watchdog { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool IsConnected => PipeClient?.IsConnected == true;

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedTime { get; set; }
}

/// <summary>
/// CAD连接管理器
/// 管理多个CAD实例的连接
/// </summary>
public class CadConnectionManager : IDisposable
{
    // 已知的CAD进程名称
    private static readonly string[] KnownCadProcessNames = new[]
    {
        "acad",      // AutoCAD
        "gcad",      // GstarCAD
        "zwcad",     // ZWCAD
        "bricscad",  // BricsCAD
    };

    // 实例编号分配器
    private int _nextInstanceId = 1;

    // 连接集合：PID -> 连接信息
    private readonly ConcurrentDictionary<int, CadConnectionInfo> _connections = new();

    // 连接变化事件
    public event Action<CadConnectionInfo>? OnConnected;
    public event Action<CadConnectionInfo>? OnDisconnected;

    /// <summary>
    /// 获取所有连接
    /// </summary>
    public IReadOnlyCollection<CadConnectionInfo> GetAllConnections()
    {
        return _connections.Values.ToList();
    }

    /// <summary>
    /// 根据实例ID获取连接
    /// </summary>
    public CadConnectionInfo? GetConnectionByInstanceId(int instanceId)
    {
        return _connections.Values.FirstOrDefault(c => c.InstanceId == instanceId);
    }

    /// <summary>
    /// 根据PID获取连接
    /// </summary>
    public CadConnectionInfo? GetConnectionByPid(int pid)
    {
        _connections.TryGetValue(pid, out var connection);
        return connection;
    }

    /// <summary>
    /// 检测所有CAD进程并尝试连接
    /// </summary>
    public async Task DetectAndConnectAllAsync(CancellationToken ct)
    {
        var cadProcesses = FindAllCadProcesses();

        foreach (var process in cadProcesses)
        {
            if (ct.IsCancellationRequested) break;

            // 已连接的跳过
            if (_connections.ContainsKey(process.Id))
            {
                var existing = _connections[process.Id];
                if (existing.IsConnected)
                {
                    continue;
                }
                // 断开的移除旧连接
                _connections.TryRemove(process.Id, out _);
            }

            // 尝试连接
            await TryConnectAsync(process, ct);
        }
    }

    /// <summary>
    /// 查找所有CAD进程
    /// </summary>
    private List<Process> FindAllCadProcesses()
    {
        var result = new List<Process>();

        try
        {
            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses)
            {
                try
                {
                    var processName = process.ProcessName.ToLowerInvariant();

                    // 检查是否是已知的CAD进程
                    if (KnownCadProcessNames.Any(name => processName.Contains(name)))
                    {
                        result.Add(process);
                    }
                }
                catch
                {
                    // 忽略无法访问的进程
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 查找CAD进程失败: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 尝试连接指定进程
    /// </summary>
    private async Task TryConnectAsync(Process process, CancellationToken ct)
    {
        try
        {
            string? exePath = null;
            try
            {
                exePath = process.MainModule?.FileName;
            }
            catch { }

            // 管道名称格式：MCP_CAD_PIPE_{pid}
            var pipeName = $"MCP_CAD_PIPE_{process.Id}";

            Console.Error.WriteLine($"[信息] 检测到CAD进程: {process.ProcessName} (PID: {process.Id})");
            Console.Error.WriteLine($"[信息] 尝试连接管道: {pipeName}");

            var pipeClient = new NamedPipeClient(pipeName);
            bool connected = await pipeClient.ConnectAsync(5, ct);

            if (connected)
            {
                var connectionInfo = new CadConnectionInfo
                {
                    InstanceId = Interlocked.Increment(ref _nextInstanceId) - 1,
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    ExePath = exePath,
                    PipeClient = pipeClient,
                    ConnectedTime = DateTime.Now
                };

                // 创建并启动看门狗
                var watchdogConfig = new WatchdogConfig
                {
                    TimeoutStage1 = 5000,
                    TimeoutStage2 = 5000,
                    EscRetryCount = 3
                };

                //var watchdog = new Watchdog(pipeClient, watchdogConfig, process.Id);
                //watchdog.OnCadProcessExited += () => {
                //    Console.Error.WriteLine($"[信息] 看门狗检测到CAD实例 #{connectionInfo.InstanceId} 进程已退出");
                //    HandleDisconnection(connectionInfo);
                //};
                //await watchdog.StartAsync(ct);
                //connectionInfo.Watchdog = watchdog;

                _connections[process.Id] = connectionInfo;

                Console.Error.WriteLine($"[信息] CAD实例 #{connectionInfo.InstanceId} 已连接 (PID: {process.Id})");

                OnConnected?.Invoke(connectionInfo);

                // 启动监控任务
                _ = MonitorConnectionAsync(connectionInfo, ct);
            }
            else
            {
                Console.Error.WriteLine($"[警告] 无法连接到CAD进程 {process.Id}，请确保已加载MCP插件");
                pipeClient.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 连接CAD进程 {process.Id} 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 监控连接状态
    /// </summary>
    private async Task MonitorConnectionAsync(CadConnectionInfo connection, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && connection.IsConnected)
            {
                await Task.Delay(1000, ct);
            }

            // 连接断开（看门狗可能已经处理了）
            if (_connections.ContainsKey(connection.ProcessId))
            {
                HandleDisconnection(connection);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 监控CAD实例 #{connection.InstanceId} 时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理连接断开
    /// </summary>
    private void HandleDisconnection(CadConnectionInfo connection)
    {
        if (_connections.TryRemove(connection.ProcessId, out _))
        {
            connection.Watchdog?.Dispose();
            connection.PipeClient?.Dispose();

            Console.Error.WriteLine($"[信息] CAD实例 #{connection.InstanceId} 已断开 (PID: {connection.ProcessId})");

            OnDisconnected?.Invoke(connection);
        }
    }

    /// <summary>
    /// 清理断开的连接
    /// </summary>
    public void CleanupDisconnected()
    {
        var disconnectedPids = _connections
            .Where(kv => !kv.Value.IsConnected)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var pid in disconnectedPids)
        {
            if (_connections.TryRemove(pid, out var connection))
            {
                connection?.Watchdog?.Dispose();
                connection?.PipeClient?.Dispose();
                Console.Error.WriteLine($"[信息] 清理断开的CAD连接 (PID: {pid})");
            }
        }
    }

    public void Dispose()
    {
        foreach (var connection in _connections.Values)
        {
            connection.Watchdog?.Dispose();
            connection.PipeClient?.Dispose();
        }
        _connections.Clear();
    }
}
