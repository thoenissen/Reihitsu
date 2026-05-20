using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/>
/// </summary>
[TestClass]
public class RH0812FinalEnumMemberMustNotHaveTrailingCommaFormatterTests : FormatterTestsBase<RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer>
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
                             internal enum RH0812
                             {
                                 First,
                                 Second{|#0:,|}
                             }
                             """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     First,
                                     Second
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter preserves a same-line comment attached to the final enum member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesSameLineCommentOnFinalEnumMember()
    {
        const string input = """
                             internal enum RH0812
                             {
                                 Value{|#0:,|} // Comment
                             }
                             """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     Value // Comment
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
    }

    #endregion // Tests
}