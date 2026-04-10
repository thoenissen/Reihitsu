using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Unit.Rules.Base;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — logical-expression alignment
/// </summary>
[TestClass]
public class LogicalExpressionAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single-line logical expression remains unchanged.
    /// </summary>
    [TestMethod]
    public void SingleLineExpressionRemainsUnchanged()
    {
        // Arrange
        const string input = """
        var x = a && b;
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line <c>&amp;&amp;</c> expression aligns operators to the left operand column.
    /// </summary>
    [TestMethod]
    public void MultiLineAndAlignsOperators()
    {
        // Arrange
        const string input = """
        var x = a
                  && b;
        """;

        const string expected = """
        var x = a
                && b;
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line <c>||</c> expression aligns operators to the left operand column.
    /// </summary>
    [TestMethod]
    public void MultiLineOrAlignsOperators()
    {
        // Arrange
        const string input = """
        var x = a
                  || b;
        """;

        const string expected = """
        var x = a
                || b;
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already correctly aligned expression remains unchanged.
    /// </summary>
    [TestMethod]
    public void AlreadyAlignedStaysAligned()
    {
        // Arrange
        const string input = """
        var x = a
                && b;
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that mixed <c>&amp;&amp;</c> and <c>||</c> operators in a nested expression
    /// are all aligned to the left operand column.
    /// </summary>
    [TestMethod]
    public void NestedMixedOperatorsAlignToLeftOperand()
    {
        // Arrange
        const string input = """
        var x = a
                && b
                || c;
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a non-logical binary expression (e.g., addition) has its continuation
    /// line normalized to column 0 by block indentation.
    /// </summary>
    [TestMethod]
    public void NonLogicalBinaryExpressionNormalizesToColumnZero()
    {
        // Arrange
        const string input = """
        var x = a
                  + b;
        """;

        const string expected = """
        var x = a
                + b;
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that mixed <c>&amp;&amp;</c> and <c>||</c> operators are all aligned.
    /// </summary>
    [TestMethod]
    public void MixedAndOrAlignsAllOperators()
    {
        // Arrange
        const string input = """
        var x = a
                  && b
                  || c;
        """;

        const string expected = """
        var x = a
                && b
                || c;
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line null-coalescing expression aligns the <c>??</c> operator
    /// to the left operand column.
    /// </summary>
    [TestMethod]
    public void MultiLineNullCoalescingAlignsOperator()
    {
        // Arrange
        const string input = """
        using System;

        class CustomMessageException : Exception
        {
        }

        class C
        {
            void M(Exception error)
            {
                var selectedException = error as CustomMessageException
                                        ?? error.InnerException as CustomMessageException;
            }
        }
        """;

        const string expected = """
        using System;

        class CustomMessageException : Exception
        {
        }

        class C
        {
            void M(Exception error)
            {
                var selectedException = error as CustomMessageException
                                        ?? error.InnerException as CustomMessageException;
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>||</c> inside a parenthesized sub-expression of a larger
    /// <c>&amp;&amp;</c> expression keeps its alignment to the left operand inside the parentheses.
    /// </summary>
    [TestMethod]
    public void OrInsideParenthesizedAndExpressionKeepsAlignment()
    {
        // Arrange — || is aligned with SearchChildNode (col 20), not with && (col 16)
        const string input = """
        namespace N
        {
            class C
            {
                void M(object node)
                {
                    if (node != null
                        && (SearchChildNode(node)
                            || SearchParentNode(node)))
                    {
                    }
                }
                bool SearchChildNode(object n)
                {
                    return true;
                }
                bool SearchParentNode(object n)
                {
                    return true;
                }
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that <c>or</c> pattern operators in an <c>is</c> statement
    /// align to the <c>is</c> column.
    /// </summary>
    [TestMethod]
    public void OrPatternInIsStatementAlignsToIsColumn()
    {
        // Arrange
        const string input = """
        class C
        {
            void M(object item)
            {
                if (item.ToString() is "alpha"
                      or "beta"
                           or "gamma")
                {
                }
            }
        }
        """;

        const string expected = """
        class C
        {
            void M(object item)
            {
                if (item.ToString() is "alpha"
                                       or "beta"
                                       or "gamma")
                {
                }
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that already aligned <c>or</c> pattern operators in an
    /// <c>is</c> statement remain unchanged.
    /// </summary>
    [TestMethod]
    public void AlreadyAlignedOrPatternInIsStatementRemainsUnchanged()
    {
        // Arrange
        const string input = """
        class C
        {
            void M(object item)
            {
                if (item.ToString() is "alpha"
                                       or "beta"
                                       or "gamma")
                {
                }
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that pattern <c>or</c> clauses inside a statement lambda align to the pattern anchor.
    /// </summary>
    [TestMethod]
    public void PatternOrInsideStatementLambdaAlignsToPatternAnchor()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 void M(IEnumerable<object> source)
                                 {
                                     var result = source.Select(item =>
                                     {
                                                                    if (item.ToString() is "alpha"
                                                                                           or "beta"
                                                     or "gamma")
                                         {
                                         }

                                         return item;
                                     });
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    void M(IEnumerable<object> source)
                                    {
                                        var result = source.Select(item =>
                                                                   {
                                                                       if (item.ToString() is "alpha"
                                                                                              or "beta"
                                                                                              or "gamma")
                                                                       {
                                                                       }

                                                                       return item;
                                                                   });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>&amp;&amp;</c> inside a lambda on a chain-continuation line
    /// keeps its alignment to the left operand inside the lambda.
    /// </summary>
    [TestMethod]
    public void AndInsideLambdaOnChainContinuationLineKeepsAlignment()
    {
        // Arrange
        const string input = """
        using System.Linq;
        class C
        {
            void M()
            {
                var result = source.Items
                                   .Where(x => x.Name != null
                                               && x.Value > 0);
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that <c>&amp;&amp;</c> inside a statement lambda aligns to the left operand.
    /// </summary>
    [TestMethod]
    public void AndInsideStatementLambdaAlignsToLeftOperand()
    {
        // Arrange
        const string input = """
        using System.Collections.Generic;
        using System.Linq;

        class C
        {
            void M(IEnumerable<object> source)
            {
                var result = source.Select(item =>
                {
                    if (item != null
                            && item.ToString() != null)
                    {
                    }

                    return item;
                });
            }
        }
        """;

        const string expected = """
        using System.Collections.Generic;
        using System.Linq;

        class C
        {
            void M(IEnumerable<object> source)
            {
                var result = source.Select(item =>
                                           {
                                               if (item != null
                                                   && item.ToString() != null)
                                               {
                                               }

                                               return item;
                                           });
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line ternary expression aligns <c>?</c> to the condition expression plus one indent level
    /// and aligns <c>:</c> under <c>?</c>.
    /// </summary>
    [TestMethod]
    public void MultiLineTernaryAlignsQuestionAndColonTokens()
    {
        // Arrange
        const string input = """
        class C
        {
            public TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return reader.Value != null
                  ? reader.ValueType == typeof(long)
                        ? TimeSpan.FromSeconds((long)reader.Value)
                    : TimeSpan.FromSeconds((double)reader.Value)
                      : TimeSpan.Zero;
            }
        }
        """;

        const string expected = """
        class C
        {
            public TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return reader.Value != null
                           ? reader.ValueType == typeof(long)
                               ? TimeSpan.FromSeconds((long)reader.Value)
                               : TimeSpan.FromSeconds((double)reader.Value)
                           : TimeSpan.Zero;
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already aligned multi-line ternary expression remains unchanged.
    /// </summary>
    [TestMethod]
    public void AlreadyAlignedMultiLineTernaryRemainsUnchanged()
    {
        // Arrange
        const string input = """
        class C
        {
            public TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return reader.Value != null
                           ? reader.ValueType == typeof(long)
                               ? TimeSpan.FromSeconds((long)reader.Value)
                               : TimeSpan.FromSeconds((double)reader.Value)
                           : TimeSpan.Zero;
            }
        }
        """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}