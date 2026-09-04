using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Regression tests for issue #725: repeated application of the RH5302 code fix on a chain with more than one
/// trailing logical operator must converge instead of growing the line without bound
/// </summary>
[TestClass]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyRepeatedFixTests : BatchCodeFixTestsBase<RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Reproduces the issue's scenario: a single application of the code fix must resolve every trailing
    /// operator in the chain, leaving no RH5302 diagnostic behind for a second application to act on
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleFixApplicationConvergesTheWholeChain()
    {
        const string initialSource = """
                                     internal class Example
                                     {
                                         bool condition1;
                                         bool condition2;
                                         bool condition3;

                                         void Run()
                                         {
                                             if (condition1 &&
                                                 condition2 ||
                                                 condition3)
                                             {
                                             }
                                         }
                                     }
                                     """;
        const string expectedSource = """
                                      internal class Example
                                      {
                                          bool condition1;
                                          bool condition2;
                                          bool condition3;

                                          void Run()
                                          {
                                              if (condition1
                                                  && condition2
                                                  || condition3)
                                              {
                                              }
                                          }
                                      }
                                      """;

        var afterFirstApplication = await ApplyCodeFixAsync(initialSource);

        Assert.AreEqual(expectedSource, afterFirstApplication, "A single fix application should resolve every trailing operator in the chain.");

        await Verify(afterFirstApplication);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2, bool condition3, bool condition4)
                                    {
                                        if (condition1 {|#0:&&|}
                                            condition2 {|#1:&&|}
                                            condition3 {|#2:|||}
                                            condition4)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5302
                                 {
                                     void Run(bool condition1, bool condition2, bool condition3, bool condition4)
                                     {
                                         if (condition1
                                             && condition2
                                             && condition3
                                             || condition4)
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Reproduces issue #725 under Fix All rather than under a single code-fix application: all three
        // trailing operators belong to one chain, so every diagnostic resolves to the same outermost logical
        // expression. The batch fixer discards the two overlapping actions, and the single surviving fix must
        // still converge the whole chain in this one batch application, leaving no RH5302 diagnostic behind
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 3));
    }

    #endregion // BatchCodeFixTestsBase
}