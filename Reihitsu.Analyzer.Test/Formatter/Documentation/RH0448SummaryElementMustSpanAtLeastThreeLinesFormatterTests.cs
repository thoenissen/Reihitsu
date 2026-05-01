using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Documentation;

/// <summary>
/// Formatter validation tests for <see cref="RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH0448SummaryElementMustSpanAtLeastThreeLinesFormatterTests : FormatterTestsBase<RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter expands single-line summary elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 /// <summary>Summary text</summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, 3, 9, 3, 40, AnalyzerResources.RH0448MessageFormat));
    }

    #endregion // Members
}