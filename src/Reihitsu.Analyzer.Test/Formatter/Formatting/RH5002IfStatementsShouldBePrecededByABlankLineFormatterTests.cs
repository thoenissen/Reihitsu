using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5002IfStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before if statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int value)
                                 {
                                     var result = value;
                                     if (result > 0)
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int value)
                                     {
                                         var result = value;

                                         if (result > 0)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 11, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies code with a blank line directly below an endif directive, above a comment line and an if statement, is
    /// left unchanged by the formatter and not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyFormatterKeepsBlankLineBelowEndIfDirectiveAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if DEBUG
                                     a--;
                             #endif

                                     // note
                                     if (a > 1)
                                     {
                                     }
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}