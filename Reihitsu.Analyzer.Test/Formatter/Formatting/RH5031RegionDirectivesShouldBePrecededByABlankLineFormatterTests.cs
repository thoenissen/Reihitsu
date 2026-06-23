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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    #endregion // Tests
}