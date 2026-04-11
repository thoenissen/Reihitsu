using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Formatter.Pipeline.SwitchCaseBraces;

namespace Reihitsu.Formatter.Test.Unit.SwitchCaseBraces;

/// <summary>
/// Tests for <see cref="SwitchCaseBraceRewriter"/> and <see cref="SwitchCaseBracePhase"/>.
/// </summary>
[TestClass]
public class SwitchCaseBraceRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that braces are added to all case sections when at least one section
    /// contains multiple non-terminal statements (multi-line).
    /// </summary>
    [TestMethod]
    public void AddsBracesToMultiLineSwitchSection()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            var a = 1;
                            Console.WriteLine(a);
                            break;
                        case 2:
                            Console.WriteLine(2);
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyPhase(input);

        // Assert — both sections must have braces because case 1 is multi-line
        Assert.Contains("{", actual, "Expected braces to be added.");

        // Verify case 1 got a block
        Assert.IsGreaterThan(3, CountOccurrences(actual, "{"), "Expected additional braces from wrapping case bodies.");
    }

    /// <summary>
    /// Verifies that braces are not added when all case sections are single-line
    /// (each section has at most one non-terminal statement that fits on one line).
    /// </summary>
    [TestMethod]
    public void DoesNotAddBracesToSingleLineSwitchSection()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            Console.WriteLine(1);
                            break;
                        case 2:
                            Console.WriteLine(2);
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyPhase(input);

        // Assert — no extra braces should be added; brace count should stay at 3 pairs
        // (class, method, switch)
        Assert.AreEqual(3, CountOccurrences(actual, "{"), "No additional braces should be added for single-line sections.");
    }

    /// <summary>
    /// Verifies that existing braces are preserved when the switch has multi-line sections
    /// that are already wrapped in blocks.
    /// </summary>
    [TestMethod]
    public void PreservesExistingBraces()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                        {
                            var a = 1;
                            Console.WriteLine(a);
                            break;
                        }
                        case 2:
                        {
                            Console.WriteLine(2);
                            break;
                        }
                    }
                }
            }
            """;

        var expected = input;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual, "Already-braced multi-line sections should be unchanged.");
    }

    /// <summary>
    /// Verifies that braces are preserved when sections are wrapped in a block
    /// that spans multiple lines, even if the body contains only a single statement.
    /// </summary>
    [TestMethod]
    public void RemovesBracesFromSingleLineSection()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                        {
                            Console.WriteLine(1);
                        }
                            break;
                        case 2:
                        {
                            Console.WriteLine(2);
                        }
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyPhase(input);

        // Assert — braces are preserved because the existing blocks span multiple lines,
        // making the sections count as multi-line
        Assert.AreEqual(5, CountOccurrences(actual, "{"), "Braces should be preserved since block content spans multiple lines.");
    }

    /// <summary>
    /// Verifies that fall-through sections (case labels with no statements) are left
    /// unchanged and do not interfere with brace insertion on other sections.
    /// </summary>
    [TestMethod]
    public void HandlesFallThroughSection()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                        case 2:
                            var a = 1;
                            Console.WriteLine(a);
                            break;
                        case 3:
                            Console.WriteLine(3);
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyPhase(input);

        // Assert — case 1 is fall-through (no statements), should be left as-is
        // case 2 is multi-line, so case 2 and case 3 get braces
        Assert.Contains("case 1:", actual, "Fall-through label should be preserved.");
        Assert.IsGreaterThan(3, CountOccurrences(actual, "{"), "Braces should be added to non-fall-through sections.");
    }

    /// <summary>
    /// Verifies that the default section is handled correctly and receives braces when
    /// the switch contains multi-line sections.
    /// </summary>
    [TestMethod]
    public void HandlesDefaultSection()
    {
        // Arrange
        const string input =
            """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            var a = 1;
                            Console.WriteLine(a);
                            break;
                        default:
                            Console.WriteLine(0);
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyPhase(input);

        // Assert — both case 1 and default should have braces
        Assert.IsGreaterThan(3, CountOccurrences(actual, "{"), "Default section should also receive braces.");
    }

    /// <summary>
    /// Counts the number of non-overlapping occurrences of a substring in a string.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="value">The substring to count.</param>
    /// <returns>The number of occurrences.</returns>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    /// <summary>
    /// Applies <see cref="SwitchCaseBracePhase"/> to the given input source code and
    /// returns the resulting full string.
    /// </summary>
    /// <param name="input">The C# source code to format.</param>
    /// <returns>The formatted source code as a string.</returns>
    private string ApplyPhase(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = SwitchCaseBracePhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);

        return result.ToFullString();
    }

    #endregion // Methods
}