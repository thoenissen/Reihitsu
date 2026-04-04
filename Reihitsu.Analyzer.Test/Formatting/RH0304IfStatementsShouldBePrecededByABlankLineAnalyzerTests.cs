using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0304IfStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer, RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that if statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIfWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0304
                                {
                                    public RH0304()
                                    {
                                        if (true)
                                        {
                                        }
                                        {|#0:if|} (true)
                                        {
                                        }

                                        if (true)
                                        {
                                        }
                                        // Test
                                        if (true)
                                        {
                                        }
                                        /* Test */
                                        if (true)
                                        {
                                        }
                                        else if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0304
                                  {
                                      public RH0304()
                                      {
                                          if (true)
                                          {
                                          }

                                          if (true)
                                          {
                                          }

                                          if (true)
                                          {
                                          }
                                          // Test
                                          if (true)
                                          {
                                          }
                                          /* Test */
                                          if (true)
                                          {
                                          }
                                          else if (true)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0304MessageFormat));
    }
}