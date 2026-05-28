using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/>
/// </summary>
[TestClass]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaFormatterTests : FormatterTestsBase<RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes a trailing comma from the final enum member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTrailingCommaOnFinalEnumMember()
    {
        const string input = """
                             internal enum RH5409
                             {
                                 First,
                                 Second{|#0:,|}
                             }
                             """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     First,
                                     Second
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter preserves a same-line comment attached to the final enum member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesSameLineCommentOnFinalEnumMember()
    {
        const string input = """
                             internal enum RH5409
                             {
                                 Value{|#0:,|} // Comment
                             }
                             """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value // Comment
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    #endregion // Tests
}