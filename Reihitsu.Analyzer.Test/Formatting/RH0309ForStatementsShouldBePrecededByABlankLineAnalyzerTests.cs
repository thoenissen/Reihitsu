using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0309ForStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer, RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that for statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyForWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0309
                                {
                                    public RH0309()
                                    {
                                        for (int i = 0; i < 10; i++)
                                        {
                                        }
                                        {|#0:for|} (int i = 0; i < 10; i++)
                                        {
                                        }

                                        for (int i = 0; i < 10; i++)
                                        {
                                        }
                                        // Test
                                        for (int i = 0; i < 10; i++)
                                        {
                                        }
                                        /* Test */
                                        for (int i = 0; i < 10; i++)
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0309
                                  {
                                      public RH0309()
                                      {
                                          for (int i = 0; i < 10; i++)
                                          {
                                          }

                                          for (int i = 0; i < 10; i++)
                                          {
                                          }

                                          for (int i = 0; i < 10; i++)
                                          {
                                          }
                                          // Test
                                          for (int i = 0; i < 10; i++)
                                          {
                                          }
                                          /* Test */
                                          for (int i = 0; i < 10; i++)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0309MessageFormat));
    }
}