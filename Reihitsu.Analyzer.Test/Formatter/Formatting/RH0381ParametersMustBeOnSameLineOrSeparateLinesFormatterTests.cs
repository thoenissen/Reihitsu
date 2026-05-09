using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH0381ParametersMustBeOnSameLineOrSeparateLinesFormatterTests : FormatterTestsBase<RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter puts parameters either on one line or one per line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method{|#0:(|}int first, int second,
                                             int third)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0381MessageFormat));
    }

    #endregion // Members
}