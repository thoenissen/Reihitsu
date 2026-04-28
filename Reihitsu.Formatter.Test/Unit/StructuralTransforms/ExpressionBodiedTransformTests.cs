using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.StructuralTransforms;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests for <see cref="StructuralTransformPhase"/> expression-bodied transforms
/// </summary>
[TestClass]
public class ExpressionBodiedTransformTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied method returning a value is converted to block body with a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo() => 42;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int Foo(){return42;}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied void method is converted to block body with an expression statement
    /// </summary>
    [TestMethod]
    public void ConvertsVoidExpressionBodiedMethodToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void Foo() => Console.WriteLine();
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void Foo(){Console.WriteLine();}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a method already using block body is not modified
    /// </summary>
    [TestMethod]
    public void PreservesBlockBodiedMethod()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo()
                                 {
                                     return 42;
                                 }
                             }
                             """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied constructor is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedConstructorToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C() => _x = 1;
                                 int _x;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    C(){_x = 1;}
                                    int _x;
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied local function is converted to block body
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
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied operator is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedOperatorToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C a, C b) => new C();
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    public static C operator +(C a, C b) {returnnew C();}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied indexer is converted to block body with a get accessor
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedIndexerToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] _data;
                                 int this[int i] => _data[i];
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int[] _data;
                                    int this[int i] {get{return_data[i];}}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied finalizer is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedFinalizerToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ~C() => Cleanup();
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    ~C() {Cleanup();}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied conversion operator is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedConversionToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static implicit operator int(C c) => 0;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    public static implicit operator int(C c) {return0;}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied property is not converted (properties are excluded)
    /// </summary>
    [TestMethod]
    public void DoesNotConvertExpressionBodiedProperty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo => 42;
                             }
                             """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that multiple expression-bodied methods are all converted in a single pass
    /// </summary>
    [TestMethod]
    public void HandlesMultipleExpressionBodiedMembers()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo() => 1;
                                 int Bar() => 2;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int Foo(){return1;}
                                    int Bar(){return2;}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied method throwing an exception is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodThrowingException()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo() => throw new System.Exception();
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int Foo(){returnthrow new System.Exception();}
                                }
                                """;

        // Act
        var actual = RunTransform(input, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Runs the structural transform phase on the given input
    /// </summary>
    /// <param name="input">The source text to transform</param>
    /// <param name="token">The cancellation token</param>
    /// <returns>The transformed source text</returns>
    private static string RunTransform(string input, CancellationToken token)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: token);
        var context = new FormattingContext(Environment.NewLine);
        var result = StructuralTransformPhase.Execute(tree.GetRoot(token), context, token);

        return result.ToFullString();
    }

    #endregion // Methods
}