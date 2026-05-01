using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests : AnalyzerTestsBase<RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer, RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying that a multi-line expression-bodied get-only property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0327
                                {
                                    {|#0:public int P2
                                            => 2;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH0327
                                 {
                                     public int P2 => 2;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0327MessageFormat));
    }

    /// <summary>
    /// Verifying that multi-line expression-bodied get-only properties are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetected()
    {
        const string testData = """
                                internal class RH0327
                                {
                                    public int P1 => 0;

                                    {|#0:public int P2
                                            => 2;|}

                                    {|#1:public int P3 => 0
                                                          + 3;|}

                                    public int P4 { get; set; }
                                    public int P5 { get; }
                                    public int P6 { get; } = 6;

                                    public int P7
                                    {
                                        get => 7;
                                    }

                                    public int P8
                                    {
                                        get => P4;
                                        set => P4 = value;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0327MessageFormat, 2));
    }

    #endregion // Members
}