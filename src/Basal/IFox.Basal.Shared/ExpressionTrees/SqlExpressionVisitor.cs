#if NET45
using Microsoft.CSharp.RuntimeBinder;

namespace IFoxCAD.Basal;

/// <summary>
/// sql表达式访问者类
/// </summary>
public abstract class SqlExpressionVisitor
{
    /// <summary>
    /// 访问
    /// </summary>
    /// <param name="expression">表达式</param>
    /// <returns>表达式</returns>
    /// <exception cref="RuntimeBinderException"></exception>
    protected virtual Expression? Visit(Expression expression)
    {
        if (expression is null)
            return null;

        return expression.NodeType switch
        {
            ExpressionType.Add => VisitBinary((BinaryExpression)expression),
            ExpressionType.AddChecked => VisitBinary((BinaryExpression)expression),
            ExpressionType.And => VisitBinary((BinaryExpression)expression),
            ExpressionType.AndAlso => VisitBinary((BinaryExpression)expression),
            ExpressionType.ArrayIndex => VisitBinary((BinaryExpression)expression),
            ExpressionType.Coalesce => VisitBinary((BinaryExpression)expression),
            ExpressionType.Divide => VisitBinary((BinaryExpression)expression),
            ExpressionType.Equal => VisitBinary((BinaryExpression)expression),
            ExpressionType.ExclusiveOr => VisitBinary((BinaryExpression)expression),
            ExpressionType.GreaterThan => VisitBinary((BinaryExpression)expression),
            ExpressionType.GreaterThanOrEqual => VisitBinary((BinaryExpression)expression),
            ExpressionType.LeftShift => VisitBinary((BinaryExpression)expression),
            ExpressionType.LessThan => VisitBinary((BinaryExpression)expression),
            ExpressionType.LessThanOrEqual => VisitBinary((BinaryExpression)expression),
            ExpressionType.Modulo => VisitBinary((BinaryExpression)expression),
            ExpressionType.Multiply => VisitBinary((BinaryExpression)expression),
            ExpressionType.MultiplyChecked => VisitBinary((BinaryExpression)expression),
            ExpressionType.NotEqual => VisitBinary((BinaryExpression)expression),
            ExpressionType.Or => VisitBinary((BinaryExpression)expression),
            ExpressionType.OrElse => VisitBinary((BinaryExpression)expression),
            ExpressionType.RightShift => VisitBinary((BinaryExpression)expression),
            ExpressionType.Subtract => VisitBinary((BinaryExpression)expression),
            ExpressionType.SubtractChecked => VisitBinary((BinaryExpression)expression),
            ExpressionType.ArrayLength => VisitUnary((UnaryExpression)expression),
            ExpressionType.Convert => VisitUnary((UnaryExpression)expression),
            ExpressionType.ConvertChecked => VisitUnary((UnaryExpression)expression),
            ExpressionType.Negate => VisitUnary((UnaryExpression)expression),
            ExpressionType.NegateChecked => VisitUnary((UnaryExpression)expression),
            ExpressionType.Not => VisitUnary((UnaryExpression)expression),
            ExpressionType.Quote => VisitUnary((UnaryExpression)expression),
            ExpressionType.TypeAs => VisitUnary((UnaryExpression)expression),
            ExpressionType.Call => VisitMethodCall((MethodCallExpression)expression),
            ExpressionType.Conditional => VisitConditional((ConditionalExpression)expression),
            ExpressionType.Constant => VisitConstant((ConstantExpression)expression),
            ExpressionType.Invoke => VisitInvocation((InvocationExpression)expression),
            ExpressionType.Lambda => VisitLambda((LambdaExpression)expression),
            ExpressionType.ListInit => VisitListInit((ListInitExpression)expression),
            ExpressionType.MemberAccess => VisitMemberAccess((MemberExpression)expression),
            ExpressionType.MemberInit => VisitMemberInit((MemberInitExpression)expression),
            ExpressionType.New => VisitNew((NewExpression)expression),
            ExpressionType.NewArrayInit => VisitNewArray((NewArrayExpression)expression),
            ExpressionType.NewArrayBounds => VisitNewArray((NewArrayExpression)expression),
            ExpressionType.Parameter => VisitParameter((ParameterExpression)expression),
            ExpressionType.TypeIs => VisitTypeIs((TypeBinaryExpression)expression),
            _ => throw new RuntimeBinderException(nameof(expression.NodeType))
        };
    }
    /// <summary>
    /// 访问者绑定
    /// </summary>
    /// <param name="binding">绑定的类</param>
    /// <returns>绑定的类</returns>
    /// <exception cref="RuntimeBinderException"></exception>
    protected virtual MemberBinding VisitBinding(MemberBinding binding)
    {
        return binding.BindingType switch
        {
            MemberBindingType.Assignment => VisitMemberAssignment((MemberAssignment)binding),
            MemberBindingType.MemberBinding => VisitMemberMemberBinding((MemberMemberBinding)binding),
            MemberBindingType.ListBinding => VisitMemberListBinding((MemberListBinding)binding),
            _ => throw new RuntimeBinderException(nameof(binding.BindingType))
        };
    }
    /// <summary>
    /// 访问集合初始设定项
    /// </summary>
    /// <param name="initializer">集合初始设定项</param>
    /// <returns>集合初始设定项</returns>
    protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
        var arguments = VisitExpressionList(initializer.Arguments);

        if (arguments != initializer.Arguments)
            return Expression.ElementInit(initializer.AddMethod, arguments);

        return initializer;
    }
    /// <summary>
    /// 访问一元运算符
    /// </summary>
    /// <param name="unary">一元运算符</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitUnary(UnaryExpression unary)
    {
        var operand = Visit(unary.Operand);

        if (operand != unary.Operand)
            return Expression.MakeUnary(unary.NodeType, operand, unary.Type, unary.Method);

        return unary;
    }
    /// <summary>
    /// 访问二进制运算符
    /// </summary>
    /// <param name="binary">二进制运算符</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitBinary(BinaryExpression binary)
    {
        var left = Visit(binary.Left);
        var right = Visit(binary.Right);
        var conversion = Visit(binary.Conversion);

        if (left == binary.Left && right == binary.Right && conversion == binary.Conversion)
            return binary;

        if (binary.NodeType == ExpressionType.Coalesce && binary.Conversion != null)
            return Expression.Coalesce(left, right, conversion as LambdaExpression);

        return Expression.MakeBinary(binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method);
    }
    /// <summary>
    /// 访问类型
    /// </summary>
    /// <param name="typeBinary">类型</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinary)
    {
        var expression = Visit(typeBinary.Expression);

        if (expression != typeBinary.Expression)
            return Expression.TypeIs(expression, typeBinary.TypeOperand);

        return typeBinary;
    }
    /// <summary>
    /// 访问常数值
    /// </summary>
    /// <param name="constant">常数值</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitConstant(ConstantExpression constant)
    {
        return constant;
    }
    /// <summary>
    /// 访问条件运算符
    /// </summary>
    /// <param name="conditional">条件运算符</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitConditional(ConditionalExpression conditional)
    {
        var test = Visit(conditional.Test);
        var ifTrue = Visit(conditional.IfTrue);
        var ifFalse = Visit(conditional.IfFalse);

        if (test != conditional.Test)
            return Expression.Condition(test, ifTrue, ifFalse);

        if (ifTrue != conditional.IfTrue)
            return Expression.Condition(test, ifTrue, ifFalse);

        if (ifFalse != conditional.IfFalse)
            return Expression.Condition(test, ifTrue, ifFalse);

        return conditional;
    }
    /// <summary>
    /// 访问参数
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitParameter(ParameterExpression parameter)
    {
        return parameter;
    }
    /// <summary>
    /// 访问成员
    /// </summary>
    /// <param name="member">成员</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitMemberAccess(MemberExpression member)
    {
        var expression = Visit(member.Expression);

        if (expression != member.Expression)
            return Expression.MakeMemberAccess(expression, member.Member);

        return member;
    }
    /// <summary>
    /// 访问方法调用
    /// </summary>
    /// <param name="methodCall">方法调用</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitMethodCall(MethodCallExpression methodCall)
    {
        var instance = Visit(methodCall.Object);
        var arguments = (IEnumerable<Expression>)VisitExpressionList(methodCall.Arguments);

        if (instance != methodCall.Object || !Equals(arguments, methodCall.Arguments))
            return Expression.Call(instance, methodCall.Method, arguments);

        return methodCall;
    }
    /// <summary>
    /// 访问表达式集合
    /// </summary>
    /// <param name="original">表达式集合</param>
    /// <returns>表达式只读集合</returns>
    protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
        var index1 = 0;
        var expressions = default(List<Expression>);

        for (var count = original.Count; index1 < count; ++index1)
        {
            var expression = Visit(original[index1]);
            if (expression != null)
            {
                if (expressions != null)
                {
                    expressions.Add(expression);
                }

                else if (expression != original[index1])
                {
                    expressions = new List<Expression>(count);

                    for (var index2 = 0; index2 < index1; ++index2)
                        expressions.Add(original[index2]);

                    expressions.Add(expression);
                }
            }
            
        }

        return expressions != null ? expressions.AsReadOnly() : original;
    }
    /// <summary>
    /// 访问成员赋值
    /// </summary>
    /// <param name="assignment">成员赋值</param>
    /// <returns></returns>
    protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
        var expression = Visit(assignment.Expression);

        if (expression != assignment.Expression)
            return Expression.Bind(assignment.Member, expression);

        return assignment;
    }
    /// <summary>
    /// 访问新对象成员的成员
    /// </summary>
    /// <param name="binding">新对象成员的成员</param>
    /// <returns>新对象成员的成员</returns>
    protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
        var bindings = VisitBindingList(binding.Bindings);

        if (!Equals(bindings, binding.Bindings))
            return Expression.MemberBind(binding.Member, bindings);

        return binding;
    }
    /// <summary>
    /// 访问成员初始化
    /// </summary>
    /// <param name="binding">成员初始化</param>
    /// <returns>成员初始化</returns>
    protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
        var initializers = VisitElementInitializerList(binding.Initializers);

        if (!Equals(initializers, binding.Initializers))
            return Expression.ListBind(binding.Member, initializers);

        return binding;
    }
    /// <summary>
    /// 访问成员初始化列表
    /// </summary>
    /// <param name="original">成员初始化列表</param>
    /// <returns>成员初始化列表</returns>
    protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
        var index1 = 0;
        var bindings = default(List<MemberBinding>);

        for (var count = original.Count; index1 < count; ++index1)
        {
            var memberBinding = VisitBinding(original[index1]);

            if (bindings != null)
            {
                bindings.Add(memberBinding);
            }

            else if (memberBinding != original[index1])
            {
                bindings = new List<MemberBinding>(count);

                for (var index2 = 0; index2 < index1; ++index2)
                    bindings.Add(original[index2]);

                bindings.Add(memberBinding);
            }
        }

        return (IEnumerable<MemberBinding>)bindings! ?? original;
    }
    /// <summary>
    /// 访问集合设定项集合
    /// </summary>
    /// <param name="original">集合设定项集合</param>
    /// <returns>集合设定项集合</returns>
    protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
        var index1 = 0;
        var initializers = default(List<ElementInit>);

        for (var count = original.Count; index1 < count; ++index1)
        {
            var initializer = VisitElementInitializer(original[index1]);

            if (initializers != null)
            {
                initializers.Add(initializer);
            }

            else if (initializer != original[index1])
            {
                initializers = new List<ElementInit>(count);

                for (var index2 = 0; index2 < index1; ++index2)
                    initializers.Add(original[index2]);

                initializers.Add(initializer);
            }
        }

        return (IEnumerable<ElementInit>)initializers! ?? original;
    }
    /// <summary>
    /// 访问lambda表达式
    /// </summary>
    /// <param name="lambda">lambda表达式</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitLambda(LambdaExpression lambda)
    {
        var body = Visit(lambda.Body);

        if (body != lambda.Body)
            return Expression.Lambda(lambda.Type, body, lambda.Parameters);

        return lambda;
    }
    /// <summary>
    /// 访问构造函数
    /// </summary>
    /// <param name="expression">构造函数</param>
    /// <returns>构造函数</returns>
    protected virtual NewExpression VisitNew(NewExpression expression)
    {
        var arguments = VisitExpressionList(expression.Arguments);

        if (Equals(arguments, expression.Arguments))
            return expression;

        if (expression.Members != null)
            return Expression.New(expression.Constructor, arguments, expression.Members);

        return Expression.New(expression.Constructor, arguments);
    }
    /// <summary>
    /// 访问成员初始化
    /// </summary>
    /// <param name="memberInit">成员初始化</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitMemberInit(MemberInitExpression memberInit)
    {
        var expression = VisitNew(memberInit.NewExpression);
        var bindings = VisitBindingList(memberInit.Bindings);

        if (expression != memberInit.NewExpression || !Equals(bindings, memberInit.Bindings))
            return Expression.MemberInit(expression, bindings);

        return memberInit;
    }
    /// <summary>
    /// 访问集合初始化
    /// </summary>
    /// <param name="listInit">集合初始化</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitListInit(ListInitExpression listInit)
    {
        var expression = VisitNew(listInit.NewExpression);
        var initializers = VisitElementInitializerList(listInit.Initializers);

        if (expression != listInit.NewExpression || !Equals(initializers, listInit.Initializers))
            return Expression.ListInit(expression, initializers);

        return listInit;
    }
    /// <summary>
    /// 访问新数组
    /// </summary>
    /// <param name="newArray">新数组</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitNewArray(NewArrayExpression newArray)
    {
        var expressions = VisitExpressionList(newArray.Expressions);

        if (Equals(expressions, newArray.Expressions))
            return newArray;

        if (newArray.NodeType == ExpressionType.NewArrayInit)
            return Expression.NewArrayInit(newArray.Type.GetElementType(), expressions);

        return Expression.NewArrayBounds(newArray.Type.GetElementType(), expressions);
    }
    /// <summary>
    /// 访问委托调用表达式
    /// </summary>
    /// <param name="invocation">委托调用表达式</param>
    /// <returns>表达式</returns>
    protected virtual Expression VisitInvocation(InvocationExpression invocation)
    {
        var arguments = VisitExpressionList(invocation.Arguments);
        var expression = Visit(invocation.Expression);

        if (arguments != invocation.Arguments || expression != invocation.Expression)
            return Expression.Invoke(expression, arguments);

        return invocation;
    }
}
#endif