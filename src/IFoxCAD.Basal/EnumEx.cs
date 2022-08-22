namespace IFoxCAD.Basal;

using System.ComponentModel;
using System.Linq;

public static class EnumEx
{
    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    public static string? PrintNote(this Enum e, bool noDescrToString = true)
    {
        var hash = GetAttribute<DescriptionAttribute>(e, noDescrToString);
        if (hash != null)
            return string.Join("|", hash.ToArray());
        return null;
    }

    //缓存
    static readonly Dictionary<string, HashSet<string>> _enumDict = new();

    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    /// <param name="e">枚举</param>
    /// <returns>注释内容</returns>
    public static HashSet<string>? GetAttribute<T>(this Enum e, bool noDescrToString = true)
        where T : DescriptionAttribute
    {
        var eType = e.GetType();
        string eFullName = GetDisplayName(e);

        if (_enumDict.ContainsKey(eFullName))
            return _enumDict[eFullName];

        var fieldInfo = eType.GetField(Enum.GetName(eType, e));
        if (fieldInfo == null)
            return null!;

        var attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(T)) as T;
        if (attribute != null)
        {
            var res = new HashSet<string>() { attribute.Description };
            _enumDict.Add(eFullName, res);
            return res;
        }

        //如果就是空的,就尝试去遍历所有的字段进行组合
        List<Enum> enums = new();
        foreach (Enum item in Enum.GetValues(eType)) //遍历这个枚举类型
        {
            if ((e.GetHashCode() & item.GetHashCode()) == item.GetHashCode() &&
                 e.GetHashCode() != item.GetHashCode())
                enums.Add(item);
        }

        HashSet<string> eNames = new();
        if (enums.Count > 0)
        {
            for (int i = 0; i < enums.Count; i++)
            {
                var key = enums[i];
                var tname = GetDisplayName(key);
                if (_enumDict.ContainsKey(tname))
                {
                    foreach (var item in _enumDict[tname])
                        eNames.Add(item);
                }
                else
                {
                    var get = GetAttribute<T>(key, noDescrToString);
                    if (get != null)
                        foreach (var item in get)  //没有缓存就递归
                            eNames.Add(item);
                }
            }
        }

        if (eNames.Count == 0 && noDescrToString)
            eNames.Add(e.ToString());

        _enumDict.Add(eFullName, eNames);
        return eNames;
    }

    static string GetDisplayName(Enum e)
    {
        return e.GetType().FullName + "." + e.ToString();
    }


    //这个完全被上面替代了
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

