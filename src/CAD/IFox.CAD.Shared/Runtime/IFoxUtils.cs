namespace IFoxCAD.Cad;

public class IFoxUtils
{
    /// <summary>
    /// 刷新图层状态，在修改图层的锁定或冻结状态后使用
    /// </summary>
    /// <param name="objectIds">图层id集合</param>
    public static void RegenLayers(IEnumerable<ObjectId> objectIds)
    {
        var type = Acap.Version.Major >= 21 ? Assembly.Load("accoremgd")?.GetType("Autodesk.AutoCAD.Internal.CoreLayerUtilities") : Assembly.Load("acmgd")?.GetType("Autodesk.AutoCAD.Internal.LayerUtilities");
        var mi = type?.GetMethods().FirstOrDefault(e => e.Name == "RegenLayers");
        var pi = type?.GetProperties().FirstOrDefault(e => e.Name == "RegenPending");
        var regenPending = (int)(pi?.GetValue(null) ?? 0);
        mi?.Invoke(null, new object[] { objectIds.ToArray(), regenPending });
    }
}
