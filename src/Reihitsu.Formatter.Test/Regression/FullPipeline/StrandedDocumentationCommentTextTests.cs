using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for documentation comments written behind code that documents nothing: the comment stays where
/// it was written, and the text of every one of its lines is kept as written
/// </summary>
[TestClass]
public class StrandedDocumentationCommentTextTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a one-line summary behind the last field of a type keeps its text and its end tag
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-PIPE-002")]
    public void OneLineSummaryBehindLastFieldKeepsItsText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>end</summary>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example of a comment without a separator: no separator is inserted and no element is
    /// laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void MultiLineRemarksWithoutSeparatorBehindLastFieldIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; ///<remarks>a
                                 /// b</remarks>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a three-line summary behind the last field of a type keeps every line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void ThreeLineSummaryBehindLastFieldKeepsEveryLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>
                                 /// end
                                 /// </summary>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a two-line remarks element behind the last field of a type keeps its second line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-PIPE-002")]
    public void TwoLineRemarksBehindLastFieldKeepsItsSecondLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <remarks>a
                                 /// b</remarks>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a two-line summary behind the last field of a type is not expanded
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void TwoLineSummaryBehindLastFieldIsNotExpanded()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>
                                 /// end</summary>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a two-line remarks element behind an argument keeps its text, its continuation line included, while
    /// the closing parenthesis on the line behind it is aligned with the first argument as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void TwoLineRemarksBehindArgumentKeepsItsText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     M(1 /// <remarks>a
                                     /// b</remarks>
                                     );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        M(1 /// <remarks>a
                                        /// b</remarks>
                                          );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line summary behind the left operand of a binary expression keeps its text, while the
    /// operator on the line behind it is aligned as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void OneLineSummaryBeforeBinaryOperatorKeepsItsText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A = 1 /// <summary>end</summary>
                                     + 2;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A = 1 /// <summary>end</summary>
                                            + 2;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line summary before the dot of a member access keeps its text, while the dot on the line
    /// behind it is aligned behind its receiver as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void OneLineSummaryBeforeMemberAccessDotKeepsItsText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string S = "a" /// <summary>end</summary>
                                     .Trim();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string S = "a" /// <summary>end</summary>
                                               .Trim();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no separator is inserted into a one-line element behind the last field of a type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-DOC-005")]
    public void MissingSeparatorIsNotInsertedBehindLastField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; ///<remarks>a</remarks>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the continuation line of a comment behind the last field of a type keeps its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void MisindentedContinuationLineBehindLastFieldIsLeftAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <remarks>a
                               /// b</remarks>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the other side of the boundary: a one-line summary behind a field that is followed by another member is
    /// moved onto its own line and laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-004")]
    public void OneLineSummaryBehindFieldBeforeAnotherMemberIsMovedAndLaidOut()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>end</summary>
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /// <summary>
                                    /// end
                                    /// </summary>
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}