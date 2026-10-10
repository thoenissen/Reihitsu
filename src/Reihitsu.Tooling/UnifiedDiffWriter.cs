using System.Text;

namespace Reihitsu.Tooling;

/// <summary>
/// Renders the difference between a fixture's input and the source the code fix produced from it
/// </summary>
public static class UnifiedDiffWriter
{
    #region Constants

    /// <summary>
    /// The marker emitted after a line that is the last line of a file lacking a trailing newline
    /// </summary>
    private const string NoNewlineMarker = "\\ No newline at end of file";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Generates a unified diff between two revisions of the same fixture. Sweep fixtures are minimal by
    /// construction, so the whole file is emitted as a single hunk rather than split into context windows
    /// </summary>
    /// <param name="path">Display path of the fixture</param>
    /// <param name="before">Source before the fix</param>
    /// <param name="after">Source after the fix</param>
    /// <returns>The unified diff, or an empty string when both revisions are identical</returns>
    public static string Generate(string path, string before, string after)
    {
        if (string.Equals(before, after, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var beforeLines = ToDiffLines(before);
        var afterLines = ToDiffLines(after);
        var builder = new StringBuilder();

        builder.Append("--- a/").Append(path).Append('\n');
        builder.Append("+++ b/").Append(path).Append('\n');
        builder.Append("@@ -1,").Append(beforeLines.Count).Append(" +1,").Append(afterLines.Count).Append(" @@").Append('\n');

        foreach (var (kind, line) in Compare(beforeLines, afterLines))
        {
            AppendLine(builder, kind, line);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Splits content into diff lines, carrying the trailing-newline state as a flag on the last line rather than
    /// encoding it into the line's authored text. Only the last line can ever be unterminated
    /// </summary>
    /// <param name="content">Content to split</param>
    /// <returns>The diff lines, each paired with whether it is followed by a line break in the source</returns>
    private static List<(string Text, bool IsTerminated)> ToDiffLines(string content)
    {
        var lines = FixtureLineEndings.SplitLines(content);
        var lastLineIsTerminated = EndsWithLineBreak(content);
        var result = new List<(string Text, bool IsTerminated)>(lines.Count);

        for (var index = 0; index < lines.Count; index++)
        {
            var isLastLine = index == lines.Count - 1;

            result.Add((lines[index], isLastLine == false || lastLineIsTerminated));
        }

        return result;
    }

    /// <summary>
    /// Appends a diff line, rendering the authored text verbatim and emitting the no-newline marker only when the
    /// line itself is genuinely unterminated
    /// </summary>
    /// <param name="builder">Builder receiving the line</param>
    /// <param name="kind">Unified-diff line prefix</param>
    /// <param name="line">Line text and its termination state</param>
    private static void AppendLine(StringBuilder builder, char kind, (string Text, bool IsTerminated) line)
    {
        builder.Append(kind).Append(line.Text).Append('\n');

        if (line.IsTerminated == false)
        {
            builder.Append(NoNewlineMarker).Append('\n');
        }
    }

    /// <summary>
    /// Determines whether content ends with a line break
    /// </summary>
    /// <param name="content">Content to inspect</param>
    /// <returns><see langword="true"/> when the content ends with a line break; otherwise, <see langword="false"/></returns>
    private static bool EndsWithLineBreak(string content)
    {
        return content.Length > 0 && (content[^1] == '\n' || content[^1] == '\r');
    }

    /// <summary>
    /// Compares two line sequences and yields each line with its unified-diff prefix. Two lines are equal only
    /// when both their text and their termination state match, so a real change in trailing-newline state is
    /// never collapsed into an unchanged context line
    /// </summary>
    /// <param name="beforeLines">Lines before the fix</param>
    /// <param name="afterLines">Lines after the fix</param>
    /// <returns>The prefixed lines, in output order</returns>
    private static List<(char Kind, (string Text, bool IsTerminated) Line)> Compare(List<(string Text, bool IsTerminated)> beforeLines,
                                                                                    List<(string Text, bool IsTerminated)> afterLines)
    {
        var commonLength = ComputeCommonSubsequenceLengths(beforeLines, afterLines);
        var result = new List<(char Kind, (string Text, bool IsTerminated) Line)>();
        var beforeIndex = 0;
        var afterIndex = 0;

        while (beforeIndex < beforeLines.Count && afterIndex < afterLines.Count)
        {
            if (LinesEqual(beforeLines[beforeIndex], afterLines[afterIndex]))
            {
                result.Add((' ', beforeLines[beforeIndex]));
                beforeIndex++;
                afterIndex++;
            }
            else if (commonLength[beforeIndex + 1, afterIndex] >= commonLength[beforeIndex, afterIndex + 1])
            {
                result.Add(('-', beforeLines[beforeIndex]));
                beforeIndex++;
            }
            else
            {
                result.Add(('+', afterLines[afterIndex]));
                afterIndex++;
            }
        }

        while (beforeIndex < beforeLines.Count)
        {
            result.Add(('-', beforeLines[beforeIndex]));
            beforeIndex++;
        }

        while (afterIndex < afterLines.Count)
        {
            result.Add(('+', afterLines[afterIndex]));
            afterIndex++;
        }

        return result;
    }

    /// <summary>
    /// Computes the longest-common-subsequence length for every suffix pair of the two line sequences
    /// </summary>
    /// <param name="beforeLines">Lines before the fix</param>
    /// <param name="afterLines">Lines after the fix</param>
    /// <returns>A table whose entry [i, j] is the common length of the suffixes starting at i and j</returns>
    private static int[,] ComputeCommonSubsequenceLengths(List<(string Text, bool IsTerminated)> beforeLines, List<(string Text, bool IsTerminated)> afterLines)
    {
        var lengths = new int[beforeLines.Count + 1, afterLines.Count + 1];

        for (var beforeIndex = beforeLines.Count - 1; beforeIndex >= 0; beforeIndex--)
        {
            for (var afterIndex = afterLines.Count - 1; afterIndex >= 0; afterIndex--)
            {
                lengths[beforeIndex, afterIndex] = LinesEqual(beforeLines[beforeIndex], afterLines[afterIndex])
                                                       ? lengths[beforeIndex + 1, afterIndex + 1] + 1
                                                       : Math.Max(lengths[beforeIndex + 1, afterIndex], lengths[beforeIndex, afterIndex + 1]);
            }
        }

        return lengths;
    }

    /// <summary>
    /// Determines whether two diff lines are equal, which requires both their text and their termination state to
    /// match
    /// </summary>
    /// <param name="first">First line</param>
    /// <param name="second">Second line</param>
    /// <returns><see langword="true"/> when the lines are equal; otherwise, <see langword="false"/></returns>
    private static bool LinesEqual((string Text, bool IsTerminated) first, (string Text, bool IsTerminated) second)
    {
        return first.IsTerminated == second.IsTerminated && string.Equals(first.Text, second.Text, StringComparison.Ordinal);
    }

    #endregion // Methods
}