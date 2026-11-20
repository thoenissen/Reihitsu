using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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
    protected FluentChainAnalyzerBase(string diagnosticId)
        : base(diagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a fluent chain that has at least one link
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="chain">The chain, built from its outermost node</param>
    protected abstract void AnalyzeChain(SyntaxNodeAnalysisContext context, FluentChain chain);

    /// <summary>
    /// Analyzes the outermost node of a fluent chain
    /// </summary>
    /// <param name="context">Context</param>
    private void OnChainNode(SyntaxNodeAnalysisContext context)
    {
        var chain = FluentChain.Create(context.Node);

        if (chain == null)
        {
            return;
        }

        AnalyzeChain(context, chain);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnChainNode,
                                         SyntaxKind.SimpleMemberAccessExpression,
                                         SyntaxKind.ConditionalAccessExpression,
                                         SyntaxKind.InvocationExpression,
                                         SyntaxKind.ElementAccessExpression,
                                         SyntaxKind.SuppressNullableWarningExpression);
    }

    #endregion // DiagnosticAnalyzer
}