using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using IFoxCAD.Cad;

namespace TestAcad08.MCP
{
    /// <summary>
    /// Named Pipe服务器，用于与MCP Server通讯
    /// </summary>
    public class NamedPipeServer : IDisposable
    {
        private NamedPipeServerStream? _pipeServer;
        private ManualResetEvent? _stopEvent;
        private Thread? _listenerThread;
        private volatile bool _isRunning;
        private readonly string _pipeName;
        private readonly CommandExecutor _commandExecutor;
        private readonly PgpParser _pgpParser;

        public NamedPipeServer(string pipeName)
        {
            _pipeName = pipeName;
            _commandExecutor = new CommandExecutor();
            _pgpParser = new PgpParser();
        }

        /// <summary>
        /// 启动管道服务器
        /// </summary>
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

            Env.Printl($"[MCP] Server started on pipe: {_pipeName}");
        }

        /// <summary>
        /// 停止管道服务器
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _stopEvent?.Set();
            _pipeServer?.Close();
            _pipeServer?.Dispose();
            _pipeServer = null;

            Env.Printl("[MCP] Server stopped");
        }

        private void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // 使用 Byte 传输模式和同步模式
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
                        Env.Printl($"[MCP] Pipe error: {ex.Message}");
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
                    StringBuilder messageBuilder = new StringBuilder();
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

                    if (messageBuilder.Length == 0) continue;

                    string request = messageBuilder.ToString().Trim();
                    Env.Printl($"[MCP] Received: {request}");

                    string response = ProcessRequest(request);
                    Env.Printl($"[MCP] 准备发送响应: {response}");

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                    pipe.Write(responseBytes, 0, responseBytes.Length);
                    pipe.Flush();

                    Env.Printl($"[MCP] 响应已发送");
                }
            }
            catch (Exception ex)
            {
                Env.Printl($"[MCP] Client handling error: {ex.Message}");
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
                    Env.Printl($"[MCP] Pipe close error: {ex.Message}");
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
                Env.Printl($"[MCP] 开始处理请求: {requestJson}");
                var request = MyJson.DeserializeObject<PipeRequest>(requestJson);
                Env.Printl($"[MCP] 反序列化成功, Type: {request?.Type}, Id: {request?.Id}");

                return request?.Type switch
                {
                    "get_cad_info" => HandleGetCadInfo(request),
                    "send_command" => HandleSendCommand(request),
                    "get_history" => HandleGetHistory(request),
                    "switch_document" => HandleSwitchDocument(request),
                    "update_pgp" => HandleUpdatePgp(request),
                    "get_command_status" => HandleGetCommandStatus(request),
                    "say_hello_cad" => HandleSayHello(request),
                    _ => CreateErrorResponse(request?.Id ?? -500, "UNKNOWN_TYPE", $"Unknown request type: {request?.Type}")
                };
            }
            catch (Exception ex)
            {
                Env.Printl($"[MCP] 处理请求异常: {ex.GetType().Name} - {ex.Message}");
                Env.Printl($"[MCP] 堆栈跟踪: {ex.StackTrace}");
                return CreateErrorResponse(-500, "PARSE_ERROR", ex.Message);
            }
        }

        private string HandleGetCadInfo(PipeRequest request)
        {
            Env.Printl($"[MCP] 处理 get_cad_info 请求");
            var doc = Acap.DocumentManager.MdiActiveDocument;
            var info = new
            {
                pid = System.Diagnostics.Process.GetCurrentProcess().Id,
                year = "2008",
                version = Acap.Version.ToString(),
                document_name = doc?.Name ?? "No document",
                is_idle = IsCadIdle()
            };

            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = CreateSuccessResponse(request.Id, info);
            Env.Printl($"[MCP] get_cad_info 响应: {response}");
            return response;
        }

        private string HandleSendCommand(PipeRequest request)
        {
            string? command = request.Payload != null && request.Payload.TryGetValue("command", out var cmdValue)
                ? cmdValue?.ToString()
                : null;
            int timeout = 30;
            if (request.Payload != null && request.Payload.TryGetValue("timeout", out var timeoutValue))
            {
                if (timeoutValue is int intVal)
                    timeout = intVal;
                else if (timeoutValue != null && int.TryParse(timeoutValue.ToString(), out int parsed))
                    timeout = parsed;
            }

            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (command is null || string.IsNullOrEmpty(command))
            {
                return CreateErrorResponse(request.Id, "INVALID_COMMAND", "Command is empty");
            }



            // 检查是否有-前缀版本
            if (_pgpParser.HasDashVersion(command))
            {
                string dashCmd = "-" + command.Split(' ')[0];
                return CreateErrorResponse(request.Id, "DASH_VERSION_AVAILABLE",
                    $"Command '{command}' has a dash-prefixed version '{dashCmd}'. " +
                    "Please use the dash version for non-interactive execution.");
            }

            var result = _commandExecutor.Execute(command, timeout);
            return CreateSuccessResponse(request.Id, result);
        }

        private string HandleGetHistory(PipeRequest request)
        {
            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string? docName = request.Payload != null && request.Payload.TryGetValue("doc_name", out var docValue)
                ? docValue?.ToString()
                : null;
            var history = HistoryManager.GetHistory(docName);
            return CreateSuccessResponse(request.Id, new { history });
        }

        private string HandleSwitchDocument(PipeRequest request)
        {
            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string? docName = request.Payload != null && request.Payload.TryGetValue("doc_name", out var docValue)
                ? docValue?.ToString()
                : null;
            if (docName == null || StringHelper.IsNullOrWhiteSpace(docName))
            {
                return CreateErrorResponse(request.Id, "INVALID_DOC_NAME", "Document name is empty");
            }

            bool success = DocumentManager.SwitchDocument(docName);
            return CreateSuccessResponse(request.Id, new { success });
        }

        private string HandleUpdatePgp(PipeRequest request)
        {
            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _pgpParser.Reload();
            return CreateSuccessResponse(request.Id, new { success = true, command_count = _pgpParser.CommandCount });
        }

        private string HandleGetCommandStatus(PipeRequest request)
        {
            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var status = new
            {
                is_idle = IsCadIdle(),
                cmd_active = GetCmdActive(),
                last_input = GetLastInputString()
            };
            return CreateSuccessResponse(request.Id, status);
        }

        private string HandleSayHello(PipeRequest request)
        {
            if (request is null || request.Id is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Env.Printl($"[MCP] 处理 say_hello_cad 请求");
            var greetings = new[]
            {
                "你好啊", "今天天气真不错", "欢迎使用CAD", "新年快乐",
                "工作顺利", "加油", "天气晴朗", "心情美好",
                "一天之计在于晨", "你好啊，朋友"
            };
            var random = new Random();
            var greeting = greetings[random.Next(greetings.Length)];

            Env.Printl($"[say_hello_cad] {greeting}");
            var response = CreateSuccessResponse(request.Id, new { message = greeting });
            Env.Printl($"[MCP] say_hello_cad 响应: {response}");
            return response;
        }

        private bool IsCadIdle()
        {
            return GetCmdActive() == 0;
        }

        private int GetCmdActive()
        {
            try
            {
                return (int)Acap.GetSystemVariable("CMDACTIVE");
            }
            catch
            {
                return -1;
            }
        }

        private string GetLastInputString()
        {
            // .NET 3.5 / AutoCAD 2008 不支持 GetLastInputString
            return "";
        }

        private string CreateSuccessResponse(object id, object payload)
        {
            var response = JsonRpcProtocol.CreateSuccessResponse(id, payload);
            return JsonRpcProtocol.SerializeResponse(response);
        }

        private string CreateErrorResponse(object id, string code, string message)
        {
            JsonRpcErrorResponse errorResponse;
            if (int.TryParse(code, out int codeNum))
            {
                errorResponse = JsonRpcProtocol.CreateErrorResponse(id, codeNum, message);
            }
            else
            {
                errorResponse = JsonRpcProtocol.CreateErrorResponse(id, -32000, $"{code}: {message}");
            }
            return JsonRpcProtocol.SerializeErrorResponse(errorResponse);
        }

        public void Dispose()
        {
            Stop();
            _stopEvent?.Close();
        }
    }
}
