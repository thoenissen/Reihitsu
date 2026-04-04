using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0320LockStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0320LockStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer, RH0320LockStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that lock statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLockWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0320
                                {
                                    private static object _lock = new object();
                                    
                                    public RH0320()
                                    {
                                        lock (_lock)
                                        {
                                        }
                                        {|#0:lock|} (_lock)
                                        {
                                        }

                                        lock (_lock)
                                        {
                                        }
                                        // Test
                                        lock (_lock)
                                        {
                                        }
                                        /* Test */
                                        lock (_lock)
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0320
                                  {
                                      private static object _lock = new object();
                                      
                                      public RH0320()
                                      {
                                          lock (_lock)
                                          {
                                          }

                                          lock (_lock)
                                          {
                                          }

                                          lock (_lock)
                                          {
                                          }
                                          // Test
                                          lock (_lock)
                                          {
                                          }
                                          /* Test */
                                          lock (_lock)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0320MessageFormat));
    }
}