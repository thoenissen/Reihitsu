using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for conditional (ternary) expression formatting (issue #310).
/// Every <c>?</c> and <c>:</c> of a multi-line conditional must be placed on its own line with
/// consistent indentation, nested conditionals are formatted the same way as the outer conditional,
/// and split null-conditional/member chains are rejoined into their canonical single-line form
/// </summary>
[TestClass]
public class ConditionalExpressionFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Canonical formatting that every variant of the issue #310 sample must normalize to
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
    /// the inner conditional aligning one indent deeper than the outer operator (issue #310 case 1)
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
    /// to <c>?.Length</c> while the nested conditional is aligned consistently (issue #310 case 2)
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
    /// before the conditional is aligned (issue #310 case 3)
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
    /// the inner conditional kept on a single line in the input is broken out (issue #310 case 4)
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
    /// one step deeper than the previous operator column (issue #310 case 5)
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
    /// question-mark operator column (issue #310 case 6)
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
    /// every operator is on its own line (issue #310 case 7)
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
    /// never broken across lines (issue #310 case 8 — guard against over-breaking)
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
    /// (issue #310 case 9 — idempotency guard)
    /// </summary>
    [TestMethod]
    public void CanonicalConditionalIsIdempotent()
    {
        // Act & Assert
        AssertRuleResult(Canonical);
    }

    /// <summary>
    /// Verifies that a trailing comment on a conditional branch is preserved and not joined into
    /// a comment when the operators are normalized (issue #310 case 10 — comment guard)
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

    #endregion // Methods
}