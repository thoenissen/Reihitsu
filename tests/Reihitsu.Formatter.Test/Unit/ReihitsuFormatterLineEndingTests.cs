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
    #region Constants

    /// <summary>
    /// Placeholder for the one foreign line break of an input, which a test replaces with the terminator under test
    /// </summary>
    private const string ForeignLineBreakPlaceholder = "¶";

    #endregion // Constants

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
        const string expected = "class C\r\n{\r\n    #region Fields\r\n\r\n#pragma warning disable CS0169\r\n    int x;\r\n#pragma warning restore CS0169\r\n\r\n    #endregion // Fields\r\n}";

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
    /// Verifies that a foreign line break inside a CDATA section of a single-line documentation comment is rewritten to the
    /// detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the CDATA section</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideDocumentationCDataSectionIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    /// <summary>\n    /// <![CDATA[a¶    /// b]]>\n    /// </summary>\n    int A;\n}";
        const string expected = "class C\n{\n    /// <summary>\n    /// <![CDATA[a\n    /// b]]>\n    /// </summary>\n    int A;\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside an XML comment of a single-line documentation comment is rewritten to the
    /// detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the XML comment</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideDocumentationXmlCommentIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    /// <summary>\n    /// <!-- a¶    /// b -->\n    /// </summary>\n    int A;\n}";
        const string expected = "class C\n{\n    /// <summary>\n    /// <!-- a\n    /// b -->\n    /// </summary>\n    int A;\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside an element tag of a single-line documentation comment, between the element
    /// name and an attribute, is rewritten to the detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the element tag</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideDocumentationElementTagIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    /// <summary>\n    /// <para¶    /// foo=\"a\">x</para>\n    /// </summary>\n    int A;\n}";
        const string expected = "class C\n{\n    /// <summary>\n    /// <para\n    /// foo=\"a\">x</para>\n    /// </summary>\n    int A;\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside an attribute value of a single-line documentation comment is rewritten to
    /// the detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the attribute value</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideDocumentationAttributeValueIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    /// <summary>\n    /// <para foo=\"a¶    /// b\">x</para>\n    /// </summary>\n    int A;\n}";
        const string expected = "class C\n{\n    /// <summary>\n    /// <para foo=\"a\n    /// b\">x</para>\n    /// </summary>\n    int A;\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside a CDATA section of a delimited documentation comment is rewritten to the
    /// detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the CDATA section</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideDelimitedDocumentationCDataSectionIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    /**\n     * <summary><![CDATA[a¶     * b]]></summary>\n     */\n    int A;\n}";
        const string expected = "class C\n{\n    /**\n     * <summary><![CDATA[a\n     * b]]></summary>\n     */\n    int A;\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside an interpolation hole of a regular interpolated string is a line break
    /// between tokens and is rewritten to the detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the interpolation hole</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-001")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideInterpolationHoleIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    string s = $\"{1¶                  + 2}\";\n}";
        const string expected = "class C\n{\n    string s = $\"{1\n                  + 2}\";\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break inside an interpolation hole of a raw interpolated string is a line break between
    /// tokens and is rewritten to the detected line ending
    /// </summary>
    /// <param name="terminator">The foreign line break inside the interpolation hole</param>
    /// <param name="endOfLine">The detected line ending</param>
    [TestMethod]
    [Requirement("FMT-EOL-001")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInsideRawInterpolationHoleIsRewritten(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    string s = $\"\"\"\n                {1¶                 + 2}\n                \"\"\";\n}";
        const string expected = "class C\n{\n    string s = $\"\"\"\n                {1\n                 + 2}\n                \"\"\";\n}";

        AssertForeignLineBreakFormatted(input, expected, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a foreign line break in the literal text of a raw interpolated string is kept
    /// </summary>
    /// <param name="terminator">The foreign line break in the literal text</param>
    /// <param name="endOfLine">The line ending of every other line break, which the formatter detects</param>
    [TestMethod]
    [Requirement("FMT-EOL-004")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r", "\n")]
    [DataRow("\u2028", "\r\n")]
    public void LineBreakInTextOfRawInterpolatedStringIsKept(string terminator, string endOfLine)
    {
        const string input = "class C\n{\n    string s = $\"\"\"\n                a¶                {1}\n                \"\"\";\n}";

        AssertForeignLineBreakFormatted(input, input, terminator, endOfLine);
    }

    /// <summary>
    /// Verifies that a file whose every line break is the same single-character terminator other than <c>\n</c> gets
    /// <c>\n</c>, because a line break of a single character counts as <c>\n</c>
    /// </summary>
    /// <param name="terminator">The terminator of every line break of the input</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void FileWhoseEveryLineBreakIsTheSameOtherTerminatorGetsLineFeed(string terminator)
    {
        const string input = "class C\n{\n#pragma warning disable CS0169\n    /// <summary>\n    /// Gets the value.\n    /// </summary>\n    int x;\n#pragma warning restore CS0169\n}";

        AssertFormatted(input.Replace("\n", terminator), input);
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
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/>, verifies the output byte for byte, verifies
    /// that formatting the output again leaves it unchanged, and verifies that no phase run alone changes it (FMT-PIPE-027)
    /// </summary>
    /// <param name="input">The input text</param>
    /// <param name="expected">The expected output</param>
    private void AssertFormatted(string input, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var output = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Output;
        var actual = output.ToString();

        Assert.AreEqual(Escape(expected), Escape(actual));

        var secondPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(actual, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).Output.ToString();

        Assert.AreEqual(Escape(actual), Escape(secondPass), "The formatter is not idempotent.");

        PhaseStabilityCheck.AssertNoPhaseChanges(output);
    }

    /// <summary>
    /// Replaces every <c>\n</c> of the input and the expected output with the detected line ending and the placeholder with
    /// the foreign line break, then verifies the formatted output as <see cref="AssertFormatted"/> does
    /// </summary>
    /// <param name="input">The input text, written with <c>\n</c> and one placeholder</param>
    /// <param name="expected">The expected output, written with <c>\n</c>; a placeholder in it stands for the kept foreign line break</param>
    /// <param name="terminator">The foreign line break that replaces the placeholder</param>
    /// <param name="endOfLine">The detected line ending</param>
    private void AssertForeignLineBreakFormatted(string input, string expected, string terminator, string endOfLine)
    {
        AssertFormatted(input.Replace("\n", endOfLine).Replace(ForeignLineBreakPlaceholder, terminator),
                        expected.Replace("\n", endOfLine).Replace(ForeignLineBreakPlaceholder, terminator));
    }

    #endregion // Methods
}