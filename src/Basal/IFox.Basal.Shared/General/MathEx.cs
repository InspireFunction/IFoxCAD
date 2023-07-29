namespace IFoxCAD.Basal;

/// <summary>
/// 数学函数扩展类
/// </summary>
public static class MathEx
{
    /// <summary>
    /// 转换弧度到角度
    /// </summary>
    /// <param name="rad">弧度值</param>
    /// <returns>角度（10进制小数）</returns>
    public static double ConvertRadToDeg(double rad) => rad / Math.PI * 180;

    /// <summary>
    /// 转换角度（10进制小数）到弧度
    /// </summary>
    /// <param name="deg">角度</param>
    /// <returns>弧度</returns>
    public static double ConvertDegToRad(double deg) => deg / 180 * Math.PI;
}

