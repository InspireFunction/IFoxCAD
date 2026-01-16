namespace IFoxCAD.Cad;

/// <summary>
/// 过滤器逻辑运算符抽象类
/// </summary>
public abstract class OpLogi : OpFilter, IEnumerable<OpFilter>
{
    /// <summary>
    /// 返回-4组码的开始内容
    /// </summary>
    public TypedValue First => new(-4, $"<{Name}");

    /// <summary>
    /// 返回-4组码的结束内容
    /// </summary>
    public TypedValue Last => new(-4, $"{Name}>");

    /// <summary>
    /// 获取过滤条件
    /// </summary>
    /// <returns>TypedValue迭代器</returns>
    //[System.Diagnostics.DebuggerStepThrough]
    public override IEnumerable<TypedValue> GetValues()
    {
        yield return First;
        foreach (var item in this)
        {
            foreach (var value in item.GetValues())
                yield return value;
        }
        yield return Last;
    }

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>OpFilter迭代器</returns>
    [DebuggerStepThrough]
    public abstract IEnumerator<OpFilter> GetEnumerator();

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
/// 逻辑非类
/// </summary>
public class OpNot : OpLogi
{
    private OpFilter Value { get; }

    /// <summary>
    /// 逻辑非类构造函数
    /// </summary>
    /// <param name="value">OpFilter数据</param>
    public OpNot(OpFilter value)
    {
        Value = value;
    }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Not";

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>OpFilter迭代器</returns>
    [DebuggerStepThrough]
    public override IEnumerator<OpFilter> GetEnumerator()
    {
        yield return Value;
    }
}

/// <summary>
/// 逻辑异或类
/// </summary>
public class OpXor : OpLogi
{
    /// <summary>
    /// 左操作数
    /// </summary>
    public OpFilter Left { get; }

    /// <summary>
    /// 右操作数
    /// </summary>
    public OpFilter Right { get; }

    /// <summary>
    /// 逻辑异或类构造函数
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    public OpXor(OpFilter left, OpFilter right)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Xor";

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>选择集过滤器类型迭代器</returns>
    [DebuggerStepThrough]
    public override IEnumerator<OpFilter> GetEnumerator()
    {
        yield return Left;
        yield return Right;
    }
}