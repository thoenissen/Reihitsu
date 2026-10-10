using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer, RH3207ExpressionStyleFinalizersShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied finalizer is detected and fixed to an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedFinalizerIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() {|#0:=> Release()|};

                                    private void Release()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3207
                                  {
                                      ~RH3207()
                                      {
                                          Release();
                                      }

                                      private void Release()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat));
    }

    /// <summary>
    /// Verifying that a throw-expression-bodied finalizer is fixed to a throw statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowExpressionBodiedFinalizerIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() {|#0:=> throw new System.InvalidOperationException()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3207
                                  {
                                      ~RH3207()
                                      {
                                          throw new System.InvalidOperationException();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat));
    }

    /// <summary>
    /// Verifying that a block-bodied finalizer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockBodiedFinalizerIsNotReported()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207()
                                    {
                                        System.GC.KeepAlive(this);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a finalizer whose expression body carries a directive before the expression is not reported,
    /// because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() =>
                                #pragma warning disable CS0618
                                        System.GC.KeepAlive(this);
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a finalizer whose expression body carries a region directive after the expression is not reported,
    /// because the formatter refuses to move a region directive into the generated block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithRegionAfterExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() => System.GC.KeepAlive(this)
                                #region Finalizer
                                        ;
                                #endregion // Finalizer
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a finalizer whose expression body carries a directive after the expression is still reported
    /// and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() {|#0:=> System.GC.KeepAlive(this)|}
                                #pragma warning disable CS0618
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3207
                                  {
                                      ~RH3207()
                                      {
                                          System.GC.KeepAlive(this)
                                  #pragma warning disable CS0618
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat));
    }

    /// <summary>
    /// Verifying that a finalizer whose expression starts on the line after the arrow is fixed to an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionOnNextLineIsFixedToExpressionStatement()
    {
        const string testData = """
                                internal class RH3207
                                {
                                    ~RH3207() {|#0:=>
                                        System.GC.KeepAlive(this)|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3207
                                  {
                                      ~RH3207()
                                      {
                                          System.GC.KeepAlive(this);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment trailing the arrow and a comment trailing the semicolon both survive the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentsAroundExpressionBodySurviveFix()
    {
        const string testData = """
                                internal class RH3207First
                                {
                                    ~RH3207First() {|#0:=> // why
                                        System.GC.KeepAlive(this)|};
                                }

                                internal class RH3207Second
                                {
                                    ~RH3207Second() {|#1:=> System.GC.KeepAlive(this)|}; // because
                                }
                                """;

        const string resultData = """
                                  internal class RH3207First
                                  {
                                      ~RH3207First()
                                      {// why
                                          System.GC.KeepAlive(this);
                                      }
                                  }

                                  internal class RH3207Second
                                  {
                                      ~RH3207Second()
                                      {
                                          System.GC.KeepAlive(this);
                                      } // because
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3207First
                                {
                                    ~RH3207First() {|#0:=> System.GC.KeepAlive(this)|};
                                }

                                internal class RH3207Second
                                {
                                    ~RH3207Second() {|#1:=> System.GC.KeepAlive(this)|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3207First
                                  {
                                      ~RH3207First()
                                      {
                                          System.GC.KeepAlive(this);
                                      }
                                  }

                                  internal class RH3207Second
                                  {
                                      ~RH3207Second()
                                      {
                                          System.GC.KeepAlive(this);
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied finalizers are fixed in one Fix All iteration
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3207MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}