using System;
using System.Collections.Generic;
using System.Reflection;

namespace TestAcad08.MCP;

/// <summary>
/// Tool 注册表 - 管理所有 MCP 工具定义
/// 通过反射自动从 CadCommandHandler 提取工具定义
/// </summary>
public static class ToolRegistry
{
    private static readonly List<McpTool> _tools = [];
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    /// <summary>
    /// 获取所有工具
    /// </summary>
    public static List<McpTool> GetAllTools()
    {
        if (!_isInitialized)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    InitializeTools();
                    _isInitialized = true;
                }
            }
        }
        return [.. _tools];
    }

    /// <summary>
    /// 初始化所有工具定义 - 通过反射从 CadCommandHandler 提取
    /// </summary>
    private static void InitializeTools()
    {
        _tools.Clear();

        var handlerType = typeof(CadCommandHandler);
        var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var cmdAttr = GetMcpCommandAttribute(method);
            if (cmdAttr == null) continue;

            var tool = new McpTool
            {
                Name = cmdAttr.Name,
                Description = cmdAttr.Description
            };

            // 提取参数信息（跳过第一个 PipeRequest 参数）
            var parameters = method.GetParameters();
            for (int i = 1; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var paramAttr = GetMcpParamAttribute(param);

                string paraType = "";
                if (paramAttr is not null && paramAttr.Type is not null)
                {
                    paraType = paramAttr.Type;
                }
                paraType ??= GetJsonType(param.ParameterType);

                var toolParam = new McpToolParam
                {
                    Name = param.Name ?? "param" + i,
                    Type = paraType,
                    Description = paramAttr != null ? paramAttr.Description : "",
                    Required = paramAttr != null ? paramAttr.Required : !IsOptionalParameter(param),
                    DefaultValue = paramAttr?.DefaultValue
                };

                tool.Parameters.Add(toolParam);
            }

            _tools.Add(tool);
        }
    }

    /// <summary>
    /// 获取方法上的 McpCommandAttribute
    /// </summary>
    private static McpCommandAttribute? GetMcpCommandAttribute(MethodInfo method)
    {
        var attrs = method.GetCustomAttributes(typeof(McpCommandAttribute), true);
        if (attrs != null && attrs.Length > 0)
        {
            return attrs[0] as McpCommandAttribute;
        }
        return null;
    }

    /// <summary>
    /// 获取参数上的 McpParamAttribute
    /// </summary>
    private static McpParamAttribute? GetMcpParamAttribute(ParameterInfo param)
    {
        var attrs = param.GetCustomAttributes(typeof(McpParamAttribute), true);
        if (attrs != null && attrs.Length > 0)
        {
            return attrs[0] as McpParamAttribute;
        }
        return null;
    }

    /// <summary>
    /// 判断参数是否为可选参数（.NET 3.5兼容方式）
    /// </summary>
    private static bool IsOptionalParameter(ParameterInfo param)
    {
        // 检查是否有默认参数值（通过属性或类型判断）
        // 在 .NET 3.5 中，我们依赖 McpParamAttribute 的 Required 属性
        return false;
    }

    /// <summary>
    /// 将 .NET 类型映射为 JSON Schema 类型
    /// </summary>
    private static string GetJsonType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return "string";
        if (underlyingType == typeof(int) || underlyingType == typeof(long))
            return "integer";
        if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
            return "number";
        if (underlyingType == typeof(bool))
            return "boolean";
        if (underlyingType.IsArray)
            return "array";

        return "string";
    }

    /// <summary>
    /// 获取所有工具名称
    /// </summary>
    public static List<string> GetToolNames()
    {
        var tools = GetAllTools();
        var names = new List<string>();
        foreach (var tool in tools)
        {
            names.Add(tool.Name);
        }
        return names;
    }

    /// <summary>
    /// 重置注册表（用于测试或重新加载）
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _isInitialized = false;
            _tools.Clear();
        }
    }
}
