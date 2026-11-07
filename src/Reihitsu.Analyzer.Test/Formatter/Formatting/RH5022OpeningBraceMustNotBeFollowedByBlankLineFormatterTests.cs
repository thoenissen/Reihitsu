using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5022OpeningBraceMustNotBeFollowedByBlankLineFormatterTests : FormatterTestsBase<RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines after opening braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                             {|#0:
                             |}        int value = 0;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5022MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty region directly after an opening brace stays untouched and clean under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    public async Task VerifyFormatterKeepsEmptyRegionDirectlyAfterOpeningBrace()
    {
        const string source = """
                              internal class Example
                              {
                                  #region A

                                  #endregion // A
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}