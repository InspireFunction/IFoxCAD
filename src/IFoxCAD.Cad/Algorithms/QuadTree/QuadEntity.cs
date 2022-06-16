namespace IFoxCAD.Cad;

/*
 * 这个类存在的意义是为了不暴露Rect类字段
 * 同时利用了Rect类字段的快速
 * 提供到外面去再继承
 */

/// <summary>
/// 四叉树图元
/// </summary>
public class QuadEntity : Rect, IComparable<QuadEntity>
{
    public ObjectId ObjectId;
    /// <summary>
    /// 是一个点
    /// </summary>
    public bool IsPoint => Area == 0;

    //public List<QuadEntity>? Link;//碰撞链...这里外面自己封装去

    /// <summary>
    /// 四叉树图元
    /// </summary>
    /// <param name="objectId">图元id</param>
    /// <param name="box">包围盒</param>
    public QuadEntity(ObjectId objectId, Rect box)
    {
        ObjectId = objectId;
        _X = box._X;
        _Y = box._Y;
        _Top = box._Top;
        _Right = box._Right;
    }
    public int CompareTo(QuadEntity other)
    {
        return ObjectId.GetHashCode() ^ other.ObjectId.GetHashCode();
    }
}