using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Subphase that enforces blank-line boundaries before comments and #endregion directives
/// </summary>
internal sealed class BlankLineTriviaBoundaryRewriter : BlankLineSubphaseRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineTriviaBoundaryRewriter(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is a single-line, multi-line, or documentation comment</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        var kind = trivia.Kind();

        return kind is SyntaxKind.SingleLineCommentTrivia
                    or SyntaxKind.MultiLineCommentTrivia
                    or SyntaxKind.SingleLineDocumentationCommentTrivia
                    or SyntaxKind.MultiLineDocumentationCommentTrivia;
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a comment
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any comment trivia is found in the leading trivia</returns>
    private static bool HasCommentInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(IsCommentTrivia);
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains an end region directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any end region directive trivia is found in the leading trivia</returns>
    private static bool HasEndRegionDirectiveInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
    }

    /// <summary>
    /// Determines whether the trailing trivia of the specified token contains an end region directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any end region directive trivia is found in the trailing trivia</returns>
    private static bool HasEndRegionDirectiveInTrailingTrivia(SyntaxToken token)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
    }

    /// <summary>
    /// Ensures exactly one blank line exists before the first comment in the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <returns>The token with a single blank line before the first comment</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstComment(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var commentIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (IsCommentTrivia(trivia[triviaIndex]))
            {
                commentIndex = triviaIndex;

                break;
            }
        }

        if (commentIndex < 0)
        {
            return token;
        }

        var previousTokenLine = token.GetPreviousToken();

        if (previousTokenLine != default && previousTokenLine.IsKind(SyntaxKind.None) == false)
        {
            var previousLine = previousTokenLine.GetLocation().GetLineSpan().EndLinePosition.Line;
            var commentLine = trivia[commentIndex].GetLocation().GetLineSpan().StartLinePosition.Line;
            var blankLineCountByLine = commentLine - previousLine - 1;

            if (blankLineCountByLine == 1)
            {
                return token;
            }
        }

        var lineStartIndex = commentIndex;

        while (lineStartIndex > 0 && trivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            lineStartIndex--;
        }

        var gapStartIndex = lineStartIndex;

        while (gapStartIndex > 0
               && (trivia[gapStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia)
                   || trivia[gapStartIndex - 1].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            gapStartIndex--;
        }

        var localBlankLineCount = 0;
        var localSawLineBreak = false;
        var localAtLineStart = true;

        for (var triviaIndex = gapStartIndex; triviaIndex < lineStartIndex; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (localSawLineBreak && localAtLineStart)
                {
                    localBlankLineCount++;
                }

                localSawLineBreak = true;
                localAtLineStart = true;
            }
            else if (kind != SyntaxKind.WhitespaceTrivia)
            {
                localAtLineStart = false;
            }
        }

        var blankLineCount = CountBlankLinesBeforeLeadingTriviaIndex(token, commentIndex);

        if (blankLineCount == 1 || (localBlankLineCount == 0 && HasBlankLineBeforeIndex(trivia, commentIndex)))
        {
            return token;
        }

        var indentationTrivia = new List<SyntaxTrivia>(commentIndex - lineStartIndex);

        for (var triviaIndex = lineStartIndex; triviaIndex < commentIndex; triviaIndex++)
        {
            indentationTrivia.Add(trivia[triviaIndex]);
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (lineStartIndex - gapStartIndex) + indentationTrivia.Count + 1);

        for (var triviaIndex = 0; triviaIndex < gapStartIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        newTrivia.Add(SyntaxFactory.EndOfLine(Context.EndOfLine));
        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = commentIndex; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Ensures a blank line exists before the first end region directive in the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <param name="previousTokenEndsWithLineBreak">Whether the original previous token already ended with a line break</param>
    /// <returns>The token with a blank line inserted before the first end region directive, or the original if one already exists</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstEndRegion(SyntaxToken token, bool previousTokenEndsWithLineBreak)
    {
        var trivia = token.LeadingTrivia;
        var directiveIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
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
            newTrivia = newTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(Context.EndOfLine));
            insertIndex++;
            endOfLineCount++;
        }

        return token.WithLeadingTrivia(newTrivia);
    }

    /// <summary>
    /// Ensures a blank line exists before the first end region directive in the specified token's trailing trivia
    /// </summary>
    /// <param name="token">The token whose trailing trivia should be checked</param>
    /// <returns>The token with a blank line inserted before the first end region directive, or the original if one already exists</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia(SyntaxToken token)
    {
        var trivia = token.TrailingTrivia;
        var directiveIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
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

        if (endOfLineCount >= 2)
        {
            return token;
        }

        var newTrivia = trivia;

        while (endOfLineCount < 2)
        {
            newTrivia = newTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(Context.EndOfLine));
            insertIndex++;
            endOfLineCount++;
        }

        return token.WithTrailingTrivia(newTrivia);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc />
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        CancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var previousToken = token.GetPreviousToken();
        var isFirstInBlock = IsFirstInBlock(previousToken);
        var previousTokenEndsWithLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        if (HasCommentInLeadingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstComment(token);
        }

        if (HasEndRegionDirectiveInLeadingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegion(token, previousTokenEndsWithLineBreak);
        }

        if (HasEndRegionDirectiveInTrailingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia(token);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}