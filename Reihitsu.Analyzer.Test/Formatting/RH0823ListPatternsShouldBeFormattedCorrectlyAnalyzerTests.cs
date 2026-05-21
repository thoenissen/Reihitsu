using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0823ListPatternsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0823ListPatternsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0823ListPatternsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0823ListPatternsShouldBeFormattedCorrectlyAnalyzer, RH0823ListPatternsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a multiline list pattern with multiple inner patterns on one line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineListPatternWithMultipleInnerPatternsOnOneLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is {|#0:[
                                            1, 2,
                                            3
                                                  ]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              2,
                                                              3
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0823ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0823MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline list pattern with misaligned brackets is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineListPatternWithMisalignedBrackets()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is {|#0:[
                                            1,
                                            .. var rest
                                                  ]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              .. var rest
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0823ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0823MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line list pattern remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [1, .. var rest];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted multiline list pattern remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedMultilineListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [
                                                             1,
                                                             .. var rest
                                                         ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}