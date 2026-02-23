using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IFoxCAD.Cad;

namespace MCP.Server
{
    /// <summary>
    /// MCP Server 实现
    /// 处理MCP协议请求
    /// </summary>
    public class McpServer : IDisposable
    {
        private readonly NamedPipeClient _pipeClient;
        private readonly Config _config;
        private readonly Watchdog _watchdog;
        private readonly SuccessCaseLogger _successLogger;
        private bool _isRunning;

        // 从 CAD 获取的 serverInfo，用于 initialize 响应
        private Dictionary<string, object>? _cadServerInfo;

        // 从 CAD 获取的 tools 列表，用于 tools/list 响应
        private List<object>? _cadTools;

        public McpServer(NamedPipeClient pipeClient, Config config)
        {
            _pipeClient = pipeClient;
            _config = config;
            _watchdog = new Watchdog(pipeClient, config.Watchdog);
            _successLogger = new SuccessCaseLogger(config.Logging.SuccessCasePath);
        }

        /// <summary>
        /// 启动MCP Server
        /// </summary>
        public async Task StartAsync(CancellationToken ct)
        {
            _isRunning = true;
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
                Console.Out.NewLine = "\n";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[信息] Console encoding setup skipped: {ex.Message}");
            }

            Console.Error.WriteLine("[信息] MCP 服务已启动");
            Console.Error.WriteLine("[信息] 等待 MCP 请求...");
            Console.Error.WriteLine();

            // 启动时从 CAD 获取 serverInfo 和 tools
            await FetchCadServerInfoAsync(ct);
            await FetchCadToolsAsync(ct);

            // 启动看门狗
            _ = _watchdog.StartAsync(ct);

            // 主循环：从stdin读取MCP请求
            while (_isRunning && !ct.IsCancellationRequested)
            {
                try
                {
                    var line = await Console.In.ReadLineAsync(ct);
                    if (line == null)
                    {
                        Console.Error.WriteLine("[信息] CAD 已离线 (输入流关闭)");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var response = await ProcessMcpRequestAsync(line);
                    Console.Out.WriteLine(response);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[信息] CAD 已离线: {ex.Message}");
                    break;
                }
            }
        }

        /// <summary>
        /// 处理MCP请求
        /// </summary>
        private async Task<string> ProcessMcpRequestAsync(string requestJson)
        {
            Console.Error.WriteLine($"[DEBUG] 收到原始请求: {requestJson}");

            Dictionary<string, object>? request;
            object? requestIdRaw = null;

            try
            {
                request = MyJson.DeserializeObject<Dictionary<string, object>>(requestJson);
                if (request == null)
                {
                    Console.Error.WriteLine("[DEBUG] 解析失败: request 为 null");
                    return CreateMcpErrorResponse(null, -32700, "Parse error");
                }

                Console.Error.WriteLine($"[DEBUG] 解析后的请求: {IFoxCAD.Cad.MyJson.SerializeObject(request, new IFoxCAD.Cad.MyJsonSettings
                {
                    Formatting = IFoxCAD.Cad.Formatting.None,
                    PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                    ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
                })}");

                request.TryGetValue("id", out requestIdRaw);
                var method = request.TryGetValue("method", out var m) ? m?.ToString() : null;
                var parameters = request.TryGetValue("params", out var p) ? p : null;

                Console.Error.WriteLine($"[DEBUG] 解析结果 - id: {requestIdRaw}, method: {method}, params: {(parameters != null ? "存在" : "null")}");

                if (string.IsNullOrEmpty(method))
                {
                    Console.Error.WriteLine("[DEBUG] method 为空或缺失");
                    return CreateMcpErrorResponse(requestIdRaw, -32600, "Invalid Request: missing method");
                }

                // 处理MCP方法
                return method switch
                {
                    "initialize" => HandleInitialize(requestIdRaw),
                    "notifications/initialized" => CreateMcpSuccessResponse(requestIdRaw, new Dictionary<string, object>()),
                    "tools/list" => HandleToolsList(requestIdRaw),
                    "tools/call" => await HandleToolsCallAsync(requestIdRaw, parameters),
                    _ => CreateMcpErrorResponse(requestIdRaw, -32601, $"Method not found: {method}")
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DEBUG] 解析异常: {ex.Message}");
                return CreateMcpErrorResponse(requestIdRaw, -32700, $"Parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// 从 CAD 获取 serverInfo
        /// </summary>
        private async Task FetchCadServerInfoAsync(CancellationToken ct)
        {
            try
            {
                var request = new Dictionary<string, object>
                {
                    ["id"] = 1,
                    ["type"] = "Mcp_cad_get_info",
                    ["payload"] = new Dictionary<string, object>()
                };
                var requestJson = IFoxCAD.Cad.MyJson.SerializeObject(request, new IFoxCAD.Cad.MyJsonSettings
                {
                    Formatting = IFoxCAD.Cad.Formatting.None,
                    PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                    ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
                });

                Console.Error.WriteLine("[信息] 正在从 CAD 获取 serverInfo...");
                var response = await _pipeClient.SendRequestAsync(requestJson, 10000);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseObj = MyJson.DeserializeObject<Dictionary<string, object>>(response);
                    if (responseObj != null && responseObj.TryGetValue("result", out var resultObj) && resultObj is Dictionary<string, object> result)
                    {
                        if (result.TryGetValue("serverInfo", out var serverInfoObj) && serverInfoObj is Dictionary<string, object> serverInfo)
                        {
                            _cadServerInfo = serverInfo;
                            Console.Error.WriteLine($"[信息] 已从 CAD 获取 serverInfo: name={serverInfo.GetValueOrDefault("name")}, version={serverInfo.GetValueOrDefault("version")}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[警告] 获取 CAD serverInfo 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从 CAD 获取 tools 列表
        /// </summary>
        private async Task FetchCadToolsAsync(CancellationToken ct)
        {
            try
            {
                var request = new Dictionary<string, object>
                {
                    ["id"] = 2,
                    ["type"] = "Mcp_cad_get_tools",
                    ["payload"] = new Dictionary<string, object>()
                };
                var requestJson = IFoxCAD.Cad.MyJson.SerializeObject(request, new IFoxCAD.Cad.MyJsonSettings
                {
                    Formatting = IFoxCAD.Cad.Formatting.None,
                    PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                    ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
                });

                Console.Error.WriteLine("[信息] 正在从 CAD 获取 tools 列表...");
                var response = await _pipeClient.SendRequestAsync(requestJson, 10000);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseObj = MyJson.DeserializeObject<Dictionary<string, object>>(response);
                    if (responseObj != null && responseObj.TryGetValue("result", out var resultObj) && resultObj is Dictionary<string, object> result)
                    {
                        if (result.TryGetValue("tools", out var toolsObj) && toolsObj is List<object> tools)
                        {
                            _cadTools = tools;
                            Console.Error.WriteLine($"[信息] 已从 CAD 获取 {tools.Count} 个 tools");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[警告] 获取 CAD tools 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理initialize请求
        /// </summary>
        private string HandleInitialize(object? requestId)
        {
            // 使用从 CAD 获取的 serverInfo，如果没有则使用默认值
            var serverInfo = _cadServerInfo ?? new Dictionary<string, object>
            {
                ["name"] = "mcp-cad-server",
                ["version"] = "1.0.0"
            };

            var result = new Dictionary<string, object>
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new Dictionary<string, object>
                {
                    ["tools"] = new Dictionary<string, object> { ["listChanged"] = true }
                },
                ["serverInfo"] = serverInfo
            };

            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] initialize 返回的响应: {responseJson}");
            return responseJson;
        }

        /// <summary>
        /// 处理tools/list请求
        /// 使用从 CAD 获取的 tools 列表
        /// </summary>
        private string HandleToolsList(object? requestId)
        {
            // 使用从 CAD 获取的 tools，如果没有则返回空列表
            var tools = _cadTools ?? new List<object>();

            var result = new Dictionary<string, object> { ["tools"] = tools };
            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] tools/list 返回 {tools.Count} 个工具");
            return responseJson;
        }

        /// <summary>
        /// 处理tools/call请求
        /// </summary>
        private async Task<string> HandleToolsCallAsync(object? requestId, object? parameters)
        {
            if (requestId is null)
            {
                throw new ArgumentNullException(nameof(requestId));
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Console.Error.WriteLine($"[DEBUG] 收到tools/call请求: {(parameters != null ? IFoxCAD.Cad.MyJson.SerializeObject(parameters, new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            }) : "null")}");

            if (parameters == null)
            {
                return CreateMcpErrorResponse(requestId, -32602, "Invalid params");
            }

            if (parameters is not Dictionary<string, object> paramDict)
            {
                return CreateMcpErrorResponse(requestId, -32602, "Invalid params: expected object");
            }

            var name = paramDict.TryGetValue("name", out var n) ? n?.ToString() : null;
            var arguments = paramDict.TryGetValue("arguments", out var a) ? a : null;

            if (string.IsNullOrEmpty(name))
            {
                return CreateMcpErrorResponse(requestId, -32602, "Missing tool name");
            }

            Console.Error.WriteLine($"[DEBUG] 调用工具: {name}, 参数: {(arguments != null ? IFoxCAD.Cad.MyJson.SerializeObject(arguments, new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            }) : "null")}");

            string toolName = name;
            object? toolArguments = arguments;

            if (name == "say_hello_cad")
            {
                var greetings = new[]
                {
                    "你好啊", "今天天气真不错", "欢迎使用CAD", "新年快乐",
                    "工作顺利", "加油", "天气晴朗", "心情美好",
                    "一天之计在于晨", "你好啊，朋友"
                };
                var random = new Random();
                var greeting = greetings[random.Next(greetings.Length)];

                toolArguments = new Dictionary<string, object>
                {
                    ["text"] = greeting
                };

                Console.Error.WriteLine($"[DEBUG] 随机问候语: {greeting}");
            }

            // 构建管道请求
            // 注意：CAD端只接受整数ID，所以需要转换
            // 但最终响应TRAE时要用回原始requestId
            object? payloadObj = null;
            if (toolArguments is Dictionary<string, object> jo)
            {
                payloadObj = jo;
            }

            // MCP规范是,id是trae创建的,我不需要创建,传递的原始id就好了,他可能是object,原封不动返回就好了,
            var pipeRequest = new Dictionary<string, object>
            {
                ["id"] = requestId,
                ["type"] = toolName.Replace("-", "_"),
                ["payload"] = payloadObj ?? new Dictionary<string, object>()
            };

            // 发送请求到CAD
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var pipeRequestJson = IFoxCAD.Cad.MyJson.SerializeObject(pipeRequest, new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            });
            Console.Error.WriteLine($"[DEBUG] 发送请求到CAD: {pipeRequestJson}");
            var pipeResponse = await _pipeClient.SendRequestAsync(pipeRequestJson);
            stopwatch.Stop();

            Console.Error.WriteLine($"[DEBUG] 收到CAD响应: {pipeResponse}");

            if (pipeResponse == null)
            {
                return CreateMcpToolErrorResponse(requestId, "无法和CAD进行通讯，检查CAD通讯插件是否加载");
            }

            // 解析响应（使用 MyJson）
            Dictionary<string, object>? responseDict;
            try
            {
                responseDict = MyJson.DeserializeObject<Dictionary<string, object>>(pipeResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] MyJson解析异常: {ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine($"[ERROR] 异常堆栈: {ex.StackTrace}");
                return CreateMcpToolErrorResponse(requestId, $"Invalid response from CAD: {ex.Message}");
            }

            if (responseDict == null)
            {
                return CreateMcpToolErrorResponse(requestId, "Invalid response from CAD");
            }

            var responseType = responseDict.TryGetValue("type", out var rt) ? rt?.ToString() : null;
            var hasPayload = responseDict.TryGetValue("payload", out var pl);
            if (!hasPayload)
            {
                hasPayload = responseDict.TryGetValue("result", out pl);
                if (hasPayload && responseType == null)
                    responseType = "result";
            }

            var payload = hasPayload ? pl as Dictionary<string, object> : null;
            Console.Error.WriteLine($"[DEBUG] payload转换后: {(payload != null ? "成功" : "失败, 类型不匹配")}");
            if (payload != null)
            {
                Console.Error.WriteLine($"[DEBUG] payload内容: {IFoxCAD.Cad.MyJson.SerializeObject(payload, new IFoxCAD.Cad.MyJsonSettings { Formatting = IFoxCAD.Cad.Formatting.None, PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None, ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize })}");
            }

            // 记录成功案例
            if (responseType == "result" && name == "send_command")
            {
                var success = payload != null && payload.TryGetValue("success", out var s) && s is bool b && b;
                if (success)
                {
                    _successLogger.Log(new SuccessCase
                    {
                        Timestamp = DateTime.Now,
                        Command = arguments is Dictionary<string, object> argsJo && argsJo.TryGetValue("command", out var cmd) ? cmd?.ToString() ?? "" : "",
                        ExecutionTime = stopwatch.ElapsedMilliseconds
                    });
                }
            }

            // 返回MCP响应
            if (responseType == "error")
            {
                var errorCode = payload != null && payload.TryGetValue("code", out var ec) ? ec?.ToString() ?? "UNKNOWN_ERROR" : "UNKNOWN_ERROR";
                var errorMessage = payload != null && payload.TryGetValue("message", out var em) ? em?.ToString() ?? "Unknown error" : "Unknown error";

                // 特殊处理：DASH_VERSION_AVAILABLE
                if (errorCode == "DASH_VERSION_AVAILABLE")
                {
                    return CreateMcpToolErrorResponse(requestId, errorMessage);
                }

                return CreateMcpToolErrorResponse(requestId, $"[{errorCode}] {errorMessage}");
            }

            // 构建文本内容响应
            // 修复：直接使用pl对象序列化，而不是强制转换为Dictionary<string, object>
            string payloadText = pl != null ? IFoxCAD.Cad.MyJson.SerializeObject(pl, new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            }) : "{}";
            var content = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["type"] = "text",
                    ["text"] = payloadText
                }
            };

            var result = new Dictionary<string, object>
            {
                ["content"] = content,
                ["isError"] = false
            };

            return CreateMcpSuccessResponse(requestId, result);
        }

        /// <summary>
        /// 创建MCP成功响应
        /// </summary>
        private string CreateMcpSuccessResponse(object? requestId, object result)
        {
            var response = new Dictionary<string, object>
            {
                ["jsonrpc"] = "2.0",
                ["id"] = requestId ?? "",
                ["result"] = result
            };
            var settings = new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            };
            return IFoxCAD.Cad.MyJson.SerializeObject(response, settings);
        }

        /// <summary>
        /// 创建MCP错误响应
        /// </summary>
        private string CreateMcpErrorResponse(object? requestId, int code, string message)
        {
            var response = new Dictionary<string, object>
            {
                ["jsonrpc"] = "2.0",
                ["id"] = requestId ?? "",
                ["error"] = new Dictionary<string, object>
                {
                    ["code"] = code,
                    ["message"] = message
                }
            };
            var settings = new IFoxCAD.Cad.MyJsonSettings
            {
                Formatting = IFoxCAD.Cad.Formatting.None,
                PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
                ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
            };
            return IFoxCAD.Cad.MyJson.SerializeObject(response, settings);
        }

        /// <summary>
        /// 创建MCP工具错误响应
        /// </summary>
        private string CreateMcpToolErrorResponse(object? requestId, string message)
        {
            var content = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["type"] = "text",
                    ["text"] = message
                }
            };

            var result = new Dictionary<string, object>
            {
                ["content"] = content,
                ["isError"] = true
            };

            return CreateMcpSuccessResponse(requestId, result);
        }

        public void Dispose()
        {
            _isRunning = false;
            _pipeClient?.Dispose();
            _watchdog?.Dispose();
            _successLogger?.Dispose();
        }
    }
}
