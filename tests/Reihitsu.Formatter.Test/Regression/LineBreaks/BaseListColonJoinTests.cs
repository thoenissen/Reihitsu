using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for a base list wrapped behind its colon, whose first base type joins the colon's line unless a comment, a
/// directive, or disabled text stands in the gap, and for a comma that starts a line in a base list, which moves behind the base
/// type in front of it; the later base types align under the first
/// </summary>
[TestClass]
public class BaseListColonJoinTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: the first base type of a class, an enum, and a record with a primary-constructor
    /// base type joins the colon's line, and a later base type that keeps its own line aligns under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void SpecExampleFirstBaseTypeWrappedBehindTheColonJoinsTheColonsLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class C :
                                 I1, I2
                             {
                                 int _x;
                             }

                             class D :
                                 I1,
                                 I2
                             {
                                 int _x;
                             }

                             enum E :
                                 int
                             {
                                 A
                             }

                             record R(int X) :
                                 Q(X);

                             record Q(int X);
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                class C : I1, I2
                                {
                                    int _x;
                                }

                                class D : I1,
                                          I2
                                {
                                    int _x;
                                }

                                enum E : int
                                {
                                    A
                                }

                                record R(int X) : Q(X);

                                record Q(int X);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example's other side: a comment behind the colon keeps the first base type on its own line,
    /// at the continuation indent
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void SpecExampleCommentBehindTheColonKeepsTheFirstBaseTypeOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             class C : // c
                                 I1
                             {
                                 int _x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment behind the colon blocks the join as well, and the later base type stays on the first
    /// base type's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void BlockCommentBehindTheColonKeepsTheFirstBaseTypeOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class N : /* c */
                                 I1, I2
                             {
                                 int _x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line between the colon and the first base type blocks the join, and the later base
    /// type aligns under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void CommentOnItsOwnLineBetweenTheColonAndTheFirstBaseTypeBlocksTheJoin()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class Q :

                                 // c
                                 I1,
                                 I2
                             {
                                 int _x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group between the colon and the first base type blocks the join, so the active and the
    /// disabled base type keep their lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void DirectiveBetweenTheColonAndTheFirstBaseTypeBlocksTheJoin()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class M :
                             #if DEBUG
                                 I1
                             #else
                                 I2
                             #endif
                             {
                                 int _x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a colon on its own line is joined in front and behind, so the first base type stands on the type's line,
    /// and that blank lines behind the colon go with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void ColonOnItsOwnLineAndBlankLinesBehindTheColonAreJoined()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class O
                                 :
                                 I1,
                                 I2
                             {
                                 int _x;
                             }

                             struct S :

                                 I1
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                class O : I1,
                                          I2
                                {
                                    int _x;
                                }

                                struct S : I1
                                {
                                    int _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first base type of a generic class joins the colon's line while its constraint clause keeps its own
    /// line, and that a primary-constructor base type with an argument list joins it in front of a later base type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void FirstBaseTypeInFrontOfAConstraintClauseAndAPrimaryConstructorBaseTypeJoinTheColonsLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             class G<T> :
                                 I1 where T : class
                             {
                                 int _x;
                             }

                             class B(int a)
                             {
                                 public int A => a;
                             }

                             class K() :
                                 B(1), I1
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                interface I1;

                                class G<T> : I1
                                    where T : class
                                {
                                    int _x;
                                }

                                class B(int a)
                                {
                                    public int A => a;
                                }

                                class K() : B(1), I1
                                {
                                    int _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first base type of a nested interface and of a nested record struct joins the colon's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void FirstBaseTypeOfNestedInterfaceAndRecordStructJoinsTheColonsLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class Outer
                             {
                                 interface J :
                                     I1
                                 {
                                     int X { get; }
                                 }

                                 record struct RS :
                                     I1, I2;
                             }
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                class Outer
                                {
                                    interface J : I1
                                    {
                                        int X { get; }
                                    }

                                    record struct RS : I1, I2;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a base list already written behind its colon stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-042")]
    public void BaseListBehindItsColonStaysAsWritten()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class C : I1, I2
                             {
                                 int _x;
                             }

                             class D : I1,
                                       I2
                             {
                                 int _x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: in a base list, a comma that starts a line moves behind the base type in front of
    /// it, and the next base type aligns under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void SpecExampleCommaThatStartsALineInABaseListEndsTheBaseTypesLine()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class C : I1
                                 , I2
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                class C : I1,
                                          I2
                                {
                                    int _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every comma that starts a line in a base list moves, also behind a first base type that joins the colon's
    /// line in the same pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    [Requirement("FMT-BREAK-042")]
    public void SeveralCommasThatStartALineInABaseListMoveAlsoBehindAJoinedFirstBaseType()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             interface I3;

                             class P : I1
                                 , I2
                                 , I3
                             {
                                 int _x;
                             }

                             class V :
                                 I1
                                 , I2
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                interface I3;

                                class P : I1,
                                          I2,
                                          I3
                                {
                                    int _x;
                                }

                                class V : I1,
                                          I2
                                {
                                    int _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind a comma that starts a line in a base list moves with the comma and stays behind it,
    /// and the line break then stands behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void BlockCommentBehindACommaThatStartsALineInABaseListMovesWithTheComma()
    {
        // Arrange
        const string input = """
                             interface I1;

                             interface I2;

                             class U : I1
                                 , /* c */ I2
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                interface I1;

                                interface I2;

                                class U : I1, /* c */
                                          I2
                                {
                                    int _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}