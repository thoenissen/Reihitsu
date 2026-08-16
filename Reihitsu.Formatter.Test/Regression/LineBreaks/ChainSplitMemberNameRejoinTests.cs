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
    /// Verifies that a block comment ending the dot's own line also keeps the split. Both this and
    /// <see cref="CommentBetweenDotAndMemberNameKeepsSplit"/> exercise the single
    /// <c>WouldJoinAcrossUnjoinableTrivia</c> call site that decides the refusal for every unjoinable
    /// trivia kind — comment, preprocessor directive, and disabled text alike — without branching per
    /// kind, so the directive and disabled-text arms are covered by that shared guard rather than by
    /// their own fixtures
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

    #endregion // Methods
}