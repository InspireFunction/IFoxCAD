namespace IFoxCAD.Cad;

/// <summary>
/// 文字对齐点帮助类
/// </summary>
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
    /// 输入对齐方式获得文字说明
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Get(AttachmentPoint value)
    {
        return _alignment.FirstOrDefault(q => q.Value == value).Key;
    }
}
