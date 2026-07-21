using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5010BreakStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before break statements
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
                                         var value = 1;
                                         break;
                                     }
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
                                             var value = 1;

                                             break;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 8, 13, 8, 18, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter does not insert a blank line before a break statement inside a block that is
    /// itself the braced body of a switch section (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesBreakInsideBracedSwitchSectionUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int choice)
                                 {
                                     switch (choice)
                                     {
                                         case 1:
                                             {
                                                 var value = choice;
                                                 break;
                                             }
                                     }
                                 }
                             }
                             """;

        await VerifyFormatterStability(input);
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line before a break statement that directly follows a closing
    /// brace outside a switch section, matching the analyzer's switch-section-only exemption (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesBreakStatementAfterClosingBraceOutsideSwitchSection()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     while (true)
                                     {
                                         if (flag)
                                         {
                                             Consume();
                                         }
                                         break;
                                     }
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(bool flag)
                                     {
                                         while (true)
                                         {
                                             if (flag)
                                             {
                                                 Consume();
                                             }

                                             break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 11, 13, 11, 18, AnalyzerResources.RH5010MessageFormat));
    }

    #endregion // Tests
}