using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer, RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider>
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
                                    void Method()
                                    {
                                        int value = -{|#0: |}1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = -1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6018MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates two minus signs, because removing it would glue them into a pre-decrement operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithMinusSign()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = - -x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates a minus sign from a pre-decrement operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithPreDecrement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = - --x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}