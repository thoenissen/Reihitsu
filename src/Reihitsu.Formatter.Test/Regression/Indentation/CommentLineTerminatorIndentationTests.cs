using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for comments that start their line while a multi-line comment or disabled text in the same leading
/// trivia holds a line terminator other than the file's line ending. Every C# line terminator ends a line, so the
/// comments are placed as when that terminator is <c>\n</c>, and the text of the comment or disabled text is kept
/// </summary>
[TestClass]
public class CommentLineTerminatorIndentationTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Placeholder for the line terminator a fixture places inside a comment or disabled text
    /// </summary>
    private const string TerminatorPlaceholder = "¤";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a block comment that starts its line takes the column of the code that follows it, whatever line
    /// terminator its text holds, and keeps that terminator
    /// </summary>
    /// <param name="terminator">The line terminator inside the comment</param>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void BlockCommentHoldingLineTerminatorTakesColumnOfFollowingCode(string terminator)
    {
        // Arrange
        var input = "class C\n{\n    void M()\n    {\n            /* a¤ b */\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);
        var expected = "class C\n{\n    void M()\n    {\n        /* a¤ b */\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);

        // Act & Assert
        AssertFormats(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment in front of a block comment that holds a line terminator, and the block comment
    /// itself, both take the column of the code that follows them
    /// </summary>
    /// <param name="terminator">The line terminator inside the block comment</param>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void OwnLineCommentAndBlockCommentHoldingLineTerminatorTakeColumnOfFollowingCode(string terminator)
    {
        // Arrange
        var input = "class C\n{\n    void M()\n    {\n            // first\n            /* a¤ b */\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);
        var expected = "class C\n{\n    void M()\n    {\n        // first\n        /* a¤ b */\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);

        // Act & Assert
        AssertFormats(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment in front of disabled text that holds a line terminator takes the column of the
    /// code that follows, and the disabled text is kept as written
    /// </summary>
    /// <param name="terminator">The line terminator inside the disabled text</param>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void OwnLineCommentBeforeDisabledTextHoldingLineTerminatorTakesColumnOfFollowingCode(string terminator)
    {
        // Arrange
        var input = "class C\n{\n    void M()\n    {\n            // first\n#if NEVER\n        a¤ b\n#endif\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);
        var expected = "class C\n{\n    void M()\n    {\n        // first\n#if NEVER\n        a¤ b\n#endif\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);

        // Act & Assert
        AssertFormats(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment behind disabled text that holds a line terminator takes the column of the code
    /// that follows
    /// </summary>
    /// <param name="terminator">The line terminator inside the disabled text</param>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void OwnLineCommentBehindDisabledTextHoldingLineTerminatorTakesColumnOfFollowingCode(string terminator)
    {
        // Arrange
        var input = "class C\n{\n    void M()\n    {\n#if NEVER\n        a¤ b\n#endif\n            // after\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);
        var expected = "class C\n{\n    void M()\n    {\n#if NEVER\n        a¤ b\n#endif\n        // after\n        var x = 1;\n    }\n}".Replace(TerminatorPlaceholder, terminator);

        // Act & Assert
        AssertFormats(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment and a block comment that holds a line terminator, both in front of the closing
    /// brace of a block, take the column of the block's statements
    /// </summary>
    /// <param name="terminator">The line terminator inside the block comment</param>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void CommentsBeforeClosingBraceWithBlockCommentHoldingLineTerminatorTakeStatementColumn(string terminator)
    {
        // Arrange
        var input = "class C\n{\n    void M()\n    {\n        var x = 1;\n\n            // first\n    /* a¤ b */\n    }\n}".Replace(TerminatorPlaceholder, terminator);
        var expected = "class C\n{\n    void M()\n    {\n        var x = 1;\n\n        // first\n        /* a¤ b */\n    }\n}".Replace(TerminatorPlaceholder, terminator);

        // Act & Assert
        AssertFormats(input, expected);
    }

    /// <summary>
    /// Verifies the issue's example as a whole file saved with a lone <c>\r</c> after every line: the block comment, whose
    /// text holds the file's own line terminator, takes the column of the code that follows it, as in the copy saved with
    /// <c>\n</c>, and formatting the result again changes nothing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void BlockCommentInLoneCarriageReturnFileTakesColumnOfFollowingCode()
    {
        // Arrange
        const string input = "class C\r{\r    void M()\r    {\r            /* a\r b */\r        var x = 1;\r    }\r}\r";
        const string expected = "class C\n{\n    void M()\n    {\n        /* a\r b */\n        var x = 1;\n    }\n}";

        // Act
        var firstPass = Format(input);
        var secondPass = Format(firstPass);

        // Assert
        Assert.AreEqual(Escape(expected), Escape(firstPass), "The comment should take the column of the code that follows it.");
        Assert.AreEqual(Escape(firstPass), Escape(secondPass), "Formatting the output again must change nothing.");
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
    /// Formats the input under LF and CRLF line endings, through <see cref="ReihitsuFormatter.FormatSyntaxTree"/>, and
    /// asserts the expected output and that formatting it again changes nothing. The line terminator inside a comment or
    /// disabled text is kept as it stands, so the output is compared as text rather than checked for one line ending
    /// </summary>
    /// <param name="input">The input, with <c>\n</c> for every line break the file's line ending replaces</param>
    /// <param name="expected">The expected output, with <c>\n</c> for every line break of the file's line ending</param>
    private void AssertFormats(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var firstPass = Format(NormalizeLineEndings(input, endOfLine));

            Assert.AreEqual(Escape(normalizedExpected), Escape(firstPass), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(Escape(firstPass), Escape(Format(firstPass)), $"Formatter is not idempotent under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the text through <see cref="ReihitsuFormatter.FormatSyntaxTree"/>
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The formatted text</returns>
    private string Format(string text)
    {
        return ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken)
                                .GetText(TestContext.CancellationToken)
                                .ToString();
    }

    #endregion // Methods
}