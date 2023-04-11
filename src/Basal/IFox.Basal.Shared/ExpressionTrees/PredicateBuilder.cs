
#if NET45_OR_GREATER
namespace IFoxCAD.Basal;
/// <summary>
/// Predicateί�й�����
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    /// �������ί�б��ʽ
    /// </summary>
    /// <typeparam name="T">��������ķ���</typeparam>
    /// <returns>��</returns>
    public static Expression<Func<T, bool>> True<T>()
    {
        return param => true;
    }
    /// <summary>
    /// ���ؼٵ�ί�б��ʽ
    /// </summary>
    /// <typeparam name="T">��������ķ���</typeparam>
    /// <returns>��</returns>
    public static Expression<Func<T, bool>> False<T>()
    {
        return param => false;
    }
    /// <summary>
    /// ����predicateί��
    /// </summary>
    /// <typeparam name="T">�����������</typeparam>
    /// <param name="predicate">ί�б��ʽ</param>
    /// <returns>ί�б��ʽ</returns>
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
    {
        return predicate;
    }
    /// <summary>
    /// ��ʾ���ı��ʽ
    /// </summary>
    /// <typeparam name="T">�����������</typeparam>
    /// <param name="first">��һ������</param>
    /// <param name="second">�ڶ���</param>
    /// <returns>���ʽ</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.AndAlso);
    }
    /// <summary>
    /// ��ʾ��ı��ʽ
    /// </summary>
    /// <typeparam name="T">�����������</typeparam>
    /// <param name="first">��һ������</param>
    /// <param name="second">�ڶ���</param>
    /// <returns>���ʽ</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.OrElse);
    }
    /// <summary>
    /// ���Ƿ�ı��ʽ
    /// </summary>
    /// <typeparam name="T">�����������</typeparam>
    /// <param name="expression">���ʽ</param>
    /// <returns>���ʽ</returns>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
    }

    private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
        var map = first.Parameters.Select((f, i) => new{f,s = second.Parameters[i]}).ToDictionary(p => p.s, p => p.f);
        var expression = ParameterRebinder.ReplaceParameters(map, second.Body);
        if (expression != null)
        {
            return Expression.Lambda<T>(merge(first.Body, expression), first.Parameters);
        }
        return first;
        
    }
}

#endif