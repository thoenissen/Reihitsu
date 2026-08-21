using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Formatter regression tests for the interpolated-string alignment comma (issue #696): the comma
/// separating the interpolated expression from its alignment component is a fixed grammatical
/// delimiter, not a value separator, and must stay compact regardless of the alignment's sign, operand
/// shape, or the presence of a format specifier
/// </summary>
[TestClass]
public class InterpolationAlignmentSpacingReproTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Literal scenario from issue #696: a negative alignment component should stay adjacent to the comma
    /// </summary>
    [TestMethod]
    public void NegativeAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{"Foo",-50}";
                             }
                             """;

        // Act & Assert (expected: no change, per the issue's claim)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a positive alignment component
    /// </summary>
    [TestMethod]
    public void PositiveAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{"Foo",50}";
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: negative alignment component combined with a format specifier
    /// </summary>
    [TestMethod]
    public void NegativeAlignmentComponentWithFormatSpecifierStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{42,-5:X}";
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: negative alignment component on a simple identifier expression, without a
    /// nested string literal
    /// </summary>
    [TestMethod]
    public void NegativeAlignmentComponentOnIdentifierStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(int value)
                                 {
                                     return $"{value,-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a zero alignment component
    /// </summary>
    [TestMethod]
    public void ZeroAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{"Foo",0}";
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// A format specifier without an alignment component carries no comma and must stay untouched
    /// </summary>
    [TestMethod]
    public void FormatSpecifierWithoutAlignmentStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{42:X}";
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// The formatter's own current (pre-fix) output must be normalized back to a compact comma
    /// </summary>
    [TestMethod]
    public void SingleSpaceAfterAlignmentCommaIsRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{value, -10}";
                             }
                             """;
        const string expected = """
                                public class Implementation
                                {
                                    public string Value { get; set; } = $"{value,-10}";
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Several spaces after the alignment comma collapse to a compact comma, mirroring how
    /// <c>int[,   ]</c> collapses to <c>int[,]</c>
    /// </summary>
    [TestMethod]
    public void MultipleSpacesAfterAlignmentCommaAreCollapsed()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{value,   -10}";
                             }
                             """;
        const string expected = """
                                public class Implementation
                                {
                                    public string Value { get; set; } = $"{value,-10}";
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// A stray space before the alignment comma is removed independently of the after-comma fix,
    /// because the before-gap is owned by a different spacing rule
    /// </summary>
    [TestMethod]
    public void SpaceBeforeAlignmentCommaIsRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{value ,-10}";
                             }
                             """;
        const string expected = """
                                public class Implementation
                                {
                                    public string Value { get; set; } = $"{value,-10}";
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Stray spaces on both sides of the alignment comma are removed together
    /// </summary>
    [TestMethod]
    public void SpacesOnBothSidesOfAlignmentCommaAreRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{value , -10}";
                             }
                             """;
        const string expected = """
                                public class Implementation
                                {
                                    public string Value { get; set; } = $"{value,-10}";
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Sibling shape: a parenthesized alignment expression stays adjacent to the comma
    /// </summary>
    [TestMethod]
    public void ParenthesizedAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Value { get; set; } = $"{value,(-10)}";
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a member-access operand stays adjacent to the comma
    /// </summary>
    [TestMethod]
    public void MemberAccessOperandAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(object value)
                                 {
                                     return $"{value.ToString(),-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a nested interpolated string as the operand keeps both holes compact
    /// </summary>
    [TestMethod]
    public void NestedInterpolationHoleAlignmentComponentsStayAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $"{$"{value,-3}",-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a verbatim interpolated string keeps the alignment comma compact
    /// </summary>
    [TestMethod]
    public void VerbatimInterpolatedStringAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $@"{value,-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Sibling shape: a raw-string interpolated string keeps the alignment comma compact
    /// </summary>
    [TestMethod]
    public void RawStringInterpolatedStringAlignmentComponentStaysAdjacentToComma()
    {
        // Arrange
        const string input = """"
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $$"""{{value,-10}}""";
                                 }
                             }
                             """";

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// A comment before the alignment comma leaves the comma compact and preserves the space in front
    /// of the comment
    /// </summary>
    [TestMethod]
    public void CommentBeforeAlignmentCommaStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $"{value /* comment */,-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// A comment attached to the alignment comma's trailing trivia still loses the space between the
    /// comment and the alignment component, because the comma's desired spacing after it is now zero
    /// </summary>
    [TestMethod]
    public void SpaceAfterCommentFollowingAlignmentCommaIsRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $"{value,/* comment */ -10}";
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation
                                {
                                    public string Format(string value)
                                    {
                                        return $"{value,/* comment */-10}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// A comma and its alignment component separated by a line break inside the interpolation hole
    /// receive no spacing decision at all, and the surrounding layout is left untouched
    /// </summary>
    [TestMethod]
    public void AlignmentCommaSeparatedFromAlignmentByLineBreakStaysUnchanged()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public string Format(string value)
                                 {
                                     return $"{value
                                     ,-10}";
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    /// <summary>
    /// Non-alignment commas keep exactly one space after them; the alignment-comma exemption must not
    /// widen to ordinary argument or array-initializer commas
    /// </summary>
    [TestMethod]
    public void NonAlignmentCommasStayUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     M(1, 2);
                                     _ = new int[1, 2];
                                 }

                                 private static void M(int a, int b)
                                 {
                                 }
                             }
                             """;

        // Act & Assert (expected: no change)
        AssertRuleResult(input);
    }

    #endregion // Methods
}