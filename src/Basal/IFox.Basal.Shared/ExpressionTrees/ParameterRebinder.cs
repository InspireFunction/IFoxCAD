#if NET45_OR_GREATER
namespace IFoxCAD.Basal;

/// <summary>
/// �����ذ�
/// </summary>
public class ParameterRebinder : SqlExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> map;
    /// <summary>
    /// �����ذ�
    /// </summary>
    /// <param name="map">�ֵ�</param>
    public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
    {
        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
    }
    /// <summary>
    /// �滻����
    /// </summary>
    /// <param name="map">�ֵ�</param>
    /// <param name="expression">���ʽ</param>
    /// <returns>���ʽ</returns>
    public static Expression? ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression expression)
    {
        return new ParameterRebinder(map).Visit(expression);
    }
    /// <summary>
    /// ���ʲ���
    /// </summary>
    /// <param name="expression">�������ʽ</param>
    /// <returns>���ʽ</returns>
    protected override Expression VisitParameter(ParameterExpression expression)
    {
        if (map.TryGetValue(expression, out var parameterExpression))
            expression = parameterExpression;

        return base.VisitParameter(expression);
    }
}
#endif