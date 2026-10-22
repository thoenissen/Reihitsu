using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for the normalization of existing line breaks to the detected line ending, through
/// <see cref="ReihitsuFormatter.FormatSyntaxTree"/> so the line ending is detected from the input
/// </summary>
[TestClass]
public class ReihitsuFormatterLineEndingTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the spec example: one <c>\r\n</c> among <c>\n</c> line breaks is rewritten to <c>\n</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-001")]
    public void CarriageReturnLineFeedAmongLineFeedsIsRewritten()
    {
        AssertFormatted("class C\n{\n    int x;\r\n    int y;\n}", "class C\n{\n    int x;\n    int y;\n}");
    }

    /// <summary>
    /// Verifies that one <c>\n</c> among <c>\r\n</c> line breaks is rewritten to <c>\r\n</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-001")]
    public void LineFeedAmongCarriageReturnLineFeedsIsRewritten()
    {
        AssertFormatted("class C\r\n{\r\n    int x;\n    int y;\r\n}", "class C\r\n{\r\n    int x;\r\n    int y;\r\n}");
    }

    /// <summary>
    /// Verifies the spec example and its Unicode variants: a lone <c>\r</c>, U+0085, U+2028, or U+2029 between tokens is a
    /// line break of its own and is rewritten to the detected line ending
    /// </summary>
    /// <param name="terminator">The line terminator after <c>int x;</c></param>
    [TestMethod]
    [Requirement("FMT-EOL-001")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void OtherLineTerminatorsBetweenTokensAreRewritten(string terminator)
    {
        AssertFormatted($"class C\n{{\n    int x;{terminator}    int y;\n}}", "class C\n{\n    int x;\n    int y;\n}");
        AssertFormatted($"class C\r\n{{\r\n    int x;{terminator}    int y;\r\n    int z;\r\n}}", "class C\r\n{\r\n    int x;\r\n    int y;\r\n    int z;\r\n}");
    }

    /// <summary>
    /// Verifies the spec example: the line break that ends a preprocessor directive is rewritten to the detected line ending
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-002")]
    public void LineBreakEndingADirectiveIsRewritten()
    {
        AssertFormatted("class C\n{\n#if DEBUG\r\n    int x;\n#endif\n    int y;\n}", "class C\n{\n#if DEBUG\n    int x;\n#endif\n    int y;\n}");
    }

    /// <summary>
    /// Verifies that the line breaks ending <c>#pragma</c>, <c>#region</c>, and <c>#endregion</c> directives are rewritten to
    /// <c>\r\n</c> in a <c>\r\n</c> file
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-002")]
    public void LineBreaksEndingPragmaAndRegionDirectivesAreRewritten()
    {
        const string input = "class C\r\n{\r\n    #region Fields\n\r\n#pragma warning disable CS0169\n    int x;\r\n#pragma warning restore CS0169\n\r\n    #endregion // Fields\n}";
        const string expected = "class C\r\n{\r\n    #region Fields\r\n\r\n#pragma warning disable CS0169\r\n    int x;\r\n\r\n#pragma warning restore CS0169\r\n\r\n    #endregion // Fields\r\n}";

        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a <c>\n</c> inside a single-line documentation comment of a <c>\r\n</c> file is rewritten
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    public void LineBreakInsideSingleLineDocumentationCommentIsRewritten()
    {
        const string input = "class C\r\n{\r\n    /// <summary>\n    /// Gets the value.\r\n    /// </summary>\r\n    int A;\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    /// <summary>\r\n    /// Gets the value.\r\n    /// </summary>\r\n    int A;\r\n}";

        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>\r\n</c> inside a delimited documentation comment of a <c>\n</c> file is rewritten
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    public void LineBreakInsideDelimitedDocumentationCommentIsRewritten()
    {
        const string input = "class C\n{\n    /**\r\n     * <summary>Gets the value.</summary>\n     */\n    int A;\n\n    int B;\n}";
        const string expected = "class C\n{\n    /**\n     * <summary>Gets the value.</summary>\n     */\n    int A;\n\n    int B;\n}";

        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: <c>\r\n</c> inside verbatim, interpolated verbatim, and raw string literals of a <c>\n</c>
    /// file is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-004")]
    public void LineBreaksInsideLiteralsOfLineFeedFileAreKept()
    {
        const string input = "class C\n{\n    string s = @\"a\r\nb\";\n    string t = $@\"c\r\n{1}\";\n    string u = \"\"\"\r\n               d\r\n               \"\"\";\n}";

        AssertFormatted(input, input);
    }

    /// <summary>
    /// Verifies that <c>\n</c> inside verbatim and raw string literals of a <c>\r\n</c> file is kept, also when the raw
    /// string is moved by raw-string alignment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-EOL-004")]
    public void LineBreaksInsideLiteralsOfCarriageReturnLineFeedFileAreKept()
    {
        const string input = "class C\r\n{\r\n    void M()\r\n    {\r\n            var s = @\"a\nb\";\r\n            var u = \"\"\"\n    d\n    \"\"\";\r\n    }\r\n}";
        const string expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = @\"a\nb\";\r\n        var u = \"\"\"\n                d\n                \"\"\";\r\n    }\r\n}";

        AssertFormatted(input, expected);
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
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/>, verifies the output byte for byte, and
    /// verifies that formatting the output again leaves it unchanged
    /// </summary>
    /// <param name="input">The input text</param>
    /// <param name="expected">The expected output</param>
    private void AssertFormatted(string input, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).ToString();

        Assert.AreEqual(Escape(expected), Escape(actual));

        var secondPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(actual, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).ToString();

        Assert.AreEqual(Escape(actual), Escape(secondPass), "The formatter is not idempotent.");
    }

    #endregion // Methods
}