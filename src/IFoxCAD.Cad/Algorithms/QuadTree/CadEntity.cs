namespace IFoxCAD.Cad;

/// <summary>
/// 四叉树图元
/// </summary>
public class CadEntity : IHasRect, IComparable<CadEntity>
{
    public ObjectId ObjectId;
    public new bool IsPoint => Area == 0;
    public List<CadEntity>? Link;//碰撞链

    /// <summary>
    /// 四叉树图元
    /// </summary>
    /// <param name="objectId">图元id</param>
    /// <param name="box">包围盒</param>
    public CadEntity(ObjectId objectId, Rect box)
    {
        ObjectId = objectId;
        _X = box._X;
        _Y = box._Y;
        _Top = box._Top;
        _Right = box._Right;
    }
    public int CompareTo(CadEntity other)
    {
        return ObjectId.GetHashCode() ^ other.ObjectId.GetHashCode();
    }
}