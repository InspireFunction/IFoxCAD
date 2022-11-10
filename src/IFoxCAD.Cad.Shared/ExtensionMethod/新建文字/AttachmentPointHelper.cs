namespace IFoxCAD.Cad;

public static class AttachmentPointHelper
{
    static readonly Dictionary<string, AttachmentPoint> _alignment = new()
    {
        { "左上", AttachmentPoint.TopLeft },
        { "中上", AttachmentPoint.TopCenter },// 单行的对齐
        { "右上", AttachmentPoint.TopRight },

        { "左中", AttachmentPoint.MiddleLeft },
        { "正中", AttachmentPoint.MiddleCenter },// 多行的正中
        { "右中", AttachmentPoint.MiddleRight },

        { "左对齐", AttachmentPoint.BaseLeft },// ※优先(放在前面优先获取)
        { "左", AttachmentPoint.BaseLeft },

        { "中间", AttachmentPoint.BaseMid },

        { "右对齐", AttachmentPoint.BaseRight },// ※优先(放在前面优先获取)
        { "右", AttachmentPoint.BaseRight },

        { "左下", AttachmentPoint.BottomLeft },
        { "中下", AttachmentPoint.BottomCenter },
        { "右下", AttachmentPoint.BottomRight },

        { "对齐", AttachmentPoint.BaseAlign },// ※优先(放在前面优先获取)
        { "调整", AttachmentPoint.BaseAlign },

        { "居中", AttachmentPoint.BaseCenter },// 单行的中
        { "铺满", AttachmentPoint.BaseFit },
    };

    /// <summary>
    /// 输入文字获得对齐方式
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static AttachmentPoint Get(string key)
    {
        return _alignment[key];
    }

    /// <summary>
    /// 输入对齐方式获得文字
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Get(AttachmentPoint value)
    {
        return _alignment.FirstOrDefault(q => q.Value == value).Key;
    }
}

#if false
// 反射描述
// 这些东西cad没有用到啊...所以不纳入了
public enum AttachmentPoint2
{
    [Description("下对齐")]
    BottomAlign = 14,
    [Description("中对齐")]
    MiddleAlign = 15,// 0xF
    [Description("上对齐")]
    TopAlign = 16,// 0x10
    [Description("下铺满")]
    BottomFit = 18,
    [Description("中铺满")]
    MiddleFit = 19,
    [Description("上铺满")]
    TopFit = 20,
    [Description("下居中")]
    BottomMid = 22,
    [Description("中居中")]
    MiddleMid = 23,
    [Description("下居中")]
    TopMid = 24,
}

public static Dictionary<string, string> GetEnumDic(Type enumType)
{
    Dictionary<string, string> dic = new();
    var fieldinfos = enumType.GetFields();
    for (int i = 0; i < fieldinfos.Length; i++)
    {
        var field = fieldinfos[i];
        if (field.FieldType.IsEnum)
        {
            var objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            dic.Add(field.Name, ((DescriptionAttribute)objs[0]).Description);
        }
    }
    return dic;
}
#endif