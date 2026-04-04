using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer, RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that return statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturnWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0310
                                {
                                    public RH0310()
                                    {
                                        return;
                                        {|#0:return|};

                                        return;
                                        // Test
                                        return;
                                        /* Test */
                                        return;
                                            
                                            
                                        switch (1)
                                        {
                                            case 1:
                                                return;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0310
                                  {
                                      public RH0310()
                                      {
                                          return;

                                          return;

                                          return;
                                          // Test
                                          return;
                                          /* Test */
                                          return;
                                              
                                              
                                          switch (1)
                                          {
                                              case 1:
                                                  return;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0310MessageFormat));
    }
}