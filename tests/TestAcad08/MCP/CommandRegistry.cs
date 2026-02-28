using System.Collections.Generic;
using System.Reflection;

namespace TestAcad08.MCP;

/// <summary>
/// 命令处理器接口
/// </summary>
public interface IMcpCommandHandler
{
    string Handle(PipeRequest request);
}

/// <summary>
/// 命令注册表 - 管理所有MCP命令
/// </summary>
public static class CommandRegistry
{
    private static readonly Dictionary<string, IMcpCommandHandler> _handlers = new();
    private static readonly Dictionary<string, string> _descriptions = new();
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    public static IDictionary<string, IMcpCommandHandler> Handlers => _handlers;
    public static IDictionary<string, string> Descriptions => _descriptions;

    /// <summary>
    /// 初始化命令注册表 - 从CadCommandHandler自动注册所有命令
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            var handler = McpPlugin.CommandHandler;
            if (handler == null)
            {
                throw new InvalidOperationException("McpPlugin.CommandHandler 未初始化");
            }

            RegisterFromInstance(handler);
            _isInitialized = true;
        }
    }

    /// <summary>
    /// 从实例注册命令
    /// </summary>
    public static void RegisterFromInstance(CadCommandHandler instance)
    {
        var type = typeof(CadCommandHandler);

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            // .NET 3.5 兼容方式获取特性
            var attrs = method.GetCustomAttributes(typeof(McpCommandAttribute), false);
            if (attrs == null || attrs.Length == 0) continue;
            var attr = attrs[0] as McpCommandAttribute;
            if (attr == null) continue;

            // 验证方法签名: 第一个参数必须是PipeRequest
            var parameters = method.GetParameters();
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(PipeRequest))
            {
                Env.Printl($"[MCP] 警告: 命令 {attr.Name} 的方法签名不正确,跳过注册");
                continue;
            }

            var handler = new DynamicHandler(instance, method);
            Register(attr.Name, handler, attr.Description);
        }
    }

    /// <summary>
    /// 注册命令处理器
    /// </summary>
    public static void Register(string name, IMcpCommandHandler handler, string? description = null)
    {
        _handlers[name] = handler;
        if (description != null)
            _descriptions[name] = description;
    }

    /// <summary>
    /// 尝试获取命令处理器
    /// </summary>
    public static bool TryGetHandler(string commandName, out IMcpCommandHandler? handler)
    {
        return _handlers.TryGetValue(commandName, out handler);
    }

    /// <summary>
    /// 获取所有命令名称
    /// </summary>
    public static IEnumerable<string> GetCommandNames() => _handlers.Keys;

    /// <summary>
    /// 注销指定命令
    /// </summary>
    /// <param name="name">命令名称</param>
    /// <returns>是否成功注销</returns>
    public static bool Unregister(string name)
    {
        var removed = _handlers.Remove(name);
        _descriptions.Remove(name);
        return removed;
    }

    /// <summary>
    /// 重置注册表（用于插件卸载或重新加载）
    /// 修复说明：提供清理机制，解决以下问题：
    /// 1. McpPlugin.Terminate() 时无法清理注册表状态
    /// 2. 重新加载插件时旧数据残留
    /// 3. 与 ToolRegistry.Reset() 配合保持数据一致性
    /// </summary>
    public static void Reset()
    {
        _isInitialized = false;
        _handlers.Clear();
        _descriptions.Clear();
    }

    /// <summary>
    /// 动态命令处理器包装器
    /// 在调用前验证request.Id的有效性
    /// </summary>
    private class DynamicHandler : IMcpCommandHandler
    {
        private readonly CadCommandHandler _instance;
        private readonly MethodInfo _method;

        public DynamicHandler(CadCommandHandler instance, MethodInfo method)
        {
            _instance = instance;
            _method = method;
        }

        public string Handle(PipeRequest request)
        {
            // 在反射层验证request和id的有效性
            // 这样具体命令方法不需要再检查
            if (request?.Id is null)
            {
                return McpPlugin.CreateErrorResponse(-500, "INVALID_REQUEST", "Request or Id is null");
            }

            try
            {
                var result = _method.Invoke(_instance, new object[] { request });
                return result?.ToString() ?? "";
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                // 捕获原始异常
                return McpPlugin.CreateErrorResponse(request.Id, "HANDLER_ERROR", tie.InnerException.Message);
            }
            catch (Exception ex)
            {
                return McpPlugin.CreateErrorResponse(request.Id, "HANDLER_ERROR", ex.Message);
            }
        }
    }
}
