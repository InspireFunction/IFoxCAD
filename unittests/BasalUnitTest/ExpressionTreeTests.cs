using System.Linq.Expressions;

namespace BasalUnitTests;

public class ExpressionTreeTests
{
    [Fact]
    public void SimpleExpression_ParameterAndBody_CanBeDecomposed()
    {
        Expression<Func<int, bool>> exprTree = num => num < 5;

        var param = exprTree.Parameters[0];
        var operation = (BinaryExpression)exprTree.Body;
        var left = (ParameterExpression)operation.Left;
        var right = (ConstantExpression)operation.Right;

        Assert.Equal("num", param.Name);
        Assert.Equal("num", left.Name);
        Assert.Equal(ExpressionType.LessThan, operation.NodeType);
        Assert.Equal(5, right.Value);
    }

    [Fact]
    public void ComplexExpression_CannotDirectlyCastToSimpleTypes()
    {
        Expression<Func<int, bool>> exprTree = x => x > 5 && x < 50;

        var operation = (BinaryExpression)exprTree.Body;

        Assert.Equal(ExpressionType.AndAlso, operation.NodeType);
        Assert.IsAssignableFrom<BinaryExpression>(operation.Left);
        Assert.IsAssignableFrom<BinaryExpression>(operation.Right);
    }

    [Fact]
    public void RebuildExpression_SingleCondition_WorksCorrectly()
    {
        var names = new List<string> { "Cai", "Edward", "Beauty" };
        Expression<Func<string, bool>> lambda = name => name.Length > 2 && name.Length < 4;

        var method = RebuildExpression(lambda);
        var query = names.Where(method);

        Assert.Single(query);
        Assert.Contains("Cai", query);
    }

    [Fact]
    public void RebuildExpression_MergedConditions_WorksCorrectly()
    {
        var names = new List<string> { "Cai", "Edward", "Beauty", "Li" };
        Expression<Func<string, bool>> lambda0 = item => item.Length > 2;
        Expression<Func<string, bool>> lambda1 = item => item.Length < 4;

        var method = ReBuildExpression(lambda0, lambda1);
        var query = names.Where(method);

        Assert.Single(query);
        Assert.Contains("Cai", query);
    }

    [Fact]
    public void ExpressionVisitor_CanModifyParameter()
    {
        Expression<Func<string, bool>> lambda = name => name.Length > 2;
        var visitor = new TestExpressionVisitor
        {
            Parameter = Expression.Parameter(typeof(string), "newParam")
        };

        var newBody = visitor.Modify(lambda.Body);
        var newLambda = Expression.Lambda<Func<string, bool>>(newBody, visitor.Parameter);

        Assert.Equal("newParam", newLambda.Parameters[0].Name);
    }

    [Fact]
    public void CompiledExpression_ProducesCorrectResult()
    {
        Expression<Func<int, int, int>> addExpr = (a, b) => a + b;
        var compiled = addExpr.Compile();

        var result = compiled(3, 5);

        Assert.Equal(8, result);
    }

    [Fact]
    public void ExpressionType_AndAlso_CombinesConditions()
    {
        Expression<Func<int, bool>> leftExpr = x => x > 0;
        Expression<Func<int, bool>> rightExpr = x => x < 10;

        var visitor = new TestExpressionVisitor
        {
            Parameter = Expression.Parameter(typeof(int), "x")
        };

        var left = visitor.Modify(leftExpr.Body);
        var right = visitor.Modify(rightExpr.Body);
        var combined = Expression.AndAlso(left, right);

        var lambda = Expression.Lambda<Func<int, bool>>(combined, visitor.Parameter);
        var compiled = lambda.Compile();

        Assert.True(compiled(5));
        Assert.False(compiled(0));
        Assert.False(compiled(10));
        Assert.False(compiled(-1));
    }

    private static Func<string, bool> RebuildExpression(Expression<Func<string, bool>> lambda)
    {
        var visitor = new TestExpressionVisitor
        {
            Parameter = Expression.Parameter(typeof(string), "name")
        };

        var newBody = visitor.Modify(lambda.Body);
        var newLambda = Expression.Lambda<Func<string, bool>>(newBody, visitor.Parameter);
        return newLambda.Compile();
    }

    private static Func<string, bool> ReBuildExpression(
        Expression<Func<string, bool>> lambda0,
        Expression<Func<string, bool>> lambda1)
    {
        var visitor = new TestExpressionVisitor
        {
            Parameter = Expression.Parameter(typeof(string), "name")
        };

        var left = visitor.Modify(lambda0.Body);
        var right = visitor.Modify(lambda1.Body);
        var expression = Expression.AndAlso(left, right);

        var newLambda = Expression.Lambda<Func<string, bool>>(expression, visitor.Parameter);
        return newLambda.Compile();
    }
}

public class TestExpressionVisitor : ExpressionVisitor
{
    public ParameterExpression? Parameter { get; set; }

    public Expression Modify(Expression exp)
    {
        return Visit(exp);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        return Parameter ?? p;
    }
}
