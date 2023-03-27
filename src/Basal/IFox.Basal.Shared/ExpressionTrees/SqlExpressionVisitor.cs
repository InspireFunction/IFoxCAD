#if NET45
using Microsoft.CSharp.RuntimeBinder;

namespace IFoxCAD.Basal;

/// <summary>
/// sql���ʽ��������
/// </summary>
public abstract class SqlExpressionVisitor
{
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="expression">���ʽ</param>
    /// <returns>���ʽ</returns>
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
    /// �����߰�
    /// </summary>
    /// <param name="binding">�󶨵���</param>
    /// <returns>�󶨵���</returns>
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
    /// ���ʼ��ϳ�ʼ�趨��
    /// </summary>
    /// <param name="initializer">���ϳ�ʼ�趨��</param>
    /// <returns>���ϳ�ʼ�趨��</returns>
    protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
        var arguments = VisitExpressionList(initializer.Arguments);

        if (arguments != initializer.Arguments)
            return Expression.ElementInit(initializer.AddMethod, arguments);

        return initializer;
    }
    /// <summary>
    /// ����һԪ�����
    /// </summary>
    /// <param name="unary">һԪ�����</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitUnary(UnaryExpression unary)
    {
        var operand = Visit(unary.Operand);

        if (operand != unary.Operand)
            return Expression.MakeUnary(unary.NodeType, operand, unary.Type, unary.Method);

        return unary;
    }
    /// <summary>
    /// ���ʶ����������
    /// </summary>
    /// <param name="binary">�����������</param>
    /// <returns>���ʽ</returns>
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
    /// ��������
    /// </summary>
    /// <param name="typeBinary">����</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinary)
    {
        var expression = Visit(typeBinary.Expression);

        if (expression != typeBinary.Expression)
            return Expression.TypeIs(expression, typeBinary.TypeOperand);

        return typeBinary;
    }
    /// <summary>
    /// ���ʳ���ֵ
    /// </summary>
    /// <param name="constant">����ֵ</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitConstant(ConstantExpression constant)
    {
        return constant;
    }
    /// <summary>
    /// �������������
    /// </summary>
    /// <param name="conditional">���������</param>
    /// <returns>���ʽ</returns>
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
    /// ���ʲ���
    /// </summary>
    /// <param name="parameter">����</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitParameter(ParameterExpression parameter)
    {
        return parameter;
    }
    /// <summary>
    /// ���ʳ�Ա
    /// </summary>
    /// <param name="member">��Ա</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitMemberAccess(MemberExpression member)
    {
        var expression = Visit(member.Expression);

        if (expression != member.Expression)
            return Expression.MakeMemberAccess(expression, member.Member);

        return member;
    }
    /// <summary>
    /// ���ʷ�������
    /// </summary>
    /// <param name="methodCall">��������</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitMethodCall(MethodCallExpression methodCall)
    {
        var instance = Visit(methodCall.Object);
        var arguments = (IEnumerable<Expression>)VisitExpressionList(methodCall.Arguments);

        if (instance != methodCall.Object || !Equals(arguments, methodCall.Arguments))
            return Expression.Call(instance, methodCall.Method, arguments);

        return methodCall;
    }
    /// <summary>
    /// ���ʱ��ʽ����
    /// </summary>
    /// <param name="original">���ʽ����</param>
    /// <returns>���ʽֻ������</returns>
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
    /// ���ʳ�Ա��ֵ
    /// </summary>
    /// <param name="assignment">��Ա��ֵ</param>
    /// <returns></returns>
    protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
        var expression = Visit(assignment.Expression);

        if (expression != assignment.Expression)
            return Expression.Bind(assignment.Member, expression);

        return assignment;
    }
    /// <summary>
    /// �����¶����Ա�ĳ�Ա
    /// </summary>
    /// <param name="binding">�¶����Ա�ĳ�Ա</param>
    /// <returns>�¶����Ա�ĳ�Ա</returns>
    protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
        var bindings = VisitBindingList(binding.Bindings);

        if (!Equals(bindings, binding.Bindings))
            return Expression.MemberBind(binding.Member, bindings);

        return binding;
    }
    /// <summary>
    /// ���ʳ�Ա��ʼ��
    /// </summary>
    /// <param name="binding">��Ա��ʼ��</param>
    /// <returns>��Ա��ʼ��</returns>
    protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
        var initializers = VisitElementInitializerList(binding.Initializers);

        if (!Equals(initializers, binding.Initializers))
            return Expression.ListBind(binding.Member, initializers);

        return binding;
    }
    /// <summary>
    /// ���ʳ�Ա��ʼ���б�
    /// </summary>
    /// <param name="original">��Ա��ʼ���б�</param>
    /// <returns>��Ա��ʼ���б�</returns>
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
    /// ���ʼ����趨���
    /// </summary>
    /// <param name="original">�����趨���</param>
    /// <returns>�����趨���</returns>
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
    /// ����lambda���ʽ
    /// </summary>
    /// <param name="lambda">lambda���ʽ</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitLambda(LambdaExpression lambda)
    {
        var body = Visit(lambda.Body);

        if (body != lambda.Body)
            return Expression.Lambda(lambda.Type, body, lambda.Parameters);

        return lambda;
    }
    /// <summary>
    /// ���ʹ��캯��
    /// </summary>
    /// <param name="expression">���캯��</param>
    /// <returns>���캯��</returns>
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
    /// ���ʳ�Ա��ʼ��
    /// </summary>
    /// <param name="memberInit">��Ա��ʼ��</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitMemberInit(MemberInitExpression memberInit)
    {
        var expression = VisitNew(memberInit.NewExpression);
        var bindings = VisitBindingList(memberInit.Bindings);

        if (expression != memberInit.NewExpression || !Equals(bindings, memberInit.Bindings))
            return Expression.MemberInit(expression, bindings);

        return memberInit;
    }
    /// <summary>
    /// ���ʼ��ϳ�ʼ��
    /// </summary>
    /// <param name="listInit">���ϳ�ʼ��</param>
    /// <returns>���ʽ</returns>
    protected virtual Expression VisitListInit(ListInitExpression listInit)
    {
        var expression = VisitNew(listInit.NewExpression);
        var initializers = VisitElementInitializerList(listInit.Initializers);

        if (expression != listInit.NewExpression || !Equals(initializers, listInit.Initializers))
            return Expression.ListInit(expression, initializers);

        return listInit;
    }
    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="newArray">������</param>
    /// <returns>���ʽ</returns>
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
    /// ����ί�е��ñ��ʽ
    /// </summary>
    /// <param name="invocation">ί�е��ñ��ʽ</param>
    /// <returns>���ʽ</returns>
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