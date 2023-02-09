namespace IFoxCAD.Cad;

/// <summary>
/// 文字信息类
/// </summary>
public class TextInfo
{
    readonly Database? Database;
    readonly string? Contents;
    readonly Point3d Position;
    /// <summary>
    /// 文字对齐方式的中文说明
    /// </summary>
    public string TextJustifyCn => AttachmentPointHelper.Get(TextJustify);
    readonly AttachmentPoint TextJustify;
    readonly Point3d? AlignmentPoint;

    readonly double TextHeight;
    readonly ObjectId? TextStyleId;

    /// <summary>
    /// 文字信息类
    /// </summary>
    /// <param name="contents">内容</param>
    /// <param name="position">基点</param>
    /// <param name="justify">对齐方式</param>
    /// <param name="justifyPoint">对齐点(对齐方式是左,此参数无效,为null不为左就报错)</param>
    /// <param name="textStyleId">文字样式id</param>
    /// <param name="textHeight">文字高度</param>
    /// <param name="database">数据库</param>
    public TextInfo(string? contents,
        Point3d position,
        AttachmentPoint justify,
        Point3d? justifyPoint = null,
        ObjectId? textStyleId = null,
        double textHeight = 2.5,
        Database? database = null)
    {
        Contents = contents;
        Position = position;
        TextJustify = justify;

        if (justifyPoint is null && TextJustify != AttachmentPoint.BaseLeft)
            throw new ArgumentNullException(nameof(justifyPoint));

        AlignmentPoint = justifyPoint;
        TextHeight = textHeight;
        TextStyleId = textStyleId;
        Database = database;
    }

    /// <summary>
    /// 创建单行文字
    /// </summary>
    public DBText AddDBTextToEntity()
    {
        if (string.IsNullOrEmpty(Contents))
            throw new ArgumentNullException(nameof(Contents) + "创建文字无内容");

        var acText = new DBText();
     
        acText.SetDatabaseDefaults(Database ?? DBTrans.Top.Database);

        if (TextStyleId is not null)
            acText.SetTextStyleId(TextStyleId.Value);

        acText.Height = TextHeight;     // 高度
        acText.TextString = Contents;   // 内容
        acText.Position = Position;     // 插入点(一定要先设置)
        acText.Justify = TextJustify;   // 使他们对齐
        // acText.HorizontalMode

        if (AlignmentPoint is not null)
            acText.AlignmentPoint = AlignmentPoint.Value;
        else if (acText.Justify != AttachmentPoint.BaseLeft)
            acText.AlignmentPoint = Position;


        acText.AdjustAlignment(Database ?? DBTrans.Top.Database);
        return acText;
    }

    /// <summary>
    /// 创建多行文字
    /// </summary>
    /// <returns></returns>
    public MText AddMTextToEntity()
    {
        if (string.IsNullOrEmpty(Contents))
            throw new ArgumentNullException(nameof(Contents) + "创建文字无内容");

        var mText = new MText();

        mText.SetDatabaseDefaults(Database ?? DBTrans.Top.Database);

        if (TextStyleId is not null)
            mText.SetTextStyleId(TextStyleId.Value);

        mText.TextHeight = TextHeight; // 高度
        mText.Contents = Contents;     // 内容
        mText.Location = Position;     // 插入点(一定要先设置)

        // mText.SetAttachmentMovingLocation(TextJustify);
        mText.Attachment = TextJustify;// 使他们对齐

        return mText;
    }
}


/// <summary>
/// 反射设定对象的文字样式id
/// </summary>
public static partial class TextInfoHelper
{
    /// <summary>
    /// 设置文字样式id
    /// </summary>
    /// <param name="acText">单行文字</param>
    /// <param name="ltrObjectId">文字样式表记录id</param>
    public static void SetTextStyleId(this DBText acText, ObjectId ltrObjectId)
    {
        SetEntityTxtStyleId(acText, ltrObjectId);
    }

    /// <summary>
    /// 设置文字样式id
    /// </summary>
    /// <param name="acText">多行文字</param>
    /// <param name="ltrObjectId">文字样式表记录id</param>
    public static void SetTextStyleId(this MText acText, ObjectId ltrObjectId)
    {
        SetEntityTxtStyleId(acText, ltrObjectId);
    }

    static void SetEntityTxtStyleId(Entity acText, ObjectId ltrObjectId)
    {
        GetTextStyleIdType(acText)?.SetValue(acText, ltrObjectId, null);
    }

    /// <summary>
    /// 获取文字样式id
    /// </summary>
    public static ObjectId GetTextStyleId(this DBText acText)
    {
        return GetEntityTxtStyleId(acText);
    }

    /// <summary>
    /// 获取文字样式id
    /// </summary>
    public static ObjectId GetTextStyleId(this MText acText)
    {
        return GetEntityTxtStyleId(acText);
    }

    static ObjectId GetEntityTxtStyleId(Entity acText)
    {
        var result = ObjectId.Null;
        var id = GetTextStyleIdType(acText)?.GetValue(acText, null);
        if (id != null)
            result = (ObjectId)id;
        return result;
    }

    static PropertyInfo? _textStyleId = null;
    static PropertyInfo GetTextStyleIdType(Entity acText)
    {
        if (_textStyleId == null)
        {
            var entType = acText.GetType();
            var prs = entType.GetProperties();
            _textStyleId = prs.FirstOrDefault(a => a.Name == "TextStyle");// 反射获取属性
            if (_textStyleId == null)
                _textStyleId = prs.FirstOrDefault(a => a.Name == "TextStyleId");// 反射获取属性
        }
        return _textStyleId;
    }
}
