using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — conditional access chains whose
/// source wraps between the <c>?</c> and the <c>.</c> or <c>[</c> of the operator. A conditional link is laid
/// out exactly like the same link written with a plain <c>.</c>: <c>?.</c> and <c>?[</c> stay together, and when
/// a comment, directive, or disabled text keeps them apart, the binding token is aligned under its <c>?</c>
/// </summary>
[TestClass]
public class ConditionalAccessWrapBetweenOperatorTokensTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a chain wrapped between <c>?</c> and <c>.</c> on every link moves the <c>?</c> in front of
    /// the break so each conditional link starts its own aligned line with <c>?.</c>, exactly like the plain chain
    /// </summary>
    [TestMethod]
    public void ChainWrappedBetweenQuestionMarkAndDotStartsEachLinkWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)?
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?.Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that an identifier-rooted chain wrapped inside every operator joins its first link onto the
    /// root, the same way the plain chain does, while each later link starts its own line with <c>?.</c>
    /// </summary>
    [TestMethod]
    public void ChainWithIdentifierRootJoinsFirstLinkOntoRootAndStartsLaterLinksWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list?
                                                .Select(o => o)?
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list?.Select(o => o)
                                                   ?.Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that a single conditional link wrapped inside the operator joins back onto its identifier root
    /// </summary>
    [TestMethod]
    public void SingleLinkWithIdentifierRootJoinsOntoRoot()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a?
                                             .B();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a?.B();
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that an identifier-rooted chain whose first conditional link is wrapped inside the operator
    /// joins that link onto the root and aligns the following plain link under the <c>?</c>
    /// </summary>
    [TestMethod]
    public void IdentifierRootWithPlainContinuationKeepsLaterLinkAlignedUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return obj?
                                                .Method1()
                                                .Method2();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return obj?.Method1()
                                                  .Method2();
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that a two-link chain wrapped inside the operator of its conditional link keeps the wrap,
    /// with <c>?.</c> leading the continuation line, exactly like the plain chain keeps its wrapped link
    /// </summary>
    [TestMethod]
    public void TwoLinkMixedChainKeepsWrapWithConditionalAccessLeadingContinuationLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B()?
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B()
                                                ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that an unwrapped mixed chain is left on its single line
    /// </summary>
    [TestMethod]
    public void SingleLineMixedChainStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B()?.C();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a nested conditional link wrapped inside its operator starts its own line with <c>?.</c>
    /// </summary>
    [TestMethod]
    public void NestedConditionalLinkWrappedInsideOperatorStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a?.B()?
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a?.B()
                                                ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that a nested conditional link already wrapped before its <c>?</c> keeps its layout
    /// </summary>
    [TestMethod]
    public void NestedConditionalLinkWrappedBeforeOperatorStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a?.B()
                                             ?.C();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional element access wrapped between <c>?</c> and <c>[</c> starts its own aligned
    /// line with <c>?[</c>, the same way a conditional member access does
    /// </summary>
    [TestMethod]
    public void ElementBindingInInvokedChainStartsItsOwnLineWithConditionalElementAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B()?
                                             [0]?
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B()
                                                ?[0]
                                                ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single conditional element access wrapped between <c>?</c> and <c>[</c> joins back onto
    /// its identifier root in one pass
    /// </summary>
    [TestMethod]
    public void SingleElementBindingWithIdentifierRootJoinsOntoRootInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a?
                                             [0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a?[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked conditional link wrapped inside its operator starts its own line with <c>?.</c>
    /// </summary>
    [TestMethod]
    public void MemberAccessOnlyChainStartsConditionalLinkWithQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B.C?
                                             .D;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B.C
                                                ?.D;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain inside an argument list wrapped inside every operator lays out each conditional
    /// link like the plain chain in the same position
    /// </summary>
    [TestMethod]
    public void ChainInsideArgumentStartsEachLinkWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     Use(list.Select(o => o)?
                                             .Select(o => o)?
                                             .Select(o => o));

                                     return null;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        Use(list.Select(o => o)
                                                ?.Select(o => o)
                                                ?.Select(o => o));

                                        return null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that a chain in an expression-bodied lambda wrapped inside every operator lays out each
    /// conditional link like the plain chain in the same position
    /// </summary>
    [TestMethod]
    public void ChainInsideLambdaBodyStartsEachLinkWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     Func<object> f = () => list.Select(o => o)?
                                                                .Select(o => o)?
                                                                .Select(o => o);

                                     return f;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        Func<object> f = () => list.Select(o => o)
                                                                   ?.Select(o => o)
                                                                   ?.Select(o => o);

                                        return f;
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that a chain used as the left operand of <c>??</c> lays out each conditional link with <c>?.</c>
    /// leading its line
    /// </summary>
    [TestMethod]
    public void ChainAsCoalesceOperandStartsEachLinkWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)?
                                                .Select(o => o)?
                                                .Select(o => o) ?? a;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?.Select(o => o)
                                                   ?.Select(o => o) ?? a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace between a trailing <c>?</c> and its line break does not survive the move
    /// </summary>
    [TestMethod]
    public void TrailingWhitespaceAfterQuestionMarkIsRemovedWhenLinkMoves()
    {
        // Arrange
        const string template = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)?{whitespace}
                                                   .Select(o => o)?{whitespace}
                                                   .Select(o => o);
                                    }
                                }
                                """;

        var input = template.Replace("{whitespace}", " \t ");

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?.Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an inner conditional chain inside a lambda argument and the outer chain's conditional
    /// links are each laid out by their own chain
    /// </summary>
    [TestMethod]
    public void InnerLambdaChainAndOuterLinksAreEachJoinedByTheirOwnChain()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B(x => x?
                                                     .Y())?
                                             .C()?
                                             .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B(x => x?.Y())
                                                ?.C()
                                                ?.D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertConditionalAndPlainFormsMatch(input, expected);
    }

    /// <summary>
    /// Verifies that the canonical layout with <c>?.</c> leading each continuation line is left unchanged
    /// </summary>
    [TestMethod]
    public void ChainAlreadyWrappedBeforeQuestionMarkStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)
                                                ?.Select(o => o)
                                                ?.Select(o => o);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment between the previous token and a trailing <c>?</c> stays behind that
    /// token while the conditional link moves onto its own line
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeQuestionMarkStaysBehindPreviousToken()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B() /* c */?
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B() /* c */
                                                ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment between <c>?</c> and <c>.</c> stays directly after the <c>?</c>, and the
    /// binding <c>.</c> starts the next line aligned under that <c>?</c>
    /// </summary>
    [TestMethod]
    public void LineCommentBetweenQuestionMarkAndDotKeepsCommentAndAlignsDotUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)? // note
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ? // note
                                                   .Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment between <c>?</c> and <c>.</c> stays directly after the <c>?</c>, and the
    /// binding <c>.</c> starts the next line aligned under that <c>?</c>
    /// </summary>
    [TestMethod]
    public void BlockCommentBetweenQuestionMarkAndDotKeepsCommentAndAlignsDotUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)? /* note */
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ? /* note */
                                                   .Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive between <c>?</c> and <c>.</c> keeps its own line, and the
    /// binding <c>.</c> starts the following line aligned under that <c>?</c>
    /// </summary>
    [TestMethod]
    public void PragmaBetweenQuestionMarkAndDotKeepsDirectiveAndAlignsDotUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)?
                             #pragma warning disable CS0168
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?
                                #pragma warning disable CS0168
                                                   .Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that disabled text between <c>?</c> and <c>.</c> is preserved verbatim, and the binding
    /// <c>.</c> starts the following line aligned under that <c>?</c>
    /// </summary>
    [TestMethod]
    public void DisabledTextBetweenQuestionMarkAndDotIsPreservedAndDotAlignsUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)?
                             #if false
                                                junk
                             #endif
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?
                                #if false
                                                   junk
                                #endif
                                                   .Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that each conditional link is decided on its own: the uncommented link is joined to <c>?.</c>,
    /// and the commented one keeps its comment after the <c>?</c> with the <c>.</c> aligned under it
    /// </summary>
    [TestMethod]
    public void CommentOnOneLinkOnlyLeavesOtherLinksJoined()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return list.Select(o => o)?
                                                .Select(o => o)? // note
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return list.Select(o => o)
                                                   ?.Select(o => o)
                                                   ? // note
                                                   .Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a sole conditional link with a comment between <c>?</c> and <c>.</c> is treated as
    /// wrapped: the <c>?</c> starts its own aligned line and the <c>.</c> is aligned under it
    /// </summary>
    [TestMethod]
    public void CommentOnSoleLinkMovesQuestionMarkOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B()? // note
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a.B()
                                                ? // note
                                                .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an unwrapped chain followed by a trailing comment stays on its single line
    /// </summary>
    [TestMethod]
    public void SingleLineChainWithTrailingCommentStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a.B()?.C(); // note
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between <c>?</c> and <c>.</c> on an identifier-rooted chain leaves the <c>?</c>
    /// on the root line, where the uncommented chain joins its first link, and aligns the <c>.</c> under it
    /// </summary>
    [TestMethod]
    public void CommentAfterQuestionMarkOnIdentifierRootKeepsQuestionMarkOnRootLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a? // note
                                             .B()
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a? // note
                                                .B()
                                                .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line between <c>?</c> and <c>.</c> is kept, and both the comment
    /// and the binding <c>.</c> are aligned under the <c>?</c>
    /// </summary>
    [TestMethod]
    public void OwnLineCommentBetweenQuestionMarkAndDotAlignsCommentAndDotUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a?
                                             // note
                                             .B()
                                             .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a?

                                                // note
                                                .B()
                                                .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between <c>?</c> and <c>[</c> in a short chain refuses the join, keeps the
    /// <c>?</c> on the root line, and aligns the <c>[</c> under it
    /// </summary>
    [TestMethod]
    public void CommentBetweenQuestionMarkAndBracketInShortChainAlignsBracketUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a? // c
                                         [0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a? // c
                                                [0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between <c>?</c> and <c>.</c> in a short member-access chain refuses the join,
    /// keeps the <c>?</c> on the root line, and aligns the <c>.</c> under it
    /// </summary>
    [TestMethod]
    public void CommentBetweenQuestionMarkAndDotInShortChainAlignsDotUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a, dynamic list, dynamic obj)
                                 {
                                     return a? // c
                                         .B;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a, dynamic list, dynamic obj)
                                    {
                                        return a? // c
                                                .B;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the conditional form against its expected layout, and the same source with every <c>?</c>
    /// removed against the same layout with every <c>?</c> removed, so both spellings of the chain are
    /// proven to produce the same line structure and alignment
    /// </summary>
    /// <param name="input">The conditional-access source</param>
    /// <param name="expected">The expected formatted conditional-access source</param>
    private static void AssertConditionalAndPlainFormsMatch(string input, string expected)
    {
        AssertRuleResult(input, expected);
        AssertRuleResult(input.Replace("?", string.Empty), expected.Replace("?", string.Empty));
    }

    #endregion // Methods
}