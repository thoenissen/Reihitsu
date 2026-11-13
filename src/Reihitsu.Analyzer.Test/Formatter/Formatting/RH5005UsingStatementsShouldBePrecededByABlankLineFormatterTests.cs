using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5005UsingStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before using statements
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
                                     var value = 1;
                                     using (var stream = new System.IO.MemoryStream())
                                     {
                                         value += (int)stream.Length;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = 1;

                                         using (var stream = new System.IO.MemoryStream())
                                         {
                                             value += (int)stream.Length;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 14, AnalyzerResources.RH5005MessageFormat));
    }

    /// <summary>
    /// Verifies the formatter puts the blank line in front of await of an await using statement, so the diagnostic on the
    /// using keyword is cleared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    public async Task VerifyFormatterInsertsBlankLineAboveAwaitOfAwaitUsingStatement()
    {
        const string input = """
                             class C
                             {
                                 async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                 {
                                     var a = 1;
                                     await using (resource)
                                     {
                                         a++;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                     {
                                         var a = 1;

                                         await using (resource)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 15, 6, 20, AnalyzerResources.RH5005MessageFormat));
    }

    #endregion // Tests
}