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
    /// Determines whether the leading trivia of the specified token contains a blank line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line is found in the leading trivia</returns>
    private static bool HasBlankLineInLeadingTrivia(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var atLineStart = true;
        var sawLineBreak = false;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
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
            else
            {
                atLineStart = false;
            }
        }

        return false;
    }

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
            return HasBlankLineInLeadingTrivia(token)
                       ? 1
                       : 0;
        }

        var state = BlankLineGapAnalysisState.Initial;
        state = AnalyzeTriviaRange(previousToken.TrailingTrivia, 0, previousToken.TrailingTrivia.Count, state);
        state = AnalyzeTriviaRange(token.LeadingTrivia, 0, token.LeadingTrivia.Count, state);

        return state.BlankLineCount;
    }

    /// <summary>
    /// Counts blank lines from the previous token to a leading-trivia prefix of the specified token
    /// </summary>
    /// <param name="token">The token whose leading-trivia prefix should be inspected</param>
    /// <param name="leadingTriviaEndExclusive">Exclusive upper bound in the leading-trivia list</param>
    /// <returns>The number of blank lines up to the specified leading-trivia index</returns>
    protected static int CountBlankLinesBeforeLeadingTriviaIndex(SyntaxToken token, int leadingTriviaEndExclusive)
    {
        var state = BlankLineGapAnalysisState.Initial;
        var previousToken = token.GetPreviousToken();

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            state = AnalyzeTriviaRange(previousToken.TrailingTrivia, 0, previousToken.TrailingTrivia.Count, state);
        }

        state = AnalyzeTriviaRange(token.LeadingTrivia, 0, leadingTriviaEndExclusive, state);

        return state.BlankLineCount;
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

    /// <summary>
    /// Accumulates blank-line gap state over a trivia range
    /// </summary>
    /// <param name="triviaList">Trivia list to analyze</param>
    /// <param name="startIndex">Start index in <paramref name="triviaList"/></param>
    /// <param name="endIndexExclusive">Exclusive end index in <paramref name="triviaList"/></param>
    /// <param name="state">Current analysis state</param>
    /// <returns>Updated gap analysis state</returns>
    private static BlankLineGapAnalysisState AnalyzeTriviaRange(SyntaxTriviaList triviaList,
                                                                int startIndex,
                                                                int endIndexExclusive,
                                                                BlankLineGapAnalysisState state)
    {
        for (var triviaIndex = startIndex; triviaIndex < endIndexExclusive; triviaIndex++)
        {
            state = AnalyzeTrivia(triviaList[triviaIndex], state);
        }

        return state;
    }

    /// <summary>
    /// Accumulates blank-line gap state for one trivia item
    /// </summary>
    /// <param name="trivia">Trivia to analyze</param>
    /// <param name="state">Current analysis state</param>
    /// <returns>Updated gap analysis state</returns>
    private static BlankLineGapAnalysisState AnalyzeTrivia(SyntaxTrivia trivia, BlankLineGapAnalysisState state)
    {
        if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
        {
            return state;
        }

        if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
        {
            return state.AdvanceOnLineBreak(false);
        }

        state = state.MarkLineHasContent();

        var text = trivia.ToFullString();

        for (var textIndex = 0; textIndex < text.Length; textIndex++)
        {
            var lineBreakLength = GetLineBreakLength(text, textIndex);

            if (lineBreakLength == 0)
            {
                continue;
            }

            var nextLineHasContent = textIndex + lineBreakLength < text.Length;
            state = state.AdvanceOnLineBreak(nextLineHasContent);
            textIndex += lineBreakLength - 1;
        }

        return state;
    }

    /// <summary>
    /// Gets the length of the line-break sequence that starts at the specified index
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <param name="index">The index to inspect</param>
    /// <returns>The length of the line-break sequence; otherwise, <c>0</c></returns>
    private static int GetLineBreakLength(string text, int index)
    {
        if (text[index] == '\r')
        {
            return index + 1 < text.Length && text[index + 1] == '\n'
                       ? 2
                       : 1;
        }

        return text[index] == '\n'
                   ? 1
                   : 0;
    }

    #endregion // Methods
}