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
/// RH6017: Increment/decrement symbols must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6017";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6017Title), nameof(AnalyzerResources.RH6017MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a postfix increment or decrement expression
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var token = ((PostfixUnaryExpressionSyntax)context.Node).OperatorToken;
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var previousToken = token.GetPreviousToken();

        if (previousToken == default
            || previousToken.GetLocation().GetLineSpan().EndLinePosition.Line != token.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            return;
        }

        var start = FormattingTextAnalysisUtilities.GetLeadingWhitespaceRunStart(sourceText, token.SpanStart, previousToken.Span.End);

        if (start < token.SpanStart)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(start, token.SpanStart))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression);
    }

    #endregion // DiagnosticAnalyzer
}