using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Subphase that cleans up token-adjacent blank-line trivia
/// </summary>
internal sealed class BlankLineTokenCleanupRewriter : BlankLineSubphaseRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineTokenCleanupRewriter(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the leading trivia contains documentation comments
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if documentation comment trivia is present</returns>
    private static bool HasDocumentationCommentInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                        || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
    }

    /// <summary>
    /// Removes all leading blank lines from the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading blank lines should be removed</param>
    /// <returns>The token with leading blank lines removed</returns>
    private static SyntaxToken RemoveLeadingBlankLines(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;

        var removeUntil = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                removeUntil = triviaIndex;
            }
            else if (kind == SyntaxKind.WhitespaceTrivia)
            {
                // Whitespace on blank lines is OK — continue scanning
            }
            else
            {
                break;
            }
        }

        if (removeUntil < 0)
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>();

        for (var triviaIndex = removeUntil + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Collapses a leading run of blank-line trivia to zero or one line break
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <param name="keepSingleLineBreak">Whether one line break should be preserved</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken CollapseLeadingBlankLines(SyntaxToken token, bool keepSingleLineBreak)
    {
        var trivia = token.LeadingTrivia;
        var endOfLineCount = 0;
        var runEnd = 0;
        var indentationTrivia = new List<SyntaxTrivia>();
        var afterEndOfLine = false;
        var endOfLineText = Environment.NewLine;

        while (runEnd < trivia.Count)
        {
            if (trivia[runEnd].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;
                indentationTrivia.Clear();
                afterEndOfLine = true;
                endOfLineText = trivia[runEnd].ToString();
                runEnd++;

                continue;
            }

            if (trivia[runEnd].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (afterEndOfLine)
                {
                    indentationTrivia.Add(trivia[runEnd]);
                }

                runEnd++;

                continue;
            }

            break;
        }

        if (endOfLineCount == 0 || (keepSingleLineBreak && endOfLineCount == 1))
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - runEnd + indentationTrivia.Count + (keepSingleLineBreak ? 1 : 0));

        if (keepSingleLineBreak)
        {
            newTrivia.Add(SyntaxFactory.EndOfLine(endOfLineText));
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = runEnd; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Removes blank lines that appear after leading documentation comments
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken RemoveBlankLinesAfterLeadingDocumentationComments(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var lastDocumentationCommentIndex = GetLastDocumentationCommentIndex(trivia);

        if (lastDocumentationCommentIndex < 0 || lastDocumentationCommentIndex == trivia.Count - 1)
        {
            return token;
        }

        if (TryGetDocumentationBlankLineRun(trivia, lastDocumentationCommentIndex, out var removeUntil, out var indentationTrivia) == false)
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (removeUntil - lastDocumentationCommentIndex));

        for (var triviaIndex = 0; triviaIndex <= lastDocumentationCommentIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = removeUntil + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Gets the last documentation comment trivia index in the provided list
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <returns>The last documentation comment index, or <c>-1</c> when none exists</returns>
    private static int GetLastDocumentationCommentIndex(SyntaxTriviaList trivia)
    {
        for (var triviaIndex = trivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (IsDocumentationCommentTrivia(trivia[triviaIndex]))
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Determines whether the provided trivia is a documentation comment
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><see langword="true"/> when the trivia is a documentation comment</returns>
    private static bool IsDocumentationCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Tries to locate the blank-line run that follows the last documentation comment
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="lastDocumentationCommentIndex">The last documentation comment index</param>
    /// <param name="removeUntil">The final trivia index that belongs to the removable run</param>
    /// <param name="indentationTrivia">Indentation trivia that should be preserved for the next line</param>
    /// <returns><see langword="true"/> when removable blank-line trivia was found; otherwise, <see langword="false"/></returns>
    private static bool TryGetDocumentationBlankLineRun(SyntaxTriviaList trivia,
                                                        int lastDocumentationCommentIndex,
                                                        out int removeUntil,
                                                        out List<SyntaxTrivia> indentationTrivia)
    {
        var eolIndex = lastDocumentationCommentIndex + 1;
        indentationTrivia = [];
        removeUntil = eolIndex;

        if (eolIndex >= trivia.Count || trivia[eolIndex].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return false;
        }

        while (removeUntil + 1 < trivia.Count
               && (trivia[removeUntil + 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || trivia[removeUntil + 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            removeUntil++;

            if (trivia[removeUntil].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                indentationTrivia.Clear();
            }
            else if (trivia[removeUntil].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentationTrivia.Add(trivia[removeUntil]);
            }
        }

        return removeUntil != eolIndex;
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc />
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        CancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            token = CollapseLeadingBlankLines(token, keepSingleLineBreak: false);
        }

        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            token = RemoveLeadingBlankLines(token);
        }

        if (token.IsKind(SyntaxKind.OpenBraceToken)
            || token.IsKind(SyntaxKind.CloseBraceToken)
            || token.IsKind(SyntaxKind.ElseKeyword)
            || token.IsKind(SyntaxKind.CatchKeyword)
            || token.IsKind(SyntaxKind.FinallyKeyword)
            || token.IsKind(SyntaxKind.WhileKeyword))
        {
            var keepSingleLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false;
            token = CollapseLeadingBlankLines(token, keepSingleLineBreak);
        }

        if (HasDocumentationCommentInLeadingTrivia(token))
        {
            token = RemoveBlankLinesAfterLeadingDocumentationComments(token);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}