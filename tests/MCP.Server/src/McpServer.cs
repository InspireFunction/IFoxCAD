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

            // 启动看门狗
            _ = _watchdog.StartAsync(ct);

            // 主循环：从stdin读取MCP请求
            while (_isRunning && !ct.IsCancellationRequested)
            {
                try
                {
                    var line = await Console.In.ReadLineAsync(ct);
                    if (line == null) break;

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
                    var errorResponse = CreateMcpErrorResponse(null, -32603, $"Internal error: {ex.Message}");
                    Console.Out.WriteLine(errorResponse);
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
        /// 处理initialize请求
        /// </summary>
        private string HandleInitialize(object? requestId)
        {
            var result = new Dictionary<string, object>
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new Dictionary<string, object>
                {
                    ["tools"] = new Dictionary<string, object> { ["listChanged"] = true }
                },
                ["serverInfo"] = new Dictionary<string, object>
                {
                    ["name"] = "mcp-cad-server",
                    ["version"] = "1.0.0"
                }
            };

            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] initialize 返回的响应: {responseJson}");
            return responseJson;
        }

        /// <summary>
        /// 处理tools/list请求
        /// </summary>
        private string HandleToolsList(object? requestId)
        {
            var tools = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "get_cad_info",
                    ["description"] = "获取CAD进程信息，包括PID、版本、年份等",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "send_command",
                    ["description"] = "发送命令到CAD执行。注意：如果命令有-前缀版本，请优先使用-前缀版本以避免交互式提示",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["command"] = new Dictionary<string, object>
                            {
                                ["type"] = "string",
                                ["description"] = "CAD命令，例如 '-circle 0,0,0 500'"
                            },
                            ["timeout"] = new Dictionary<string, object>
                            {
                                ["type"] = "integer",
                                ["description"] = "超时时间（秒），默认30秒",
                                ["default"] = 30
                            }
                        },
                        ["required"] = new List<object> { "command" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "get_history",
                    ["description"] = "获取CAD命令执行历史",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["doc_name"] = new Dictionary<string, object>
                            {
                                ["type"] = "string",
                                ["description"] = "文档名称（可选，默认当前文档）"
                            }
                        },
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "switch_document",
                    ["description"] = "切换到指定的CAD文档",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["doc_name"] = new Dictionary<string, object>
                            {
                                ["type"] = "string",
                                ["description"] = "目标文档名称"
                            }
                        },
                        ["required"] = new List<object> { "doc_name" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "update_pgp",
                    ["description"] = "重新加载PGP命令定义",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "get_command_status",
                    ["description"] = "获取当前命令执行状态",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_line",
                    ["description"] = "在CAD中画直线",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["x1"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "起点X坐标" },
                            ["y1"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "起点Y坐标" },
                            ["z1"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "起点Z坐标", ["default"] = 0 },
                            ["x2"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "终点X坐标" },
                            ["y2"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "终点Y坐标" },
                            ["z2"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "终点Z坐标", ["default"] = 0 }
                        },
                        ["required"] = new List<object> { "x1", "y1", "x2", "y2" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_circle",
                    ["description"] = "在CAD中画圆",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["x"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心X坐标" },
                            ["y"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心Y坐标" },
                            ["z"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心Z坐标", ["default"] = 0 },
                            ["radius"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆的半径" }
                        },
                        ["required"] = new List<object> { "x", "y", "radius" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_arc",
                    ["description"] = "在CAD中画圆弧",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["x"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心X坐标" },
                            ["y"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心Y坐标" },
                            ["z"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆心Z坐标", ["default"] = 0 },
                            ["radius"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "圆弧半径" },
                            ["start_angle"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "起始角度（度）" },
                            ["end_angle"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "终止角度（度）" }
                        },
                        ["required"] = new List<object> { "x", "y", "radius", "start_angle", "end_angle" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_rectangle",
                    ["description"] = "在CAD中画矩形",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["x1"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "角点1 X坐标" },
                            ["y1"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "角点1 Y坐标" },
                            ["x2"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "角点2 X坐标" },
                            ["y2"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "角点2 Y坐标" },
                            ["z"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "Z坐标", ["default"] = 0 }
                        },
                        ["required"] = new List<object> { "x1", "y1", "x2", "y2" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_polyline",
                    ["description"] = "在CAD中画多段线",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["points"] = new Dictionary<string, object>
                            {
                                ["type"] = "array",
                                ["description"] = "顶点坐标数组，格式: [[x,y,z], [x,y,z], ...]",
                                ["items"] = new Dictionary<string, object>
                                {
                                    ["type"] = "array",
                                    ["items"] = new Dictionary<string, object> { ["type"] = "number" }
                                }
                            },
                            ["close"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "是否闭合多段线", ["default"] = false }
                        },
                        ["required"] = new List<object> { "points" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "draw_text",
                    ["description"] = "在CAD中创建单行文字",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["text"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "文字内容" },
                            ["x"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "插入点X坐标" },
                            ["y"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "插入点Y坐标" },
                            ["z"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "插入点Z坐标", ["default"] = 0 },
                            ["height"] = new Dictionary<string, object> { ["type"] = "number", ["description"] = "文字高度", ["default"] = 2.5 }
                        },
                        ["required"] = new List<object> { "text", "x", "y" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "zoom_extents",
                    ["description"] = "缩放视图到图形范围",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "regen",
                    ["description"] = "重新生成图形",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "say_hello_cad",
                    ["description"] = "在CAD命令行随机打印一句问候语",
                    ["inputSchema"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new List<object>()
                    }
                }
            };

            var result = new Dictionary<string, object> { ["tools"] = tools };
            var responseJson = CreateMcpSuccessResponse(requestId, result);
            Console.Error.WriteLine($"[DEBUG] tools/list 返回的响应: {responseJson}");
            return responseJson;
        }

        /// <summary>
        /// 处理tools/call请求
        /// </summary>
        private async Task<string> HandleToolsCallAsync(object? requestId, object? parameters)
        {
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
