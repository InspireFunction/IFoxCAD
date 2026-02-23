using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.Server;

/// <summary>
/// 看门狗 - 监控CAD状态并处理超时
/// </summary>
public class Watchdog : IDisposable
{
    private readonly NamedPipeClient _pipeClient;
    private readonly WatchdogConfig _config;
    private CancellationTokenSource? _cts;
    private Task? _watchTask;
    private DateTime _lastActivity;
    private WatchdogState _state;

    public Watchdog(NamedPipeClient pipeClient, WatchdogConfig config)
    {
        _pipeClient = pipeClient;
        _config = config;
        _lastActivity = DateTime.Now;
        _state = WatchdogState.Idle;
    }

    /// <summary>
    /// 启动看门狗
    /// </summary>
    public Task StartAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _watchTask = WatchLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        _lastActivity = DateTime.Now;
        _state = WatchdogState.Idle;
    }

    /// <summary>
    /// 标记命令开始执行
    /// </summary>
    public void CommandStarted()
    {
        _lastActivity = DateTime.Now;
        _state = WatchdogState.CommandRunning;
    }

    /// <summary>
    /// 标记命令完成
    /// </summary>
    public void CommandCompleted()
    {
        _state = WatchdogState.Idle;
    }

    /// <summary>
    /// 看门狗循环
    /// </summary>
    private async Task WatchLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, ct);

                if (_state != WatchdogState.CommandRunning)
                    continue;

                var elapsed = DateTime.Now - _lastActivity;

                switch (_state)
                {
                    case WatchdogState.CommandRunning:
                    if (elapsed.TotalMilliseconds > _config.TimeoutStage1)
                    {
                        await HandleStage1TimeoutAsync();
                    }
                    break;

                    case WatchdogState.Stage1EscSent:
                    if (elapsed.TotalMilliseconds > _config.TimeoutStage1 + _config.TimeoutStage2)
                    {
                        await HandleStage2TimeoutAsync();
                    }
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WARN] Watchdog error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 处理第一阶段超时 - 发送ESC
    /// </summary>
    private async Task HandleStage1TimeoutAsync()
    {
        Console.Error.WriteLine("[WARN] Command timeout (Stage 1) - Sending ESC...");

        for (int i = 0; i < _config.EscRetryCount; i++)
        {
            var sent = await _pipeClient.SendEscapeAsync();
            if (sent)
            {
                Console.Error.WriteLine($"[INFO] ESC sent (attempt {i + 1})");
                _state = WatchdogState.Stage1EscSent;
                _lastActivity = DateTime.Now;
                return;
            }

            await Task.Delay(500);
        }

        Console.Error.WriteLine("[ERROR] Failed to send ESC");
    }

    /// <summary>
    /// 处理第二阶段超时 - 通知用户
    /// </summary>
    private async Task HandleStage2TimeoutAsync()
    {
        Console.Error.WriteLine("[ERROR] Command timeout (Stage 2) - CAD may be frozen");
        Console.Error.WriteLine("[ERROR] Please check CAD process and decide whether to terminate it");

        // 这里可以通过某种机制通知LLM，让用户决定
        // 由于不能自动关闭（可能有未保存图纸），只记录状态
        _state = WatchdogState.Stage2UserNotified;

        // 重置状态，避免重复通知
        await Task.Delay(1000);
        _state = WatchdogState.Idle;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

/// <summary>
/// 看门狗状态
/// </summary>
public enum WatchdogState
{
    Idle,
    CommandRunning,
    Stage1EscSent,
    Stage2UserNotified
}
