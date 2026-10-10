using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="LayoutComputer"/> and <see cref="IndentationRewriter"/> with realistic C# code
/// </summary>
[TestClass]
public class IndentationPhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that class members at the wrong indentation level are corrected
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
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that method body statements are indented correctly
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
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that correctly indented code is not modified
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
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that namespace-enclosed class members are indented to the correct depth
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
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that switch statement case labels and statements are indented correctly
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
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="IndentationPhase.Execute"/> throws <see cref="OperationCanceledException"/>
    /// when the cancellation token is already cancelled, instead of running the layout computation and
    /// rewrite to completion
    /// </summary>
    [TestMethod]
    public void ExecuteThrowsWhenCancellationIsRequested()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             public int Value;
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var phase = new IndentationPhase();

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsExactly<OperationCanceledException>(() => phase.Execute(root, context, cts.Token));
        }
    }

    /// <summary>
    /// Executes the indentation phase (LayoutComputer + IndentationRewriter) on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(cancellationToken);
        var model = LayoutComputer.Compute(root, context, cancellationToken);
        var result = IndentationRewriter.Apply(root, model, cancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}