using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Tests covering tab characters that sit in the gap between two tokens on the same line. The tab
/// replacement phase turns each such tab into four spaces before any other phase runs, and the
/// horizontal spacing phase then collapses the gap to a single space, so the output is stable in one run
/// </summary>
[TestClass]
public class TabGapSpacingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single tab between two tokens is collapsed to one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void SingleTabBetweenTokensCollapsesToOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var\tx = 1;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = 1;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two tabs between two tokens are collapsed to one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void TwoTabsBetweenTokensCollapseToOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var\t\tx = 1;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = 1;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab mixed with a space between two tokens is collapsed to one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void TabFollowedBySpaceBetweenTokensCollapsesToOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var\t x = 1;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = 1;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single space between two tokens is left untouched — the other side of the
    /// boundary the collapse decision is taken on
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void SingleSpaceBetweenTokensIsLeftUnchanged()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var x = 1;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a tab in a gap a spacing rule already owns is normalized by that rule
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    [Requirement("FMT-SPACE-010")]
    public void TabBeforeBinaryOperatorIsNormalizedToSingleSpace()
    {
        // Arrange
        const string input = "class C\n{\n    void M()\n    {\n        var x = 1\t+ 2;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = 1 + 2;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab used as indentation is replaced by the tab replacement phase and the line
    /// is indented to its column rather than collapsed — the gap rule applies between tokens, not in
    /// front of the first token on a line
    /// </summary>
    [TestMethod]
    public void TabIndentationIsStillExpandedToSpaces()
    {
        // Arrange
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tint x = 1;\n\t}\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        int x = 1;\n    }\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}