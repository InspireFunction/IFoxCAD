#if NET45_OR_GREATER
namespace IFoxCAD.Basal;

/// <summary>
/// 参数重绑定
/// </summary>
public class ParameterRebinder : SqlExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> map;
    /// <summary>
    /// 参数重绑定
    /// </summary>
    /// <param name="map">字典</param>
    public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
    {
        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
    }
    /// <summary>
    /// 替换参数
    /// </summary>
    /// <param name="map">字典</param>
    /// <param name="expression">表达式</param>
    /// <returns>表达式</returns>
    public static Expression? ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression expression)
    {
        return new ParameterRebinder(map).Visit(expression);
    }
    /// <summary>
    /// 访问参数
    /// </summary>
    /// <param name="expression">参数表达式</param>
    /// <returns>表达式</returns>
    protected override Expression VisitParameter(ParameterExpression expression)
    {
        if (map.TryGetValue(expression, out var parameterExpression))
            expression = parameterExpression;

        return base.VisitParameter(expression);
    }
}
#endif