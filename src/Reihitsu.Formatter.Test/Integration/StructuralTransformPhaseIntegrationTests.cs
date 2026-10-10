using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
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
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

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

    /// <summary>
    /// Verifies that the trailing comma before an "#if" conditional block carrying a further element is preserved
    /// in an array initializer, using the issue's exact reported input (SYMBOL undefined, top-level statement)
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaBeforeConditionalBlockInArrayInitializer()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if SYMBOL
                                 2,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma before an "#if false" disabled block is preserved, since the guard is
    /// textual and never evaluates the condition
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaBeforeDisabledConditionalBlockInArrayInitializer()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if false
                                 2,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that a nested array initializer preserves its own trailing comma when a conditional block follows
    /// it, while the outer initializer's clean trailing comma is still removed
    /// </summary>
    [TestMethod]
    public void PreservesInnerTrailingCommaInNestedArrayInitializerWhileRemovingOuter()
    {
        // Arrange
        const string input = """
                             var array = new int[][]
                             {
                                 new[]
                                 {
                                     1,
                             #if SYMBOL
                                     2,
                             #endif
                                 },
                                 new[] { 3 },
                             };

                             """;
        const string expected = """
                                var array = new int[][]
                                {
                                    new[]
                                    {
                                        1,
                                #if SYMBOL
                                        2,
                                #endif
                                    },
                                    new[] { 3 }
                                };

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma before an "#if" conditional block carrying a further element is preserved
    /// in a collection initializer
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaBeforeConditionalBlockInCollectionInitializer()
    {
        // Arrange
        const string input = """
                             var values = new List<int>
                             {
                                 1,
                             #if SYMBOL
                                 2,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma before an "#if" conditional block carrying a further member is preserved
    /// on the final enum member
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaBeforeConditionalBlockInFinalEnumMember()
    {
        // Arrange
        const string input = """
                             internal enum Status
                             {
                                 Ready,
                             #if SYMBOL
                                 Completed,
                             #endif
                             }

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is still removed when the conditional block sits before the final active
    /// element rather than after it, since the gap to the closing delimiter is clean
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaWhenConditionalBlockPrecedesFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                             #if SYMBOL
                                 1,
                             #endif
                                 2,
                             };

                             """;
        const string expected = """
                                var array = new[]
                                {
                                #if SYMBOL
                                    1,
                                #endif
                                    2
                                };

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is still removed when a "#region"/"#endregion" pair follows the final
    /// element, since a region directive can never introduce a further list element
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaWhenRegionDirectiveFollowsFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                                 2,
                             #region Trailer
                             #endregion
                             };

                             """;
        const string expected = """
                                var array = new[]
                                {
                                    1,
                                    2
                                #region Trailer
                                #endregion
                                };

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is still removed when a "#pragma" directive follows the final element,
    /// since it can never introduce a further list element
    /// </summary>
    [TestMethod]
    public void RemovesTrailingCommaWhenPragmaDirectiveFollowsFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                                 2,
                             #pragma warning restore CS0169
                             };

                             """;
        const string expected = """
                                var array = new[]
                                {
                                    1,
                                    2
                                #pragma warning restore CS0169
                                };

                                """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is preserved when the gap holds only the closing "#endif" of an
    /// "#if"/"#else"/"#endif" chain, even though the active "#else" branch's own comma is provably final in
    /// every symbol configuration. The guard is textual and withholds on the directive's presence alone
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaWhenElseBranchFollowsFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if SYMBOL
                                 2,
                             #else
                                 3,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is preserved when an active "#if true" branch follows the final element,
    /// even though the branch is always taken and the comma is therefore provably final
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaWhenActiveIfTrueBranchFollowsFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if true
                                 2,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma is preserved when an element-free conditional region follows the final
    /// element, since the guard does not check whether an element could actually appear inside it
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaWhenElementFreeConditionalRegionFollowsFinalElement()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if SYMBOL
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the trailing comma on the final enum member is preserved when an active "#if true" branch
    /// follows it
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaInEnumWhenActiveIfTrueBranchFollowsFinalMember()
    {
        // Arrange
        const string input = """
                             internal enum Status
                             {
                                 Ready,
                             #if true
                                 Completed,
                             #endif
                             }

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

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

    #endregion // Methods
}