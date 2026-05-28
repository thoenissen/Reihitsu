using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5604CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests : AnalyzerTestsBase<RH5604CodeMustNotContainMixedLineEndingsAnalyzer, RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that mixed line endings are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedLineEndingsAreDetectedAndFixed()
    {
        const string testData = "internal class TestClass\r\n{\n    void Method()\r\n    {\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(2, 1, 3, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
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

        await Verify(testData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat, 2));
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

        await Verify(testData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
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

        await Verify(testData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
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

    #endregion // Tests
}