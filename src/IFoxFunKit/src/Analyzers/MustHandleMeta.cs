using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace IFoxFunKit;

/// <summary>
/// 反射辅助类，
/// 用于从IFoxFunKit程序集中动态获取标记了MustHandleMember特性的成员信息
/// </summary>
internal static class MustHandleMeta
{
    // 使用反射动态获取标记了MustHandleMember特性的成员名称
    public static readonly ImmutableHashSet<string> ValidMemberNames = GetMustHandleMemberNames();
    public static readonly ImmutableHashSet<string> StatusCheckMemberNames = GetMustHandleMemberNames(MustHandleMemberKind.StatusCheck);
    public static readonly ImmutableHashSet<string> ValueAccessMemberNames = GetMustHandleMemberNames(MustHandleMemberKind.ValueAccess);
    public static readonly string MatchMethodName = GetMatchMethodName();

    // 使用反射获取类型名称
    public static readonly string OptionTypeFullName = typeof(Option<>).FullName?.Split('`')[0] ?? "IFoxFunKit.Option";
    public static readonly string ResultTypeFullName = typeof(Result<,>).FullName?.Split('`')[0] ?? "IFoxFunKit.Result";

    /// <summary>
    /// 通过反射获取所有标记了MustHandleMember特性的成员名称
    /// </summary>
    private static ImmutableHashSet<string> GetMustHandleMemberNames(MustHandleMemberKind? kind = null)
    {
        var names = new List<string>();
        var memberAttributeType = typeof(MustHandleMemberAttribute);

        // 从Option<>类型获取
        var optionType = typeof(Option<>);
        names.AddRange(GetMarkedMembers(optionType, memberAttributeType, kind));

        // 从Result<,>类型获取
        var resultType = typeof(Result<,>);
        names.AddRange(GetMarkedMembers(resultType, memberAttributeType, kind));

        // 从扩展方法类获取
        var optionExtType = typeof(OptionExtensions);
        names.AddRange(GetMarkedMembers(optionExtType, memberAttributeType, kind));

        var resultExtType = typeof(ResultExtensions);
        names.AddRange(GetMarkedMembers(resultExtType, memberAttributeType, kind));

        return names.ToImmutableHashSet();
    }

    /// <summary>
    /// 从类型中获取标记了指定特性的成员名称
    /// </summary>
    private static IEnumerable<string> GetMarkedMembers(Type type, Type attributeType, MustHandleMemberKind? kind)
    {
        // 获取属性
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttributes(attributeType, false).FirstOrDefault() as MustHandleMemberAttribute;
            if (attr != null && (!kind.HasValue || attr.Kind == kind.Value))
            {
                yield return prop.Name;
            }
        }

        // 获取方法
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            // 跳过属性访问器方法
            if (method.IsSpecialName) continue;

            var attr = method.GetCustomAttributes(attributeType, false).FirstOrDefault() as MustHandleMemberAttribute;
            if (attr != null && (!kind.HasValue || attr.Kind == kind.Value))
            {
                yield return method.Name;
            }
        }
    }

    /// <summary>
    /// 获取Match方法的名称
    /// </summary>
    private static string GetMatchMethodName()
    {
        // 从扩展方法中查找标记为Match的方法
        var optionExtType = typeof(OptionExtensions);
        var methods = optionExtType.GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<MustHandleMemberAttribute>();
            if (attr?.Kind == MustHandleMemberKind.Match)
            {
                return method.Name;
            }
        }

        return "Match"; //  fallback
    }
}
