using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer, RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that while statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWhileWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0305
                                {
                                    public RH0305()
                                    {
                                        while (true)
                                        {
                                        }
                                        {|#0:while|} (true)
                                        {
                                        }

                                        while (true)
                                        {
                                        }
                                        // Test
                                        while (true)
                                        {
                                        }
                                        /* Test */
                                        while (true)
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0305
                                  {
                                      public RH0305()
                                      {
                                          while (true)
                                          {
                                          }

                                          while (true)
                                          {
                                          }

                                          while (true)
                                          {
                                          }
                                          // Test
                                          while (true)
                                          {
                                          }
                                          /* Test */
                                          while (true)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0305MessageFormat));
    }
}