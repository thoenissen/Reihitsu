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
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer>
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
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var token in root.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.PlusPlusToken) == false
                && token.IsKind(SyntaxKind.MinusMinusToken) == false)
            {
                continue;
            }

            if (token.Parent is PrefixUnaryExpressionSyntax)
            {
                continue;
            }

            var previousToken = token.GetPreviousToken();

            if (previousToken == default
                || previousToken.GetLocation().GetLineSpan().EndLinePosition.Line != token.GetLocation().GetLineSpan().StartLinePosition.Line)
            {
                continue;
            }

            var start = FormattingTextAnalysisUtilities.GetLeadingWhitespaceRunStart(sourceText, token.SpanStart, previousToken.Span.End);

            if (start < token.SpanStart)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(start, token.SpanStart))));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}