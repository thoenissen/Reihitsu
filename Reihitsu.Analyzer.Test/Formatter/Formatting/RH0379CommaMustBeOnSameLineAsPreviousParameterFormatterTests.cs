using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer"/>
/// </summary>
[TestClass]
public class RH0379CommaMustBeOnSameLineAsPreviousParameterFormatterTests : FormatterTestsBase<RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter keeps commas on the same line as the previous parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(int first
                                             {|#0:,|} int second)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH0379MessageFormat));
    }

    #endregion // Members
}