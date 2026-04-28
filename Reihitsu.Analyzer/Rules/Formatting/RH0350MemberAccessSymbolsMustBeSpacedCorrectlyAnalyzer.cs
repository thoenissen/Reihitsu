using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0350: Member access symbols must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0350MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0350MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0350";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0350MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0350Title), nameof(AnalyzerResources.RH0350MessageFormat))
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

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.DotToken)))
        {
            var previousToken = token.GetPreviousToken();
            var nextToken = token.GetNextToken();
            var hasLeadingSpace = previousToken != default
                                  && previousToken.GetLocation().GetLineSpan().EndLinePosition.Line == token.GetLocation().GetLineSpan().StartLinePosition.Line
                                  && token.SpanStart > 0
                                  && (sourceText[token.SpanStart - 1] == ' ' || sourceText[token.SpanStart - 1] == '\t');
            var hasTrailingSpace = nextToken != default
                                   && nextToken.GetLocation().GetLineSpan().StartLinePosition.Line == token.GetLocation().GetLineSpan().StartLinePosition.Line
                                   && token.Span.End < sourceText.Length
                                   && (sourceText[token.Span.End] == ' ' || sourceText[token.Span.End] == '\t');

            if (hasLeadingSpace || hasTrailingSpace)
            {
                context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
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