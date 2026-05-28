using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5603FileMustNotEndWithANewlineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5603FileMustNotEndWithANewlineFormatterTests : FormatterTestsBase<RH5603FileMustNotEndWithANewlineAnalyzer>
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
                                 public int Bar { get; set; }
                             }
                             
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, 4, 2, 5, 1, AnalyzerResources.RH5603MessageFormat));
    }

    #endregion // Tests
}