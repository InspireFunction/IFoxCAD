﻿#define Debug_Cause_Error
namespace IFoxCAD.Cad;

/// <summary>
/// AABB和OBB信息
/// </summary>
public struct BoundingInfo
{
    public double MinX;
    public double MinY;
    public double MinZ;

    public double MaxX;
    public double MaxY;
    public double MaxZ;

    /// <summary>
    /// AABB这里永远是0
    /// </summary>
    public double Angle;
    public bool IsEffective;

    public Point3d Min => new(MinX, MinY, MinZ);
    public Point3d Max => new(MaxX, MaxY, MaxZ);
    public Extents3d Extents3d => new(Min, Max);
    public Extents2d Extents2d => new(MinX, MinY, MaxX, MaxY);

    public BoundingInfo(double minX, double minY, double minZ,
                        double maxX, double maxY, double maxZ,
                        bool isEffective, double angle = 0)
    {
        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
        IsEffective = isEffective;
        Angle = angle;
    }

    public BoundingInfo(Point3d min, Point3d max, bool isEffective, double angle = 0)
     : this(min.X, min.Y, min.Z,
            max.X, max.Y, max.Z,
            isEffective, angle)
    { }

    // public BoundingInfo(Rect rect, double angle = 0)
    // {
    //    MinX = rect.X;
    //    MinY = rect.Y;
    //    MinZ = 0;
    //    MaxX = rect.Right;
    //    MaxY = rect.Top;
    //    MaxZ = 0;
    //    Angle = angle;
    // }

    public override string ToString()
    {
        return Extents3d.ToString();
    }

    public void Move(Point3d pt1, Point3d pt2)
    {
        var ve = pt1 - pt2;
        MinX -= ve.X;
        MinY -= ve.Y;
        MinZ -= ve.Z;
        MaxX -= ve.X;
        MaxY -= ve.Y;
        MaxZ -= ve.Z;
    }
}

public class EntityBoundingInfo
{
    #region 保存异常类型的日志
    
    static readonly HashSet<string> _typeNames;
    /// <summary>
    /// 为了保证更好的性能,
    /// 只是在第一次调用此功能的时候进行读取,
    /// 免得高频调用时候高频触发磁盘
    /// </summary>
    static EntityBoundingInfo()
    {
        _typeNames = new();
        
       
    }

    

    
    #endregion

    /// <summary>
    /// 获取图元包围盒
    /// </summary>
    /// <param name="ent"></param>
    /// <returns>(左下角,右上角,是否有效)</returns>
    /// 异常:
    ///   会将包围盒类型记录到所属路径中,以此查询
    public static BoundingInfo GetBoundingInfo(Entity ent)
    {
#if Debug_Cause_Error
        // 错误类型处理
        if (ent is AttributeDefinition) // 属性定义
            return new(Point3d.Origin, Point3d.Origin, false);
        if (ent is Xline xline)// 参照线
            return new(xline.BasePoint, xline.BasePoint, true);
        if (ent is Ray ray)// 射线
            return new(ray.BasePoint, ray.BasePoint, true);
#endif
        // 指定类型处理
        if (ent is BlockReference brf)
            return GetBoxInfoInBlockReference(brf);
        if (ent is MText mText)
            return GetBoxInfoInMText(mText);

        if (!_typeNames.Contains(ent.GetType().Name)) // 屏蔽天正等等缺失包围盒的类型
            try
            {
                return new(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint, true);
            }
            catch (Exception e) {  }
        return new(Point3d.Origin, Point3d.Origin, false);
    }

    /// <summary>
    /// 处理块参照
    /// </summary>
    static BoundingInfo GetBoxInfoInBlockReference(BlockReference brf)
    {
        try
        {
            // 这个获取是原点附近,需要平移到块基点
            var fit = brf.GeometryExtentsBestFit();
            //var minX = fit.MinPoint.X + brf.Position.X;
            //var minY = fit.MinPoint.Y + brf.Position.Y;
            //var minZ = fit.MinPoint.Z + brf.Position.Z;
            //var maxX = fit.MaxPoint.X + brf.Position.X;
            //var maxY = fit.MaxPoint.Y + brf.Position.Y;
            //var maxZ = fit.MaxPoint.Z + brf.Position.Z;
            //return new(minX, minY, minZ, maxX, maxY, maxZ, true);
            return new(fit.MinPoint, fit.MaxPoint, true);
        }
        catch
        {
            // 如果是一条参照线的组块,将导致获取包围盒时报错
            // 0x01 是否需要进入块内,然后拿到每个图元的BasePoint再计算中点?感觉过于复杂.
            // 0x02 这个时候拿基点走就算了
            return new(brf.Position, brf.Position, true);
        }
    }

    /// <summary>
    /// 处理多行文字
    /// </summary>
    static BoundingInfo GetBoxInfoInMText(MText mtxt)
    {
        /*
         * MText Aussehen
         * ------------------------------------
         * |                |                 |
         * |                |                 |ht
         * |                |                 |
         * |-----wl-------插入点------wr-------|
         * |                |                 |
         * |                |                 |hb
         * |                |                 |
         * ------------------------------------
         */

        double width = mtxt.ActualWidth;// 实际宽度
        double height = mtxt.ActualHeight;// 实际高度
        double wl = 0.0;
        double hb = 0.0;

        // 对齐方式
        switch (mtxt.Attachment)
        {
            case AttachmentPoint.TopCenter:
            case AttachmentPoint.MiddleCenter:
            case AttachmentPoint.BottomCenter:
            wl = width * -0.5;
            break;
            case AttachmentPoint.TopRight:
            case AttachmentPoint.MiddleRight:
            case AttachmentPoint.BottomRight:
            wl = -width;
            break;
        }
        switch (mtxt.Attachment)
        {
            case AttachmentPoint.TopLeft:
            case AttachmentPoint.TopCenter:
            case AttachmentPoint.TopRight:
            hb = -height;// 下边线到插入点的距离
            break;
            case AttachmentPoint.MiddleLeft:
            case AttachmentPoint.MiddleCenter:
            case AttachmentPoint.MiddleRight:
            hb = height * -0.5;
            break;
        }

        double wr = width + wl;
        double ht = height + hb;

        Point3d center = mtxt.Location;
        return new(center.X + wl, center.Y + hb, 0,
                   center.X + wr, center.Y + ht, 0,
                   true,
                   mtxt.Rotation);
    }
}