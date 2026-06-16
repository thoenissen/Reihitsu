using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Resolves configured copyright header templates
/// </summary>
internal static class CopyrightHeaderTemplateResolver
{
    #region Fields

    /// <summary>
    /// Placeholder pattern
    /// </summary>
    private static readonly Regex _placeholderPattern = new(@"\{(?<name>[A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets all placeholders used in a template
    /// </summary>
    /// <param name="template">Template text</param>
    /// <returns>Placeholder names without curly braces</returns>
    public static IEnumerable<string> GetPlaceholders(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return [];
        }

        return _placeholderPattern.Matches(template)
                                  .Cast<Match>()
                                  .Select(match => match.Groups["name"].Value)
                                  .Distinct(StringComparer.Ordinal);
    }

    /// <summary>
    /// Resolves the configured template for a given file
    /// </summary>
    /// <param name="configuration">Copyright configuration</param>
    /// <param name="filePath">File path</param>
    /// <returns>Resolved header text</returns>
    public static string ResolveHeader(ConfigurationCategoryCopyright configuration, string filePath)
    {
        if (configuration == null
            || string.IsNullOrWhiteSpace(configuration.CopyrightText))
        {
            return string.Empty;
        }

        var resolvedHeader = configuration.CopyrightText;

        foreach (var placeholder in configuration.Placeholders)
        {
            resolvedHeader = resolvedHeader.Replace($"{{{placeholder.Key}}}", placeholder.Value ?? string.Empty);
        }

        var fileName = Path.GetFileName(filePath) ?? string.Empty;

        return resolvedHeader.Replace($"{{{ConfigurationCategoryCopyright.FileNamePlaceholderName}}}", fileName);
    }

    /// <summary>
    /// Normalizes header line endings for a source file
    /// </summary>
    /// <param name="header">Header text</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>Header with normalized line endings</returns>
    public static string NormalizeLineEndings(string header, SourceText sourceText)
    {
        if (string.IsNullOrEmpty(header))
        {
            return string.Empty;
        }

        var preferredLineEnding = "\n";

        if (sourceText?.Lines.Count > 0)
        {
            foreach (var line in sourceText.Lines)
            {
                var lineBreakLength = line.SpanIncludingLineBreak.Length - line.Span.Length;

                if (lineBreakLength > 0)
                {
                    preferredLineEnding = sourceText.GetSubText(new TextSpan(line.End, lineBreakLength)).ToString();

                    break;
                }
            }
        }

        return header.Replace("\r\n", "\n")
                     .Replace('\r', '\n')
                     .Replace("\n", preferredLineEnding);
    }

    #endregion // Methods
}