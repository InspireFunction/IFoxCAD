namespace IFoxCAD.Cad;
#if false
public static partial class EntityAdd
{
    /// <summary>
    /// 创建单行文字
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="textContents">内容</param>
    /// <param name="position">插入点</param>
    /// <param name="textHigh">字体高度</param>
    /// <param name="textStyleId">文字样式</param>
    /// <param name="justify">对齐方式</param>
    /// <param name="justifyPoint">对齐点,因样式 <see langword="justify"/> 可能无效</param>
    /// <returns></returns>
    public static Entity AddDBTextToEntity(this Database db,
        string textContents,
        Point3d position,
        double textHigh = 2.5,
        ObjectId? textStyleId = null,
        AttachmentPoint justify = AttachmentPoint.BaseLeft,
        Point3d? justifyPoint = null)
    {
        var TextInfo = new TextInfo(
            textContents,
            position,
            justify,
            justifyPoint,
            textStyleId,
            textHigh);
        return TextInfo.AddDBTextToEntity();
    }

    /// <summary>
    /// 新建多行文字
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="textContents">内容</param>
    /// <param name="position">插入点</param>
    /// <param name="textHigh">字体高度</param>
    /// <param name="textStyleId">文字样式</param>
    /// <param name="justify">对齐方式</param>
    /// <returns></returns>
    public static Entity AddMTextToEntity(this Database db,
        string textContents,
        Point3d position,
        double textHigh = 2.5,
        ObjectId? textStyleId = null,
        AttachmentPoint justify = AttachmentPoint.BaseLeft)
    {
        var TextInfo = new TextInfo(
            textContents,
            position,
            justify,
            null,
            textStyleId,
            textHigh);
        return TextInfo.AddMTextToEntity();
    }
}

#endif