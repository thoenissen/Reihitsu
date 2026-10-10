using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied conversion operators into block-bodied conversion operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 public static implicit operator int(Example value) => 42;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public static implicit operator int(Example value)
                                     {
                                         return 42;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, 3, 56, 3, 61, AnalyzerResources.RH3206MessageFormat));
    }

    #endregion // Tests
}