using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace IFoxFunKit.Analyzers;

/// <summary>
/// 强制处理Option和Result返回值的分析器
/// 如果用户没有处理所有分支，则产生编译错误
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustHandleResultAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = DiagnosticMessages.ErrorCodes.MustHandleResult;

    private static readonly DiagnosticDescriptor Rule = DiagnosticMessages.GetDescriptor(DiagnosticId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // 分析表达式语句，检查是否忽略了Option/Result返回值
        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);

        // 分析方法返回语句
        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);

        // 分析变量声明
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.LocalDeclarationStatement);

        // 分析赋值语句
        context.RegisterSyntaxNodeAction(AnalyzeAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
    }

    private void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        if (HasSkipMustHandleCheckAttribute(context)) return;

        var expressionStatement = (ExpressionStatementSyntax)context.Node;
        var expression = expressionStatement.Expression;

        // 检查是否是返回Option/Result的调用
        if (IsOptionOrResultType(context, expression))
        {
            // 检查是否被正确处理
            if (!IsProperlyHandled(context, expression))
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(expression);
                var typeName = typeInfo.Type?.ToDisplayString() ?? "Option/Result";

                var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), typeName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;
        if (returnStatement.Expression == null) return;

        // 返回语句不需要检查，因为返回值被返回了
        // 但如果返回的是Option/Result，调用方需要处理
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (HasSkipMustHandleCheckAttribute(context)) return;

        var declaration = (LocalDeclarationStatementSyntax)context.Node;

        foreach (var variable in declaration.Declaration.Variables)
        {
            if (variable.Initializer == null) continue;

            var expression = variable.Initializer.Value;

            // 检查是否是Option/Result类型
            if (IsOptionOrResultType(context, expression))
            {
                // 检查变量是否后续被正确处理
                var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable);
                if (variableSymbol != null && !IsVariableHandled(context, variableSymbol, declaration))
                {
                    var typeInfo = context.SemanticModel.GetTypeInfo(expression);
                    var typeName = typeInfo.Type?.ToDisplayString() ?? "Option/Result";

                    var diagnostic = Diagnostic.Create(Rule, variable.GetLocation(), typeName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        if (HasSkipMustHandleCheckAttribute(context)) return;

        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (IsOptionOrResultType(context, assignment.Right))
        {
            // 赋值给变量，检查变量是否被处理
            if (assignment.Left is IdentifierNameSyntax identifier)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol;
                if (symbol != null && !IsVariableHandled(context, symbol, assignment))
                {
                    var typeInfo = context.SemanticModel.GetTypeInfo(assignment.Right);
                    var typeName = typeInfo.Type?.ToDisplayString() ?? "Option/Result";

                    var diagnostic = Diagnostic.Create(Rule, assignment.GetLocation(), typeName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }



    /// <summary>
    /// 检查表达式是否是Option或Result类型
    /// </summary>
    private static bool IsOptionOrResultType(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(expression);
        var type = typeInfo.Type;

        if (type == null) return false;

        var typeName = type.ToDisplayString();

        // 检查是否是Option<T>或Result<T>或Result<T, TErr>
        return typeName.StartsWith(MustHandleMeta.OptionTypeFullName + "<") ||
               typeName.StartsWith(MustHandleMeta.ResultTypeFullName + "<");
    }

    /// <summary>
    /// 检查表达式是否被正确处理
    /// </summary>
    private static bool IsProperlyHandled(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        // 检查父节点是否是有效的处理方式
        var parent = expression.Parent;

        while (parent != null)
        {
            switch (parent.Kind())
            {
                // 作为参数传递
                case SyntaxKind.Argument:
                // 作为返回值
                case SyntaxKind.ReturnStatement:
                // 作为Lambda表达式体
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                // 作为await表达式
                case SyntaxKind.AwaitExpression:
                return true;

                // Switch表达式（C# 8.0+）
                case SyntaxKind.SwitchExpression:
                return IsSwitchExpressionExhaustive((SwitchExpressionSyntax)parent);

                // Switch语句
                case SyntaxKind.SwitchStatement:
                return IsSwitchStatementExhaustive((SwitchStatementSyntax)parent);

                // If语句 - 检查是否检查了IsSome/IsNone/IsOk/IsErr
                case SyntaxKind.IfStatement:
                return IsIfStatementCheckingOptionOrResult((IfStatementSyntax)parent, expression);

                // 条件表达式（三元运算符）
                case SyntaxKind.ConditionalExpression:
                return true; // 三元运算符强制处理两个分支

                // 成员访问 - 检查是否是IsSome/IsNone/IsOk/IsErr等属性访问
                case SyntaxKind.SimpleMemberAccessExpression:
                var memberAccess = (MemberAccessExpressionSyntax)parent;
                var memberName = memberAccess.Name.Identifier.Text;
                if (IsValidMemberAccess(memberName))
                    return true;
                break;

                // 调用表达式 - 检查是否是Match或其他处理方法
                case SyntaxKind.InvocationExpression:
                var invocation = (InvocationExpressionSyntax)parent;
                if (IsMatchInvocation(invocation))
                    return true;
                break;

                // 赋值表达式
                case SyntaxKind.SimpleAssignmentExpression:
                // 继续向上检查
                break;

                // 变量声明
                case SyntaxKind.EqualsValueClause:
                case SyntaxKind.VariableDeclarator:
                // 继续向上检查
                break;

                // 表达式语句 - 未被处理
                case SyntaxKind.ExpressionStatement:
                return false;
            }

            parent = parent.Parent;
        }

        return false;
    }

    /// <summary>
    /// 检查变量是否在后续代码中被处理
    /// </summary>
    private static bool IsVariableHandled(SyntaxNodeAnalysisContext context, ISymbol variableSymbol, SyntaxNode declarationNode)
    {
        // 获取包含该变量的方法或属性
        var containingMethod = declarationNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault()
            ?? declarationNode.Ancestors().OfType<AccessorDeclarationSyntax>().FirstOrDefault()?.Parent?.Parent as SyntaxNode;

        if (containingMethod == null) return false;

        // 查找所有引用该变量的地方
        var references = containingMethod.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(id => {
                var symbol = context.SemanticModel.GetSymbolInfo(id).Symbol;
                return symbol != null && SymbolEqualityComparer.Default.Equals(symbol, variableSymbol);
            });

        foreach (var reference in references)
        {
            // 跳过声明本身
            if (reference.Ancestors().Contains(declarationNode) &&
                reference.Ancestors().OfType<VariableDeclaratorSyntax>().FirstOrDefault()?.SpanStart == declarationNode.SpanStart)
            {
                continue;
            }

            // 检查这个引用是否被正确处理
            if (IsReferenceProperlyHandled(reference))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查变量引用是否被正确处理
    /// </summary>
    private static bool IsReferenceProperlyHandled(IdentifierNameSyntax reference)
    {
        var parent = reference.Parent;

        while (parent != null)
        {
            switch (parent.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                var memberAccess = (MemberAccessExpressionSyntax)parent;
                var memberName = memberAccess.Name.Identifier.Text;
                if (IsValidMemberAccess(memberName))
                    return true;
                break;

                case SyntaxKind.InvocationExpression:
                var invocation = (InvocationExpressionSyntax)parent;
                if (IsMatchInvocation(invocation))
                    return true;
                break;

                case SyntaxKind.SwitchExpression:
                return IsSwitchExpressionExhaustive((SwitchExpressionSyntax)parent);

                case SyntaxKind.SwitchStatement:
                return IsSwitchStatementExhaustive((SwitchStatementSyntax)parent);

                case SyntaxKind.IfStatement:
                return IsIfStatementCheckingOptionOrResult((IfStatementSyntax)parent, reference);

                case SyntaxKind.ReturnStatement:
                case SyntaxKind.Argument:
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    /// <summary>
    /// 检查是否是有效的成员访问（通过反射动态获取标记了MustHandleMember特性的成员）
    /// </summary>
    private static bool IsValidMemberAccess(string memberName)
    {
        // 使用反射动态获取的成员名称集合进行检查
        return MustHandleMeta.ValidMemberNames.Contains(memberName);
    }

    /// <summary>
    /// 检查是否是Match调用
    /// </summary>
    private static bool IsMatchInvocation(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.Text;
            return methodName == MustHandleMeta.MatchMethodName;
        }
        return false;
    }

    /// <summary>
    /// 检查Switch表达式是否穷尽了所有分支
    /// </summary>
    private static bool IsSwitchExpressionExhaustive(SwitchExpressionSyntax switchExpr)
    {
        // 检查是否有默认分支或变量模式
        var hasDefault = switchExpr.Arms.Any(arm =>
            arm.Pattern is DiscardPatternSyntax or VarPatternSyntax);

        // 对于Option/Result，检查是否处理了所有可能的情况
        // 这里简化处理，只要有默认分支就认为穷尽了
        return hasDefault;
    }

    /// <summary>
    /// 检查Switch语句是否穷尽了所有分支
    /// </summary>
    private static bool IsSwitchStatementExhaustive(SwitchStatementSyntax switchStmt)
    {
        // 检查是否有default分支
        return switchStmt.Sections.Any(section =>
            section.Labels.Any(label => label.Kind() == SyntaxKind.DefaultSwitchLabel));
    }

    /// <summary>
    /// 检查If语句是否检查了Option/Result的属性
    /// </summary>
    private static bool IsIfStatementCheckingOptionOrResult(IfStatementSyntax ifStmt, ExpressionSyntax targetExpr)
    {
        // 检查条件是否包含IsSome/IsNone/IsOk/IsErr的检查
        var condition = ifStmt.Condition;

        // 递归检查条件表达式
        return ContainsOptionOrResultCheck(condition, targetExpr);
    }

    /// <summary>
    /// 递归检查条件表达式是否包含Option/Result的属性检查
    /// </summary>
    private static bool ContainsOptionOrResultCheck(ExpressionSyntax expression, ExpressionSyntax targetExpr)
    {
        switch (expression.Kind())
        {
            case SyntaxKind.SimpleMemberAccessExpression:
            var memberAccess = (MemberAccessExpressionSyntax)expression;
            var memberName = memberAccess.Name.Identifier.Text;

            // 检查是否是状态检查成员（通过反射动态获取StatusCheck类型的成员）
            if (MustHandleMeta.StatusCheckMemberNames.Contains(memberName))
            {
                // 检查访问的对象是否是目标表达式
                if (IsSameExpression(memberAccess.Expression, targetExpr))
                {
                    return true;
                }
            }
            break;

            case SyntaxKind.LogicalNotExpression:
            var notExpr = (PrefixUnaryExpressionSyntax)expression;
            return ContainsOptionOrResultCheck(notExpr.Operand, targetExpr);

            case SyntaxKind.LogicalOrExpression:
            case SyntaxKind.LogicalAndExpression:
            var binaryExpr = (BinaryExpressionSyntax)expression;
            return ContainsOptionOrResultCheck(binaryExpr.Left, targetExpr) ||
                   ContainsOptionOrResultCheck(binaryExpr.Right, targetExpr);

            case SyntaxKind.ParenthesizedExpression:
            var parenExpr = (ParenthesizedExpressionSyntax)expression;
            return ContainsOptionOrResultCheck(parenExpr.Expression, targetExpr);
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

    // 使用nameof()获取特性名称
    private static readonly string SkipMustHandleCheckAttributeName = nameof(SkipMustHandleCheckAttribute).Replace("Attribute", "");

    /// <summary>
    /// 检查当前代码是否标记了 SkipMustHandleCheck 特性
    /// </summary>
    private static bool HasSkipMustHandleCheckAttribute(SyntaxNodeAnalysisContext context)
    {
        // 检查包含当前节点的方法是否有该特性
        var methodDecl = context.Node.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDecl != null && HasAttribute(context, methodDecl.AttributeLists, SkipMustHandleCheckAttributeName))
        {
            return true;
        }

        // 检查包含当前节点的属性是否有该特性
        var propertyDecl = context.Node.Ancestors()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDecl != null && HasAttribute(context, propertyDecl.AttributeLists, SkipMustHandleCheckAttributeName))
        {
            return true;
        }

        // 检查包含当前节点的类型是否有该特性
        var typeDecl = context.Node.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        if (typeDecl != null && HasAttribute(context, typeDecl.AttributeLists, SkipMustHandleCheckAttributeName))
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
