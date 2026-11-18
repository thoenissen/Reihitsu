using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Analyzers;

/// <summary>
/// Self-hosting scan that runs every Reihitsu analyzer over the solution's own source code, one test row per analyzer.
/// The scan is a regression safety net: a violation in the repository's own source signals an unintended change in
/// analyzer behavior, or code that does not follow the repository's own rules
/// </summary>
[TestClass]
public sealed class AnalyzerSelfHostingTests
{
    #region Fields

    /// <summary>
    /// Analyzers the scan leaves out: the line-count rules RH2008 and RH2111, which this repository's own build turns
    /// off until it has decided its own limits
    /// </summary>
    private static readonly HashSet<string> _analyzersLeftOut = new(StringComparer.Ordinal)
                                                                {
                                                                    nameof(RH2008MethodsShouldNotExceedTheLineLimitAnalyzer),
                                                                    nameof(RH2111TypesShouldNotExceedTheLineLimitAnalyzer)
                                                                };

    #endregion // Fields

    #region Properties

    /// <summary>
    /// One row per shipped analyzer, named by the analyzer type, except the analyzers the scan leaves out
    /// </summary>
    public static IEnumerable<string> AnalyzerNames => AnalyzerCatalog.DiscoverAnalyzerTypes()
                                                                      .Select(type => type.Name)
                                                                      .Where(name => _analyzersLeftOut.Contains(name) == false);

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the analyzer reports no violation on the solution's own source code; a failure lists each
    /// violation with its file, line, rule, and analyzer
    /// </summary>
    /// <param name="analyzerName">Simple name of the analyzer type</param>
    [TestMethod]
    [DynamicData(nameof(AnalyzerNames))]
    public void AnalyzerReportsNoViolationsOnOwnSourceCode(string analyzerName)
    {
        // Arrange
        var solution = SelfHostingSolution.Instance;
        var analyzer = AnalyzerCatalog.CreateAnalyzer(analyzerName);

        // Act
        var violations = AnalyzerViolationReport.Collect(analyzer, solution.Compilations, solution.SolutionRoot, TestContext.CancellationToken);

        // Assert
        if (violations.Count > 0)
        {
            Assert.Fail($"{analyzerName} reported {violations.Count} violation(s):\n{string.Join("\n", violations)}");
        }
    }

    #endregion // Methods
}