using System.Text;

using Reihitsu.Cli.Diff;
using Reihitsu.Cli.Enumerations;

namespace Reihitsu.Cli;

/// <summary>
/// Generates a simple unified diff between two text contents
/// </summary>
internal static class DiffGenerator
{
    #region Constants

    /// <summary>
    /// The marker emitted after a line that is the last line of a file lacking a trailing newline
    /// </summary>
    private const string NoNewlineMarker = "\\ No newline at end of file";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Generates a unified diff between the original and formatted content
    /// </summary>
    /// <param name="filePath">The file path to display in the diff header</param>
    /// <param name="originalContent">The original file content</param>
    /// <param name="formattedContent">The formatted file content</param>
    /// <returns>A string containing the unified diff output</returns>
    public static string Generate(string filePath, string originalContent, string formattedContent)
    {
        var originalLines = ToDiffLines(originalContent);
        var formattedLines = ToDiffLines(formattedContent);
        var editScript = EditScriptBuilder.Build(originalLines, formattedLines);
        var hunks = HunkBuilder.Build(editScript, originalLines.Length, formattedLines.Length);

        if (hunks.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        builder.AppendLine($"--- a/{filePath}");
        builder.AppendLine($"+++ b/{filePath}");

        foreach (var hunk in hunks)
        {
            AppendHunk(builder, hunk, originalLines, formattedLines);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Splits content into diff lines, carrying the trailing-newline state alongside each line's own text
    /// </summary>
    /// <param name="content">The content to split</param>
    /// <returns>The diff lines, with the last line marked unterminated when the content has no trailing newline</returns>
    private static DiffLine[] ToDiffLines(string content)
    {
        var lines = LineSplitter.Split(content);
        var isTerminated = EndsWithLineBreak(content);

        if (lines.Length == 0)
        {
            return [];
        }

        if (isTerminated)
        {
            // A trailing line break produces an empty final element; drop it so a terminated file is not treated as
            // having an extra blank line.
            Array.Resize(ref lines, lines.Length - 1);
        }

        var diffLines = new DiffLine[lines.Length];

        for (var index = 0; index < lines.Length; index++)
        {
            diffLines[index] = new DiffLine(lines[index], index < lines.Length - 1 || isTerminated);
        }

        return diffLines;
    }

    /// <summary>
    /// Appends a formatted hunk to the string builder
    /// </summary>
    /// <param name="builder">The string builder to append to</param>
    /// <param name="hunk">The diff hunk to render</param>
    /// <param name="originalLines">The original lines array</param>
    /// <param name="formattedLines">The formatted lines array</param>
    private static void AppendHunk(StringBuilder builder, DiffHunk hunk, DiffLine[] originalLines, DiffLine[] formattedLines)
    {
        builder.AppendLine($"@@ -{FormatRange(hunk.OriginalStart, hunk.OriginalCount)} +{FormatRange(hunk.FormattedStart, hunk.FormattedCount)} @@");

        foreach (var operation in hunk.Operations)
        {
            switch (operation.Kind)
            {
                case EditKind.Equal:
                    {
                        AppendLine(builder, ' ', originalLines[operation.OriginalIndex]);
                    }
                    break;

                case EditKind.Delete:
                    {
                        AppendLine(builder, '-', originalLines[operation.OriginalIndex]);
                    }
                    break;

                case EditKind.Insert:
                    {
                        AppendLine(builder, '+', formattedLines[operation.FormattedIndex]);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Appends a single diff line, emitting the no-newline marker when the line is unterminated
    /// </summary>
    /// <param name="builder">The string builder to append to</param>
    /// <param name="prefix">The unified-diff line prefix (space, '-' or '+')</param>
    /// <param name="line">The diff line to render</param>
    private static void AppendLine(StringBuilder builder, char prefix, DiffLine line)
    {
        builder.AppendLine($"{prefix}{line.Text}");

        if (line.IsTerminated == false)
        {
            builder.AppendLine(NoNewlineMarker);
        }
    }

    /// <summary>
    /// Formats a unified-diff range, following the convention that a zero-length range is numbered by the line before it
    /// </summary>
    /// <param name="start">The zero-based start line index of the range</param>
    /// <param name="count">The number of lines in the range</param>
    /// <returns>The formatted "start,count" range</returns>
    private static string FormatRange(int start, int count)
    {
        // For an empty range, unified diff uses the number of the line that precedes the location (0 when at the
        // very beginning); otherwise the range is 1-based.
        return count == 0 ? $"{start},0" : $"{start + 1},{count}";
    }

    /// <summary>
    /// Determines whether the specified content ends with a line break
    /// </summary>
    /// <param name="content">The content to inspect</param>
    /// <returns><see langword="true"/> if the content ends with a line break; otherwise, <see langword="false"/></returns>
    private static bool EndsWithLineBreak(string content)
    {
        return content.Length > 0 && (content[^1] == '\n' || content[^1] == '\r');
    }

    #endregion // Methods
}