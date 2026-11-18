using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the joins of a member-access chain and how they compose with the line breaks the chain inserts
/// </summary>
[TestClass]
public class ChainGapJoinTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies a chain that needs every kind of join at once: split member names, attached parts, wraps inside link operators, and the wrap in front of the first link
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("FMT-BREAK-020")]
    public void EveryJoinKindOfOneChainIsClosedInOneLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a
                             .B
                             ?
                             .C()
                             !.
                             D()?
                             [0]
                             .
                             E()
                             .F();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B
                                                 ?.C()
                                                 !.D()?[0]
                                                 .E()
                                                 .F();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a member name that is joined to its dot and also ends its line in front of the next link
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("FMT-BREAK-020")]
    public void TokenJoinedOnItsLeadingSideEndsItsLineOnItsTrailingSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()
                             .
                             C .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 .C
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a link operator whose parts are joined while the line breaks in front of it, because it stands behind the closing line of a multi-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void OperatorBehindMultiLineCommentTailIsJoinedInsideAndBrokenInFront()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()
                             /* one
                             two */ ?
                             .C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()

                                                 /* one
                                two */
                                                 ?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a chain whose only line break stands inside a link operator: it stays wrapped, so every later call starts its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    public void ChainWrappedOnlyInsideALinkOperatorStaysWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()?
                             .C().D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 ?.C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a prefix link whose only line break stands inside its operator: the link starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    public void PrefixLinkWrappedOnlyInsideItsOperatorStartsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B?
                             .C.D()
                             .E();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B
                                                 ?.C
                                                 .D()
                                                 .E();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a chain without links whose conditional element accesses are joined onto one line
    /// </summary>
    [TestMethod]
    public void ConditionalElementAccessesWithoutLinksAreJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a
                             ?[0]
                             ?
                             [1];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a?[0]?[1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the line break inserted in front of a later call behind a block comment, and behind whitespace that the final output does not keep
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("FMT-CLEAN-001")]
    public void LineBreakInsertedBehindBlockCommentAndBehindWhitespace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B() /* c */ .C()
                             .D();
                                     var y = a.B()   .C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B() /* c */
                                                 .C()
                                                 .D();
                                        var y = a.B()
                                                 .C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment in front of a later link and in front of the first link keeps its line, so neither gap is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void SingleLineCommentInLinkGapKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B() // c
                             .C()
                             .D();
                                     var y = a // c
                             .B()
                             .C();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B() // c
                                                 .C()
                                                 .D();
                                        var y = a // c
                                                .B()
                                                .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment inside a link operator keeps its line, so the operator is not joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void SingleLineCommentInsideLinkOperatorKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()? // c
                             .C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 ? // c
                                                 .C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment between a dot and its member name keeps its line, so the name is not joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void SingleLineCommentBetweenDotAndMemberNameKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B(). // c
                             C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 . // c
                                        C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive in front of a later link keeps its line, and the disabled text inside it stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void DirectiveInLinkGapKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()
                             #if X
                             .C()
                             #endif
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                #if X
                                .C()
                                #endif
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive inside a link operator keeps its line, so the operator is not joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void DirectiveInsideLinkOperatorKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()?
                             #if X
                             #endif
                             .C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 ?
                                #if X
                                #endif
                                                 .C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive in front of the first link keeps its line, so the first link is not joined to the root
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void DirectiveInFrontOfFirstLinkKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a
                             #if X
                             #endif
                             .B()
                             .C();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a
                                #if X
                                #endif
                                                .B()
                                                .C();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive between a dot and its member name keeps its line, so the name is not joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void DirectiveBetweenDotAndMemberNameKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B().
                             #if X
                             #endif
                             C()
                             .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()
                                                 .
                                #if X
                                #endif
                                        C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies a wrapped chain inside an argument of an outer wrapped chain: the inner split member name is joined and both chains keep their own layout
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("FMT-BREAK-020")]
    public void ChainInsideArgumentOfOuterChainIsJoinedOnItsOwn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B(b
                             .C()
                             .
                             D())
                             .E();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B(b.C()
                                                     .D())
                                                 .E();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}