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

    /// <summary>
    /// Verifying that a throw-expression-bodied method is fixed to a compiling throw statement (not <c>return throw</c>)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowExpressionBodiedMethodIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    {|#0:public int GetValue() => throw new System.Exception();|}
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          throw new System.Exception();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that an async ValueTask expression-bodied method is fixed without a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncValueTaskExpressionBodiedMethodIsFixed()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH3202
                                {
                                    {|#0:public async ValueTask DoWorkAsync() => await Task.CompletedTask;|}
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;

                                  internal class RH3202
                                  {
                                      public async ValueTask DoWorkAsync()
                                      {
                                          await Task.CompletedTask;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    #endregion // Tests
}