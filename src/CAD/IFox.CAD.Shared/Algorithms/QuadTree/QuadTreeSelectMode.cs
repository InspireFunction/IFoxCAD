namespace IFoxCAD.Cad;

/// <summary>
/// 四叉树选择模式
/// </summary>
public enum QuadTreeSelectMode
{
    /// <summary>
    /// 碰撞到就选中
    /// </summary>
    IntersectsWith, 
    /// <summary>
    /// 全包含才选中
    /// </summary>
    Contains,     
}

/// <summary>
/// 四叉树查找方向
/// </summary>
public enum QuadTreeFindMode
{
    /// <summary>
    /// 上
    /// </summary>
    Top    = 1,  
    /// <summary>
    /// 下
    /// </summary>
    Bottom = 2,  
    /// <summary>
    /// 左
    /// </summary>
    Left   = 4, 
    /// <summary>
    /// 右
    /// </summary>
    Right  = 8,  
}