using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0342ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0342ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0342ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0342ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer, RH0342ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
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
                                        int[] values = [0];
                                        _ = values[0];
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
                                        int[] values = [0];
                                        _ = values[0{|#0: |}];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = [0];
                                         _ = values[0];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0342ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0342MessageFormat));
    }

    /// <summary>
    /// Verifies that multi-line collection expressions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionExpressionsDoNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        List<int> values =
                                        [
                                            1,
                                            2,
                                            3,
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }
}