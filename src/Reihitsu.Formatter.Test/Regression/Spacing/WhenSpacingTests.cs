using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space in front of <c>when</c>: one space in front of the <c>when</c> of a <c>case</c> guard, a
/// switch-expression arm, and a <c>catch</c> filter, while the space behind it stays as written
/// </summary>
[TestClass]
public class WhenSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a <c>case</c> guard gets one space in front of <c>when</c> behind a pattern that ends in a parenthesis, a
    /// brace, a bracket, or a literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void CaseGuardBehindATokenThatIsNotAWordGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)when b:
                                             break;

                                         case { }when b:
                                             break;

                                         case [1]when b:
                                             break;

                                         case "x"when b:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case (1, 2) when b:
                                                break;

                                            case { } when b:
                                                break;

                                            case [1] when b:
                                                break;

                                            case "x" when b:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a run of whitespace in front of the <c>when</c> of a <c>case</c> guard behind an identifier becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void CaseGuardBehindAnIdentifierGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case int i   when b:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case int i when b:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the guard of a switch-expression arm gets one space in front of <c>when</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void SwitchExpressionArmGuardGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(object o, bool b)
                                 {
                                     return o switch
                                            {
                                                "x"when b => 1,
                                                (1, 2)when b => 2,
                                                _ => 0
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(object o, bool b)
                                    {
                                        return o switch
                                               {
                                                   "x" when b => 1,
                                                   (1, 2) when b => 2,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the filter of a <c>catch</c> clause gets one space in front of <c>when</c>, with and without a declared
    /// variable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void CatchFilterGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     try
                                     {
                                     }
                                     catch (System.ArgumentException e)when (e != null)
                                     {
                                     }
                                     catch (System.Exception)  when (b)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        try
                                        {
                                        }
                                        catch (System.ArgumentException e) when (e != null)
                                        {
                                        }
                                        catch (System.Exception) when (b)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: the space behind <c>when</c> is not decided by it, so a <c>when</c> directly in front
    /// of its parenthesized condition keeps that gap, and a spaced one keeps its space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void SpaceBehindWhenStaysAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     switch (o)
                                     {
                                         case int i when(i > 0):
                                             break;

                                         case int j when (j < 0):
                                             break;
                                     }

                                     try
                                     {
                                     }
                                     catch (System.Exception e) when(e != null)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>case</c> guard wrapped in front of <c>when</c> is joined and spaced like the same guard written on one
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedCaseGuardIsSpacedLikeTheSameGuardOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)
                                             when b:
                                             break;

                                         case "x"
                                             when b:
                                             break;

                                         case int i
                                             when b:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case (1, 2) when b:
                                                break;

                                            case "x" when b:
                                                break;

                                            case int i when b:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the pattern carries in front of <c>when</c> keeps its place, and <c>when</c> follows it after one
    /// space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfWhenKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)/* a */when b:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case (1, 2)/* a */ when b:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}