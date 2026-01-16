namespace IFoxCAD.Cad;

/// <summary>
/// 比较运算符类
/// </summary>
public class OpComp : OpEqual
{
    /// <summary>
    /// 比较运算符，如：
    /// <code>"&lt;="</code>
    /// 以及合并比较运算符：
    /// <code>"&lt;=,&lt;=,="</code>
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Comp";

    /// <summary>
    /// 比较运算符类构造函数
    /// </summary>
    /// <param name="content">运算符</param>
    /// <param name="value">数据</param>
    public OpComp(string content, TypedValue value)
        : base(value)
    {
        Content = content;
    }

    /// <summary>
    /// 比较运算符类构造函数
    /// </summary>
    /// <param name="content">运算符</param>
    /// <param name="code">组码</param>
    public OpComp(string content, int code)
        : base(code)
    {
        Content = content;
    }

    /// <summary>
    /// 比较运算符类构造函数
    /// </summary>
    /// <param name="content">运算符</param>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public OpComp(string content, int code, object value)
        : base(code, value)
    {
        Content = content;
    }

    /// <summary>
    /// 比较运算符类构造函数
    /// </summary>
    /// <param name="content">运算符</param>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public OpComp(string content, DxfCode code, object value)
        : base(code, value)
    {
        Content = content;
    }

    /// <summary>
    /// 获取过滤器数据迭代器
    /// </summary>
    /// <returns>TypedValue迭代器</returns>
    public override IEnumerable<TypedValue> GetValues()
    {
        yield return new TypedValue(-4, Content);
        yield return Value;
    }
}