using System;

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
    /// Gets the edit that shifts a line of a multi-line raw string literal by a number of columns, the one rule by which
    /// both the RH5202 code fix and the formatter move such a line. A shift to the right inserts that many spaces in front
    /// of the line. A shift to the left removes that many characters from the start of the line's leading whitespace,
    /// whatever whitespace they are, and at most all of it, so the line never moves below column 0. Every whitespace
    /// character the shift does not remove stays as written
    /// </summary>
    /// <param name="leadingWhitespaceLength">The number of whitespace characters at the start of the line, each of them one column (<see cref="GetLeadingWhitespaceLength"/>)</param>
    /// <param name="shift">The shift in columns; positive moves the line to the right, negative to the left</param>
    /// <returns>The number of characters to remove at the start of the line, and the number of spaces to insert there</returns>
    public static (int RemovedLength, int InsertedSpaces) GetLineShiftEdit(int leadingWhitespaceLength, int shift)
    {
        return shift >= 0
                   ? (0, shift)
                   : (Math.Min(-shift, leadingWhitespaceLength), 0);
    }

    #endregion // Methods
}