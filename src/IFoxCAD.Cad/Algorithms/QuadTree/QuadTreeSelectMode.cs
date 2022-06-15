namespace IFoxCAD.Cad
{
    /// <summary>
    /// 四叉树选择模式
    /// </summary>
    public enum QuadTreeSelectMode
    {
        IntersectsWith, //碰撞到就选中
        Contains,       //全包含才选中
    }

    /// <summary>
    /// 四叉树查找方向
    /// </summary>
    public enum QuadTreeFindMode
    {
        Top    = 1,  //上
        Bottom = 2,  //下
        Left   = 4,  //左
        Right  = 8,  //右
    }
}