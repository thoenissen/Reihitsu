using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RawStringAlignment.Utilities;

/// <summary>
/// Determines how many columns the raw-string phase moves each line of a multi-line raw string literal: plain, UTF-8,
/// or interpolated. A literal moves every line after its opening line by the distance between its opening quotes and
/// its closing marker; a literal nested in an interpolation hole adds its own distance to the lines inside it, measured
/// after the literals around it moved its opening quotes and its closing marker; and a line that starts inside the text
/// of a nested literal other than a raw string, or inside a nested <c>/* */</c> or <c>/** */</c> comment, never moves.
/// The raw-string phase applies these shifts, and the indentation phase predicts them, so a line that is aligned to a
/// token or a comment that takes a token's column ends up where that token ends up
/// </summary>
internal sealed class RawStringLineShifts
{
    #region Fields

    /// <summary>
    /// The multi-line raw string literals of the tree, outermost and earliest first
    /// </summary>
    private readonly List<RawStringLiteral> _literals = [];

    /// <summary>
    /// Maps each line that a literal moves to the indices of the literals that move it, outermost first
    /// </summary>
    private readonly Dictionary<int, List<int>> _movingLiterals = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public RawStringLineShifts(SyntaxNode root, SourceText text, CancellationToken cancellationToken)
    {
        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMultiLineRawStringLiteral(node) == false)
            {
                continue;
            }

            var openingLine = text.Lines.GetLineFromPosition(node.SpanStart);
            var closingLine = text.Lines.GetLineFromPosition(node.Span.End);

            if (openingLine.LineNumber == closingLine.LineNumber)
            {
                continue;
            }

            var literalIndex = _literals.Count;

            _literals.Add(new RawStringLiteral(node,
                                               openingLine.LineNumber,
                                               closingLine.LineNumber,
                                               GetLeadingSpaceCount(text, closingLine),
                                               GetQuoteOffset(node)));

            for (var lineNumber = openingLine.LineNumber + 1; lineNumber <= closingLine.LineNumber; lineNumber++)
            {
                if (StartsInsideNestedText(root, text.Lines[lineNumber].Start))
                {
                    continue;
                }

                if (_movingLiterals.TryGetValue(lineNumber, out var literalIndices) == false)
                {
                    literalIndices = [];
                    _movingLiterals[lineNumber] = literalIndices;
                }

                literalIndices.Add(literalIndex);
            }
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether the tree holds no multi-line raw string literal, so no line moves
    /// </summary>
    public bool IsEmpty => _literals.Count == 0;

    #endregion // Properties

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
    /// Counts the number of leading space characters of a line
    /// </summary>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="line">The line to examine</param>
    /// <returns>The number of leading space characters</returns>
    public static int GetLeadingSpaceCount(SourceText text, TextLine line)
    {
        var count = 0;

        while (line.Start + count < line.End
               && text[line.Start + count] == ' ')
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the number of columns the raw-string phase moves the given line
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="getColumnBeforeShift">Gets the column a literal's first token has before the raw-string phase moves any line</param>
    /// <returns>The number of columns; positive moves the line to the right, and zero leaves it in place</returns>
    public int GetShift(int lineNumber, Func<SyntaxToken, int> getColumnBeforeShift)
    {
        if (_movingLiterals.TryGetValue(lineNumber, out var literalIndices) == false)
        {
            return 0;
        }

        var shift = 0;

        foreach (var literalIndex in literalIndices)
        {
            shift += GetDistance(literalIndex, getColumnBeforeShift);
        }

        return shift;
    }

    /// <summary>
    /// Gets the offset of the first opening quote within the literal, so an interpolated literal is measured under its
    /// quotes rather than under its <c>$</c> characters
    /// </summary>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <returns>The offset of the first opening quote</returns>
    private static int GetQuoteOffset(SyntaxNode node)
    {
        return node is InterpolatedStringExpressionSyntax interpolated
                   ? RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text)
                   : 0;
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
    private static bool StartsInsideNestedText(SyntaxNode root, int lineStart)
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

    /// <summary>
    /// Gets the distance from a literal's closing marker to its opening quotes, both measured after the literals
    /// before and around it moved them. The literal moves its lines by that distance
    /// </summary>
    /// <param name="literalIndex">The index of the literal</param>
    /// <param name="getColumnBeforeShift">Gets the column a literal's first token has before the raw-string phase moves any line</param>
    /// <returns>The distance in columns</returns>
    private int GetDistance(int literalIndex, Func<SyntaxToken, int> getColumnBeforeShift)
    {
        var literal = _literals[literalIndex];
        var openingColumn = getColumnBeforeShift(literal.Node.GetFirstToken())
                            + literal.QuoteOffset
                            + GetShift(literal.OpeningLine, getColumnBeforeShift);
        var closingColumn = literal.ClosingColumn;

        // The closing line is moved by the literal itself and by the literals around it, which come first
        foreach (var outerIndex in _movingLiterals[literal.ClosingLine])
        {
            if (outerIndex == literalIndex)
            {
                break;
            }

            closingColumn += GetDistance(outerIndex, getColumnBeforeShift);
        }

        return openingColumn - closingColumn;
    }

    #endregion // Methods
}