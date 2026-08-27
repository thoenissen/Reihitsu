using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Verifies analyzer package metadata matches the shipped analyzers and code fixes
/// </summary>
[TestClass]
public class AnalyzerPackageMetadataTests
{
    #region Fields

    /// <summary>
    /// Diagnostic IDs whose message format is intentionally more specific than their title - naming a concrete
    /// remediation step, XML tag, or disambiguating detail the title omits - once markdown code spans are
    /// ignored, so equalizing it with the title would remove information or force ungrammatical wording
    /// rather than removing redundancy
    /// </summary>
    private static readonly HashSet<string> _messageFormatIntentionallyDiffersFromTitle = new(StringComparer.Ordinal)
                                                                                          {
                                                                                              // Message states a concrete remediation step or configuration pointer the title omits
                                                                                              "RH0002",
                                                                                              "RH2109",
                                                                                              "RH4009",
                                                                                              "RH7004",
                                                                                              "RH7501",

                                                                                              // Message disambiguates an order or scope the title leaves ambiguous or under-specifies
                                                                                              "RH5111",
                                                                                              "RH5113",
                                                                                              "RH5205",
                                                                                              "RH7110",
                                                                                              "RH7205",
                                                                                              "RH7207",

                                                                                              // Message names the concrete XML documentation tag the title only describes generically
                                                                                              "RH8030",
                                                                                              "RH8031",
                                                                                              "RH8032",
                                                                                              "RH8033",
                                                                                              "RH8101",
                                                                                              "RH8102",
                                                                                              "RH8103",
                                                                                              "RH8104",
                                                                                              "RH8105",
                                                                                              "RH8106",
                                                                                              "RH8107",
                                                                                              "RH8108",
                                                                                              "RH8109",
                                                                                              "RH8110",
                                                                                              "RH8111",
                                                                                              "RH8204",

                                                                                              // Message cannot be derived by stripping the title's markdown code span - the span sits mid-sentence,
                                                                                              // so stripping it breaks the grammar instead of just removing formatting
                                                                                              "RH3101",

                                                                                              // Message states a broader scope (also covers interface implementation) that the title's "inheriting
                                                                                              // class" wording omits
                                                                                              "RH8203",

                                                                                              // Message covers a second, independently checked modifier pair the title's single example does not name
                                                                                              "RH7106",

                                                                                              // Message names the actual, narrower element kind the analyzer checks; the title matches the rule
                                                                                              // document's own heading, which is outside the scope of this test to rewrite
                                                                                              "RH7109"
                                                                                          };

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Removes markdown code-span backticks and a trailing sentence period, so a title's markdown and a message
    /// format's plain runtime text can be compared for the same underlying wording
    /// </summary>
    /// <param name="value">Resource text to normalize</param>
    /// <returns>The value without backticks or a trailing period</returns>
    private static string IgnoreMarkdownCodeSpans(string value)
    {
        return value.Replace("`", string.Empty, StringComparison.Ordinal).TrimEnd('.');
    }

    #endregion // Methods

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
    /// Verifies every analyzer's title resource matches its rule documentation title
    /// </summary>
    [TestMethod]
    public void AnalyzerTitleResourcesMatchRuleDocumentationTitles()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var documentedRules = AnalyzerMetadataDiscovery.ParseRuleDocumentationTitles();

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          TitleResource = AnalyzerResources.ResourceManager.GetString($"{analyzer.DiagnosticId}Title"),
                                                          DocumentedRule = documentedRules.SingleOrDefault(rule => rule.DiagnosticId == analyzer.DiagnosticId)
                                                      })
                                  .Where(entry => entry.TitleResource == null
                                                  || entry.DocumentedRule == null
                                                  || string.Equals(AnalyzerMetadataDiscovery.NormalizeRuleTitle(entry.TitleResource),
                                                                   AnalyzerMetadataDiscovery.NormalizeRuleTitle(entry.DocumentedRule.Title),
                                                                   StringComparison.OrdinalIgnoreCase) is false)
                                  .Select(entry => entry.TitleResource == null
                                                       ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) has no '{entry.Analyzer.DiagnosticId}Title' resource string."
                                                       : entry.DocumentedRule == null
                                                           ? $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) is missing rule documentation."
                                                           : $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) title resource '{entry.TitleResource}' does not match rule title '{entry.DocumentedRule.Title}'.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"Every analyzer title resource must match its rule documentation title.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
    }

    /// <summary>
    /// Verifies every analyzer's message format resource matches its title resource when the message format
    /// contains no placeholders, except for the documented cases where the divergence is intentional
    /// </summary>
    [TestMethod]
    public void MessageFormatResourcesMatchTitleWhenNoPlaceholders()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();

        var mismatches = analyzers.Select(analyzer => new
                                                      {
                                                          Analyzer = analyzer,
                                                          TitleResource = AnalyzerResources.ResourceManager.GetString($"{analyzer.DiagnosticId}Title"),
                                                          MessageFormatResource = AnalyzerResources.ResourceManager.GetString($"{analyzer.DiagnosticId}MessageFormat")
                                                      })
                                  .Where(entry => entry.TitleResource != null
                                                  && entry.MessageFormatResource != null
                                                  && AnalyzerMetadataDiscovery.ContainsPlaceholder(entry.MessageFormatResource) is false
                                                  && _messageFormatIntentionallyDiffersFromTitle.Contains(entry.Analyzer.DiagnosticId) is false
                                                  && string.Equals(IgnoreMarkdownCodeSpans(entry.MessageFormatResource),
                                                                   IgnoreMarkdownCodeSpans(entry.TitleResource),
                                                                   StringComparison.Ordinal) is false)
                                  .Select(entry => $"{entry.Analyzer.DiagnosticId} ({entry.Analyzer.AnalyzerType.Name}) message format '{entry.MessageFormatResource}' does not match title '{entry.TitleResource}'.")
                                  .ToArray();

        Assert.IsEmpty(mismatches, $"Every analyzer message format without placeholders must match its title resource, unless listed in {nameof(_messageFormatIntentionallyDiffersFromTitle)}.{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
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