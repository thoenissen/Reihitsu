using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

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

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId, 2, 1, 3, 1, AnalyzerResources.RH5604MessageFormat));
    }

    /// <summary>
    /// Verifies XML CRLF normalizes to predominant LF and formatter entry points remain stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterNormalizesXmlCrLfToPredominantLfAndIsIdempotent()
    {
        const string input = "/// <summary>\r\n"
                             + "/// Documentation.\n"
                             + "/// </summary>\n"
                             + "internal class Example\n"
                             + "{\n"
                             + "    internal int Value => 42;\n"
                             + "}";
        const string expected = "/// <summary>\n"
                                + "/// Documentation.\n"
                                + "/// </summary>\n"
                                + "internal class Example\n"
                                + "{\n"
                                + "    internal int Value => 42;\n"
                                + "}";

        await VerifyFormatterEntryPointsAndIdempotency(input, expected);
    }

    /// <summary>
    /// Verifies XML LF normalizes to predominant CRLF and formatter entry points remain stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterNormalizesXmlLfToPredominantCrLfAndIsIdempotent()
    {
        const string input = "/// <summary>\n"
                             + "/// Documentation.\r\n"
                             + "/// </summary>\r\n"
                             + "internal class Example\r\n"
                             + "{\r\n"
                             + "    internal int Value => 42;\r\n"
                             + "}";
        const string expected = "/// <summary>\r\n"
                                + "/// Documentation.\r\n"
                                + "/// </summary>\r\n"
                                + "internal class Example\r\n"
                                + "{\r\n"
                                + "    internal int Value => 42;\r\n"
                                + "}";

        await VerifyFormatterEntryPointsAndIdempotency(input, expected);
    }

    /// <summary>
    /// Verifies syntax-tree and detached-node formatting normalize mixed input, and document-scoped formatting remains stable
    /// </summary>
    /// <param name="input">Mixed-line-ending input</param>
    /// <param name="expected">Expected normalized output</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterEntryPointsAndIdempotency(string input, string expected)
    {
        // Suppression verification injects directives whose line endings change this document-wide policy fixture.
        await Verify(input,
                     static config => config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck,
                     ExpectedDiagnostic(RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId,
                                        1,
                                        1,
                                        2,
                                        1,
                                        AnalyzerResources.RH5604MessageFormat));

        var syntaxTree = CSharpSyntaxTree.ParseText(input, cancellationToken: CancellationToken.None);
        var firstTree = ReihitsuFormatter.FormatSyntaxTree(syntaxTree, CancellationToken.None);
        var firstTreeText = firstTree.GetRoot(CancellationToken.None).ToFullString();

        Assert.AreEqual(expected, firstTreeText);
        await Verify(firstTreeText);

        var secondTree = ReihitsuFormatter.FormatSyntaxTree(firstTree, CancellationToken.None);

        Assert.AreEqual(firstTreeText, secondTree.GetRoot(CancellationToken.None).ToFullString());

        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: CancellationToken.None).GetRoot(CancellationToken.None);
        var firstNodeText = ReihitsuFormatter.FormatNode(root, cancellationToken: CancellationToken.None).ToFullString();
        var secondNode = CSharpSyntaxTree.ParseText(firstNodeText, cancellationToken: CancellationToken.None).GetRoot(CancellationToken.None);

        Assert.AreEqual(expected, firstNodeText);
        Assert.AreEqual(firstNodeText, ReihitsuFormatter.FormatNode(secondNode, cancellationToken: CancellationToken.None).ToFullString());

        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
                                   .WithParseOptions(CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));

            // Document-scoped formatting must remain stable after the mixed input has converged.
            var document = project.AddDocument("Test.cs", SourceText.From(expected));
            var documentRoot = await document.GetSyntaxRootAsync(CancellationToken.None);

            Assert.IsNotNull(documentRoot);

            var firstDocument = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, documentRoot, CancellationToken.None);
            var firstDocumentText = (await firstDocument.GetTextAsync(CancellationToken.None)).ToString();
            var firstDocumentRoot = await firstDocument.GetSyntaxRootAsync(CancellationToken.None);

            Assert.AreEqual(expected, firstDocumentText);
            Assert.IsNotNull(firstDocumentRoot);

            var secondDocument = await ReihitsuFormatter.FormatNodeInDocumentAsync(firstDocument, firstDocumentRoot, CancellationToken.None);

            Assert.AreEqual(firstDocumentText, (await secondDocument.GetTextAsync(CancellationToken.None)).ToString());
        }
    }

    #endregion // Tests
}