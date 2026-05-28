using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer, RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        int value = 0;
                                        value++;
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
                                        int value = 0;
                                        value{|#0: |}++;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));
    }

    /// <summary>
    /// Verifies that indentation before a line-leading increment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineLeadingIncrementDoesNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    bool Method(System.Collections.Generic.IEnumerable<int> values)
                                    {
                                        int endOfLineCount = 0;
                                        var leadingTrivia = values;

                                        return leadingTrivia.Any(value => value > 0
                                                                       && ++endOfLineCount >= 2);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}