using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH3001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3001NotOperatorShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3001NotOperatorShouldNotBeUsedAnalyzer, RH3001NotOperatorShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying not operator on literal is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnLiteral()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return {|#0:!|}false;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return false == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnField()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on property is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnProperty()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Property { get; set; }

                                    public bool GetProperty()
                                    {
                                        return {|#0:!|}Property;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Property { get; set; }

                                     public bool GetProperty()
                                     {
                                         return Property == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnMethodCall()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return true;
                                    }

                                    public bool GetMethod()
                                    {
                                        return {|#0:!|}GetBool();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return true;
                                     }

                                     public bool GetMethod()
                                     {
                                         return GetBool() == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on a nullable bool operand is not reported, because <c>!x</c> and <c>x == false</c> are not equivalent for <see langword="null"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnNullableBoolIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool? GetValue(bool? value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying not operator on an operand with a user-defined operator is not reported, because the rewrite is not equivalent
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnUserDefinedOperatorIsNotReported()
    {
        const string testCode = """
                                public struct Custom
                                {
                                    public static Custom operator !(Custom value)
                                    {
                                        return value;
                                    }
                                }

                                public class Test
                                {
                                    public Custom GetValue(Custom value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying leading trivia of the not operator is preserved when the fix is applied
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorTriviaIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return /* comment */ {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return /* comment */ _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies a comment between a not operator and its operand survives the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue469CommentBetweenNotOperatorAndOperandIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|} /* Keep. */ _field;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return /* Keep. */ _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies an end-of-line comment between a not operator and its operand survives the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EndOfLineCommentBetweenNotOperatorAndOperandIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|} // Keep.
                                               _field;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return // Keep.
                                                _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies directives and disabled text nested inside an operand do not suppress a fix for a clean operator gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedOperandDirectivesRemainAvailableAndArePreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField()
                                    {
                                        return {|#0:!|}(
                                #if FEATURE
                                            true /* enabled branch */
                                #else
                                            false /* disabled branch */
                                #endif
                                        );
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetField()
                                     {
                                         return (
                                 #if FEATURE
                                             true /* enabled branch */
                                 #else
                                             false /* disabled branch */
                                 #endif
                                         ) == false;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test => test.SolutionTransforms.Add(static (solution, projectId) =>
                                                                       {
                                                                           var project = solution.GetProject(projectId);

                                                                           return project?.ParseOptions is Microsoft.CodeAnalysis.CSharp.CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("FEATURE"))
                                                                                      : solution;
                                                                       }),
                     Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies no fix is offered when conditional-compilation trivia separates the not operator and operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedAcrossDirectiveAndDisabledText()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField()
                                    {
                                        return !
                                #if FEATURE
                                               true
                                #else
                                               false
                                #endif
                                               ;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>().Single().OperatorToken.GetLocation(),
                                                   "FEATURE");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies no fix is offered when skipped syntax separates the not operator and operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedAcrossSkippedSyntax()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField(bool value)
                                    {
                                        return ! @ value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>().Single().OperatorToken.GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool first, bool second)
                                    {
                                        return {|#0:!|}(first && {|#1:!|}second);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool first, bool second)
                                     {
                                         return (first && second == false) == false;
                                     }
                                 }
                                 """;

        // The inner operator sits inside the operand the outer rewrite replaces, so the two text changes overlap
        // and the batch cannot apply them in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}