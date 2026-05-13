using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Resolves configured copyright header templates
/// </summary>
internal static class CopyrightHeaderTemplateResolver
{
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

        return Regex.Matches(template, @"\{(?<name>[A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))
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

    #endregion // Methods
}