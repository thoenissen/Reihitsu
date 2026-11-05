using System;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.DocumentationComments;

/// <summary>
/// Tests that <see cref="DocumentationCommentFormattingPhase"/>, run on its own, decides where a delimited documentation
/// comment written behind code starts its line: it starts a new line in front of every such comment that is moved
/// today, removes the whitespace in front of it, keeps the code behind it and its text, inserts no blank line, and
/// leaves every delimited comment that stays today where it was written. The column of the moved comment belongs to
/// the indentation phase, so the line comparison ignores the whitespace at the start of each line
/// </summary>
[TestClass]
public class DelimitedDocumentationRelocationPhaseTests
{
    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind a field, with the documented field behind it on its line,
    /// starts a new line and keeps the documented field behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindFieldWithCodeBehindIt()
    {
        const string input = "class C\n{\n    int A; /** doc */ int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** doc */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that ends the line of a field starts a new line in front of the
    /// documented field's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationEndingFieldLine()
    {
        const string input = "class C\n{\n    int A; /** doc */\n    int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** doc */\n    int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of an enum member: the delimited documentation comment behind the first member
    /// starts a new line in front of the second member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindEnumMember()
    {
        const string input = "enum E\n{\n    First, /** doc */\n    Second\n}";
        const string expected = "enum E\n{\n    First,\n    /** doc */\n    Second\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of a comment that trails the code: the delimited documentation comment behind
    /// it starts a new line, and the block comment stays behind the field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindTrailingBlockComment()
    {
        const string input = "class C\n{\n    int A; /* c */ /** doc */ int B;\n}";
        const string expected = "class C\n{\n    int A; /* c */\n    /** doc */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a multi-line block comment that trails the field and ends
    /// on the comment's line starts a new line, and the block comment keeps its text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindMultiLineTrailingBlockComment()
    {
        const string input = "class C\n{\n    int A; /* a\n       b */ /** doc */ int B;\n}";
        const string expected = "class C\n{\n    int A; /* a\n       b */\n    /** doc */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written directly behind the semicolon of a field, with no
    /// whitespace in between, starts a new line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationWrittenDirectlyBehindField()
    {
        const string input = "class C\n{\n    int A;/** doc */ int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** doc */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that only the first of two delimited documentation comments behind a field starts a new line, and the
    /// second stays behind the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineOnlyInFrontOfFirstOfTwoDelimitedDocumentationComments()
    {
        const string input = "class C\n{\n    int A; /** a */ /** b */ int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** a */ /** b */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind a moved delimited documentation comment stays behind it on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void KeepsBlockCommentBehindMovedDelimitedDocumentation()
    {
        const string input = "class C\n{\n    int A; /** d */ /* c */ int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** d */ /* c */ int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by a region directive starts a new line, and the directive
    /// stays on its own line behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationFollowedByRegionDirective()
    {
        const string input = "class C\n{\n    int A; /** d */\n#region Values\n    int B;\n#endregion // Values\n}";
        const string expected = "class C\n{\n    int A;\n    /** d */\n#region Values\n    int B;\n#endregion // Values\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by an active conditional section starts a new line, and
    /// the directives stay where they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void StartsNewLineInFrontOfDelimitedDocumentationFollowedByActiveConditionalSection()
    {
        const string input = "class C\n{\n    int A; /** d */\n#if !DEBUG\n    int B;\n#endif\n}";
        const string expected = "class C\n{\n    int A;\n    /** d */\n#if !DEBUG\n    int B;\n#endif\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a moved delimited documentation comment whose text spans several lines keeps every character of its
    /// text, including the whitespace at the start of its continuation lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-012")]
    public void KeepsTextOfMovedMultiLineDelimitedDocumentation()
    {
        const string comment = "/** <summary>\n           Gets B.\n           </summary> */";
        const string input = "class C\n{\n    int A; " + comment + "\n    int B;\n}";
        const string expected = "class C\n{\n    int A;\n    " + comment + "\n    int B;\n}";

        AssertMoves(input, expected);

        foreach (var endOfLine in _lineEndings)
        {
            var actual = Format(Normalize(input, endOfLine), endOfLine);

            Assert.Contains(Normalize(comment, endOfLine), actual, $"The text of the moved comment must stay unchanged under {Describe(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by a single-line documentation comment starts a new line,
    /// and that the single-line comment behind it starts a line of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-002")]
    public void StartsNewLinesInFrontOfDelimitedAndFollowingSingleLineDocumentation()
    {
        const string input = "class C\n{\n    int A; /** a */ /// b\n    int B;\n}";
        const string expected = "class C\n{\n    int A;\n    /** a */\n    /// b\n    int B;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement, before the next statement, starts a new line,
    /// as it does today; no requirement decides this position yet
    /// </summary>
    [TestMethod]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindStatement()
    {
        const string input = "class C\n{\n    void M(int a)\n    {\n        a = 1; /** d */ a = 2;\n    }\n}";
        const string expected = "class C\n{\n    void M(int a)\n    {\n        a = 1;\n        /** d */ a = 2;\n    }\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a using directive, before the next using directive, starts
    /// a new line, as it does today; no requirement decides this position yet
    /// </summary>
    [TestMethod]
    public void StartsNewLineInFrontOfDelimitedDocumentationBehindUsingDirective()
    {
        const string input = "using System; /** doc */\nusing System.IO;\n\nclass C\n{\n    int A;\n}";
        const string expected = "using System;\n/** doc */\nusing System.IO;\n\nclass C\n{\n    int A;\n}";

        AssertMoves(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement and before the closing brace, which documents
    /// nothing, stays where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void KeepsStrandedDelimitedDocumentationBeforeClosingBrace()
    {
        AssertUnchanged("class C\n{\n    void M()\n    {\n        var x = 1; /** stray */\n    }\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment before the dot of a member access, which documents nothing, stays
    /// where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void KeepsStrandedDelimitedDocumentationBeforeMemberAccessDot()
    {
        AssertUnchanged("class C\n{\n    string M(string s)\n    {\n        return s.Trim() /** d */ .ToString();\n    }\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment whose following member is disabled text, so that the closing brace
    /// follows it, stays where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void KeepsStrandedDelimitedDocumentationFollowedByInactiveConditionalSection()
    {
        AssertUnchanged("class C\n{\n    int A; /** d */\n#if DEBUG\n    int B;\n#endif\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that ends the line of an opening brace stays where it is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void KeepsDelimitedDocumentationEndingOpeningBraceLine()
    {
        AssertUnchanged("class C\n{ /** doc */\n    void M()\n    {\n    }\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind an opening brace with code behind it on the brace's line is
    /// not moved by the documentation-comment phase
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindOpeningBraceWithCodeBehindIt()
    {
        AssertUnchanged("class C { /** d */ void M() { } }");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a case label stays where it is
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindCaseLabel()
    {
        AssertUnchanged("class C\n{\n    void M(int v)\n    {\n        switch (v)\n        {\n            case 1: /** d */\n                M(2);\n                break;\n        }\n    }\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment on a line of its own stays behind that
    /// comment
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindBlockCommentOnItsOwnLine()
    {
        AssertUnchanged("class C\n{\n    int A;\n\n    /* c */ /** d */\n    int B;\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a banner comment at the start of the file stays behind it
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindBannerCommentAtFileStart()
    {
        AssertUnchanged("/* banner */ /** doc */\nclass C\n{\n    int A;\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that already starts its line is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void KeepsDelimitedDocumentationThatAlreadyStartsItsLine()
    {
        AssertUnchanged("class C\n{\n    int A;\n\n    /** doc */ int B;\n}");
    }

    /// <summary>
    /// Asserts that every line break of the text is the given line ending
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="endOfLine">The line ending</param>
    private static void AssertUsesOnly(string text, string endOfLine)
    {
        var rest = text.Replace(endOfLine, string.Empty);

        Assert.IsFalse(rest.Contains('\n') || rest.Contains('\r'), $"Every line break must be {Describe(endOfLine)}.");
    }

    /// <summary>
    /// Removes the whitespace at the start of every line, keeping the line structure and the whitespace at line ends
    /// </summary>
    /// <param name="text">The text with LF line endings</param>
    /// <returns>The text without leading whitespace on any line</returns>
    private static string TrimLineStarts(string text)
    {
        return string.Join("\n", text.Split('\n').Select(line => line.TrimStart(' ', '\t')));
    }

    /// <summary>
    /// Rewrites every line break of the text to the given line ending
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="endOfLine">The line ending</param>
    /// <returns>The normalized text</returns>
    private static string Normalize(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Names the given line ending
    /// </summary>
    /// <param name="endOfLine">The line ending</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    private static string Describe(string endOfLine)
    {
        return endOfLine == "\n" ? "LF" : "CRLF";
    }

    /// <summary>
    /// Runs the phase under every line ending and asserts the expected lines, compared without the whitespace at the start
    /// of each line, and the line ending of every line break
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    /// <param name="expectedWithLf">The expected output with LF line endings</param>
    private void AssertMoves(string inputWithLf, string expectedWithLf)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var actual = Format(Normalize(inputWithLf, endOfLine), endOfLine);

            AssertUsesOnly(actual, endOfLine);
            Assert.AreEqual(TrimLineStarts(expectedWithLf), TrimLineStarts(actual.Replace("\r\n", "\n")), $"Unexpected phase output under {Describe(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the phase under every line ending and asserts that it changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    private void AssertUnchanged(string inputWithLf)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = Normalize(inputWithLf, endOfLine);

            Assert.AreEqual(input, Format(input, endOfLine), $"The phase must leave the input unchanged under {Describe(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the documentation-comment phase over the given source
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <returns>The phase output</returns>
    private string Format(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}