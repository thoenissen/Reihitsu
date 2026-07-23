using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6002: Commas must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6002CommasMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6002CommasMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6002Title), nameof(AnalyzerResources.RH6002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the same-line whitespace runs directly adjacent to a comma
    /// </summary>
    /// <param name="token">Comma token</param>
    /// <param name="sourceText">Source text backing the token</param>
    /// <returns>
    /// The leading and trailing whitespace spans together with whether the trailing side crosses a line break
    /// </returns>
    internal static (TextSpan LeadingWhitespaceSpan, TextSpan TrailingWhitespaceSpan, bool HasTrailingLineBreak) AnalyzeSpacing(SyntaxToken token, SourceText sourceText)
    {
        var previousToken = token.GetPreviousToken();
        var leadingWhitespaceStart = token.SpanStart;

        if (previousToken.RawKind != 0
            && previousToken.GetLocation().GetLineSpan().EndLinePosition.Line == token.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            while (leadingWhitespaceStart > previousToken.Span.End
                   && IsHorizontalWhitespace(sourceText[leadingWhitespaceStart - 1]))
            {
                leadingWhitespaceStart--;
            }
        }

        var nextToken = token.GetNextToken();
        var hasTrailingLineBreak = nextToken.RawKind == 0
                                   || token.GetLocation().GetLineSpan().EndLinePosition.Line != nextToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var trailingWhitespaceEnd = token.Span.End;

        if (hasTrailingLineBreak == false)
        {
            while (trailingWhitespaceEnd < nextToken.SpanStart
                   && IsHorizontalWhitespace(sourceText[trailingWhitespaceEnd]))
            {
                trailingWhitespaceEnd++;
            }
        }

        return (TextSpan.FromBounds(leadingWhitespaceStart, token.SpanStart),
                TextSpan.FromBounds(token.Span.End, trailingWhitespaceEnd),
                hasTrailingLineBreak);
    }

    /// <summary>
    /// Determines whether the trailing whitespace span contains exactly one space
    /// </summary>
    /// <param name="trailingWhitespaceSpan">Trailing whitespace span</param>
    /// <param name="sourceText">Source text backing the span</param>
    /// <returns><see langword="true"/> if the span contains exactly one space; otherwise, <see langword="false"/></returns>
    internal static bool HasExactlyOneTrailingSpace(TextSpan trailingWhitespaceSpan, SourceText sourceText)
    {
        return trailingWhitespaceSpan.Length == 1 && sourceText[trailingWhitespaceSpan.Start] == ' ';
    }

    /// <summary>
    /// Determines whether a character is horizontal whitespace normalized by the formatter
    /// </summary>
    /// <param name="character">Character to inspect</param>
    /// <returns><see langword="true"/> for a space or tab; otherwise, <see langword="false"/></returns>
    private static bool IsHorizontalWhitespace(char character)
    {
        return character == ' ' || character == '\t';
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.CommaToken)))
        {
            if (token.Parent is ArrayRankSpecifierSyntax)
            {
                continue;
            }

            if (token.Parent is TypeArgumentListSyntax typeArgumentList
                && typeArgumentList.Arguments.Any(argument => argument is OmittedTypeArgumentSyntax))
            {
                continue;
            }

            var (leadingWhitespaceSpan, trailingWhitespaceSpan, hasTrailingLineBreak) = AnalyzeSpacing(token, sourceText);

            if (leadingWhitespaceSpan.IsEmpty
                && (hasTrailingLineBreak || HasExactlyOneTrailingSpace(trailingWhitespaceSpan, sourceText)))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
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