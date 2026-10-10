using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3207ExpressionStyleFinalizersShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied finalizers into block-bodied finalizers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 ~Example() => System.GC.KeepAlive(this);
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     ~Example()
                                     {
                                         System.GC.KeepAlive(this);
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, 3, 16, 3, 44, AnalyzerResources.RH3207MessageFormat));
    }

    #endregion // Tests
}