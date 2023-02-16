

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
    /// <param name="blockReference">动态块</param>
    /// <param name="propertyNameValues">属性值字典</param>
    public static void ChangeBlockProperty(this BlockReference blockReference,
                                           Dictionary<string, object> propertyNameValues)
    {
        if (!blockReference.IsDynamicBlock)
            return;

        using (blockReference.ForWrite())
        {
            foreach (DynamicBlockReferenceProperty item in blockReference.DynamicBlockReferencePropertyCollection)
                if (propertyNameValues.ContainsKey(item.PropertyName))
                    item.Value = propertyNameValues[item.PropertyName];
        }
    }
    #endregion

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <param name="tr"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId> action)
    {
        //if (action == null)
        //    throw new ArgumentNullException(nameof(action));
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        var btr = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);
        if (btr == null)
            return;
        btr.ForEach(action);
    }
    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <param name="tr"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState> action)
    {
        //if (action == null)
        //    throw new ArgumentNullException(nameof(action));
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        var btr = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);
        if (btr == null)
            return;
        btr.ForEach(action);
    }
    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <param name="tr"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState, int> action)
    {
        action.NotNull(nameof(action));
        var tr = DBTrans.GetTopTransaction(brf.Database);
        var btr = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);
        if (btr == null)
            return;
        btr.ForEach(action);
    }
}