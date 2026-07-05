using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Shared base for analyzers that inspect outermost fluent-call chains
/// </summary>
public abstract class FluentChainAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    protected FluentChainAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, DiagnosticCategory.Layout, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a fluent chain starting from its outermost node
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="outermostNode">The outermost node of the chain</param>
    protected abstract void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode);

    /// <summary>
    /// Analyzes member access expressions at the outer edge of a fluent chain
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess
            || FluentChainAnalysisHelper.IsInnerChainMember(memberAccess))
        {
            return;
        }

        AnalyzeChain(context, memberAccess);
    }

    /// <summary>
    /// Analyzes conditional access expressions at the outer edge of a fluent chain
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConditionalAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConditionalAccessExpressionSyntax conditionalAccess
            || FluentChainAnalysisHelper.IsInnerChainMember(conditionalAccess))
        {
            return;
        }

        AnalyzeChain(context, conditionalAccess);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(OnConditionalAccessExpression, SyntaxKind.ConditionalAccessExpression);
    }

    #endregion // DiagnosticAnalyzer
}