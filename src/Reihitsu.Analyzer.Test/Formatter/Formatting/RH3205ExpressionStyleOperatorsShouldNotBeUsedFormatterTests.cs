using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3205ExpressionStyleOperatorsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied operators into block-bodied operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int _value;

                                 public static Example operator +(Example left, Example right) => left;

                                 public void operator +=(int amount) => _value += amount;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int _value;

                                     public static Example operator +(Example left, Example right)
                                     {
                                         return left;
                                     }

                                     public void operator +=(int amount)
                                     {
                                         _value += amount;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, 5, 67, 5, 74, AnalyzerResources.RH3205MessageFormat),
                              ExpectedDiagnostic(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, 7, 41, 7, 60, AnalyzerResources.RH3205MessageFormat));
    }

    #endregion // Tests
}