using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Testing solution policies
/// </summary>
[TestClass]
public class AnalyzerRulePolicyTests
{
    #region Tests

    /// <summary>
    /// Validating the namespace of the analyzer
    /// </summary>
    [TestMethod]
    public void ValidateAnalyzerNamespace()
    {
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var invalidNames = new List<(string Expected, string Actual)>();

        foreach (var analyzer in analyzers)
        {
            var instance = (DiagnosticAnalyzer)Activator.CreateInstance(analyzer.AnalyzerType);

            Assert.IsNotNull(instance);
            Assert.HasCount(1, instance.SupportedDiagnostics);

            var expectedTypeName = $"Reihitsu.Analyzer.Rules.{instance.SupportedDiagnostics[0].Category}.{analyzer.AnalyzerType.Name}";
            var actualTypeName = analyzer.AnalyzerType.FullName;

            if (expectedTypeName != actualTypeName)
            {
                invalidNames.Add((expectedTypeName, actualTypeName));
            }
        }

        if (invalidNames.Count > 0)
        {
            Assert.Fail($"The following changes are required:\n\n{string.Join(Environment.NewLine, invalidNames.Select(invalidName => $"{invalidName.Actual} => {invalidName.Expected}"))}");
        }
    }

    /// <summary>
    /// Validating the namespace of the analyzer
    /// </summary>
    [TestMethod]
    public void ValidateCodeFixNamespace()
    {
        var codeFixProviders = AnalyzerMetadataDiscovery.DiscoverCodeFixProviders();
        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers();
        var invalidNames = new List<(string Expected, string Actual)>();

        foreach (var codeFixProvider in codeFixProviders)
        {
            var codeFixerInstance = (CodeFixProvider)Activator.CreateInstance(codeFixProvider.CodeFixProviderType);

            Assert.IsNotNull(codeFixerInstance);
            Assert.HasCount(1, codeFixerInstance.FixableDiagnosticIds);

            var analyzer = analyzers.FirstOrDefault(analyzer => analyzer.DiagnosticId == codeFixProvider.DiagnosticId);

            Assert.IsNotNull(analyzer);

            var analyzerInstance = (DiagnosticAnalyzer)Activator.CreateInstance(analyzer.AnalyzerType);

            Assert.IsNotNull(analyzerInstance);
            Assert.HasCount(1, analyzerInstance.SupportedDiagnostics);

            var expectedTypeName = $"Reihitsu.Analyzer.CodeFixes.Rules.{analyzerInstance.SupportedDiagnostics[0].Category}.{codeFixProvider.CodeFixProviderType.Name}";
            var actualTypeName = codeFixProvider.CodeFixProviderType.FullName;

            if (expectedTypeName != actualTypeName)
            {
                invalidNames.Add((expectedTypeName, actualTypeName));
            }
        }

        if (invalidNames.Count > 0)
        {
            Assert.Fail($"The following changes are required:\n\n{string.Join(Environment.NewLine, invalidNames.Select(invalidName => $"{invalidName.Actual} => {invalidName.Expected}"))}");
        }
    }

    #endregion // Tests
}