using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Reproduction-gate tests for issue #696: the formatter is reported to insert a space between the
/// comma and a negative alignment component of an interpolated-string alignment clause
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

    #endregion // Methods
}