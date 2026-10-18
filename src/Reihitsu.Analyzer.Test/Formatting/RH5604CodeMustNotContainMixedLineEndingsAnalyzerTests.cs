using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-004")]
    [Requirement("RH5604-005")]
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-005")]
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-005")]
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-005")]
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
    [Requirement("RH5604-002")]
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
    [Requirement("RH5604-002")]
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
    [Requirement("RH5604-002")]
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-004")]
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
    [Requirement("RH5604-001")]
    [Requirement("RH5604-004")]
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
    /// Verifies that CRLF is the predominant line ending on a tie when it is also the line ending encountered first,
    /// so the LF line is reported. <see cref="VerifyCrLfWinsTieWhenLfIsEncounteredFirst"/> separates the CRLF tie
    /// rule from a "first encountered wins" rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-001")]
    public async Task VerifyCrLfWinsTieWhenItIsEncounteredFirst()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{|#0:{\n|}"
                                + "}";

        await Verify(testData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that CRLF is the predominant line ending on a tie even when LF is the line ending encountered first,
    /// so the first line is reported and the fix converts it to CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-001")]
    [Requirement("RH5604-004")]
    [Requirement("RH5604-005")]
    public async Task VerifyCrLfWinsTieWhenLfIsEncounteredFirst()
    {
        const string testData = "{|#0:internal class TestClass\n|}"
                                + "{\r\n"
                                + "}";
        const string fixedData = "internal class TestClass\r\n"
                                 + "{\r\n"
                                 + "}";

        await Verify(testData, fixedData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that line endings embedded in verbatim strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-003")]
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
    [Requirement("RH5604-003")]
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

    /// <summary>
    /// Verifies that a mixed line ending inside a delimited (<c>/** ... */</c>) XML documentation comment is
    /// detected and fixed the same way a single-line (<c>///</c>) one already is, because Roslyn models both
    /// forms' newlines as <see cref="Microsoft.CodeAnalysis.CSharp.SyntaxKind.XmlTextLiteralNewLineToken"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-001")]
    [Requirement("RH5604-005")]
    public async Task VerifyMixedLineEndingsInsideDelimitedXmlDocumentationAreDetectedAndFixed()
    {
        const string testData = "/** <summary>\n"
                                + "{|#0: * Documentation.\r\n|}"
                                + " * </summary> */\n"
                                + "internal class TestClass { }";
        const string fixedData = "/** <summary>\n"
                                 + " * Documentation.\n"
                                 + " * </summary> */\n"
                                 + "internal class TestClass { }";

        await Verify(testData,
                     fixedData,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     Diagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId).WithSpan(2, 1, 3, 1).WithMessage(AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that a line ending inside a multi-line comment stays exempt even when the very same ending is
    /// reported elsewhere in the same, genuinely mixed file — the interior is never counted or reported,
    /// regardless of whether it happens to match the file's minority or majority style
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingInsideMultiLineCommentStaysExemptWhenTheSameEndingIsReportedElsewhere()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "{|#0:    internal int Value = 1;\r\n|}"
                                + "    /* line1\r\n"
                                + "    line2 */\n"
                                + "}";

        // Suppression verification injects directives whose line endings change this document-wide policy fixture.
        await Verify(testData,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that a line ending inside disabled text stays exempt even when the very same ending is
    /// reported elsewhere in the same, genuinely mixed file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineEndingInsideDisabledTextStaysExemptWhenTheSameEndingIsReportedElsewhere()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "{|#0:    internal int Value = 1;\n|}"
                                + "#if false\r\n"
                                + "    internal int Disabled = 1;\n"
                                + "#endif\r\n"
                                + "}";

        await Verify(testData, Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that the line break that ends a directive is counted and reported like any other line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-001")]
    [Requirement("RH5604-004")]
    public async Task VerifyLfAtTheEndOfADirectiveIsDetectedWhenCrLfIsPredominant()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "{|#0:#pragma warning disable CS0168\n|}"
                                + "    int _value;\r\n"
                                + "#pragma warning restore CS0168\r\n"
                                + "}";

        await Verify(testData,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     Diagnostics(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies that line breaks in the text of an interpolated verbatim string literal are neither reported nor counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-003")]
    public async Task VerifyLineEndingsInsideInterpolatedVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = "internal class TestClass\r\n"
                                + "{\r\n"
                                + "    string Value = $@\"line1\n{1}\nline3\n\";\r\n"
                                + "}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that one application of the code fix converts every differing line ending of the file — between
    /// tokens and inside documentation comments — to the predominant one, whichever diagnostic it is applied to,
    /// and leaves the line breaks inside a string literal unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5604-005")]
    public async Task VerifyCodeFixConvertsEveryLineEndingWhicheverDiagnosticItIsAppliedTo()
    {
        const string testData = "internal class TestClass\n"
                                + "{\r\n"
                                + "    int _value;\n"
                                + "    /// <summary>\r\n"
                                + "    /// Gets the value.\n"
                                + "    /// </summary>\n"
                                + "    int Value => _value;\n"
                                + "    string _text = @\"a\r\nb\";\n"
                                + "}";
        const string fixedData = "internal class TestClass\n"
                                 + "{\n"
                                 + "    int _value;\n"
                                 + "    /// <summary>\n"
                                 + "    /// Gets the value.\n"
                                 + "    /// </summary>\n"
                                 + "    int Value => _value;\n"
                                 + "    string _text = @\"a\r\nb\";\n"
                                 + "}";

        var firstLineStart = testData.IndexOf("{\r\n", StringComparison.Ordinal);
        var secondLineStart = testData.IndexOf("    /// <summary>\r\n", StringComparison.Ordinal);

        foreach (var span in new[] { new TextSpan(firstLineStart, "{\r\n".Length), new TextSpan(secondLineStart, "    /// <summary>\r\n".Length) })
        {
            var actions = await GetCodeFixActionsAsync(testData,
                                                       RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId,
                                                       root => Location.Create(root.SyntaxTree, span));

            Assert.HasCount(1, actions);

            var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
            var changedDocument = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.Projects.Single().Documents.Single();
            var changedText = (await changedDocument.GetTextAsync(CancellationToken.None)).ToString();

            Assert.AreEqual(fixedData, changedText, $"Applying the fix to the diagnostic at {span} must convert every differing line ending of the file.");
        }
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