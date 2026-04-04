using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer, RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that yield statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyYieldWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0321
                                {
                                    public System.Collections.Generic.IEnumerable<int> YieldReturn()
                                    {
                                        yield return 1;
                                        yield return 1;

                                        yield return 1;
                                        // Test
                                        yield return 1;
                                        /* Test */
                                        yield return 1;

                                        int i = 0;
                                        {|#0:yield|} return 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0321
                                  {
                                      public System.Collections.Generic.IEnumerable<int> YieldReturn()
                                      {
                                          yield return 1;
                                          yield return 1;

                                          yield return 1;
                                          // Test
                                          yield return 1;
                                          /* Test */
                                          yield return 1;

                                          int i = 0;

                                          yield return 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0321MessageFormat));
    }
}