using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0321YieldStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line before yield statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 internal IEnumerable<int> Method()
                                 {
                                     var value = 1;
                                     yield return value;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     internal IEnumerable<int> Method()
                                     {
                                         var value = 1;

                                         yield return value;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 8, 9, 8, 14, AnalyzerResources.RH0321MessageFormat));
    }

    #endregion // Members
}