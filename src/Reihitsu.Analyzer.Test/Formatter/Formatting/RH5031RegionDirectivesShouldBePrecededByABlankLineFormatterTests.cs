using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5031RegionDirectivesShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before a region directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 private int _a;
                                 {|#0:#region Helpers|}

                                 private int _b;

                                 #endregion
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int _a;

                                     #region Helpers

                                     private int _b;

                                     #endregion // Helpers
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line between a comment that is the first content after an opening
    /// brace and the region directive below it, so the output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public async Task VerifyFormatterSeparatesCommentFirstAfterBraceFromRegion()
    {
        const string input = """
                             internal class Example
                             {
                                 // c
                                 {|#0:#region A|}
                                 {|#1:#endregion // A|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     // c

                                     #region A

                                     #endregion // A
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat, 2));
    }

    #endregion // Tests
}