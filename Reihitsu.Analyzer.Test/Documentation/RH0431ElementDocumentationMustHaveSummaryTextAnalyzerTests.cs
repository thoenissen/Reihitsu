using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0431ElementDocumentationMustHaveSummaryTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH0431ElementDocumentationMustHaveSummaryTextAnalyzerTests : AnalyzerTestsBase<RH0431ElementDocumentationMustHaveSummaryTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// {|#0:<summary></summary>|}
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0431ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH0431MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an enum member with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberWithEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              internal enum Status
                              {
                                  /// {|#0:<summary></summary>|}
                                  Active,
                              }
                              """;

        await Verify(source, Diagnostics(RH0431ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH0431MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class with a non-empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForClassWithNonEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A test class.</summary>
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// {|#0:<summary></summary>|}
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}