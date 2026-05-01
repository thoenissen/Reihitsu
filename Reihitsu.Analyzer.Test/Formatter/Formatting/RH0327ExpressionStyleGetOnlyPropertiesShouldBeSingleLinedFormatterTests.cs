using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter collapses multi-line expression-bodied properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                     => 42;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value => 42;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0327MessageFormat));
    }

    #endregion // Members
}