using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    /// <param name="previousTokenEndsWithLineBreak">Whether the original previous token already ended with a line break</param>
    /// <returns>The token with a blank line inserted before the first matching directive, or the original if one already exists</returns>
    public SyntaxToken EnsureBlankLineBeforeFirstDirective(SyntaxToken token, SyntaxKind directiveKind, bool previousTokenEndsWithLineBreak)
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

        var endOfLineCount = 0;

        for (var triviaIndex = insertIndex - 1; triviaIndex >= 0 && trivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia); triviaIndex--)
        {
            endOfLineCount++;
        }

        var requiredEndOfLineCount = previousTokenEndsWithLineBreak
                                         ? 1
                                         : 2;

        if (endOfLineCount >= requiredEndOfLineCount)
        {
            return token;
        }

        var newTrivia = trivia;

        while (endOfLineCount < requiredEndOfLineCount)
        {
            newTrivia = newTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(_context.EndOfLine));
            insertIndex++;
            endOfLineCount++;
        }

        return token.WithLeadingTrivia(newTrivia);
    }

    /// <summary>
    /// Ensures a blank line exists before the specified statement by inserting one if absent
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    /// <remarks>
    /// No blank line is inserted when the statement is immediately preceded by a preprocessor directive.
    /// Inserting at leading-trivia index 0 in that case would land the blank line above the directive,
    /// i.e. inside the conditional region the directive opens or closes rather than outside it. This
    /// mirrors the exemption the PrecededBy analyzer bases apply (issue #415)
    /// </remarks>
    public StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        if (IsPrecededByDirective(firstToken))
        {
            return statement;
        }

        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeading = firstToken.LeadingTrivia.Insert(0, eol);
        var newToken = firstToken.WithLeadingTrivia(newLeading);

        return statement.ReplaceToken(firstToken, newToken);
    }

    /// <summary>
    /// Determines whether the line immediately preceding the specified token is a preprocessor directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is immediately preceded by a preprocessor directive</returns>
    private static bool IsPrecededByDirective(SyntaxToken token)
    {
        var leadingTrivia = token.LeadingTrivia;

        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = leadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return trivia.IsDirective;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return false;
        }

        var trailingTrivia = previousToken.TrailingTrivia;

        for (var triviaIndex = trailingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = trailingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return trivia.IsDirective;
        }

        return false;
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

    #endregion // Methods
}