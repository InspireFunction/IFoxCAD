namespace IFoxCAD.Cad;

/// <summary>
/// 填充扩展
/// </summary>
public static class HatchEx
{
    /// <summary>
    /// 遍历填充每条边
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="action"></param>
    public static void ForEach(this Hatch hatch, Action<HatchLoop> action)
    {
        for (int i = 0; i < hatch.NumberOfLoops; i++)
            action.Invoke(hatch.GetLoopAt(i));
    }
}