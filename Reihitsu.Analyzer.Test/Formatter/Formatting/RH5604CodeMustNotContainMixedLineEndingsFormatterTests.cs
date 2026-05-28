using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5604CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5604CodeMustNotContainMixedLineEndingsFormatterTests : FormatterTestsBase<RH5604CodeMustNotContainMixedLineEndingsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter normalizes mixed line endings to the predominant style
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var alternativeLineEnding = Environment.NewLine == "\r\n"
                                        ? "\n"
                                        : "\r\n";

        var input = $"internal class Example{Environment.NewLine}{{{alternativeLineEnding}    internal int Value => 42;{Environment.NewLine}}}";
        var fixedData = $"internal class Example{Environment.NewLine}{{{Environment.NewLine}    internal int Value => 42;{Environment.NewLine}}}";

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, 2, 1, 3, 1, AnalyzerResources.RH5604MessageFormat));
    }

    #endregion // Tests
}