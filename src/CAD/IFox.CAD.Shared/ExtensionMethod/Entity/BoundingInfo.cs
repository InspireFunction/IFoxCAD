namespace IFoxCAD.Cad;

/// <summary>
/// 和尚777 重构
/// 包围盒信息
/// </summary>
public struct BoundingInfo
{
    #region 成员


    /// <summary>
    /// MinPoint.X
    /// </summary>
    public double MinX;
    /// <summary>
    /// MinPoint.Y
    /// </summary>
    public double MinY;
    /// <summary>
    /// MinPoint.Z
    /// </summary>
    public double MinZ;
    /// <summary>
    /// MaxPoint.X
    /// </summary>
    public double MaxX;
    /// <summary>
    /// MaxPoint.Y
    /// </summary>
    public double MaxY;
    /// <summary>
    /// MaxPoint.Z
    /// </summary>
    public double MaxZ;

    #region 包围盒9位码坐标
    /*
         * 包围盒9位码坐标
         * P7---------------P8----------------P9
         * |                |                 |
         * |                |                 |
         * |                |                 |
         * P4---------------P5----------------P6
         * |                |                 |
         * |                |                 |
         * |                |                 |
         * P1---------------P2----------------P3
         */
    /// <summary>
    /// MinPoint 左下点 P1
    /// </summary>
    public readonly Point3d BottomLeft => new(MinX, MinY, MinZ);
    /// <summary>
    /// P2
    /// </summary>
    public readonly Point3d BottomCenter => BottomLeft.GetMidPointTo(BottomRight);
    /// <summary>
    /// P3
    /// </summary>
    public readonly Point3d BottomRight => new(MaxX, MinY, MinZ);
    /// <summary>
    /// P4
    /// </summary>
    public readonly Point3d MidLeft => BottomLeft.GetMidPointTo(TopLeft);
    /// <summary> 
    /// P5
    /// </summary>
    public readonly Point3d MidCenter => BottomLeft.GetMidPointTo(TopRight);
    /// <summary>  
    /// P6 
    /// </summary> 
    public readonly Point3d MidRight => BottomRight.GetMidPointTo(TopRight);
    /// <summary>
    /// P7
    /// </summary>
    public readonly Point3d TopLeft => new(MinX, MaxY, MinZ);
    /// <summary>
    /// P8
    /// </summary>
    public readonly Point3d TopCenter => TopLeft.GetMidPointTo(TopRight);
    /// <summary>
    /// MaxPoint 右上点 P9
    /// </summary>
    public readonly Point3d TopRight => new(MaxX, MaxY, MaxZ);

    // public Point3d Min => new(MinX, MinY, MinZ);

    // public Point3d Max => new(MaxX, MaxY, MaxZ);
    #endregion

    /// <summary>
    /// 高
    /// </summary>
    public readonly double Height => Math.Abs(MaxY - MinY);

    /// <summary>
    /// 宽
    /// </summary>
    public readonly double Width => Math.Abs(MaxX - MinX);

    /// <summary>
    /// 面积
    /// </summary>
    public readonly double Area => Height * Width;

    /// <summary>
    /// 3D包围盒
    /// </summary>
    public Extents3d Extents3d { get; }

    /// <summary>
    /// 2D包围盒
    /// </summary>
    public readonly Extents2d Extents2d => new(MinX, MinY, MaxX, MaxY);

    #endregion

    #region 构造

    /// <summary>
    /// 包围盒信息3D构造
    /// </summary>
    /// <param name="ext">包围盒</param>
    public BoundingInfo(Extents3d ext)
    {
        MinX = ext.MinPoint.X;
        MinY = ext.MinPoint.Y;
        MinZ = ext.MinPoint.Z;
        MaxX = ext.MaxPoint.X;
        MaxY = ext.MaxPoint.Y;
        MaxZ = ext.MaxPoint.Z;
        Extents3d = ext;
    }

    /// <summary>
    /// 包围盒信息2D构造
    /// </summary>
    /// <param name="ext">包围盒</param>
    public BoundingInfo(Extents2d ext)
    {
        MinX = ext.MinPoint.X;
        MinY = ext.MinPoint.Y;
        MinZ = 0;
        MaxX = ext.MaxPoint.X;
        MaxY = ext.MaxPoint.Y;
        MaxZ = 0;
        var pt1 = new Point3d(MinX, MinY, 0);
        var pt9 = new Point3d(MaxX, MaxY, 0);
        Extents3d = new Extents3d(pt1, pt9);
    }

    #endregion

    /// <summary>
    /// 重写ToString
    /// </summary>
    /// <returns>返回MinPoint,MaxPoint坐标</returns>
    public override string ToString()
    {
        return Extents3d.ToString();
    }
    /// <summary>
    /// 移动包围盒
    /// </summary>
    /// <param name="pt1">基点</param>
    /// <param name="pt2">目标点</param>
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
