using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5006ForeachStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before foreach statements
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
                                     var values = new[] { 1 };
                                     foreach (var value in values)
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var values = new[] { 1 };

                                         foreach (var value in values)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 16, AnalyzerResources.RH5006MessageFormat));
    }

    /// <summary>
    /// Verifies the formatter puts the blank line in front of await of an await foreach statement, so the diagnostic on
    /// the foreach keyword is cleared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    public async Task VerifyFormatterInsertsBlankLineAboveAwaitOfAwaitForeachStatement()
    {
        const string input = """
                             class C
                             {
                                 async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                 {
                                     var sum = 0;
                                     await foreach (var item in items)
                                     {
                                         sum += item;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                     {
                                         var sum = 0;

                                         await foreach (var item in items)
                                         {
                                             sum += item;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 15, 6, 22, AnalyzerResources.RH5006MessageFormat));
    }

    #endregion // Tests
}