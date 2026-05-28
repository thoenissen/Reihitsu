using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5008ReturnStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before return statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int Method()
                                 {
                                     var value = 1;
                                     return value;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Method()
                                     {
                                         var value = 1;

                                         return value;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 15, AnalyzerResources.RH5008MessageFormat));
    }

    #endregion // Tests
}