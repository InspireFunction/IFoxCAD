namespace IFoxCAD.Cad;

/// <summary>
/// 扩展数据封装类
/// </summary>
public class XDataList : TypedValueList
{
    #region 构造函数

    /// <summary>
    /// 扩展数据封装类
    /// </summary>
    public XDataList()
    {
    }

    /// <summary>
    /// 扩展数据封装类
    /// </summary>
    public XDataList(IEnumerable<TypedValue> values) : base(values)
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
        if (code is < 1000 or > 1071)
            throw new Exception("传入的组码值不是 XData 有效范围!");

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

    /// <summary>
    /// 是否含有注册名
    /// </summary>
    /// <param name="appName">注册名</param>
    public bool Contains(string appName)
    {
        var result = false;
        RangeTask(appName, (_, state, _) =>
        {
            result = true;
            state.Break();
        });
        return result;
    }

    /// <summary>
    /// 注册名下含有指定成员
    /// </summary>
    /// <param name="appName">注册名</param>
    /// <param name="value">内容</param>
    public bool Contains(string appName, object value)
    {
        var result = false;
        RangeTask(appName, (tv, state, _) =>
        {
            if (tv.Value.Equals(value))
            {
                result = true;
                state.Break();
            }
        });
        return result;
    }

    /// <summary>
    /// 获取appName的索引区间
    /// </summary>
    /// <param name="appName">注册名称</param>
    /// <param name="dxfCodes">任务组码对象</param>
    /// <returns>返回任务组码的索引</returns>
    public List<int> GetXdataAppIndex(string appName, DxfCode[] dxfCodes)
    {
        List<int> indexes = [];
        RangeTask(appName, (tv, _, i) =>
        {
            if (dxfCodes.Contains((DxfCode)tv.TypeCode))
                indexes.Add(i);
        });
        return indexes;
    }

    /// <summary>
    /// 区间任务
    /// </summary>
    /// <param name="appName"></param>
    /// <param name="action"></param>
    private void RangeTask(string appName, Action<TypedValue, LoopState, int> action)
    {
        LoopState state = new();
        // 在名称和名称之间找
        var appNameIndex = -1;
        for (var i = 0; i < Count; i++)
        {
            if (this[i].TypeCode == (short)DxfCode.ExtendedDataRegAppName)
            {
                if (this[i].Value.ToString() == appName)
                {
                    appNameIndex = i;
                    continue;
                }

                if (appNameIndex != -1) //找到了下一个名称
                    break;
            }

            if (appNameIndex == -1) continue; // 找下一个的时候,获取任务(移除)的对象
            action(this[i], state, i);
            if (!state.IsRun)
                break;
        }
    }

    #endregion

    #region 转换器

    /// <summary>
    /// ResultBuffer 隐式转换到 XDataList
    /// </summary>
    /// <param name="buffer">ResultBuffer 实例</param>
    public static implicit operator XDataList(ResultBuffer? buffer) => new(buffer?.AsArray() ?? []);

    /// <summary>
    /// XDataList 隐式转换到 TypedValue 数组
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>
    public static implicit operator TypedValue[](XDataList values) => values.ToArray();

    /// <summary>
    /// XDataList 隐式转换到 ResultBuffer
    /// </summary>
    /// <param name="values">TypedValueList 实例</param>
    public static implicit operator ResultBuffer(XDataList values) => new(values);

    /// <summary>
    /// TypedValue 数组隐式转换到 XDataList
    /// </summary>
    /// <param name="values">TypedValue 数组</param>
    public static implicit operator XDataList(TypedValue[] values) => new(values);

    #endregion
}