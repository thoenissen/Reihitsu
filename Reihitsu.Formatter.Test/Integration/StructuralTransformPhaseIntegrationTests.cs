using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.StructuralTransforms;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests for <see cref="StructuralTransformPhase"/> with realistic C# code
/// </summary>
[TestClass]
public class StructuralTransformPhaseIntegrationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="StructuralTransformPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The transformed source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new StructuralTransformPhase().Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Verifies that an expression-bodied non-void method is converted to a block body with return
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => 42;
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    int M(){return42;}
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied void method is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedVoidMethodToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() => Console.WriteLine();
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M(){Console.WriteLine();}
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied constructor is converted to a block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedConstructorToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _x;
                                 C(int x) => _x = x;
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    int _x;
                                    C(int x){_x = x;}
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a method already using block body is not modified
    /// </summary>
    [TestMethod]
    public void PreservesBlockBodyMethod()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 42;
                                 }
                             }

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied local function is converted to a block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedLocalFunctionToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Add(int a, int b) => a + b;
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int Add(int a, int b) {returna + b;}
                                    }
                                }

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a trailing comma is removed from the final enum member
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalEnumMember()
    {
        // Arrange
        const string input = """
                             internal enum Status
                             {
                                 Ready,
                                 Completed,
                             }
                             """;
        const string expected = """
                                internal enum Status
                                {
                                    Ready,
                                    Completed
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a comment attached to the final enum member is preserved when removing the trailing comma
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalEnumMemberWhilePreservingComment()
    {
        // Arrange
        const string input = """
                             internal enum Status
                             {
                                 Completed, // Final value
                             }
                             """;
        const string expected = """
                                internal enum Status
                                {
                                    Completed // Final value
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a trailing comma is removed from the final array initializer item
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalArrayInitializerItem()
    {
        // Arrange
        const string input = """
                             internal class Example
                             {
                                 private static readonly int[] Values =
                                 {
                                     1,
                                     2,
                                 };
                             }
                             """;
        const string expected = """
                                internal class Example
                                {
                                    private static readonly int[] Values =
                                    {
                                        1,
                                        2
                                    };
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a comment attached to the final array initializer item is preserved when removing the trailing comma
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalArrayInitializerItemWhilePreservingComment()
    {
        // Arrange
        const string input = """
                             internal class Example
                             {
                                 private static readonly int[] Values =
                                 {
                                     2, // Final value
                                 };
                             }
                             """;
        const string expected = """
                                internal class Example
                                {
                                    private static readonly int[] Values =
                                    {
                                        2 // Final value
                                    };
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a trailing comma is removed from the final collection initializer item
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalCollectionInitializerItem()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static readonly List<int> Values = new()
                                                                       {
                                                                           1,
                                                                           2,
                                                                       };
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                              1,
                                                                              2
                                                                          };
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a comment attached to the final collection initializer item is preserved when removing the trailing comma
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaFromFinalCollectionInitializerItemWhilePreservingComment()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static readonly List<int> Values = new()
                                                                       {
                                                                           2, // Final value
                                                                       };
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                              2 // Final value
                                                                          };
                                }
                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}