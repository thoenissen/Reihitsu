using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0385CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0385CodeMustNotContainMixedLineEndingsAnalyzerTests : AnalyzerTestsBase<RH0385CodeMustNotContainMixedLineEndingsAnalyzer, RH0385CodeMustNotContainMixedLineEndingsCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that mixed line endings are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedLineEndingsAreDetectedAndFixed()
    {
        var alternativeLineEnding = Environment.NewLine == "\r\n"
                                        ? "\n"
                                        : "\r\n";

        var testData = $"internal class TestClass{Environment.NewLine}"
                       + "{|#0:{" + alternativeLineEnding + "|}"
                       + $"    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";
        var fixedData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH0385MessageFormat));
    }

    /// <summary>
    /// Verifies that LF-only files do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenFileUsesLfOnly()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method()\n"
                                + "    {\n"
                                + "    }\n"
                                + "}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that CRLF-only files do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenFileUsesCrLfOnly()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "    void Method()\r\n"
                                + "    {\r\n"
                                + "    }\r\n"
                                + "}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that CRLF line endings are detected when LF is predominant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCrLfLineEndingsAreDetectedWhenLfIsPredominant()
    {
        const string testData = "{|#0:internal class TestClass\r\n|}"
                                + "{\n"
                                + "{|#1:    void Method()\r\n|}"
                                + "    {\n"
                                + "    }\n"
                                + "}";

        await Verify(testData, Diagnostics(RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH0385MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that LF line endings are detected when CRLF is predominant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLfLineEndingsAreDetectedWhenCrLfIsPredominant()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{|#0:{\n|}"
                                + "    void Method()\r\n"
                                + "    {\r\n"
                                + "    }\r\n"
                                + "}";

        await Verify(testData, Diagnostics(RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH0385MessageFormat));
    }

    /// <summary>
    /// Verifies that ties use the first encountered line ending as the predominant style
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFirstEncounteredLineEndingWinsWhenCountsAreTied()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{|#0:{\n|}"
                                + "}";

        await Verify(testData, Diagnostics(RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH0385MessageFormat));
    }

    /// <summary>
    /// Verifies that line endings embedded in verbatim strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingsInsideVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "    string Value = @\"line1\nline2\";\r\n"
                                + "}";

        await Verify(testData);
    }

    #endregion // Members
}