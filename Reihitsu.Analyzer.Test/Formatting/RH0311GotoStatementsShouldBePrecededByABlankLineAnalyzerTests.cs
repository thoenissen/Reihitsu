using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer, RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that goto statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGotoWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0311
                                {
                                    public RH0311()
                                    {
                                        goto Label;
                                        {|#0:goto|} Label;

                                        goto Label;
                                        // Test
                                        goto Label;
                                        /* Test */
                                        goto Label;

                                        Label:
                                        return;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0311
                                  {
                                      public RH0311()
                                      {
                                          goto Label;

                                          goto Label;

                                          goto Label;
                                          // Test
                                          goto Label;
                                          /* Test */
                                          goto Label;

                                          Label:
                                          return;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0311MessageFormat));
    }
}