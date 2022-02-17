namespace IFoxCAD.WPF;

/// <summary>
/// 字符串到整数的转换器
/// </summary>
public class StringToIntConverter : IValueConverter
{
    /// <summary>
    /// 字符串转换到整数
    /// </summary>
    /// <param name="value">绑定源生成的值</param>
    /// <param name="targetType">绑定目标属性的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? a = value as string;
        _ = int.TryParse(a, out int b);
        return b;
    }
    /// <summary>
    /// 整数转换到字符串
    /// </summary>
    /// <param name="value">绑定目标生成的值</param>
    /// <param name="targetType">要转换为的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }
}
/// <summary>
/// 字符串到实数的转换器
/// </summary>
public class StringToDoubleConverter : IValueConverter
{
    /// <summary>
    /// 字符串转换到实数
    /// </summary>
    /// <param name="value">绑定源生成的值</param>
    /// <param name="targetType">绑定目标属性的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? a = value as string;
        _ = double.TryParse(a, out double b);
        return b;
    }
    /// <summary>
    /// 实数转换到字符串
    /// </summary>
    /// <param name="value">绑定目标生成的值</param>
    /// <param name="targetType">要转换为的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }
}
/// <summary>
/// 整数到字符串的转换器
/// </summary>
public class IntToStringConverter : IValueConverter
{
    /// <summary>
    /// 整数转换到字符串
    /// </summary>
    /// <param name="value">绑定源生成的值</param>
    /// <param name="targetType">绑定目标属性的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }
    /// <summary>
    /// 字符串转换到整数
    /// </summary>
    /// <param name="value">绑定目标生成的值</param>
    /// <param name="targetType">要转换为的类型</param>
    /// <param name="parameter">要使用的转换器参数</param>
    /// <param name="culture">要用在转换器中的区域性</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? a = value as string;
        _ = int.TryParse(a, out int b);
        return b;
    }
}
