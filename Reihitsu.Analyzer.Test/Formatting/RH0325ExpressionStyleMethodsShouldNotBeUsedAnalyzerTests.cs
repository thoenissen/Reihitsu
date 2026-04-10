using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer, RH0325ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider>
{
    /// <summary>
    /// Verifying that expression-bodied methods are detected and can be fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedMethodsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0325
                                {
                                    {|#0:public int GetValueExpression() => 42;|}
                                    
                                    public int GetValueBlock()
                                    {
                                        return 42;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0325
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

        await Verify(testData, resultData, Diagnostics(RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0325MessageFormat));
    }
}