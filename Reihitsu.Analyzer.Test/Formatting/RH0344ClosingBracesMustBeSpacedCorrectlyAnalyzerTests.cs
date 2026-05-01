using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0344ClosingBracesMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer, RH0344ClosingBracesMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
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
                                internal class TestClass
                                {
                                    int Property { get; set;{|#0:}|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0344MessageFormat));
    }

    /// <summary>
    /// Verifies that interpolated-string braces do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterpolatedStringBracesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int value)
                                    {
                                        var text = $"{value}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Members
}