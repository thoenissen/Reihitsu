using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared layout analysis for initializer anchors and multi-line complex elements
/// </summary>
internal static class InitializerLayoutAnalysisUtilities
{
    #region Methods

    /// <summary>
    /// Finds the first brace or expression that does not match recursive complex-element layout. The opening brace's
    /// line is checked at the expected column; the closing brace and the expressions are measured from the column the
    /// opening brace stands in, which is behind a comment that ends on the brace's line in front of it, and otherwise
    /// the expected column itself (SHARED-INIT-002)
    /// </summary>
    /// <param name="complexElement">Complex-element initializer to inspect</param>
    /// <param name="expectedOpenBraceColumn">Required column of the complex element's opening brace</param>
    /// <returns>The first misaligned location, or <see langword="null"/> when the element is valid</returns>
    public static Location FindFirstComplexElementMisalignment(InitializerExpressionSyntax complexElement, int expectedOpenBraceColumn)
    {
        var openBrace = complexElement.OpenBraceToken;
        var closeBrace = complexElement.CloseBraceToken;
        var openBracePosition = openBrace.GetLocation().GetLineSpan().StartLinePosition;
        var closeBracePosition = closeBrace.GetLocation().GetLineSpan().StartLinePosition;

        if (IsAlignedAt(openBrace, expectedOpenBraceColumn) == false)
        {
            return complexElement.GetLocation();
        }

        // The brace passed the check above, so it stands in the expected column unless a comment that ends on its line
        // stands in front of it; then the brace's own column, behind the comment, anchors the element's other lines
        var openBraceColumn = openBracePosition.Character;

        if (openBracePosition.Line == closeBracePosition.Line)
        {
            return null;
        }

        if (IsAlignedAt(closeBrace, openBraceColumn) == false)
        {
            return complexElement.GetLocation();
        }

        var expressionLines = new HashSet<int>();

        foreach (var expression in complexElement.Expressions)
        {
            var firstToken = expression.GetFirstToken();
            var expressionPosition = firstToken.GetLocation().GetLineSpan().StartLinePosition;

            if (expressionPosition.Line <= openBracePosition.Line
                || expressionPosition.Line >= closeBracePosition.Line
                || expressionLines.Add(expressionPosition.Line) == false
                || IsAlignedAt(firstToken, openBraceColumn + SyntaxIndentationUtilities.IndentSize) == false)
            {
                return expression.GetLocation();
            }

            if (expression is InitializerExpressionSyntax nestedComplexElement
                && nestedComplexElement.IsKind(SyntaxKind.ComplexElementInitializerExpression))
            {
                var nestedMisalignment = FindFirstComplexElementMisalignment(nestedComplexElement, openBraceColumn + SyntaxIndentationUtilities.IndentSize);

                if (nestedMisalignment != null)
                {
                    return nestedMisalignment;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a token's line starts at the expected column (SHARED-INIT-001). Every comment in the token's
    /// leading trivia that starts an earlier line - only whitespace in front of it on that line - starts at the expected
    /// column, one level deeper in front of a closing brace, with the further lines of a <c>///</c> comment aligned to
    /// it; a further comment behind it on that line, or behind the tail of a comment from a line above, is not checked,
    /// also when it ends on the token's line. On the token's line, the comment that starts
    /// the line is its first element and starts at the expected column (SHARED-COMMENT-011); a further comment behind it
    /// is not checked, and neither is the token behind a comment that ends on its line. Otherwise the token starts at the
    /// expected column
    /// </summary>
    /// <param name="token">Token whose line anchor should be checked</param>
    /// <param name="expectedColumn">Required line-anchor column</param>
    /// <returns><see langword="true"/> when the line anchor matches the expected column</returns>
    public static bool IsAlignedAt(SyntaxToken token, int expectedColumn)
    {
        var tokenLine = SyntaxTokenPositionUtilities.GetLine(token);
        var earlierLineColumn = token.IsKind(SyntaxKind.CloseBraceToken)
                                    ? expectedColumn + SyntaxIndentationUtilities.IndentSize
                                    : expectedColumn;

        var leadingTrivia = token.LeadingTrivia;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            var trivia = leadingTrivia[triviaIndex];

            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) == false
                || TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, triviaIndex) == false)
            {
                continue;
            }

            var commentStart = CommentPositionUtilities.GetStartPosition(trivia);

            if (commentStart.Line != tokenLine
                && (commentStart.Character != earlierLineColumn
                    || DocumentationCommentUtilities.AreContinuationExteriorMarkersAligned(trivia, earlierLineColumn) == false))
            {
                return false;
            }
        }

        // The formatter controls the indentation of the comment that starts the line but preserves the token's raw
        // position behind a comment that ends on its line
        if (CommentPositionUtilities.TryGetLineStartComment(token, true, out var lineStartComment))
        {
            var commentStart = CommentPositionUtilities.GetStartPosition(lineStartComment);

            return commentStart.Line != tokenLine
                   || commentStart.Character == expectedColumn;
        }

        return SyntaxTokenPositionUtilities.GetColumn(token) == expectedColumn;
    }

    #endregion // Methods
}