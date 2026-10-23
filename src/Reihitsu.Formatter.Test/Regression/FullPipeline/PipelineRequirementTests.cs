using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for the invariants that hold for the formatter as a whole: the comments' layout when the
/// formatter wraps code in a block, one treatment per syntax family, and the line a comment that spans several lines
/// belongs to
/// </summary>
[TestClass]
public class PipelineRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the comment behind the expression of a converted expression body stays in front of the
    /// statement's semicolon, and that the own-line comment above an unbraced <c>if</c> body and the comment behind
    /// its statement keep their lines in the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void WrappingInABlockKeepsOwnLineAndTrailingCommentsOnTheirLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Get() => 42 /* why */;

                                 void M(bool a)
                                 {
                                     if (a)
                                         // leading
                                         Foo(); // trailing
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return 42 /* why */;
                                    }

                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            // leading
                                            Foo(); // trailing
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between two comments behind the expression of a converted expression body is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void ConvertedExpressionBodyKeepsTheSpaceBetweenTwoTrailingComments()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Get() => 42 /* a */ /* b */;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return 42 /* a */ /* b */;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comment behind the expression stays in front of the statement's semicolon for an
    /// expression-bodied local function, constructor, finalizer, operator, and conversion operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void EveryConvertedExpressionBodyKindKeepsItsTrailingComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 C(int value) => _value = value /* store */;

                                 ~C() => Release() /* release */;

                                 public static C operator +(C a, C b) => a /* left */;

                                 public static explicit operator int(C c) => c._value /* value */;

                                 int M()
                                 {
                                     return L();

                                     int L() => 1 /* one */;
                                 }

                                 void Release()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    C(int value)
                                    {
                                        _value = value /* store */;
                                    }

                                    ~C()
                                    {
                                        Release() /* release */;
                                    }

                                    public static C operator +(C a, C b)
                                    {
                                        return a /* left */;
                                    }

                                    public static explicit operator int(C c)
                                    {
                                        return c._value /* value */;
                                    }

                                    int M()
                                    {
                                        return L();

                                        int L()
                                        {
                                            return 1 /* one */;
                                        }
                                    }

                                    void Release()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the own-line comment above an unbraced <c>else</c> body and the comment behind its statement
    /// keep their lines in the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void BracedElseBodyKeepsOwnLineAndTrailingCommentsOnTheirLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         Foo();
                                     }
                                     else
                                         // leading
                                         Bar(); // trailing
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            Foo();
                                        }
                                        else
                                        {
                                            // leading
                                            Bar(); // trailing
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a braced switch section keeps the comment behind its label on the label's line, the own-line
    /// comment above its first statement on its own line, and the comment behind its last statement on that
    /// statement's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void BracedSwitchSectionKeepsOwnLineAndTrailingCommentsOnTheirLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1: // note
                                             // explain
                                             DoWork();
                                             Another(); // trailing
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1: // note
                                                {
                                                    // explain
                                                    DoWork();
                                                    Another(); // trailing
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the parameter lists of a constructor and a method are laid out alike, and so are two
    /// assignments wrapped before and after their <c>=</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-014")]
    public void ParameterListsAndAssignmentsAreLaidOutAlike()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C(int a
                                   , int b)
                                 {
                                 }

                                 void M(int a
                                        , int b)
                                 {
                                     a
                                         = b;
                                     b =
                                         a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    C(int a,
                                      int b)
                                    {
                                    }

                                    void M(int a,
                                           int b)
                                    {
                                        a = b;
                                        b = a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a function-pointer parameter list is joined onto one line, as its own requirement chooses
    /// inside the parameter-list family
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-014")]
    public void FunctionPointerParameterListKeepsItsOwnJoinedLayout()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 delegate*<int,
                                     int, void> _f;
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    delegate*<int, int, void> _f;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an array initializer and a collection expression written on one line stay on it, as their own
    /// requirements choose inside the family of initializer-like blocks
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-014")]
    public void OneLineArrayInitializerAndCollectionExpressionKeepTheirOwnLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] b = { 1, 2 };
                                     int[] a = [1, 2];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the arrow of an expression-bodied property joins the declaration's line, as its own requirement
    /// chooses inside the arrow family
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-014")]
    public void PropertyArrowKeepsItsOwnJoinedLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                     => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P => 1;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatter breaks the line in front of a statement that stands behind the closing
    /// <c>*/</c> of a comment that spans several lines, where the block asks the statement to start its own line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-COMMENT-012")]
    public void StatementBehindTheEndOfAMultiLineCommentIsPutOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     { /* p
                                     q */ Foo(); }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        { /* p
                                        q */
                                            Foo();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}