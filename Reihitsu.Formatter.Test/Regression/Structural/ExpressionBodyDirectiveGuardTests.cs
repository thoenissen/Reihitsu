using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for expression-bodied members carrying a preprocessor directive between the
/// arrow and the expression. Converting such a member to a block body re-hosted the directive
/// directly after the generated <c>return</c>, where a directive cannot sit because it must start
/// its own line, so source that parsed cleanly became source that no longer compiles. The transform
/// is refused for those members. A directive that follows the expression travels inside the
/// generated statement and is still converted
/// </summary>
[TestClass]
public class ExpressionBodyDirectiveGuardTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a method whose expression body carries a directive before the expression is left alone
    /// </summary>
    [TestMethod]
    public void MethodWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() =>
                             #if DEBUG
                                     1;
                             #else
                                 0;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an indexer whose expression body carries a directive before the expression is left alone
    /// </summary>
    [TestMethod]
    public void IndexerWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int this[int index] =>
                             #if DEBUG
                                     1;
                             #else
                                 0;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a constructor whose expression body carries a directive before the expression is left alone
    /// </summary>
    [TestMethod]
    public void ConstructorWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public C() =>
                             #if DEBUG
                                     _value = 1;
                             #else
                                 _value = 0;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an operator whose expression body carries a directive before the expression is left alone
    /// </summary>
    [TestMethod]
    public void OperatorWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C left, C right) =>
                             #if DEBUG
                                     left;
                             #else
                                 right;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conversion operator whose expression body is split by a conditional directive
    /// is left alone
    /// </summary>
    [TestMethod]
    public void ConversionOperatorWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static implicit operator int(C value) =>
                             #if DEBUG
                                     1;
                             #else
                                 0;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a finalizer whose expression body carries a directive before the expression is left alone
    /// </summary>
    [TestMethod]
    public void FinalizerWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 ~C() =>
                             #if DEBUG
                                     _value = 1;
                             #else
                                 _value = 0;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local function whose expression body is split by a conditional directive is
    /// left alone
    /// </summary>
    [TestMethod]
    public void LocalFunctionWithDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public void M()
                                 {
                                     int Inner() =>
                             #if DEBUG
                                         1;
                             #else
                                     0;
                             #endif

                                     Inner();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pragma directive before the expression is refused, so the guard is not limited
    /// to conditional directives
    /// </summary>
    [TestMethod]
    public void MethodWithPragmaBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() =>
                             #pragma warning disable CS0618
                                 1;
                             #pragma warning restore CS0618
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a nullable directive before the expression is refused
    /// </summary>
    [TestMethod]
    public void MethodWithNullableDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public string M() =>
                             #nullable disable
                                 null;
                             #nullable restore
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a line directive before the expression is refused
    /// </summary>
    [TestMethod]
    public void MethodWithLineDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() =>
                             #line 200
                                 1;
                             #line default
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region directive before the expression is refused. Converting it glued the
    /// directive onto the declaration line, and the region phase then removed that whole line
    /// </summary>
    [TestMethod]
    public void MethodWithRegionDirectiveBeforeExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() =>

                                 #region Body

                                 1;

                                 #endregion // Body
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive following the expression is still converted, because it travels
    /// inside the generated statement and keeps its own line
    /// </summary>
    [TestMethod]
    public void MethodWithDirectiveAfterExpressionIsStillConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo() => 42
                             #pragma warning disable CS0168
                                     ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Foo()
                                    {
                                        return 42
                                #pragma warning disable CS0168
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional group lying wholly between the expression and the semicolon is
    /// still converted, because the whole group travels into the generated statement
    /// </summary>
    [TestMethod]
    public void MethodWithBalancedConditionalAfterExpressionIsStillConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() => 1
                             #if DEBUG
                                                   + 2
                             #endif
                                                   ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int M()
                                    {
                                        return 1
                                #if DEBUG
                                                      + 2
                                #endif
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied member carrying only a comment is still converted, so the
    /// guard stays limited to directives
    /// </summary>
    [TestMethod]
    public void MethodWithInlineCommentIsStillConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() => /* inline */ 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int M()
                                    {/* inline */
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}