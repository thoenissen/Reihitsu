using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for reasoning about the whitespace directly surrounding a token, so an analyzer and its
/// matching code fix always agree on which side of the token is considered spaced
/// </summary>
public static class AdjacentTokenSpacingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the leading and trailing sides of <paramref name="token"/> are spaced, treating a
    /// line break on either side as satisfying the requirement. Use this for rules that require a token to be
    /// spaced (for example a base-list or constructor-initializer colon), where a side that starts a
    /// continuation line is left to indentation handling and must never be reported or rewritten
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <param name="sourceText">Source text backing the token</param>
    /// <returns>A tuple indicating whether the leading and trailing side are spaced</returns>
    public static (bool HasLeadingSpace, bool HasTrailingSpace) DetermineLineBreakTolerantSpacing(SyntaxToken token, SourceText sourceText)
    {
        var previousToken = token.GetPreviousToken();
        var nextToken = token.GetNextToken();

        var hasLeadingLineBreak = previousToken.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                                  || token.LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        var hasTrailingLineBreak = token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                                   || nextToken.LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        var hasLeadingSpace = hasLeadingLineBreak || (token.SpanStart > 0 && sourceText[token.SpanStart - 1] == ' ');
        var hasTrailingSpace = hasTrailingLineBreak || (token.Span.End < sourceText.Length && sourceText[token.Span.End] == ' ');

        return (hasLeadingSpace, hasTrailingSpace);
    }

    /// <summary>
    /// Determines whether the leading and trailing sides of <paramref name="token"/> carry an adjacent space or
    /// tab on the same line, without any tolerance for line breaks. Use this for rules that forbid a token from
    /// being spaced (for example a member-access dot), where a side that spans a line break can never be the
    /// offending whitespace and must never be reported or rewritten
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <param name="sourceText">Source text backing the token</param>
    /// <returns>A tuple indicating whether the leading and trailing side carry a same-line space or tab</returns>
    public static (bool HasLeadingSpace, bool HasTrailingSpace) DetermineSameLineAdjacentSpacing(SyntaxToken token, SourceText sourceText)
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

        return (hasLeadingSpace, hasTrailingSpace);
    }

    #endregion // Methods
}