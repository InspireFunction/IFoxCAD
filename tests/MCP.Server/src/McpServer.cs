using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IFoxCAD.Cad;

namespace MCP.Server
{
    /// <summary>
    /// MCP Server 实现
    /// 处理MCP协议请求，支持多CAD实例
    /// </summary>
    public class McpServer : IDisposable
    {
        private readonly CadConnectionManager _connectionManager;
        private readonly SuccessCaseLogger _successLogger;
        private bool _isRunning;

        private Dictionary<string, object>? _cadServerInfo;

        private Dictionary<string, object> _cachedTools = [];

        private Dictionary<string, int> _toolToInstanceMap = new();

        public McpServer(CadConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _successLogger = new SuccessCaseLogger("success_cases.json");

            _connectionManager.OnConnected += OnCadConnected;
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
            Console.Error.WriteLine("[信息] 正在从 CAD 获取工具列表...");
            Console.Error.WriteLine();

            // mcp协议有严格的初始化逻辑,需要同步等待获取工具列表,
            // 然后它会自动缓存,所以是不允许多次动态更新工具表的,而这个缓存是在trae内部,我们无法修改.
            await FetchAllCadInfoAsync(ct);

            Console.Error.WriteLine("[信息] 等待 MCP 请求...");
            Console.Error.WriteLine();

            while (_isRunning && !ct.IsCancellationRequested)
            {
                try
                {
                    // 使用带超时的读取，定期检查连接状态
                    using var readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    readCts.CancelAfter(TimeSpan.FromSeconds(2));

                    try
                    {
                        var line = await Console.In.ReadLineAsync(readCts.Token);
                        if (line == null)
                        {
                            Console.Error.WriteLine("[信息] 输入流关闭，退出");
                            break;
                        }

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // 检查是否有可用的CAD连接
                            if (!HasAnyConnectedCad())
                            {
                                Console.Error.WriteLine("[警告] 没有可用的CAD连接，等待重连...");
                                var errorResponse = CreateMcpErrorResponse(null, -32603, "没有可用的CAD连接，请确保CAD已启动并加载MCP插件");
                                Console.Out.WriteLine(errorResponse);
                                continue;
                            }

                            var response = await ProcessMcpRequestAsync(line);
                            Console.Out.WriteLine(response);
                        }
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        // 读取超时，继续循环检查连接状态
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[错误] 处理请求异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从所有CAD实例获取信息并合并工具列表
        /// </summary>
        private async Task FetchAllCadInfoAsync(CancellationToken ct)
        {
            var connections = _connectionManager.GetAllConnections();
            if (connections.Count == 0)
            {
                Console.Error.WriteLine("[警告] 没有可用的CAD连接");
                return;
            }

            foreach (var connection in connections)
            {
                Console.Error.WriteLine($"[信息] 正在从 CAD #{connection.InstanceId} (PID: {connection.ProcessId}) 获取信息...");

                var serverInfo = await FetchCadServerInfoAsync(connection, ct);
                if (serverInfo != null && _cadServerInfo == null)
                {
                    _cadServerInfo = serverInfo;
                }

                var tools = await FetchCadToolsAsync(connection, ct);
                if (tools != null)
                {
                    foreach (var tool in tools)
                    {
                        if (tool is Dictionary<string, object> toolDict)
                        {
                            var toolName = toolDict.TryGetValue("name", out var name) ? name?.ToString() : null;
                            if (!string.IsNullOrEmpty(toolName))
                            {
                                _toolToInstanceMap[toolName] = connection.InstanceId;

                                var enhancedTool = new Dictionary<string, object>(toolDict)
                                {
                                    ["_instanceId"] = connection.InstanceId,
                                    ["_processId"] = connection.ProcessId
                                };

                                if (toolDict.TryGetValue("description", out var desc) && desc is string descStr)
                                {
                                    enhancedTool["description"] = $"[CAD#{connection.InstanceId}] {descStr}";
                                }

                                _cachedTools[toolName] = enhancedTool;
                            }
                        }
                    }
                }
            }

            Console.Error.WriteLine($"[信息] 已获取 {_cachedTools.Count} 个工具 (来自 {connections.Count} 个CAD实例)");
        }

        /// <summary>
        /// 当新CAD实例连接时触发
        /// </summary>
        private async void OnCadConnected(CadConnectionInfo connection)
        {
            try
            {
                Console.Error.WriteLine($"[信息] 检测到新CAD实例连接: #{connection.InstanceId} (PID: {connection.ProcessId})");
                Console.Error.WriteLine($"[信息] 正在重新获取工具列表...");

                var ct = CancellationToken.None;
                await FetchAllCadInfoAsync(ct);

                Console.Error.WriteLine($"[信息] 工具列表已更新，发送 list_changed 通知...");

                // 发送工具列表变更通知，触发客户端重新获取工具列表
                // TODO 实际上这个触发不会引发更新: HandleToolsList 返回工具数量 这个才是真正的更新.
                SendToolsListChangedNotification();

                Console.Error.WriteLine($"[信息] 已通知客户端工具列表变更");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[错误] 更新工具列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送工具列表变更通知
        /// 这会触发MCP客户端重新调用 tools/list 获取最新工具列表
        /// </summary>
        private void SendToolsListChangedNotification()
        {
            var notification = new Dictionary<string, object>
            {
                ["jsonrpc"] = "2.0",
                ["method"] = "notifications/tools/list_changed"
            };

            var settings = new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            var notificationJson = MyJson.SerializeObject(notification, settings);

            // 必须写入 stdout，这是MCP协议通知
            Console.Out.WriteLine(notificationJson);

            Console.Error.WriteLine($"[DEBUG] 已发送 notifications/tools/list_changed 通知: {notificationJson}");
        }

        /// <summary>
        /// 从单个CAD获取 serverInfo
        /// </summary>
        private async Task<Dictionary<string, object>?> FetchCadServerInfoAsync(CadConnectionInfo connection, CancellationToken ct)
        {
            try
            {
                var request = new Dictionary<string, object>
                {
                    ["id"] = 1,
                    ["type"] = "Mcp_cad_get_info",
                    ["payload"] = new Dictionary<string, object>()
                };
                var requestJson = MyJson.SerializeObject(request, new MyJsonSettings
                {
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                });

                var response = await connection.PipeClient!.SendRequestAsync(requestJson, 10000);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseObj = MyJson.DeserializeObject<Dictionary<string, object>>(response);
                    if (responseObj != null && responseObj.TryGetValue("result", out var resultObj) && resultObj is Dictionary<string, object> result)
                    {
                        if (result.TryGetValue("serverInfo", out var serverInfoObj) && serverInfoObj is Dictionary<string, object> serverInfo)
                        {
                            Console.Error.WriteLine($"[信息] CAD #{connection.InstanceId} serverInfo: name={serverInfo.GetValueOrDefault("name")}, version={serverInfo.GetValueOrDefault("version")}");
                            return serverInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[警告] 获取 CAD #{connection.InstanceId} serverInfo 失败: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 从单个CAD获取 tools 列表
        /// </summary>
        private async Task<List<object>?> FetchCadToolsAsync(CadConnectionInfo connection, CancellationToken ct)
        {
            try
            {
                var request = new Dictionary<string, object>
                {
                    ["id"] = 2,
                    ["type"] = "Mcp_cad_get_tools",
                    ["payload"] = new Dictionary<string, object>()
                };
                var requestJson = MyJson.SerializeObject(request, new MyJsonSettings
                {
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                });

                var response = await connection.PipeClient!.SendRequestAsync(requestJson, 10000);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseObj = MyJson.DeserializeObject<Dictionary<string, object>>(response);
                    if (responseObj != null && responseObj.TryGetValue("result", out var resultObj) && resultObj is Dictionary<string, object> result)
                    {
                        if (result.TryGetValue("tools", out var toolsObj) && toolsObj is List<object> tools)
                        {
                            Console.Error.WriteLine($"[信息] CAD #{connection.InstanceId} 返回 {tools.Count} 个工具");
                            return tools;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[警告] 获取 CAD #{connection.InstanceId} tools 失败: {ex.Message}");
            }
            return null;
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

                request.TryGetValue("id", out requestIdRaw);
                var method = request.TryGetValue("method", out var m) ? m?.ToString() : null;
                var parameters = request.TryGetValue("params", out var p) ? p : null;

                Console.Error.WriteLine($"[DEBUG] 解析结果 - id: {requestIdRaw}, method: {method}");

                if (string.IsNullOrEmpty(method))
                {
                    return CreateMcpErrorResponse(requestIdRaw, -32600, "Invalid Request: missing method");
                }

                return method switch
                {
                    "initialize" => HandleInitialize(requestIdRaw),
                    "notifications/initialized" => HandleNotificationsInitialized(requestIdRaw),
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
        /// 处理 notifications/initialized 请求
        /// 客户端初始化完成后，发送工具列表变更通知以触发重新获取
        /// </summary>
        private string HandleNotificationsInitialized(object? requestId)
        {
            Console.Error.WriteLine("[DEBUG] 收到 notifications/initialized，准备发送工具列表变更通知...");

            // 先返回空响应
            var response = CreateMcpSuccessResponse(requestId, new Dictionary<string, object>());

            // 然后发送工具列表变更通知
            // 使用 Task.Run 避免阻塞，让响应先发送出去
            Task.Run(async () => {
                // 等待一小段时间确保响应已发送
                await Task.Delay(100);
                SendToolsListChangedNotification();
            });

            return response;
        }

        /// <summary>
        /// 处理initialize请求
        /// </summary>
        private string HandleInitialize(object? requestId)
        {
            var serverInfo = _cadServerInfo ?? new Dictionary<string, object>
            {
                ["name"] = "mcp-cad-server",
                ["version"] = "2.0.0"
            };

            var connections = _connectionManager.GetAllConnections();
            var instanceCount = connections.Count;

            // 生成一个基于当前工具列表的哈希，用于让客户端识别工具列表已变更
            var toolsHash = GenerateToolsHash();

            var result = new Dictionary<string, object>
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new Dictionary<string, object>
                {
                    ["tools"] = new Dictionary<string, object>
                    {
                        ["listChanged"] = true  // 告诉客户端支持工具列表变更通知
                    }
                },
                ["serverInfo"] = new Dictionary<string, object>
                {
                    ["name"] = serverInfo.TryGetValue("name", out var n) ? n : "mcp-cad-server",
                    ["version"] = serverInfo.TryGetValue("version", out var v) ? v : "2.0.0"
                },
                ["_cadInstances"] = instanceCount,
                ["_toolsHash"] = toolsHash,  // 添加工具列表哈希，让客户端能检测变更
                ["_toolsCount"] = _cachedTools.Count
            };

            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] initialize 返回的响应: {responseJson}");
            return responseJson;
        }

        /// <summary>
        /// 生成工具列表的哈希值，用于客户端检测变更
        /// </summary>
        private string GenerateToolsHash()
        {
            // 基于工具名称和CAD实例ID生成哈希
            var toolKeys = string.Join(",", _cachedTools.Keys.OrderBy(k => k));
            var instanceIds = string.Join(",", _toolToInstanceMap.Values.OrderBy(v => v).Distinct());
            var hashInput = $"{toolKeys}|{instanceIds}|{_cachedTools.Count}";

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(hashInput);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).Substring(0, 16);  // 取前16位
        }

        /// <summary>
        /// 处理tools/list请求
        /// 返回合并后的工具列表
        /// </summary>
        private string HandleToolsList(object? requestId)
        {
            // TODO 这里才是真正的加入函数表,但是除了初始化,貌似没有机会更新.
            // 可能是mcp协议的问题?可能是trae的问题?
            var tools = _cachedTools.Values;
            var result = new Dictionary<string, object> { ["tools"] = tools };
            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] tools/list 返回工具数量:: {tools.Count}");
            return responseJson;
        }

        /// <summary>
        /// 处理tools/call请求
        /// 根据工具名称路由到对应的CAD实例
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

            Console.Error.WriteLine($"[DEBUG] 调用工具: {name}");

            CadConnectionInfo? targetConnection = null;

            if (_toolToInstanceMap.TryGetValue(name, out var instanceId))
            {
                targetConnection = _connectionManager.GetConnectionByInstanceId(instanceId);
            }

            if (targetConnection == null)
            {
                targetConnection = _connectionManager.GetAllConnections().FirstOrDefault();
            }

            if (targetConnection == null || targetConnection.PipeClient == null)
            {
                return CreateMcpToolErrorResponse(requestId, "没有可用的CAD连接");
            }

            Console.Error.WriteLine($"[DEBUG] 路由到 CAD #{targetConnection.InstanceId} (PID: {targetConnection.ProcessId})");

            string toolName = name;
            object? toolArguments = arguments;
            object? payloadObj = null;
            if (toolArguments is Dictionary<string, object> jo)
            {
                payloadObj = jo;
            }

            var pipeRequest = new Dictionary<string, object>
            {
                ["id"] = requestId,
                ["type"] = toolName.Replace("-", "_"),
                ["payload"] = payloadObj ?? new Dictionary<string, object>()
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var pipeRequestJson = MyJson.SerializeObject(pipeRequest, new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            Console.Error.WriteLine($"[DEBUG] 发送请求到CAD: {pipeRequestJson}");
            var pipeResponse = await targetConnection.PipeClient.SendRequestAsync(pipeRequestJson);
            stopwatch.Stop();

            Console.Error.WriteLine($"[DEBUG] 收到CAD响应: {pipeResponse}");

            if (pipeResponse == null)
            {
                return CreateMcpToolErrorResponse(requestId, "无法和CAD进行通讯，检查CAD通讯插件是否加载");
            }

            Dictionary<string, object>? responseDict;
            try
            {
                responseDict = MyJson.DeserializeObject<Dictionary<string, object>>(pipeResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] MyJson解析异常: {ex.GetType().Name}: {ex.Message}");
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

            if (responseType == "error")
            {
                var errorCode = payload != null && payload.TryGetValue("code", out var ec) ? ec?.ToString() ?? "UNKNOWN_ERROR" : "UNKNOWN_ERROR";
                var errorMessage = payload != null && payload.TryGetValue("message", out var em) ? em?.ToString() ?? "Unknown error" : "Unknown error";

                if (errorCode == "DASH_VERSION_AVAILABLE")
                {
                    return CreateMcpToolErrorResponse(requestId, errorMessage);
                }

                return CreateMcpToolErrorResponse(requestId, $"[{errorCode}] {errorMessage}");
            }

            string payloadText = pl != null ? MyJson.SerializeObject(pl, new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
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
            var settings = new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };
            return MyJson.SerializeObject(response, settings);
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
            var settings = new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };
            return MyJson.SerializeObject(response, settings);
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

        /// <summary>
        /// 检查是否还有任何可用的CAD连接
        /// </summary>
        private bool HasAnyConnectedCad()
        {
            var connections = _connectionManager.GetAllConnections();
            return connections.Any(c => c.IsConnected);
        }

        public void Dispose()
        {
            _isRunning = false;
            _successLogger?.Dispose();
            _connectionManager.OnConnected -= OnCadConnected;
        }
    }
}
