#if !NET8_0_OR_GREATER
using System.Diagnostics;
using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif

namespace IFoxCAD.Cad;

/// <summary>
/// 块参照扩展类
/// </summary>
public static class BlockReferenceEx
{
    #region 裁剪块参照

    private const string kFilterDictName = "ACAD_FILTER";
    private const string kSpatialName = "SPATIAL";

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="brf">块参照</param>
    /// <param name="pt3ds">裁剪多边形点表</param>
    public static void XClip(this BlockReference brf, IEnumerable<Point3d> pt3ds)
    {
        var mat = brf.BlockTransform.Inverse();
        var pts = pt3ds.Select(p => p.TransformBy(mat).Point2d()).ToCollection();
        SpatialFilterDefinition sfd = new(pts, Vector3d.ZAxis, 0.0, double.PositiveInfinity,
            double.NegativeInfinity, true);
        using SpatialFilter sf = new();
        sf.Definition = sfd;
        var dict = brf.GetXDictionary()?.GetSubDictionary(true, [kFilterDictName]);
        dict?.SetAt(kSpatialName, sf);
    }

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="brf">块参照</param>
    /// <param name="pt1">第一角点</param>
    /// <param name="pt2">第二角点</param>
    public static void XClip(this BlockReference brf, Point3d pt1, Point3d pt2)
    {
        var mat = brf.BlockTransform.Inverse();
        pt1 = pt1.TransformBy(mat);
        pt2 = pt2.TransformBy(mat);
        Point2dCollection pts =
        [
            new Point2d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y)),
            new Point2d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y))
        ];
        using SpatialFilter sf = new();
        sf.Definition = new(pts, Vector3d.ZAxis, 0.0, double.PositiveInfinity,
            double.NegativeInfinity, true);

        brf.GetXDictionary()
           ?.GetSubDictionary(true, [kFilterDictName])
           ?.SetAt(kSpatialName, sf);
#if !acad
        pts.Dispose();
#endif
    }

    #endregion

    #region 属性

    /// <summary>
    /// 更新动态块参数值
    /// </summary>
    public static bool ChangeBlockProperty(this BlockReference blockReference,
        Dictionary<string, object> propertyNameValues)
    {
        if (!blockReference.IsDynamicBlock)
            return false;
        using (blockReference.ForWrite())
        {
            foreach (DynamicBlockReferenceProperty item in blockReference
                         .DynamicBlockReferencePropertyCollection)
            {
                if (propertyNameValues.TryGetValue(item.PropertyName, out var value))
                {
                    item.Value = item.PropertyTypeCode switch
                    {
                        1 => Convert.ToDouble(value),
                        2 => Convert.ToInt32(value),
                        3 => Convert.ToInt16(value),
                        4 => Convert.ToInt16(value),
                        5 => Convert.ToString(value),
                        13 => Convert.ToInt64(value),
                        _ => value,
                    };
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 更新动态块参数值
    /// </summary>
    public static bool ChangeBlockProperty(this BlockReference blockReference, string propName,
        object value)
    {
        if (!blockReference.IsDynamicBlock)
            return false;
        using (blockReference.ForWrite())
        {
            foreach (DynamicBlockReferenceProperty item in blockReference
                         .DynamicBlockReferencePropertyCollection)
            {
                if (item.PropertyName != propName)
                    continue;
                item.Value = item.PropertyTypeCode switch
                {
                    1 => Convert.ToDouble(value),
                    2 => Convert.ToInt32(value),
                    3 => Convert.ToInt16(value),
                    4 => Convert.ToInt16(value),
                    5 => Convert.ToString(value),
                    13 => Convert.ToInt64(value),
                    _ => value,
                };
                break;
            }
        }

        return true;
    }

    /// <summary>
    /// 更新属性块的属性值
    /// </summary>
    public static void ChangeBlockAttribute(this BlockReference blockReference,
        Dictionary<string, string> propertyNameValues)
    {
        var tr = DBTrans.GetTopTransaction(blockReference.Database);
        if (tr == null) return;
        foreach (var item in blockReference.AttributeCollection)
        {
            AttributeReference att;
            if (item is ObjectId id)
            {
                // 通常情况下返回的都是 ObjectId
                att = (AttributeReference)tr.GetObject(id, OpenMode.ForRead);
            }
            else
            {
                // 某些情况下，比如你exploded炸开块后的子块块参照是没有在数据库里的，这时候返回的结果就是 AttributeReference
                att = (AttributeReference)item;
            }

            if (propertyNameValues.TryGetValue(att.Tag, out var value))
            {
                using (att.ForWrite())
                {
                    att.TextString = value;
                    att.AdjustAlignment(blockReference.Database);
                }
            }
        }
    }

    /// <summary>
    /// 获取普通块参照的属性集合
    /// </summary>
    /// <param name="owner">普通块参照</param>
    /// <returns>属性集合</returns>
    public static IEnumerable<AttributeReference> GetAttributes(this BlockReference owner)
    {
        if (owner.Database != null)
        {
            var trans = DBTrans.GetTopTransaction(owner.Database);
            if (trans is null)
            {
                yield break;
            }
            foreach (ObjectId id in owner.AttributeCollection)
                yield return (AttributeReference)trans.GetObject(id, OpenMode.ForRead);
        }
        else
        {
            foreach (AttributeReference att in owner.AttributeCollection)
                yield return att;
        }
    }

    #endregion

    /// <summary>
    /// 获取块表记录
    /// </summary>
    /// <param name="brf">块参照</param>
    /// <returns>块表记录</returns>
    public static BlockTableRecord GetBlockTableRecord(this BlockReference brf)
    {
        return (BlockTableRecord)brf.BlockTableRecord.GetObject(OpenMode.ForRead);
    }

    /// <summary>
    /// 获取块的有效名字
    /// </summary>
    /// <param name="blk">块参照</param>
    /// <returns>名字</returns>
    public static string GetBlockName(this BlockReference blk)
    {
        ArgumentNullException.ThrowIfNull(blk);
        if (blk.IsDynamicBlock)
        {
            var btrId = blk.DynamicBlockTableRecord;
            var tr = btrId.Database.TransactionManager.TopTransaction;
            ArgumentNullException.ThrowIfNull(tr);
            var btr = (BlockTableRecord)tr.GetObject(btrId);
            return btr.Name;
        }

        return blk.Name;
    }

    /// <summary>
    /// 获取嵌套块的位置(wcs)
    /// </summary>
    /// <param name="parentBlockRef">父块</param>
    /// <param name="nestedBlockName">子块名</param>
    /// <returns>子块的位置</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Point3d? GetNestedBlockPosition(this BlockReference parentBlockRef,
        string nestedBlockName)
    {
        var tr = DBTrans.GetTopTransaction(parentBlockRef.Database);
        if (tr is null)
        {
            throw new ArgumentException("无法获取事务");
        }
        var btr = tr.GetObject<BlockTableRecord>(parentBlockRef.BlockTableRecord);
        if (btr == null)
            return null;
        foreach (var id in btr)
        {
#if NET35
            var a = id.ObjectClass().DxfName;
#else
            var a = id.ObjectClass.DxfName;
#endif
            if (a == "AcDbBlockReference")
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
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr is null)
        {
            return;
        }
        if (tr.GetObject(brf.BlockTableRecord, OpenMode.ForRead) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, CtrlState> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr is null)
        {
            return;
        }
        if (tr.GetObject(brf.BlockTableRecord, OpenMode.ForRead) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, CtrlState, int> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr is null)
        {
            return;
        }
        if (tr.GetObject(brf.BlockTableRecord, OpenMode.ForRead) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历嵌套块中块图元
    /// </summary>
    /// <param name="blockReference">块参照</param>
    /// <param name="action">委托</param>
    /// <param name="tr">事务</param>
    public static void NestedForEach(this Entity blockReference, Action<Entity, Matrix3d> action,
        DBTrans? tr = null)
    {
        tr ??= DBTrans.GetTop(blockReference.IsNewObject ? Env.Database : blockReference.Database);
        var queue = new Queue<(Entity, Matrix3d)>();
        queue.Enqueue((blockReference, Matrix3d.Identity));
        while (queue.Any())
        {
            var (ent, mt) = queue.Dequeue();
            action?.Invoke(ent, mt);
            if (ent is BlockReference brfTemp)
            {
                var mtNext = mt * brfTemp.BlockTransform;
                tr.BlockTable.Change(brfTemp.BlockTableRecord, btr => {
                    foreach (var id in btr)
                    {
                        if (tr.GetObject(id) is Entity entNext)
                        {
                            queue.Enqueue((entNext, mtNext));
                        }
                    }
                });
            }
        }
    }

    /// <summary>
    /// 获取块可见性信息
    /// </summary>
    /// <param name="blockReference">块参照</param>
    /// <returns>可见性信息</returns>
    public static BlockVisibilityInfo? GetVisibilityInfo(this BlockReference blockReference)
    {
        var tr = DBTrans.GetTopTransaction(blockReference.Database);
        if (tr is null)
            throw new InvalidOperationException("没有活动事务，无法获取块可见性信息");
        var btr = (BlockTableRecord)tr.GetObject(blockReference.DynamicBlockTableRecord, OpenMode.ForRead);
        return btr.GetVisibilityInfo();
    }

    /// <summary>
    /// 获取块可见性信息
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <returns>可见性信息</returns>
    public static BlockVisibilityInfo? GetVisibilityInfo(this BlockTableRecord btr)
    {
        if (btr.IsDynamicBlock && btr.ExtensionDictionary.IsOk())
        {
            var dict = btr.GetXDictionary();
            if (dict is null)
            {
                return null;
            }
            if (dict.Contains("ACAD_ENHANCEDBLOCK"))
            {
                var info = new BlockVisibilityInfo();
                var idEnhancedBlock = dict.GetAt("ACAD_ENHANCEDBLOCK");
                var enhancedBlockTypedValues = Env.EntGet(idEnhancedBlock);
                var parm = enhancedBlockTypedValues.FirstOrDefault(e => e.TypeCode == 360 && e.Value is ObjectId id && IsDxfBV(id)).Value;
                if (parm is ObjectId parmId)
                {
                    info.Has = true;
                    var parmTypedValues = Env.EntGet(parmId);
                    info.PropertyName = parmTypedValues.FirstOrDefault(e => e.TypeCode == 301)
                        .Value?.ToString() ?? "";
                    info.AllowedValues = parmTypedValues.Where(e => e.TypeCode == 303)
                        .Select(e => e.Value?.ToString() ?? "")
                        .Where(e => !StringHelper.IsNullOrWhiteSpace(e))
                        .ToList();
                }
                return info;
            }
        }
        return null;
    }

    static bool IsDxfBV(ObjectId id)
    {
        if (!id.IsOk()) { return false; }
#if NET35
        var a = id.ObjectClass().DxfName.ToUpper();
#else
        var a = id.ObjectClass.DxfName.ToUpper();
#endif
        return a == "BLOCKVISIBILITYPARAMETER";
    }
}

/// <summary>
/// 块可见性信息
/// </summary>
public class BlockVisibilityInfo
{
    /// <summary>
    /// 有无可见性
    /// </summary>
    public bool Has { get; set; }

    /// <summary>
    /// 属性名
    /// </summary>
    public string PropertyName { get; set; } = "";

    /// <summary>
    /// 允许值
    /// </summary>
    public List<string> AllowedValues { get; set; } = [];
}