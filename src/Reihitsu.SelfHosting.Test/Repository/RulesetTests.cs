using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Repository;

/// <summary>
/// Verifies the Reihitsu entries of the rulesets the repository's own build uses
/// </summary>
[TestClass]
public sealed class RulesetTests
{
    #region Constants

    /// <summary>
    /// Ruleset of the Debug configuration
    /// </summary>
    private const string DebugRuleset = "Debug.ruleset";

    /// <summary>
    /// Ruleset of the Release configuration
    /// </summary>
    private const string ReleaseRuleset = "Release.ruleset";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that every Reihitsu entry of a ruleset names a rule the analyzer assembly ships, so no entry is
    /// silently ignored
    /// </summary>
    /// <param name="rulesetName">File name of the ruleset</param>
    [TestMethod]
    [DataRow(DebugRuleset)]
    [DataRow(ReleaseRuleset)]
    public void EveryReihitsuEntryNamesAShippedRule(string rulesetName)
    {
        // Arrange
        var shippedRuleIds = AnalyzerCatalog.DiscoverAnalyzerTypes()
                                            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                                            .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                                            .Select(descriptor => descriptor.Id)
                                            .ToHashSet(StringComparer.Ordinal);

        // Act
        var unknownRuleIds = LoadReihitsuEntries(rulesetName).Keys
                                                             .Where(ruleId => shippedRuleIds.Contains(ruleId) == false)
                                                             .Order(StringComparer.Ordinal)
                                                             .ToList();

        // Assert
        Assert.IsGreaterThan(0, shippedRuleIds.Count, "No shipped rule was found.");
        Assert.IsEmpty(unknownRuleIds, $"{rulesetName} names rules that no analyzer ships: {string.Join(", ", unknownRuleIds)}");
    }

    /// <summary>
    /// Verifies that the Release ruleset turns the line-count rules RH2008 and RH2111 on as errors, like every other
    /// rule it enables
    /// </summary>
    /// <param name="ruleId">Rule ID</param>
    [TestMethod]
    [DataRow("RH2008")]
    [DataRow("RH2111")]
    public void ReleaseRulesetReportsLineCountRuleAsError(string ruleId)
    {
        // Act
        var entries = LoadReihitsuEntries(ReleaseRuleset);

        // Assert
        Assert.AreEqual("Error", entries.GetValueOrDefault(ruleId), $"Action of {ruleId} in {ReleaseRuleset}");
    }

    /// <summary>
    /// Verifies that the Debug ruleset, which lists Reihitsu rules only to switch them off, leaves the line-count rules
    /// RH2008 and RH2111 on
    /// </summary>
    /// <param name="ruleId">Rule ID</param>
    [TestMethod]
    [DataRow("RH2008")]
    [DataRow("RH2111")]
    public void DebugRulesetHasNoEntryForLineCountRule(string ruleId)
    {
        // Act
        var entries = LoadReihitsuEntries(DebugRuleset);

        // Assert
        Assert.IsFalse(entries.ContainsKey(ruleId), $"{DebugRuleset} lists {ruleId} with the action {entries.GetValueOrDefault(ruleId)}.");
    }

    /// <summary>
    /// Verifies that both rulesets switch RH8402 off the same way
    /// </summary>
    /// <param name="rulesetName">File name of the ruleset</param>
    [TestMethod]
    [DataRow(DebugRuleset)]
    [DataRow(ReleaseRuleset)]
    public void RulesetSwitchesOffRH8402(string rulesetName)
    {
        // Act
        var entries = LoadReihitsuEntries(rulesetName);

        // Assert
        Assert.AreEqual("None", entries.GetValueOrDefault("RH8402"), $"Action of RH8402 in {rulesetName}");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Loads the entries a ruleset at the repository root lists for the Reihitsu analyzer
    /// </summary>
    /// <param name="rulesetName">File name of the ruleset</param>
    /// <returns>The action of each listed rule, by rule ID</returns>
    private static Dictionary<string, string> LoadReihitsuEntries(string rulesetName)
    {
        var document = XDocument.Load(Path.Combine(RepositoryRoot.Find(), rulesetName));

        return document.Descendants("Rules")
                       .Where(rules => string.Equals(rules.Attribute("AnalyzerId")?.Value, "Reihitsu.Analyzer", StringComparison.Ordinal))
                       .Elements("Rule")
                       .ToDictionary(rule => rule.Attribute("Id")?.Value ?? string.Empty,
                                     rule => rule.Attribute("Action")?.Value ?? string.Empty,
                                     StringComparer.Ordinal);
    }

    #endregion // Methods
}