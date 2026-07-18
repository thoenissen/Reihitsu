using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Shared collaborator for the blank-line subphases. Provides the blank-line gap queries and the
/// blank-line insertion edit that previously lived on the removed <c>BlankLineSubphaseRewriter</c> base type
/// </summary>
internal sealed class BlankLineEditor
{
    #region Fields

    /// <summary>
    /// Formatting context used to source the end-of-line sequence for inserted blank lines
    /// </summary>
    private readonly FormattingContext _context;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    public BlankLineEditor(FormattingContext context)
    {
        _context = context;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the full gap before the specified token already contains a blank line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line exists before the token; otherwise, <see langword="false"/></returns>
    public static bool HasBlankLineBeforeToken(SyntaxToken token)
    {
        return CountBlankLinesBeforeToken(token) > 0;
    }

    /// <summary>
    /// Counts blank lines from the previous token to a leading-trivia prefix of the specified token
    /// </summary>
    /// <param name="token">The token whose leading-trivia prefix should be inspected</param>
    /// <param name="leadingTriviaEndExclusive">Exclusive upper bound in the leading-trivia list</param>
    /// <returns>The number of blank lines up to the specified leading-trivia index</returns>
    public static int CountBlankLinesBeforeLeadingTriviaIndex(SyntaxToken token, int leadingTriviaEndExclusive)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            return TokenGapAnalysis.Between(previousToken, token, leadingTriviaEndExclusive).BlankLineCount;
        }

        return TokenGapAnalysis.OfLeadingTrivia(token, leadingTriviaEndExclusive).BlankLineCount;
    }

    /// <summary>
    /// Determines whether the specified token is the first token in a block or switch section
    /// </summary>
    /// <param name="previousToken">The token that precedes the token being evaluated</param>
    /// <returns><see langword="true"/> if the token is the first in its containing block</returns>
    public static bool IsFirstInBlock(SyntaxToken previousToken)
    {
        if (previousToken == default)
        {
            return true;
        }

        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            return true;
        }

        if (previousToken.IsKind(SyntaxKind.ColonToken)
            && previousToken.Parent is SwitchLabelSyntax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified token is exempt from requiring a blank line before a region or end
    /// region directive, matching the Core policy in
    /// <see cref="Reihitsu.Core.RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore"/>. Unlike
    /// <see cref="IsFirstInBlock"/>, a switch-label colon is not exempt here (issue #428)
    /// </summary>
    /// <param name="previousToken">The token that precedes the token being evaluated</param>
    /// <returns><see langword="true"/> if no blank line is required before the directive</returns>
    public static bool IsExemptFromPrecedingBlankLineBeforeRegionDirective(SyntaxToken previousToken)
    {
        return previousToken == default || previousToken.IsKind(SyntaxKind.OpenBraceToken);
    }

    /// <summary>
    /// Determines whether a blank line exists in the trivia list before the specified index
    /// </summary>
    /// <param name="trivia">The trivia list to search</param>
    /// <param name="endIndex">The exclusive upper bound index to search up to</param>
    /// <returns><see langword="true"/> if a blank line is found before the specified index</returns>
    /// <remarks>
    /// This intentionally does not route through <see cref="TokenGapAnalysis"/>. Region directive trivia carries
    /// its own trailing newline, so this method must treat it as resetting the line-start state, whereas
    /// <see cref="TokenGapAnalysis"/> treats directives as ordinary line content. Folding that quirk into the shared
    /// collaborator would change its behaviour for every other call site, so the two are deliberately kept distinct
    /// </remarks>
    public static bool HasBlankLineBeforeIndex(SyntaxTriviaList trivia, int endIndex)
    {
        var atLineStart = true;
        var sawLineBreak = false;

        for (var triviaIndex = 0; triviaIndex < endIndex; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (sawLineBreak && atLineStart)
                {
                    return true;
                }

                sawLineBreak = true;
                atLineStart = true;
            }
            else if (kind == SyntaxKind.WhitespaceTrivia)
            {
                // Whitespace doesn't change line-start status
            }
            else if (kind is SyntaxKind.RegionDirectiveTrivia
                          or SyntaxKind.EndRegionDirectiveTrivia)
            {
                // Structured directive trivia includes its own trailing newline,
                // so the next line effectively starts after it.
                atLineStart = true;
            }
            else
            {
                atLineStart = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Ensures a blank line exists before the first directive of the specified kind in the token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <param name="directiveKind">Kind of the directive that requires a preceding blank line</param>
    /// <returns>The token with a blank line inserted before the first matching directive, or the original if one already exists</returns>
    /// <remarks>
    /// Counts blank lines in the full gap up to the insertion point via <see cref="CountBlankLinesBeforeLeadingTriviaIndex"/>
    /// rather than the contiguous end-of-line trivia run immediately before the directive. A comment sitting in
    /// that gap (for example <c>code();\n// header\n#region R</c>) ends its own line with a single end-of-line
    /// trivia that is not itself a blank line, but a run-length count could mistake it for one (issue #428)
    /// </remarks>
    public SyntaxToken EnsureBlankLineBeforeFirstDirective(SyntaxToken token, SyntaxKind directiveKind)
    {
        var trivia = token.LeadingTrivia;
        var directiveIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(directiveKind))
            {
                directiveIndex = triviaIndex;

                break;
            }
        }

        if (directiveIndex < 0)
        {
            return token;
        }

        var insertIndex = directiveIndex;

        while (insertIndex > 0 && trivia[insertIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            insertIndex--;
        }

        if (CountBlankLinesBeforeLeadingTriviaIndex(token, insertIndex) > 0)
        {
            return token;
        }

        var newTrivia = trivia.Insert(insertIndex, SyntaxFactory.EndOfLine(_context.EndOfLine));

        return token.WithLeadingTrivia(newTrivia);
    }

    /// <summary>
    /// Ensures a blank line exists before the specified statement by inserting one if absent, exempting
    /// statements immediately preceded by a preprocessor directive
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    /// <remarks>
    /// No blank line is inserted when the statement is immediately preceded by a preprocessor directive,
    /// mirroring the exemption the PrecededBy/FollowedBy analyzer bases apply (issue #415). Callers whose
    /// rule has no such exemption, such as the "blank line after a closing brace" rule, must use
    /// <see cref="EnsureBlankLineAfterClosingBrace"/> instead
    /// </remarks>
    public StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        if (SyntaxTriviaUtilities.IsPrecededByDirective(CombinedLeadingTrivia(firstToken)))
        {
            return statement;
        }

        return InsertBlankLineBeforeToken(statement, firstToken, insertIndex: 0);
    }

    /// <summary>
    /// Ensures a blank line exists between a closing brace and the specified statement, repositioning the
    /// insertion past any leading directive rather than exempting it
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    /// <remarks>
    /// RH5030 (blank line required after a closing brace) carries no directive exemption, unlike the
    /// PrecededBy/FollowedBy statement rules, so the blank line must still be inserted here — just after
    /// the directive instead of at leading-trivia index 0, which would otherwise land it inside the
    /// conditional region the directive opens or closes. This mirrors
    /// <see cref="Reihitsu.Analyzer.CodeFixes.Rules.Layout.RH5030BlankLineAfterClosingBraceCodeFixProvider"/> (issue #415)
    /// </remarks>
    public StatementSyntax EnsureBlankLineAfterClosingBrace(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        var insertIndex = SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(firstToken.LeadingTrivia);

        return InsertBlankLineBeforeToken(statement, firstToken, insertIndex);
    }

    /// <summary>
    /// Combines the trailing trivia of the token preceding the specified token with its own leading trivia
    /// </summary>
    /// <param name="token">The token whose preceding trivia gap should be combined</param>
    /// <returns>The combined trivia sequence, in source order</returns>
    private static IEnumerable<SyntaxTrivia> CombinedLeadingTrivia(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return token.LeadingTrivia;
        }

        return previousToken.TrailingTrivia.Concat(token.LeadingTrivia);
    }

    /// <summary>
    /// Counts blank lines in the full gap before the specified token
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns>The number of blank lines before the token</returns>
    private static int CountBlankLinesBeforeToken(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return TokenGapAnalysis.OfLeadingTrivia(token, token.LeadingTrivia.Count).BlankLineCount;
        }

        return TokenGapAnalysis.Between(previousToken, token).BlankLineCount;
    }

    /// <summary>
    /// Inserts a blank line into the leading trivia of the specified token at the given index
    /// </summary>
    /// <param name="statement">The statement whose token should be replaced</param>
    /// <param name="token">The token to insert the blank line before</param>
    /// <param name="insertIndex">The leading-trivia index at which to insert the blank line</param>
    /// <returns>The statement with the token replaced by a copy carrying the inserted blank line</returns>
    private StatementSyntax InsertBlankLineBeforeToken(StatementSyntax statement, SyntaxToken token, int insertIndex)
    {
        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeading = token.LeadingTrivia.Insert(insertIndex, eol);
        var newToken = token.WithLeadingTrivia(newLeading);

        return statement.ReplaceToken(token, newToken);
    }

    #endregion // Methods
}