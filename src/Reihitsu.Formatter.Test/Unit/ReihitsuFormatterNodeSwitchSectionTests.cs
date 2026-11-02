using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that a node-level entry point places the switch sections, labels, and first statements inside its target on lines
/// of their own, and keeps the gap in front of the target itself
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeSwitchSectionTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the sections and first statements of a switch statement given as target start their own lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public async Task SwitchStatementTargetGetsItsSectionsOnLinesOfTheirOwn()
    {
        await AssertFormatsTarget("class C\n{\n    int M(int x)\n    {\n        switch (x) { case 1: return 1; case 2: return 2; }\n\n        return 0;\n    }\n}",
                                  "class C\n{\n    int M(int x)\n    {\n        switch (x)\n        {\n            case 1:\n                return 1;\n            case 2:\n                return 2;\n        }\n\n        return 0;\n    }\n}",
                                  root => root.DescendantNodes().OfType<SwitchStatementSyntax>().Single());
    }

    /// <summary>
    /// Verifies that a first statement on its label's line, given as target, keeps the gap in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    public async Task FirstStatementTargetOnItsLabelsLineKeepsTheGapInFrontOfIt()
    {
        const string input = "class C\n{\n    void M(int x)\n    {\n        switch (x)\n        {\n            case 1: A();\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input,
                                  input,
                                  root => root.DescendantNodes().OfType<ExpressionStatementSyntax>().Single());
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates a document with the given text in a new workspace
    /// </summary>
    /// <param name="text">Document text</param>
    /// <returns>The document</returns>
    private static Document CreateDocument(string text)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);

        return project.AddDocument("Test.cs", SourceText.From(text));
    }

    /// <summary>
    /// Formats the selected target under LF and CRLF line endings through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>,
    /// asserting the document's text after one pass and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedDocumentWithLf">Expected document with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedDocumentWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var description = $"under {DescribeLineEnding(endOfLine)} line endings";
            var expected = NormalizeLineEndings(expectedDocumentWithLf, endOfLine);
            var firstPass = await FormatInDocument(NormalizeLineEndings(inputWithLf, endOfLine), selectTarget);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass of FormatNodeInDocumentAsync {description}.");

            var secondPass = await FormatInDocument(firstPass, selectTarget);

            Assert.AreEqual(expected, secondPass, $"The second pass of FormatNodeInDocumentAsync must not change the output {description}.");
        }
    }

    /// <summary>
    /// Formats the selected target of the given text through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> FormatInDocument(string text, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        var document = CreateDocument(text);
        var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
        var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, selectTarget(root), TestContext.CancellationToken);

        return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
    }

    #endregion // Methods
}