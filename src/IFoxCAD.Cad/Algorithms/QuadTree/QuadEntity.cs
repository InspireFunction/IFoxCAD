namespace IFoxCAD.Cad;

/// <summary>
/// 四叉树图元
/// </summary>
public class QuadEntity : IHasRect, IComparable<QuadEntity>
{
    public ObjectId ObjectId;
    public new bool IsPoint => Area == 0;
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