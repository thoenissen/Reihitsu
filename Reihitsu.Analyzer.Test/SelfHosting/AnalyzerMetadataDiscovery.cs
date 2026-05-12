using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Reflection-based discovery helpers for analyzer metadata validation
/// </summary>
internal static partial class AnalyzerMetadataDiscovery
{
    #region Members

    /// <summary>
    /// Regex for rule rows in the analyzer package README
    /// </summary>
    [GeneratedRegex(@"^\| \[(RH\d{4})\]\([^)]+\)\| (?<description>.*?)\| (?<analyzer>[✔❌])\| (?<codeFix>[✔❌])\| (?<formatter>[✔❌])\|$", RegexOptions.CultureInvariant, 100)]
    private static partial Regex PackageRuleRowRegex();

    /// <summary>
    /// Regex for rule document title headings
    /// </summary>
    [GeneratedRegex(@"^# (?<diagnosticId>RH\d{4}) [—-] (?<title>.+?)\s*$", RegexOptions.CultureInvariant, 100)]
    private static partial Regex RuleDocumentationTitleRegex();

    /// <summary>
    /// Regex for diagnostic IDs encoded in formatter test class names
    /// </summary>
    [GeneratedRegex(@"^(RH\d{4})", RegexOptions.CultureInvariant, 100)]
    private static partial Regex FormatterTestClassDiagnosticIdRegex();

    /// <summary>
    /// Discovers all shipped analyzers
    /// </summary>
    /// <returns>Discovered analyzers</returns>
    internal static IReadOnlyList<DiscoveredAnalyzer> DiscoverAnalyzers()
    {
        return typeof(DiagnosticAnalyzerBase<>).Assembly
                                               .GetTypes()
                                               .Where(type => type.IsAbstract is false
                                                              && type.IsInterface is false
                                                              && typeof(DiagnosticAnalyzer).IsAssignableFrom(type)
                                                              && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is not null)
                                               .Select(CreateAnalyzerMetadata)
                                               .OrderBy(analyzer => analyzer.DiagnosticId, StringComparer.Ordinal)
                                               .ToArray();
    }

    /// <summary>
    /// Discovers all shipped code-fix providers
    /// </summary>
    /// <returns>Discovered code-fix providers</returns>
    internal static IReadOnlyList<DiscoveredCodeFixProvider> DiscoverCodeFixProviders()
    {
        return typeof(StatementShouldBePrecededByABlankLineCodeFixProviderBase).Assembly
                                                                               .GetTypes()
                                                                               .Where(type => type.IsAbstract is false
                                                                                              && type.IsInterface is false
                                                                                              && typeof(CodeFixProvider).IsAssignableFrom(type)
                                                                                              && type.GetCustomAttribute<ExportCodeFixProviderAttribute>() is not null)
                                                                               .SelectMany(CreateCodeFixProviderMetadata)
                                                                               .OrderBy(codeFixProvider => codeFixProvider.DiagnosticId, StringComparer.Ordinal)
                                                                               .ThenBy(codeFixProvider => codeFixProvider.CodeFixProviderType.Name, StringComparer.Ordinal)
                                                                               .ToArray();
    }

    /// <summary>
    /// Discovers analyzer test classes in the test assembly
    /// </summary>
    /// <returns>Analyzer test classes</returns>
    internal static IReadOnlyList<Type> DiscoverAnalyzerTestClasses()
    {
        return typeof(SelfHostingTests).Assembly
                                       .GetTypes()
                                       .Where(type => type.IsAbstract is false
                                                      && type.IsClass
                                                      && type.Name.EndsWith("AnalyzerTests", StringComparison.Ordinal)
                                                      && type.GetCustomAttribute<TestClassAttribute>() is not null)
                                       .OrderBy(type => type.Name, StringComparer.Ordinal)
                                       .ToArray();
    }

    /// <summary>
    /// Discovers diagnostic IDs covered by formatter integration tests in the test assembly
    /// </summary>
    /// <returns>Diagnostic IDs covered by formatter tests</returns>
    internal static IReadOnlyList<string> DiscoverFormatterCoveredDiagnosticIds()
    {
        return typeof(SelfHostingTests).Assembly
                                       .GetTypes()
                                       .Where(type => type.IsAbstract is false
                                                      && type.IsClass
                                                      && type.Namespace?.StartsWith("Reihitsu.Analyzer.Test.Formatter", StringComparison.Ordinal) is true
                                                      && type.Name.EndsWith("FormatterTests", StringComparison.Ordinal)
                                                      && type.GetCustomAttribute<TestClassAttribute>() is not null)
                                       .Select(ParseDiagnosticIdFromFormatterTestClass)
                                       .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                       .ToArray();
    }

    /// <summary>
    /// Parses the analyzer package README rule metadata
    /// </summary>
    /// <returns>Parsed rule metadata</returns>
    internal static IReadOnlyList<PackageReadmeRuleMetadata> ParsePackageReadmeRules()
    {
        var packageReadmePath = Path.Combine(FindRepositoryRoot(), "Reihitsu.Analyzer.Package", "README.MD");

        return File.ReadLines(packageReadmePath)
                   .Select(line => PackageRuleRowRegex().Match(line))
                   .Where(match => match.Success)
                   .Select(match => new PackageReadmeRuleMetadata(match.Groups[1].Value,
                                                                  match.Groups["description"].Value.Trim(),
                                                                  ParseAvailability(match.Groups["analyzer"].Value),
                                                                  ParseAvailability(match.Groups["codeFix"].Value),
                                                                  ParseAvailability(match.Groups["formatter"].Value)))
                   .OrderBy(rule => rule.DiagnosticId, StringComparer.Ordinal)
                   .ToArray();
    }

    /// <summary>
    /// Parses rule documentation titles from documentation files
    /// </summary>
    /// <returns>Parsed rule documentation metadata</returns>
    internal static IReadOnlyList<RuleDocumentationMetadata> ParseRuleDocumentationTitles()
    {
        var ruleDocumentationDirectory = Path.Combine(FindRepositoryRoot(), "documentation", "rules");

        return Directory.EnumerateFiles(ruleDocumentationDirectory, "RH*.md", SearchOption.TopDirectoryOnly)
                        .Select(ParseRuleDocumentationTitle)
                        .OrderBy(rule => rule.DiagnosticId, StringComparer.Ordinal)
                        .ToArray();
    }

    /// <summary>
    /// Normalizes a human-readable rule title for metadata comparisons
    /// </summary>
    /// <param name="value">Title to normalize</param>
    /// <returns>Normalized title</returns>
    internal static string NormalizeRuleTitle(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedValue = value.Normalize(NormalizationForm.FormKC)
                                   .Replace(@"\<", "<", StringComparison.Ordinal)
                                   .Replace(@"\>", ">", StringComparison.Ordinal);

        normalizedValue = Regex.Replace(normalizedValue, @"<(?<name>[A-Za-z][A-Za-z0-9]*)\s*/>", "<${name}>", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        return Regex.Replace(normalizedValue, @"\s+", " ")
                    .Trim()
                    .TrimEnd('.');
    }

    /// <summary>
    /// Creates analyzer metadata from the reflected analyzer type
    /// </summary>
    /// <param name="analyzerType">Analyzer type</param>
    /// <returns>Analyzer metadata</returns>
    private static DiscoveredAnalyzer CreateAnalyzerMetadata(Type analyzerType)
    {
        var analyzer = Activator.CreateInstance(analyzerType) as DiagnosticAnalyzer
                           ?? throw new InvalidOperationException($"Failed to create analyzer '{analyzerType.FullName}'.");
        var supportedDiagnostics = analyzer.SupportedDiagnostics;

        if (supportedDiagnostics.Length != 1)
        {
            throw new InvalidOperationException($"Analyzer '{analyzerType.FullName}' should expose exactly one diagnostic descriptor.");
        }

        return new DiscoveredAnalyzer(analyzerType, supportedDiagnostics.Single().Id);
    }

    /// <summary>
    /// Creates code-fix metadata from the reflected code-fix provider type
    /// </summary>
    /// <param name="codeFixProviderType">Code-fix provider type</param>
    /// <returns>Code-fix metadata</returns>
    private static IEnumerable<DiscoveredCodeFixProvider> CreateCodeFixProviderMetadata(Type codeFixProviderType)
    {
        var codeFixProvider = Activator.CreateInstance(codeFixProviderType) as CodeFixProvider
                                  ?? throw new InvalidOperationException($"Failed to create code-fix provider '{codeFixProviderType.FullName}'.");

        return codeFixProvider.FixableDiagnosticIds
                              .Select(diagnosticId => new DiscoveredCodeFixProvider(codeFixProviderType, diagnosticId));
    }

    /// <summary>
    /// Parses the diagnostic ID from a formatter test class name
    /// </summary>
    /// <param name="formatterTestClass">Formatter test class</param>
    /// <returns>Diagnostic ID covered by the formatter test class</returns>
    private static string ParseDiagnosticIdFromFormatterTestClass(Type formatterTestClass)
    {
        var match = FormatterTestClassDiagnosticIdRegex().Match(formatterTestClass.Name);

        return match.Success
                   ? match.Groups[1].Value
                   : throw new InvalidOperationException($"Formatter test class '{formatterTestClass.FullName}' does not start with a diagnostic ID.");
    }

    /// <summary>
    /// Parses metadata from a single rule documentation file
    /// </summary>
    /// <param name="path">Rule documentation path</param>
    /// <returns>Parsed rule documentation metadata</returns>
    private static RuleDocumentationMetadata ParseRuleDocumentationTitle(string path)
    {
        var firstLine = File.ReadLines(path).FirstOrDefault()
                            ?? throw new InvalidOperationException($"Rule documentation '{path}' is empty.");
        var match = RuleDocumentationTitleRegex().Match(firstLine);

        if (match.Success is false)
        {
            throw new InvalidOperationException($"Rule documentation '{path}' must start with a '# RH#### — Title' heading.");
        }

        return new RuleDocumentationMetadata(match.Groups["diagnosticId"].Value, match.Groups["title"].Value.Trim());
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

    /// <summary>
    /// Parses a README availability marker
    /// </summary>
    /// <param name="value">Marker value</param>
    /// <returns><see langword="true"/> when the capability is available; otherwise <see langword="false"/></returns>
    private static bool ParseAvailability(string value)
    {
        return value switch
               {
                   "✔" => true,
                   "❌" => false,
                   _ => throw new InvalidOperationException($"Unknown availability marker '{value}'.")
               };
    }

    #endregion // Members
}