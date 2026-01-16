namespace IFoxCAD.Cad;

/// <summary>
/// 相等运算符类
/// </summary>
public class OpEqual : OpFilter
{
    /// <summary>
    ///  组码与匹配值的TypedValue类型值
    /// </summary>
    public TypedValue Value { get; private set; }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Equal";

    /// <summary>
    /// 相等运算符类构造函数
    /// </summary>
    /// <param name="code">组码</param>
    public OpEqual(int code)
    {
        Value = new TypedValue(code);
    }

    /// <summary>
    /// 相等运算符类构造函数
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public OpEqual(int code, object value)
    {
        Value = new TypedValue(code, value);
    }

    /// <summary>
    /// 相等运算符类构造函数
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public OpEqual(DxfCode code, object value)
    {
        Value = new TypedValue((int)code, value);
    }

    /// <summary>
    /// 相等运算符类构造函数
    /// </summary>
    /// <param name="value">组码与组码值的TypedValue类型值</param>
    internal OpEqual(TypedValue value)
    {
        Value = value;
    }

    /// <summary>
    /// 过滤器数据迭代器
    /// </summary>
    /// <returns>TypedValue迭代器</returns>
    public override IEnumerable<TypedValue> GetValues()
    {
        yield return Value;
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="value">组码值</param>
    public void SetValue(object value)
    {
        Value = new TypedValue(Value.TypeCode, value);
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public void SetValue(int code, object value)
    {
        Value = new TypedValue(code, value);
    }
}
