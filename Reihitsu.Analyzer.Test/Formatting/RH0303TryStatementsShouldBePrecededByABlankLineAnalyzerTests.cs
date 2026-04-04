using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0303TryStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer, RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that try statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTryWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0303
                                {
                                    public RH0303()
                                    {
                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                        {|#0:try|}
                                        {
                                        }
                                        catch
                                        {
                                        }

                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                        // Test
                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                        /* Test */
                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0303
                                  {
                                      public RH0303()
                                      {
                                          try
                                          {
                                          }
                                          catch
                                          {
                                          }

                                          try
                                          {
                                          }
                                          catch
                                          {
                                          }

                                          try
                                          {
                                          }
                                          catch
                                          {
                                          }
                                          // Test
                                          try
                                          {
                                          }
                                          catch
                                          {
                                          }
                                          /* Test */
                                          try
                                          {
                                          }
                                          catch
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0303MessageFormat));
    }
}