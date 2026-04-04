using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer, RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that switch statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySwitchWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0316
                                {
                                    public RH0316()
                                    {
                                        switch (1)
                                        {
                                            case 1:
                                                break;
                                        }
                                        {|#0:switch|} (1)
                                        {
                                            case 1:
                                            break;
                                        }

                                        switch (1)
                                        {
                                            case 1:
                                                break;
                                        }
                                        // Test
                                        switch (1)
                                        {
                                            case 1:
                                                break;
                                        }
                                        /* Test */
                                        switch (1)
                                        {
                                            case 1:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0316
                                  {
                                      public RH0316()
                                      {
                                          switch (1)
                                          {
                                              case 1:
                                                  break;
                                          }

                                          switch (1)
                                          {
                                              case 1:
                                              break;
                                          }

                                          switch (1)
                                          {
                                              case 1:
                                                  break;
                                          }
                                          // Test
                                          switch (1)
                                          {
                                              case 1:
                                                  break;
                                          }
                                          /* Test */
                                          switch (1)
                                          {
                                              case 1:
                                                  break;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0316MessageFormat));
    }
}