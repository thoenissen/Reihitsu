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
public class RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer, RH3203ExpressionStyleConstructorsShouldNotBeUsedCodeFixProvider>
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
                                    {|#0:public RH3203() => System.Console.WriteLine();|}
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

        await Verify(testData,
                     fixedData,
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
                                    {|#0:public RH3203() => System.Console.WriteLine();|}
                                    {|#1:public RH3203(int i) => System.Console.WriteLine(i);|}
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

                                    {|#0:public RH3203() => _value = 1
                                #pragma warning disable CS0168
                                        ;|}
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

        await Verify(testData, resultData, Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat));
    }

    #endregion // Tests
}