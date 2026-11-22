using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

namespace Reihitsu.Analyzer.Test.Resources;

/// <summary>
/// Tests for <see cref="AnalyzerResources"/> and the texts every analyzer publishes from it: an analyzer names no
/// resource itself, its title and its message are the resources <c>&lt;ID&gt;Title</c> and
/// <c>&lt;ID&gt;MessageFormat</c> of its rule
/// </summary>
[TestClass]
public class AnalyzerResourcesTests
{
    #region Tests

    /// <summary>
    /// Verifies that every shipped analyzer publishes a title and a message format that are not empty
    /// </summary>
    [TestMethod]
    public void EveryAnalyzerPublishesTitleAndMessageFormat()
    {
        // Act
        var analyzersWithoutText = AnalyzerMetadataDiscovery.DiscoverAnalyzers()
                                                            .Select(analyzer => CreateAnalyzer(analyzer.AnalyzerType))
                                                            .Where(analyzer => string.IsNullOrEmpty(analyzer.Title) || string.IsNullOrEmpty(analyzer.MessageFormat))
                                                            .Select(analyzer => analyzer.GetType().Name)
                                                            .ToList();

        // Assert
        Assert.IsGreaterThan(0, AnalyzerMetadataDiscovery.DiscoverAnalyzers().Count);
        Assert.IsEmpty(analyzersWithoutText, $"Analyzers without a title or a message format: {string.Join(", ", analyzersWithoutText)}");
    }

    /// <summary>
    /// Verifies that every shipped analyzer publishes the resources its diagnostic ID names, and reports its diagnostic
    /// with exactly those texts
    /// </summary>
    [TestMethod]
    public void EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames()
    {
        // Act
        var mismatches = AnalyzerMetadataDiscovery.DiscoverAnalyzers()
                                                  .Select(discovered => (discovered.DiagnosticId, Analyzer: CreateAnalyzer(discovered.AnalyzerType)))
                                                  .Where(entry => entry.Analyzer.Title != AnalyzerResources.ResourceManager.GetString($"{entry.DiagnosticId}Title")
                                                                  || entry.Analyzer.MessageFormat != AnalyzerResources.ResourceManager.GetString($"{entry.DiagnosticId}MessageFormat")
                                                                  || entry.Analyzer.SupportedDiagnostics.Single().Title.ToString() != entry.Analyzer.Title
                                                                  || entry.Analyzer.SupportedDiagnostics.Single().MessageFormat.ToString() != entry.Analyzer.MessageFormat)
                                                  .Select(entry => $"{entry.DiagnosticId} ({entry.Analyzer.GetType().Name})")
                                                  .ToList();

        // Assert
        Assert.IsEmpty(mismatches, $"Analyzers whose texts are not the resources of their diagnostic ID: {string.Join(", ", mismatches)}");
    }

    /// <summary>
    /// Verifies that every title and message resource of a rule belongs to a shipped analyzer, so no resource is left
    /// behind that no analyzer reads
    /// </summary>
    [TestMethod]
    public void EveryRuleResourceBelongsToAShippedAnalyzer()
    {
        // Arrange
        var diagnosticIds = AnalyzerMetadataDiscovery.DiscoverAnalyzers()
                                                     .Select(analyzer => analyzer.DiagnosticId)
                                                     .ToHashSet(StringComparer.Ordinal);
        var resourceSet = AnalyzerResources.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

        // Act
        var orphans = resourceSet.Cast<DictionaryEntry>()
                                 .Select(entry => (string)entry.Key)
                                 .Select(key => (Key: key, Match: Regex.Match(key, "^(?<id>RH[0-9]+[A-Z]?)(Title|MessageFormat)$", RegexOptions.CultureInvariant, Regex.InfiniteMatchTimeout)))
                                 .Where(entry => entry.Match.Success == false || diagnosticIds.Contains(entry.Match.Groups["id"].Value) == false)
                                 .Select(entry => entry.Key)
                                 .Order(StringComparer.Ordinal)
                                 .ToList();

        // Assert
        Assert.IsEmpty(orphans, $"Resources no shipped analyzer reads: {string.Join(", ", orphans)}");
    }

    /// <summary>
    /// Verifies that <see cref="AnalyzerResources"/> holds no property per rule: an analyzer finds its texts by its
    /// diagnostic ID, so the class does not grow with every rule
    /// </summary>
    [TestMethod]
    public void AnalyzerResourcesHoldsNoPropertyPerRule()
    {
        // Act
        var ruleProperties = typeof(AnalyzerResources).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                      .Select(property => property.Name)
                                                      .Where(name => name.StartsWith("RH", StringComparison.Ordinal))
                                                      .ToList();

        // Assert
        Assert.IsNotNull(AnalyzerResources.ResourceManager);
        Assert.IsEmpty(ruleProperties, $"Properties per rule: {ruleProperties.Count}");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates an analyzer of the given type
    /// </summary>
    /// <param name="analyzerType">Type of a shipped analyzer</param>
    /// <returns>The analyzer</returns>
    private static DiagnosticAnalyzerBase CreateAnalyzer(Type analyzerType)
    {
        return (DiagnosticAnalyzerBase)Activator.CreateInstance(analyzerType);
    }

    #endregion // Methods
}