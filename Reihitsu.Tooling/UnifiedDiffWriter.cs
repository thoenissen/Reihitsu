using System.Text;

namespace Reihitsu.Tooling;

/// <summary>
/// Renders the difference between a fixture's input and the source the code fix produced from it
/// </summary>
public static class UnifiedDiffWriter
{
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

        var beforeLines = FixtureLineEndings.SplitLines(before);
        var afterLines = FixtureLineEndings.SplitLines(after);
        var builder = new StringBuilder();

        builder.Append("--- a/").Append(path).Append('\n');
        builder.Append("+++ b/").Append(path).Append('\n');
        builder.Append("@@ -1,").Append(beforeLines.Count).Append(" +1,").Append(afterLines.Count).Append(" @@").Append('\n');

        foreach (var (kind, line) in Compare(beforeLines, afterLines))
        {
            builder.Append(kind).Append(line).Append('\n');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Compares two line sequences and yields each line with its unified-diff prefix
    /// </summary>
    /// <param name="beforeLines">Lines before the fix</param>
    /// <param name="afterLines">Lines after the fix</param>
    /// <returns>The prefixed lines, in output order</returns>
    private static List<(char Kind, string Line)> Compare(List<string> beforeLines, List<string> afterLines)
    {
        var commonLength = ComputeCommonSubsequenceLengths(beforeLines, afterLines);
        var result = new List<(char Kind, string Line)>();
        var beforeIndex = 0;
        var afterIndex = 0;

        while (beforeIndex < beforeLines.Count && afterIndex < afterLines.Count)
        {
            if (string.Equals(beforeLines[beforeIndex], afterLines[afterIndex], StringComparison.Ordinal))
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
    private static int[,] ComputeCommonSubsequenceLengths(List<string> beforeLines, List<string> afterLines)
    {
        var lengths = new int[beforeLines.Count + 1, afterLines.Count + 1];

        for (var beforeIndex = beforeLines.Count - 1; beforeIndex >= 0; beforeIndex--)
        {
            for (var afterIndex = afterLines.Count - 1; afterIndex >= 0; afterIndex--)
            {
                lengths[beforeIndex, afterIndex] = string.Equals(beforeLines[beforeIndex], afterLines[afterIndex], StringComparison.Ordinal)
                                                       ? lengths[beforeIndex + 1, afterIndex + 1] + 1
                                                       : Math.Max(lengths[beforeIndex + 1, afterIndex], lengths[beforeIndex, afterIndex + 1]);
            }
        }

        return lengths;
    }

    #endregion // Methods
}