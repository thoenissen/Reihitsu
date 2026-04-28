using System.Text;

using Reihitsu.Cli.Diff;

namespace Reihitsu.Cli;

/// <summary>
/// Generates a simple unified diff between two text contents
/// </summary>
internal static class DiffGenerator
{
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
    /// Appends a formatted hunk to the string builder
    /// </summary>
    /// <param name="builder">The string builder to append to</param>
    /// <param name="hunk">The diff hunk to render</param>
    /// <param name="originalLines">The original lines array</param>
    /// <param name="formattedLines">The formatted lines array</param>
    private static void AppendHunk(StringBuilder builder, DiffHunk hunk, string[] originalLines, string[] formattedLines)
    {
        builder.AppendLine($"@@ -{hunk.OriginalStart + 1},{hunk.OriginalCount} +{hunk.FormattedStart + 1},{hunk.FormattedCount} @@");

        foreach (var operation in hunk.Operations)
        {
            switch (operation.Kind)
            {
                case EditKind.Equal:
                    builder.AppendLine($" {originalLines[operation.OriginalIndex]}");
                    break;

                case EditKind.Delete:
                    builder.AppendLine($"-{originalLines[operation.OriginalIndex]}");
                    break;

                case EditKind.Insert:
                    builder.AppendLine($"+{formattedLines[operation.FormattedIndex]}");
                    break;
            }
        }
    }

    #endregion // Methods
}