using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Discovers and parses the <c>### Violation</c> / <c>### Correction</c> examples embedded in
/// <c>documentation/rules/</c> pages
/// </summary>
internal static class RuleDocumentationExampleDiscovery
{
    #region Fields

    /// <summary>
    /// Regex for rule document title headings
    /// </summary>
    private static readonly Regex _titleRegex = new(@"^# (?<diagnosticId>RH\d{4}[A-Z]?) [—-] ", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Regex for the metadata table's <c>Code Fix</c> row
    /// </summary>
    private static readonly Regex _codeFixRowRegex = new(@"^\|\s*\*\*Code Fix\*\*\s*\|\s*(?<marker>✓|❌)\s*\|$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Discovers every rule document's violation/correction example pair
    /// </summary>
    /// <returns>Discovered examples, ordered by diagnostic ID</returns>
    internal static IReadOnlyList<RuleDocumentationExample> DiscoverExamples()
    {
        var ruleDocumentationDirectory = Path.Combine(FindRepositoryRoot(), "documentation", "rules");

        return Directory.EnumerateFiles(ruleDocumentationDirectory, "RH*.md", SearchOption.TopDirectoryOnly)
                        .Select(ParseExample)
                        .OrderBy(example => example.DiagnosticId, StringComparer.Ordinal)
                        .ToArray();
    }

    /// <summary>
    /// Parses a single rule document's violation/correction example pair
    /// </summary>
    /// <param name="path">Rule document path</param>
    /// <returns>The parsed example</returns>
    private static RuleDocumentationExample ParseExample(string path)
    {
        var lines = File.ReadAllLines(path);
        var diagnosticId = ParseDiagnosticId(path, lines);
        var hasCodeFix = ParseHasCodeFix(path, lines);

        if (TryParseFirstFencedBlock(lines, "### Violation", out var violation) == false)
        {
            throw new InvalidOperationException($"Rule document '{path}' has no fenced code block under '### Violation'.");
        }

        if (TryParseFirstFencedBlock(lines, "### Correction", out var correction) == false)
        {
            throw new InvalidOperationException($"Rule document '{path}' has no fenced code block under '### Correction'.");
        }

        return new RuleDocumentationExample(path, diagnosticId, hasCodeFix, violation, correction);
    }

    /// <summary>
    /// Parses the diagnostic ID from the document's title heading
    /// </summary>
    /// <param name="path">Rule document path</param>
    /// <param name="lines">Document lines</param>
    /// <returns>The diagnostic ID</returns>
    private static string ParseDiagnosticId(string path, string[] lines)
    {
        var titleLine = lines.FirstOrDefault(line => _titleRegex.IsMatch(line))
                            ?? throw new InvalidOperationException($"Rule document '{path}' has no '# RH#### — Title' heading.");

        return _titleRegex.Match(titleLine).Groups["diagnosticId"].Value;
    }

    /// <summary>
    /// Parses whether the document's metadata table advertises a code fix
    /// </summary>
    /// <param name="path">Rule document path</param>
    /// <param name="lines">Document lines</param>
    /// <returns><see langword="true"/> when the metadata table's Code Fix row is <c>✓</c>; otherwise, <see langword="false"/></returns>
    private static bool ParseHasCodeFix(string path, string[] lines)
    {
        foreach (var line in lines)
        {
            var match = _codeFixRowRegex.Match(line);

            if (match.Success)
            {
                return match.Groups["marker"].Value == "✓";
            }
        }

        throw new InvalidOperationException($"Rule document '{path}' has no '**Code Fix**' metadata row.");
    }

    /// <summary>
    /// Parses the first fenced code block that appears under the given heading, before the next heading
    /// </summary>
    /// <param name="lines">Document lines</param>
    /// <param name="heading">Heading line to search after</param>
    /// <param name="block">The fenced block content, when found</param>
    /// <returns><see langword="true"/> when a fenced block was found; otherwise, <see langword="false"/></returns>
    private static bool TryParseFirstFencedBlock(string[] lines, string heading, out string block)
    {
        block = string.Empty;

        var headingIndex = Array.FindIndex(lines, line => string.Equals(line.Trim(), heading, StringComparison.Ordinal));

        if (headingIndex < 0)
        {
            return false;
        }

        var fenceStart = -1;

        for (var index = headingIndex + 1; index < lines.Length; index++)
        {
            var line = lines[index];

            if (line.StartsWith("##", StringComparison.Ordinal))
            {
                return false;
            }

            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                fenceStart = index;

                break;
            }
        }

        if (fenceStart < 0)
        {
            return false;
        }

        var fenceEnd = -1;

        for (var index = fenceStart + 1; index < lines.Length; index++)
        {
            if (lines[index].StartsWith("```", StringComparison.Ordinal))
            {
                fenceEnd = index;

                break;
            }
        }

        if (fenceEnd < 0)
        {
            return false;
        }

        block = string.Join("\n", lines[(fenceStart + 1)..fenceEnd]);

        return true;
    }

    /// <summary>
    /// Finds the repository root
    /// </summary>
    /// <returns>Repository root path</returns>
    private static string FindRepositoryRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory != null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "Reihitsu.sln")))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    #endregion // Methods
}