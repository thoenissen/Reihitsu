using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Executes every rule document's <c>### Violation</c> / <c>### Correction</c> example against the shipped
/// analyzer and, where one exists, the shipped code fix. This is the executable counterpart to a documentation
/// page: a page can look correct to a reviewer while its example is off by a single space, and only running it
/// finds that
/// </summary>
[TestClass]
public class RuleDocumentationExampleTests
{
    #region Fields

    /// <summary>
    /// Verification results for every discovered rule document example, computed once for the whole test class
    /// </summary>
    private static IReadOnlyList<RuleDocumentationExampleResult> _results = [];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies every rule document example once for the whole test class
    /// </summary>
    /// <param name="testContext">Test context</param>
    [ClassInitialize]
    public static void VerifyAllExamples(TestContext testContext)
    {
        _ = testContext;

        var analyzers = AnalyzerMetadataDiscovery.DiscoverAnalyzers().ToDictionary(analyzer => analyzer.DiagnosticId, StringComparer.Ordinal);
        var codeFixProviders = AnalyzerMetadataDiscovery.DiscoverCodeFixProviders();
        var examples = RuleDocumentationExampleDiscovery.DiscoverExamples();

        _results = examples.Select(example => RuleDocumentationExampleVerifier.Verify(example, analyzers, codeFixProviders)).ToArray();
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that every rule document not listed in <see cref="RuleDocumentationExampleOptOuts.Reasons"/>
    /// reports its own diagnostic ID on the violation example, reports it zero times on the correction
    /// example, and - where a code fix is shipped - that applying the fix to the violation until the
    /// diagnostic is gone produces exactly the documented correction
    /// </summary>
    [TestMethod]
    public void EveryNonOptedOutDocumentPassesVerification()
    {
        var failures = _results.Where(result => result.Outcome != RuleDocumentationExampleOutcome.Passed
                                                && RuleDocumentationExampleOptOuts.Reasons.ContainsKey(result.Example.DiagnosticId) == false)
                               .OrderBy(result => result.Example.DiagnosticId, StringComparer.Ordinal)
                               .Select(result => $"{result.Example.DiagnosticId} ({result.Example.DocumentPath}) [{result.Outcome}]: {result.Detail}")
                               .ToArray();

        Assert.IsEmpty(failures, $"The following rule documents failed example verification and are not opted out:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    /// <summary>
    /// Verifies that every opted-out rule document still fails verification, so the opt-out list can only
    /// shrink: a document that starts passing must be removed from <see cref="RuleDocumentationExampleOptOuts.Reasons"/>
    /// in the same change that fixed it
    /// </summary>
    [TestMethod]
    public void OptedOutDocumentsStillFailVerification()
    {
        var resultsByDiagnosticId = _results.ToDictionary(result => result.Example.DiagnosticId, StringComparer.Ordinal);

        var nowPassing = RuleDocumentationExampleOptOuts.Reasons.Keys
                                                        .Where(diagnosticId => resultsByDiagnosticId.TryGetValue(diagnosticId, out var result)
                                                                               && result.Outcome == RuleDocumentationExampleOutcome.Passed)
                                                        .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                                        .ToArray();

        Assert.IsEmpty(nowPassing, $"The following rule documents are opted out but now pass verification; remove them from {nameof(RuleDocumentationExampleOptOuts)}.{nameof(RuleDocumentationExampleOptOuts.Reasons)}:{Environment.NewLine}{string.Join(Environment.NewLine, nowPassing)}");

        var unknownOptOuts = RuleDocumentationExampleOptOuts.Reasons.Keys
                                                            .Where(diagnosticId => resultsByDiagnosticId.ContainsKey(diagnosticId) == false)
                                                            .OrderBy(diagnosticId => diagnosticId, StringComparer.Ordinal)
                                                            .ToArray();

        Assert.IsEmpty(unknownOptOuts, $"The following opt-out entries do not match any discovered rule document:{Environment.NewLine}{string.Join(Environment.NewLine, unknownOptOuts)}");
    }

    #endregion // Tests
}