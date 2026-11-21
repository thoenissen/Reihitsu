using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for analyzing raw string literals
/// </summary>
public static class RawStringLiteralUtilities
{
    #region Methods

    /// <summary>
    /// Gets the offset of the first quote character in a raw string start token
    /// </summary>
    /// <param name="startTokenText">The text of the start token (for example <c>$"""</c> or <c>$$"""</c>)</param>
    /// <returns>The offset of the first quote character</returns>
    public static int GetQuoteOffset(string startTokenText)
    {
        for (var charIndex = 0; charIndex < startTokenText.Length; charIndex++)
        {
            if (startTokenText[charIndex] == '"')
            {
                return charIndex;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the number of whitespace characters at the start of a line of a raw string literal: every character for
    /// which <see cref="char.IsWhiteSpace(char)"/> holds counts as one. Applied to the line of the closing quotes, it is
    /// the width of the whitespace in front of them, which is compared with the column of the opening quotes
    /// </summary>
    /// <param name="lineText">The text of the line, without its line break</param>
    /// <returns>The number of leading whitespace characters</returns>
    public static int GetLeadingWhitespaceLength(string lineText)
    {
        var length = 0;

        while (length < lineText.Length
               && char.IsWhiteSpace(lineText[length]))
        {
            length++;
        }

        return length;
    }

    /// <summary>
    /// Gets the number of columns by which the lines of a multi-line raw string literal after its first line have to
    /// shift so that its closing marker starts in the column of its opening quotes: the column of the opening quotes
    /// minus the width of the whitespace in front of the closing marker (<see cref="GetLeadingWhitespaceLength"/>). Both
    /// are measured on the lines of <paramref name="sourceText"/>, so every line terminator C# recognizes ends a line,
    /// whatever line breaks the document and the literal use. This is the one measure by which the RH5202 analyzer
    /// decides whether a literal is aligned and its code fix decides how far to shift it. For an interpolated literal,
    /// the opening quotes start behind its <c>$</c> characters
    /// </summary>
    /// <param name="sourceText">Text of the document that holds the literal</param>
    /// <param name="literal">The raw string literal expression or interpolated raw string expression</param>
    /// <returns>The shift in columns, positive to the right and negative to the left, or 0 when the literal stands on one line or is aligned</returns>
    public static int GetClosingLineShift(SourceText sourceText, SyntaxNode literal)
    {
        var span = literal.Span;
        var openingLine = sourceText.Lines.GetLineFromPosition(span.Start);
        var closingLine = sourceText.Lines.GetLineFromPosition(span.End);

        if (openingLine.LineNumber == closingLine.LineNumber)
        {
            return 0;
        }

        var quoteOffset = literal is InterpolatedStringExpressionSyntax interpolated
                              ? GetQuoteOffset(interpolated.StringStartToken.Text)
                              : 0;
        var openingQuoteColumn = span.Start - openingLine.Start + quoteOffset;
        var closingWhitespaceLength = GetLeadingWhitespaceLength(sourceText.ToString(closingLine.Span));

        return openingQuoteColumn - closingWhitespaceLength;
    }

    /// <summary>
    /// Gets the width, after the shift, of the indentation the compiler strips from every line of a multi-line raw string
    /// literal that the formatter and the RH5202 code fix write as spaces (FMT-RAW-010, RH5202-007): when the whitespace
    /// in front of the closing marker holds a tab, it is the width that whitespace has after the shift, each whitespace
    /// character one column; otherwise nothing is written as spaces and the width is 0
    /// </summary>
    /// <param name="closingLineText">The text of the line of the closing marker before the shift, without its line break</param>
    /// <param name="closingWhitespaceLengthAfterShift">The number of whitespace characters in front of the closing marker after the shift</param>
    /// <returns>The number of leading whitespace characters of a line of the literal's own text to write as spaces</returns>
    public static int GetStrippedIndentationSpacesWidth(string closingLineText, int closingWhitespaceLengthAfterShift)
    {
        return HasTabInLeadingWhitespace(closingLineText)
                   ? Math.Max(closingWhitespaceLengthAfterShift, 0)
                   : 0;
    }

    /// <summary>
    /// Determines whether the whitespace at the start of a line holds a tab. Applied to the line of a multi-line raw string
    /// literal's closing marker, it decides whether the indentation the compiler strips from the literal is written as
    /// spaces (<see cref="GetStrippedIndentationSpacesWidth"/>)
    /// </summary>
    /// <param name="lineText">The text of the line, without its line break</param>
    /// <returns><see langword="true"/> if the leading whitespace holds a tab</returns>
    public static bool HasTabInLeadingWhitespace(string lineText)
    {
        return lineText.IndexOf('\t', 0, GetLeadingWhitespaceLength(lineText)) >= 0;
    }

    /// <summary>
    /// Gets the indentation a line of a multi-line raw string literal has after it shifts by a number of columns, the one
    /// rule by which both the RH5202 code fix and the formatter move such a line. A shift to the right inserts that many
    /// spaces in front of the line. A shift to the left removes that many characters from the start of the line's leading
    /// whitespace, whatever whitespace they are, and at most all of it, so the line never moves below column 0. Then the
    /// first <paramref name="spacesWidth"/> characters of the shifted indentation, or all of it when it is shorter, are
    /// written as one space each (<see cref="GetStrippedIndentationSpacesWidth"/>); every other whitespace character stays
    /// as written, so each character stays one character and the line keeps the column the shift gives it
    /// </summary>
    /// <param name="indentation">The leading whitespace of the line, each character of it one column (<see cref="GetLeadingWhitespaceLength"/>)</param>
    /// <param name="shift">The shift in columns; positive moves the line to the right, negative to the left</param>
    /// <param name="spacesWidth">The number of leading characters to write as spaces after the shift; 0 writes none</param>
    /// <returns>The leading whitespace of the line after the shift</returns>
    public static string GetShiftedIndentation(string indentation, int shift, int spacesWidth)
    {
        var shiftedIndentation = shift >= 0
                                     ? new string(' ', shift) + indentation
                                     : indentation.Substring(Math.Min(-shift, indentation.Length));
        var spacesLength = Math.Min(spacesWidth, shiftedIndentation.Length);

        return spacesLength > 0
                   ? new string(' ', spacesLength) + shiftedIndentation.Substring(spacesLength)
                   : shiftedIndentation;
    }

    #endregion // Methods
}