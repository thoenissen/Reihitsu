using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for documentation comments that hold an XML element without an end tag of its own: the comment
/// is left as written, and the code that follows it stays code
/// </summary>
[TestClass]
public class DocumentationCommentWithoutOwnEndTagTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a summary without an end tag does not turn the field that follows it into comment text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    [Requirement("FMT-PIPE-003")]
    public void SummaryWithoutEndTagKeepsTheFollowingFieldAsCode()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a remarks element without an end tag after a complete one-line summary leaves the whole comment
    /// as written, including the summary, and keeps the field that follows it as code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    [Requirement("FMT-PIPE-003")]
    public void RemarksWithoutEndTagAfterCompleteSummaryLeavesTheCommentAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>Gets the value.</summary>
                                 /// <remarks>Text
                                 /// more
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: comments with an element without an end tag, with an end tag of another
    /// name, and with an end tag that differs only in case are all left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SpecificationExampleLeavesEveryCommentUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S
                                 int A;

                                 /// <summary>Gets the value.</summary>
                                 /// <remarks>Text
                                 /// more
                                 int B;

                                 /// <summary>S</returns>
                                 int D;

                                 /// <Returns>Nothing</returns>
                                 void E()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a summary closed by an end tag of another name is not laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryClosedByEndTagOfAnotherNameIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S</returns>
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that end-tag names are compared exactly, so a summary whose end tag differs only in case is not laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryClosedByEndTagDifferingOnlyInCaseIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <Summary>S</summary>
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the other side of the boundary: a one-line summary closed by its own end tag is still laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    [Requirement("FMT-DOC-008")]
    public void SummaryClosedByItsOwnEndTagIsStillLaidOut()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S</summary>
                                 int A;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// S
                                    /// </summary>
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a summary whose end tag is taken by a nested element leaves the comment as written and the field
    /// as code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryWhoseEndTagClosesANestedElementIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>A <see cref="C">x</summary>
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a nested element closed by an end tag of another name inside a complete summary leaves the whole
    /// comment as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void NestedElementClosedByEndTagOfAnotherNameLeavesTheCommentAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>a <see>b</x></summary>
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no separator is inserted after <c>///</c> in a comment with an element without an end tag
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    [Requirement("FMT-DOC-005")]
    public void MissingSeparatorIsNotInsertedInCommentWithoutEndTag()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ///<summary>S
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a one-line remarks element without an end tag after a complete three-line summary is not collapsed
    /// onto the following field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void OneLineRemarksWithoutEndTagAfterThreeLineSummaryIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// S
                                 /// </summary>
                                 /// <remarks>Text
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a one-line remarks element without an end tag whose line ends in white space is left as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void OneLineRemarksWithoutEndTagEndingInWhiteSpaceIsLeftAsWritten()
    {
        // Arrange
        const string input = "class C\n{\n    /// <summary>\n    /// S\n    /// </summary>\n    /// <remarks>a   \n    int A;\n}";

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the RH8107 example of an element closed by the end tag of the element around it: the comment, including
    /// its complete one-line summary, is left as written and the method stays code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void ElementClosedByEndTagOfTheEnclosingElementLeavesTheCommentAsWritten()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 /// <summary>Runs</summary>
                                 /// <remarks><returns>Nothing</remarks>
                                 public void Run()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment with a summary without an end tag before the closing brace of a type leaves the brace as
    /// code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryWithoutEndTagBeforeClosingBraceKeepsTheBraceAsCode()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A;

                                 /// <summary>S
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment with a summary without an end tag inside a method body keeps the following statement as
    /// code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryWithoutEndTagInsideMethodBodyKeepsTheStatementAsCode()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /// <summary>S
                                     int x = 1;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a summary without an end tag followed by an ordinary comment keeps the comment and the field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryWithoutEndTagFollowedByCommentKeepsTheCommentAndTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S
                                 // c
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a summary without an end tag followed by a conditional directive keeps the directive and the field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void SummaryWithoutEndTagFollowedByDirectiveKeepsTheDirectiveAndTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>S
                             #if DEBUG
                                 int A;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind code that holds a summary without an end tag is moved onto
    /// its own line before the member it documents, and its text is left as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-004")]
    public void RelocatedCommentWithoutEndTagIsMovedAndLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>S
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /// <summary>S
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a misindented comment with an element without an end tag is re-indented, while the text after the
    /// indentation of each line is left as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-013")]
    public void MisindentedCommentWithoutEndTagIsReindentedOnly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                       /// <remarks>
                                       /// Text
                                 int A;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <remarks>
                                    /// Text
                                    int A;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}