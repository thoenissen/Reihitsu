using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>: the listed positions, throw expressions, the space between a keyword and the unwrapped expression, and comments and directives in and around the parentheses
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// A document whose <c>/* */</c> comment starts inside the <c>#if !X</c> group and holds the line <c>#else</c>, so a build
    /// with <c>X</c> reads the reported line as the content of the verbatim string <c>s</c>
    /// </summary>
    private const string CarrierDocumentSource = """
                                                 public class Other
                                                 {
                                                     public int Run(int value)
                                                     {
                                                 #if !X
                                                         /*
                                                 #else
                                                         var s = @"
                                                         */
                                                         value = {|RH3002:(value)|}; //";
                                                 #endif
                                                         return value;
                                                     }
                                                 }
                                                 """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying unnecessary parentheses in return statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    [Requirement("RH3002-008")]
    public async Task UnnecessaryParenthesesInReturnAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:(value)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in throw statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    [Requirement("RH3002-008")]
    public async Task UnnecessaryParenthesesInThrowAreReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Run(Exception ex)
                                    {
                                        throw {|#0:(ex)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Run(Exception ex)
                                     {
                                         throw ex;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in variable assignment are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    [Requirement("RH3002-008")]
    public async Task UnnecessaryParenthesesInAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        int x = {|#0:(42)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         int x = 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in method argument are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    [Requirement("RH3002-008")]
    public async Task UnnecessaryParenthesesInArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(int value)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method({|#0:(5)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Method(int value)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(5);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in expression-bodied member are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    [Requirement("RH3002-008")]
    public async Task UnnecessaryParenthesesInExpressionBodiedMemberAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Value => {|#0:(10)|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Value => 10;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying necessary parentheses around cast are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-003")]
    public async Task NecessaryParenthesesAroundCastAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(object value)
                                    {
                                        return ((int)value);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying necessary parentheses around lambda are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-003")]
    public async Task NecessaryParenthesesAroundLambdaAreNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int> Run()
                                    {
                                        return (() => 5);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around safe member access are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    public async Task UnnecessaryParenthesesAroundMemberAccessAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(string text)
                                    {
                                        return {|#0:(text)|}.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(string text)
                                     {
                                         return text.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around safe invocation are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    public async Task UnnecessaryParenthesesAroundInvocationAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int GetValue()
                                    {
                                        return 0;
                                    }

                                    public int Run()
                                    {
                                        return {|#0:(GetValue())|}.ToString().Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int GetValue()
                                     {
                                         return 0;
                                     }

                                     public int Run()
                                     {
                                         return GetValue().ToString().Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in compound assignment are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInCompoundAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        int x = 0;
                                        x += {|#0:(5)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         int x = 0;
                                         x += 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that no code fix is offered when the parentheses enclose a documentation comment, because
    /// removing them would discard it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentedParenthesesAreNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Method(int y)
                                       {
                                           return (/** The wrapped operand. */ y);
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment written before the parenthesized expression does not withhold the code fix. The
    /// rewrite transplants the node's own outer trivia onto the replacement, so that comment is never crossed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    [Requirement("RH3002-009")]
    public async Task VerifyCommentBeforeTheParenthesesIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Method(int y)
                                       {
                                           return
                                               /* note */ (y);
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);

        var fixedCode = await ApplyCodeFixAsync(codeFixData);

        Assert.Contains("/* note */ y;", fixedCode);
        Assert.DoesNotContain("(y)", fixedCode);
    }

    /// <summary>
    /// Verifying that a comment written inside the parentheses still withholds the code fix, because removing them
    /// would discard it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    public async Task VerifyCommentInsideTheParenthesesWithholdsTheCodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Method(int y)
                                       {
                                           return (/* note */ y);
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying unnecessary parentheses in a throw expression of an expression-bodied accessor are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowExpressionAccessorAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X
                                    {
                                        get => throw {|#0:(new System.Exception())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X
                                     {
                                         get => throw new System.Exception();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in a throw expression of an expression-bodied accessor are reported and fixed with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowExpressionAccessorCrlfAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X
                                    {
                                        get => throw {|#0:(new System.Exception())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X
                                     {
                                         get => throw new System.Exception();
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in a throw expression of an expression-bodied method are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowExpressionMethodAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run() => throw {|#0:(new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run() => throw new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in a throw expression on the right side of a coalesce are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowExpressionCoalesceAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run(string text)
                                    {
                                        return text ?? throw {|#0:(new System.Exception())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run(string text)
                                     {
                                         return text ?? throw new System.Exception();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an object creation in a throw statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowStatementWithObjectCreationAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        throw {|#0:(new System.Exception())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         throw new System.Exception();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a coalesce operand of a throw expression are reported and fixed, because the operand binds at the coalesce level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundCoalesceInThrowExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int X => throw {|#0:(_exception ?? new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Exception _exception;

                                     public int X => throw _exception ?? new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying parentheses around a conditional operand of a throw expression are not reported, because removing them would make the throw expression the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    public async Task NecessaryParenthesesAroundConditionalInThrowExpressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a conditional operand of a throw expression on the right side of a coalesce are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundConditionalInCoalesceThrowExpressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _condition;

                                    public string Run(string text)
                                    {
                                        return text ?? throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an assignment operand of a throw expression are not reported, because removing them would make the throw expression the assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundAssignmentInThrowExpressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int X => throw (_exception = new System.Exception());
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a coalesce assignment operand of a throw expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundCoalesceAssignmentInThrowExpressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int X => throw (_exception ??= new System.Exception());
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a cast operand of a throw expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundCastInThrowExpressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private object _exception;

                                    public int X => throw ((System.Exception)_exception);
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying nested unnecessary parentheses in a throw expression are both reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedUnnecessaryParenthesesInThrowExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int X => throw {|#0:({|#1:(_exception)|})|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Exception _exception;

                                     public int X => throw _exception;
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional operand of a throw expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-006")]
    public async Task NestedParenthesesAroundConditionalInThrowExpressionKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw ({|#0:(_condition ? new System.Exception() : new System.InvalidOperationException())|});
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _condition;

                                     public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying a comment inside the parentheses of a throw expression is reported but withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInsideThrowExpressionParenthesesWithholdsTheCodeFix()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw {|#0:(/* note */ new System.Exception())|};
                                }
                                """;

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying a comment between the throw keyword and the parentheses of a throw expression is kept by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeThrowExpressionParenthesesIsKept()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw /* note */ {|#0:(new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw /* note */ new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying multi-line unnecessary parentheses in a throw expression are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultiLineUnnecessaryParenthesesInThrowExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw {|#0:(
                                        new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in several throw expressions of one document are reported and fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SeveralThrowExpressionsInOneDocumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw {|#0:(new System.Exception())|};

                                    public int Y => throw {|#1:(new System.InvalidOperationException())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw new System.Exception();

                                     public int Y => throw new System.InvalidOperationException();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the code fix separates the throw keyword of a throw expression from the operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ThrowExpressionWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw{|#0:(new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the throw keyword of a throw statement from the operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ThrowStatementWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        throw{|#0:(new System.Exception())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         throw new System.Exception();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the return keyword from an identifier operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return{|#0:(value)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the return keyword from a parenthesized member access target when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReturnWithoutSpaceBeforeMemberAccessParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run(int value)
                                    {
                                        return{|#0:(value)|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run(int value)
                                     {
                                         return value.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the await keyword from the operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AwaitWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public async System.Threading.Tasks.Task Run(System.Threading.Tasks.Task task)
                                    {
                                        await{|#0:(task)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public async System.Threading.Tasks.Task Run(System.Threading.Tasks.Task task)
                                     {
                                         await task;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the ref keyword of an argument from the operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RefArgumentWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run(int value)
                                    {
                                        Increment(ref{|#0:(value)|});
                                    }

                                    private static void Increment(ref int value)
                                    {
                                        value++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run(int value)
                                     {
                                         Increment(ref value);
                                     }

                                     private static void Increment(ref int value)
                                     {
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix adds no whitespace when the keyword and the first operand token stay separate tokens without it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReturnWithoutSpaceBeforeUnaryOperandAddsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return{|#0:(-value)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return-value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix adds no whitespace when trivia already separates the keyword from the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CommentBetweenKeywordAndParenthesesAddsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw/* note */{|#0:(new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw/* note */new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying parentheses around a query operand of a throw expression are not reported, because the query would
    /// otherwise stand below the precedence of a throw expression operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundQueryInThrowExpressionAreNotReported()
    {
        const string testCode = """
                                using System;

                                public class QueryableException : Exception
                                {
                                    public QueryableException Select(Func<QueryableException, QueryableException> selector) => this;
                                }

                                public class Test
                                {
                                    private QueryableException _exception;

                                    public int X => throw (from exception in _exception select exception);
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying Fix All removes nested parentheses of a throw expression whose fixes produce different text without losing a closing parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesWithInvocationInThrowExpressionAreFixedTogether()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(System.Exception exception) => throw {|#0:(Make({|#1:(exception)|}))|};

                                    private static System.Exception Make(System.Exception exception) => exception;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(System.Exception exception) => throw Make(exception);

                                     private static System.Exception Make(System.Exception exception) => exception;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying Fix All removes nested parentheses of a return statement whose fixes produce different text without losing a closing parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesWithInvocationInReturnAreFixedTogether()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:(Get({|#1:(value)|}))|};
                                    }

                                    private static int Get(int value) => value;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return Get(value);
                                     }

                                     private static int Get(int value) => value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the code fix separates the operand from the following keyword when no whitespace follows the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AwaitWithoutSpaceAfterParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                public class Test
                                {
                                    public async System.Threading.Tasks.Task<bool> Run(System.Threading.Tasks.Task<object> task)
                                    {
                                        return await {|#0:(task)|}is string;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public async System.Threading.Tasks.Task<bool> Run(System.Threading.Tasks.Task<object> task)
                                     {
                                         return await task is string;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix separates the operand of a throw expression from a following query continuation when no whitespace follows the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ThrowExpressionWithoutSpaceAfterParenthesesKeepsTheTokensSeparate()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    public System.Collections.Generic.IEnumerable<object> Run(object[] items, System.Exception exception)
                                    {
                                        return from item in items
                                               select item ?? throw {|#0:(exception)|}into grouped
                                               select grouped;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 public class Test
                                 {
                                     public System.Collections.Generic.IEnumerable<object> Run(object[] items, System.Exception exception)
                                     {
                                         return from item in items
                                                select item ?? throw exception into grouped
                                                select grouped;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying a directive between the throw keyword and the parentheses of a throw expression is kept by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveBetweenThrowAndParenthesesIsKept()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw
                                    #if DEBUG
                                    #endif
                                        {|#0:(new System.Exception())|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw
                                     #if DEBUG
                                     #endif
                                         new System.Exception();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying a directive inside the parentheses of a throw expression is reported but withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    public async Task VerifyDirectiveInsideThrowExpressionParenthesesWithholdsTheCodeFix()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw {|#0:(
                                    #if DEBUG
                                    #endif
                                        new System.Exception())|};
                                }
                                """;

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying a comment after the closing parenthesis of a throw expression is kept by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterThrowExpressionParenthesesIsKept()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int X => throw {|#0:(new System.Exception())|} /* note */;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int X => throw new System.Exception() /* note */;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string whose parentheses are removed with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringCrlfAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after a verbatim interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundVerbatimInterpolatedStringAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($@"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $@"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after a raw interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRawInterpolatedStringAddNoSpace()
    {
        const string testCode = """"
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"""x{_i}""")|};
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"""x{_i}""";
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string argument whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringArgumentAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = string.Concat({|#0:($"x{_i}")|}, "y");
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = string.Concat($"x{_i}", "y");
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string member access receiver whose inner parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundInterpolatedStringReceiverAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var n = ({|#0:($"x{_i}")|}).Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var n = ($"x{_i}").Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying nested parentheses around an interpolated string are removed together without adding a space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundInterpolatedStringAreFixedWithoutSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:({|#1:($"x{_i}")|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying no space is added after a multi-line raw interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundMultiLineRawInterpolatedStringAddNoSpace()
    {
        const string testCode = """"
                                public class Test
                                {
                                    private int _i;

                                    public string Run()
                                    {
                                        return {|#0:($"""
                                               x{_i}
                                               """)|};
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 public class Test
                                 {
                                     private int _i;

                                     public string Run()
                                     {
                                         return $"""
                                                x{_i}
                                                """;
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no space is added on either side when parentheses around an interpolated string directly follow a return keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReturnWithoutSpaceBeforeInterpolatedStringParenthesesAddsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public string Run()
                                    {
                                        return{|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public string Run()
                                     {
                                         return$"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies parentheses around a whole expression are reported and fixed in every position where the expression
    /// stands alone: an expression body of a member and of an accessor, a parameter default value, a variable
    /// initializer, an assignment, an argument, a tuple element, a return and a throw statement, the operand of a
    /// throw expression, and a pair directly inside another pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    int _f;

                                    int P => {|RH3002:(_f)|};

                                    int Q { get => {|RH3002:(_f)|}; }

                                    int M(int a, string s, int d = {|RH3002:(1)|})
                                    {
                                        int b = {|RH3002:(a)|};
                                        b = {|RH3002:(a + 1)|};
                                        Console.WriteLine({|RH3002:(a)|});
                                        var t = ({|RH3002:(a)|}, 1);
                                        var u = {|RH3002:(s ?? throw {|RH3002:(new InvalidOperationException())|})|};
                                        return {|RH3002:({|RH3002:(b)|})|};
                                    }

                                    void Fail()
                                    {
                                        throw {|RH3002:(new InvalidOperationException())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Threading.Tasks;

                                 class C
                                 {
                                     int _f;

                                     int P => _f;

                                     int Q { get => _f; }

                                     int M(int a, string s, int d = 1)
                                     {
                                         int b = a;
                                         b = a + 1;
                                         Console.WriteLine(a);
                                         var t = (a, 1);
                                         var u = s ?? throw new InvalidOperationException();
                                         return b;
                                     }

                                     void Fail()
                                     {
                                         throw new InvalidOperationException();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a whole argument of an object creation are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAnArgumentOfAnObjectCreationAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    C(int a)
                                    {
                                    }

                                    static C M(int a)
                                    {
                                        return new C({|RH3002:(a)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     C(int a)
                                     {
                                     }

                                     static C M(int a)
                                     {
                                         return new C(a);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a whole argument of an element access are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAnArgumentOfAnElementAccessAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int[] x, int a)
                                    {
                                        return x[{|RH3002:(a)|}];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int[] x, int a)
                                     {
                                         return x[a];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the value after <c>=</c> in a field and in a property declaration are reported over
    /// the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheValueOfAMemberDeclarationAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _f = {|RH3002:(1)|};

                                    int P { get; } = {|RH3002:(2)|};
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _f = 1;

                                     int P { get; } = 2;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the expression body of a local function are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheExpressionBodyOfALocalFunctionAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        int L() => {|RH3002:(a)|};

                                        return L();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         int L() => a;

                                         return L();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the left side of an assignment are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheLeftSideOfAnAssignmentAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    void M()
                                    {
                                        {|RH3002:(_a)|} = 3;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     void M()
                                     {
                                         _a = 3;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a conditional expression as the operand of a throw expression keeps its parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    public async Task ConditionalOperandOfThrowExpressionInExpressionBodyIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    int Fail(bool c) => throw (c ? new Exception() : new InvalidOperationException());
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix inserts a space where the keyword would otherwise join the content, and keeps the comments
    /// in front of and after the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task FixSeparatesTheKeywordAndKeepsTheCommentsAroundTheParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return{|RH3002:(a)|};
                                    }

                                    int N(int a)
                                    {
                                        return /* x */ {|RH3002:(a)|} /* y */;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         return a;
                                     }

                                     int N(int a)
                                     {
                                         return /* x */ a /* y */;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment directly inside the parentheses keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    public async Task CommentAfterTheOpeningParenthesisIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(/* x */ a)|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that redundant parentheses in a statement condition, a lambda body, a <c>switch</c> expression
    /// arm, an attribute argument, a binary operand, and around a <c>typeof</c> receiver are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RedundantParenthesesOutsideTheListedPositionsAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    [Obsolete(("x"))]
                                    int M(int a, bool c)
                                    {
                                        if ((c)) { }
                                        while ((c)) { }
                                        Func<int, int> f = x => (x);
                                        var n = (typeof(int)).Name;
                                        var s = a switch { _ => (a) };
                                        return a + (a);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a number, which would read as one identifier with <c>return</c>, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundANumberGetsOneSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run()
                                    {
                                        return{|#0:(42)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run()
                                     {
                                         return 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around an identifier that starts with a Unicode escape, which would read as one identifier with <c>return</c>, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAUnicodeEscapeIdentifierGetsOneSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int a)
                                    {
                                        return{|#0:(\u0061)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int a)
                                     {
                                         return \u0061;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a verbatim identifier, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimIdentifierGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int a)
                                    {
                                        return{|#0:(@a)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int a)
                                     {
                                         return@a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a verbatim string, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimStringGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return{|#0:(@"s")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return@"s";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a string, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAStringGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return{|#0:("s")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return"s";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a Fix All in the project, started from a document without a hiding carrier, removes the parentheses in that
    /// document and leaves a second document that holds one as it is, with its diagnostic still reported: in a build with
    /// <c>X</c>, the reported line is the content of a verbatim string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged()
    {
        const string source = """
                              public class Test
                              {
                                  public int Run(int value)
                                  {
                                      return {|RH3002:(value)|};
                                  }
                              }
                              """;
        const string fixedSource = """
                                   public class Test
                                   {
                                       public int Run(int value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that a Fix All in the solution, started from a document without a hiding carrier, removes the parentheses in that
    /// document and leaves a document of another project that holds a hiding carrier as it is, with its diagnostic still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheSolutionLeavesADocumentWithAHidingCarrierInAnotherProjectUnchanged()
    {
        const string source = """
                              public class Test
                              {
                                  public int Run(int value)
                                  {
                                      return {|RH3002:(value)|};
                                  }
                              }
                              """;
        const string fixedSource = """
                                   public class Test
                                   {
                                       public int Run(int value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));
                         test.FixedState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that unnecessary parentheses in an <c>#if</c> condition in front of the closing brace of a class with
    /// a primary constructor are reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-007")]
    public async Task ParenthesesInIfConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorAreReportedOnce()
    {
        const string testData = """
                                public class Service(int value)
                                {
                                    public int Value => value;
                                #if ({|#0:(A)|})
                                #endif
                                }
                                """;

        await Verify(testData, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that unnecessary parentheses in an <c>#if</c> condition in front of a class with a primary constructor
    /// are reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-007")]
    public async Task ParenthesesInIfConditionInFrontOfClassWithPrimaryConstructorAreReportedOnce()
    {
        const string testData = """
                                #if ({|#0:(A)|})
                                #endif
                                public class Service(int value)
                                {
                                    public int Value => value;
                                }
                                """;

        await Verify(testData, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if DEBUG</c> inside the
    /// parentheses are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if DEBUG</c> inside the
    /// parentheses are reported, and no code fix is offered, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFixWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r\n"));
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose guarded code is active, because the <c>#if</c> condition
    /// holds, are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveWithActiveCodeInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if !NEVER
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if false</c> inside the
    /// parentheses are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyIfFalseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if false
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value split into an excluded <c>#if</c> branch and an active
    /// <c>#else</c> branch are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyElseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #else
                                            a
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value split into <c>#if</c>, <c>#elif</c>, and <c>#else</c> branches are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyElifDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #elif Y
                                            a - b
                                #else
                                            a
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an argument whose excluded code stands behind <c>#if</c> inside the parentheses are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideArgumentParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        N({|RH3002:(a
                                #if X
                                            + 1
                                #endif
                                            )|});
                                    }

                                    void N(int value)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver are reported, without a code fix, when the build leaves only a
    /// name between them because the <c>#if</c> code that would make them necessary is excluded
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundExcludedCodeAreReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a)
                                    {
                                        return {|RH3002:(a
                                #if X
                                            + 1
                                #endif
                                            )|}.ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver are not reported when the build defines the symbol whose
    /// <c>#if</c> code makes them necessary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundActiveBinaryCodeAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a)
                                    {
                                        return (a
                                #if X
                                            + 1
                                #endif
                                            ).ToString();
                                    }
                                }
                                """;

        await Verify(testCode,
                     static test => test.SolutionTransforms.Add(static (solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                                    ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("X"))
                                                                                                    : solution));
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver whose active <c>#else</c> branch holds only a name are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundElseBranchNameAreReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #else
                                            a
                                #endif
                                            )|}.ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying Fix All removes a removable pair and leaves the reported pair with a conditional directive inside it
    /// unchanged in the same document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    [Requirement("RH3002-010")]
    public async Task VerifyFixAllLeavesParenthesesWithConditionalDirectiveReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        var first = {|RH3002:(a)|};

                                        return {|RH3002:(first
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         var first = a;

                                         return {|RH3002:(first
                                 #if DEBUG
                                             + 1
                                 #endif
                                             )|};
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic whose fix is withheld stays and is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                            });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:({|#1:(value)|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        // Both pairs are rewritten together, building the outer replacement from the already fixed inner pair
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}