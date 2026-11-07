using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.DocumentationComments;

/// <summary>
/// Full-pipeline tests for the documentation-comment behavior no narrower test pins: relocation behind an opening
/// brace, stranded comments, the exterior separator, the summary and element layouts, the content a rebuild keeps,
/// and delimited documentation comments
/// </summary>
[TestClass]
public class DocumentationCommentRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a documentation comment written behind the opening brace of a type moves onto its own line
    /// directly before the member it documents
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-BLANK-012")]
    public void DocumentationBehindOpeningBraceMovesOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             { /// <inheritdoc/>
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <inheritdoc/>
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment behind a statement, followed by the closing brace, stays on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void StrandedDocumentationBeforeClosingBraceStaysOnItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1; /// stray
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment followed by a semicolon stays where it was written,
    /// and the semicolon on the line behind it takes its declaration's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrandedDocumentationBeforeSemicolonStaysOnItsLineAndSemicolonTakesDeclarationColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A /// stray
                                     ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A /// stray
                                    ;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab separating <c>///</c> from its content is replaced by a single space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-005")]
    public void TabSeparatorAfterExteriorBecomesASpace()
    {
        // Arrange
        const string input = "class C\n{\n    ///\tGets the value.\n    int A;\n}";
        const string expected = """
                                class C
                                {
                                    /// Gets the value.
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace after the single separating space is kept as content indentation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-005")]
    public void ContentIndentationAfterTheSeparatorIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 ///    indented content
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the whitespace after <c>///</c> on a line without content is removed, while a single space is
    /// kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-006")]
    public void WhitespaceOnExteriorOnlyLineIsRemovedExceptASingleSpace()
    {
        // Arrange
        const string input = "class C\n{\n    /// <remarks>\n    /// First.\n    ///   \n    /// \n    /// Second.\n    /// </remarks>\n    int A;\n}";
        const string expected = "class C\n{\n    /// <remarks>\n    /// First.\n    ///\n    /// \n    /// Second.\n    /// </remarks>\n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a further <c>///</c> inside the content of a documentation line is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-007")]
    public void LaterSlashesInDocumentationContentAreKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// See http://example.com/// for details.
                                 /// </summary>
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a two-line summary whose end tag shares the content line is expanded to three lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-008")]
    public void TwoLineSummaryIsExpandedToThreeLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.</summary>
                                 int A;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element whose content starts beside its start tag and spans two lines is expanded, and one
    /// whose single content line starts beside its start tag is collapsed onto that line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-009")]
    public void HalfWrappedElementsAreExpandedOrCollapsed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 /// <remarks>First line.
                                 /// Second line.</remarks>
                                 int A;

                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 /// <remarks>Only line.
                                 /// </remarks>
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    /// <remarks>
                                    /// First line.
                                    /// Second line.
                                    /// </remarks>
                                    int A;

                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    /// <remarks>Only line.</remarks>
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line element, a block-form element, and a three-line summary are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-010")]
    public void CanonicalElementsAreLeftAlone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 /// <remarks>
                                 /// Already aligned.
                                 /// </remarks>
                                 /// <param name="x">One line.</param>
                                 void M(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that rebuilding an element keeps nested elements, rebuilding them in the same run, and keeps the
    /// indentation of code content
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-011")]
    public void RebuildKeepsNestedElementsAndCodeIndentation()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 /// <remarks>Use it like this:
                                 /// <code>if (x)
                                 ///     M();</code>
                                 /// </remarks>
                                 int A;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    /// <remarks>
                                    /// Use it like this:
                                    /// <code>
                                    /// if (x)
                                    ///     M();
                                    /// </code>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that rebuilding a summary keeps its inline XML and writes an empty interior content line as a bare
    /// <c>///</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-011")]
    public void RebuildKeepsInlineXmlAndParagraphBreak()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>Uses <see cref="C"/> and <c>A</c>.
                                 ///
                                 /// Second paragraph.</summary>
                                 int A;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Uses <see cref="C"/> and <c>A</c>.
                                    ///
                                    /// Second paragraph.
                                    /// </summary>
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the text of a delimited documentation comment with a one-line summary is not rewritten
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-012")]
    public void DelimitedDocumentationCommentIsNotRewritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /** <summary>Gets the value.</summary> */
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}