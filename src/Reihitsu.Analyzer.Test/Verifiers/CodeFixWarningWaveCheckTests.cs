using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Test methods for the check of <see cref="CSharpCodeFixVerifierTest{TAnalyzer, TCodeFix}"/> that fails a code-fix
/// test whose fixed state holds a warning-wave diagnostic its source state does not hold. The tests construct the
/// verifier directly, as <c>AnalyzerTestsBase{TAnalyzer, TCodeFix}.Verify</c> and the test classes that bypass the base
/// classes do, and use <see cref="ParenthesisRemovalProbeCodeFixProvider"/>, which removes parentheses without checking
/// whether they were needed
/// </summary>
[TestClass]
public class CodeFixWarningWaveCheckTests
{
    #region Constants

    /// <summary>
    /// A type whose <c>Select</c> method lets a query expression stand as the operand of <c>throw</c>
    /// </summary>
    private const string QueryableException = """
                                              internal class QueryableException : Exception
                                              {
                                                  public QueryableException Select(Func<QueryableException, QueryableException> selector) => this;
                                              }
                                              """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Creates the verifier for the probe analyzer and code fix
    /// </summary>
    /// <param name="source">Source text with markup</param>
    /// <param name="fixedSource">Expected fixed source text with markup</param>
    /// <returns>The verifier</returns>
    private static CSharpCodeFixVerifierTest<ParenthesisRemovalProbeAnalyzer, ParenthesisRemovalProbeCodeFixProvider> CreateTest(string source, string fixedSource)
    {
        return new CSharpCodeFixVerifierTest<ParenthesisRemovalProbeAnalyzer, ParenthesisRemovalProbeCodeFixProvider>
               {
                   TestCode = source,
                   FixedCode = fixedSource,
                   ReferenceAssemblies = ReferenceAssemblies.Net.Net90
               };
    }

    /// <summary>
    /// Compiles every state of the test at the highest warning level and compares compiler warnings as well as errors,
    /// so a test can declare the warnings a state holds and prove that they are there
    /// </summary>
    /// <param name="test">Test</param>
    private static void CompareWarningsAtHighestWarningLevel(CSharpCodeFixTest<ParenthesisRemovalProbeAnalyzer, ParenthesisRemovalProbeCodeFixProvider, DefaultVerifier> test)
    {
        test.CompilerDiagnostics = CompilerDiagnostics.Warnings;
        test.SolutionTransforms.Add((solution, projectId) =>
                                    {
                                        var options = solution.GetProject(projectId)?.CompilationOptions;

                                        return options == null
                                                   ? solution
                                                   : solution.WithProjectCompilationOptions(projectId, ((CSharpCompilationOptions)options).WithWarningLevel(9999));
                                    });
    }

    /// <summary>
    /// Runs the verification of the test
    /// </summary>
    /// <param name="test">Test</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyAsync(CSharpCodeFixVerifierTest<ParenthesisRemovalProbeAnalyzer, ParenthesisRemovalProbeCodeFixProvider> test)
    {
        await test.RunAsync(CancellationToken.None);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a code-fix test fails when the fix writes a query expression as the operand of <c>throw</c> without
    /// its parentheses, so the fixed state holds CS8848 (a wave 5 warning) and the source state does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixedStateThatIntroducesAWarningWaveDiagnosticFailsVerification()
    {
        const string source = $$"""
                                using System;

                                {{QueryableException}}

                                internal class Test
                                {
                                    private readonly QueryableException _exception = new QueryableException();

                                    public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                }
                                """;
        const string fixedSource = $$"""
                                     using System;

                                     {{QueryableException}}

                                     internal class Test
                                     {
                                         private readonly QueryableException _exception = new QueryableException();

                                         public int X => throw from exception in _exception select exception;
                                     }
                                     """;

        var test = CreateTest(source, fixedSource);

        var exception = await Assert.ThrowsAsync<Exception>(() => test.RunAsync(CancellationToken.None));

        Assert.Contains("CS8848", exception.Message);
    }

    /// <summary>
    /// Verifies that a code-fix test fails when its source state already holds CS8848 once and the fix writes it a second
    /// time: the check counts the diagnostics of each ID, so a source state that holds the ID does not let the fix add
    /// more of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixedStateThatHoldsAWarningWaveDiagnosticMoreOftenThanTheSourceStateFailsVerification()
    {
        const string source = $$"""
                                using System;

                                {{QueryableException}}

                                internal class Test
                                {
                                    private readonly QueryableException _exception = new QueryableException();

                                    public int Y => throw from exception in _exception select exception;

                                    public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                }
                                """;
        const string fixedSource = $$"""
                                     using System;

                                     {{QueryableException}}

                                     internal class Test
                                     {
                                         private readonly QueryableException _exception = new QueryableException();

                                         public int Y => throw from exception in _exception select exception;

                                         public int X => throw from exception in _exception select exception;
                                     }
                                     """;

        var test = CreateTest(source, fixedSource);

        var exception = await Assert.ThrowsAsync<Exception>(() => test.RunAsync(CancellationToken.None));

        Assert.Contains("CS8848", exception.Message);
    }

    /// <summary>
    /// Verifies that a code-fix test fails when only its Fix All fixed state holds a warning-wave diagnostic the source
    /// state does not hold: the one-step fix removes the harmless parentheses first, and Fix All also removes the
    /// parentheses a query expression needs as the operand of <c>throw</c>, which writes CS8848
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixAllFixedStateThatIntroducesAWarningWaveDiagnosticFailsVerification()
    {
        const string source = $$"""
                                using System;

                                {{QueryableException}}

                                internal class Test
                                {
                                    private readonly QueryableException _exception = new QueryableException();

                                    public int Y => {|RH3002:(1)|};

                                    public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                }
                                """;
        const string fixedSource = $$"""
                                     using System;

                                     {{QueryableException}}

                                     internal class Test
                                     {
                                         private readonly QueryableException _exception = new QueryableException();

                                         public int Y => 1;

                                         public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                     }
                                     """;
        const string batchFixedSource = $$"""
                                          using System;

                                          {{QueryableException}}

                                          internal class Test
                                          {
                                              private readonly QueryableException _exception = new QueryableException();

                                              public int Y => 1;

                                              public int X => throw from exception in _exception select exception;
                                          }
                                          """;

        var test = CreateTest(source, fixedSource);

        test.BatchFixedCode = batchFixedSource;
        test.CodeFixTestBehaviors = CodeFixTestBehaviors.FixOne;
        test.FixedState.MarkupHandling = MarkupMode.Allow;

        var exception = await Assert.ThrowsAsync<Exception>(() => test.RunAsync(CancellationToken.None));

        Assert.Contains("CS8848", exception.Message);
        Assert.Contains("Fix All", exception.Message);
    }

    /// <summary>
    /// Verifies that the one-step fixed state of <see cref="FixAllFixedStateThatIntroducesAWarningWaveDiagnosticFailsVerification"/>
    /// passes the check on its own, so that test fails on its Fix All fixed state and nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OneStepFixedStateThatHoldsNoNewWarningWaveDiagnosticPassesVerification()
    {
        const string source = $$"""
                                using System;

                                {{QueryableException}}

                                internal class Test
                                {
                                    private readonly QueryableException _exception = new QueryableException();

                                    public int Y => {|RH3002:(1)|};

                                    public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                }
                                """;
        const string fixedSource = $$"""
                                     using System;

                                     {{QueryableException}}

                                     internal class Test
                                     {
                                         private readonly QueryableException _exception = new QueryableException();

                                         public int Y => 1;

                                         public int X => throw {|RH3002:(from exception in _exception select exception)|};
                                     }
                                     """;

        var test = CreateTest(source, fixedSource);

        test.CodeFixTestBehaviors = CodeFixTestBehaviors.FixOne | CodeFixTestBehaviors.SkipFixAllCheck;
        test.FixedState.MarkupHandling = MarkupMode.Allow;

        await VerifyAsync(test);
    }

    /// <summary>
    /// Verifies that a code-fix test passes when its source state already holds the warning-wave diagnostic CS8848 and
    /// the fixed state holds it as often as the source state does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task WarningWaveDiagnosticTheSourceStateAlreadyHoldsPassesVerification()
    {
        const string source = $$"""
                                using System;

                                {{QueryableException}}

                                internal class Test
                                {
                                    private readonly QueryableException _exception = new QueryableException();

                                    public int Y => {|RH3002:(1)|};

                                    public int X => throw {|#0:from exception in _exception|} select exception;
                                }
                                """;
        const string fixedSource = $$"""
                                     using System;

                                     {{QueryableException}}

                                     internal class Test
                                     {
                                         private readonly QueryableException _exception = new QueryableException();

                                         public int Y => 1;

                                         public int X => throw {|#0:from exception in _exception|} select exception;
                                     }
                                     """;

        var test = CreateTest(source, fixedSource);

        CompareWarningsAtHighestWarningLevel(test);

        test.TestState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("CS8848").WithLocation(0).WithArguments("from"));
        test.FixedState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("CS8848").WithLocation(0).WithArguments("from"));

        await VerifyAsync(test);
    }

    /// <summary>
    /// Verifies that a code-fix test passes when its source state holds the warning-wave diagnostic CS8981 (a type name
    /// of lower-case ASCII letters) and the fix leaves it in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task LowerCaseTypeNameTheFixLeavesInPlacePassesVerification()
    {
        const string source = """
                              internal class {|#0:lowercase|}
                              {
                                  public int Y => {|RH3002:(1)|};
                              }
                              """;
        const string fixedSource = """
                                   internal class {|#0:lowercase|}
                                   {
                                       public int Y => 1;
                                   }
                                   """;

        var test = CreateTest(source, fixedSource);

        CompareWarningsAtHighestWarningLevel(test);

        test.TestState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("CS8981").WithLocation(0));
        test.FixedState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("CS8981").WithLocation(0));

        await VerifyAsync(test);
    }

    /// <summary>
    /// Verifies that a code-fix test passes when its fixed state holds an ordinary warning of warning level 1 to 4 that
    /// the source state does not hold: removing the parentheses around an <c>is</c> expression makes it test the result
    /// of the string concatenation, which is never an <see cref="int"/> (CS0184)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrdinaryWarningTheFixIntroducesPassesVerification()
    {
        const string source = """
                              internal class Test
                              {
                                  public object Get(object value) => "a" + {|RH3002:(value is int)|};
                              }
                              """;
        const string fixedSource = """
                                   internal class Test
                                   {
                                       public object Get(object value) => {|#0:"a" + value is int|};
                                   }
                                   """;

        var test = CreateTest(source, fixedSource);

        CompareWarningsAtHighestWarningLevel(test);

        test.FixedState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("CS0184").WithLocation(0).WithArguments("int"));

        await VerifyAsync(test);
    }

    #endregion // Tests
}