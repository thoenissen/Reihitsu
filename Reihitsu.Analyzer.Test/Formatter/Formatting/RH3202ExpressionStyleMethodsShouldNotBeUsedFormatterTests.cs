using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3202ExpressionStyleMethodsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied methods into block-bodied methods
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int GetValue() => 42;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int GetValue()
                                     {
                                         return 42;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, 3, 5, 3, 35, AnalyzerResources.RH3202MessageFormat));
    }

    #endregion // Tests
}