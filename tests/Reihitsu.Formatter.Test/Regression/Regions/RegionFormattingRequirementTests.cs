using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Full-pipeline tests for the region-formatting behavior no narrower test pins: non-ASCII and digit-led
/// descriptions, a block comment after <c>#endregion</c>, the label of an undescribed region, and a region that
/// straddles a member body
/// </summary>
[TestClass]
public class RegionFormattingRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a non-ASCII lower-case first letter is upper-cased, that the rest of the description is kept as
    /// written, and that the rewritten line carries one space and the trimmed description
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-001")]
    public void NonAsciiLowerCaseFirstLetterIsUpperCased()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region   äpfel und birnen

                                 int _value;

                                 #endregion // Äpfel und birnen
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Äpfel und birnen

                                    int _value;

                                    #endregion // Äpfel und birnen
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a description starting with a digit keeps its first character
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-001")]
    public void DescriptionStartingWithDigitIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region 1st fields

                                 int _value;

                                 #endregion // 1st fields
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment after <c>#endregion</c> is replaced by the line-comment form of the region
    /// description
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-002")]
    public void BlockCommentAfterEndRegionIsReplaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Fields

                                 int _value;

                                 #endregion /* Fields */
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    int _value;

                                    #endregion // Fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the text after the <c>#endregion</c> of a region without a description is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TextAfterEndRegionOfUndescribedRegionIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 int _value;

                                 #endregion // Stale
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region opened among the members and closed inside a method body keeps both directives in
    /// place and is still named
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-005")]
    public void RegionClosedInsideMethodBodyKeepsItsDirectivesAndIsNamed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region members

                                 void M()
                                 {
                                     var x = 1;

                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Members

                                    void M()
                                    {
                                        var x = 1;

                                        #endregion // Members
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}