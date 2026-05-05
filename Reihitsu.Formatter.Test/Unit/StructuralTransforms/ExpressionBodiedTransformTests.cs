using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.StructuralTransforms;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests for <see cref="StructuralTransformPhase"/> expression-bodied member transforms
/// </summary>
[TestClass]
public class ExpressionBodiedTransformTests : FormatterPhaseTestsBase
{
    #region Tests

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async Task expression-bodied method is converted without a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncTaskExpressionBodiedMethodToExpressionStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task AwaitAndReturnNothing() => await Task.CompletedTask;
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task AwaitAndReturnNothing(){await Task.CompletedTask;}
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async Task{T} expression-bodied method is converted with a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncGenericTaskExpressionBodiedMethodToReturnStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task<int> AwaitAndReturnValue() => await Task.FromResult(1);
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task<int> AwaitAndReturnValue(){returnawait Task.FromResult(1);}
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async void expression-bodied method is converted without a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncVoidExpressionBodiedMethodToExpressionStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async void Notify() => await Task.CompletedTask;
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async void Notify(){await Task.CompletedTask;}
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a non-async Task expression-bodied method is converted with a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsNonAsyncTaskExpressionBodiedMethodToReturnStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 Task PassThrough() => Task.CompletedTask;
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    Task PassThrough(){returnTask.CompletedTask;}
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

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
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Tests

    #region FormatterPhaseTestsBase

    /// <inheritdoc/>
    protected override SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken)
    {
        var context = new FormattingContext(Environment.NewLine);

        return StructuralTransformPhase.Execute(root, context, cancellationToken);
    }

    #endregion // FormatterPhaseTestsBase
}