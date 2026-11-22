using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class BlankLineAfterStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted after a <c>break</c> statement
    /// when followed by another statement in a block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BreakInBlockInsertsBlankLineAfter()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         break;
                                         var x = 1;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        while (true)
                                        {
                                            break;

                                            var x = 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a <c>break</c> statement
    /// when it is the last statement in a block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BreakLastInBlockNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before the next case label
    /// after a <c>break</c> statement in a switch section
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BreakInSwitchSectionInsertsBlankLineBeforeNextLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             break;
                                         case 2:
                                             break;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                break;

                                            case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a <c>break</c> statement
    /// in the last section of a switch statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BreakInSwitchSectionLastSectionNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no duplicate blank line is inserted when a blank line
    /// already exists after the <c>break</c> statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void AlreadyHasBlankLineNoDoubleInsert()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         break;

                                         var x = 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a non-break statement
    /// such as <c>return</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void NonBreakStatementNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         return;

                                         var x = 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>break</c> in a nested block correctly gets a blank
    /// line after it when followed by another statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void NestedBlocksHandlesCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         if (true)
                                         {
                                             break;
                                             var y = 2;
                                         }
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        while (true)
                                        {
                                            if (true)
                                            {
                                                break;

                                                var y = 2;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above an <c>#endif</c> directive that immediately
    /// follows a <c>break</c> statement, since that would land the blank line inside the conditional
    /// region rather than outside it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    public void BreakFollowedByDirectiveNoBlankLineInsertedInsideRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                             #if true
                                         break;
                             #endif
                                         var x = 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}