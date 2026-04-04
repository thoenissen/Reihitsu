using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.Rules.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineAfterStatementRule"/>
/// </summary>
[TestClass]
public class BlankLineAfterStatementRuleTests
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted after a <c>break</c> statement
    /// when followed by another statement in a block.
    /// </summary>
    [TestMethod]
    public void BreakInBlockInsertsBlankLineAfter()
    {
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

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a <c>break</c> statement
    /// when it is the last statement in a block.
    /// </summary>
    [TestMethod]
    public void BreakLastInBlockNoBlankLine()
    {
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

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before the next case label
    /// after a <c>break</c> statement in a switch section.
    /// </summary>
    [TestMethod]
    public void BreakInSwitchSectionInsertsBlankLineBeforeNextLabel()
    {
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

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a <c>break</c> statement
    /// in the last section of a switch statement.
    /// </summary>
    [TestMethod]
    public void BreakInSwitchSectionLastSectionNoBlankLine()
    {
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

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that no duplicate blank line is inserted when a blank line
    /// already exists after the <c>break</c> statement.
    /// </summary>
    [TestMethod]
    public void AlreadyHasBlankLineNoDoubleInsert()
    {
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

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a non-break statement
    /// such as <c>return</c>.
    /// </summary>
    [TestMethod]
    public void NonBreakStatementNoBlankLine()
    {
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

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that a <c>break</c> in a nested block correctly gets a blank
    /// line after it when followed by another statement.
    /// </summary>
    [TestMethod]
    public void NestedBlocksHandlesCorrectly()
    {
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

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="BlankLineAfterStatementRule.Phase"/> returns
    /// <see cref="FormattingPhase.BlankLineManagement"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsBlankLineManagement()
    {
        var context = new FormattingContext("\n");
        var rule = new BlankLineAfterStatementRule(context, CancellationToken.None);

        Assert.AreEqual(FormattingPhase.BlankLineManagement, rule.Phase);
    }

    /// <summary>
    /// Applies the <see cref="BlankLineAfterStatementRule"/> to the given input
    /// and asserts the result matches the expected output.
    /// </summary>
    /// <param name="input">The input source code.</param>
    /// <param name="expected">The expected output source code.</param>
    private static void AssertRule(string input, string expected)
    {
        input = input.Replace("\r\n", "\n");
        expected = expected.Replace("\r\n", "\n");

        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new BlankLineAfterStatementRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}