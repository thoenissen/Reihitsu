using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineFormatterTests : FormatterTestsBase<RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line after break statements when needed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     while (true)
                                     {
                                         break;
                                         Consume();
                                     }
                                 }

                                 private static void Consume()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private static void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 7, 13, 7, 18, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line between a break statement and a block statement that follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public async Task VerifyFormatterInsertsBlankLineAboveBlockStatementAfterBreak()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     while (true)
                                     {
                                         break;
                                         {
                                             Consume();
                                         }
                                     }
                                 }

                                 private static void Consume()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         while (true)
                                         {
                                             break;

                                             {
                                                 Consume();
                                             }
                                         }
                                     }

                                     private static void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 7, 13, 7, 18, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves a switch section directly preceded by a directive behind a section that ends in a
    /// break statement unchanged, and that the result stays analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    public async Task VerifyFormatterKeepsSwitchSectionDirectlyPrecededByDirectiveWithoutBlankLine()
    {
        const string source = """
                              internal class Example
                              {
                                  internal void Method(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                              Consume();
                                              break;
                              #pragma warning disable CS0168
                                          case 2:
                                              Consume();
                                              break;
                                      }
                                  }

                                  private static void Consume()
                                  {
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies code with a break statement directly above the footer of a do statement is left unchanged by the formatter
    /// and not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-008")]
    public async Task VerifyFormatterKeepsBreakDirectlyAboveDoStatementFooterAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (true)
                                     {
                                         do
                                             break;
                                         while (a > 3);
                                     }
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that code with labeled <c>break</c> statements in front of a closing brace, in front of the footer of a
    /// <c>do</c> statement, and above a blank line is left unchanged by the formatter and not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterKeepsLabeledBreakStatementsAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 void M(int[] values, int a)
                                 {
                                     outer: foreach (var v in values)
                                     {
                                         foreach (var w in values)
                                         {
                                             if (v == w)
                                             {
                                                 break outer;
                                             }
                                         }

                                         do
                                             break outer;
                                         while (a > 3);

                                         while (true)
                                         {
                                             break outer;

                                             a++;
                                         }
                                     }
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line after a labeled <c>break</c> followed directly by a statement,
    /// and that the analyzer reports nothing on the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterFixesLabeledBreakViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     outer: while (true)
                                     {
                                         while (true)
                                         {
                                             break outer;
                                             Consume();
                                         }
                                     }
                                 }

                                 private static void Consume()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         outer: while (true)
                                         {
                                             while (true)
                                             {
                                                 break outer;

                                                 Consume();
                                             }
                                         }
                                     }

                                     private static void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 9, 17, 9, 22, MessageFormat));
    }

    #endregion // Tests
}