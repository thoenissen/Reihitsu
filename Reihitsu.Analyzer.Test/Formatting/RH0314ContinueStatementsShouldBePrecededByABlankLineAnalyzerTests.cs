using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer, RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that continue statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinueWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0314
                                {
                                    public RH0314()
                                    {
                                        while (true)
                                        {   
                                            continue;
                                            {|#0:continue|};

                                            continue;
                                            // Test
                                            continue;
                                            /* Test */
                                            continue;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0314
                                  {
                                      public RH0314()
                                      {
                                          while (true)
                                          {   
                                              continue;

                                              continue;

                                              continue;
                                              // Test
                                              continue;
                                              /* Test */
                                              continue;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0314MessageFormat));
    }
}