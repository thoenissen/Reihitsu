using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied local functions into block-bodied local functions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int GetValue()
                                 {
                                     return Double(21);

                                     int Double(int value) => value * 2;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int GetValue()
                                     {
                                         return Double(21);

                                         int Double(int value)
                                         {
                                             return value * 2;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, 7, 31, 7, 43, AnalyzerResources.RH3208MessageFormat));
    }

    #endregion // Tests
}