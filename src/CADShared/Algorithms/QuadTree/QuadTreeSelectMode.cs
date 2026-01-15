#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记


namespace IFoxCAD.Cad;

/// <summary>
/// 四叉树选择模式
/// </summary>
public enum QuadTreeSelectMode
{
    IntersectsWith, // 碰撞到就选中
    Contains,       // 全包含才选中
}

/// <summary>
/// 四叉树查找方向
/// </summary>
public enum QuadTreeFindMode
{
    Top = 1,  // 上
    Bottom = 2,  // 下
    Left = 4,  // 左
    Right = 8,  // 右
}