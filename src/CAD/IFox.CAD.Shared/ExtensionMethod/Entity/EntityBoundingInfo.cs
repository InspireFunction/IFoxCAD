#define Debug_Cause_Error
namespace IFoxCAD.Cad;

/// <summary>
/// 和尚777 重构
/// 包围盒信息
/// </summary>
public struct BoundingInfo
{
  public double MinX;
  public double MinY;
  public double MinZ;

  public double MaxX;
  public double MaxY;
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
  /// 左下点 P1
  /// </summary>
  public Point3d BottomLeft => new(MinX, MinY, MinZ);
  /// <summary>
  /// P2
  /// </summary>
  public Point3d BottomCenter => BottomLeft.GetMidPointTo(BottomRight);
  /// <summary>
  /// P3
  /// </summary>
  public Point3d BottomRight => new(MaxX, MinY, MinZ);
  /// <summary>
  /// P4
  /// </summary>
  public Point3d MidLeft => BottomLeft.GetMidPointTo(TopLeft);
  /// <summary> 
  /// P5
  /// </summary>
  public Point3d MidCenter => BottomLeft.GetMidPointTo(TopRight);
  /// <summary>  
  /// P6 
  /// </summary> 
  public Point3d MidRight => BottomRight.GetMidPointTo(TopRight);
  /// <summary>
  /// P7
  /// </summary>
  public Point3d TopLeft => new(MinX, MaxY, MinZ);
  /// <summary>
  /// P8
  /// </summary>
  public Point3d TopCenter => TopLeft.GetMidPointTo(TopRight);
  /// <summary>
  /// 右上点 P9
  /// </summary>
  public Point3d TopRight => new(MaxX, MaxY, MaxZ);

  // public Point3d Min => new(MinX, MinY, MinZ);

  // public Point3d Max => new(MaxX, MaxY, MaxZ);
  #endregion

  public double Height => Math.Abs(MaxX - MinX);
  public double Width => Math.Abs(MaxY - MinY);
  public double Area => Height * Width;
  public Extents3d Extents3d { get; }
  public Extents2d Extents2d => new(MinX, MinY, MaxX, MaxY);

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
public static class EntityBoundingInfo
{

  /// <summary>
  /// 获取包围盒信息
  /// </summary>
  /// <param name="ext"></param>
  /// <param name="dist"></param>
  /// <returns></returns>
  public static BoundingInfo? GetBoundingInfo(this Extents3d ext)
  {
    return new(ext);
  }

  // 包围盒外扩
  //public static BoundingInfo? GetBoundingInfo(this Extents3d ext, double dist = 0)
  //{
  //  var p1 = ext.MinPoint.Offset(-dist, -dist);
  //  var p2 = ext.MaxPoint.Offset(dist, dist);
  //  var e = new Extents3d(p1, p2);
  //  return new(e);
  //}

  /// <summary>
  /// 获取实体包围盒
  /// </summary>
  /// <param name="ent"></param>
  /// <returns></returns>
  static Extents3d? GetEntityBox(this Entity ent)
  {
    if (!ent.Bounds.HasValue)
      return null;
    //if (ent is BlockReference brf)
    //  ext = brf.GeometryExtentsBestFit();

    if (ent is Spline spl)
      return spl.ToPolyline().GeometricExtents;

    else if (ent is MText mtext)
      return GetMTextBox(mtext);

    else if (ent is Table table)
    {
      table.RecomputeTableBlock(true);
      return table.GeometricExtents;
    }

    else if (ent is Dimension dim)
    {
      dim.RecomputeDimensionBlock(true);
      return dim.GeometricExtents;
    }
    else
      return ent.GeometricExtents;

  }

  /// <summary>
  /// 获取多行文本的正交包围盒
  /// </summary>
  /// <param name="mText"></param>
  /// <returns></returns>
  static Extents3d GetMTextBox(MText mText)
  {
    return mText.GetMTextBoxCorners().ToExtents3D();
  }

  /// <summary>
  /// 获取点集包围盒
  /// </summary>
  /// <param name="pts"></param>
  /// <returns></returns>
  static Extents3d ToExtents3D(this IEnumerable<Point3d> pts)
  {
    var ext = new Extents3d();
    foreach (Point3d pt in pts)
    {
      ext.AddPoint(pt);
    }
    return ext;
  }

  /// <summary>
  /// 获取块的包围盒
  /// </summary>
  /// <param name="en"></param>
  /// <param name="ext"></param>
  /// <param name="mat"></param>
  static void GetBlockBox(this Entity en, ref Extents3d ext, ref Matrix3d mat)
  {
    if (en is BlockReference block)
    {
      var matins = mat * block.BlockTransform;
      if (block.BlockTableRecord.GetObject<BlockTableRecord>() is BlockTableRecord btr)
        foreach (ObjectId id in btr)
        {
          id.GetObject<Entity>();
          if (id.GetObject<Entity>() is Entity ent1)
          {
            if (ent1.Visible != true)
              continue;
            if (ent1 is AttributeDefinition att)
            {
              if (att != null && (!att.Constant || att.Invisible))
                continue;
            }
            GetBlockBox(ent1, ref ext, ref matins);
          }
        }
      if (block.AttributeCollection.Count > 0)
      {
        foreach (ObjectId attid in block.AttributeCollection)
        {
          if (attid.GetObject<AttributeReference>() is AttributeReference att)
          {
            if (!att.Invisible && att.Visible)
              GetBlockBox(att, ref ext, ref mat);
          }
        }
      }
    }
    else
    {
      if (mat.IsUniscaledOrtho())
      {
        using (var ent1 = en.GetTransformedCopy(mat))
        {
          if (ext.IsEmptyExt())
          {
            var e = ent1.GetEntityBox();
            if (e.HasValue)
              ext = e.Value;
          }
          else
          {
            var e = ent1.GetEntityBox();
            if (e.HasValue)
              ext.AddExtents(e.Value);
          }
        }
      }
      else
      {
        var e = en.GetEntityBox();
        if (e.HasValue)
        {
          Extents3d entext = e.Value;
          entext.TransformBy(mat);
          if (ext.IsEmptyExt())
            ext = entext;
          else
            ext.AddExtents(entext);
        }
        return;
      }
    }
    return;
  }
  /// <summary>
  /// 获取多行文字最小包围盒4点坐标
  /// </summary>
  /// <param name="mtext"></param>
  /// <returns></returns>
  public static Point3d[] GetMTextBoxCorners(this MText mtext)
  {
    double width = mtext.ActualWidth;
    double height = mtext.ActualHeight;
    Point3d point1, point2;
    switch (mtext.Attachment)
    {
      case AttachmentPoint.TopLeft:
      default:
        point1 = new Point3d(0.0, -height, 0.0);
        point2 = new Point3d(width, 0.0, 0.0);
        break;
      case AttachmentPoint.TopCenter:
        point1 = new Point3d(-width * 0.5, -height, 0.0);
        point2 = new Point3d(width * 0.5, 0.0, 0.0);
        break;
      case AttachmentPoint.TopRight:
        point1 = new Point3d(-width, -height, 0.0);
        point2 = new Point3d(0.0, 0.0, 0.0);
        break;
      case AttachmentPoint.MiddleLeft:
        point1 = new Point3d(0.0, -height * 0.5, 0.0);
        point2 = new Point3d(width, height * 0.5, 0.0);
        break;
      case AttachmentPoint.MiddleCenter:
        point1 = new Point3d(-width * 0.5, -height * 0.5, 0.0);
        point2 = new Point3d(width * 0.5, height * 0.5, 0.0);
        break;
      case AttachmentPoint.MiddleRight:
        point1 = new Point3d(-width, -height * 0.5, 0.0);
        point2 = new Point3d(0.0, height * 0.5, 0.0);
        break;
      case AttachmentPoint.BottomLeft:
        point1 = new Point3d(0.0, 0.0, 0.0);
        point2 = new Point3d(width, height, 0.0);
        break;
      case AttachmentPoint.BottomCenter:
        point1 = new Point3d(-width * 0.5, 0.0, 0.0);
        point2 = new Point3d(width * 0.5, height, 0.0);
        break;
      case AttachmentPoint.BottomRight:
        point1 = new Point3d(-width, 0.0, 0.0);
        point2 = new Point3d(0.0, height, 0.0);
        break;
    }

    var xform =
        Matrix3d.Displacement(mtext.Location.GetAsVector()) *
        Matrix3d.Rotation(mtext.Rotation, mtext.Normal, Point3d.Origin) *
        Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, mtext.Normal));

    return new[]
    {
              point1.TransformBy(xform),
              new Point3d(point2.X, point1.Y, 0.0).TransformBy(xform),
              point2.TransformBy(xform),
              new Point3d(point1.X, point2.Y, 0.0).TransformBy(xform)
          };
  }
  /// <summary>
  /// 获取实体包围盒
  /// </summary>
  /// <param name="ent"></param>
  /// <returns></returns>
  public static Extents3d? GetEntityBoxEx( Entity ent)
  {
    if (ent is BlockReference block)
    {
      Extents3d blockExt = default;
      var mat = Matrix3d.Identity;
      block!.GetBlockBox(ref blockExt, ref mat);
      if (blockExt.IsEmptyExt())
        return null;
      return blockExt;
    }
    return GetEntityBox(ent);
  }

  /// <summary>
  /// 判断包围盒是否有效
  /// </summary>
  /// <param name="ext"></param>
  /// <returns></returns>
  static bool IsEmptyExt(this Extents3d ext)
  {
    if (ext.MinPoint.DistanceTo(ext.MaxPoint) < Tolerance.Global.EqualPoint)
      return true;
    else
      return false;
  }
  /// <summary>
  /// 点偏移
  /// </summary>
  /// <param name="pt"></param>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  /// <returns></returns>
  //static Point3d Offset(this Point3d pt, double x, double y, double z = 0)
  //{
  //  return new Point3d(pt.X + x, pt.Y + y, pt.Z + z);
  //}
}