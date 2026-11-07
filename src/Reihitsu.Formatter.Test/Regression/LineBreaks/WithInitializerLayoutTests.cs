using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the initializer of a <c>with</c> expression, which reaches the layout of an
/// object initializer in the first pass
/// </summary>
[TestClass]
public class WithInitializerLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specified example: a <c>with</c> initializer in an expression-bodied property and in a
    /// local declaration gets the opening brace, every element, and the closing brace on their own lines
    /// in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    [Requirement("FMT-PIPE-004")]
    public void WithInitializersAreLaidOutOverSeveralLinesInTheFirstPass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 R P => _r with { A = 1, B = 2 };

                                 void M()
                                 {
                                     var q = p with { X = 1, Y = 2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    R P => _r with
                                              {
                                                  A = 1,
                                                  B = 2
                                              };

                                    void M()
                                    {
                                        var q = p with
                                                  {
                                                      X = 1,
                                                      Y = 2
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported shape: the <c>with</c> initializer of an expression-bodied property reaches
    /// its layout in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    [Requirement("FMT-PIPE-004")]
    public void WithInitializerOfExpressionBodiedPropertyIsLaidOutInTheFirstPass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 R P => _r with { A = 1, B = 2 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    R P => _r with
                                              {
                                                  A = 1,
                                                  B = 2
                                              };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>with</c> initializer of a <c>return</c> statement reaches its layout in the
    /// first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    [Requirement("FMT-PIPE-004")]
    public void WithInitializerOfReturnStatementIsLaidOutInTheFirstPass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 R M(R p)
                                 {
                                     return p with { A = 1, B = 2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    R M(R p)
                                    {
                                        return p with
                                                 {
                                                     A = 1,
                                                     B = 2
                                                 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied method returning a <c>with</c> expression is converted and its
    /// initializer reaches its layout in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    [Requirement("FMT-PIPE-004")]
    public void WithInitializerOfConvertedExpressionBodiedMethodIsLaidOutInTheFirstPass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 R M(R p) => p with { A = 1, B = 2 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    R M(R p)
                                    {
                                        return p with
                                                 {
                                                     A = 1,
                                                     B = 2
                                                 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-element <c>with</c> initializer whose element holds an object creation with
    /// its own initializer reaches its layout in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    [Requirement("FMT-PIPE-004")]
    public void WithInitializerWithNestedObjectInitializerIsLaidOutInTheFirstPass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(R p)
                                 {
                                     var q = p with { A = new X(1, 2) { B = 1, C = 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(R p)
                                    {
                                        var q = p with
                                                  {
                                                      A = new X(1, 2)
                                                          {
                                                              B = 1,
                                                              C = 2
                                                          }
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-element <c>with</c> initializer is laid out over several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    public void OneElementWithInitializerIsLaidOutOverSeveralLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(R p)
                                 {
                                     var q = p with { A = 1 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(R p)
                                    {
                                        var q = p with
                                                  {
                                                      A = 1
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with</c> initializer already laid out over several lines is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    public void LaidOutWithInitializerIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(R p)
                                 {
                                     var q = p with
                                               {
                                                   A = 1,
                                                   B = 2
                                               };
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}