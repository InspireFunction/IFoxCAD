namespace IFoxCAD.Cad;

/// <summary>
/// lisp数据封装类
/// </summary>
public class LispList : TypedValueList
{
    #region 构造函数
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public LispList() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="values">TypedValue 迭代器</param>
    public LispList(IEnumerable<TypedValue> values) : base(values) { }
    #endregion
    /// <summary>
    /// lisp 列表的值
    /// </summary>
    public virtual List<TypedValue> Value
    {
        get
        {
            var value = new List<TypedValue>
                {
                    new TypedValue((int)LispDataType.ListBegin,-1),
                    new TypedValue((int)LispDataType.ListEnd,-1)
                };
            value.InsertRange(1, this);
            return value;
        }
    }

    #region 添加数据
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="code">组码</param>
    /// <param name="obj">组码值</param>
    public override void Add(int code, object? obj)
    {
        if (code < 5000)
        {
            throw new System.Exception("传入的组码值不是 lisp数据 有效范围！");
        }
        Add(new TypedValue(code, obj));
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="code">dxfcode枚举值</param>
    /// <param name="obj">组码值</param>
    public void Add(LispDataType code, object? obj)
    {
        Add((int)code, obj);
    }
    /// <summary>
    /// 添加数据，参数为true时添加 lisp 中的 T，false时添加 lisp 中的 nil
    /// </summary>
    /// <param name="value">bool 型的数据</param>
    public void Add(bool value)
    {
        if (value)
        {
            Add(LispDataType.T_atom, true);
        }
        else
        {
            Add(LispDataType.Nil, null);
        }
    }
    /// <summary>
    /// 添加字符串
    /// </summary>
    /// <param name="value">字符串</param>
    public void Add(string value)
    {
        Add(LispDataType.Text, value);
    }
    /// <summary>
    /// 添加短整型数
    /// </summary>
    /// <param name="value">短整型数</param>
    public void Add(short value)
    {
        Add(LispDataType.Int16, value);
    }
    /// <summary>
    /// 添加整型数
    /// </summary>
    /// <param name="value">整型数</param>
    public void Add(int value)
    {
        Add(LispDataType.Int32, value);
    }
    /// <summary>
    /// 添加浮点数
    /// </summary>
    /// <param name="value">浮点数</param>
    public void Add(double value)
    {
        Add(LispDataType.Double, value);
    }
    /// <summary>
    /// 添加对象id
    /// </summary>
    /// <param name="value">对象id</param>
    public void Add(ObjectId value)
    {
        Add(LispDataType.ObjectId, value);
    }
    /// <summary>
    /// 添加选择集
    /// </summary>
    /// <param name="value">选择集</param>
    public void Add(SelectionSet value)
    {
        Add(LispDataType.SelectionSet, value);
    }
    /// <summary>
    /// 添加二维点
    /// </summary>
    /// <param name="value">二维点</param>
    public void Add(Point2d value)
    {
        Add(LispDataType.Point2d, value);
    }
    /// <summary>
    /// 添加三维点
    /// </summary>
    /// <param name="value">三维点</param>
    public void Add(Point3d value)
    {
        Add(LispDataType.Point3d, value);
    }
    /// <summary>
    /// 添加二维点
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    public void Add(double x, double y)
    {
        Add(LispDataType.Point2d, new Point2d(x, y));
    }
    /// <summary>
    /// 添加三维点
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    public void Add(double x, double y, double z)
    {
        Add(LispDataType.Point3d, new Point3d(x, y, z));
    }
    /// <summary>
    /// 添加列表
    /// </summary>
    /// <param name="value">lisp 列表</param>
    public void Add(LispList value)
    {
        this.AddRange(value.Value);
    }

    #endregion

    #region 转换器
    /// <summary>
    /// ResultBuffer 隐式转换到 LispList
    /// </summary>
    /// <param name="buffer">ResultBuffer 实例</param>
    public static implicit operator LispList(ResultBuffer buffer) => new(buffer.AsArray());
    /// <summary>
    /// LispList 隐式转换到 TypedValue 数组
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>
    public static implicit operator TypedValue[](LispList values) => values.Value.ToArray();
    /// <summary>
    /// LispList 隐式转换到 ResultBuffer
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>
    public static implicit operator ResultBuffer(LispList values) => new(values.Value.ToArray());
    /// <summary>
    /// TypedValue 数组隐式转换到 LispList
    /// </summary>
    /// <param name="values">TypedValue 数组</param>
    public static implicit operator LispList(TypedValue[] values) => new(values);
    #endregion
}
