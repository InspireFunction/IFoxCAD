

namespace IFoxCAD.Cad;

/// <summary>
/// 块参照扩展类
/// </summary>
public static class BlockReferenceEx
{
    #region 裁剪块参照

    private const string filterDictName = "ACAD_FILTER";
    private const string spatialName = "SPATIAL";

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="bref">块参照</param>
    /// <param name="pt3ds">裁剪多边形点表</param>
    public static void ClipBlockRef(this BlockReference bref, IEnumerable<Point3d> pt3ds)
    {
        Matrix3d mat = bref.BlockTransform.Inverse();
        var pts =
            pt3ds
            .Select(p => p.TransformBy(mat).Point2d())
            .ToCollection();

        SpatialFilterDefinition sfd = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
        using SpatialFilter sf = new() { Definition = sfd };
        var dict = bref.GetXDictionary()!.GetSubDictionary(true, new string[] { filterDictName })!;
        dict.SetAt<SpatialFilter>(spatialName, sf);
        // SetToDictionary(dict, spatialName, sf);
    }

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="bref">块参照</param>
    /// <param name="pt1">第一角点</param>
    /// <param name="pt2">第二角点</param>
    public static void ClipBlockRef(this BlockReference bref, Point3d pt1, Point3d pt2)
    {
        Matrix3d mat = bref.BlockTransform.Inverse();
        pt1 = pt1.TransformBy(mat);
        pt2 = pt2.TransformBy(mat);

        Point2dCollection pts = new()
        {
            new Point2d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y)),
            new Point2d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y))
        };

        using SpatialFilter sf = new()
        {
            Definition = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true)
        };
        var dict = bref.GetXDictionary()!
                       .GetSubDictionary(true, new string[] { filterDictName })!;
        dict.SetAt<SpatialFilter>(spatialName, sf);
        // SetToDictionary(dict, spatialName, sf);
#if !acad
        pts.Dispose();
#endif
    }
    #endregion

    #region 属性
    /// <summary>
    /// 更新动态块属性值
    /// </summary>
    public static void ChangeDynamicBlockProperty<T>(this BlockReference blockReference,
                                           Dictionary<string, T> propertyNameValues)
    {
        using (blockReference.ForWrite())
        {
            foreach (DynamicBlockReferenceProperty item in blockReference.DynamicBlockReferencePropertyCollection)
                if (propertyNameValues.TryGetValue(item.PropertyName, out T? value))
                    item.Value = value;
        }
    }
    /// <summary>
    /// 更新普通块的属性值
    /// </summary>
    public static void ChangeBlockProperty(this BlockReference blockReference, Dictionary<string, string> propertyNameValues)
    {
        var tr = DBTrans.GetTopTransaction(blockReference.Database);
        AttributeReference att;
        foreach (var item in blockReference.AttributeCollection)
        {
            if (item is ObjectId id)
            {
                // 通常情况下返回的都是 ObjectId
                att = (AttributeReference)tr.GetObject(id);
            }
            else
            {
                // 某些情况下，比如你exploded炸开块后的子块块参照是没有在数据库里的，这时候返回的结果就是 AttributeReference
                att = (AttributeReference)item;
            }
            using (att.ForWrite())
            {
                if (propertyNameValues.TryGetValue(att.Tag, out string value))
                {
                    att.TextString = value;
                }
            }
        }
    }
    
    /// <summary>
    /// 获取嵌套块的位置(wcs)
    /// </summary>
    /// <param name="parentBlockRef">父块</param>
    /// <param name="nestedBlockName">子块名</param>
    /// <returns>子块的位置</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Point3d? GetNestedBlockPosition(this BlockReference parentBlockRef, string nestedBlockName)
    {
        var tr = DBTrans.GetTopTransaction(parentBlockRef.Database);

        var btr = tr.GetObject<BlockTableRecord>(parentBlockRef.BlockTableRecord);
        if (btr == null) return null;
        foreach (ObjectId id in btr)
        {
            if (id.ObjectClass.Name == "AcDbBlockReference")
            {
                var nestedBlockRef = tr.GetObject<BlockReference>(id);
                if (nestedBlockRef?.Name == nestedBlockName)
                {
                    return nestedBlockRef.Position.TransformBy(parentBlockRef.BlockTransform);
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 获取普通块参照的属性集合
    /// </summary>
    /// <param name="owner">普通块参照</param>
    /// <returns>属性集合</returns>
    public static IEnumerable<AttributeReference> GetAttributes(this BlockReference owner)
    {
        var trans = DBTrans.GetTopTransaction(owner.Database);
        if (owner.Database != null)
        {
            foreach (ObjectId id in owner.AttributeCollection)
                yield return (AttributeReference)trans.GetObject(id);
        }
        else
        {
            foreach (AttributeReference att in owner.AttributeCollection)
                yield return att;
        }
    }
    #endregion

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId> action)
    {
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }
    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState> action)
    {
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }
    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState, int> action)
    {
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }

    }
}