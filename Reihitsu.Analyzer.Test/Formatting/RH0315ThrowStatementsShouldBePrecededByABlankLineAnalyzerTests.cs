using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0315ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer, RH0315ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that throw statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0315
                                {
                                    public RH0315()
                                    {
                                        throw new System.Exception();
                                        {|#0:throw|} new System.Exception();

                                        throw new System.Exception();
                                        // Test
                                        throw new System.Exception();
                                        /* Test */
                                        throw new System.Exception();

                                        switch (1)
                                        {
                                            case 1:
                                                throw new System.Exception();
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0315
                                  {
                                      public RH0315()
                                      {
                                          throw new System.Exception();

                                          throw new System.Exception();

                                          throw new System.Exception();
                                          // Test
                                          throw new System.Exception();
                                          /* Test */
                                          throw new System.Exception();

                                          switch (1)
                                          {
                                              case 1:
                                                  throw new System.Exception();
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0315MessageFormat));
    }
}