using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report an expression body the formatter converts to a block body
/// </summary>
/// <typeparam name="TNode">Node type that owns the expression body</typeparam>
public abstract class ExpressionBodyAnalyzerBase<TNode> : DiagnosticAnalyzerBase
    where TNode : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Syntax kind
    /// </summary>
    private readonly SyntaxKind _syntaxKind;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message format resource name</param>
    /// <param name="syntaxKind">Syntax kind of <typeparamref name="TNode"/></param>
    private protected ExpressionBodyAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)
        : base(diagnosticId, DiagnosticCategory.Clarity, titleResourceName, messageFormatResourceName)
    {
        _syntaxKind = syntaxKind;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the expression body of the node
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>The expression body, or <see langword="null"/> when the node has none</returns>
    protected abstract ArrowExpressionClauseSyntax GetExpressionBody(TNode node);

    /// <summary>
    /// Gets the semicolon token that terminates the expression body
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>The semicolon token</returns>
    protected abstract SyntaxToken GetSemicolonToken(TNode node);

    /// <summary>
    /// Analyzing all matching syntax nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TNode node)
        {
            return;
        }

        var expressionBody = GetExpressionBody(node);

        if (expressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(node, expressionBody, GetSemicolonToken(node)))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(expressionBody.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, _syntaxKind);
    }

    #endregion // DiagnosticAnalyzer
}