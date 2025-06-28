namespace IFoxCAD.Cad;

/// <summary>
/// RXClass扩展
/// </summary>
public static class RXClassEx
{
    /// <summary>
    /// 获取RXClass
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <returns><see cref="RXClass"/></returns>
    public static RXClass Get<T>() where T : DBObject
    {
        var type = typeof(T);
        if (!_dict.TryGetValue(type, out var rxClass))
        {
            _dict[type] = rxClass = RXObject.GetClass(type);
        }

        return rxClass;
    }

    /// <summary>
    /// 由于RXObject.GetClass速度极慢，采用内部字典优化速度
    /// </summary>
    private static readonly Dictionary<Type, RXClass> _dict = [];
}