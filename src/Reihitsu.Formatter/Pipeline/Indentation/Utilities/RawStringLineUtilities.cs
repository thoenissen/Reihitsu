using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Decides which nodes are multi-line raw string literals (<see cref="IsMultiLineRawStringLiteral"/>) and which of their
/// lines the move of the literal's lines writes. <see cref="MovesLine"/> is the one statement of that decision, which
/// the indentation phase and the code fix that aligns a raw string literal both follow: every empty line stays empty,
/// and a line that starts inside nested text (<see cref="StartsInsideNestedText"/>) stays as written
/// </summary>
internal static class RawStringLineUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the node is a multi-line raw string literal: plain, UTF-8, or interpolated
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node is a multi-line raw string literal</returns>
    public static bool IsMultiLineRawStringLiteral(SyntaxNode node)
    {
        return node switch
               {
                   LiteralExpressionSyntax literal => literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                                                      || literal.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken),
                   InterpolatedStringExpressionSyntax interpolated => interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether moving the lines of a multi-line raw string literal writes the given line, a line after the
    /// literal's opening line. An empty line is not written, so it stays empty and no whitespace is left at its end; a
    /// line that starts inside nested text (<see cref="StartsInsideNestedText"/>) is not written, so the nested
    /// literal's value and the comment's text stay as they are
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="line">The line, from the text of the root's tree</param>
    /// <returns><see langword="true"/> if the move writes the line</returns>
    public static bool MovesLine(SyntaxNode root, TextLine line)
    {
        return line.Span.Length > 0
               && StartsInsideNestedText(root, line.Start) == false;
    }

    /// <summary>
    /// Determines whether a line that starts at the given position starts inside the text of a literal other than a
    /// raw string, or inside a <c>/* */</c> or <c>/** */</c> comment, that began on an earlier line. Moving the start
    /// of such a line would change the literal's value or the comment's text. The following lines of a <c>///</c>
    /// comment start with a marker that is laid out like the first one, so they move
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="lineStart">The position at which the line starts</param>
    /// <returns><see langword="true"/> if the line starts inside such text</returns>
    public static bool StartsInsideNestedText(SyntaxNode root, int lineStart)
    {
        var trivia = root.FindTrivia(lineStart);

        if (trivia.Span.Start < lineStart
            && trivia.Span.End > lineStart
            && (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)))
        {
            return true;
        }

        var token = root.FindToken(lineStart);

        if (token.SpanStart > lineStart)
        {
            return false;
        }

        if (token.SpanStart < lineStart)
        {
            return IsNonRawStringText(token);
        }

        var previousToken = token.GetPreviousToken();

        return token.LeadingTrivia.Count == 0
               && previousToken.Span.End == lineStart
               && IsNonRawStringText(previousToken);
    }

    /// <summary>
    /// Determines whether the token holds the text of a string literal other than a raw string: a regular or verbatim
    /// string, or the text of an interpolated string that is not raw
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token holds such text</returns>
    private static bool IsNonRawStringText(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.StringLiteralToken)
            || token.IsKind(SyntaxKind.Utf8StringLiteralToken))
        {
            return true;
        }

        return token.IsKind(SyntaxKind.InterpolatedStringTextToken)
               && token.Parent?.Parent is InterpolatedStringExpressionSyntax interpolated
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken) == false;
    }

    #endregion // Methods
}