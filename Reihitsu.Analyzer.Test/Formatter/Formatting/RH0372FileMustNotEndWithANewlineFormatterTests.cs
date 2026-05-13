using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0372FileMustNotEndWithANewlineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0372FileMustNotEndWithANewlineFormatterTests : FormatterTestsBase<RH0372FileMustNotEndWithANewlineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes trailing newlines at the end of a file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                             }
                             
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0372FileMustNotEndWithANewlineAnalyzer.DiagnosticId, 3, 2, 4, 1, AnalyzerResources.RH0372MessageFormat));
    }

    #endregion // Tests
}