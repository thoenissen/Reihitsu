using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that checked statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCheckedWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0317
                                {
                                    public RH0317()
                                    {
                                        checked
                                        {
                                        }
                                        {|#0:checked|}
                                        {
                                        }

                                        checked
                                        {
                                        }
                                        // Test
                                        checked
                                        {
                                        }
                                        /* Test */
                                        checked
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0317
                                  {
                                      public RH0317()
                                      {
                                          checked
                                          {
                                          }

                                          checked
                                          {
                                          }

                                          checked
                                          {
                                          }
                                          // Test
                                          checked
                                          {
                                          }
                                          /* Test */
                                          checked
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0317MessageFormat));
    }
}