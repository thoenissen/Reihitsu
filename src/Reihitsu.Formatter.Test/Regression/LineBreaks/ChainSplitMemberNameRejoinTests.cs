using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A member-access dot immediately followed by a line break, with the member name on the next
/// line (<c>x.</c> ⏎ <c>Name</c>), must be rejoined. That break sits in the dot's trailing
/// trivia, which every other chain predicate ignores, so without this fix the split survives and
/// the orphaned name gets re-indented to block level
/// </summary>
[TestClass]
public class ChainSplitMemberNameRejoinTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a plain, non-conditional chain rejoins its split member name in the simplest
    /// case — a single split dot with no conditional access, directive, or comment involved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-020")]
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
    [Requirement("FMT-BREAK-020")]
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
    [Requirement("FMT-BREAK-020")]
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
    [Requirement("FMT-BREAK-020")]
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
    /// Verifies that a conditional-access chain rejoins the name onto its binding dot. The split is no wrap, so the first
    /// call stays on the root line and only the wrapped <c>.Then()</c> starts a continuation line under it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-020")]
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
                                        var x = a?.Prop.Call()
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
    [Requirement("FMT-BREAK-020")]
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
    /// Verifies that a line comment behind the dot refuses the input, so the join of the name onto its dot never meets the
    /// comment in the dot's trailing trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    public void CommentBetweenDotAndMemberNameIsUnsupported()
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
        AssertUnsupportedFor(LinkDotNameCommentReason, input, "// keep");
    }

    /// <summary>
    /// Verifies that a block comment ending the dot's own line also refuses the input. Together with
    /// <see cref="CommentBetweenDotAndMemberNameIsUnsupported"/> this covers a comment in the <b>dot's trailing</b> trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    public void BlockCommentBetweenDotAndMemberNameIsUnsupported()
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
        AssertUnsupportedFor(LinkDotNameCommentReason, input, "/* keep */");
    }

    /// <summary>
    /// Verifies that a comment on its own line before the member name refuses the input. This covers a comment in the
    /// <b>name token's leading</b> trivia, which the two trailing-trivia fixtures above never exercise
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    public void OwnLineCommentBeforeMemberNameIsUnsupported()
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

        // Act & Assert
        AssertUnsupportedFor(LinkDotNameCommentReason, input, "// keep");
    }

    /// <summary>
    /// Verifies that a preprocessor directive between the dot and its member name refuses the input in every build. The
    /// directive reaches the same name-token leading trivia as <see cref="OwnLineCommentBeforeMemberNameIsUnsupported"/>, with a
    /// trivia kind a comment-only check would miss
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void DirectiveBetweenDotAndMemberNameIsUnsupported(bool defineDebug)
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
        var parseOptions = defineDebug ? CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") : CSharpParseOptions.Default;

        // Act & Assert
        AssertUnsupportedFor(LinkDotNameCommentReason, input, "#if DEBUG", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that a member name split from its dot by a line break alone is joined back onto the dot in every enclosing
    /// construct, also outside a wrapped chain — an assignment target, a <c>nameof</c> argument, an interpolation hole, and a local
    /// initializer — so no member name of a link starts a line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-020")]
    public void SplitMemberNameRejoinsInEveryEnclosingConstruct()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 void A()
                                 {
                                     this.
                                         _x = 1;
                                 }

                                 void M(string t)
                                 {
                                     var n = nameof(C.
                                                    M);
                                     var s = $"{t.
                                                 Length}";
                                     var q = t.
                                         Length;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;

                                    void A()
                                    {
                                        this._x = 1;
                                    }

                                    void M(string t)
                                    {
                                        var n = nameof(C.M);
                                        var s = $"{t.Length}";
                                        var q = t.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first invoked link dot carries a comment directly above it must
    /// still rejoin a split member name elsewhere in the chain. The comment sits above
    /// <c>.Foo()</c>, nowhere near the <c>.Bar()</c> split, but the whole-chain bail for a
    /// commented first invoked link runs before the rejoin today
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
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a documentation comment above the first invoked link does not suppress the split
    /// member name rejoin
    /// </summary>
    [TestMethod]
    public void DocumentationCommentAboveFirstInvokedLinkDotStillRejoinsSplitMemberName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         /// keep wrapped
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

                                                /// keep wrapped
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a block comment on its own line above the first invoked link does not suppress the
    /// split member name rejoin
    /// </summary>
    [TestMethod]
    public void BlockCommentAboveFirstInvokedLinkDotStillRejoinsSplitMemberName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         /* keep wrapped */
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

                                                /* keep wrapped */
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}