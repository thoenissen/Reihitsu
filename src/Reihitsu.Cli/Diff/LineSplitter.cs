namespace Reihitsu.Cli.Diff;

/// <summary>
/// Splits text content into individual lines, handling both Unix and Windows line endings
/// </summary>
internal static class LineSplitter
{
    #region Methods

    /// <summary>
    /// Splits the content into individual lines, treating an empty string as zero lines
    /// </summary>
    /// <param name="content">The text content to split</param>
    /// <returns>An array of lines</returns>
    public static string[] Split(string content)
    {
        if (content.Length == 0)
        {
            return [];
        }

        // Order matters: "\r\n" must precede the lone separators so a Windows line ending is consumed as one break.
        return content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
    }

    #endregion // Methods
}