using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for <c>async</c> methods and local functions whose return type the formatter
/// cannot recognize as a task type: they keep their expression body, while a task type written by
/// name, qualified or nullable-annotated, is converted
/// </summary>
[TestClass]
public class AsyncUnknownTaskTypeExpressionBodyTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported shape: an <c>async</c> method returning an alias of <c>Task</c> keeps its
    /// expression body instead of becoming a <c>return</c> statement that does not compile
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-PIPE-003")]
    public void AsyncMethodReturningTaskAliasKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 async T RunAsync() => await T.Delay(1);
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specified example: an alias of <c>Task</c>, an alias of <c>ValueTask&lt;int&gt;</c>,
    /// and a custom task-like type all keep their expression bodies
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    public void AsyncMethodsReturningAliasesAndCustomTaskTypeKeepExpressionBodies()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;
                             using V = System.Threading.Tasks.ValueTask<int>;

                             class C
                             {
                                 async T RunAsync() => await T.Delay(1);

                                 async V GetAsync() => await Compute();

                                 async MyTask StartAsync() => await Prepare();
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>async</c> method returning a generic custom task-like type keeps its expression
    /// body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    public void AsyncMethodReturningGenericCustomTaskTypeKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async MyTask<int> GetAsync() => await Compute();
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>async</c> local function returning an alias of <c>Task</c> keeps its expression
    /// body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-PIPE-003")]
    public void AsyncLocalFunctionReturningTaskAliasKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 void M()
                                 {
                                     async T RunAsync() => await T.Delay(1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>static async</c> local function returning an alias of <c>Task</c> keeps its
    /// expression body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    public void StaticAsyncLocalFunctionReturningTaskAliasKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 void M()
                                 {
                                     static async T RunAsync() => await T.Delay(1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a generic <c>async</c> method returning an alias of <c>Task</c> keeps its expression
    /// body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    public void GenericAsyncMethodReturningTaskAliasKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 async T RunAsync<TArg>(TArg argument) => await T.Delay(1);
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>async</c> method returning an alias keeps its expression body also when the
    /// expression is a throw expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    public void AsyncMethodReturningTaskAliasWithThrowExpressionKeepsExpressionBody()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 async T RunAsync() => throw new System.InvalidOperationException();
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specified example: a task type written by its qualified name is converted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-STRUCT-004")]
    public void AsyncMethodsReturningQualifiedTaskTypesAreConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async System.Threading.Tasks.Task RunAsync() => await Prepare();

                                 async System.Threading.Tasks.Task<int> GetAsync() => await Compute();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async System.Threading.Tasks.Task RunAsync()
                                    {
                                        await Prepare();
                                    }

                                    async System.Threading.Tasks.Task<int> GetAsync()
                                    {
                                        return await Compute();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a task type written behind the <c>global::</c> alias is converted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-STRUCT-004")]
    public void AsyncMethodReturningGlobalQualifiedTaskIsConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async global::System.Threading.Tasks.Task RunAsync() => await Prepare();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async global::System.Threading.Tasks.Task RunAsync()
                                    {
                                        await Prepare();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specified example: a nullable annotation does not hide the task type, so
    /// <c>Task?</c> becomes an expression statement and <c>Task&lt;int&gt;?</c> a <c>return</c> statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-STRUCT-004")]
    [Requirement("FMT-PIPE-003")]
    public void AsyncMethodsReturningNullableTaskTypesAreConverted()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task? RunAsync() => await Task.Delay(1);

                                 async Task<int>? GetAsync() => await Task.FromResult(1);
                             }
                             """;
        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task? RunAsync()
                                    {
                                        await Task.Delay(1);
                                    }

                                    async Task<int>? GetAsync()
                                    {
                                        return await Task.FromResult(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>async</c> local function returning a nullable-annotated <c>Task</c> is converted
    /// to an expression statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-025")]
    [Requirement("FMT-STRUCT-004")]
    public void AsyncLocalFunctionReturningNullableTaskIsConverted()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     async Task? RunAsync() => await Task.Delay(1);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        async Task? RunAsync()
                                        {
                                            await Task.Delay(1);
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method returning an alias without <c>async</c> is still converted to a
    /// <c>return</c> statement, because only <c>async</c> functions depend on the task type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-004")]
    public void NonAsyncMethodReturningTaskAliasIsConvertedToReturn()
    {
        // Arrange
        const string input = """
                             using T = System.Threading.Tasks.Task;

                             class C
                             {
                                 T RunAsync() => T.Delay(1);
                             }
                             """;
        const string expected = """
                                using T = System.Threading.Tasks.Task;

                                class C
                                {
                                    T RunAsync()
                                    {
                                        return T.Delay(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>async void</c> method is converted to an expression statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-004")]
    public void AsyncVoidMethodIsConvertedToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async void Run() => await Prepare();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async void Run()
                                    {
                                        await Prepare();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}