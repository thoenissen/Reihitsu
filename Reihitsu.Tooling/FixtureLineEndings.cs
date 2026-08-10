using System.Text;

namespace Reihitsu.Tooling;

/// <summary>
/// Normalizes and describes the line endings of fixture source text
/// </summary>
public static class FixtureLineEndings
{
    #region Constants

    /// <summary>
    /// Line feed line ending
    /// </summary>
    public const string LineFeed = "\n";

    /// <summary>
    /// Carriage return line feed line ending
    /// </summary>
    public const string CarriageReturnLineFeed = "\r\n";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Rewrites every line ending of the provided text to the requested sequence
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <param name="lineEnding">Line ending every separator is rewritten to</param>
    /// <returns>The text using only the requested line ending</returns>
    public static string Normalize(string text, string lineEnding)
    {
        var normalized = text.Replace(CarriageReturnLineFeed, LineFeed).Replace("\r", LineFeed);

        return lineEnding == LineFeed
                   ? normalized
                   : normalized.Replace(LineFeed, lineEnding);
    }

    /// <summary>
    /// Gets the display name of a line ending sequence
    /// </summary>
    /// <param name="lineEnding">Line ending to name</param>
    /// <returns>The name used in the fixture headers</returns>
    public static string GetName(string lineEnding)
    {
        return lineEnding == CarriageReturnLineFeed ? "CRLF" : "LF";
    }

    /// <summary>
    /// Determines whether the provided text uses the requested line ending exclusively. A code fix that inserts
    /// <see cref="System.Environment.NewLine"/> instead of the document's own separator produces mixed endings,
    /// which is exactly the instability a sweep needs to see
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <param name="lineEnding">Line ending the text is expected to use</param>
    /// <returns><see langword="true"/> when every separator matches; otherwise, <see langword="false"/></returns>
    public static bool UsesOnly(string text, string lineEnding)
    {
        var (carriageReturnLineFeed, lineFeed, carriageReturn) = Count(text);

        return lineEnding == CarriageReturnLineFeed
                   ? lineFeed == 0 && carriageReturn == 0
                   : carriageReturnLineFeed == 0 && carriageReturn == 0;
    }

    /// <summary>
    /// Describes the line-ending sequences contained in the provided text
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <returns>Counts for CRLF, LF, and CR</returns>
    public static string Describe(string text)
    {
        var (carriageReturnLineFeed, lineFeed, carriageReturn) = Count(text);

        return $"CRLF={carriageReturnLineFeed}, LF={lineFeed}, CR={carriageReturn}";
    }

    /// <summary>
    /// Splits the provided text into lines, discarding the separators
    /// </summary>
    /// <param name="text">Text to split</param>
    /// <returns>The content lines</returns>
    public static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        var builder = new StringBuilder();

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (character == '\r')
            {
                if (index + 1 < text.Length && text[index + 1] == '\n')
                {
                    index++;
                }

                lines.Add(builder.ToString());
                builder.Clear();
            }
            else if (character == '\n')
            {
                lines.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(character);
            }
        }

        if (builder.Length > 0)
        {
            lines.Add(builder.ToString());
        }

        return lines;
    }

    /// <summary>
    /// Counts the line-ending sequences contained in the provided text
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <returns>The number of CRLF, lone LF, and lone CR sequences</returns>
    private static (int CarriageReturnLineFeed, int LineFeed, int CarriageReturn) Count(string text)
    {
        var carriageReturnLineFeed = 0;
        var lineFeed = 0;
        var carriageReturn = 0;

        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '\r')
            {
                if (index + 1 < text.Length && text[index + 1] == '\n')
                {
                    carriageReturnLineFeed++;
                    index++;
                }
                else
                {
                    carriageReturn++;
                }
            }
            else if (text[index] == '\n')
            {
                lineFeed++;
            }
        }

        return (carriageReturnLineFeed, lineFeed, carriageReturn);
    }

    #endregion // Methods
}