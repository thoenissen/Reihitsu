using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer, RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        _ = values[{|#0: |}0];
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

        await Verify(testData, fixedData, Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] first = [0];
                                        int[] second = [0];
                                        _ = first[{|#0: |}0];
                                        _ = second[{|#1: |}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] first = [0];
                                         int[] second = [0];
                                         _ = first[0];
                                         _ = second[0];
                                     }
                                 }
                                 """;

        // Two element-access brackets sit on adjacent lines with no blank line between them; each fix only
        // removes its own trailing whitespace run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}