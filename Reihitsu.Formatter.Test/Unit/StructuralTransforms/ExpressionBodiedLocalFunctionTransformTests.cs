using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.StructuralTransforms;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests for <see cref="StructuralTransformPhase"/> local-function expression-bodied transforms
/// </summary>
[TestClass]
public class ExpressionBodiedLocalFunctionTransformTests : FormatterPhaseTestsBase
{
    #region Tests

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
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied void local function is converted to block body with an expression statement
    /// </summary>
    [TestMethod]
    public void ConvertsVoidExpressionBodiedLocalFunctionToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     void Local() => Console.WriteLine();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        void Local() {Console.WriteLine();}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async Task expression-bodied local function is converted without a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncTaskExpressionBodiedLocalFunctionToExpressionStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     async Task AwaitAndReturnNothing() => await Task.CompletedTask;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        async Task AwaitAndReturnNothing() {await Task.CompletedTask;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async Task{T} expression-bodied local function is converted with a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncGenericTaskExpressionBodiedLocalFunctionToReturnStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     async Task<int> AwaitAndReturnValue() => await Task.FromResult(1);
                                 }
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        async Task<int> AwaitAndReturnValue() {returnawait Task.FromResult(1);}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an async void expression-bodied local function is converted without a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsAsyncVoidExpressionBodiedLocalFunctionToExpressionStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     async void Notify() => await Task.CompletedTask;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        async void Notify() {await Task.CompletedTask;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a non-async Task expression-bodied local function is converted with a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsNonAsyncTaskExpressionBodiedLocalFunctionToReturnStatement()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     Task PassThrough() => Task.CompletedTask;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Task PassThrough() {returnTask.CompletedTask;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied generic local function is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsGenericExpressionBodiedLocalFunctionToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     T Local<T>() => default;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        T Local<T>() {returndefault;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a static expression-bodied local function is converted to block body
    /// </summary>
    [TestMethod]
    public void ConvertsStaticExpressionBodiedLocalFunctionToBlockBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     static int Local() => 1;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        static int Local() {return1;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a static async Task expression-bodied local function is converted without a return statement
    /// </summary>
    [TestMethod]
    public void ConvertsStaticAsyncTaskExpressionBodiedLocalFunctionToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     static async Task LocalAsync() => await Task.CompletedTask;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        static async Task LocalAsync() {await Task.CompletedTask;}
                                    }
                                }
                                """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a local function already using block body is not modified
    /// </summary>
    [TestMethod]
    public void PreservesBlockBodiedLocalFunction()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local()
                                     {
                                         return 42;
                                     }
                                 }
                             }
                             """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.AreEqual(input, actual);
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