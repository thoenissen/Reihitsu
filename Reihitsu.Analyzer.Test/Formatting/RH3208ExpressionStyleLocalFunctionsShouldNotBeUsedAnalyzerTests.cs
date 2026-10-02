using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer, RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied returning local function is detected and fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturningLocalFunctionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate(int value)
                                    {
                                        return Double(value);

                                        int Double(int x) {|#0:=> x * 2|};
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate(int value)
                                      {
                                          return Double(value);

                                          int Double(int x)
                                          {
                                              return x * 2;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied void local function is fixed to an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVoidLocalFunctionIsFixedToExpressionStatement()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public void Run()
                                    {
                                        Log("start");

                                        void Log(string message) {|#0:=> System.Console.WriteLine(message)|};
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public void Run()
                                      {
                                          Log("start");

                                          void Log(string message)
                                          {
                                              System.Console.WriteLine(message);
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied async Task local function is fixed to an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncTaskLocalFunctionIsFixedToExpressionStatement()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH3208
                                {
                                    public Task RunAsync()
                                    {
                                        return WaitAsync();

                                        async Task WaitAsync() {|#0:=> await Task.Delay(1)|};
                                    }
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;

                                  internal class RH3208
                                  {
                                      public Task RunAsync()
                                      {
                                          return WaitAsync();

                                          async Task WaitAsync()
                                          {
                                              await Task.Delay(1);
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied async Task of T local function is fixed to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncGenericTaskLocalFunctionIsFixedToReturnStatement()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH3208
                                {
                                    public Task<int> RunAsync()
                                    {
                                        return LoadAsync();

                                        async Task<int> LoadAsync() {|#0:=> await Task.FromResult(1)|};
                                    }
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;

                                  internal class RH3208
                                  {
                                      public Task<int> RunAsync()
                                      {
                                          return LoadAsync();

                                          async Task<int> LoadAsync()
                                          {
                                              return await Task.FromResult(1);
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that a throw-expression-bodied local function is fixed to a throw statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyThrowExpressionBodiedLocalFunctionIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate()
                                    {
                                        return Fail();

                                        int Fail() {|#0:=> throw new System.InvalidOperationException()|};
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate()
                                      {
                                          return Fail();

                                          int Fail()
                                          {
                                              throw new System.InvalidOperationException();
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied local function inside a lambda block is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLocalFunctionInsideLambdaIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public System.Func<int, int> Create()
                                    {
                                        return value =>
                                               {
                                                   return Double(value);

                                                   int Double(int x) {|#0:=> x * 2|};
                                               };
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public System.Func<int, int> Create()
                                      {
                                          return value =>
                                                 {
                                                     return Double(value);

                                                     int Double(int x)
                                                     {
                                                         return x * 2;
                                                     }
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression-bodied local function in top-level statements is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLocalFunctionInTopLevelStatementsIsDetectedAndFixed()
    {
        const string testData = """
                                System.Console.WriteLine(Double(2));

                                int Double(int x) {|#0:=> x * 2|};
                                """;

        const string resultData = """
                                  System.Console.WriteLine(Double(2));

                                  int Double(int x)
                                  {
                                      return x * 2;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     static test =>
                            {
                                test.TestState.OutputKind = OutputKind.ConsoleApplication;
                                test.FixedState.OutputKind = OutputKind.ConsoleApplication;
                            },
                     Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that a nested expression-bodied local function is converted together with its expression-bodied owner
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedLocalFunctionsAreBothFixed()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate(int value)
                                    {
                                        return Outer(value);

                                        int Outer(int x) {|#0:=> Apply(y =>
                                                                {
                                                                    return Inner(y);

                                                                    int Inner(int z) {|#1:=> z + 1|};
                                                                }, x)|};

                                        int Apply(System.Func<int, int> function, int argument)
                                        {
                                            return function(argument);
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate(int value)
                                      {
                                          return Outer(value);

                                          int Outer(int x)
                                          {
                                              return Apply(y =>
                                                           {
                                                               return Inner(y);

                                                               int Inner(int z)
                                                               {
                                                                   return z + 1;
                                                               }
                                                           },
                                                           x);
                                          }

                                          int Apply(System.Func<int, int> function, int argument)
                                          {
                                              return function(argument);
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     static test =>
                            {
                                // Fixing the outer function formats its whole body, which converts the inner function too
                                test.NumberOfIncrementalIterations = 1;
                                test.NumberOfFixAllIterations = -2;
                            },
                     Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a block-bodied local function is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockBodiedLocalFunctionIsNotReported()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate(int value)
                                    {
                                        return Double(value);

                                        int Double(int x)
                                        {
                                            return x * 2;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an extern local function without a body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExternLocalFunctionWithoutBodyIsNotReported()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate()
                                    {
                                        return GetValue();

                                        [System.Runtime.InteropServices.DllImport("native")]
                                        static extern int GetValue();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a local function whose expression body carries a directive before the expression is not reported,
    /// because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate()
                                    {
                                        return GetValue();

                                        int GetValue() =>
                                #pragma warning disable CS0618
                                            1;
                                #pragma warning restore CS0618
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a local function whose expression body carries a conditional group before the expression is not reported,
    /// because the formatter refuses to move the group behind the generated return keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithConditionalGroupBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate()
                                    {
                                        return GetValue();

                                        int GetValue() =>
                                #if DEBUG
                                            1;
                                #else
                                            2;
                                #endif
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a local function whose expression body carries a directive after the expression is still reported
    /// and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate()
                                    {
                                        return GetValue();

                                        int GetValue() {|#0:=> 1|}
                                #pragma warning disable CS0618
                                            ;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate()
                                      {
                                          return GetValue();

                                          int GetValue()
                                          {
                                              return 1
                                  #pragma warning disable CS0618
                                              ;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    /// <summary>
    /// Verifying that a local function whose expression starts on the line after the arrow is fixed to a single-spaced return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate(int value)
                                    {
                                        return Double(value);

                                        int Double(int x) {|#0:=>
                                            x * 2|};
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate(int value)
                                      {
                                          return Double(value);

                                          int Double(int x)
                                          {
                                              return x * 2;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3208
                                {
                                    public int Calculate(int value)
                                    {
                                        return Double(value) + Triple(value);

                                        int Double(int x) {|#0:=> x * 2|};

                                        int Triple(int x) {|#1:=> x * 3|};
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3208
                                  {
                                      public int Calculate(int value)
                                      {
                                          return Double(value) + Triple(value);

                                          int Double(int x)
                                          {
                                              return x * 2;
                                          }

                                          int Triple(int x)
                                          {
                                              return x * 3;
                                          }
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied local functions are fixed in one Fix All iteration
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3208MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}