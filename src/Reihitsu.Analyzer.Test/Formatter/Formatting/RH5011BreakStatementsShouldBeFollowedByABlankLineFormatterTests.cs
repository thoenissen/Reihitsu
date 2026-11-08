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
                              ExpectedDiagnostic(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 7, 13, 7, 18, AnalyzerResources.RH5011MessageFormat));
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
                              ExpectedDiagnostic(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 7, 13, 7, 18, AnalyzerResources.RH5011MessageFormat));
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

    #endregion // Tests
}