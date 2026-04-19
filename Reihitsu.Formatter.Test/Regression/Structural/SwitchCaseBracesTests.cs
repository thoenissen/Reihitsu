using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — switch-case brace insertion
/// </summary>
[TestClass]
public class SwitchCaseBracesTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that braces are correctly added around multi-statement type-pattern switch sections.
    /// </summary>
    [TestMethod]
    public void TypePatternCaseWithMultipleStatementsAddsBracesCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object value)
                                 {
                                     var current = value;

                                     while (current != null)
                                     {
                                         switch (current)
                                         {
                                             case string:
                                             case int:
                                                 current = current.ToString();
                                                 continue;
                                             case bool:
                                             case double:
                                                 return true;
                                         }

                                         break;
                                     }

                                     return false;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    bool M(object value)
                                    {
                                        var current = value;

                                        while (current != null)
                                        {
                                            switch (current)
                                            {
                                                case string:
                                                case int:
                                                    {
                                                        current = current.ToString();

                                                        continue;
                                                    }
                                                case bool:
                                                case double:
                                                    {
                                                        return true;
                                                    }
                                            }

                                            break;
                                        }

                                        return false;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when braces are added, the break statement is placed outside the block.
    /// </summary>
    [TestMethod]
    public void BreakStatementIsPlacedOutsideBracesBlock()
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
                                             Alpha();
                                             Beta();
                                             break;
                                         case 2:
                                             Gamma();
                                             Delta();
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
                                                {
                                                    Alpha();
                                                    Beta();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Gamma();
                                                    Delta();
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
    /// Verifies that when braces are added and the case ends with return instead of break,
    /// the return stays inside the block.
    /// </summary>
    [TestMethod]
    public void ReturnStatementRemainsInsideBracesBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             var a = 1;
                                             Console.Write(a);
                                             return a;
                                         case 2:
                                             var b = 2;
                                             Console.Write(b);
                                             return b;
                                     }

                                     return 0;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    var a = 1;
                                                    Console.Write(a);

                                                    return a;
                                                }
                                            case 2:
                                                {
                                                    var b = 2;
                                                    Console.Write(b);

                                                    return b;
                                                }
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}