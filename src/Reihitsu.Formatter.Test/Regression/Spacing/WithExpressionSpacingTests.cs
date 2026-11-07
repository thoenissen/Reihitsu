using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the <c>with</c> of a <c>with</c> expression: one space on each side of it, whatever its left operand
/// ends in and whether the author wrote it on one line or the line-break phase joined it
/// </summary>
[TestClass]
public class WithExpressionSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that <c>with</c> behind an invocation gets one space in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithBehindAnInvocationGetsOneSpaceInFrontOfIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     var a = p.Q()with { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        var a = p.Q() with
                                                      {
                                                      };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>with</c> behind an element access, a null-forgiving operator, and an object creation gets one space in
    /// front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithBehindOtherNonWordTokensGetsOneSpaceInFrontOfIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p, P[] ps)
                                 {
                                     var a = ps[0]with { };
                                     var b = p!with { };
                                     var c = new P()with { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p, P[] ps)
                                    {
                                        var a = ps[0] with
                                                      {
                                                      };
                                        var b = p! with
                                                   {
                                                   };
                                        var c = new P() with
                                                        {
                                                        };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that runs of whitespace on either side of <c>with</c> become one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WhitespaceRunsAroundWithAreCollapsed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     var a = p   with   { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        var a = p with
                                                  {
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with</c> expression with members in its initializer gets one space in front of <c>with</c>, while its
    /// initializer is laid out below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithInFrontOfANonEmptyInitializerGetsOneSpaceInFrontOfIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     var a = p.Q()with { X = 1 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        var a = p.Q() with
                                                      {
                                                          X = 1
                                                      };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with</c> expression wrapped in front of <c>with</c> inside an argument is joined and spaced like the
    /// same expression on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedWithIsSpacedLikeTheSameWithOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     N(p.Q()
                                       with { });
                                     N(p.Q()with { });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        N(p.Q() with
                                                {
                                                });
                                        N(p.Q() with
                                                {
                                                });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with</c> expression wrapped in front of <c>with</c> behind an element access inside an argument is joined
    /// and spaced like the same expression on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedWithBehindAnElementAccessIsSpacedLikeTheSameWithOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P[] ps)
                                 {
                                     N(ps[0]
                                       with { });
                                     N(ps[0]with { });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P[] ps)
                                    {
                                        N(ps[0] with
                                                {
                                                });
                                        N(ps[0] with
                                                {
                                                });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the left operand carries keeps its place, and <c>with</c> follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfWithKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     var a = p/* a */with { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        var a = p/* a */ with
                                                         {
                                                         };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment <c>with</c> carries keeps the whitespace in front of it, and <c>with</c>
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfWithKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p)
                                 {
                                     var a = p /** a */with { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p)
                                    {
                                        var a = p /** a */ with
                                                           {
                                                           };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}