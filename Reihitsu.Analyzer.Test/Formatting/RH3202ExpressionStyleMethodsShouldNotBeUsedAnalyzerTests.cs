using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer, RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that expression-bodied methods are detected and can be fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedMethodsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    {|#0:public int GetValueExpression() => 42;|}
                                    
                                    public int GetValueBlock()
                                    {
                                        return 42;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValueExpression()
                                      {
                                          return 42;
                                      }
                                      
                                      public int GetValueBlock()
                                      {
                                          return 42;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    #endregion // Tests
}