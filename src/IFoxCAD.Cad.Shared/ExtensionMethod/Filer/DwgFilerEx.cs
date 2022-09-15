namespace IFoxCAD.Cad;

/// <summary>
/// Dwg序列化
/// </summary>
public class DwgFilerEx
{
    #region 成员
    DBObject? _entity;
    public DwgFiler DwgFiler { get; private set; }
    #endregion

    #region 构造
    /// <summary>
    /// Dwg序列化
    /// </summary>
    public DwgFilerEx(DwgFiler? Cad_DwgFiler = null)
    {
        if (Cad_DwgFiler == null)
            Cad_DwgFiler = new();
        DwgFiler = Cad_DwgFiler;
    }

    /// <summary>
    /// Dwg序列化
    /// </summary>
    public DwgFilerEx(DBObject entity) : this()
    {
        DwgOut(entity);
    }

    #endregion

    #region 方法
    public void DwgOut(DBObject entity)
    {
        _entity = entity;
        _entity.DwgOut(DwgFiler);
    }

    public void DwgIn()
    {
        _entity?.DwgIn(DwgFiler);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public DwgFilerEx? DeserializeObject(string json)
    {
        throw new ArgumentException();
        //return JsonConvert.DeserializeObject<DwgFilerEx>(json);// 反序列化*字符串转类
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public string SerializeObject()
    {
        throw new ArgumentException();
        //return JsonConvert.SerializeObject(DwgFiler, Formatting.Indented);  // 序列化*类转字符串
    }

    public override string ToString()
    {
        // 替换中括号以外的字符串,替换逗号为换行符 https://bbs.csdn.net/topics/370134253
        //var str = SerializeObject();
        //str = str.Substring(1, str.Length - 2);
        //str = Regex.Replace(str, @"(?:,)(?![^\[]*?\])", "\r\n");
        //return str;

        return DwgFiler.ToString();
    }

    public static implicit operator Cad_DwgFiler(DwgFilerEx df)
    {
        return df.DwgFiler;
    }
    #endregion
}