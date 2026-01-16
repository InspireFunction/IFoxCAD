#if !gcad
#if !NET35
#if acad
using Autodesk.AutoCAD.BoundaryRepresentation;
#endif

#if zcad
using ZwSoft.ZwCAD.BoundaryRepresentation;
#endif
namespace IFoxCAD.Cad;

/// <summary>
/// 面域扩展
/// </summary>
public static class RegionEx
{
    /// <summary>
    /// 面域转曲线
    /// </summary>
    /// <param name="region">面域</param>
    /// <returns>曲线集合</returns>
    public static IEnumerable<Curve> ToCurves(this Region region)
    {
        if (region.IsNull)
            yield break;
        using var brep = new Brep(region);
        var loops = brep.Complexes.SelectMany(complex => complex.Shells)
            .SelectMany(shell => shell.Faces)
            .SelectMany(face => face.Loops);
        foreach (var loop in loops)
        {
            var curves3d = loop.Edges.Select(edge => ((ExternalCurve3d)edge.Curve).NativeCurve)
                .ToList();
            var cur = Curve.CreateFromGeCurve(1 < curves3d.Count
                ? new CompositeCurve3d(curves3d.ToOrderedArray())
                : curves3d.First());

            foreach (var curve3d in curves3d)
            {
                curve3d.Dispose();
            }

            cur.SetPropertiesFrom(region);
            yield return cur;
        }
    }

    /// <summary>
    /// 按首尾相连对曲线集合进行排序
    /// </summary>
    /// <param name="source"></param>
    /// <returns>曲线列表</returns>
    /// <exception cref="ArgumentException">当不能首尾相连时会抛出此异常</exception>
    private static Curve3d[] ToOrderedArray(this IEnumerable<Curve3d> source)
    {
        var tol = new Tolerance(0.001, 0.001);
        var list = source.ToList();
        var count = list.Count;
        var array = new Curve3d[count];
        var i = 0;
        array[0] = list[0];
        list.RemoveAt(0);
        while (i < count - 1)
        {
            var pt = array[i++].EndPoint;
            int index;
            if ((index = list.FindIndex(c => c.StartPoint.IsEqualTo(pt, tol))) != -1)
                array[i] = list[index];
            else if ((index = list.FindIndex(c => c.EndPoint.IsEqualTo(pt, tol))) != -1)
                array[i] = list[index].GetReverseParameterCurve();
            else
                throw new ArgumentException("非连续曲线.");
            list.RemoveAt(index);
        }

        return array;
    }
}
#endif
#endif