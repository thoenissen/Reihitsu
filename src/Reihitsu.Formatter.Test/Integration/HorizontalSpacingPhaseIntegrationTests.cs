using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="HorizontalSpacingPhase"/> with realistic C# code
/// </summary>
[TestClass]
public class HorizontalSpacingPhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that spaces are inserted around binary operators
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-010")]
    public void InsertsSpacesAroundBinaryOperators()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1+2;
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1 + 2;
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that exactly one space is inserted after commas
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-008")]
    public void InsertsSpaceAfterComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a,int b)
                                 {
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b)
                                    {
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that spaces inside parentheses are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-004")]
    public void RemovesSpacesInsideParentheses()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M( int a, int b )
                                 {
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b)
                                    {
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a space is inserted after keywords such as <c>if</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void InsertsSpaceAfterKeyword()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if(true)
                                     {
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
    /// Verifies that multiple consecutive spaces between tokens are collapsed to a single space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void CollapsesMultipleSpaces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void  M()
                                 {
                                     var  x  =  1;
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
    /// Executes the <see cref="HorizontalSpacingPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var result = HorizontalSpacingPhase.Execute(tree.GetRoot(cancellationToken), cancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}