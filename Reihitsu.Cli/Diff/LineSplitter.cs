namespace Reihitsu.Cli.Diff;

/// <summary>
/// Splits text content into individual lines, handling both Unix and Windows line endings
/// </summary>
internal static class LineSplitter
{
    #region Methods

    /// <summary>
    /// Splits the content into individual lines
    /// </summary>
    /// <param name="content">The text content to split</param>
    /// <returns>An array of lines</returns>
    public static string[] Split(string content)
    {
        return content.Split(["\r\n", "\n"], StringSplitOptions.None);
    }

    #endregion // Methods
}