using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report a trailing comma after the final element of a brace-delimited list
/// </summary>
/// <typeparam name="TNode">Node type</typeparam>
public abstract class TrailingCommaAnalyzerBase<TNode> : DiagnosticAnalyzerBase
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
    /// <param name="category">Category</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message format resource name</param>
    /// <param name="syntaxKind">Syntax kind</param>
    private protected TrailingCommaAnalyzerBase(string diagnosticId, Enumerations.DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName)
    {
        _syntaxKind = syntaxKind;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get the elements of the node interleaved with their separators
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Elements and separators</returns>
    protected abstract SyntaxNodeOrTokenList GetElementsWithSeparators(TNode node);

    /// <summary>
    /// Get the token closing the element list
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Closing brace token</returns>
    protected abstract SyntaxToken GetCloseBraceToken(TNode node);

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

        var elementsAndSeparators = GetElementsWithSeparators(node);

        if (elementsAndSeparators.Count == 0)
        {
            return;
        }

        var lastItem = elementsAndSeparators[elementsAndSeparators.Count - 1];

        if (lastItem.IsToken == false || lastItem.AsToken().IsKind(SyntaxKind.CommaToken) == false)
        {
            return;
        }

        var lastSeparator = lastItem.AsToken();
        var gap = TextSpan.FromBounds(lastSeparator.Span.End, GetCloseBraceToken(node).SpanStart);

        if (SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary(node, gap))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(lastSeparator.GetLocation()));
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