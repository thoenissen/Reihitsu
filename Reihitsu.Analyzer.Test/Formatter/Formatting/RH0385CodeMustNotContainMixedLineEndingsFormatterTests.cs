using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0385CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0385CodeMustNotContainMixedLineEndingsFormatterTests : FormatterTestsBase<RH0385CodeMustNotContainMixedLineEndingsAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter normalizes mixed line endings to the predominant style
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             """
                             + "{\n"
                             + """
                                   internal int Value => 42;
                               }
                               """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value => 42;
                                 }
                                 """;
        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, 2, 1, 3, 1, AnalyzerResources.RH0385MessageFormat));
    }

    #endregion // Members
}