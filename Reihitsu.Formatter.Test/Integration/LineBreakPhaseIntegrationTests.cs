using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="LineBreakPhase"/> with realistic C# code
/// </summary>
[TestClass]
public class LineBreakPhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = LineBreakPhase.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Verifies that K&amp;R-style braces are converted to Allman style
    /// </summary>
    [TestMethod]
    public void ConvertsKAndRBracesToAllmanStyle()
    {
        // Arrange
        const string input = """
                             class C {
                                 void M() {
                                 }
                             }

                             """;
        const string expected = """
                                class C 
                                {
                                    void M() 
                                {
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that code already using Allman-style braces is not modified
    /// </summary>
    [TestMethod]
    public void PreservesAllmanStyleBraces()
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
    /// Verifies that binary operators at the end of a line are moved to the beginning of the next line
    /// </summary>
    [TestMethod]
    public void MovesBinaryOperatorToNextLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1 +
                                         2;
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1 
                                +2;
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a constructor initializer on the same line is placed on a new line
    /// </summary>
    [TestMethod]
    public void PlacesConstructorInitializerOnNewLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C(int x) : base()
                                 {
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    C(int x) 
                                : base()
                                    {
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied property split across lines is handled by the line break phase
    /// </summary>
    [TestMethod]
    public void HandlesExpressionBodiedPropertyAcrossLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Value
                                     => 42;
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    public int Value => 42;
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}