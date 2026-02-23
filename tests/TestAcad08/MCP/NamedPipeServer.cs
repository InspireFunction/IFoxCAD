using System.IO.Pipes;
using System.Threading;

namespace TestAcad08.MCP;

/// <summary>
/// 命名管道服务器
/// 处理MCP客户端连接和请求
/// </summary>
public class NamedPipeServer : IDisposable
{
    private NamedPipeServerStream? _pipeServer;
    private ManualResetEvent? _stopEvent;
    private Thread? _listenerThread;
    private volatile bool _isRunning;
    private readonly string _pipeName;

    public NamedPipeServer(string pipeName)
    {
        _pipeName = pipeName;
    }

    public void Start()
    {
        if (_isRunning) return;

        _stopEvent = new ManualResetEvent(false);
        _isRunning = true;

        _listenerThread = new Thread(ListenLoop)
        {
            IsBackground = true,
            Name = "MCP_PipeListener"
        };
        _listenerThread.Start();

        Env.Printl($"[MCP] 管道服务器已启动: {_pipeName}");
    }

    public void Stop()
    {
        _isRunning = false;
        _stopEvent?.Set();
        _pipeServer?.Close();
        _pipeServer?.Dispose();
        _pipeServer = null;

        Env.Printl("[MCP] 管道服务器已停止");
    }

    private void ListenLoop()
    {
        while (_isRunning)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.None);

                _pipeServer.WaitForConnection();

                Env.Printl("[MCP] 连接上MCP服务了");

                HandleClient(_pipeServer);
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Env.Printl($"[MCP] 管道错误: {ex.Message}");
                }
            }
        }
    }

    private void HandleClient(NamedPipeServerStream pipe)
    {
        try
        {
            SendConnectionConfirmationRaw(pipe);

            byte[] buffer = new byte[4096];

            while (pipe.IsConnected && _isRunning)
            {
                StringBuilder messageBuilder = new();
                int bytesRead;

                do
                {
                    bytesRead = pipe.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(chunk);
                    }
                } while (bytesRead > 0 && !messageBuilder.ToString().Contains("\n"));

                if (messageBuilder.Length == 0) break;

                string request = messageBuilder.ToString().Trim();
                Env.Printl($"[MCP] 收到请求: {request}");

                string response = ProcessRequest(request);
                Env.Printl("[MCP] 发送响应");

                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                pipe.Write(responseBytes, 0, responseBytes.Length);
                pipe.Flush();

                Env.Printl("[MCP] 响应已发送");
            }

            if (!pipe.IsConnected)
            {
                Env.Printl("[MCP] 服务端已离线 (健康检测失败)");
            }
        }
        catch (Exception)
        {
            Env.Printl("[MCP] 客户端连接处理错误");
        }
        finally
        {
            try
            {
                pipe?.Close();
                pipe?.Dispose();
            }
            catch (Exception ex)
            {
                Env.Printl($"[MCP] 管道关闭错误: {ex.Message}");
            }
        }
    }

    private void SendConnectionConfirmationRaw(NamedPipeServerStream pipe)
    {
        var confirmation = new PipeResponse
        {
            Id = 400,
            Type = "connected",
            Payload = new { message = "CAD MCP Plugin connected successfully" }
        };
        var settings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
        string json = MyJson.SerializeObject(confirmation, settings);
        byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
        pipe.Write(bytes, 0, bytes.Length);
        pipe.Flush();
        Env.Printl("[MCP] 已发送连接确认");
    }

    private string ProcessRequest(string requestJson)
    {
        try
        {
            Env.Printl("[MCP] 开始处理请求");
            var request = MyJson.DeserializeObject<PipeRequest>(requestJson);
            Env.Printl($"[MCP] 反序列化成功, 类型: {request?.Type}, 编号: {request?.Id}");

            if (request?.Type is null)
            {
                return McpPlugin.CreateErrorResponse(request?.Id ?? -500, "INVALID_REQUEST", "Request type is null");
            }

            if (CommandRegistry.TryGetHandler(request.Type, out var handler))
            {
                return handler!.Handle(request);
            }

            return McpPlugin.CreateErrorResponse(request.Id ?? -500, "UNKNOWN_TYPE", $"Unknown request type: {request.Type}");
        }
        catch (Exception ex)
        {
            Env.Printl($"[MCP] 处理请求异常: {ex.GetType().Name} - {ex.Message}");
            Env.Printl("[MCP] 堆栈跟踪:");
            foreach (var line in (ex.StackTrace ?? "").Split('\n').Take(3))
                Env.Printl($"  {line.Trim()}");
            return McpPlugin.CreateErrorResponse(-500, "PARSE_ERROR", ex.Message);
        }
    }

    public void Dispose()
    {
        Stop();
        _stopEvent?.Close();
    }
}
