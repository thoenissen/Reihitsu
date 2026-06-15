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
        var originalLines = LineSplitter.Split(originalContent);
        var formattedLines = LineSplitter.Split(formattedContent);
        var editScript = EditScriptBuilder.Build(originalLines, formattedLines);
        var hunks = HunkBuilder.Build(editScript, originalLines.Length, formattedLines.Length);

        if (hunks.Count == 0)
        {
            return string.Empty;
        }

        var originalEndsWithNewline = EndsWithLineBreak(originalContent);
        var formattedEndsWithNewline = EndsWithLineBreak(formattedContent);

        var builder = new StringBuilder();

        builder.AppendLine($"--- a/{filePath}");
        builder.AppendLine($"+++ b/{filePath}");

        foreach (var hunk in hunks)
        {
            AppendHunk(builder, hunk, originalLines, formattedLines, originalEndsWithNewline, formattedEndsWithNewline);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Appends a formatted hunk to the string builder
    /// </summary>
    /// <param name="builder">The string builder to append to</param>
    /// <param name="hunk">The diff hunk to render</param>
    /// <param name="originalLines">The original lines array</param>
    /// <param name="formattedLines">The formatted lines array</param>
    /// <param name="originalEndsWithNewline">Whether the original content ends with a line break</param>
    /// <param name="formattedEndsWithNewline">Whether the formatted content ends with a line break</param>
    private static void AppendHunk(StringBuilder builder, DiffHunk hunk, string[] originalLines, string[] formattedLines, bool originalEndsWithNewline, bool formattedEndsWithNewline)
    {
        builder.AppendLine($"@@ -{FormatRange(hunk.OriginalStart, hunk.OriginalCount)} +{FormatRange(hunk.FormattedStart, hunk.FormattedCount)} @@");

        foreach (var operation in hunk.Operations)
        {
            switch (operation.Kind)
            {
                case EditKind.Equal:
                    {
                        builder.AppendLine($" {originalLines[operation.OriginalIndex]}");

                        if ((IsLastLine(operation.OriginalIndex, originalLines) && originalEndsWithNewline == false)
                            || (IsLastLine(operation.FormattedIndex, formattedLines) && formattedEndsWithNewline == false))
                        {
                            builder.AppendLine(NoNewlineMarker);
                        }
                    }
                    break;

                case EditKind.Delete:
                    {
                        builder.AppendLine($"-{originalLines[operation.OriginalIndex]}");

                        if (IsLastLine(operation.OriginalIndex, originalLines) && originalEndsWithNewline == false)
                        {
                            builder.AppendLine(NoNewlineMarker);
                        }
                    }
                    break;

                case EditKind.Insert:
                    {
                        builder.AppendLine($"+{formattedLines[operation.FormattedIndex]}");

                        if (IsLastLine(operation.FormattedIndex, formattedLines) && formattedEndsWithNewline == false)
                        {
                            builder.AppendLine(NoNewlineMarker);
                        }
                    }
                    break;
            }
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
    /// Determines whether the specified line index is the last line of the given lines array
    /// </summary>
    /// <param name="lineIndex">The line index to check</param>
    /// <param name="lines">The lines array</param>
    /// <returns><see langword="true"/> if the index is the last line; otherwise, <see langword="false"/></returns>
    private static bool IsLastLine(int lineIndex, string[] lines)
    {
        return lineIndex >= 0 && lineIndex == lines.Length - 1;
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