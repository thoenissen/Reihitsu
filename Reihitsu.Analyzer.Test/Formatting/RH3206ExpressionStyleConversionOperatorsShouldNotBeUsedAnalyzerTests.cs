using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer, RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied implicit conversion operator is detected and fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyImplicitConversionOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    private int _value;

                                    public static implicit operator int(RH3206 instance) {|#0:=> instance._value|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      private int _value;

                                      public static implicit operator int(RH3206 instance)
                                      {
                                          return instance._value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied explicit conversion operator is detected and fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExplicitConversionOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static explicit operator RH3206(int value) {|#0:=> new RH3206()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      public static explicit operator RH3206(int value)
                                      {
                                          return new RH3206();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat));
    }

    /// <summary>
    /// Verifying that expression-bodied checked and unchecked explicit conversion operators are both detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCheckedExplicitConversionOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    private long _value;

                                    public static explicit operator int(RH3206 instance) {|#0:=> (int)instance._value|};

                                    public static explicit operator checked int(RH3206 instance) {|#1:=> checked((int)instance._value)|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      private long _value;

                                      public static explicit operator int(RH3206 instance)
                                      {
                                          return (int)instance._value;
                                      }

                                      public static explicit operator checked int(RH3206 instance)
                                      {
                                          return checked((int)instance._value);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a throw-expression-bodied conversion operator is fixed to a throw statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowExpressionBodiedConversionOperatorIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static explicit operator int(RH3206 instance) {|#0:=> throw new System.InvalidCastException()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      public static explicit operator int(RH3206 instance)
                                      {
                                          throw new System.InvalidCastException();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat));
    }

    /// <summary>
    /// Verifying that a block-bodied conversion operator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockBodiedConversionOperatorIsNotReported()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static implicit operator int(RH3206 instance)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a conversion operator whose expression body carries a directive before the expression is not reported,
    /// because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static implicit operator int(RH3206 instance) =>
                                #pragma warning disable CS0618
                                        0;
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a conversion operator whose expression body carries a directive after the expression is still reported
    /// and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static implicit operator int(RH3206 instance) {|#0:=> 0|}
                                #pragma warning disable CS0618
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      public static implicit operator int(RH3206 instance)
                                      {
                                          return 0
                                  #pragma warning disable CS0618
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat));
    }

    /// <summary>
    /// Verifying that a conversion operator whose expression starts on the line after the arrow is fixed to a single-spaced return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static implicit operator int(RH3206 instance) {|#0:=>
                                        0|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      public static implicit operator int(RH3206 instance)
                                      {
                                          return 0;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3206
                                {
                                    public static implicit operator int(RH3206 instance) {|#0:=> 0|};

                                    public static explicit operator RH3206(int value) {|#1:=> new RH3206()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3206
                                  {
                                      public static implicit operator int(RH3206 instance)
                                      {
                                          return 0;
                                      }

                                      public static explicit operator RH3206(int value)
                                      {
                                          return new RH3206();
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied conversion operators are fixed in one Fix All iteration
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3206MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}