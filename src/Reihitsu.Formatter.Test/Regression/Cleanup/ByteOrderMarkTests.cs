using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Cleanup;

/// <summary>
/// Full-pipeline tests for a byte order mark at the start of the text. The fixtures are strings that contain the byte
/// order mark character, because a file reader takes a leading byte order mark as the encoding signature and never
/// hands it to the formatter
/// </summary>
[TestClass]
public class ByteOrderMarkTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The byte order mark character
    /// </summary>
    private const string ByteOrderMark = "\uFEFF";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a byte order mark followed directly by a line break is kept while the blank first line is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeLineBreakIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "\nclass C{int x;}";
        const string expected = ByteOrderMark + "class C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark followed by several blank lines is kept while the blank lines are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeSeveralBlankLinesIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "\n\nclass C{int x;}";
        const string expected = ByteOrderMark + "class C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark followed by white space and a line break is kept while the white space and the
    /// blank first line are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeWhiteSpaceAndLineBreakIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + " \nclass C{int x;}";
        const string expected = ByteOrderMark + "class C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark on a line of its own before a leading comment is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeLineBreakAndCommentIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "\n// c\nclass C{}";
        const string expected = ByteOrderMark + "// c\nclass C;";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a text of nothing but a byte order mark and a line break keeps the byte order mark
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkAloneBeforeLineBreakIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "\n";
        const string expected = ByteOrderMark;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark followed by indentation on the line of the first token is kept while the
    /// indentation is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeIndentedFirstTokenIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "    class C{int x;}";
        const string expected = ByteOrderMark + "class C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark directly before a leading comment is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkBeforeCommentOnTheSameLineIsKept()
    {
        // Arrange
        const string input = ByteOrderMark + "// c\nclass C{int x;}";
        const string expected = ByteOrderMark + "// c\nclass C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment rebuilt at the start of a text that begins with a
    /// byte order mark keeps the byte order mark on its first line only
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-011")]
    [Requirement("FMT-CLEAN-009")]
    public void RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark()
    {
        // Arrange
        const string input = ByteOrderMark + "/// <summary>S</summary>\nclass C\n{\n    int A;\n}";
        const string expected = ByteOrderMark + "/// <summary>\n/// S\n/// </summary>\nclass C\n{\n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element rebuilt onto one line at the start of a text that begins with a byte order mark keeps
    /// the byte order mark on the first line only
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-011")]
    public void CollapsedDocumentationElementDoesNotRepeatTheByteOrderMark()
    {
        // Arrange
        const string input = ByteOrderMark + "/// <remarks>R\n/// </remarks>\nclass C\n{\n    int A;\n}";
        const string expected = ByteOrderMark + "/// <remarks>R</remarks>\nclass C\n{\n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark on a line of its own before an indented first token is kept and the
    /// result is stable on the second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    public void ByteOrderMarkBeforeLineBreakAndIndentedFirstTokenIsKeptInOnePass()
    {
        // Arrange
        const string input = ByteOrderMark + "\n    class C{int x;}";
        const string expected = ByteOrderMark + "class C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark before a blank line and an indented leading comment is kept and the result
    /// is stable on the second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    public void ByteOrderMarkBeforeBlankLineAndIndentedCommentIsKeptInOnePass()
    {
        // Arrange
        const string input = ByteOrderMark + "\n\n    // c\nclass C{int x;}";
        const string expected = ByteOrderMark + "// c\nclass C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark before an indented documentation comment is kept once, and the result is
    /// stable on the second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    [Requirement("FMT-DOC-011")]
    public void ByteOrderMarkBeforeIndentedDocumentationCommentIsKeptInOnePass()
    {
        // Arrange
        const string input = ByteOrderMark + "\n    /// <summary>S</summary>\n    class C{int x;}";
        const string expected = ByteOrderMark + "/// <summary>\n/// S\n/// </summary>\nclass C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a byte order mark before an indented using directive is kept and the result is stable on the
    /// second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    public void ByteOrderMarkBeforeIndentedUsingDirectiveIsKeptInOnePass()
    {
        // Arrange
        const string input = ByteOrderMark + "\n    using System;\n\nclass C{int x;}";
        const string expected = ByteOrderMark + "using System;\n\nclass C\n{\n    int x;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the boundary: the byte order mark character on a blank line in the middle of the
    /// text, before <c>else</c>, is not at the start of the text and is removed with the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    public void ByteOrderMarkCharacterOnBlankLineBeforeElseIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    void M(bool b)\n    {\n        if (b)\n        {\n        }\n" + ByteOrderMark + "\n        else\n        {\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(bool b)\n    {\n        if (b)\n        {\n        }\n        else\n        {\n        }\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the boundary: the byte order mark character on a blank line in the middle of the
    /// text, before <c>finally</c>, is removed with the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-PIPE-004")]
    public void ByteOrderMarkCharacterOnBlankLineBeforeFinallyIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        try\n        {\n        }\n" + ByteOrderMark + "\n        finally\n        {\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        try\n        {\n        }\n        finally\n        {\n        }\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the boundary: the byte order mark character on a blank line between a type
    /// declaration and its opening brace is removed with the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkCharacterOnBlankLineBeforeOpeningBraceIsRemoved()
    {
        // Arrange
        const string input = "class C\n" + ByteOrderMark + "\n{\n    int A;\n}";
        const string expected = "class C\n{\n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the boundary: the byte order mark character on a blank line before the closing
    /// brace of a type is removed with the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkCharacterOnBlankLineBeforeClosingBraceIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    int A;\n" + ByteOrderMark + "\n}";
        const string expected = "class C\n{\n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the boundary: the byte order mark character on a blank line between two
    /// statements is white space and is removed, while the blank line stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void ByteOrderMarkCharacterOnBlankLineBeforeStatementIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        int x = 0;\n" + ByteOrderMark + "\n        x++;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        int x = 0;\n\n        x++;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}