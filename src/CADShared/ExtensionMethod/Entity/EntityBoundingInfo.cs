namespace IFoxCAD.Cad;

/// <summary>
/// 获取实体包围盒信息方法
/// </summary>
internal static class EntityBoundingInfo
{
    /// <summary>
    /// 获取包围盒信息
    /// </summary>
    /// <param name="ext">包围盒</param>
    /// <returns>包围盒信息</returns>
    public static BoundingInfo? GetBoundingInfo(this Extents3d ext)
    {
        return new(ext);
    }

    /// <summary>
    /// 获取多行文本的正交包围盒
    /// </summary>
    /// <param name="mText">多行文本</param>
    /// <returns>包围盒</returns>
    static Extents3d GetMTextBox(MText mText)
    {
        var ext = new Extents3d();
        foreach (var p in GetMTextBoxCorners(mText))
        {
            ext.AddPoint(p);
        }

        return ext;
    }

    /// <summary>
    /// 获取块的包围盒
    /// </summary>
    /// <param name="en">实体</param>
    /// <param name="ext"></param>
    /// <param name="mat"></param>
    static void GetBlockBox(this Entity en, ref Extents3d ext, ref Matrix3d mat)
    {

        if (en is BlockReference block)
        {
            var matins = mat * block.BlockTransform;
            using var a = DBTrans.Top.GetObject(block.BlockTableRecord);
            if (a is BlockTableRecord btr)
                foreach (var id in btr)
                {
                    using var b = DBTrans.Top.GetObject(id);
                    if (b is Entity ent1)
                    {
                        if (ent1.Visible != true)
                            continue;
                        if (ent1 is AttributeDefinition att)
                        {
                            if (!att.Constant || att.Invisible)
                                continue;
                        }
                        GetBlockBox(ent1, ref ext, ref matins);
                    }
                }

            if (block.AttributeCollection.Count > 0)
            {
                foreach (var att in block.GetAttributes())
                {
                    if (!att.Invisible && att.Visible)
                        GetBlockBox(att, ref ext, ref mat);
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
                        //var e = ent1.GetEntityBox();
                        var e = GetEntityBoxEx(ent1);
                        if (e.HasValue)
                            ext = e.Value;
                    }
                    else
                    {
                        //var e = ent1.GetEntityBox();
                        var e = GetEntityBoxEx(ent1);
                        if (e.HasValue)
                            ext.AddExtents(e.Value);
                    }
                }
            }
            else
            {
                //var e = en.GetEntityBox();
                var e = GetEntityBoxEx(en);
                if (e.HasValue)
                {
                    var entext = e.Value;
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
    /// <param name="mtext">多行文本</param>
    /// <returns>最小包围盒4点坐标</returns>
    public static Point3d[] GetMTextBoxCorners(MText mtext)
    {
        var width = mtext.ActualWidth;
        var height = mtext.ActualHeight;
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

        var xform = Matrix3d.Displacement(mtext.Location.GetAsVector()) *
                    Matrix3d.Rotation(mtext.Rotation, mtext.Normal, Point3d.Origin) *
                    Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, mtext.Normal));

        return
        [
            point1.TransformBy(xform),
            new Point3d(point2.X, point1.Y, 0.0).TransformBy(xform),
            point2.TransformBy(xform),
            new Point3d(point1.X, point2.Y, 0.0).TransformBy(xform)
        ];
    }


#if NET35
    static Curve ToPolyline(this Curve c)
    {
        throw new System.Exception();
    }
#endif

    /// <summary>
    /// 获取实体包围盒
    /// </summary>
    /// <param name="ent">实体</param>
    /// <returns>包围盒</returns>
    public static Extents3d? GetEntityBoxEx(Entity ent)
    {
        Extents3d? ext = null;
        switch (ent)
        {
            case Spline spl:
            ext = spl.ToPolyline().GeometricExtents;
            break;
            case MText mtext:
            ext = GetMTextBox(mtext);
            break;
            case Table table:
            if (table.IsNewObject)
                table.GenerateLayout();
            table.RecomputeTableBlock(true);
            ext = table.GeometricExtents;
            break;
            case Dimension dim:
            if (dim.IsNewObject)
                dim.GenerateLayout(); // 新new的实体生成布局,即可获取包围盒
            dim.RecomputeDimensionBlock(true);
            ext = dim.GeometricExtents;
            break;
            case BlockReference block:
            Extents3d blockExt = default;
            var mat = Matrix3d.Identity;
            block!.GetBlockBox(ref blockExt, ref mat);
            if (!blockExt.IsEmptyExt())
                ext = blockExt;
            break;
            // 和尚_2024-10-26 
            case Hatch hatch:
            var extTmp = new Extents3d();

            var tr = DBTrans.GetTop(hatch.Database);
            var hc = HatchConverter.Create(hatch);
            hc.GetBoundarysData();
            hc.CreateBoundary();
            var ids = hc.BoundaryNewlyIds;
            if (ids is not null)
            {
                foreach (var curveId in ids)
                {
                    using var curve = (Entity)tr.GetObject(curveId);
                    extTmp.AddExtents(GetEntityBoxEx(curve)!.Value);
                }
            }
            ext = extTmp;
            break;
            default:
#if !NET35
            if (ent.Bounds.HasValue)
                ext = ent.GeometricExtents;
            break;
#else
            throw new System.Exception("net35 不支持,需要获取包围盒");
#endif
        }

        if (ext != null)
            //实体不是点时，pass
            if (ent is not DBPoint && ext.Value.MinPoint.IsEqualTo(ext.Value.MaxPoint))
                return null;
        return ext;
    }

    /// <summary>
    /// 判断包围盒是否有效
    /// </summary>
    /// <param name="ext">包围盒</param>
    /// <returns></returns>
    static bool IsEmptyExt(this Extents3d ext)
    {
        if (ext.MinPoint.DistanceTo(ext.MaxPoint) < Tolerance.Global.EqualPoint)
            return true;
        else
            return false;
    }
}