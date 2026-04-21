using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report unnecessary empty parentheses on a specific syntax node type.
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TNode">Node type</typeparam>
public abstract class EmptyParenthesesAnalyzerBase<TAnalyzer, TNode> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
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
    internal EmptyParenthesesAnalyzerBase(string diagnosticId, Enumerations.DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName)
    {
        _syntaxKind = syntaxKind;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the specified node contains unnecessary empty parentheses.
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node violates the rule</returns>
    protected abstract bool HasUnnecessaryParentheses(TNode node);

    /// <summary>
    /// Get the diagnostic location.
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Diagnostic location</returns>
    protected abstract Location GetDiagnosticLocation(TNode node);

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

        if (HasUnnecessaryParentheses(node) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(GetDiagnosticLocation(node)));
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