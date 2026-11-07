using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the colon of a type-parameter constraint clause: one space on each side, as for a base-list colon
/// </summary>
[TestClass]
public class ConstraintClauseColonSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the change-doc example: a constraint colon written without a space on either side or on both sides gets one
    /// space on each side, for a method's clause and a type's clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void ConstraintColonsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T:class
                                 {
                                 }

                                 void N<T>()
                                     where T :new()
                                 {
                                 }

                                 void O<T>()
                                     where T: struct
                                 {
                                 }
                             }

                             class D<T>
                                 where T:class
                             {
                                 void P()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class
                                    {
                                    }

                                    void N<T>()
                                        where T : new()
                                    {
                                    }

                                    void O<T>()
                                        where T : struct
                                    {
                                    }
                                }

                                class D<T>
                                    where T : class
                                {
                                    void P()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constraint colon already spaced on both sides stays unchanged, and that a wider gap on either side
    /// becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void SpacedConstraintColonStaysAndWiderGapsBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class
                                 {
                                 }

                                 void N<T>()
                                     where T   :   class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class
                                    {
                                    }

                                    void N<T>()
                                        where T : class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the constraint colon of a local function, a delegate, and an interface member without a body gets one
    /// space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void ConstraintColonsOfLocalFunctionDelegateAndMemberWithoutBodyAreSpaced()
    {
        // Arrange
        const string input = """
                             delegate void D<T>()
                                 where T:class;

                             interface I
                             {
                                 void M<T>()
                                     where T:class;
                             }

                             class C
                             {
                                 void M()
                                 {
                                     void L<T>()
                                         where T:class
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                delegate void D<T>()
                                    where T : class;

                                interface I
                                {
                                    void M<T>()
                                        where T : class;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        void L<T>()
                                            where T : class
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the colon of every clause of one declaration is spaced, and that the colons of a base list and of
    /// the constraint clause on the same declaration are spaced alike
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void BaseListColonAndConstraintColonsOfOneDeclarationAreSpaced()
    {
        // Arrange
        const string input = """
                             class B
                             {
                                 void M()
                                 {
                                 }
                             }

                             class C<TKey, TValue>:B
                                 where TKey:struct
                                 where TValue:class
                             {
                                 void N()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class B
                                {
                                    void M()
                                    {
                                    }
                                }

                                class C<TKey, TValue> : B
                                    where TKey : struct
                                    where TValue : class
                                {
                                    void N()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tight constraint colon in a wrapped clause is spaced, and that the later constraints align under
    /// the first constraint behind the spaced colon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    [Requirement("FMT-INDENT-032")]
    public void TightColonOfWrappedClauseIsSpacedAndConstraintsAlignBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T:class,
                                         new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  new()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the type parameter carries in front of the colon keeps its place and the colon gets one
    /// space behind the comment and one in front of the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfConstraintColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T /* c */:class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T /* c */ : class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the colon carries behind it stays glued to the colon and the first constraint gets one
    /// space behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindConstraintColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T:/* c */class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T :/* c */ class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constraint colon at the start of a line keeps its line and gets one space only behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void ConstraintColonStartingALineIsSpacedOnlyBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T // c
                                         :class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T // c
                                            : class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a name colon in the body of a method whose constraint colon is spaced keeps the name-colon spacing:
    /// no space before it and one behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    [Requirement("FMT-SPACE-019")]
    public void NameColonNextToConstraintClauseKeepsItsSpacing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>(int a)
                                     where T:class
                                 {
                                     M<T>(a:1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>(int a)
                                        where T : class
                                    {
                                        M<T>(a: 1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}