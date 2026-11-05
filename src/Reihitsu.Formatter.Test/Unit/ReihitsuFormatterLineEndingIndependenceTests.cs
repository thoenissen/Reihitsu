using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> and <see cref="ReihitsuFormatter.FormatDocumentAsync"/> give a
/// file the output of its copy whose rewritten line breaks are already the detected line ending, whatever terminators the
/// file uses and in whatever mix
/// </summary>
[TestClass]
public class ReihitsuFormatterLineEndingIndependenceTests
{
    #region Constants

    /// <summary>
    /// Placeholder for a line break that the line-ending rules rewrite
    /// </summary>
    private const char RewrittenLineBreakPlaceholder = '¶';

    /// <summary>
    /// Placeholder for a line break inside a literal, a comment, or disabled text, which keeps its characters
    /// </summary>
    private const string KeptLineBreakPlaceholder = "¤";

    /// <summary>
    /// Placeholder for a line break inside a literal or disabled text that the file writes as a lone <c>\r</c>, like its
    /// rewritten line breaks, and that the copy keeps as it stands
    /// </summary>
    private const string KeptFileLineBreakPlaceholder = "§";

    /// <summary>
    /// The class of the spec example, with every line break marked as rewritten
    /// </summary>
    private const string TwoRegionClass = "class C¶{¶    #region A¶    int a;¶    #endregion¶¶    #region B¶    int b;¶    #endregion¶}¶";

    /// <summary>
    /// The result the spec example gives for <see cref="TwoRegionClass"/>, with LF line endings
    /// </summary>
    private const string TwoRegionClassResult = "class C\n{\n    #region A\n\n    int a;\n\n    #endregion // A\n\n    #region B\n\n    int b;\n\n    #endregion // B\n}";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the spec example: the class with two regions saved with a lone <c>\r</c> after every line gets the result of
    /// its copy saved with <c>\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    public async Task LoneCarriageReturnFileGetsTheLayoutOfItsLineFeedCopy()
    {
        await AssertSameOutputAsCopy(Fill(TwoRegionClass, "\r"), Fill(TwoRegionClass, "\n"), TwoRegionClassResult);
    }

    /// <summary>
    /// Verifies the spec example with the Unicode terminators: the class with two regions saved with U+0085, U+2028, or U+2029
    /// after every line gets the result of its copy saved with <c>\n</c>
    /// </summary>
    /// <param name="terminator">The terminator after every line</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public async Task UnicodeTerminatorFileGetsTheLayoutOfItsLineFeedCopy(string terminator)
    {
        await AssertSameOutputAsCopy(Fill(TwoRegionClass, terminator), Fill(TwoRegionClass, "\n"), TwoRegionClassResult);
    }

    /// <summary>
    /// Verifies the spec example with mixed line breaks: the class with two regions whose line breaks are <c>\r\n</c>,
    /// <c>\n</c>, and a lone <c>\r</c> in turn, so <c>\n</c> is detected, gets the result of its copy saved with <c>\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    public async Task MixedLineBreaksGetTheLayoutOfTheirLineFeedCopy()
    {
        await AssertSameOutputAsCopy(Fill(TwoRegionClass, "\r\n", "\n", "\r"), Fill(TwoRegionClass, "\n"), TwoRegionClassResult);
    }

    /// <summary>
    /// Verifies that the class with two regions whose line breaks are <c>\r\n</c> and a lone <c>\r</c> in turn, as many of
    /// one as of the other, so <c>\r\n</c> is detected, gets the result of its copy saved with <c>\r\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    public async Task TiedLineBreaksGetTheLayoutOfTheirCarriageReturnLineFeedCopy()
    {
        await AssertSameOutputAsCopy(Fill(TwoRegionClass, "\r\n", "\r"), Fill(TwoRegionClass, "\r\n"), TwoRegionClassResult.Replace("\n", "\r\n"));
    }

    /// <summary>
    /// Verifies that an LF file whose only other line breaks are the newlines of a documentation comment, written as a lone
    /// <c>\r</c>, U+0085, U+2028, or U+2029, gets the result of its copy saved with <c>\n</c>, which moves the comment
    /// above the documentation comment and the documentation lines from column 0 to the member's column
    /// </summary>
    /// <param name="terminator">The terminator after every line of the documentation comment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public async Task ForeignNewlinesInsideADocumentationCommentGiveTheLayoutOfTheLineFeedCopy(string terminator)
    {
        const string template = "class C\n{\n// c\n/// <summary>¶/// A¶/// </summary>¶int x;\n}";
        const string expected = "class C\n{\n    // c\n    /// <summary>\n    /// A\n    /// </summary>\n    int x;\n}";

        await AssertSameOutputAsCopy(Fill(template, terminator), Fill(template, "\n"), expected);
    }

    /// <summary>
    /// Verifies that a file saved with a lone <c>\r</c> after every line, with a lone <c>\r</c> or a U+2028 kept inside a
    /// verbatim string, a multi-line raw string, a block comment behind code, a block comment that starts its line, or
    /// disabled text, gets the result of its copy saved with <c>\n</c> in which that kept line break is unchanged
    /// </summary>
    /// <param name="template">The file, where <c>¶</c> marks a rewritten line break, <c>¤</c> a kept one, and <c>§</c> a kept one that the file writes as a lone <c>\r</c></param>
    /// <param name="keptTerminator">The terminator of the kept line break</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-020")]
    [DataRow("class C¶{¶    private string _s = @\"a¤b\";¶}¶", "\r")]
    [DataRow("class C¶{¶    private string _s = @\"a¤b\";¶}¶", "\u2028")]
    [DataRow("class C¶{¶    private string _r = \"\"\"§                        a¤                        b§                        \"\"\";¶}¶", "\r")]
    [DataRow("class C¶{¶    private string _r = \"\"\"§                        a¤                        b§                        \"\"\";¶}¶", "\u2028")]
    [DataRow("class C¶{¶    private int _x; /* a¤b */¶}¶", "\r")]
    [DataRow("class C¶{¶    private int _x; /* a¤b */¶}¶", "\u2028")]
    [DataRow("class C¶{¶    void M()¶    {¶        /* a¤b */¶        Foo();¶    }¶}¶", "\r")]
    [DataRow("class C¶{¶    void M()¶    {¶        /* a¤b */¶        Foo();¶    }¶}¶", "\u2028")]
    [DataRow("class C¶{¶    void M()¶    {¶        // c¶#if false¶        a¤b§#endif¶        Foo();¶    }¶}¶", "\r")]
    [DataRow("class C¶{¶    void M()¶    {¶        // c¶#if false¶        a¤b§#endif¶        Foo();¶    }¶}¶", "\u2028")]
    public async Task KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy(string template, string keptTerminator)
    {
        var kept = template.Replace(KeptLineBreakPlaceholder, keptTerminator)
                           .Replace(KeptFileLineBreakPlaceholder, "\r");
        var file = Fill(kept, "\r");
        var copy = Fill(kept, "\n");

        await AssertSameOutputAsCopy(file, copy, expected: null);

        foreach (var output in new[] { FormatTree(file), await FormatDocument(file) })
        {
            Assert.Contains($"a{keptTerminator}", output, "The kept line break should keep its characters.");
        }
    }

    /// <summary>
    /// Replaces every <c>¶</c> in the template with the given terminators in turn
    /// </summary>
    /// <param name="template">The template</param>
    /// <param name="terminators">The terminators, used one after another and from the start again</param>
    /// <returns>The text</returns>
    private static string Fill(string template, params string[] terminators)
    {
        var builder = new System.Text.StringBuilder();
        var count = 0;

        foreach (var character in template)
        {
            if (character == RewrittenLineBreakPlaceholder)
            {
                builder.Append(terminators[count % terminators.Length]);
                count++;
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Makes line terminators visible, so an assertion message shows which one differs
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with its line terminators escaped</returns>
    private static string Escape(string text)
    {
        return text.Replace("\r", "\\r")
                   .Replace("\n", "\\n")
                   .Replace("\u0085", "\\u0085")
                   .Replace("\u2028", "\\u2028")
                   .Replace("\u2029", "\\u2029");
    }

    /// <summary>
    /// Asserts, through <see cref="ReihitsuFormatter.FormatSyntaxTree"/> and <see cref="ReihitsuFormatter.FormatDocumentAsync"/>,
    /// that the file gets the output of its copy, that this output is the expected text when one is given, and that
    /// formatting the output again leaves it unchanged
    /// </summary>
    /// <param name="file">The file</param>
    /// <param name="copy">The copy whose rewritten line breaks are the detected line ending</param>
    /// <param name="expected">The expected output, or <see langword="null"/> to compare with the copy only</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertSameOutputAsCopy(string file, string copy, string expected)
    {
        var copyOutput = FormatTree(copy);

        if (expected != null)
        {
            Assert.AreEqual(Escape(expected), Escape(copyOutput), "The copy should give the expected output.");
        }

        var treeOutput = FormatTree(file);

        Assert.AreEqual(Escape(copyOutput), Escape(treeOutput), "FormatSyntaxTree should give the file the output of its copy.");
        Assert.AreEqual(Escape(treeOutput), Escape(FormatTree(treeOutput)), "FormatSyntaxTree is not idempotent on the file's output.");

        var documentOutput = await FormatDocument(file);

        Assert.AreEqual(Escape(copyOutput), Escape(documentOutput), "FormatDocumentAsync should give the file the output of its copy.");
        Assert.AreEqual(Escape(documentOutput), Escape(await FormatDocument(documentOutput)), "FormatDocumentAsync is not idempotent on the file's output.");
    }

    /// <summary>
    /// Formats the text through <see cref="ReihitsuFormatter.FormatSyntaxTree"/>
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The formatted text</returns>
    private string FormatTree(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

        return ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).ToString();
    }

    /// <summary>
    /// Formats the text through <see cref="ReihitsuFormatter.FormatDocumentAsync"/>
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The formatted text</returns>
    private async Task<string> FormatDocument(string text)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}