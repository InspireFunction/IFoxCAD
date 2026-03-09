using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace IFoxFunKit;

/// <summary>
/// 诊断消息的统一管理类
/// 集中管理所有错误码对应的标题、消息格式和描述
/// </summary>
internal static class DiagnosticMessages
{
    /// <summary>
    /// 错误码定义
    /// </summary>
    public static class ErrorCodes
    {
        public const string MustHandleResult = "IFOX001";
        public const string MustCheckBeforeAccess = "IFOX002";
    }

    /// <summary>
    /// 分类定义
    /// </summary>
    public static class Categories
    {
        public const string Design = "Design";
    }

    /// <summary>
    /// 诊断消息模板
    /// </summary>
    private static readonly Dictionary<string, DiagnosticTemplate> Templates = new Dictionary<string, DiagnosticTemplate>
    {
        [ErrorCodes.MustHandleResult] = new DiagnosticTemplate
        {
            Title = "必须处理 Option<T> 或 Result<T> 的返回值",
            MessageFormat = "返回值类型 '{0}' 必须被显式处理，不能忽略。\n" +
                          "建议处理方式：\n" +
                          "  • 使用 Match() 方法处理所有分支\n" +
                          "  • 使用 Switch 表达式匹配\n" +
                          "  • 检查 IsSome/IsOk/IsErr 属性后再访问值",
            Description = "Option<T> 和 Result<T> 的返回值必须被显式处理所有分支，不能忽略。"
        },

        [ErrorCodes.MustCheckBeforeAccess] = new DiagnosticTemplate
        {
            Title = "访问值之前必须先检查状态",
            MessageFormat = "不安全访问：在读取 '{0}' 之前，必须先验证 '{1}'。\n" +
                          "正确用法示例：\n" +
                          "  if (opt.IsSome) {{ var v = opt.Value; }}      // 先检查 IsSome\n" +
                          "  if (res.IsOk)  {{ var v = res.OkValue; }}     // 先检查 IsOk\n" +
                          "  if (res.IsErr) {{ var e = res.ErrValue; }}    // 先检查 IsErr",
            Description = "Option<T>.Value 必须在检查 IsSome 后才能安全访问，" +
                         "Result<T>.OkValue 必须在检查 IsOk 后才能安全访问，" +
                         "Result<T>.ErrValue 必须在检查 IsErr 后才能安全访问。"
        }
    };

    /// <summary>
    /// 获取诊断描述符
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="severity">严重级别，默认为 Error</param>
    /// <returns>诊断描述符</returns>
    public static DiagnosticDescriptor GetDescriptor(
        string errorCode,
        DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        if (!Templates.TryGetValue(errorCode, out var template))
        {
            throw new ArgumentException($"未知的错误码: {errorCode}", nameof(errorCode));
        }

        return new DiagnosticDescriptor(
            id: errorCode,
            title: new SimpleLocalizableString(template.Title),
            messageFormat: new SimpleLocalizableString(template.MessageFormat),
            category: Categories.Design,
            defaultSeverity: severity,
            isEnabledByDefault: true,
            description: new SimpleLocalizableString(template.Description));
    }

    /// <summary>
    /// 获取格式化的错误消息
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="args">格式化参数</param>
    /// <returns>格式化后的消息</returns>
    public static string GetFormattedMessage(string errorCode, params object[] args)
    {
        if (!Templates.TryGetValue(errorCode, out var template))
        {
            return $"未知错误码: {errorCode}";
        }

        return string.Format(template.MessageFormat, args);
    }

    /// <summary>
    /// 创建带有格式化消息的 DiagnosticDescriptor
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="args">格式化参数</param>
    /// <returns>格式化后的 DiagnosticDescriptor</returns>
    public static DiagnosticDescriptor GetFormattedDescriptor(string errorCode, params object[] args)
    {
        if (!Templates.TryGetValue(errorCode, out var template))
        {
            throw new ArgumentException($"未知的错误码: {errorCode}", nameof(errorCode));
        }

        var formattedMessage = string.Format(template.MessageFormat, args);

        return new DiagnosticDescriptor(
            id: errorCode,
            title: new SimpleLocalizableString(template.Title),
            messageFormat: new SimpleLocalizableString(formattedMessage),
            category: Categories.Design,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new SimpleLocalizableString(template.Description));
    }

    /// <summary>
    /// 获取错误标题
    /// </summary>
    public static string GetTitle(string errorCode)
    {
        return Templates.TryGetValue(errorCode, out var template)
            ? template.Title
            : $"未知错误码: {errorCode}";
    }

    /// <summary>
    /// 获取错误描述
    /// </summary>
    public static string GetDescription(string errorCode)
    {
        return Templates.TryGetValue(errorCode, out var template)
            ? template.Description
            : $"未知错误码: {errorCode}";
    }

    /// <summary>
    /// 诊断消息模板结构
    /// </summary>
    private class DiagnosticTemplate
    {
        public string Title { get; set; } = string.Empty;
        public string MessageFormat { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

/// <summary>
/// 简单的 LocalizableString 实现，直接返回固定文本
/// </summary>
internal class SimpleLocalizableString : LocalizableString
{
    private readonly string _text;

    public SimpleLocalizableString(string text)
    {
        _text = text ?? string.Empty;
    }

    protected override string GetText(IFormatProvider? formatProvider)
    {
        return _text;
    }

    protected override bool AreEqual(object? other)
    {
        if (other is SimpleLocalizableString otherString)
        {
            return _text == otherString._text;
        }
        return false;
    }

    protected override int GetHash()
    {
        return _text.GetHashCode();
    }
}
