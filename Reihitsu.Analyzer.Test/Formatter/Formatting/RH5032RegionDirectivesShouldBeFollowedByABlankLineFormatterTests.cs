using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5032RegionDirectivesShouldBeFollowedByABlankLineFormatterTests : FormatterTestsBase<RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line after a region directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:#region Helpers|}
                                 private int _b;

                                 #endregion
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Helpers

                                     private int _b;

                                     #endregion // Helpers
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    #endregion // Tests
}