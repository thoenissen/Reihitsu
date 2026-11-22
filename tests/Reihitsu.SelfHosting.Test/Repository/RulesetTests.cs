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

    #region Fields

    /// <summary>
    /// Shipped rules that deliberately have no entry in the Release ruleset, so the analyzer's own default applies to
    /// them: RH7305A is the alternative to RH7305, which the Release ruleset enables, and stays off as it is by default
    /// </summary>
    private static readonly string[] _rulesWithoutReleaseEntry = ["RH7305A"];

    #endregion // Fields

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
        var shippedRuleIds = GetShippedRuleIds();

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
    /// Verifies that the Release ruleset lists every rule the analyzer assembly ships, except the rules that
    /// deliberately stay out of it, so the action of every shipped rule in the repository's own Release build is
    /// written out explicitly and does not depend on the default severity or on warnings being treated as errors
    /// </summary>
    [TestMethod]
    public void ReleaseRulesetListsEveryShippedRule()
    {
        // Arrange
        var shippedRuleIds = GetShippedRuleIds();
        var entries = LoadReihitsuEntries(ReleaseRuleset);

        // Act
        var missingRuleIds = shippedRuleIds.Where(ruleId => entries.ContainsKey(ruleId) == false)
                                           .Where(ruleId => _rulesWithoutReleaseEntry.Contains(ruleId, StringComparer.Ordinal) == false)
                                           .Order(StringComparer.Ordinal)
                                           .ToList();

        // Assert
        Assert.IsGreaterThan(0, shippedRuleIds.Count, "No shipped rule was found.");
        Assert.IsEmpty(missingRuleIds, $"{ReleaseRuleset} has no entry for these shipped rules: {string.Join(", ", missingRuleIds)}");
    }

    /// <summary>
    /// Verifies that every rule exempted from the Release ruleset is a shipped rule the Release ruleset indeed does not
    /// list, so the exemption list cannot go stale
    /// </summary>
    [TestMethod]
    public void ReleaseRulesetExemptionsNameShippedRulesWithoutEntry()
    {
        // Arrange
        var shippedRuleIds = GetShippedRuleIds();
        var entries = LoadReihitsuEntries(ReleaseRuleset);

        // Act
        var staleRuleIds = _rulesWithoutReleaseEntry.Where(ruleId => shippedRuleIds.Contains(ruleId) == false
                                                                     || entries.ContainsKey(ruleId))
                                                    .Order(StringComparer.Ordinal)
                                                    .ToList();

        // Assert
        Assert.IsEmpty(staleRuleIds, $"These exemptions are not shipped rules without an entry in {ReleaseRuleset}: {string.Join(", ", staleRuleIds)}");
    }

    /// <summary>
    /// Verifies that the Release ruleset sets the action of the line-count rules RH2008 and RH2111, of the complexity
    /// rule RH2009, and of the shipped rules it once had no entry for to error, like every other rule it enables
    /// </summary>
    /// <param name="ruleId">Rule ID</param>
    [TestMethod]
    [DataRow("RH2008")]
    [DataRow("RH2009")]
    [DataRow("RH2111")]
    [DataRow("RH5113")]
    [DataRow("RH5308")]
    [DataRow("RH5417")]
    [DataRow("RH6023")]
    [DataRow("RH6024")]
    [DataRow("RH7110")]
    [DataRow("RH7308")]
    [DataRow("RH7309")]
    [DataRow("RH7310")]
    [DataRow("RH7311")]
    [DataRow("RH7409")]
    [DataRow("RH7410")]
    [DataRow("RH7411")]
    [DataRow("RH7412")]
    public void ReleaseRulesetReportsRuleAsError(string ruleId)
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
    /// Verifies that the Debug ruleset, which lists Reihitsu rules only to switch them off, leaves the complexity rule
    /// RH2009 on, so a Debug build reports it at its shipped severity
    /// </summary>
    [TestMethod]
    public void DebugRulesetHasNoEntryForComplexityRule()
    {
        // Act
        var entries = LoadReihitsuEntries(DebugRuleset);

        // Assert
        Assert.IsFalse(entries.ContainsKey("RH2009"), $"{DebugRuleset} lists RH2009 with the action {entries.GetValueOrDefault("RH2009")}.");
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
    /// Gets the IDs of every rule the analyzer assembly ships
    /// </summary>
    /// <returns>The shipped rule IDs</returns>
    private static HashSet<string> GetShippedRuleIds()
    {
        return AnalyzerCatalog.DiscoverAnalyzerTypes()
                              .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                              .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                              .Select(descriptor => descriptor.Id)
                              .ToHashSet(StringComparer.Ordinal);
    }

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