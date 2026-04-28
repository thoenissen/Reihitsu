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
}