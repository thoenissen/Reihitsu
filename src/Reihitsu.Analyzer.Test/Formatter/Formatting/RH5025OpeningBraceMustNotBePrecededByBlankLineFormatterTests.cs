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

    #endregion // Tests
}