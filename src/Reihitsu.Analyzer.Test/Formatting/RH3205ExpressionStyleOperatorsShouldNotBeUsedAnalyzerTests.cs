using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer, RH3205ExpressionStyleOperatorsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied binary operator is detected and fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) {|#0:=> left|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator +(RH3205 left, RH3205 right)
                                      {
                                          return left;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied unary operator is detected and fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedUnaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static bool operator !(RH3205 value) {|#0:=> value == null|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static bool operator !(RH3205 value)
                                      {
                                          return value == null;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that a throw-expression-bodied operator is fixed to a throw statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowExpressionBodiedOperatorIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator -(RH3205 value) {|#0:=> throw new System.NotSupportedException()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator -(RH3205 value)
                                      {
                                          throw new System.NotSupportedException();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that a void compound assignment operator is fixed to an expression statement without a return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVoidCompoundAssignmentOperatorIsFixedToExpressionStatement()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    private int _value;

                                    public void operator +=(int amount) {|#0:=> _value += amount|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      private int _value;

                                      public void operator +=(int amount)
                                      {
                                          _value += amount;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that a void instance increment operator is fixed to an expression statement without a return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVoidIncrementOperatorIsFixedToExpressionStatement()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    private int _value;

                                    public void operator ++() {|#0:=> _value++|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      private int _value;

                                      public void operator ++()
                                      {
                                          _value++;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied static virtual interface operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticVirtualInterfaceOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal interface IRH3205<T>
                                    where T : IRH3205<T>
                                {
                                    static virtual T operator +(T left, T right) {|#0:=> left|};
                                }
                                """;

        const string resultData = """
                                  internal interface IRH3205<T>
                                      where T : IRH3205<T>
                                  {
                                      static virtual T operator +(T left, T right)
                                      {
                                          return left;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that an abstract interface operator without a body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorWithoutBodyIsNotReported()
    {
        const string testData = """
                                internal interface IRH3205<T>
                                    where T : IRH3205<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a block-bodied operator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockBodiedOperatorIsNotReported()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right)
                                    {
                                        return left;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an operator whose expression body carries a directive before the expression is not reported,
    /// because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) =>
                                #pragma warning disable CS0618
                                        left;
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an operator whose expression body carries a directive after the expression is still reported
    /// and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) {|#0:=> left|}
                                #pragma warning disable CS0618
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator +(RH3205 left, RH3205 right)
                                      {
                                          return left
                                  #pragma warning disable CS0618
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that an operator whose expression starts on the line after the arrow is fixed to a single-spaced return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) {|#0:=>
                                        left|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator +(RH3205 left, RH3205 right)
                                      {
                                          return left;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment trailing the arrow and a comment trailing the semicolon both survive the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentsAroundExpressionBodySurviveFix()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) {|#0:=> // why
                                        left|};

                                    public static RH3205 operator -(RH3205 left, RH3205 right) {|#1:=> right|}; // because
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator +(RH3205 left, RH3205 right)
                                      {// why
                                          return left;
                                      }

                                      public static RH3205 operator -(RH3205 left, RH3205 right)
                                      {
                                          return right;
                                      } // because
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3205
                                {
                                    public static RH3205 operator +(RH3205 left, RH3205 right) {|#0:=> left|};

                                    public static RH3205 operator -(RH3205 left, RH3205 right) {|#1:=> right|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3205
                                  {
                                      public static RH3205 operator +(RH3205 left, RH3205 right)
                                      {
                                          return left;
                                      }

                                      public static RH3205 operator -(RH3205 left, RH3205 right)
                                      {
                                          return right;
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied operators are fixed in one Fix All iteration
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3205MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}