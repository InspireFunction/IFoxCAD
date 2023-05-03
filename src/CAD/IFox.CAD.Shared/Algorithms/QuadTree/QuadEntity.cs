namespace IFoxCAD.Cad;

/*
 * 这个类存在的意义是为了不暴露Rect类字段
 * 同时利用了Rect类字段的快速
 * 提供到外面去再继承
 */

/// <summary>
/// 四叉树图元
/// </summary>
public class QuadEntity : Rect
{
    /// <summary>
    /// 四叉树图元
    /// </summary>
    /// <param name="box">包围盒</param>
    public QuadEntity(Rect box)
    {
        _X = box._X;
        _Y = box._Y;
        _Top = box._Top;
        _Right = box._Right;
    }
}