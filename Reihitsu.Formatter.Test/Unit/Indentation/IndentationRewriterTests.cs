using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests for <see cref="IndentationRewriter"/>.
/// </summary>
[TestClass]
public class IndentationRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that class members are indented to the correct column.
    /// </summary>
    [TestMethod]
    public void ApplyIndentsClassMembers()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             public int X;
                             public int Y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int X;
                                    public int Y;
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that statements inside a method body are indented correctly.
    /// </summary>
    [TestMethod]
    public void ApplyIndentsMethodBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             void M()
                             {
                             var x = 1;
                             var y = 2;
                             }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;
                                        var y = 2;
                                    }
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that nested blocks (e.g. if inside method) are indented at increasing depths.
    /// </summary>
    [TestMethod]
    public void ApplyIndentsNestedBlocks()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             void M()
                             {
                             if (true)
                             {
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
                                        if (true)
                                        {
                                            var x = 1;
                                        }
                                    }
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that empty lines between statements are preserved during indentation.
    /// </summary>
    [TestMethod]
    public void ApplyPreservesEmptyLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             void M()
                             {
                             var x = 1;

                             var y = 2;
                             }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        var y = 2;
                                    }
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that applying an empty layout model does not alter the source.
    /// </summary>
    [TestMethod]
    public void ApplyHandlesEmptyModel()
    {
        // Arrange
        const string input = """
                             int x = 1;
                             """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that multiple nesting levels (namespace, class, method) are indented correctly.
    /// </summary>
    [TestMethod]
    public void ApplyIndentsMultipleLevels()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                             class C
                             {
                             void M()
                             {
                             var x = 1;
                             }
                             }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        void M()
                                        {
                                            var x = 1;
                                        }
                                    }
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the content of string literals is not altered by indentation.
    /// </summary>
    [TestMethod]
    public void ApplyPreservesStringLiterals()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             void M()
                             {
                             var s = "  hello  ";
                             }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var s = "  hello  ";
                                    }
                                }
                                """;

        // Act
        var actual = RunIndent(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Computes the layout model and applies indentation to the given input.
    /// </summary>
    /// <param name="input">The source text to indent.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The indented source text.</returns>
    private static string RunIndent(string input, CancellationToken token)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(token);
        var model = LayoutComputer.Compute(root, context);
        var result = IndentationRewriter.Apply(root, model);

        return result.ToFullString();
    }

    #endregion // Methods
}