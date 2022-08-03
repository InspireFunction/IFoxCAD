namespace IFoxCAD.Cad;

public static class KeywordCollectionEx
{
    public enum KeywordName
    {
        GlobalName,
        LocalName,
        DisplayName,
    }

    /// <summary>
    /// 含有关键字
    /// </summary>
    /// <param name="collection">关键字集合</param>
    /// <param name="name">关键字</param>
    /// <param name="keywordName">关键字容器字段名</param>
    /// <returns>true含有</returns>
    public static bool Contains(this KeywordCollection collection, string name,
        KeywordName keywordName = KeywordName.GlobalName)
    {
        bool contains = false;
        switch (keywordName)
        {
            case KeywordName.GlobalName:
                for (int i = 0; i < collection.Count; i++)
                    if (collection[i].GlobalName == name)
                    {
                        contains = true;
                        break;
                    }
                break;
            case KeywordName.LocalName:
                for (int i = 0; i < collection.Count; i++)
                    if (collection[i].LocalName == name)
                    {
                        contains = true;
                        break;
                    }
                break;
            case KeywordName.DisplayName:
                for (int i = 0; i < collection.Count; i++)
                    if (collection[i].DisplayName == name)
                    {
                        contains = true;
                        break;
                    }
                break;
            default:
                break;
        }
        return contains;
    }

    /// <summary>
    /// 获取词典<see langword="(GlobalName"/>,<see langword="DisplayName)"/>
    /// <para>KeywordCollection是允许重复关键字的,没有哈希索引,在多次判断时候会遍历多次O(n),所以生成一个词典进行O(1)</para>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetDict(this KeywordCollection collection)
    {
        Dictionary<string, string> map = new();
        for (int i = 0; i < collection.Count; i++)
            map.Add(collection[i].GlobalName, collection[i].DisplayName);
        return map;
    }
}