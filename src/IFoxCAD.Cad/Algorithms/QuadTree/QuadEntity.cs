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
    /// <summary>
    /// cad图元id
    /// </summary>
    public ObjectId ObjectId;

    /// <summary>
    /// 是一个点
    /// </summary>
    /// 面积是0不一定是点,所以需要这样判断,
    /// 因为可能是水平或者垂直的直线,没有斜率的时候是包围盒面积是0
    public bool IsPoint => Math.Abs(_X - _Right) < 1e-10 && Math.Abs(_Y - _Top) < 1e-10;

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

    public int CompareTo(QuadEntity? other)
    {
        if (other == null)
            return -1;

        return (base.GetHashCode(), IsPoint).GetHashCode() ^ other.GetHashCode();
    }
}