namespace IFoxCAD.Basal;

/// <summary>
/// 枚举扩展
/// </summary>
public static class EnumEx
{
    /// <summary>
    /// 清理缓存
    /// </summary>
    public static void CleanCache()
    {
        Cache.Clear();
    }

    // (类型完整名,描述组合)
    private static readonly Dictionary<string, HashSet<string>> Cache = [];

    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    /// <param name="e">枚举</param>
    /// <param name="noDescrToString"></param>
    /// <returns>注释内容</returns>
    public static HashSet<string>? GetAttribute<T>(this Enum e, bool noDescrToString = true)
        where T : DescriptionAttribute
    {
        var eType = e.GetType();
        var eFullName = eType.FullName + "." + e;

        if (Cache.TryGetValue(eFullName, out var attribute1))
            return attribute1;

        var fieldInfo = eType.GetField(Enum.GetName(eType, e) ?? string.Empty);
        if (fieldInfo == null)
            return null!;

        // 注释存放的容器
        HashSet<string> nodes = [];
        if (Attribute.GetCustomAttribute(fieldInfo, typeof(T)) is T attribute)
        {
            nodes.Add(attribute.Description);
            Cache.Add(eFullName, nodes);
            return nodes;
        }

        // 通常到这里的就是 ALL = A | B | C
        // 遍历所有的枚举,组合每个注释
        List<Enum> enumHas = [];
        enumHas.AddRange(Enum.GetValues(eType).Cast<Enum>().Where(em =>
            (e.GetHashCode() & em.GetHashCode()) == em.GetHashCode() && e.GetHashCode() != em.GetHashCode()));

        // 遍历这个枚举类型,获取枚举按位包含的成员


        // 采取的行为是:注释的行为是特殊的,就按照注释的,否则,遍历子元素提取注释
        // 大的在前面才能判断是否按位包含后面的,后面的就是要移除的
        enumHas = [.. enumHas.OrderByDescending(a => a.GetHashCode())];
        ArrayEx.Deduplication(enumHas, (a, b) => (a.GetHashCode() & b.GetHashCode()) == b.GetHashCode());

        // 逆序仅仅为排序后处理,不一定和书写顺序一样,尤其是递归可能存在重复的元素
        for (var i = enumHas.Count - 1; i >= 0; i--)
        {
            var atts = GetAttribute<T>(enumHas[i], noDescrToString);// 递归
            if (atts == null)
                continue;
            foreach (var item in atts)
                nodes.Add(item);
        }

        if (nodes.Count == 0 && noDescrToString)
            nodes.Add(e.ToString());

        Cache.Add(eFullName, nodes);
        return nodes;
    }

    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    public static string? PrintNote(this Enum e, bool noDescToString = true)
    {
        var hash = GetAttribute<DescriptionAttribute>(e, noDescToString);
        return hash == null ? null : string.Join("|", [.. hash]);
    }
    
    /// <summary>
    /// 获取枚举的描述内容
    /// </summary>
    /// <remarks>不按位运算的情况下,直接获取比较快捷</remarks>
    /// <param name="e"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum e)
    {
        return GetDescription(e.GetType(), e.ToString());
    }

    /// <summary>
    /// 获取字段的描述内容
    /// </summary>
    /// <param name="type"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static string GetDescription(this Type type, string field)
    {
        var memberInfo = type.GetMember(field);
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        // 如果没有定义描述,就把当前枚举值的对应名称返回
        return attributes.Length != 1 ? field : ((DescriptionAttribute)attributes.Single()).Description;
    }
}