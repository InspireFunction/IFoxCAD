namespace IFoxCAD.Cad;

public static class ObjEx
{
    /// <summary>
    /// cad的打印
    /// </summary>
    /// <param name="obj"></param>
    public static void Print(this object obj)
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"{obj}\n");
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
