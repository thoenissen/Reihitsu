using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5604CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests : BatchCodeFixTestsBase<RH5604CodeMustNotContainMixedLineEndingsAnalyzer, RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider>
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
    /// Verifies that a mixed XML documentation line ending is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedLineEndingsInsideXmlDocumentationAreDetectedAndFixed()
    {
        const string testData = "{|#0:/// <summary>\r\n|}"
                                + "/// Documentation.\n"
                                + "/// </summary>\n"
                                + "internal class TestClass\n"
                                + "{\n"
                                + "}";
        const string fixedData = "/// <summary>\n"
                                 + "/// Documentation.\n"
                                 + "/// </summary>\n"
                                 + "internal class TestClass\n"
                                 + "{\n"
                                 + "}";

        // Suppression verification injects directives whose line endings change this document-wide policy fixture.
        await Verify(testData,
                     fixedData,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(1, 1, 2, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that an LF XML documentation line ending is detected and fixed when CRLF is predominant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLfInsideXmlDocumentationIsDetectedWhenCrLfIsPredominant()
    {
        const string testData = "{|#0:/// <summary>\n|}"
                                + "/// Documentation.\r\n"
                                + "/// </summary>\r\n"
                                + "internal class TestClass\r\n"
                                + "{\r\n"
                                + "}";
        const string fixedData = "/// <summary>\r\n"
                                 + "/// Documentation.\r\n"
                                 + "/// </summary>\r\n"
                                 + "internal class TestClass\r\n"
                                 + "{\r\n"
                                 + "}";

        await Verify(testData,
                     fixedData,
                     Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(1, 1, 2, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that a lone CR XML documentation line ending is normalized to predominant LF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf()
    {
        const string testData = "{|#0:/// <summary>\r|}"
                                + "/// Documentation.\n"
                                + "/// </summary>\n"
                                + "internal class TestClass\n"
                                + "{\n"
                                + "}";
        const string fixedData = "/// <summary>\n"
                                 + "/// Documentation.\n"
                                 + "/// </summary>\n"
                                 + "internal class TestClass\n"
                                 + "{\n"
                                 + "}";

        // Suppression verification injects directives whose line endings change this document-wide policy fixture.
        await Verify(testData,
                     fixedData,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(1, 1, 2, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that an XML and ordinary line-ending tie uses CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCrLfWinsTieIncludingXmlDocumentationLineEnding()
    {
        const string testData = "{|#0:/// Documentation\n|}"
                                + "internal class TestClass { }\r\n";
        const string fixedData = "/// Documentation\r\n"
                                 + "internal class TestClass { }\r\n";

        await Verify(testData,
                     fixedData,
                     Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(1, 1, 2, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that uniform XML documentation line endings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenOnlyXmlDocumentationLineEndingsAreUniform()
    {
        const string testData = "/// <summary>\n"
                                + "/// Documentation.\n"
                                + "/// </summary>\n"
                                + "internal class TestClass { }";

        await Verify(testData);
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

    /// <summary>
    /// Verifies that line endings embedded in raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingsInsideRawStringsDoNotProduceDiagnostics()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "    private const string Value = \"\"\"\n"
                                + "line1\n"
                                + "line2\n"
                                + "\"\"\";\r\n"
                                + "}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that line endings embedded in multi-line comments do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingsInsideMultiLineCommentsDoNotProduceDiagnostics()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "    /* line1\n"
                                + "    line2 */\r\n"
                                + "}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation-shaped text and line endings in disabled text do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingsInsideDisabledTextDoNotProduceDiagnostics()
    {
        const string testData = "#if false\r\n"
                                + "/// <summary>\n"
                                + "/// Disabled documentation.\n"
                                + "/// </summary>\n"
                                + "internal class DisabledClass { }\n"
                                + "#endif\r\n"
                                + "internal class TestClass { }";

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = "{|#0:/// <summary>\r\n|}"
                                + "/// Documentation.\n"
                                + "/// </summary>\n"
                                + "{|#1:internal class TestClass\r\n|}"
                                + "{\n"
                                + "}";
        const string fixedData = "/// <summary>\n"
                                 + "/// Documentation.\n"
                                 + "/// </summary>\n"
                                 + "internal class TestClass\n"
                                 + "{\n"
                                 + "}";

        // Verifies that XML and ordinary minority line endings are fixed in one Fix All iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}