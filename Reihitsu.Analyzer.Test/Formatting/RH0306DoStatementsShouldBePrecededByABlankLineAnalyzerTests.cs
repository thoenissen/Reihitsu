using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0306DoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer, RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that do statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDoWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0306
                                {
                                    public RH0306()
                                    {
                                        do
                                        {
                                        } while (true);
                                        {|#0:do|}
                                        {
                                        } while (true);

                                        do
                                        {
                                        } while (true);
                                        // Test
                                        do
                                        {
                                        } while (true);
                                        /* Test */
                                        do
                                        {
                                        } while (true);
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0306
                                  {
                                      public RH0306()
                                      {
                                          do
                                          {
                                          } while (true);

                                          do
                                          {
                                          } while (true);

                                          do
                                          {
                                          } while (true);
                                          // Test
                                          do
                                          {
                                          } while (true);
                                          /* Test */
                                          do
                                          {
                                          } while (true);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0306MessageFormat));
    }
}