using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer, RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                [System.Obsolete]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                [System.Obsolete{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0348MessageFormat));
    }
}