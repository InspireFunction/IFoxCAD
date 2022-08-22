namespace IFoxCAD.Basal;

using System.ComponentModel;
using System.Linq;

public static class EnumEx
{
    /// <summary>
    /// 清理缓存
    /// </summary>
    public static void CleanCache()
    {
        _cache.Clear();
    }
     
    //(类型完整名,描述组合)
    static readonly Dictionary<string, HashSet<string>> _cache = new();

    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    /// <param name="e">枚举</param>
    /// <returns>注释内容</returns>
    public static HashSet<string>? GetAttribute<T>(this Enum e, bool noDescrToString = true)
        where T : DescriptionAttribute
    {
        var eType = e.GetType();
        string eFullName = eType.FullName + "." + e.ToString();

        if (_cache.ContainsKey(eFullName))
            return _cache[eFullName];

        var fieldInfo = eType.GetField(Enum.GetName(eType, e));
        if (fieldInfo == null)
            return null!;

        if (Attribute.GetCustomAttribute(fieldInfo, typeof(T)) is T attribute)
        {
            var res = new HashSet<string>() { attribute.Description };
            _cache.Add(eFullName, res);
            return res;
        }

        //注释存放的容器
        HashSet<string> nodes = new();

        //通常到这里的就是 ALL = A | B | C
        //遍历所有的枚举,组合每个注释
        List<Enum> enumHas = new();
        foreach (Enum enumItem in Enum.GetValues(eType)) //遍历这个枚举类型
        {
            if ((e.GetHashCode() & enumItem.GetHashCode()) == enumItem.GetHashCode() &&
                 e.GetHashCode() != enumItem.GetHashCode())
                enumHas.Add(enumItem);
        }
        for (int i = 0; i < enumHas.Count; i++)
        {
            var atts = GetAttribute<T>(enumHas[i], noDescrToString);//递归
            if (atts == null)
                continue;
            foreach (var item in atts)
                nodes.Add(item);//递归时候可能存在重复的元素
        }

        if (nodes.Count == 0 && noDescrToString)
            nodes.Add(e.ToString());

        _cache.Add(eFullName, nodes);
        return nodes;
    }
  
    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    public static string? PrintNote(this Enum e, bool noDescToString = true)
    {
        var hash = GetAttribute<DescriptionAttribute>(e, noDescToString);
        if (hash != null)
            return string.Join("|", hash.ToArray());
        return null;
    }


    //TODO 山人审核代码之后可以删除,这个完全被上面替代了
    public static string GetDesc(this Enum val)
    {
        var type = val.GetType();
        var memberInfo = type.GetMember(val.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        //如果没有定义描述,就把当前枚举值的对应名称返回
        if (attributes is null || attributes.Length != 1)
            return val.ToString();

        return ((DescriptionAttribute)attributes.Single()).Description;
    }
}