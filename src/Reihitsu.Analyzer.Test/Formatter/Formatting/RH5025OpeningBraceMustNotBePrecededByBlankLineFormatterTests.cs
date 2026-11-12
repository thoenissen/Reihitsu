using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5025OpeningBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before opening braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                             {|#0:
                             |}    {
                                     int value = 0;
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
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter keeps the blank line above a block statement that follows a statement not ending with a
    /// closing brace, and that the result is analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public async Task VerifyFormatterKeepsBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string source = """
                              internal class Example
                              {
                                  void Method()
                                  {
                                      int value = 0;

                                      {
                                          value++;
                                      }
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter keeps the blank line above a block statement that follows a statement ending with a
    /// closing brace, and that the result is analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public async Task VerifyFormatterKeepsBlankLineAboveBlockStatementAfterClosingBrace()
    {
        const string source = """
                              internal class Example
                              {
                                  void Method(bool flag)
                                  {
                                      if (flag)
                                      {
                                      }

                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies code with a blank line directly below a region directive is left unchanged by the formatter and not
    /// reported, so the formatter and the analyzer agree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-007")]
    public async Task VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.Dictionary<string, int> _d = new()
                                 {
                                     { "a", 1 },
                                     #region A

                                     { "b", 2 },
                                     #endregion // A

                                     { "c", 3 },
                                 };
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}