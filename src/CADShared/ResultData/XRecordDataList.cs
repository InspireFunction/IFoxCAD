namespace IFoxCAD.Cad;

/// <summary>
/// 扩展字典数据封装类
/// </summary>
public class XRecordDataList : TypedValueList
{
    #region 构造函数

    /// <summary>
    /// 扩展字典数据封装类
    /// </summary>
    public XRecordDataList()
    {
    }

    /// <summary>
    /// 扩展字典数据封装类
    /// </summary>
    public XRecordDataList(IEnumerable<TypedValue> values) : base(values)
    {
    }

    #endregion

    #region 添加数据

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="obj">组码值</param>
    public override void Add(int code, object obj)
    {
        if (code >= 1000)
            throw new Exception("传入的组码值不是 XRecordData 有效范围！");

        Add(new TypedValue(code, obj));
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="code">dxfcode枚举值</param>
    /// <param name="obj">组码值</param>
    public void Add(DxfCode code, object obj)
    {
        Add((int)code, obj);
    }

    #endregion

    #region 转换器

    /// <summary>
    /// ResultBuffer 隐式转换到 XRecordDataList
    /// </summary>
    /// <param name="buffer">ResultBuffer 实例</param>
    public static implicit operator XRecordDataList(ResultBuffer? buffer) =>
        buffer is null ? [] : new(buffer.AsArray());

    /// <summary>
    /// XRecordDataList 隐式转换到 TypedValue 数组
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>
    public static implicit operator TypedValue[](XRecordDataList values) => values.ToArray();

    /// <summary>
    /// XRecordDataList 隐式转换到 ResultBuffer
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>s
    public static implicit operator ResultBuffer(XRecordDataList values) => new(values);

    /// <summary>
    /// TypedValue 数组隐式转换到 XRecordDataList
    /// </summary>
    /// <param name="values">TypedValue 数组</param>
    public static implicit operator XRecordDataList(TypedValue[] values) => new(values);

    #endregion
}