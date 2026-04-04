using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0318UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH0318UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that unchecked statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUncheckedWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0318
                                {
                                    public RH0318()
                                    {
                                        unchecked
                                        {
                                        }
                                        {|#0:unchecked|}
                                        {
                                        }

                                        unchecked
                                        {
                                        }
                                        // Test
                                        unchecked
                                        {
                                        }
                                        /* Test */
                                        unchecked
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0318
                                  {
                                      public RH0318()
                                      {
                                          unchecked
                                          {
                                          }

                                          unchecked
                                          {
                                          }

                                          unchecked
                                          {
                                          }
                                          // Test
                                          unchecked
                                          {
                                          }
                                          /* Test */
                                          unchecked
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0318MessageFormat));
    }
}