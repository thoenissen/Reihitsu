using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer, RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
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

    #endregion // Tests
}