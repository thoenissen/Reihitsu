using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying unnecessary parentheses in return statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in throw statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in variable assignment are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in method argument are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in expression-bodied member are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying necessary parentheses around cast are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around safe invocation are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a conditional operand of a throw expression are not reported, because removing them would make the throw expression the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional operand of a throw expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the code fix separates the return keyword from an identifier operand when no whitespace precedes the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying a directive inside the parentheses of a throw expression is reported but withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}