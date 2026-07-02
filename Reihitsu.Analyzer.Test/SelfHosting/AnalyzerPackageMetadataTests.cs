using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Verifies analyzer package metadata matches the shipped analyzers and code fixes
/// </summary>
[TestClass]
public class AnalyzerPackageMetadataTests
{
    #region Tests

    /// <summary>
    /// Verifies every analyzer appears exactly once in the analyzer package README
    /// </summary>
    [TestMethod]
    public void PackageReadmeListsEachAnalyzerExactlyOnce()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var packageRules = AnalyzerMetadataDiscovery.ParsePackageReadmeRules();

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          Count = packageRules.Count(rule => rule.DiagnosticId == analyzer.DiagnosticId)
                                                      })
                                  .Where(entry => entry.Count != 1)
                                  .Select(entry => $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) has {entry.Count} README rows.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"Every analyzer should appear exactly once in the analyzer package README.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
    }

    /// <summary>
    /// Verifies the analyzer package README does not contain rows for unknown analyzers
    /// </summary>
    [TestMethod]
    public void PackageReadmeContainsNoExtraAnalyzerRows()
    {
        var analyzerIds = AnalyzerMetadataDiscovery.DiscoverAnalyzers()
                                                   .Select(analyzer => analyzer.DiagnosticId)
                                                   .ToHashSet(StringComparer.Ordinal);
        var extraRuleIds = AnalyzerMetadataDiscovery.ParsePackageReadmeRules()
                                                    .Select(rule => rule.DiagnosticId)
                                                    .Except(analyzerIds, StringComparer.Ordinal)
                                                    .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                                    .ToArray();

        Assert.IsEmpty(extraRuleIds, $"The analyzer package README contains rows for unknown analyzers: {string.Join(", ", extraRuleIds)}");
    }

    /// <summary>
    /// Verifies the analyzer package README code-fix column matches the shipped code-fix providers
    /// </summary>
    [TestMethod]
    public void PackageReadmeCodeFixColumnMatchesDiscoveredCodeFixProviders()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var packageRules = AnalyzerMetadataDiscovery.ParsePackageReadmeRules();
        var codeFixIds = AnalyzerMetadataDiscovery.DiscoverCodeFixProviders()
                                                  .Select(codeFixProvider => codeFixProvider.DiagnosticId)
                                                  .ToHashSet(StringComparer.Ordinal);

        var unknownCodeFixIds = codeFixIds.Except(analyzers.Select(analyzer => analyzer.DiagnosticId), StringComparer.Ordinal)
                                          .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                          .ToArray();

        Assert.IsEmpty(unknownCodeFixIds, $"The analyzer package ships code-fix providers for unknown diagnostics: {string.Join(", ", unknownCodeFixIds)}");

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          Rule = packageRules.SingleOrDefault(rule => rule.DiagnosticId == analyzer.DiagnosticId),
                                                          HasCodeFix = codeFixIds.Contains(analyzer.DiagnosticId)
                                                      })
                                  .Where(entry => entry.Rule == null || entry.Rule.HasCodeFix != entry.HasCodeFix)
                                  .Select(entry => entry.Rule == null
                                                       ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) is missing from the analyzer package README."
                                                       : $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) README Code Fix={entry.Rule.HasCodeFix} but discovered Code Fix={entry.HasCodeFix}.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"The analyzer package README Code Fix column must match the shipped code-fix providers.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
    }

    /// <summary>
    /// Verifies the analyzer package README formatter column matches formatter test coverage
    /// </summary>
    [TestMethod]
    public void PackageReadmeFormatterColumnMatchesDiscoveredFormatterCoverage()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var packageRules = AnalyzerMetadataDiscovery.ParsePackageReadmeRules();
        var formatterIds = AnalyzerMetadataDiscovery.DiscoverFormatterCoveredDiagnosticIds()
                                                    .ToHashSet(StringComparer.Ordinal);

        var unknownFormatterIds = formatterIds.Except(analyzers.Select(analyzer => analyzer.DiagnosticId), StringComparer.Ordinal)
                                              .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                              .ToArray();

        Assert.IsEmpty(unknownFormatterIds, $"The analyzer test project contains formatter coverage for unknown diagnostics: {string.Join(", ", unknownFormatterIds)}");

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          Rule = packageRules.SingleOrDefault(rule => rule.DiagnosticId == analyzer.DiagnosticId),
                                                          HasFormatterCoverage = formatterIds.Contains(analyzer.DiagnosticId)
                                                      })
                                  .Where(entry => entry.Rule == null || entry.Rule.SupportsFormatter != entry.HasFormatterCoverage)
                                  .Select(entry => entry.Rule == null
                                                       ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) is missing from the analyzer package README."
                                                       : $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) README Formatter={entry.Rule.SupportsFormatter} but discovered formatter coverage={entry.HasFormatterCoverage}.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"The analyzer package README Formatter column must match formatter test coverage.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
    }

    /// <summary>
    /// Verifies the analyzer package README descriptions match the rule documentation titles
    /// </summary>
    [TestMethod]
    public void PackageReadmeDescriptionsMatchRuleDocumentationTitles()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var packageRules = AnalyzerMetadataDiscovery.ParsePackageReadmeRules();
        var documentedRules = AnalyzerMetadataDiscovery.ParseRuleDocumentationTitles();

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          PackageRule = packageRules.SingleOrDefault(rule => rule.DiagnosticId == analyzer.DiagnosticId),
                                                          DocumentedRule = documentedRules.SingleOrDefault(rule => rule.DiagnosticId == analyzer.DiagnosticId)
                                                      })
                                  .Where(entry => entry.PackageRule == null
                                                  || entry.DocumentedRule == null
                                                  || string.Equals(AnalyzerMetadataDiscovery.NormalizeRuleTitle(entry.PackageRule.Description),
                                                                   AnalyzerMetadataDiscovery.NormalizeRuleTitle(entry.DocumentedRule.Title),
                                                                   StringComparison.OrdinalIgnoreCase) is false)
                                  .Select(entry => entry.PackageRule == null
                                                       ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) is missing from the analyzer package README."
                                                       : entry.DocumentedRule == null
                                                           ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) is missing rule documentation."
                                                           : $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) README description '{entry.PackageRule.Description}' does not match rule title '{entry.DocumentedRule.Title}'.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"The analyzer package README descriptions must match the rule documentation titles.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
    }

    /// <summary>
    /// Verifies every analyzer has a matching analyzer unit-test class
    /// </summary>
    [TestMethod]
    public void EveryAnalyzerHasMatchingAnalyzerTestsClass()
    {
        var analyzerTestClassNames = AnalyzerMetadataDiscovery.DiscoverAnalyzerTestClasses()
                                                              .Select(type => type.Name)
                                                              .ToHashSet(StringComparer.Ordinal);
        var missingTestClasses = AnalyzerMetadataDiscovery.DiscoverAnalyzers()
                                                          .Select(analyzer => $"{analyzer.AnalyzerType.Name}Tests")
                                                          .Where(expectedTestClassName => analyzerTestClassNames.Contains(expectedTestClassName) == false)
                                                          .OrderBy(expectedTestClassName => expectedTestClassName, StringComparer.Ordinal)
                                                          .ToArray();

        Assert.IsEmpty(missingTestClasses, $"Missing analyzer test classes: {string.Join(", ", missingTestClasses)}");
    }

    #endregion // Tests
}