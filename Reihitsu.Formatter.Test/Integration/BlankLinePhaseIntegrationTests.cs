using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="BlankLinePhase"/> with realistic C# code
/// </summary>
[TestClass]
public class BlankLinePhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="BlankLinePhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = BlankLinePhase.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a return statement preceded by another statement
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that blank lines after an opening brace are removed
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before an if statement preceded by another statement
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that no blank line is inserted before the first statement in a block
    /// </summary>
    [TestMethod]
    public void DoesNotInsertBlankLineForFirstStatement()
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
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that excessive consecutive blank lines are collapsed to a single blank line
    /// </summary>
    [TestMethod]
    public void CollapsesExcessiveBlankLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;



                                     return x;
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        return x;
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