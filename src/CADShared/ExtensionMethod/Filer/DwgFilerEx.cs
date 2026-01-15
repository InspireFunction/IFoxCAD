namespace IFoxCAD.Cad;

/// <summary>
/// Dwg序列化
/// </summary>
public class DwgFilerEx
{
    #region 成员
    DBObject? _entity;
    /// <summary>
    /// DWG文件操作器
    /// </summary>
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
    /// <summary>
    /// 将实体数据写出到DWG文件
    /// </summary>
    /// <param name="entity">要写出的实体对象</param>
    public void DwgOut(DBObject entity)
    {
        _entity = entity;
        _entity.DwgOut(DwgFiler);
    }

    /// <summary>
    /// 从DWG文件读取实体数据
    /// </summary>
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
        var settings = new MyJsonSettings
        {
            Converters = new List<MyJsonConverter> { new ObjectIdConverter() }
        };
        return MyJson.DeserializeObject<DwgFilerEx>(json, settings);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public string SerializeObject()
    {
        var settings = new MyJsonSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<MyJsonConverter> { new ObjectIdConverter() }
        };
        return MyJson.SerializeObject(DwgFiler, settings);
    }

    /// <summary>
    /// 返回表示当前对象的字符串
    /// </summary>
    /// <returns>表示当前对象的字符串</returns>
    public override string ToString()
    {
        return DwgFiler.ToString();
    }

    /// <summary>
    /// 隐式转换运算符，将DwgFilerEx转换为Cad_DwgFiler
    /// </summary>
    /// <param name="df">DwgFilerEx实例</param>
    /// <returns>Cad_DwgFiler实例</returns>
    public static implicit operator Cad_DwgFiler(DwgFilerEx df)
    {
        return df.DwgFiler;
    }
    #endregion
}