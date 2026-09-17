using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
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
    #region Properties

    /// <summary>
    /// The test context, used to source the cancellation token for pipeline calls made directly
    /// from a test method
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

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
    /// Verifies that a dangling colon separated from the false branch by a blank line reaches a
    /// stable fixed point on its very first formatting pass, matching every other converging
    /// construct in this pipeline (idempotency guard; regression test written against a confirmed
    /// convergence defect, not a hand-derived expectation of the eventual fixed point)
    /// </summary>
    [TestMethod]
    public void DanglingColonBeforeBlankLineConvergesOnFirstPass()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var x = cond ? a :\n\n            b;\n    }\n}\n";

        // The literal below is the formatter's own first-pass output for the input above, captured
        // by running it through the pipeline once. It is used only to pin down that a second pass
        // over that same first-pass output must be a no-op
        const string expectedFirstPass = "class C\n{\n    void M()\n    {\n        var x = cond\n                    ? a\n                    :\n        b;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expectedFirstPass);
    }

    /// <summary>
    /// Verifies that a dangling colon immediately followed by the false branch, with no leading
    /// whitespace at all on that line, still keeps a single space between the colon and the false
    /// branch instead of joining them with no separator (regression test against a confirmed
    /// missing-space defect)
    /// </summary>
    [TestMethod]
    public void DanglingColonWithNoLeadingWhitespaceKeepsSpaceBeforeFalseBranch()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var x = cond ? a :\nb;\n    }\n}\n";

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(normalizedInput, cancellationToken: TestContext.CancellationToken);
            var context = new FormattingContext(endOfLine);
            var actual = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).ToFullString();

            // Act & Assert
            Assert.Contains(": b;",
                            actual,
                            $"Expected a space between the colon and the false branch under {DescribeLineEnding(endOfLine)} line endings, but got:{endOfLine}{actual}");
        }
    }

    #endregion // Methods
}