using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer"/>
/// </summary>
[TestClass]
public class RH0371CodeMustNotContainBlankLinesAtStartOfFileFormatterTests : FormatterTestsBase<RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines at the start of a file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var input = $"{Environment.NewLine}{Environment.NewLine}internal class Example{Environment.NewLine}{{{Environment.NewLine}}}";
        var fixedData = $"internal class Example{Environment.NewLine}{{{Environment.NewLine}}}";

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId, 1, 1, 3, 1, AnalyzerResources.RH0371MessageFormat));
    }

    #endregion // Tests
}