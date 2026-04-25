using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer, RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// Verifies that the issue is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        await Verify(testData, fixedData, Diagnostics(RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0351MessageFormat));
    }

    /// <summary>
    /// Verifies that indentation before a line-leading increment does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
}