using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Assert that the fixed code parses into the same syntax kinds as the tested code once the parenthesized expressions
    /// are left out of both, so that a fix cannot pass by producing the expected text with a different reading
    /// </summary>
    /// <param name="testCode">Tested code, including diagnostic markup</param>
    /// <param name="fixedCode">Fixed code</param>
    private static void AssertParseIsPreserved(string testCode, string fixedCode)
    {
        var source = Regex.Replace(testCode, @"\{\|#\d+:|\|\}", string.Empty, RegexOptions.None, Regex.InfiniteMatchTimeout);

        Assert.AreSequenceEqual(GetSyntaxKindsWithoutParentheses(source), GetSyntaxKindsWithoutParentheses(fixedCode));
    }

    /// <summary>
    /// Get the kinds of all syntax nodes of the code except the parenthesized expressions
    /// </summary>
    /// <param name="code">Code</param>
    /// <returns>The syntax kinds in document order</returns>
    private static List<SyntaxKind> GetSyntaxKindsWithoutParentheses(string code)
    {
        return CSharpSyntaxTree.ParseText(code)
                               .GetRoot()
                               .DescendantNodesAndSelf()
                               .Where(static node => node is not ParenthesizedExpressionSyntax)
                               .Select(static node => node.Kind())
                               .ToList();
    }

    #endregion // Methods

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

    /// <summary>
    /// Verifying parentheses around a ref conditional simple assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional compound assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalCompoundAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) += 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional coalesce assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalCoalesceAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int? _a;
                                    private int? _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) ??= 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional simple assignment target are not reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalAssignmentTargetAreNotReportedWithCrLf()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifying parentheses around a ref assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ref int r = ref _a;

                                        (r = ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional target of a chained assignment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalTargetOfChainedAssignmentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        _c = (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional assignment target containing a comment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCommentedRefConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (/* note */ condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a non-ref conditional assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? _a : _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a value assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundValueAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (_a = _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a query expression assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundQueryAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private int[] _xs;

                                    public void Run(bool condition)
                                    {
                                        (from x in _xs select x) = null;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a conditional access assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a conditional element access compound assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessElementAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _arr;

                                    public void Run(bool condition)
                                    {
                                        (_arr?[0]) += 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a prefix unary assignment target whose operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (-_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a binary assignment target whose right operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundBinaryAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a + _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a coalesce assignment target whose right operand is a query expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundBinaryAssignmentTargetEndingInQueryAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private System.Collections.Generic.IEnumerable<int> _xs;

                                    public void Run(bool condition)
                                    {
                                        (_xs ?? from x in _xs select x) = null;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a binary assignment target whose right operand is a cast of a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCastAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a + (int)_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an await assignment target whose operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        (await _o?._t) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a coalesce assignment target whose right operand throws a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundThrowAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        (_n ?? throw _o?._e) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a range assignment target whose right operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRangeAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a.._o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a ref returning invocation assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRefReturningInvocationAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    private ref int GetRef() => ref _a;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(GetRef())|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     private ref int GetRef() => ref _a;

                                     public void Run(bool condition)
                                     {
                                         GetRef() = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a member access of a parenthesized conditional access assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundMemberAccessOfConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:((_o?._o)._f)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._o)._f = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying nested unnecessary parentheses around an identifier assignment target are both reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedUnnecessaryParenthesesAroundAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:({|#1:(_a)|})|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a prefix unary assignment target whose operand is parenthesized are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundNegatedParenthesizedConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(-(_o?._f))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         -(_o?._f) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a binary assignment target whose left operand is a conditional access are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundBinaryAssignmentTargetStartingWithConditionalAccessAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_o?._f + _a)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         _o?._f + _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a ref conditional assignment value are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRefConditionalAssignmentValueAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        _c = {|#0:(condition ? ref _a : ref _b)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private int _c;

                                     public void Run(bool condition)
                                     {
                                         _c = condition ? ref _a : ref _b;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the assignment value parentheses are reported when the target is a parenthesized ref conditional
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OnlyTheAssignmentValueParenthesesAreReportedNextToARefConditionalTarget()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = {|#0:(condition ? _c : 5)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private int _c;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = condition ? _c : 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a ref conditional assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundRefConditionalAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:(condition ? ref _a : ref _b)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying both redundant inner pairs are reported and fixed when three pairs wrap a ref conditional assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundRefConditionalAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:({|#1:(condition ? ref _a : ref _b)|})|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a conditional access operand of a throw expression stay reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundConditionalAccessInThrowExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Exception _e;
                                    private object _x;

                                    public object Get() => _x ?? throw {|#0:(_o?._e)|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Exception _e;
                                     private object _x;

                                     public object Get() => _x ?? throw _o?._e;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a null-forgiving conditional access assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_o?._f!) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a prefix unary assignment target whose operand is a null-forgiving conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedSuppressedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (-_o?._f!) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-forgiving conditional access assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundSuppressedConditionalAccessAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:(_o?._f!)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._f!) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a null-forgiving identifier assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundSuppressedIdentifierAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_s!)|} = null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public void Run(bool condition)
                                     {
                                         _s! = null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a null-forgiving assignment target whose operand is parenthesized are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundSuppressedParenthesizedConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:((_o?._f)!)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._f)! = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a relational pattern assignment target whose expression is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRelationalPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is > _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a combined pattern assignment target whose right pattern ends in a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCombinedPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is > 1 and < _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a negated constant pattern assignment target whose expression is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is not _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a relational pattern assignment target ending in a literal are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRelationalPatternAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a is > 5)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a is > 5 = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an assignment target ending in a parenthesized pattern are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundParenthesizedPatternAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a is (> _o?._f))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         _a is (> _o?._f) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a conditional access throw operand ending an assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessThrowOperandEndingAnAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        _n ?? throw (_o?._e) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying only the throw operand parentheses are reported when an assignment target ends in a parenthesized conditional access throw operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCoalesceAssignmentTargetEndingInParenthesizedThrowOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        (_n ?? throw {|#0:(_o?._e)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         (_n ?? throw _o?._e) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access await operand ending an assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessAwaitOperandEndingAnAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        _a + await ({|#0:(_o?._t)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         _a + await (_o?._t) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an await assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundAwaitAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        {|#0:(await _t)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         await _t = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a binary assignment target whose right operand is a cast are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundBinaryAssignmentTargetEndingInCastAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a + (int)_b)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a + (int)_b = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a coalesce assignment target whose right operand is a throw expression are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundCoalesceAssignmentTargetEndingInThrowAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_n ?? throw _e)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         _n ?? throw _e = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a range assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRangeAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a.._b)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a.._b = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an assignment target that is a range without a right operand are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundOpenRangeAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a..)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a.. = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier throw operand ending an assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundThrowOperandEndingAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        _n ?? throw {|#0:(_e)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         _n ?? throw _e = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier await operand ending an assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundAwaitOperandEndingAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        await {|#0:(_t)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         await _t = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation receiver with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessChainCrlfKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an element access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInElementAccessKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _a;
                                    private Test _o;

                                    public int[] Items => _a;

                                    public int Run()
                                    {
                                        return ({|#0:(_o?.Items)|})[0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int[] _a;
                                     private Test _o;

                                     public int[] Items => _a;

                                     public int Run()
                                     {
                                         return (_o?.Items)[0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as a member access assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessAssignmentKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private Test _p;
                                    public int X;

                                    public void Run()
                                    {
                                        ({|#0:(_o?._p)|}).X = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private Test _p;
                                     public int X;

                                     public void Run()
                                     {
                                         (_o?._p).X = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInInvocationKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ({|#0:(_o?._d)|})();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Func<int> _d;

                                     public int Run()
                                     {
                                         return (_o?._d)();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access await operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessAwaitOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task<int> Run()
                                    {
                                        return await ({|#0:(_o?._t)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task<int> Run()
                                     {
                                         return await (_o?._t);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a binary expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundBinaryExpressionInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public string Run()
                                    {
                                        return ({|#0:(_a + _b)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public string Run()
                                     {
                                         return (_a + _b).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-coalescing await operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCoalesceAwaitOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t1;
                                    private System.Threading.Tasks.Task<int> _t2;

                                    public async System.Threading.Tasks.Task<int> Run()
                                    {
                                        return await ({|#0:(_t1 ?? _t2)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t1;
                                     private System.Threading.Tasks.Task<int> _t2;

                                     public async System.Threading.Tasks.Task<int> Run()
                                     {
                                         return await (_t1 ?? _t2);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-forgiving expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundSuppressedExpressionInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ({|#0:(_s!)|}).Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return (_s!).Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying both redundant inner pairs are reported and fixed when three pairs wrap a conditional access used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundConditionalAccessInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:({|#1:(_o?._f)|})|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the outermost and the innermost pair are reported when the pair a chain needs sits in the middle of three pairs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCoalesceInsideAChainKeepTheNecessaryMiddlePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;
                                    private string _t;

                                    public string Run()
                                    {
                                        return {|#0:(({|#1:(_s ?? _t)|}).Length)|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;
                                     private string _t;

                                     public string Run()
                                     {
                                         return (_s ?? _t).Length.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying an assignment target ending in an await operand is reported together with the redundant inner pair of that operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessAwaitOperandInsideAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run()
                                    {
                                        {|#0:(_a + await ({|#1:(_o?._t)|}))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run()
                                     {
                                         _a + await (_o?._t) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the redundant inner pair is fixed when a comment sits inside the necessary outer pair around a conditional access chain receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessWithCommentInsideTheOuterPairAreFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return (/* note */ {|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (/* note */ _o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner pair is reported and no code fix is offered when a comment sits inside it around a conditional access chain receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessWithCommentInsideTheInnerPairWithholdTheCodeFix()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(/* note */ _o?._f)|}).ToString();
                                    }
                                }
                                """;

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .Last()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying both pairs are reported and fixed in one pass when nested parentheses wrap an identifier used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundIdentifierInMemberAccessChainAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public string Run()
                                    {
                                        return {|#0:({|#1:(_a)|})|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public string Run()
                                     {
                                         return _a.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the outer pair is reported when nested parentheses wrap a cast used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCastInMemberAccessChainReportTheOuterPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public string Run()
                                    {
                                        return {|#0:(((object)_a))|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public string Run()
                                     {
                                         return ((object)_a).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the outer pair is reported when nested parentheses wrap a switch expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundSwitchExpressionInMemberAccessChainReportTheOuterPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public int Run()
                                    {
                                        return {|#0:((_a switch { _ => "a" }))|}.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public int Run()
                                     {
                                         return (_a switch { _ => "a" }).Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying Fix All keeps the necessary pair of one chain and removes both pairs of another chain in the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesInSeveralChainsAreFixedTogether()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;
                                    private int _a;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString() + {|#1:({|#2:(_a)|})|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;
                                     private int _a;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString() + _a.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked delegate are not reported, because removing one pair leaves a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ((_d))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a field invocation are not reported when the field shares its name with its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundColorColorInvocationAreNotReported()
    {
        const string testCode = """
                                public enum Color
                                {
                                    Red
                                }

                                public class Test
                                {
                                    private System.Func<int, Color> Color;

                                    public Color Run()
                                    {
                                        return ((Color))(0);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a null-forgiving member access receiver are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedReceiverAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ((_s))!.Length;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a suppressed identifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedIdentifierAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public string Run()
                                    {
                                        return ((_s))!;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked member access of names are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundQualifiedDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ((_t._d))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around the receiver of a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithExpressionReceiverAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _v;

                                    public Value Run()
                                    {
                                        return ((_v)) with { P = 1 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an awaited receiver of a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedWithExpressionReceiverAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Task<Value> _task;

                                    public async Task<Value> Run()
                                    {
                                        return await ((_task)) with { P = 1 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a generic name followed by a bracket are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameBeforeBracketAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private static T Id<T>(T value) => value;

                                    public void Run()
                                    {
                                        var e = ((Id<int>))[0];
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a tuple of names followed by a null-forgiving operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleOfNamesBeforeSuppressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public (int, int) Run()
                                    {
                                        return (((_i, _i)))!;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying only the innermost of three pairs around an invoked delegate is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundInvokedDelegateKeepTwoPairs()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return (({|#0:(_d)|}))();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Func<int> _d;

                                     public int Run()
                                     {
                                         return ((_d))();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the innermost of three pairs around a null-forgiving member access receiver is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundSuppressedReceiverKeepTwoPairs()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return (({|#0:(_s)|}))!.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return ((_s))!.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the inner pair around a this member access followed by a null-forgiving operator is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundThisMemberBeforeSuppressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ({|#0:(this._s)|})!.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return (this._s)!.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying nested parentheses around an array followed by an element access are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundArrayBeforeBracketAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _arr;

                                    public int Run()
                                    {
                                        return {|#0:({|#1:(_arr)|})|}[0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int[] _arr;

                                     public int Run()
                                     {
                                         return _arr[0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the inner pair before a query keyword is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesBeforeQueryKeywordAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private int[] _seq;

                                    public int[] Run()
                                    {
                                        return (from x in ({|#0:(_seq)|}) select x).ToArray();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 public class Test
                                 {
                                     private int[] _seq;

                                     public int[] Run()
                                     {
                                         return (from x in (_seq) select x).ToArray();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the inner pair before a switch expression is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesBeforeSwitchExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public int Run()
                                    {
                                        return ({|#0:(_i)|}) switch { _ => 0 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public int Run()
                                     {
                                         return (_i) switch { _ => 0 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        (A * B) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a multiplication assignment target are still reported and fixed when they do not start the statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTargetOfChainedAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        _i = {|#0:(A * B)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;
                                     private int _i;

                                     public void Run()
                                     {
                                         _i = A * B = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are still reported and fixed when the inner expression cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMultiplicationWithLiteralAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;

                                    public void Run()
                                    {
                                        {|#0:(A * 5)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;

                                     public void Run()
                                     {
                                         A * 5 = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a with expression at the start of a statement are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithExpressionTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    public void Run()
                                    {
                                        (_a with { }) = _a;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a with expression on an invocation at the start of a statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundInvocationWithExpressionTargetAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    private Value M() => _a;

                                    public void Run()
                                    {
                                        {|#0:(M() with { })|} = _a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Value
                                 {
                                     public int P;
                                 }

                                 public class Test
                                 {
                                     private Value _a;

                                     private Value M() => _a;

                                     public void Run()
                                     {
                                         M() with { } = _a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are not reported when their removal would declare a generic local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int c;

                                    public void Run()
                                    {
                                        (A < B > c) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a relational expression at the start of a statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRelationalTargetAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        {|#0:(A < B)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         A < B = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a receiver at the start of a statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundReceiverOfMemberAccessChainAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test A;
                                    private int B;
                                    private int c;

                                    public void Run()
                                    {
                                        (A).B * c = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the first initializer of a for statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedFirstForInitializerAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        for ((A * B) = 5; ;)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a later initializer of a for statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedSecondForInitializerAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        for (_i = 0, {|#0:(A * B)|} = 5; ;)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;
                                     private int _i;

                                     public void Run()
                                     {
                                         for (_i = 0, A * B = 5; ;)
                                         {
                                             break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around the resource of a using statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedUsingResourceAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        using ((A * B) = 5)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an out argument are not reported when their removal would declare a pointer variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedOutArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private void O(out int value) => value = 0;

                                    public void Run()
                                    {
                                        O(out (A * B));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an identifier out argument are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundIdentifierOutArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    private void O(out int value) => value = 0;

                                    public void Run()
                                    {
                                        O(out {|#0:(_i)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     private void O(out int value) => value = 0;

                                     public void Run()
                                     {
                                         O(out _i);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a multiplication at the start of a statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundPointerShapedTargetAtStatementStartKeepOnePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        ({|#0:(A * B)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         (A * B) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a with expression at the start of a statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundWithExpressionTargetAtStatementStartKeepOnePair()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    public void Run()
                                    {
                                        ({|#0:(_a with { })|}) = _a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Value
                                 {
                                     public int P;
                                 }

                                 public class Test
                                 {
                                     private Value _a;

                                     public void Run()
                                     {
                                         (_a with { }) = _a;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around the target of an initializer element are not reported when their removal would assign a member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundObjectInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                public class Bag : System.Collections.IEnumerable
                                {
                                    public int Value { get; set; }

                                    public void Add(int item)
                                    {
                                    }

                                    public System.Collections.IEnumerator GetEnumerator() => null;
                                }

                                public class Test
                                {
                                    public Bag Run()
                                    {
                                        return new Bag { (Value) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of a collection initializer element are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCollectionInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { (_i) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of an anonymous object member are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAnonymousObjectMemberTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public object Run()
                                    {
                                        return new { (_i) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of a with initializer element are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;
                                    private int P;

                                    public Value Run()
                                    {
                                        return _a with { (P) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of an array initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArrayInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public int[] Run()
                                    {
                                        int[] values = { {|#0:(_i)|} = 1 };

                                        return values;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public int[] Run()
                                     {
                                         int[] values = { _i = 1 };

                                         return values;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a member access target of a collection initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMemberAccessCollectionInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private Test _t;
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { {|#0:(_t._i)|} = 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     private Test _t;
                                     private int _i;

                                     public List<int> Run()
                                     {
                                         return new List<int> { _t._i = 1 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around the target of a compound assignment initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCompoundAssignedInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Bag : System.Collections.IEnumerable
                                {
                                    public int Value { get; set; }

                                    public void Add(int item)
                                    {
                                    }

                                    public System.Collections.IEnumerator GetEnumerator() => null;
                                }

                                public class Test
                                {
                                    public Bag Run()
                                    {
                                        return new Bag { {|#0:(Value)|} += 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Bag : System.Collections.IEnumerable
                                 {
                                     public int Value { get; set; }

                                     public void Add(int item)
                                     {
                                     }

                                     public System.Collections.IEnumerator GetEnumerator() => null;
                                 }

                                 public class Test
                                 {
                                     public Bag Run()
                                     {
                                         return new Bag { Value += 1 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying only the inner of two pairs around the target of a collection initializer element is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCollectionInitializerElementTargetKeepOnePair()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { ({|#0:(_i)|}) = 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     private int _i;

                                     public List<int> Run()
                                     {
                                         return new List<int> { (_i) = 1 };
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a generic name followed by an assignment operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        (A<B>) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a generic name inside a chained assignment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameTargetOfChainedAssignmentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        _i = (A<B>) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by an arithmetic operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeArithmeticOperatorAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public async Task<int> Run()
                                    {
                                        return await (A<B>) + 1;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by a logical operator are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeLogicalOperatorAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public async Task<bool> Run()
                                    {
                                        return await {|#0:(A<B>)|} && true;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public async Task<bool> Run()
                                     {
                                         return await A<B> && true;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a generic method group followed by a semicolon are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericMethodGroupBeforeSemicolonAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private static T Id<T>(T value) => value;

                                    public System.Func<int, int> Run()
                                    {
                                        return {|#0:(Id<int>)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private static T Id<T>(T value) => value;

                                     public System.Func<int, int> Run()
                                     {
                                         return Id<int>;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a generic name followed by an assignment operator is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundGenericNameAssignmentTargetKeepOnePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        ({|#0:(A<B>)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         (A<B>) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the opening argument's parentheses are reported when removing both pairs would join the arguments into a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsJoiningIntoGenericInvocationKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are not reported when the opening argument has no parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterBareOpeningArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(G < A, (B > (7)));
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an opening argument are not reported when the closing argument has no parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundOpeningArgumentBeforeBareClosingArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M((G < A), B > (7));
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around both arguments are still reported and fixed when no parenthesis follows the greater-than operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsWithoutTypeArgumentFollowerAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, {|#1:(B > 7)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, B > 7);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying parentheses around named arguments are still reported and fixed, because the name keeps the arguments apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNamedArgumentsJoiningIntoGenericInvocationAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(x: {|#0:(G < A)|}, y: {|#1:(B > (7))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(x: G < A, y: B > (7));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the opening and the type shaped middle arguments are reported while the closing argument keeps its parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTypeShapedMiddleArgumentKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:(C)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, C, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying every argument is reported and fixed when a literal middle argument keeps the arguments apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundLiteralMiddleArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:(1)|}, {|#2:(B > (7))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, 1, B > (7));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
    }

    /// <summary>
    /// Verifying only the opening tuple element's parentheses are reported when removing both pairs would join the elements into a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleElementsJoiningIntoGenericInvocationKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ({|#0:(G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a receiver at the start of a statement are still reported and fixed when the remaining tokens cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundReceiverOfMemberAssignmentAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private int _i;

                                    public void Run()
                                    {
                                        {|#0:(_t)|}._i = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private int _i;

                                     public void Run()
                                     {
                                         _t._i = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses inside a parenthesized member access followed by a null-forgiving operator are not reported, because removing them leaves a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideSuppressedReceiverChainAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return ((_t)._s)!.Length;
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses inside a parenthesized member access followed by a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideWithExpressionReceiverChainAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return ((_t)._v) with { P = 1 };
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an element of a parenthesized tuple followed by a null-forgiving operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideSuppressedTupleAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return (((_i), _i))!;
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the outer parentheses around an invoked member access are reported while the inner pair is kept until the outer pair is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OuterParenthesesAroundInvokedReceiverChainAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return {|#0:((_t)._d)|}();
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private Value _v;
                                     private string _s;
                                     private int _i;
                                     private System.Func<int> _d;

                                     public object Run()
                                     {
                                         return _t._d();
                                     }
                                 }

                                 public struct Value
                                 {
                                     public int P;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 2;
                                config.NumberOfFixAllIterations = 2;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked alias qualified name are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAliasQualifiedDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var x = ((global::N.C.D))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an alias qualified multiplication at the start of a statement are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAliasQualifiedPointerShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        (global::N.C.A * B) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when an alias qualified middle argument continues the span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsJoiningThroughAliasQualifiedMiddleKeepTheClosingPair()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, global::N.C.Ci, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public class C
                                     {
                                         public static System.Func<int> D;
                                         public static int A;
                                         public static int Ci;
                                     }
                                 }

                                 public class Test
                                 {
                                     private int B;
                                     private int G;
                                     private int A;

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, global::N.C.Ci, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when an index operator follows its greater-than operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsJoiningBeforeIndexOperatorKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, (B > ^1));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, (B > ^1));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when the opening argument ends in the false branch of a conditional
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundOpeningArgumentEndingInConditionalKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(z ? x : G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(z ? x : G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when a middle argument becomes type shaped once its inner parentheses are gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMiddleArgumentWithParenthesizedReceiverKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:({|#2:(_t)|}.B)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, _t.B, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
    }

    /// <summary>
    /// Verifying parentheses around a generic shaped later tuple element are not reported, because removing them would declare a variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedSecondTupleElementAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, (A < B > c));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a generic shaped first tuple element are not reported, because removing them would declare a variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedFirstTupleElementAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ((A < B > c), x);
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a multiplication tuple element are still reported and fixed, because a tuple element is never read as a pointer declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTupleElementAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, {|#0:(A * B)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (x, A * B);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a generic name with a predefined type argument are still reported and fixed before an assignment operator, because such a type argument list stays one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundDefiniteGenericNameAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        {|#0:(Id<int>)|} = 5;

                                        return null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     private static T Id<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         Id<int> = 5;

                                         return null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by an is operator are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeIsOperatorAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await {|#0:(A<B>)|} is object;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     private static T Id<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         return await A<B> is object;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
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