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
/// <summary>
/// 符号表模式
/// </summary>
[Flags]
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
    /// <summary>
    /// 图层|字体|标注|线型|应用
    /// </summary>
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
    /// <summary>
    /// 坐标|视口|视图
    /// </summary>
    Option2 = UcsTable | ViewTable | ViewportTable,

    /// <summary>
    /// 全部
    /// </summary>
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


/// <summary>
/// ttf字体枚举
/// </summary>
public enum FontTTF
{
    /// <summary>
    /// 宋体
    /// </summary>
    [Description("宋体.ttf")]
    宋体,
    /// <summary>
    /// 仿宋
    /// </summary>
    [Description("simfang.ttf")]
    仿宋,
    /// <summary>
    /// 仿宋GB2312
    /// </summary>
    [Description("FSGB2312.ttf")]
    仿宋GB2312,
    /// <summary>
    /// Arial
    /// </summary>
    [Description("Arial.ttf")]
    Arial,
    /// <summary>
    /// Romans
    /// </summary>
    [Description("Romans")]
    Romans
}