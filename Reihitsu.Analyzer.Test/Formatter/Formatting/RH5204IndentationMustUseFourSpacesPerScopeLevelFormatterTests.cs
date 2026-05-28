using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[TestClass]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests : FormatterTestsBase<RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter normalizes indentation to four spaces per level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                               internal bool Value { get; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value { get; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, 3, 3, 3, 11, AnalyzerResources.RH5204MessageFormat));
    }

    #endregion // Tests
}