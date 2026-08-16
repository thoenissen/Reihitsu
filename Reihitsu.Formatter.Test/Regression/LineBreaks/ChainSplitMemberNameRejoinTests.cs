using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #685: a member-access dot immediately followed by a line break, with
/// the member name on the next line (<c>x.</c> ⏎ <c>Name</c>), must be rejoined. That break sits in
/// the dot's trailing trivia, which every other chain predicate ignores, so the split survived and
/// the orphaned name was re-indented to block level
/// </summary>
[TestClass]
public class ChainSplitMemberNameRejoinTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a plain, non-conditional chain rejoins its split member name — the first fixture
    /// reported in issue #685
    /// </summary>
    [TestMethod]
    public void SplitMemberNameRejoinsOntoItsDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a.
                                         Prop.Call();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a1 = a.Prop.Call();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that rejoining the split preserves a deliberate wrap further along the chain: the
    /// name joins its dot while the wrapped <c>.Trim()</c> link keeps its own line and aligns to the
    /// first invoked link
    /// </summary>
    [TestMethod]
    public void SplitMemberNameRejoinsAndKeepsLaterWrappedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.
                                         Prop?.ToString()
                                         .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop?.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a split whose chain is kept wrapped by a multi-line argument list still rejoins
    /// the member name, and that the argument wrap itself survives
    /// </summary>
    [TestMethod]
    public void SplitMemberNameRejoinsWithMultiLineArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.
                                         Call(1,
                                             2);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Call(1,
                                                       2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every split at a chain-link boundary rejoins, not only the first one
    /// </summary>
    [TestMethod]
    public void EverySplitMemberNameRejoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var v = a.
                                         Prop.
                                         Call(1,
                                             2);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = a.Prop.Call(1,
                                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional-access chain rejoins the name onto its binding dot. The break
    /// before <c>.Call()</c> is not the split being repaired: it is the wrap RH5201 requires once the
    /// chain wraps at all, so the invoked links end up on their own lines at one column
    /// </summary>
    [TestMethod]
    public void SplitMemberNameAfterConditionalAccessRejoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a?.
                                         Prop.Call()
                                         .Then();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a?.Prop
                                                 .Call()
                                                 .Then();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain carrying no invocation at all — reached through the member-access
    /// rewriter rather than the chain normalizer — rejoins its split member name too
    /// </summary>
    [TestMethod]
    public void SplitMemberNameRejoinsOnMemberAccessOnlyChain()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var v = a.
                                         B.C;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = a.B.C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between the dot and its member name keeps the split: rejoining across
    /// it would absorb the name into the comment
    /// </summary>
    [TestMethod]
    public void CommentBetweenDotAndMemberNameKeepsSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a. // keep
                                     Prop.Call();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment ending the dot's own line also keeps the split. Together with
    /// <see cref="CommentBetweenDotAndMemberNameKeepsSplit"/> this covers the refusal predicate's
    /// first disjunct, which inspects the <b>dot's trailing</b> trivia
    /// </summary>
    [TestMethod]
    public void BlockCommentBetweenDotAndMemberNameKeepsSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a. /* keep */
                                     Prop.Call();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line before the member name keeps the split. This covers
    /// the refusal predicate's <b>second</b> disjunct, which inspects the <b>name token's leading</b>
    /// trivia — the only one a directive or an own-line comment can reach, and the one the two
    /// trailing-trivia fixtures above never exercise. The blank line in the expected output is
    /// pre-existing behavior of the surrounding phases and is not what this test guards
    /// </summary>
    [TestMethod]
    public void OwnLineCommentBeforeMemberNameKeepsSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a.
                                         // keep
                                         Prop.Call();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a1 = a.

                                        // keep
                                        Prop.Call();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a preprocessor directive between the dot and its member name keeps the split:
    /// removing the end-of-line that terminates the directive's line would invalidate it. The
    /// directive reaches the same name-token leading-trivia disjunct as
    /// <see cref="OwnLineCommentBeforeMemberNameKeepsSplit"/>, with a trivia kind a comment-only
    /// guard would miss. The indentation of the inactive branch is pre-existing behavior of the
    /// surrounding phases and is not what this test guards
    /// </summary>
    [TestMethod]
    public void DirectiveBetweenDotAndMemberNameKeepsSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a.
                             #if DEBUG
                                         Prop.Call();
                             #else
                                         Other.Call();
                             #endif
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a1 = a.
                                #if DEBUG
                                            Prop.Call();
                                #else
                                        Other.Call();
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the remaining arm from issue #685's follow-up comment: a chain whose first invoked
    /// link dot carries a comment directly above it must still rejoin a split member name elsewhere
    /// in the chain. The comment sits above <c>.Foo()</c>, nowhere near the <c>.Bar()</c> split, but
    /// the whole-chain bail for a commented first invoked link runs before the rejoin today
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstInvokedLinkDotStillRejoinsLaterSplitMemberName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Foo().
                                         Bar()
                                         .Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Foo().Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}