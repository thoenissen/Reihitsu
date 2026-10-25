using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for a region directive that is the first content after an opening brace: no
/// blank line follows the brace, and a comment or directive between the brace and the region does not
/// stand in for the blank line above the region
/// </summary>
[TestClass]
public class RegionDirectlyAfterOpeningBraceTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported shape: an empty region directly after the opening brace of a type body gets
    /// no blank line after the brace, and the blank line between the two directives is inserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("SHARED-BLANK-006")]
    public void EmptyRegionAfterTypeBodyBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region A
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region A

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of a type body that already has
    /// the blank line between its directives is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void FormattedEmptyRegionAfterTypeBodyBraceIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region A

                                 #endregion // A
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of a method body gets no blank
    /// line after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterMethodBodyBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region A

                                     #endregion // A
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of a block-scoped namespace gets
    /// no blank line after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterNamespaceBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                                 #region A

                                 #endregion // A
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of an enum gets no blank line
    /// after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterEnumBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             enum E
                             {
                                 #region A

                                 #endregion // A
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of a statement block gets no
    /// blank line after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterStatementBlockBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     if (b)
                                     {
                                         #region A

                                         #endregion // A
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of a switch statement gets no
    /// blank line after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterSwitchStatementBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         #region A

                                         #endregion // A
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty region directly after the opening brace of an accessor body gets no blank
    /// line after the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionAfterAccessorBodyBraceGetsNoBlankLineAfterTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                 {
                                     set
                                     {
                                         #region A

                                         #endregion // A
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specified comment variant: a comment that is the first content after the opening
    /// brace gets no blank line above it, but a blank line between it and the region below
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void CommentFirstAfterBraceIsSeparatedFromEmptyRegionBelow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // c
                                 #region A
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // c

                                    #region A

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment first after the brace is separated from a region that holds a member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void CommentFirstAfterBraceIsSeparatedFromRegionHoldingMember()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // c
                                 #region A

                                 int _x;

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // c

                                    #region A

                                    int _x;

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment first after a method body brace is separated from a region that holds a
    /// statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void CommentFirstAfterMethodBraceIsSeparatedFromRegionHoldingStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // c
                                     #region A

                                     var v = 1;

                                     #endregion // A
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        // c

                                        #region A

                                        var v = 1;

                                        #endregion // A
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that only the comment directly above the region counts: two comments first after the
    /// brace stay together, and the second one is separated from the region
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void TwoCommentsFirstAfterBraceAreSeparatedFromRegionBelow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // a
                                 // b
                                 #region A

                                 int _x;

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // a
                                    // b

                                    #region A

                                    int _x;

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment first after the brace is separated from the empty region below
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void DelimitedCommentFirstAfterBraceIsSeparatedFromEmptyRegionBelow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /* c */
                                 #region A
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /* c */

                                    #region A

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive first after the brace is separated from the empty region
    /// below, and no blank line follows the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("FMT-BLANK-007")]
    [Requirement("SHARED-BLANK-005")]
    public void PragmaFirstAfterBraceIsSeparatedFromEmptyRegionBelow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #pragma warning disable CS0168
                                 #region A
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #pragma warning disable CS0168

                                    #region A

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an opening brace whose line carries a trailing comment is not exempt, so the empty
    /// region below it gets its blank line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-005")]
    public void EmptyRegionBelowBraceWithTrailingCommentGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             { // x
                                 #region A
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                { // x

                                    #region A

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty region inside an inactive conditional block directly after the opening brace
    /// is neither padded nor separated from the brace
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-007")]
    [Requirement("FMT-BLANK-007")]
    public void InactiveEmptyRegionDirectlyAfterOpeningBraceIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if NEVER
                                 #region R
                                 #endregion // R
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that conditional directives directly inside braces are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-007")]
    public void ConditionalDirectivesDirectlyInsideBracesAreUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if X
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}