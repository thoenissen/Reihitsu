using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for a same-line gap that holds a multi-line delimited comment: a line break inside the comment's
/// text is not a line break of the gap, so the gap behind the comment follows the rule for the token pair exactly as it
/// does behind a single-line comment
/// </summary>
[TestClass]
public class MultiLineCommentGapSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the space behind a multi-line comment in front of the name behind the dot of a qualified parameter
    /// type is removed, as it is behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    [Requirement("FMT-SPACE-013")]
    public void SpaceBehindMultiLineCommentAfterQualifiedNameDotIsRemovedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(System. /* a
                                     b */ IDisposable x)
                                 {
                                 }

                                 void N(System. /* a b */ IDisposable x)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(System. /* a
                                        b */IDisposable x)
                                    {
                                    }

                                    void N(System. /* a b */IDisposable x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space behind a multi-line comment in front of the name behind the dot of a qualified constraint
    /// is removed, as it is behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    [Requirement("FMT-SPACE-013")]
    public void SpaceBehindMultiLineCommentInConstraintClauseIsRemovedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class D<T>
                                 where T : System. /* a
                                     b */ IDisposable
                             {
                                 private T _value;
                             }

                             class E<T>
                                 where T : System. /* a b */ IDisposable
                             {
                                 private T _value;
                             }
                             """;
        const string expected = """
                                class D<T>
                                    where T : System. /* a
                                        b */IDisposable
                                {
                                    private T _value;
                                }

                                class E<T>
                                    where T : System. /* a b */IDisposable
                                {
                                    private T _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space behind a multi-line comment in front of a type argument is removed, as it is behind a
    /// single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    [Requirement("FMT-SPACE-013")]
    public void SpaceBehindMultiLineCommentInFrontOfTypeArgumentIsRemovedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             using System;

                             class D<T> : IComparable</* a
                                 b */ T>
                             {
                                 public int CompareTo(T other)
                                 {
                                     return 0;
                                 }
                             }

                             class E<T> : IComparable</* a b */ T>
                             {
                                 public int CompareTo(T other)
                                 {
                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class D<T> : IComparable</* a
                                    b */T>
                                {
                                    public int CompareTo(T other)
                                    {
                                        return 0;
                                    }
                                }

                                class E<T> : IComparable</* a b */T>
                                {
                                    public int CompareTo(T other)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space behind a multi-line comment in front of a binary operator becomes one space, and that the run of
    /// spaces in front of the comment collapses, as behind and in front of a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    [Requirement("FMT-SPACE-013")]
    public void SpaceBehindMultiLineCommentInFrontOfBinaryOperatorBecomesOneSpaceAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int y = 1  /* a
                                     b */  + 2;

                                 int z = 1  /* a b */  + 2;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int y = 1 /* a
                                        b */ + 2;

                                    int z = 1 /* a b */ + 2;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space a binary operator demands in front of it is inserted behind a multi-line comment, and that the gap
    /// in front of the comment keeps its width, as at a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    [Requirement("FMT-SPACE-013")]
    public void SpaceDemandedBehindMultiLineCommentIsInsertedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int y = 1/* a
                                     b */+ 2;

                                 int z = 1/* a b */+ 2;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int y = 1/* a
                                        b */ + 2;

                                    int z = 1/* a b */ + 2;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}