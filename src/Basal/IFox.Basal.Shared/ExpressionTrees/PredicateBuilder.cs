
#if NET45
namespace IFoxCAD.Basal;
/// <summary>
/// Predicate委托构造器
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    /// 返回真的委托表达式
    /// </summary>
    /// <typeparam name="T">传入参数的泛型</typeparam>
    /// <returns>真</returns>
    public static Expression<Func<T, bool>> True<T>()
    {
        return param => true;
    }
    /// <summary>
    /// 返回假的委托表达式
    /// </summary>
    /// <typeparam name="T">传入参数的泛型</typeparam>
    /// <returns>假</returns>
    public static Expression<Func<T, bool>> False<T>()
    {
        return param => false;
    }
    /// <summary>
    /// 创建predicate委托
    /// </summary>
    /// <typeparam name="T">传入参数类型</typeparam>
    /// <param name="predicate">委托表达式</param>
    /// <returns>委托表达式</returns>
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
    {
        return predicate;
    }
    /// <summary>
    /// 表示并的表达式
    /// </summary>
    /// <typeparam name="T">传入参数类型</typeparam>
    /// <param name="first">第一个参数</param>
    /// <param name="second">第二个</param>
    /// <returns>表达式</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.AndAlso);
    }
    /// <summary>
    /// 表示或的表达式
    /// </summary>
    /// <typeparam name="T">传入参数类型</typeparam>
    /// <param name="first">第一个参数</param>
    /// <param name="second">第二个</param>
    /// <returns>表达式</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.OrElse);
    }
    /// <summary>
    /// 表是否的表达式
    /// </summary>
    /// <typeparam name="T">传入参数类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <returns>表达式</returns>
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