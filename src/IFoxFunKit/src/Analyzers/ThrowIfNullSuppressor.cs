using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IFoxFunKit;

/// <summary>
/// 诊断抑制器：抑制 ArgumentNullEx.ThrowIfNull 调用后的 CS8602 警告
/// 
/// 【原理】
/// DiagnosticSuppressor 可以抑制编译器或其他分析器生成的诊断。
/// 当检测到 ArgumentNullEx.ThrowIfNull(variable) 调用后，
/// 抑制该变量在后续代码中的 CS8602（解引用可能出现空引用）警告。
/// 
/// 【关键修复记录】
/// 问题：原本使用 semanticModel.GetDiagnostics() 获取诊断，但抑制器不生效
/// 原因：DiagnosticSuppressor 必须使用 context.ReportedDiagnostics 获取编译器已报告的诊断实例
///       使用 semanticModel.GetDiagnostics() 会创建新的诊断实例，而不是编译器实际报告的那些
/// 修复：改为使用 context.ReportedDiagnostics，并添加 SourceTree 检查确保诊断来自当前语法树
/// 
/// 【使用方式】
/// 此分析器通过 Directory.Build.props 自动引用到所有项目（除 IFoxFunKit 本身外）
/// 需要在 IFoxCAD.slnx 中配置项目依赖关系，确保 IFoxFunKit 先编译
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThrowIfNullSuppressor : DiagnosticSuppressor
{
    // 定义要抑制的诊断 ID 集合，使用 HashSet 提高查找效率
    private static readonly HashSet<string> SuppressibleDiagnosticIds = new HashSet<string>
    {
        "CS8602",  // 解引用可能出现空引用
        "CS8604"   // 可能传入 null 引用实参
    };

    // 定义要抑制的诊断描述符
    private static readonly SuppressionDescriptor CS8602Suppression = new SuppressionDescriptor(
        id: "IFOXSUP001",
        suppressedDiagnosticId: "CS8602",
        justification: "变量已通过 ArgumentNullEx.ThrowIfNull 验证为非 null");

    private static readonly SuppressionDescriptor CS8604Suppression = new SuppressionDescriptor(
        id: "IFOXSUP002",
        suppressedDiagnosticId: "CS8604",
        justification: "变量已通过 ArgumentNullEx.ThrowIfNull 验证为非 null");


    /// <summary>
    /// 支持的抑制描述符集合
    /// </summary>
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
        ImmutableArray.Create(CS8602Suppression, CS8604Suppression);

    /// <summary>
    /// 查找并抑制后续对该变量的 CS8602 警告
    /// </summary>
    /// <param name="context">抑制分析上下文</param>
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // 遍历所有编译单元
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            // DiagnosticSuppressor 需要获取语义模型来分析代码
            // 这是必要的，因为我们需要识别 ThrowIfNull 调用和变量引用
            // 虽然 GetSemanticModel 有一定性能开销，但对于抑制器来说是必需的
#pragma warning disable RS1030
            var semanticModel = context.Compilation.GetSemanticModel(tree);
#pragma warning restore RS1030
            var root = tree.GetRoot(context.CancellationToken);

            // 查找所有 ThrowIfNull 调用
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (!IsThrowIfNullInvocation(semanticModel, invocation))
                    continue;

                // 获取被检查的变量符号
                var checkedVariable = GetCheckedVariableSymbol(semanticModel, invocation);
                if (checkedVariable == null)
                    continue;

                // 查找并抑制后续对该变量的 CS8602 警告
                SuppressWarningsForVariable(context, tree, semanticModel, invocation, checkedVariable);
            }
        }
    }

    /// <summary>
    /// 检查是否是 ThrowIfNull 调用
    /// </summary>
    private static bool IsThrowIfNullInvocation(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
    {
        var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
            return false;

        // 只检查方法名，不限制命名空间，使其更通用
        // 适用于 ArgumentNullEx.ThrowIfNull、ArgumentNullException.ThrowIfNull 等
        return methodSymbol.Name == "ThrowIfNull";
    }

    /// <summary>
    /// 获取被检查变量的符号
    /// </summary>
    private static ISymbol? GetCheckedVariableSymbol(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
    {
        var firstArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument == null)
            return null;

        var expression = firstArgument.Expression;

        // 处理标识符
        if (expression is IdentifierNameSyntax identifier)
        {
            return semanticModel.GetSymbolInfo(identifier).Symbol;
        }

        // 处理成员访问
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            return semanticModel.GetSymbolInfo(memberAccess).Symbol;
        }

        return null;
    }

    /// <summary>
    /// 抑制指定变量的 CS8602 警告
    /// </summary>
    private static void SuppressWarningsForVariable(
        SuppressionAnalysisContext context,
        SyntaxTree tree,
        SemanticModel semanticModel,
        InvocationExpressionSyntax throwIfNullInvocation,
        ISymbol checkedVariable)
    {
        // 获取包含 ThrowIfNull 调用的语句
        var containingStatement = throwIfNullInvocation.FirstAncestorOrSelf<StatementSyntax>();
        if (containingStatement == null)
            return;

        // 获取包含语句的语法节点（可能是语句列表或方法体等）
        var parent = containingStatement.Parent;
        if (parent == null)
            return;

        // 使用 context.ReportedDiagnostics 获取编译器已经报告的诊断
        // 原因：DiagnosticSuppressor 的工作流程是：
        // 1. 编译器首先分析代码并生成诊断（如 CS8602）
        // 2. 然后调用 DiagnosticSuppressor 的 ReportSuppressions 方法
        // 3. context.ReportedDiagnostics 包含的就是这些已生成的诊断实例
        // 4. 只有抑制这些已报告的诊断实例才能生效
        // 注意：不能使用 semanticModel.GetDiagnostics()，因为那会创建新的诊断实例，
        //       而不是编译器实际报告的那些实例
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            // 使用 HashSet 快速判断是否需要处理的诊断 ID
            if (!SuppressibleDiagnosticIds.Contains(diagnostic.Id))
                continue;

            // 检查诊断是否来自当前语法树
            if (diagnostic.Location.SourceTree != tree)
                continue;

            // 检查诊断位置是否在 ThrowIfNull 调用之后
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var throwIfNullSpan = throwIfNullInvocation.Span;

            // 确保诊断在 ThrowIfNull 调用之后
            if (diagnosticSpan.Start <= throwIfNullSpan.End)
                continue;

            // 获取诊断位置的语法节点
            var diagnosticNode = tree.GetRoot().FindNode(diagnosticSpan);
            if (diagnosticNode == null)
                continue;

            // 检查诊断是否涉及被检查的变量
            if (IsDiagnosticForVariable(semanticModel, diagnosticNode, checkedVariable))
            {
                // 根据诊断类型选择抑制描述符
                var suppressionDescriptor = diagnostic.Id == "CS8602" ? CS8602Suppression : CS8604Suppression;
                var suppression = Suppression.Create(suppressionDescriptor, diagnostic);
                context.ReportSuppression(suppression);
            }
        }
    }

    /// <summary>
    /// 检查诊断是否针对指定变量
    /// </summary>
    private static bool IsDiagnosticForVariable(SemanticModel semanticModel, SyntaxNode node, ISymbol targetVariable)
    {
        // 查找节点中的所有标识符和成员访问
        var identifiers = node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
        var memberAccesses = node.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();

        // 检查标识符
        foreach (var identifier in identifiers)
        {
            var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (SymbolEqualityComparer.Default.Equals(symbol, targetVariable))
                return true;
        }

        // 检查成员访问的表达式部分
        foreach (var memberAccess in memberAccesses)
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
            if (SymbolEqualityComparer.Default.Equals(symbol, targetVariable))
                return true;
        }

        return false;
    }
}
