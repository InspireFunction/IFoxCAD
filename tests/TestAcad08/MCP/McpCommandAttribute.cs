using System;
using System.Collections.Generic;

namespace TestAcad08.MCP;

/// <summary>
/// 参数定义特性 - 用于描述命令参数
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class McpParamAttribute : Attribute
{
    /// <summary>
    /// 参数描述
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 参数类型（覆盖实际类型）
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// 默认值
    /// </summary>
    public object? DefaultValue { get; set; }

    public McpParamAttribute(string description)
    {
        Description = description;
    }
}

/// <summary>
/// MCP命令特性 - 用于标记CAD命令方法
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class McpCommandAttribute : Attribute
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 命令描述
    /// </summary>
    public string Description { get; }

    public McpCommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// 工具参数定义
/// </summary>
public class McpToolParam
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public string Description { get; set; } = "";
    public bool Required { get; set; } = true;
    public object? DefaultValue { get; set; }
}

/// <summary>
/// MCP工具定义
/// </summary>
public class McpTool
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<McpToolParam> Parameters { get; set; } = new();

    /// <summary>
    /// 转换为JSON Schema格式
    /// </summary>
    public Dictionary<string, object> ToInputSchema()
    {
        var properties = new Dictionary<string, object>();
        var required = new List<object>();

        foreach (var param in Parameters)
        {
            var propDict = new Dictionary<string, object>
            {
                ["type"] = param.Type,
                ["description"] = param.Description
            };

            if (param.DefaultValue != null)
            {
                propDict["default"] = param.DefaultValue;
            }

            properties[param.Name] = propDict;

            if (param.Required)
            {
                required.Add(param.Name);
            }
        }

        return new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required
        };
    }
}
