using System.IO.Pipes;
using System.Text;
using IFoxCAD.Cad;

namespace MCP.Server;

/// <summary>
/// Named Pipe客户端
/// 用于与CAD插件通讯
/// </summary>
public class NamedPipeClient : IDisposable
{
    private readonly string _pipeName;
    private NamedPipeClientStream? _pipeClient;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public NamedPipeClient(string pipeName)
    {
        _pipeName = pipeName;
    }

    /// <summary>
    /// 连接到管道服务器
    /// </summary>
    public async Task<bool> ConnectAsync(int timeoutSeconds, CancellationToken ct)
    {
        try
        {
            // 使用同步模式，与CAD端保持一致
            _pipeClient = new NamedPipeClientStream(
                ".",
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.None,
                System.Security.Principal.TokenImpersonationLevel.None);

            await _pipeClient.ConnectAsync(timeoutSeconds * 1000, ct);

            if (_pipeClient.IsConnected)
            {
                await ReceiveConnectionConfirmationAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] 管道连接失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 接收连接确认消息
    /// </summary>
    private async Task ReceiveConnectionConfirmationAsync()
    {
        try
        {
            var buffer = new byte[4096];
            var responseBuilder = new StringBuilder();
            var pipeClient = _pipeClient;

            while (pipeClient != null)
            {
                int bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0) break;

                var chunk = new UTF8Encoding(false).GetString(buffer, 0, bytesRead);
                responseBuilder.Append(chunk);

                if (chunk?.Contains('\n') ?? false)
                    break;
            }

            var response = responseBuilder.ToString().Trim();
            Console.Error.WriteLine($"[INFO] Received confirmation: {response}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to receive confirmation: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送请求并等待响应
    /// </summary>
    public async Task<string?> SendRequestAsync(string request, int timeoutMs = 30000)
    {
        await _lock.WaitAsync();
        try
        {
            if (_pipeClient == null || !_pipeClient.IsConnected)
            {
                Console.Error.WriteLine($"[ERROR] SendRequest: 发送管道关闭请求");
                return null;
            }

            // 发送请求
            var requestWithNewline = request + "\n";
            var requestBytes = new UTF8Encoding(false).GetBytes(requestWithNewline);
            Console.Error.WriteLine($"[DEBUG] 发送请求: {requestWithNewline}");
            await _pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);
            await _pipeClient.FlushAsync();
            Console.Error.WriteLine("[DEBUG] 请求已发送，等待响应...");

            // 读取响应
            using var cts = new CancellationTokenSource(timeoutMs);
            var responseBuilder = new StringBuilder();
            var buffer = new byte[4096];
            int totalBytesRead = 0;

            while (!cts.IsCancellationRequested)
            {
                int bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (bytesRead == 0)
                {
                    Console.Error.WriteLine($"[DEBUG] 读取到 {totalBytesRead} 字节，连接关闭");
                    break;
                }

                totalBytesRead += bytesRead;
                var chunk = new UTF8Encoding(false).GetString(buffer, 0, bytesRead);
                responseBuilder.Append(chunk);
                //Console.Error.WriteLine($"[DEBUG] 收到数据块: {chunk}");

                // 检查是否收到完整消息（以换行符结尾）
                if (chunk.Contains('\n'))
                {
                    Console.Error.WriteLine("[DEBUG] 收到完整响应");
                    break;
                }
            }

            var response = responseBuilder.ToString().Trim();
            //Console.Error.WriteLine($"[DEBUG] 完整响应: {response}");
            return response;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine($"[ERROR] SendRequest: 超时 {timeoutMs}ms");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] SendRequest error: {ex.Message}");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 发送ESC命令
    /// </summary>
    public async Task<bool> SendEscapeAsync()
    {
        var request = new
        {
            id = -555,
            type = "send_escape",
            payload = new { }
        };

        var response = await SendRequestAsync(MyJson.SerializeObject(request), 5000);
        return response != null;
    }

    /// <summary>
    /// 检查连接状态
    /// </summary>
    public bool IsConnected => _pipeClient?.IsConnected ?? false;

    public void Dispose()
    {
        _lock?.Dispose();
        _pipeClient?.Close();
        _pipeClient?.Dispose();
    }
}
