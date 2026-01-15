namespace IFoxCAD.Cad;

/// <summary>
/// 参照路径转换
/// </summary>
public enum PathConverterModes : byte
{
    /// <summary>
    /// 相对路径
    /// </summary>
    Relative,
    /// <summary>
    /// 绝对路径
    /// </summary>
    Complete
}

/// <summary>
/// 参照绑定
/// </summary>
public enum XrefModes : byte
{
    /// <summary>
    /// 卸载
    /// </summary>
    Unload,
    /// <summary>
    /// 重载
    /// </summary>
    Reload,
    /// <summary>
    /// 拆离
    /// </summary>
    Detach,
    /// <summary>
    /// 绑定
    /// </summary>
    Bind,
}

public enum SymModes : ushort
{
    /// <summary>
    /// 块表
    /// </summary>
    BlockTable = 1,

    /// <summary>
    /// 图层表
    /// </summary>
    LayerTable = 2,
    /// <summary>
    /// 文字样式表
    /// </summary>
    TextStyleTable = 4,
    /// <summary>
    /// 注册应用程序表
    /// </summary>
    RegAppTable = 8,
    /// <summary>
    /// 标注样式表
    /// </summary>
    DimStyleTable = 16,
    /// <summary>
    /// 线型表
    /// </summary>
    LinetypeTable = 32,
    Option1 = LayerTable | TextStyleTable | DimStyleTable | LinetypeTable | RegAppTable,

    /// <summary>
    /// 用户坐标系表
    /// </summary>
    UcsTable = 64,
    /// <summary>
    /// 视图表
    /// </summary>
    ViewTable = 128,
    /// <summary>
    /// 视口表
    /// </summary>
    ViewportTable = 256,
    Option2 = UcsTable | ViewTable | ViewportTable,

    // 全部
    All = BlockTable | Option1 | Option2
}


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