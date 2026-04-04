using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer, RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that break statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBreakWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0312
                                {
                                    public RH0312()
                                    {
                                        while (true)
                                        { 
                                            break;
                                            {|#0:break|};

                                            break;
                                            // Test
                                            break;
                                            /* Test */
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
                                                System.Console.WriteLine();
                                                break;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0312
                                  {
                                      public RH0312()
                                      {
                                          while (true)
                                          { 
                                              break;

                                              break;

                                              break;
                                              // Test
                                              break;
                                              /* Test */
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
                                                  System.Console.WriteLine();
                                                  break;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0312MessageFormat));
    }
}