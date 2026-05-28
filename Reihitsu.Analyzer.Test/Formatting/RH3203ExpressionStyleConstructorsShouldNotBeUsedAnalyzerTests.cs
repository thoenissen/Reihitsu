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

    #endregion // Tests
}