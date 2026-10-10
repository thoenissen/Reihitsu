using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Tests covering the gap in front of a member-access symbol. The chain rewriter already tightens
/// the gap that follows a conditional-access or pointer member access, so only the leading gap is
/// left for the horizontal spacing phase; leaving it out produced <c>x ?.Foo</c> and <c>p -&gt;V</c>
/// </summary>
[TestClass]
public class MemberAccessSymbolSpacingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the space in front of a conditional-access operator is removed
    /// </summary>
    [TestMethod]
    public void SpaceBeforeConditionalAccessIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(string value)
                                 {
                                     _ = value ?.Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string value)
                                    {
                                        _ = value?.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that spaces on both sides of a conditional-access operator are removed
    /// </summary>
    [TestMethod]
    public void SpacesAroundConditionalAccessAreRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(string value)
                                 {
                                     _ = value ?. Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string value)
                                    {
                                        _ = value?.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space in front of a conditional element access is removed
    /// </summary>
    [TestMethod]
    public void SpaceBeforeConditionalElementAccessIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] values)
                                 {
                                     _ = values ?[0];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[] values)
                                    {
                                        _ = values?[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space in front of a conditional invocation is removed
    /// </summary>
    [TestMethod]
    public void SpaceBeforeConditionalInvocationIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(C other)
                                 {
                                     other ?.M(null);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(C other)
                                    {
                                        other?.M(null);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that spaces around a pointer member access are removed
    /// </summary>
    [TestMethod]
    public void SpacesAroundPointerMemberAccessAreRemoved()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 struct Data
                                 {
                                     public int Value;
                                 }

                                 void M(Data* pointer)
                                 {
                                     _ = pointer -> Value;
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    struct Data
                                    {
                                        public int Value;
                                    }

                                    void M(Data* pointer)
                                    {
                                        _ = pointer->Value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already tight conditional access is left unchanged
    /// </summary>
    [TestMethod]
    public void TightConditionalAccessIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(string value)
                                 {
                                     _ = value?.Length;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the question mark of a ternary keeps its surrounding spaces — the other side of
    /// the boundary the conditional-access predicate is taken on
    /// </summary>
    [TestMethod]
    public void TernaryQuestionMarkKeepsItsSpaces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool flag)
                                 {
                                     _ = flag ? 1 : 2;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a nullable type question mark stays attached to its type
    /// </summary>
    [TestMethod]
    public void NullableTypeQuestionMarkStaysAttached()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int? value = null;

                                     _ = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional access chain broken across lines is aligned by the chain rewriter
    /// rather than joined by the spacing phase, which only owns gaps between tokens that share a line
    /// </summary>
    [TestMethod]
    public void ConditionalAccessAcrossLinesIsAligned()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(string value)
                                 {
                                     _ = value
                                         ?.Trim()
                                         ?.Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string value)
                                    {
                                        _ = value?.Trim()
                                                 ?.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an aligned conditional access chain is left untouched
    /// </summary>
    [TestMethod]
    public void AlignedConditionalAccessChainIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(string value)
                                 {
                                     _ = value?.Trim()
                                              ?.Length;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}