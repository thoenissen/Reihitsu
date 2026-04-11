using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="LayoutComputer"/> and <see cref="IndentationRewriter"/> with realistic C# code.
/// </summary>
[TestClass]
public class IndentationPhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the indentation phase (LayoutComputer + IndentationRewriter) on the given input.
    /// </summary>
    /// <param name="input">The C# source text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The formatted source text.</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(cancellationToken);
        var model = LayoutComputer.Compute(root, context);
        var result = IndentationRewriter.Apply(root, model);

        return result.ToFullString();
    }

    /// <summary>
    /// Verifies that class members at the wrong indentation level are corrected.
    /// </summary>
    [TestMethod]
    public void IndentsClassMembers()
    {
        // Arrange
        const string input = """
            class C
            {
            public int Value;
            }

            """;
        const string expected = """
            class C
            {
                public int Value;
            }

            """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that method body statements are indented correctly.
    /// </summary>
    [TestMethod]
    public void IndentsMethodBody()
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that correctly indented code is not modified.
    /// </summary>
    [TestMethod]
    public void PreservesCorrectIndentation()
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

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that namespace-enclosed class members are indented to the correct depth.
    /// </summary>
    [TestMethod]
    public void IndentsNamespaceMembers()
    {
        // Arrange
        const string input = """
            namespace N
            {
            class C
            {
            public int Value;
            }
            }

            """;
        const string expected = """
            namespace N
            {
                class C
                {
                    public int Value;
                }
            }

            """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that switch statement case labels and statements are indented correctly.
    /// </summary>
    [TestMethod]
    public void IndentsSwitchStatement()
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
        const string expected = """
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}