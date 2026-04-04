using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer, RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that foreach statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyForeachWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0308
                                {
                                    private System.Collections.Generic.IAsyncEnumerable<int> _enumerable;
                                    
                                    public async void Test()
                                    {
                                        foreach (var item in new int[0])
                                        {
                                        }
                                        {|#0:foreach|} (var item in new int[0])
                                        {
                                        }

                                        foreach (var item in new int[0])
                                        {
                                        }
                                        // Test
                                        foreach (var item in new int[0])
                                        {
                                        }
                                        /* Test */
                                        foreach (var item in new int[0])
                                        {
                                        }

                                        await foreach (var item in _enumerable)
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0308
                                  {
                                      private System.Collections.Generic.IAsyncEnumerable<int> _enumerable;
                                      
                                      public async void Test()
                                      {
                                          foreach (var item in new int[0])
                                          {
                                          }

                                          foreach (var item in new int[0])
                                          {
                                          }

                                          foreach (var item in new int[0])
                                          {
                                          }
                                          // Test
                                          foreach (var item in new int[0])
                                          {
                                          }
                                          /* Test */
                                          foreach (var item in new int[0])
                                          {
                                          }

                                          await foreach (var item in _enumerable)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0308MessageFormat));
    }
}