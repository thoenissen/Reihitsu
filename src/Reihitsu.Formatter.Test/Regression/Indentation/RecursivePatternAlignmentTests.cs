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
    /// Verifies that a multi-line recursive pattern in a case label keeps its opening brace on the
    /// case line, aligns subpatterns one level past the brace, and wraps the section body in braces
    /// </summary>
    [TestMethod]
    public void CasePatternRecursivePatternKeepsBraceOnCaseLineAndBracesBody()
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
                                            case {
                                                     Length: > 0,
                                                     Count: 0
                                                 }:
                                                {
                                                    return "a";
                                                }
                                            default:
                                                {
                                                    return "b";
                                                }
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
                                                     }
                                                     or
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
    /// Verifies that a parenthesized recursive pattern breaks the parenthesis onto its own line
    /// and indents the wrapped pattern one level inside
    /// </summary>
    [TestMethod]
    public void ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine()
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
                                        return value is
                                                     (
                                                         {
                                                             Length: > 0,
                                                             Count: 0
                                                         }
                                                     );
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

    /// <summary>
    /// Verifies that each combinator in an <c>and</c>/<c>or</c> chain of recursive patterns is placed on its own line aligned to the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void AndOrChainRecursivePatternsPlaceEachCombinatorOnOwnLine()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     A: 1 } and {
                                     B: 2 } or {
                                     C: 3 };
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
                                                         A: 1
                                                     }
                                                     and
                                                     {
                                                         B: 2
                                                     }
                                                     or
                                                     {
                                                         C: 3
                                                     };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>or</c> pattern combining a recursive pattern with a constant keeps the constant on the combinator line
    /// </summary>
    [TestMethod]
    public void OrPatternWithConstantOperandKeepsConstantWithCombinator()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 bool Check(object value)
                                 {
                                     return value is {
                                     A: 1 } or 2;
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
                                                         A: 1
                                                     }
                                                     or 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line recursive pattern in a case label is left on one line and the
    /// section body is not braced
    /// </summary>
    [TestMethod]
    public void SingleLineCasePatternRemainsInlineWithoutBodyBraces()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 string Check(object value)
                                 {
                                     switch (value)
                                     {
                                         case { Length: > 0 }:
                                             return "a";
                                         default:
                                             return "b";
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line recursive pattern in a case label with a name designation keeps
    /// the designation on the closing brace line and wraps the section body in braces
    /// </summary>
    [TestMethod]
    public void CasePatternWithDesignationKeepsNameOnCloseBraceLine()
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
                                         Count: 0 } shape:
                                             return shape.ToString();
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
                                            case {
                                                     Length: > 0,
                                                     Count: 0
                                                 } shape:
                                                {
                                                    return shape.ToString();
                                                }
                                            default:
                                                {
                                                    return "b";
                                                }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}