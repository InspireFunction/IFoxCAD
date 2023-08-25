namespace IFoxCAD.Cad;

/// <summary>
/// 对象扩展类
/// </summary>
public static class ObjEx
{
    /// <summary>
    /// cad的打印
    /// </summary>
    /// <param name="obj"></param>
    public static void Print(this object obj)
    {
        Acap.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n{obj}\n");
    }
    /// <summary>
    /// 系统的打印
    /// </summary>
    /// <param name="obj"></param>
    public static void PrintLine(this object obj)
    {
        Console.WriteLine(obj.ToString());
    }
}