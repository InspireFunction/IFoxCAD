namespace IFoxCAD.Basal;

using System.ComponentModel;

public static class EnumEx
{
    //缓存
    static readonly Dictionary<Enum, string?> _enumDict = new();

    /// <summary>
    /// 打印枚举的特性<see cref="DescriptionAttribute"/>注释内容
    /// </summary>
    /// <param name="e">枚举</param>
    /// <returns>注释内容</returns>
    public static string? PrintDescription(this Enum e)
    {
        if (_enumDict.ContainsKey(e))
            return _enumDict[e];

        var eType = e.GetType();
        var fieldInfo = eType.GetField(Enum.GetName(eType, e));
        if (fieldInfo == null)
            return null;

        var attribute = Attribute.GetCustomAttribute(fieldInfo,
               typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            _enumDict.Add(e, attribute.Description);
            return attribute.Description;
        }

        //如果就是空的,就尝试去遍历所有的字段进行组合
        List<Enum> enums = new();
        foreach (Enum item in Enum.GetValues(eType)) //遍历这个枚举类型
        {
            if ((e.GetHashCode() & item.GetHashCode()) == item.GetHashCode() &&
                 e.GetHashCode() != item.GetHashCode())
                enums.Add(item);
        }

        string? result = null;
        if (enums.Count > 0)
        {
            List<string> eNames = new();
            for (int i = 0; i < enums.Count; i++)
                if (_enumDict.ContainsKey(enums[i]))
                    eNames.Add(_enumDict[enums[i]]!);

            result = string.Join("|", eNames.ToArray());
        }
        _enumDict.Add(e, result);
        return _enumDict[e];
    }
}