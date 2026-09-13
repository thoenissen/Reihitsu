using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6018: Negative signs must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6018";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6018Title), nameof(AnalyzerResources.RH6018MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a unary minus expression
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var node = (PrefixUnaryExpressionSyntax)context.Node;

        if (UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node))
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var start = node.OperatorToken.Span.End;
        var end = start;

        while (end < sourceText.Length
               && (sourceText[end] == ' ' || sourceText[end] == '\t'))
        {
            end++;
        }

        if (end > start)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(start, end))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.UnaryMinusExpression);
    }

    #endregion // DiagnosticAnalyzer
}