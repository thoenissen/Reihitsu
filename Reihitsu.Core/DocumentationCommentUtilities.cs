using System;

using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides shared helpers for working with XML documentation comment lines
/// </summary>
public static class DocumentationCommentUtilities
{
    #region Constants

    /// <summary>
    /// The exterior marker that introduces a single-line documentation comment
    /// </summary>
    public const string DocumentationExterior = "///";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Determines whether the text consists of whitespace and documentation comment exterior markers only.
    /// Both the <c>///</c> exterior of a single-line comment and the <c>*</c> continuation exterior of a
    /// <c>/** … */</c> comment are recognized. The <c>/**</c> opener is deliberately not an exterior, so
    /// callers never treat the line that starts the comment as content-free
    /// </summary>
    /// <param name="text">Text</param>
    /// <returns><see langword="true"/> if the text carries no documentation content</returns>
    public static bool IsExteriorOnly(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return true;
        }

        var isSlashOnly = true;
        var isAsteriskOnly = true;

        foreach (var character in trimmed)
        {
            isSlashOnly &= character == '/';
            isAsteriskOnly &= character == '*';
        }

        return isSlashOnly || isAsteriskOnly;
    }

    /// <summary>
    /// Gets the continuation prefix for the specified documentation comment line. The prefix consists of
    /// the leading indentation, the <c>///</c> exterior marker and a single trailing space. Any sentence
    /// text that follows the exterior on the line is intentionally excluded so that rebuilt continuation
    /// lines never duplicate it
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>The continuation prefix, or an empty string when the line has no documentation exterior</returns>
    public static string GetContinuationPrefix(SourceText sourceText, TextLine line)
    {
        var lineText = sourceText.ToString(line.Span);
        var exteriorIndex = lineText.IndexOf(DocumentationExterior, StringComparison.Ordinal);

        if (exteriorIndex < 0)
        {
            return string.Empty;
        }

        return string.Concat(lineText.Substring(0, exteriorIndex + DocumentationExterior.Length), " ");
    }

    #endregion // Methods
}