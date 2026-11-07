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

    #endregion // Methods
}