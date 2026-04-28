using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer, RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                [{|#0: |}System.Obsolete]
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

        await Verify(testData, fixedData, Diagnostics(RH0347OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0347MessageFormat));
    }
}