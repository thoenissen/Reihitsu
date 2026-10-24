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
    private const string ByteOrderMark = "﻿";

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

    #endregion // Methods
}