using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for conditional (ternary) expression formatting.
/// Every <c>?</c> and <c>:</c> of a multi-line conditional must be placed on its own line with
/// consistent indentation, nested conditionals are formatted the same way as the outer conditional,
/// and split null-conditional/member chains are rejoined into their canonical single-line form
/// </summary>
[TestClass]
public class ConditionalExpressionFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Canonical formatting that every variant of the conditional-expression fixtures below must
    /// normalize to
    /// </summary>
    private const string Canonical = """
                                     class C
                                     {
                                         void M()
                                         {
                                             var outer = "123";
                                             var inner = 1;
                                             var title = outer?.Substring(0, 0).Length == 0
                                                             ? "A"
                                                             : inner == 1
                                                                 ? "B"
                                                                 : "C";
                                         }
                                     }
                                     """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a single-line nested conditional is broken with consistent indentation,
    /// the inner conditional aligning one indent deeper than the outer operator
    /// </summary>
    [TestMethod]
    public void SingleLineNestedConditionalGetsConsistentIndentation()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var outer = "123";
                                     var inner = 1;
                                     var title = outer?.Substring(0, 0).Length == 0 ? "A" : inner == 1 ? "B" : "C";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, Canonical);
    }

    /// <summary>
    /// Verifies that an oddly spaced null-conditional access (<c>? .Length</c>) is collapsed
    /// to <c>?.Length</c> while the nested conditional is aligned consistently
    /// </summary>
    [TestMethod]
    public void OddlySpacedConditionalAccessIsCollapsed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var outer = "123";
                                     var inner = 1;
                                     var title = outer? .Length == 0 ? "A" : inner == 1 ? "B" : "C";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var outer = "123";
                                        var inner = 1;
                                        var title = outer?.Length == 0
                                                        ? "A"
                                                        : inner == 1
                                                            ? "B"
                                                            : "C";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a member access split across lines is rejoined onto a single line
    /// before the conditional is aligned
    /// </summary>
    [TestMethod]
    public void SplitMemberAccessIsRejoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var outer = "123";
                                     var inner = 1;
                                     var title = outer
                                         .Length == 0
                                                     ? "A"
                                                     : inner == 1
                                                           ? "B"
                                                           : "C";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var outer = "123";
                                        var inner = 1;
                                        var title = outer.Length == 0
                                                        ? "A"
                                                        : inner == 1
                                                            ? "B"
                                                            : "C";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a split null-conditional chain with a dangling <c>?.</c> is rejoined and
    /// the inner conditional kept on a single line in the input is broken out
    /// </summary>
    [TestMethod]
    public void SplitConditionalAccessChainWithInlineNestedConditionalIsNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var outer = "123";
                                     var inner = 1;
                                     var title = outer?.
                                         Substring(0, 0).
                                         Length == 0
                                         ? "A"
                                         : inner == 1 ? "B" : "C";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, Canonical);
    }

    /// <summary>
    /// Verifies that a three-level nested conditional in the false branch indents each level
    /// one step deeper than the previous operator column
    /// </summary>
    [TestMethod]
    public void TripleNestedConditionalInFalseBranchIndentsConsistently()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0 ? "1" : inner == 1 ? "2" : inner == 2 ? "3" : "4";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int inner)
                                    {
                                        var title = inner == 0
                                                        ? "1"
                                                        : inner == 1
                                                            ? "2"
                                                            : inner == 2
                                                                ? "3"
                                                                : "4";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional nested in the true branch indents relative to the parent
    /// question-mark operator column
    /// </summary>
    [TestMethod]
    public void NestedConditionalInTrueBranchIndentsRelativeToParentOperator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0 ? inner == 1 ? "a" : "b" : "c";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int inner)
                                    {
                                        var title = inner == 0
                                                        ? inner == 1
                                                            ? "a"
                                                            : "b"
                                                        : "c";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a short nested conditional that fits on a single line is still broken so that
    /// every operator is on its own line
    /// </summary>
    [TestMethod]
    public void ShortNestedConditionalIsBrokenOntoOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0 ? "a" : inner == 1 ? "b" : "c";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int inner)
                                    {
                                        var title = inner == 0
                                                        ? "a"
                                                        : inner == 1
                                                            ? "b"
                                                            : "c";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a simple single-line conditional without nesting is left untouched and is
    /// never broken across lines (guard against over-breaking)
    /// </summary>
    [TestMethod]
    public void SimpleSingleLineConditionalIsNotBroken()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0 ? "a" : "b";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the canonical conditional formatting is stable when formatted again
    /// (idempotency guard)
    /// </summary>
    [TestMethod]
    public void CanonicalConditionalIsIdempotent()
    {
        // Act & Assert
        AssertRuleResult(Canonical);
    }

    /// <summary>
    /// Verifies that a trailing comment on a conditional branch is preserved and not joined into
    /// a comment when the operators are normalized (comment guard)
    /// </summary>
    [TestMethod]
    public void ConditionalBranchTrailingCommentIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0
                                         ? "a" // note
                                         : "b";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int inner)
                                    {
                                        var title = inner == 0
                                                        ? "a" // note
                                                        : "b";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a dangling colon separated from the false branch by a blank line is joined onto
    /// the colon's line in one pass, mirroring how the <c>?</c> join already collapses a blank line
    /// before the true branch (regression test for a confirmed convergence defect: the pre-fix
    /// formatter needed three passes to reach this same fixed point)
    /// </summary>
    [TestMethod]
    public void DanglingColonWithBlankLineBeforeFalseBranchIsJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :

                                         b;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = cond
                                                    ? a
                                                    : b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a dangling colon followed by the false branch on the very next line, with no
    /// intervening blank line, is joined onto the colon's line in one pass instead of leaving the
    /// false branch's indentation as residual whitespace after the colon (regression test for a
    /// confirmed convergence defect that needed a second pass to clean up the residual whitespace)
    /// </summary>
    [TestMethod]
    public void DanglingColonWithNoBlankLineBeforeFalseBranchIsJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :
                                         b;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = cond
                                                    ? a
                                                    : b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a dangling colon whose false branch starts at column 0, with no leading
    /// whitespace at all, still gets exactly one space inserted before the false branch instead of
    /// joining them with no separator (regression test for a confirmed missing-space defect; unlike
    /// the other dangling-colon shapes this one converged immediately, to permanently wrong output)
    /// </summary>
    [TestMethod]
    public void DanglingColonWithUnindentedFalseBranchGetsSingleSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :
                             b;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = cond
                                                    ? a
                                                    : b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a dangling colon is not joined across a comment that sits between the colon and
    /// the false branch, because the comment would otherwise absorb the joined token (boundary test:
    /// the join guard already inspects this span and must keep refusing it after the colon join gained
    /// the ability to rewrite the false branch's leading trivia)
    /// </summary>
    [TestMethod]
    public void DanglingColonBeforeCommentIsNotJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :
                                         // note
                                         b;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = cond
                                                    ? a :

                                        // note
                                        b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a dangling colon is not joined across a preprocessor directive spanning the
    /// false branch, because removing the colon's trailing end-of-line would move the directive off
    /// the start of a line (boundary test, same guard as the comment case above)
    /// </summary>
    [TestMethod]
    public void DanglingColonBeforeDirectiveIsNotJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :
                             #if DEBUG
                                         b;
                             #else
                                         c;
                             #endif
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = cond
                                                    ? a :
                                #if DEBUG
                                            b;
                                #else
                                        c;
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a hand-written single-line conditional missing the space after the colon is left
    /// untouched (control: proves the missing-space fix lives in the dangling-colon join and did not
    /// leak into the horizontal-spacing policy for ternary colons, which deliberately does not cover
    /// them)
    /// </summary>
    [TestMethod]
    public void SingleLineConditionalWithoutColonSpaceIsNotRewritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond ? a :b;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a dangling colon before a nested conditional in the false branch is joined at
    /// every nesting level in one pass, not only at the outermost level
    /// </summary>
    [TestMethod]
    public void DanglingColonInNestedFalseBranchIsJoinedAtEveryLevel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int inner)
                                 {
                                     var title = inner == 0 ? "a" : inner == 1 ? "b" :

                                         "c";
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int inner)
                                    {
                                        var title = inner == 0
                                                        ? "a"
                                                        : inner == 1
                                                            ? "b"
                                                            : "c";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}