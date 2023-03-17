namespace IFoxCAD.Cad;

/// <summary>
/// 单行文字扩展类
/// </summary>
public static class DBTextEx
{
    /// <summary>
    /// 创建单行文字
    /// </summary>
    /// <param name="position">插入点</param>
    /// <param name="text">文本内容</param>
    /// <param name="height">文字高度</param>
    /// <param name="database">文字所在的数据库</param>
    /// <param name="action">文字属性设置委托</param>
    /// <returns>文字对象</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DBText CreateDBText(Point3d position, string text, double height, Database? database = null,Action<DBText>? action = null)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentNullException(nameof(text), "创建文字无内容");

        var acText = new DBText();

        acText.SetDatabaseDefaults(database ?? DBTrans.Top.Database);

        acText.Height = height;
        acText.TextString = text;
        acText.Position = position; // 插入点(一定要先设置)

        acText.Justify = AttachmentPoint.BaseLeft; // 使他们对齐

        action?.Invoke(acText);

        if (acText.Justify != AttachmentPoint.BaseLeft)
            acText.AlignmentPoint = position;

        acText.AdjustAlignment(database ?? DBTrans.Top.Database);
        return acText;
    }

    /// <summary>
    /// 添加单行文字到块表记录
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="position">插入点</param>
    /// <param name="text">文本内容</param>
    /// <param name="height">文字高度</param>
    /// <param name="action">文字属性设置委托</param>
    /// <returns>文字对象id</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ObjectId AddDBText(this BlockTableRecord btr, Point3d position, string text, double height, Action<DBText>? action = null)
    {
        var acText = CreateDBText(position, text, height, btr.Database, action);
        return btr.AddEntity(acText);
    }

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