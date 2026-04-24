using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0322SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer, RH0322SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that valid comment placements do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForValidCommentPlacements()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {
                                            // First comment in scope
                                            Consume();
                                        }

                                        // A separated comment
                                        Consume();
                                        Consume();

                                        // Commented out code
                                        // Another comment in the same block
                                        Consume();
                                    }

                                    void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a missing blank line before a single-line comment is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineBeforeCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;
                                        {|#0:// Explain the value|}
                                        Consume(value);
                                    }

                                    void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;

                                         // Explain the value
                                         Consume(value);
                                     }

                                     void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0322MessageFormat));
    }
}