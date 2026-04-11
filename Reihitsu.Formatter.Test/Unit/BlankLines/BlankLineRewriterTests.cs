using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineRewriter"/>
/// </summary>
[TestClass]
public class BlankLineRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted before a try statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeTryStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    try { } catch { }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    try { } catch { }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before an if statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeIfStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    if (x > 0) { }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    if (x > 0) { }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a return statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeReturnStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                int M()
                {
                    var x = 1;
                    return x;
                }
            }
            """;
        const string expected = """
            class C
            {
                int M()
                {
                    var x = 1;

                    return x;
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a foreach statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeForEachStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var items = new int[0];
                    foreach (var item in items) { }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var items = new int[0];

                    foreach (var item in items) { }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a while statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeWhileStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    while (x > 0) { x--; }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    while (x > 0) { x--; }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a throw statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeThrowStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    throw new System.Exception();
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    throw new System.Exception();
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a switch statement when preceded by another statement.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeSwitchStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    switch (x)
                    {
                        case 1:
                            break;
                    }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    switch (x)
                    {
                        case 1:
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that no blank line is inserted before the first statement in a block.
    /// </summary>
    [TestMethod]
    public void DoesNotInsertBlankLineForFirstStatementInBlock()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    return;
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after an else keyword before a statement.
    /// </summary>
    [TestMethod]
    public void DoesNotInsertBlankLineAfterElseKeyword()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    }
                    else
                    {
                        return;
                    }
                }
            }
            """;

        // The return is the first statement in the else block, so no blank line should be added.

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that no blank line is inserted after a case label for the first statement in the section.
    /// </summary>
    [TestMethod]
    public void DoesNotInsertBlankLineAfterCaseLabel()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    switch (1)
                    {
                        case 1:
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that blank lines after an opening brace are removed.
    /// </summary>
    [TestMethod]
    public void RemovesBlankLineAfterOpenBrace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {

                    var x = 1;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that no duplicate blank line is inserted when one already exists before a statement.
    /// </summary>
    [TestMethod]
    public void PreservesExistingBlankLineBeforeStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                int M()
                {
                    var x = 1;

                    return x;
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that blank lines are inserted between switch sections when the previous section ends with a break.
    /// </summary>
    [TestMethod]
    public void HandlesSwitchSectionBlankLines()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    switch (1)
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
                void M()
                {
                    switch (1)
                    {
                        case 1:
                            break;

                        case 2:
                            break;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that blank lines are correctly inserted in nested blocks.
    /// </summary>
    [TestMethod]
    public void HandlesNestedBlocks()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    if (x > 0)
                    {
                        var y = 2;
                        return;
                    }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    if (x > 0)
                    {
                        var y = 2;

                        return;
                    }
                }
            }
            """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Applies the <see cref="BlankLineRewriter"/> to the given source text.
    /// </summary>
    /// <param name="source">The source text to rewrite.</param>
    /// <returns>The rewritten source text.</returns>
    private static string ApplyRewriter(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var rewriter = new BlankLineRewriter(context, CancellationToken.None);
        var result = rewriter.Visit(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Methods
}