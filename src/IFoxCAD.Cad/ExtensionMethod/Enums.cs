namespace IFoxCAD.Cad;

/// <summary>
/// 坐标系类型枚举
/// </summary>
public enum CoordinateSystemCode
{
    /// <summary>
    /// 世界坐标系
    /// </summary>
    Wcs = 0,

    /// <summary>
    /// 用户坐标系
    /// </summary>
    Ucs,

    /// <summary>
    /// 模型空间坐标系
    /// </summary>
    MDcs,

    /// <summary>
    /// 图纸空间坐标系
    /// </summary>
    PDcs
}

/// <summary>
/// 方向的枚举
/// </summary>
public enum OrientationType
{
    /// <summary>
    /// 左转或逆时针
    /// </summary>
    CounterClockWise,
    /// <summary>
    /// 右转或顺时针
    /// </summary>
    ClockWise,
    /// <summary>
    /// 重合或平行
    /// </summary>
    Parallel
}

/// <summary>
/// 点与多边形的关系类型枚举
/// </summary>
public enum PointOnRegionType
{
    /// <summary>
    /// 多边形内部
    /// </summary>
    Inside,

    /// <summary>
    /// 多边形上
    /// </summary>
    On,

    /// <summary>
    /// 多边形外
    /// </summary>
    Outside,

    /// <summary>
    /// 错误
    /// </summary>
    Error
}



public enum FontTTF
{
    [Description("宋体.ttf")]
    宋体,
    [Description("simfang.ttf")]
    仿宋,
    [Description("FSGB2312.ttf")]
    仿宋GB2312,
    [Description("Arial.ttf")]
    Arial,
    [Description("Romans")]
    Romans
}



public static class EnumHelper
{
    public static string GetDesc(this Enum val)
    {
        var type = val.GetType();
        var memberInfo = type.GetMember(val.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        //如果没有定义描述，就把当前枚举值的对应名称返回
        if (attributes is null || attributes.Length != 1)
        {
            return val.ToString();
        }
        return ((DescriptionAttribute)attributes.Single()).Description;
    }
}

