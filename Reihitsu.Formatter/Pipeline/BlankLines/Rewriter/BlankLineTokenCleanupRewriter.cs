using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;

/// <summary>
/// Subphase that cleans up token-adjacent blank-line trivia
/// </summary>
internal sealed class BlankLineTokenCleanupRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Whether one serialized line break before root documentation should be preserved for node-scoped formatting
    /// </summary>
    private readonly bool _preserveRootDocumentationBoundary;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="preserveRootDocumentationBoundary">Whether one line break before root documentation should be preserved</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineTokenCleanupRewriter(bool preserveRootDocumentationBoundary, CancellationToken cancellationToken)
    {
        _preserveRootDocumentationBoundary = preserveRootDocumentationBoundary;
        _cancellationToken = cancellationToken;
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
        return token.LeadingTrivia.Any(SyntaxTriviaUtilities.IsDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether a token starts with a line break followed by a single-line documentation comment
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> when one boundary line break can be preserved before the documentation comment</returns>
    private static bool StartsWithSingleLineDocumentationCommentAfterLineBreak(SyntaxToken token)
    {
        var foundLineBreak = false;

        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                foundLineBreak = true;

                continue;
            }

            return foundLineBreak && trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        return false;
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
    /// Removes blank lines that appear between a trailing <c>#endregion</c> directive in the
    /// leading trivia and the token itself
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken RemoveBlankLinesAfterTrailingEndRegion(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var endRegionIndex = -1;

        for (var triviaIndex = trivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                endRegionIndex = triviaIndex;

                break;
            }
        }

        if (endRegionIndex < 0)
        {
            return token;
        }

        // The #endregion directive trivia carries its own trailing line break, so any
        // end-of-line trivia that follows it in the list represents a blank line
        var endOfLineCount = 0;
        var endOfLineText = Environment.NewLine;
        var indentationTrivia = new List<SyntaxTrivia>();

        for (var triviaIndex = endRegionIndex + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineText = trivia[triviaIndex].ToString();
                endOfLineCount++;
                indentationTrivia.Clear();
            }
            else if (trivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentationTrivia.Add(trivia[triviaIndex]);
            }
            else
            {
                // A comment or other directive sits between the #endregion and the token — leave it alone
                return token;
            }
        }

        var endRegionEndsWithLineBreak = BlankLineTriviaUtilities.EndsWithLineBreak(trivia[endRegionIndex]);
        var requiredLineBreaks = endRegionEndsWithLineBreak ? 0 : 1;

        if (endOfLineCount <= requiredLineBreaks)
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>(endRegionIndex + 1 + requiredLineBreaks + indentationTrivia.Count);

        for (var triviaIndex = 0; triviaIndex <= endRegionIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        if (requiredLineBreaks == 1)
        {
            newTrivia.Add(SyntaxFactory.EndOfLine(endOfLineText));
        }

        newTrivia.AddRange(indentationTrivia);

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Removes blank lines that appear after a documentation comment in leading trivia
    /// </summary>
    /// <param name="trivia">The leading trivia to update</param>
    /// <param name="documentationCommentIndex">The documentation comment index</param>
    /// <returns>The updated leading trivia</returns>
    private static SyntaxTriviaList RemoveBlankLinesAfterDocumentationComment(SyntaxTriviaList trivia,
                                                                              int documentationCommentIndex)
    {
        if (documentationCommentIndex == trivia.Count - 1)
        {
            return trivia;
        }

        var documentationCommentEndsWithLineBreak = BlankLineTriviaUtilities.EndsWithLineBreak(trivia[documentationCommentIndex]);

        if (TryGetDocumentationBlankLineRun(trivia,
                                            documentationCommentIndex,
                                            documentationCommentEndsWithLineBreak,
                                            out var firstEndOfLineIndex,
                                            out var removeUntil,
                                            out var indentationTrivia) == false)
        {
            return trivia;
        }

        var preservedLineBreakCount = documentationCommentEndsWithLineBreak ? 0 : 1;
        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (removeUntil - documentationCommentIndex) + preservedLineBreakCount);

        for (var triviaIndex = 0; triviaIndex <= documentationCommentIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        if (documentationCommentEndsWithLineBreak == false)
        {
            newTrivia.Add(trivia[firstEndOfLineIndex]);
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = removeUntil + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return SyntaxFactory.TriviaList(newTrivia);
    }

    /// <summary>
    /// Removes blank lines that appear after every documentation comment in a token's leading trivia
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken RemoveBlankLinesAfterLeadingDocumentationComments(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;

        for (var documentationCommentIndex = trivia.Count - 1; documentationCommentIndex >= 0; documentationCommentIndex--)
        {
            if (SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia[documentationCommentIndex]))
            {
                trivia = RemoveBlankLinesAfterDocumentationComment(trivia, documentationCommentIndex);
            }
        }

        return token.WithLeadingTrivia(trivia);
    }

    /// <summary>
    /// Tries to locate the blank-line run that follows the specified documentation comment
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="documentationCommentIndex">The documentation comment index</param>
    /// <param name="documentationCommentEndsWithLineBreak">Whether the documentation comment embeds its terminating line break</param>
    /// <param name="firstEndOfLineIndex">The index of the first explicit end-of-line after the documentation comment</param>
    /// <param name="removeUntil">The final trivia index that belongs to the removable run</param>
    /// <param name="indentationTrivia">Indentation trivia that should be preserved for the next line</param>
    /// <returns><see langword="true"/> when removable blank-line trivia was found; otherwise, <see langword="false"/></returns>
    private static bool TryGetDocumentationBlankLineRun(SyntaxTriviaList trivia,
                                                        int documentationCommentIndex,
                                                        bool documentationCommentEndsWithLineBreak,
                                                        out int firstEndOfLineIndex,
                                                        out int removeUntil,
                                                        out List<SyntaxTrivia> indentationTrivia)
    {
        var endOfLineCount = 1;
        firstEndOfLineIndex = documentationCommentIndex + 1;
        indentationTrivia = [];
        removeUntil = firstEndOfLineIndex;

        while (firstEndOfLineIndex < trivia.Count
               && trivia[firstEndOfLineIndex].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            firstEndOfLineIndex++;
        }

        if (firstEndOfLineIndex >= trivia.Count
            || trivia[firstEndOfLineIndex].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return false;
        }

        removeUntil = firstEndOfLineIndex;

        while (removeUntil + 1 < trivia.Count
               && (trivia[removeUntil + 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || trivia[removeUntil + 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            removeUntil++;

            if (trivia[removeUntil].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;
                indentationTrivia.Clear();
            }
            else if (trivia[removeUntil].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentationTrivia.Add(trivia[removeUntil]);
            }
        }

        var requiredEndOfLineCount = documentationCommentEndsWithLineBreak ? 0 : 1;

        return endOfLineCount > requiredEndOfLineCount;
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc />
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            var keepDocumentationBoundary = _preserveRootDocumentationBoundary
                                            && StartsWithSingleLineDocumentationCommentAfterLineBreak(token);

            token = CollapseLeadingBlankLines(token, keepDocumentationBoundary);
        }

        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            token = RemoveLeadingBlankLines(token);
        }

        // CollapseLeadingBlankLines only inspects the run starting at the first leading trivia, which is
        // the token's own adjacent line only when nothing else precedes it. When a directive or disabled
        // text interposes, that run is the author's separator in front of the directive, not a blank line
        // adjacent to this token — collapsing it would delete a blank line RH5024/25/26/27 do not report.
        // TokenGapNormalizer owns the region that is actually adjacent to the token in that case (issue #711)
        if ((token.IsKind(SyntaxKind.OpenBraceToken)
             || token.IsKind(SyntaxKind.CloseBraceToken)
             || token.IsKind(SyntaxKind.ElseKeyword)
             || token.IsKind(SyntaxKind.CatchKeyword)
             || token.IsKind(SyntaxKind.FinallyKeyword)
             || token.IsKind(SyntaxKind.WhileKeyword))
            && token.LeadingTrivia.Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia) == false)
        {
            var keepSingleLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false;
            token = CollapseLeadingBlankLines(token, keepSingleLineBreak);
        }

        if (token.IsKind(SyntaxKind.CloseBraceToken))
        {
            token = RemoveBlankLinesAfterTrailingEndRegion(token);
        }

        if (HasDocumentationCommentInLeadingTrivia(token))
        {
            token = RemoveBlankLinesAfterLeadingDocumentationComments(token);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}