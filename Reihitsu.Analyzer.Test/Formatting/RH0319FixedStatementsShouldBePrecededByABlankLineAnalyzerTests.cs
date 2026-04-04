using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that fixed statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixedWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0319
                                {
                                    public unsafe RH0319()
                                    {
                                        fixed (byte* ptr = stackalloc byte[10])
                                        {
                                        }
                                        {|#0:fixed|} (byte* ptr = stackalloc byte[10])
                                        {
                                        }

                                        fixed (byte* ptr = stackalloc byte[10])
                                        {
                                        }
                                        // Test
                                        fixed (byte* ptr = stackalloc byte[10])
                                        {
                                        }
                                        /* Test */
                                        fixed (byte* ptr = stackalloc byte[10])
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0319
                                  {
                                      public unsafe RH0319()
                                      {
                                          fixed (byte* ptr = stackalloc byte[10])
                                          {
                                          }

                                          fixed (byte* ptr = stackalloc byte[10])
                                          {
                                          }

                                          fixed (byte* ptr = stackalloc byte[10])
                                          {
                                          }
                                          // Test
                                          fixed (byte* ptr = stackalloc byte[10])
                                          {
                                          }
                                          /* Test */
                                          fixed (byte* ptr = stackalloc byte[10])
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0319MessageFormat));
    }
}