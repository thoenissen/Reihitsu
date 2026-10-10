using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — the layout of member-access chains: link operators (<c>.</c>, <c>?.</c>, <c>!.</c>, <c>!?.</c>) are never split, attached parts (argument lists, element accesses, a trailing <c>!</c>) belong to the element in front of them, the formatter only wraps where the user started, and continuation links are aligned to the chain's anchor
/// </summary>
[TestClass]
public class ChainLinkLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that breaks inside <c>?.</c> and <c>!.</c> of later links move in front of the operator, with <c>?</c> and
    /// <c>!</c> aligned like <c>.</c>
    /// </summary>
    [TestMethod]
    public void NullForgivingAndConditionalLinksMoveTheirBreakInFrontOfTheOperator()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = list.Where(o => o)?
                                         .Select(o => o)!
                                         .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = list.Where(o => o)
                                                    ?.Select(o => o)
                                                    !.ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break inside <c>!?.</c> after a call moves in front of the <c>!</c> and keeps <c>!?.</c> together
    /// </summary>
    [TestMethod]
    public void BreakInsideNullForgivingConditionalOperatorMovesInFrontOfTheNullForgivingOperator()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()!?
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break between <c>!</c> and <c>?.</c> moves in front of the <c>!</c>
    /// </summary>
    [TestMethod]
    public void BreakBetweenNullForgivingAndConditionalOperatorMovesInFrontOfTheNullForgivingOperator()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()!
                                         ?.C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>!</c>, <c>?</c> and <c>.</c> written on three lines are joined into one <c>!?.</c> operator
    /// </summary>
    [TestMethod]
    public void NullForgivingConditionalOperatorSplitOverThreeLinesIsJoined()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()!
                                         ?
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break inside <c>!.</c> of a later link moves in front of the <c>!</c>, under the first link
    /// </summary>
    [TestMethod]
    public void BreakInsideNullForgivingMemberAccessOfLaterLinkMovesInFrontOfTheOperator()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Y()!
                                         .Z();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.Y()
                                                 !.Z();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a moved <c>!.</c> link aligns under the <c>?</c> of a conditional first link
    /// </summary>
    [TestMethod]
    public void BreakInsideNullForgivingMemberAccessAfterConditionalLinkAlignsUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = value?.B()!
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = value?.B()
                                                     !.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a moved <c>!.</c> call after a property prefix keeps its own line under the prefix
    /// </summary>
    [TestMethod]
    public void BreakInsideNullForgivingMemberAccessAfterPrefixStaysWrapped()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B!
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 !.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a user wrap inside a <c>!.</c> prefix link is kept as a wrap in front of the <c>!</c>
    /// </summary>
    [TestMethod]
    public void NullForgivingPrefixLinkWrappedByUserKeepsItsLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .Prop1!
                                         .Prop2
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.Prop1
                                                 !.Prop2
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a prefix ending in a split <c>!?.</c> operator reaches its layout in one pass
    /// </summary>
    [TestMethod]
    public void WrappedPrefixWithNullForgivingConditionalOperatorConvergesInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B!?
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>!</c> and <c>?</c> on separate lines after a prefix reach the same layout in one pass
    /// </summary>
    [TestMethod]
    public void WrappedPrefixWithNullForgivingAndConditionalOnSeparateLinesConvergesInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B!
                                         ?
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a later conditional link of a split <c>!?.</c> chain gets its own <c>?.</c> line in one pass
    /// </summary>
    [TestMethod]
    public void WrappedPrefixWithNullForgivingConditionalAndConditionalLaterLinkConvergesInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B!?
                                         .C()?
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 !?.C()
                                                 ?.D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that of two <c>!</c> only the one in front of <c>?.</c> belongs to the operator
    /// </summary>
    [TestMethod]
    public void DoubleNullForgivingKeepsTheFirstOperatorAttached()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B!!?
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B!
                                                 !?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>!?[</c> is an attached part and joins its element
    /// </summary>
    [TestMethod]
    public void NullForgivingConditionalElementAccessIsAttachedInFull()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B!?
                                         [0]
                                         .C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B!?[0]
                                                 .C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that split operators in the prefix and an attached element access converge in one pass
    /// </summary>
    [TestMethod]
    public void SplitOperatorsInsidePrefixAndChainPartConvergeInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = Get()
                                         .B!
                                         ?.C.C()
                                         ?[0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = Get().B
                                                     !?.C
                                                     .C()?[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>!.</c> first link joins the root and a link on its line stays there
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingFirstLinkJoinsRootAndKeepsLaterLinkOnRootLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         !
                                         .B?.C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a!.B?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a later wrapped link aligns under the <c>!</c> of a joined first link
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingFirstLinkJoinsRootAndLaterLinkAlignsUnderNullForgiving()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         !
                                         .B
                                         ?.C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a!.B
                                                 ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>!?.</c> first link joins the root and the next link aligns under <c>!</c>
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingConditionalFirstLinkJoinsRootInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         !
                                         ?.B()
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a!?.B()
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the anchor of a <c>!?.</c> first link is its <c>!</c>
    /// </summary>
    [TestMethod]
    public void ConditionalNullForgivingFirstLinkAlignsLaterLinkUnderNullForgiving()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a!?
                                         .B()
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a!?.B()
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call-less chain keeps the user's later wrap with <c>?.</c> leading the line
    /// </summary>
    [TestMethod]
    public void SplitConditionalLinksOfCallLessChainKeepTheirWraps()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = Get()?
                                         .Bar?
                                         .Baz;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = Get()?.Bar
                                                     ?.Baz;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that split <c>!.</c> and <c>?.</c> operators of a call-less chain move in front of their first character
    /// </summary>
    [TestMethod]
    public void SplitOperatorsInCallLessChainAfterPrefixKeepTheirWraps()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B!
                                         .C?.C?
                                         .C;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 !.C?.C
                                                 ?.C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every chain-part link after a split <c>?.</c> starts its own line, also non-invoked ones
    /// </summary>
    [TestMethod]
    public void SplitNonInvokedConditionalLinkAfterCallStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()?
                                         .C?.D;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 ?.C
                                                 ?.D;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a split <c>?.</c> after an element access moves in front of the <c>?</c>
    /// </summary>
    [TestMethod]
    public void SplitConditionalLinkAfterElementAccessMovesInFrontOfQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()[0]?
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()[0]
                                                 ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every split <c>?.</c> after an element access starts its own aligned line
    /// </summary>
    [TestMethod]
    public void SplitConditionalLinksAfterElementAccessMoveInFrontOfQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()[0]?
                                         .C()?
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()[0]
                                                 ?.C()
                                                 ?.D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked link with an element access in a wrapped chain starts its own line
    /// </summary>
    [TestMethod]
    public void NonInvokedLinkWithElementAccessStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B().C[0]?
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 .C[0]
                                                 ?.D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain part after a wrapped conditional prefix aligns under its <c>?</c>
    /// </summary>
    [TestMethod]
    public void ChainPartAfterConditionalPrefixAlignsUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         ?.C
                                         !.C()?
                                         .C;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.C
                                                 !.C()
                                                 ?.C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break inside the chain's first operator counts as the user's wrap
    /// </summary>
    [TestMethod]
    public void WrappedFirstOperatorCountsAsWrapForLaterLinks()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a?
                                         .B().C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.B()
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break after a dot is rejoined and does not count as a wrap
    /// </summary>
    [TestMethod]
    public void MemberNameSplitAfterConditionalAccessIsRejoinedAndIsNoWrap()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a?.
                                         Prop.Call()
                                         .Then();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.Prop.Call()
                                                       .Then();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between <c>!</c> and <c>?.</c> stays behind the <c>!</c>, which starts the line
    /// </summary>
    [TestMethod]
    public void CommentBetweenNullForgivingAndConditionalKeepsCommentBehindNullForgiving()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()! // c
                                         ?.C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 ! // c
                                                 ?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between <c>!?</c> and <c>.</c> keeps <c>!?</c> together and puts <c>.</c> under <c>!</c>
    /// </summary>
    [TestMethod]
    public void CommentInsideNullForgivingConditionalOperatorKeepsOperatorStartTogether()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()!? // c
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 !? // c
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment inside <c>!.</c> stays behind <c>!</c> and the <c>.</c> goes under the <c>!</c>
    /// </summary>
    [TestMethod]
    public void CommentInsideNullForgivingMemberAccessPutsDotUnderNullForgiving()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Y()! // c
                                         .Z();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.Y()
                                                 ! // c
                                                 .Z();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>?[</c> joins the element in front of it
    /// </summary>
    [TestMethod]
    public void ConditionalElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()
                                         ?[0]
                                         ?.C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()?[0]
                                                 ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>[</c> joins the element in front of it in one pass
    /// </summary>
    [TestMethod]
    public void ElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()
                                         [0]
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()[0]
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument list joins its member name
    /// </summary>
    [TestMethod]
    public void ArgumentListJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B
                                         (1)
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B(1)
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument list joins a generic member name
    /// </summary>
    [TestMethod]
    public void ArgumentListJoinsGenericMemberName()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B<int>
                                         (1)
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B<int>(1)
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped trailing <c>!</c> joins its element
    /// </summary>
    [TestMethod]
    public void TrailingNullForgivingJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B()
                                         !;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B()!;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped trailing <c>!</c> joins the last link of a wrapped chain
    /// </summary>
    [TestMethod]
    public void TrailingNullForgivingJoinsLastLink()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()
                                         .C()
                                         !;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 .C()!;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped trailing <c>!</c> inside an argument joins its element
    /// </summary>
    [TestMethod]
    public void TrailingNullForgivingInsideArgumentJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     Use(x.B()
                                         !);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        Use(x.B()!);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped element access ending the chain joins its element
    /// </summary>
    [TestMethod]
    public void OutermostElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B()
                                         [0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B()[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access ending the chain joins its element
    /// </summary>
    [TestMethod]
    public void OutermostConditionalElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()
                                         ?[0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()?[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped element access joins a member name
    /// </summary>
    [TestMethod]
    public void OutermostElementAccessJoinsMemberName()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Y
                                         [0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.Y[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break inside <c>?[</c> is closed and the element access joins its element
    /// </summary>
    [TestMethod]
    public void SplitConditionalElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()?
                                         [0]
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()?[0]
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a split <c>?[</c> joins its element while the following split <c>?.</c> starts its own line
    /// </summary>
    [TestMethod]
    public void SplitConditionalElementAccessBeforeSplitConditionalLinkJoinsItsElement()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()?
                                         [0]?
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()?[0]
                                                 ?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access after a member name joins in one pass and the call stays wrapped
    /// </summary>
    [TestMethod]
    public void ElementAccessAfterMemberNameJoinsInOnePass()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B
                                         [0]
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B[0]
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument list of the root joins inside a member chain
    /// </summary>
    [TestMethod]
    public void ArgumentListOfRootJoinsInMemberChain()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = Foo
                                         (1).Bar();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = Foo(1).Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access outside a member chain keeps the user's break and today's indentation
    /// </summary>
    [TestMethod]
    public void ElementAccessWithoutMemberAccessKeepsItsBreak()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = list
                                         [0];
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = list
                                        [0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list outside a member chain keeps the user's break and today's indentation
    /// </summary>
    [TestMethod]
    public void ArgumentListWithoutMemberAccessKeepsItsBreak()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = Foo
                                         (1);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = Foo
                                        (1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before an element access keeps it on its own line at the anchor column
    /// </summary>
    [TestMethod]
    public void CommentBeforeElementAccessKeepsElementAccessAtAnchorColumn()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B() // c
                                         [0]
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B() // c
                                                 [0]
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment inside <c>?[</c> keeps the <c>?</c> at its element and puts <c>[</c> under it
    /// </summary>
    [TestMethod]
    public void CommentInsideConditionalElementAccessPutsBracketUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B()? // c
                                         [0]
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B()? // c
                                                     [0]
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before an argument list keeps it on its own line at the anchor column
    /// </summary>
    [TestMethod]
    public void CommentBeforeArgumentListKeepsArgumentListAtAnchorColumn()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.B // c
                                         (1)
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.B // c
                                                 (1)
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line chain is never wrapped
    /// </summary>
    [TestMethod]
    public void SingleLineChainWithNonInvokedTailStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = list.Where(o => o).First().Name;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line qualified name is never wrapped
    /// </summary>
    [TestMethod]
    public void SingleLineQualifiedNameStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var e = System.Text.Encoding.UTF8;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a call-less chain joins its first wrap and aligns the user's other wraps
    /// </summary>
    [TestMethod]
    public void CallLessChainJoinsFirstWrapAndAlignsTheOthers()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var c = order
                                         .Customer
                                         .Address
                                         .City;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var c = order.Customer
                                                     .Address
                                                     .City;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call-less chain with one conditional link joins the root
    /// </summary>
    [TestMethod]
    public void CallLessChainWithSingleLinkJoinsRoot()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = Get()
                                         ?.Bar;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = Get()?.Bar;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call-less chain keeps the user's wrap and adds none
    /// </summary>
    [TestMethod]
    public void CallLessChainKeepsUserWrapWithoutAddingOne()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B
                                         .C.D;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B
                                                 .C.D;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call-less chain wrapped only at its first link joins onto one line
    /// </summary>
    [TestMethod]
    public void CallLessChainWithConditionalTailJoinsRoot()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B?.C;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B?.C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call-less conditional chain joins its first wrap and keeps the later one under <c>?</c>
    /// </summary>
    [TestMethod]
    public void CallLessConditionalChainKeepsLaterWrap()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         ?.B
                                         ?.C;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.B
                                                 ?.C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that misaligned continuation lines of a call-less chain are aligned
    /// </summary>
    [TestMethod]
    public void MisalignedCallLessChainIsAligned()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var c = order.Customer
                                 .Address
                                               .City;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var c = order.Customer
                                                     .Address
                                                     .City;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the first link keeps a call-less chain wrapped at the root column
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstLinkOfCallLessChainKeepsItAtRootColumn()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var c = order // c
                                         .Customer
                                         .Address;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var c = order // c
                                                .Customer
                                                .Address;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked access after a call in a wrapped chain starts its own line
    /// </summary>
    [TestMethod]
    public void NonInvokedTailAfterWrappedCallStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = list.Where(o => o)
                                         .First().Name;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = list.Where(o => o)
                                                    .First()
                                                    .Name;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional non-invoked access after a call in a wrapped chain starts its own line
    /// </summary>
    [TestMethod]
    public void ConditionalNonInvokedTailAfterWrappedCallStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = list.Where(o => o)
                                         .FirstOrDefault()?.Name;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = list.Where(o => o)
                                                    .FirstOrDefault()
                                                    ?.Name;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain with one link wrapped only there joins onto one line
    /// </summary>
    [TestMethod]
    public void SingleConditionalCallAfterElementAccessRootJoins()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a[0]
                                         ?.C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a[0]?.C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property between calls in a wrapped chain starts its own line
    /// </summary>
    [TestMethod]
    public void PropertyAfterFirstCallStartsItsOwnLineInWrappedChain()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var f = syntaxRoot.FindToken(position).Parent
                                         ?.AncestorsAndSelf()
                                         .FirstOrDefault();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var f = syntaxRoot.FindToken(position)
                                                          .Parent
                                                          ?.AncestorsAndSelf()
                                                          .FirstOrDefault();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first call sharing a line with a wrapped prefix link gets its own line
    /// </summary>
    [TestMethod]
    public void ChainPartAfterWrappedPrefixStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var v = declaration.Declaration
                                         .Variables.Select(x => x);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var v = declaration.Declaration
                                                           .Variables
                                                           .Select(x => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a joined first prefix wrap keeps the first call on the root line and wraps the later call
    /// </summary>
    [TestMethod]
    public void PrefixWrapJoinedKeepsFirstCallOnRootLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .Prop.Foo().Bar();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.Prop.Foo()
                                                      .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional call after a joined prefix stays on the root line and the later call aligns under it
    /// </summary>
    [TestMethod]
    public void ConditionalCallAfterJoinedPrefixKeepsRootLineAndWrapsLaterCall()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .Prop?.ToString().Trim();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.Prop?.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked tail after a joined first call starts its own line
    /// </summary>
    [TestMethod]
    public void NonInvokedTailOfWrappedFirstCallStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         .B().C;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 .C;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call after a wrapped conditional prefix stays wrapped under the <c>?</c>
    /// </summary>
    [TestMethod]
    public void CallAfterConditionalPrefixStaysWrappedUnderQuestionMark()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         ?.B
                                         .C();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.B
                                                 .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call after a split conditional prefix on a <c>typeof</c> root stays wrapped under <c>?</c>
    /// </summary>
    [TestMethod]
    public void CallAfterSplitConditionalOnTypeofRootStaysWrapped()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = typeof(int)?
                                         .Name
                                         .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = typeof(int)?.Name
                                                           .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked link after the first call of a wrapped prefix chain starts its own line
    /// </summary>
    [TestMethod]
    public void LaterNonInvokedLinkAfterWrappedFirstCallStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Items
                                         .Where(o => o).Count;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = x.Items
                                                 .Where(o => o)
                                                 .Count;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that properties between calls on the root line start their own lines once the chain is wrapped
    /// </summary>
    [TestMethod]
    public void PropertyBetweenCallsStartsItsOwnLineInWrappedChain()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B().C.D()
                                         .E();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 .C
                                                 .D()
                                                 .E();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property after an element access starts its own line in a wrapped chain
    /// </summary>
    [TestMethod]
    public void PropertyAfterElementAccessStartsItsOwnLineInWrappedChain()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.B()
                                         .C()[0].D;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a.B()
                                                 .C()[0]
                                                 .D;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the first link keeps it wrapped and every link starts its own line at the root column
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstLinkPutsEveryLinkOnItsOwnLineAtRootColumn()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a
                                         // keep wrapped
                                         .Prop.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a

                                                // keep wrapped
                                                .Prop
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
    /// Verifies that calls after a prefix ending in <c>?[</c> align under the prefix, never under <c>?</c>
    /// </summary>
    [TestMethod]
    public void CallsAfterConditionalElementAccessPrefixAlignUnderPrefix()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.P?[0]
                                              .B()
                                              .C();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a call after a prefix ending in an element access aligns under the prefix
    /// </summary>
    [TestMethod]
    public void CallAfterElementAccessPrefixAlignsUnderPrefix()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Items[0]
                                              .ToString();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a non-invoked conditional prefix link is no anchor: the later call aligns under the first call
    /// </summary>
    [TestMethod]
    public void NonInvokedConditionalPrefixIsNoAnchor()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a?.B.C()
                                         .D();
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M()
                                    {
                                        var r = a?.B.C()
                                                    .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>!?.</c> link after a property prefix keeps its layout under the prefix
    /// </summary>
    [TestMethod]
    public void NullForgivingConditionalLinkAfterPrefixStaysAligned()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = a.X
                                              !?.B()
                                              .C();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped call after a property prefix stays wrapped under the prefix
    /// </summary>
    [TestMethod]
    public void WrappedCallAfterPropertyPrefixStaysUnderPrefix()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M()
                                 {
                                     var r = x.Items.Count
                                              .ToString();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}