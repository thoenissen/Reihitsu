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

    /// <summary>
    /// Verifies that the formatter's output for a region directive below an active <c>#if</c> directive directly after an
    /// opening brace is not reported, so the formatter and the analyzer apply one rule, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public async Task VerifyFormatterSeparatesActiveConditionalAfterBraceFromRegion()
    {
        const string input = """
                             internal class Example
                             {
                             #if !DEBUG
                                 {|#0:#region A|}
                                 private void M() { }
                                 {|#1:#endregion|}
                             #endif
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                 #if !DEBUG

                                     #region A

                                     private void M()
                                     {
                                     }

                                     #endregion // A

                                 #endif
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a region directive inside a branch of <c>#if</c> that is not taken stays unchanged and unreported,
    /// so neither side pads a region directive the compiler skips
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-007")]
    public async Task VerifyFormatterLeavesRegionInInactiveConditionalAfterBraceUnchanged()
    {
        const string input = """
                             internal class Example
                             {
                             #if DEBUG
                                 #region A
                                 private void M()
                                 {
                                 }
                                 #endregion // A
                             #endif
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter's output for a region directive below a header comment at the start of a file is not
    /// reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public async Task VerifyFormatterSeparatesHeaderCommentAtFileStartFromRegion()
    {
        const string input = """
                             // header
                             {|#0:#region A|}

                             internal class Example
                             {
                                 private int _a;
                             }

                             #endregion // A
                             """;
        const string fixedData = """
                                 // header

                                 #region A

                                 internal class Example
                                 {
                                     private int _a;
                                 }

                                 #endregion // A
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for a region directive below a comment, behind an earlier region directive in
    /// the same gap, is not reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public async Task VerifyFormatterSeparatesCommentFromSecondRegionInGap()
    {
        const string input = """
                             internal class Example
                             {
                                 private int _a;

                                 #region A

                                 // c
                                 {|#0:#region B|}

                                 private int _b;

                                 #endregion // B

                                 #endregion // A
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int _a;

                                     #region A

                                     // c

                                     #region B

                                     private int _b;

                                     #endregion // B

                                     #endregion // A
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    #endregion // Tests
}