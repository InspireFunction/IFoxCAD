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
    public static DwgFilerEx? DeserializeObject(string json)
    {
#if NewtonsoftJson
        return JsonConvert.DeserializeObject<DwgFilerEx>(json);
#else
        JavaScriptSerializer serializer = new();
        serializer.RegisterConverters(new[] { new ObjectIdConverter() });
        return serializer.Deserialize<DwgFilerEx>(json);
#endif
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public string SerializeObject()
    {
#if NewtonsoftJson
        return JsonConvert.SerializeObject(DwgFiler, Formatting.Indented);
#else
        JavaScriptSerializer serializer = new();
        serializer.RegisterConverters(new[] { new ObjectIdConverter() });
        return serializer.Serialize(DwgFiler);
#endif
    }

    public override string ToString()
    {
        return DwgFiler.ToString();
    }

    public static implicit operator Cad_DwgFiler(DwgFilerEx df)
    {
        return df.DwgFiler;
    }
    #endregion
}