using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace IFoxFunKit;

/// <summary>
/// 检查在访问 Value/OkValue/ErrValue 之前是否检查了对应的状态属性
/// 例如：在使用 opt.Value 之前必须先检查 opt.IsSome
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustCheckBeforeAccessAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// 诊断器ID
    /// </summary>
    public const string DiagnosticId = DiagnosticMessages.ErrorCodes.MustCheckBeforeAccess;

    private static readonly DiagnosticDescriptor Rule = DiagnosticMessages.GetDescriptor(DiagnosticId);

    /// <summary>
    /// 支持的诊断描述符集合
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    // 值访问成员与状态检查成员的映射关系
    // 使用 nameof 确保编译时检查，避免硬编码字符串的错误
    private static readonly ImmutableDictionary<string, string[]> ValueAccessToStatusChecks =
        new System.Collections.Generic.Dictionary<string, string[]>
        {
            [nameof(Option<object>.Value)] = new[] { nameof(Option<object>.IsSome), nameof(Option<object>.IsNone) },
            [nameof(Result<object, object>.OkValue)] = new[] { nameof(Result<object, object>.IsOk), nameof(Result<object, object>.IsErr) },
            [nameof(Result<object, object>.ErrValue)] = new[] { nameof(Result<object, object>.IsErr), nameof(Result<object, object>.IsOk) }
        }.ToImmutableDictionary();

    /// <summary>
    /// 初始化分析器
    /// </summary>
    /// <param name="context">分析上下文</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // 分析成员访问表达式
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        var memberName = memberAccess.Name.Identifier.Text;

        // 检查是否是值访问成员
        if (!MustHandleMeta.ValueAccessMemberNames.Contains(memberName))
            return;

        // 检查是否标记了跳过特性
        if (HasSkipMustHandleCheckAttribute(context))
            return;

        // 获取被访问的表达式（如 opt.Value 中的 opt）
        var targetExpression = memberAccess.Expression;

        // 检查被访问的对象是否是 Option<T> 或 Result<T> 类型
        if (!IsOptionOrResultType(context, targetExpression))
            return;

        // 检查当前上下文是否已经验证了状态
        if (IsStateCheckedInContext(context, targetExpression, memberName))
            return;

        // 报告错误
        var statusChecks = ValueAccessToStatusChecks.TryGetValue(memberName, out var checks)
            ? string.Join(" 或 ", checks)
            : "对应的状态属性";

        // 获取完整的成员访问表达式文本（如 "opt.Value"）
        var fullExpressionText = memberAccess.ToString();

        // 创建带有格式化消息的 DiagnosticDescriptor
        var formattedRule = DiagnosticMessages.GetFormattedDescriptor(DiagnosticId, fullExpressionText, statusChecks);
        var diagnostic = Diagnostic.Create(formattedRule, memberAccess.Name.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// 检查表达式是否是 Option&lt;T&gt; 或 Result&lt;T&gt; 类型
    /// </summary>
    private static bool IsOptionOrResultType(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(expression);
        var type = typeInfo.Type;

        if (type == null)
            return false;

        var typeName = type.ToDisplayString();

        // 检查是否是 Option<T> 类型（包括原始类型名和显示名称）
        if (typeName.StartsWith(MustHandleMeta.OptionTypeFullName) ||
            typeName.StartsWith("Option<"))
        {
            return true;
        }

        // 检查是否是 Result<T, TError> 类型
        if (typeName.StartsWith(MustHandleMeta.ResultTypeFullName) ||
            typeName.StartsWith("Result<"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查在当前上下文中是否已经验证了状态
    /// </summary>
    private static bool IsStateCheckedInContext(SyntaxNodeAnalysisContext context, ExpressionSyntax targetExpression, string valueMemberName)
    {
        // 获取需要检查的状态成员
        if (!ValueAccessToStatusChecks.TryGetValue(valueMemberName, out var requiredStatusChecks))
            return false;

        // 向上遍历语法树，检查是否在条件分支中
        var currentNode = context.Node.Parent;

        while (currentNode != null)
        {
            // 检查是否在 if 语句的条件为真的分支中
            if (currentNode is IfStatementSyntax ifStatement)
            {
                // 检查当前节点是否在 if 的 true 分支中
                if (ifStatement.Statement.DescendantNodesAndSelf().Contains(context.Node))
                {
                    // 检查条件是否包含状态检查
                    if (ContainsStatusCheck(ifStatement.Condition, targetExpression, requiredStatusChecks))
                        return true;
                }
                // 检查是否在 else 分支中（对于 IsNone/IsErr 的检查）
                else if (ifStatement.Else != null && ifStatement.Else.Statement.DescendantNodesAndSelf().Contains(context.Node))
                {
                    // 检查条件是否包含反向状态检查（如 !opt.IsSome 意味着 else 分支是 IsNone）
                    if (ContainsNegatedStatusCheck(ifStatement.Condition, targetExpression, requiredStatusChecks))
                        return true;
                }
            }

            // 检查是否在条件表达式（三元运算符）的相应分支中
            if (currentNode is ConditionalExpressionSyntax conditionalExpr)
            {
                // 检查当前节点在哪个分支
                bool inTrueBranch = conditionalExpr.WhenTrue.DescendantNodesAndSelf().Contains(context.Node);
                bool inFalseBranch = conditionalExpr.WhenFalse.DescendantNodesAndSelf().Contains(context.Node);

                if (inTrueBranch && ContainsStatusCheck(conditionalExpr.Condition, targetExpression, requiredStatusChecks))
                    return true;

                if (inFalseBranch && ContainsNegatedStatusCheck(conditionalExpr.Condition, targetExpression, requiredStatusChecks))
                    return true;
            }

            // 检查是否在 switch 语句的 case 中
            if (currentNode is SwitchSectionSyntax switchSection)
            {
                var switchStatement = switchSection.Parent as SwitchStatementSyntax;
                if (switchStatement != null)
                {
                    // 简化处理：检查 switch 表达式是否是目标变量的状态属性
                    if (IsSwitchOnStatusCheck(switchStatement, targetExpression))
                        return true;
                }
            }

            // 检查是否在 && 表达式的右侧（左侧已经检查了条件）
            if (currentNode is BinaryExpressionSyntax binaryExpr && binaryExpr.Kind() == SyntaxKind.LogicalAndExpression)
            {
                // 如果当前节点在右侧，检查左侧是否包含状态检查
                if (binaryExpr.Right.DescendantNodesAndSelf().Contains(context.Node))
                {
                    if (ContainsStatusCheck(binaryExpr.Left, targetExpression, requiredStatusChecks))
                        return true;
                }
            }

            // 检查是否在 while 循环的条件为真的循环体中
            if (currentNode is WhileStatementSyntax whileStatement)
            {
                if (whileStatement.Statement.DescendantNodesAndSelf().Contains(context.Node))
                {
                    if (ContainsStatusCheck(whileStatement.Condition, targetExpression, requiredStatusChecks))
                        return true;
                }
            }

            currentNode = currentNode.Parent;
        }

        return false;
    }

    /// <summary>
    /// 检查表达式是否包含对目标变量的状态检查
    /// </summary>
    private static bool ContainsStatusCheck(ExpressionSyntax expression, ExpressionSyntax targetExpression, string[] statusChecks)
    {
        return ContainsStatusCheckInternal(expression, targetExpression, statusChecks, false);
    }

    /// <summary>
    /// 检查表达式是否包含对目标变量的反向状态检查（用于 else 分支）
    /// </summary>
    private static bool ContainsNegatedStatusCheck(ExpressionSyntax expression, ExpressionSyntax targetExpression, string[] statusChecks)
    {
        // 首先尝试查找显式的否定形式（如 !opt.IsSome）
        if (ContainsStatusCheckInternal(expression, targetExpression, statusChecks, true))
            return true;

        // 处理隐含的否定：在 else 分支中，if (opt.IsSome) 意味着 else 分支是 opt.IsNone
        // 检查条件是否是互斥状态的直接访问
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var memberName = memberAccess.Name.Identifier.Text;
            // 检查是否是同一目标变量的状态访问
            if (IsSameExpression(memberAccess.Expression, targetExpression))
            {
                // IsSome 和 IsNone 是互斥的，IsOk 和 IsErr 是互斥的
                // 如果条件是 IsSome，else 分支等价于 IsNone
                // 如果条件是 IsOk，else 分支等价于 IsErr
                // 如果条件是 IsNone，else 分支等价于 IsSome
                // 如果条件是 IsErr，else 分支等价于 IsOk
                var oppositeMap = new System.Collections.Generic.Dictionary<string, string>
                {
                    [nameof(Option<object>.IsSome)] = nameof(Option<object>.IsNone),
                    [nameof(Option<object>.IsNone)] = nameof(Option<object>.IsSome),
                    [nameof(Result<object, object>.IsOk)] = nameof(Result<object, object>.IsErr),
                    [nameof(Result<object, object>.IsErr)] = nameof(Result<object, object>.IsOk)
                };

                if (oppositeMap.TryGetValue(memberName, out var opposite) &&
                    statusChecks.Contains(opposite))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 内部实现：检查是否包含状态检查
    /// </summary>
    private static bool ContainsStatusCheckInternal(ExpressionSyntax expression, ExpressionSyntax targetExpression, string[] statusChecks, bool lookForNegation)
    {
        switch (expression.Kind())
        {
            case SyntaxKind.SimpleMemberAccessExpression:
                var memberAccess = (MemberAccessExpressionSyntax)expression;
                var memberName = memberAccess.Name.Identifier.Text;

                // 检查是否是目标变量的状态检查
                if (statusChecks.Contains(memberName) && IsSameExpression(memberAccess.Expression, targetExpression))
                {
                    return !lookForNegation;
                }
                break;

            case SyntaxKind.LogicalNotExpression:
                var notExpr = (PrefixUnaryExpressionSyntax)expression;
                // 对于反向检查，查找 !opt.IsSome 这样的模式
                if (lookForNegation)
                {
                    return ContainsStatusCheckInternal(notExpr.Operand, targetExpression, statusChecks, false);
                }
                else
                {
                    // 检查是否是对其他状态的否定（如 !opt.IsNone 等价于 opt.IsSome）
                    var operand = notExpr.Operand as MemberAccessExpressionSyntax;
                    if (operand != null)
                    {
                        var negatedMember = operand.Name.Identifier.Text;
                        // IsSome 和 IsNone 是互斥的，IsOk 和 IsErr 是互斥的
                        // 使用 nameof 确保编译时检查
                        var oppositeMap = new System.Collections.Generic.Dictionary<string, string>
                        {
                            [nameof(Option<object>.IsNone)] = nameof(Option<object>.IsSome),
                            [nameof(Option<object>.IsSome)] = nameof(Option<object>.IsNone),
                            [nameof(Result<object, object>.IsErr)] = nameof(Result<object, object>.IsOk),
                            [nameof(Result<object, object>.IsOk)] = nameof(Result<object, object>.IsErr)
                        };

                        if (oppositeMap.TryGetValue(negatedMember, out var opposite) &&
                            statusChecks.Contains(opposite) &&
                            IsSameExpression(operand.Expression, targetExpression))
                        {
                            return !lookForNegation;
                        }
                    }
                }
                break;

            case SyntaxKind.LogicalAndExpression:
            case SyntaxKind.LogicalOrExpression:
                var binaryExpr = (BinaryExpressionSyntax)expression;
                return ContainsStatusCheckInternal(binaryExpr.Left, targetExpression, statusChecks, lookForNegation) ||
                       ContainsStatusCheckInternal(binaryExpr.Right, targetExpression, statusChecks, lookForNegation);

            case SyntaxKind.ParenthesizedExpression:
                var parenExpr = (ParenthesizedExpressionSyntax)expression;
                return ContainsStatusCheckInternal(parenExpr.Expression, targetExpression, statusChecks, lookForNegation);

            case SyntaxKind.EqualsExpression:
            case SyntaxKind.NotEqualsExpression:
                var equalityExpr = (BinaryExpressionSyntax)expression;
                // 处理 opt.IsSome == true 或 opt.IsSome == false 的情况
                var leftMemberAccess = equalityExpr.Left as MemberAccessExpressionSyntax;
                var rightLiteral = equalityExpr.Right as LiteralExpressionSyntax;

                if (leftMemberAccess != null && rightLiteral != null)
                {
                    var checkedMemberName = leftMemberAccess.Name.Identifier.Text;
                    var isTrue = rightLiteral.Token.Value is true;
                    var isEquals = expression.Kind() == SyntaxKind.EqualsExpression;

                    if (statusChecks.Contains(checkedMemberName) && IsSameExpression(leftMemberAccess.Expression, targetExpression))
                    {
                        var conditionMet = isEquals == isTrue;
                        return lookForNegation ? !conditionMet : conditionMet;
                    }
                }
                break;
        }

        return false;
    }

    /// <summary>
    /// 检查 switch 语句是否是基于状态属性的 switch
    /// </summary>
    private static bool IsSwitchOnStatusCheck(SwitchStatementSyntax switchStatement, ExpressionSyntax targetExpression)
    {
        // 简化处理：检查 switch 表达式是否是目标变量的成员访问
        if (switchStatement.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return IsSameExpression(memberAccess.Expression, targetExpression);
        }
        return false;
    }

    /// <summary>
    /// 检查两个表达式是否相同
    /// </summary>
    private static bool IsSameExpression(ExpressionSyntax expr1, ExpressionSyntax expr2)
    {
        // 简化比较：比较文本表示
        return expr1.ToString() == expr2.ToString();
    }

    /// <summary>
    /// 检查当前代码是否标记了 SkipMustHandleCheck 特性
    /// </summary>
    private static bool HasSkipMustHandleCheckAttribute(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = context.Node.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDecl != null && HasAttribute(context, methodDecl.AttributeLists, "SkipMustHandleCheck"))
        {
            return true;
        }

        var propertyDecl = context.Node.Ancestors()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDecl != null && HasAttribute(context, propertyDecl.AttributeLists, "SkipMustHandleCheck"))
        {
            return true;
        }

        var typeDecl = context.Node.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        if (typeDecl != null && HasAttribute(context, typeDecl.AttributeLists, "SkipMustHandleCheck"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查属性列表中是否包含指定名称的特性
    /// </summary>
    private static bool HasAttribute(SyntaxNodeAnalysisContext context, SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
    {
        foreach (var attrList in attributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrName = attr.Name.ToString();
                if (attrName == attributeName ||
                    attrName == attributeName + "Attribute" ||
                    attrName.EndsWith("." + attributeName) ||
                    attrName.EndsWith("." + attributeName + "Attribute"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
