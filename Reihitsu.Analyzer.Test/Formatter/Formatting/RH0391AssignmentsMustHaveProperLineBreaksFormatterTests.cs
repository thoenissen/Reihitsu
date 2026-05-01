using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0391AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
[TestClass]
public class RH0391AssignmentsMustHaveProperLineBreaksFormatterTests : FormatterTestsBase<RH0391AssignmentsMustHaveProperLineBreaksAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter keeps assignments on a single line around the equals sign
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
                                     var value
                                         = "test";
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value  = "test";
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0391AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, 5, 13, 6, 21, AnalyzerResources.RH0391MessageFormat));
    }

    #endregion // Members
}