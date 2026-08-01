using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for expression-bodied members whose expression is split by a conditional
/// directive. Converting such a member to a block body relocated the directives into the middle of
/// the generated statement, turning source that parsed cleanly into source that no longer compiles
/// (CS1040 plus CS1028). The transform is refused for those members instead, so the expression body
/// is left exactly as written
/// </summary>
[TestClass]
public class ExpressionBodyConditionalDirectiveGuardTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a method whose expression body is split by a conditional directive is left alone
    /// </summary>
    [TestMethod]
    public void MethodWithConditionalDirectiveIsNotConverted()
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
    /// Verifies that an indexer whose expression body is split by a conditional directive is left alone
    /// </summary>
    [TestMethod]
    public void IndexerWithConditionalDirectiveIsNotConverted()
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
    /// Verifies that a constructor whose expression body is split by a conditional directive is left alone
    /// </summary>
    [TestMethod]
    public void ConstructorWithConditionalDirectiveIsNotConverted()
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
    /// Verifies that an operator whose expression body is split by a conditional directive is left alone
    /// </summary>
    [TestMethod]
    public void OperatorWithConditionalDirectiveIsNotConverted()
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
    public void ConversionOperatorWithConditionalDirectiveIsNotConverted()
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
    /// Verifies that a finalizer whose expression body is split by a conditional directive is left alone
    /// </summary>
    [TestMethod]
    public void FinalizerWithConditionalDirectiveIsNotConverted()
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
    public void LocalFunctionWithConditionalDirectiveIsNotConverted()
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
    /// Verifies that an expression-bodied member carrying only a comment is still converted, so the
    /// guard stays limited to conditional directives
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