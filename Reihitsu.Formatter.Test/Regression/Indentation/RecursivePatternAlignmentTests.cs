using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — recursive (property) pattern alignment
/// </summary>
[TestClass]
public class RecursivePatternAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single-line recursive pattern is left untouched
    /// </summary>
    [TestMethod]
    public void SingleLineRecursivePatternRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is { Length: > 0 };
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line recursive pattern in an <c>is</c> expression aligns its braces to the <c>is</c> keyword and indents subpatterns by +4
    /// </summary>
    [TestMethod]
    public void MultiLineIsPatternAlignsBracesToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     Length: > 0,
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is
                                                     {
                                                         Length: > 0,
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a typed recursive pattern in an <c>is</c> expression aligns its braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void MultiLineIsPatternWithTypeAlignsBracesToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is Foo {
                                     Length: > 0,
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is Foo
                                                     {
                                                         Length: > 0,
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested recursive pattern aligns its braces to the containing subpattern and indents its own subpatterns by +4
    /// </summary>
    [TestMethod]
    public void NestedRecursivePatternIndentsFromContainingSubpattern()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     Inner: {
                                     A: 1,
                                     B: 2 },
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is
                                                     {
                                                         Inner:
                                                         {
                                                             A: 1,
                                                             B: 2
                                                         },
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a short nested recursive pattern stays on a single line while the outer pattern wraps
    /// </summary>
    [TestMethod]
    public void ShortNestedRecursivePatternStaysOnOneLine()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     Inner: { A: 1 },
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is
                                                     {
                                                         Inner: { A: 1 },
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line recursive pattern in a switch expression arm aligns its braces to the arm and indents subpatterns by +4
    /// </summary>
    [TestMethod]
    public void SwitchExpressionArmRecursivePatternAligns()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 string Check(object value)
                                 {
                                     return value switch
                                     {
                                         { Length: > 0,
                                     Count: 0 } => "a",
                                         _ => "b"
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    string Check(object value)
                                    {
                                        return value switch
                                               {
                                                   {
                                                       Length: > 0,
                                                       Count: 0
                                                   } => "a",
                                                   _ => "b"
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line recursive pattern in a case label aligns its braces to the <c>case</c> keyword and indents subpatterns by +4
    /// </summary>
    [TestMethod]
    public void CasePatternRecursivePatternAligns()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 string Check(object value)
                                 {
                                     switch (value)
                                     {
                                         case { Length: > 0,
                                         Count: 0 }:
                                             return "a";
                                         default:
                                             return "b";
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    string Check(object value)
                                    {
                                        switch (value)
                                        {
                                            case
                                            {
                                                Length: > 0,
                                                Count: 0
                                            }:
                                                return "a";
                                            default:
                                                return "b";
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that both operands of an <c>or</c> pattern in an <c>is</c> expression align their braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void OrPatternRecursivePatternsAlignToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     Length: > 0 } or {
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is
                                                     {
                                                         Length: > 0
                                                     } or
                                                     {
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line recursive pattern in an <c>if</c> condition aligns its braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void IfConditionRecursivePatternAligns()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 void Check(object value)
                                 {
                                     if (value is {
                                     Length: > 0,
                                     Count: 0 })
                                     {
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    void Check(object value)
                                    {
                                        if (value is
                                                  {
                                                      Length: > 0,
                                                      Count: 0
                                                  })
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a recursive pattern wrapped in a <c>not</c> pattern aligns its braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void MultiLineNotPatternAlignsBracesToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is not {
                                     Length: > 0,
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is not
                                                     {
                                                         Length: > 0,
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parenthesized recursive pattern aligns its braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void ParenthesizedRecursivePatternAlignsBracesToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is ({
                                     Length: > 0,
                                     Count: 0 });
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is (
                                                     {
                                                         Length: > 0,
                                                         Count: 0
                                                     });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a recursive pattern that also has a positional clause aligns its braces to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void RecursivePatternWithPositionalClauseAlignsBracesToIsKeyword()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is (1, 2) {
                                     Length: > 0,
                                     Count: 0 };
                                 }
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    bool Check(object value)
                                    {
                                        return value is (1, 2)
                                                     {
                                                         Length: > 0,
                                                         Count: 0
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}