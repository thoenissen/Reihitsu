using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests covering the blank line between two braced switch sections. Brace insertion keeps the
/// terminal break outside the block, so the break-spacing rule sees it as a direct section
/// statement; a break written inside the block is the shape RH7501 reports and RH5011 exempts,
/// and the formatter leaves it to the code fix rather than spacing around it
/// </summary>
[TestClass]
public class BracedSwitchSectionBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted between two braced switch sections
    /// </summary>
    [TestMethod]
    public void BracedSwitchSectionsAreSeparatedByBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             {
                                                 M(0);
                                             }

                                             break;
                                         case 2:
                                             {
                                                 M(1);
                                             }

                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    M(0);
                                                }

                                                break;

                                            case 2:
                                                {
                                                    M(1);
                                                }

                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between two unbraced switch sections
    /// </summary>
    [TestMethod]
    public void UnbracedSwitchSectionsAreSeparatedByBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             M(0);
                                             break;
                                         case 2:
                                             M(1);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                M(0);
                                                break;

                                            case 2:
                                                M(1);
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break written inside an explicit case block is left alone. Moving it out is
    /// RH7501's code fix, and adding a blank line around the shape it reports would format code the
    /// repository asks the author to delete
    /// </summary>
    [TestMethod]
    public void BreakInsideExplicitCaseBlockIsLeftToRH7501()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             {
                                                 M(0);

                                                 break;
                                             }
                                         case 2:
                                             {
                                                 M(1);

                                                 break;
                                             }
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}