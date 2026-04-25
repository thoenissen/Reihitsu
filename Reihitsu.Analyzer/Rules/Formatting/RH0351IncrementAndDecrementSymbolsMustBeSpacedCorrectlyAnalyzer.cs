using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0351: Increment/decrement symbols must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0351";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0351Title), nameof(AnalyzerResources.RH0351MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
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

            if (token.SpanStart > previousToken.Span.End
                && sourceText.ToString(TextSpan.FromBounds(previousToken.Span.End, token.SpanStart)).Any(character => character == ' ' || character == '\t'))
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(previousToken.Span.End, token.SpanStart))));
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