namespace IFoxCAD.Cad;

/// <summary>
/// 逻辑操作符的列表抽象类
/// </summary>
public abstract class OpList : OpLogi
{
    /// <summary>
    /// 过滤器列表
    /// </summary>
    protected readonly List<OpFilter> Lst = [];

    /// <summary>
    /// 添加过滤器条件的虚函数，子类可以重写
    /// </summary>
    /// <example>举个利用这个类及其子类创建选择集过滤的例子
    /// <code>
    /// <![CDATA[
    /// var fd = new OpOr
    ///          {
    ///              !new OpAnd
    ///              {
    ///                  { 0, "line" },
    ///                  { 8, "0" },
    ///              },
    ///              new OpAnd
    ///              {
    ///                  !new OpEqual(0, "circle"),
    ///                  { 8, "2" },
    ///                 { 10, new Point3d(10,10,0), ">,>,*" }
    ///              },
    ///          };
    /// ]]>
    /// </code></example>
    /// <param name="value">过滤器对象</param>
    public virtual void Add(OpFilter value)
    {
        Lst.Add(value);
    }

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="speccode">逻辑非~</param>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public void Add(string speccode, int code, object value)
    {
        if (speccode == "~")
            Lst.Add(new OpEqual(code, value).Not);
    }

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public void Add(int code, object value)
    {
        Lst.Add(new OpEqual(code, value));
    }

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    public void Add(DxfCode code, object value)
    {
        Lst.Add(new OpEqual(code, value));
    }

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    /// <param name="comp">比较运算符</param>
    public void Add(int code, object value, string comp)
    {
        Lst.Add(new OpComp(comp, code, value));
    }

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="value">组码值</param>
    /// <param name="comp">比较运算符</param>
    public void Add(DxfCode code, object value, string comp)
    {
        Lst.Add(new OpComp(comp, code, value));
    }

    /// <summary>
    /// 过滤器迭代器
    /// </summary>
    /// <returns>OpFilter迭代器</returns>
    [DebuggerStepThrough]
    public override IEnumerator<OpFilter> GetEnumerator()
    {
        return Lst.GetEnumerator();
    }
}

/// <summary>
/// 逻辑与类
/// </summary>
public class OpAnd : OpList
{
    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "And";

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="value">过滤器对象</param>
    public override void Add(OpFilter value)
    {
        if (value is OpAnd opand)
        {
            foreach (var item in opand)
                Lst.Add(item);
        }
        else
        {
            Lst.Add(value);
        }
    }
}

/// <summary>
/// 逻辑或类
/// </summary>
public class OpOr : OpList
{
    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Or";

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="value">过滤器对象</param>
    public override void Add(OpFilter value)
    {
        if (value is OpOr opor)
        {
            foreach (var item in opor)
                Lst.Add(item);
        }
        else
        {
            Lst.Add(value);
        }
    }
}