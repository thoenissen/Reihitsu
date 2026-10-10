using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer"/>
/// </summary>
[TestClass]
public class RH5107CommaMustBeOnSameLineAsPreviousParameterFormatterTests : FormatterTestsBase<RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer>
{
    #region Tests

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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that a leading comma kept on its own line by a line comment is aligned with the parameters, stays
    /// unreported, and is otherwise left as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLeadingCommaAfterLineCommentIsStable()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(int first // keep
                                             , int second)
                                 {
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}