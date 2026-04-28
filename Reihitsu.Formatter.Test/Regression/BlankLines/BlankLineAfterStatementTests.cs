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

    #endregion // Methods
}