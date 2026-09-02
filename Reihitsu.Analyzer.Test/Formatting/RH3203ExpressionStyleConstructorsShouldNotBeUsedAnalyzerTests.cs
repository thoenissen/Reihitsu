using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer, RH3203ExpressionStyleConstructorsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied constructor is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedConstructorsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    public RH3203() {|#0:=> System.Console.WriteLine()|};
                                }
                                """;
        const string fixedData = """
                                 internal class RH3203
                                 {
                                     public RH3203()
                                     {
                                         System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData.Replace("\r\n", "\n"),
                     fixedData.Replace("\r\n", "\n"),
                     Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat));
    }

    /// <summary>
    /// Verifying that multiple expression-bodied constructors are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleExpressionBodiedConstructorsAreDetected()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    public RH3203() {|#0:=> System.Console.WriteLine()|};
                                    public RH3203(int i) {|#1:=> System.Console.WriteLine(i)|};
                                }

                                internal class RH3203Expression
                                {
                                    public RH3203Expression()
                                    {
                                        System.Console.WriteLine();
                                    }

                                    public RH3203Expression(int i)
                                    {
                                        System.Console.WriteLine(i);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a constructor whose expression body carries a directive before the expression is not
    /// reported, because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() =>
                                #pragma warning disable CS0618
                                        _value = 1;
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a constructor whose expression body carries a directive after the expression is still
    /// reported and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() {|#0:=> _value = 1|}
                                #pragma warning disable CS0168
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3203
                                  {
                                      private int _value;
                                      public RH3203()
                                      {
                                          _value = 1
                                  #pragma warning disable CS0168
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() {|#0:=> _value = 1|};

                                    public RH3203(int value) {|#1:=> _value = value|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3203
                                  {
                                      private int _value;
                                      public RH3203()
                                      {
                                          _value = 1;
                                      }
                                      public RH3203(int value)
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied constructors are fixed in one Fix All iteration
        return new FixAllScenario(testData.Replace("\r\n", "\n"),
                                  resultData.Replace("\r\n", "\n"),
                                  Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}