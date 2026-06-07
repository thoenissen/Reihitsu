using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Base type for blank-line subphase rewriters
/// </summary>
internal abstract class BlankLineSubphaseRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context of the current blank-line subphase
    /// </summary>
    protected FormattingContext Context { get; }

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    protected CancellationToken CancellationToken { get; }

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected BlankLineSubphaseRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        Context = context;
        CancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the full gap before the specified token already contains a blank line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line exists before the token; otherwise, <see langword="false"/></returns>
    protected static bool HasBlankLineBeforeToken(SyntaxToken token)
    {
        return CountBlankLinesBeforeToken(token) > 0;
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
    /// Counts blank lines from the previous token to a leading-trivia prefix of the specified token
    /// </summary>
    /// <param name="token">The token whose leading-trivia prefix should be inspected</param>
    /// <param name="leadingTriviaEndExclusive">Exclusive upper bound in the leading-trivia list</param>
    /// <returns>The number of blank lines up to the specified leading-trivia index</returns>
    protected static int CountBlankLinesBeforeLeadingTriviaIndex(SyntaxToken token, int leadingTriviaEndExclusive)
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
    protected static bool IsFirstInBlock(SyntaxToken previousToken)
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
    protected static bool HasBlankLineBeforeIndex(SyntaxTriviaList trivia, int endIndex)
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
    /// Ensures a blank line exists before the specified statement by inserting one if absent
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    protected StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        var eol = SyntaxFactory.EndOfLine(Context.EndOfLine);
        var newLeading = firstToken.LeadingTrivia.Insert(0, eol);
        var newToken = firstToken.WithLeadingTrivia(newLeading);

        return statement.ReplaceToken(firstToken, newToken);
    }

    #endregion // Methods
}