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
    /// Determines whether a token or its formatter-controlled leading comment starts at the expected column
    /// </summary>
    /// <param name="token">Token whose line anchor should be checked</param>
    /// <param name="expectedColumn">Required line-anchor column</param>
    /// <returns><see langword="true"/> when the line anchor matches the expected column</returns>
    public static bool IsAlignedAt(SyntaxToken token, int expectedColumn)
    {
        var tokenPosition = token.GetLocation().GetLineSpan().StartLinePosition;
        var commentSharesTokenLine = false;

        foreach (var trivia in token.LeadingTrivia.Where(SyntaxTriviaUtilities.IsCommentTrivia))
        {
            var commentSpan = trivia.SyntaxTree.GetLineSpan(trivia.FullSpan);
            var commentStartsOnTokenLine = commentSpan.StartLinePosition.Line == tokenPosition.Line;
            var expectedCommentColumn = expectedColumn;

            if (commentStartsOnTokenLine == false && token.IsKind(SyntaxKind.CloseBraceToken))
            {
                expectedCommentColumn += SyntaxIndentationUtilities.IndentSize;
            }

            if (commentSpan.StartLinePosition.Character != expectedCommentColumn)
            {
                return false;
            }

            if (DocumentationCommentUtilities.AreContinuationExteriorMarkersAligned(trivia, expectedCommentColumn) == false)
            {
                return false;
            }

            if (commentSpan.EndLinePosition.Line == tokenPosition.Line
                && commentSpan.EndLinePosition.Character > 0)
            {
                commentSharesTokenLine = true;
            }
        }

        // The formatter controls the indentation before a leading comment but preserves the token's
        // raw position after a comment that ends on the same line.
        return commentSharesTokenLine || tokenPosition.Character == expectedColumn;
    }

    #endregion // Methods
}