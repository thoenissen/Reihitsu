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

    #endregion // Methods
}