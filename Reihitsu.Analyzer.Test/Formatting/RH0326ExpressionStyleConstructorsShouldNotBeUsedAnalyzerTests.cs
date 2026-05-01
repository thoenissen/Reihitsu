using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that expression-bodied constructors are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedConstructorsAreDetected()
    {
        const string testData = """
                                internal class RH0326
                                {
                                    {|#0:public RH0326() => System.Console.WriteLine();|}
                                    {|#1:public RH0326(int i) => System.Console.WriteLine(i);|}
                                    
                                }

                                internal class RH0326Expression
                                {   
                                    public RH0326Expression()
                                    {
                                        System.Console.WriteLine();
                                    }

                                    public RH0326Expression(int i)
                                    {
                                        System.Console.WriteLine(i);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0326MessageFormat, 2));
    }

    #endregion // Members
}