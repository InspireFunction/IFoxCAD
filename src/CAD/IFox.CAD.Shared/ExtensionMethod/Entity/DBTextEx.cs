namespace IFoxCAD.Cad;

/// <summary>
/// 单行文字扩展类
/// </summary>
public static class DBTextEx
{

    /// <summary>
    /// 更正单行文字的镜像属性
    /// </summary>
    /// <param name="txt">单行文字</param>
    public static void ValidateMirror(this DBText txt)
    {
        if (!txt.Database.Mirrtext)
        {
            txt.IsMirroredInX = false;
            txt.IsMirroredInY = false;
        }
    }

}