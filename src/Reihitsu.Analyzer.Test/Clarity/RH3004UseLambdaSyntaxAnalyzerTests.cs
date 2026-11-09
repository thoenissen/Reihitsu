using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3004UseLambdaSyntaxAnalyzer"/> and <see cref="RH3004UseLambdaSyntaxCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3004UseLambdaSyntaxAnalyzerTests : BatchCodeFixTestsBase<RH3004UseLambdaSyntaxAnalyzer, RH3004UseLambdaSyntaxCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying anonymous methods with parameters are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    [Requirement("RH3004-002")]
    [Requirement("RH3004-003")]
    public async Task AnonymousMethodWithParametersIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int, int, int> Run()
                                    {
                                        return {|#0:delegate|}(int left, int right) { return left + right; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int, int, int> Run()
                                     {
                                         return (int left, int right) => left + right;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying anonymous methods with block bodies are converted to lambda expressions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task AnonymousMethodWithBlockBodyIsConverted()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Action<string> GetLogger()
                                    {
                                        return {|#0:delegate|}(string message)
                                        {
                                            Console.WriteLine(message);
                                            Console.WriteLine("Logged");
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Action<string> GetLogger()
                                     {
                                         return (string message) =>
                                                {
                                                    Console.WriteLine(message);
                                                    Console.WriteLine("Logged");
                                                };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying anonymous methods with zero parameters are converted correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    [Requirement("RH3004-003")]
    public async Task AnonymousMethodWithNoParametersIsConverted()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int> GetValue()
                                    {
                                        return {|#0:delegate|}() { return 42; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int> GetValue()
                                     {
                                         return () => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying async anonymous methods are preserved as async lambdas
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    public async Task AsyncAnonymousMethodIsConvertedToAsyncLambda()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    public Func<Task<int>> GetAsyncValue()
                                    {
                                        return async {|#0:delegate|}() { return await Task.FromResult(42); };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     public Func<Task<int>> GetAsyncValue()
                                     {
                                         return async () => await Task.FromResult(42);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying anonymous methods with single parameter are converted correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    [Requirement("RH3004-003")]
    public async Task AnonymousMethodWithSingleParameterIsConverted()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<string, int> GetLength()
                                    {
                                        return {|#0:delegate|}(string text) { return text.Length; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<string, int> GetLength()
                                     {
                                         return (string text) => text.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying a comment inside a single-return delegate block is preserved by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task CommentInsideSingleReturnBlockIsPreserved()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Process()
                                    {
                                        Func<int, int> square = {|#0:delegate|}(int x) { return x * x; /* keep me */ };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Process()
                                     {
                                         Func<int, int> square = (int x) =>
                                                                 {
                                                                     return x * x; /* keep me */
                                                                 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying anonymous methods without explicit parameter lists are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    public async Task AnonymousMethodWithoutParameterListIsNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Action GetAction()
                                    {
                                        return delegate { Console.WriteLine("Hello"); };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying multiple anonymous methods in the same file are all reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    [Requirement("RH3004-002")]
    public async Task MultipleAnonymousMethodsAreReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Process()
                                    {
                                        Func<int, int> square = {|#0:delegate|}(int x) { return x * x; };
                                        Func<int, int> cube = {|#1:delegate|}(int x) { return x * x * x; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Process()
                                     {
                                         Func<int, int> square = (int x) => x * x;
                                         Func<int, int> cube = (int x) => x * x * x;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax.", 2));
    }

    /// <summary>
    /// Verifying that a documentation comment in the anonymous method body is preserved: the rewrite keeps
    /// the block instead of inlining the return expression and discarding the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task DocumentationCommentedAnonymousMethodKeepsItsBody()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int> Run()
                                    {
                                        return {|#0:delegate|}()
                                               {
                                                   /// <summary>Result of the callback.</summary>
                                                   return 1;
                                               };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int> Run()
                                     {
                                         return () =>
                                                {
                                                    /// <summary>Result of the callback.</summary>
                                                    return 1;
                                                };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    /// <summary>
    /// Verifying that a comment between the parameter list and the block keeps the fix from being offered. The
    /// rewrite reassembles the lambda from the parameter list and the block text, and trivia in that gap belongs
    /// to neither, so offering the fix would delete it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-004")]
    public async Task CommentBetweenParameterListAndBlockIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   public class Test
                                   {
                                       public Func<int> Run()
                                       {
                                           return delegate()
                                                  /** Keep me. */
                                                  {
                                                      return 1;
                                                  };
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3004UseLambdaSyntaxAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AnonymousMethodExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment between the delegate keyword and the parameter list keeps the fix from being
    /// offered. It lies outside both spans the rewrite copies, so it would be deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-004")]
    public async Task CommentBetweenDelegateKeywordAndParameterListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   public class Test
                                   {
                                       public Func<int, int> Run()
                                       {
                                           return delegate /* keep me */ (int value) { return value; };
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3004UseLambdaSyntaxAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AnonymousMethodExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a preprocessor directive between the delegate keyword and the parameter list keeps the fix
    /// from being offered, just like a comment in the same gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-004")]
    public async Task DirectiveBetweenDelegateKeywordAndParameterListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   public class Test
                                   {
                                       public Func<int, int> Run()
                                       {
                                           return delegate
                                   #if DEBUG
                                   #endif
                                                  (int value) { return value; };
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3004UseLambdaSyntaxAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AnonymousMethodExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a preprocessor directive between the parameter list and the block keeps the fix from being
    /// offered, just like a comment in the same gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-004")]
    public async Task DirectiveBetweenParameterListAndBlockIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   public class Test
                                   {
                                       public Func<int, int> Run()
                                       {
                                           return delegate (int value)
                                   #if DEBUG
                                   #endif
                                                  { return value; };
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3004UseLambdaSyntaxAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AnonymousMethodExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies anonymous methods with a parameter list, empty or not, are reported on <c>delegate</c>, and an
    /// anonymous method without a parameter list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-001")]
    [Requirement("RH3004-002")]
    public async Task AnonymousMethodsWithParameterListAreReportedOnTheDelegateKeyword()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (int x) { return x + 1; };
                                        Func<int> b = {|RH3004:delegate|} () { return 1; };
                                        Action c = delegate { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> a = (int x) => x + 1;
                                         Func<int> b = () => 1;
                                         Action c = delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix writes an expression body for a single return statement and keeps the block for any other
    /// body, including an <c>async</c> one and one with a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task FixWritesAnExpressionBodyOnlyForASingleReturnStatement()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (int x) { return x + 1; };
                                        Action<int> b = {|RH3004:delegate|} (int x) { Console.WriteLine(x); };
                                        Func<Task> c = async {|RH3004:delegate|} () { await Task.Delay(1); };
                                        Func<int, int> d = {|RH3004:delegate|} (int x) { return x; /* c */ };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Threading.Tasks;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> a = (int x) => x + 1;
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(x);
                                                         };
                                         Func<Task> c = async () =>
                                                        {
                                                            await Task.Delay(1);
                                                        };
                                         Func<int, int> d = (int x) =>
                                                            {
                                                                return x; /* c */
                                                            };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment between the parameter list and the body keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-004")]
    public async Task CommentBetweenParameterListAndBodyIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (int x) /* c */ { return x; };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that the fix drops the <c>static</c> modifier of a static anonymous method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfStaticAnonymousMethodDropsTheStaticModifier()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = static {|RH3004:delegate|} (int x) { return x; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> a = (int x) => x;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an anonymous method that is the operand of a cast is reported without a code fix, because a lambda does
    /// not parse as the operand of a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task CastOperandIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var f = (Func<int, int>){|RH3004:delegate|} (int x) { return x + 1; };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that is the left operand of a binary operator is reported without a code fix,
    /// because the expression body of the lambda would take in the rest of the binary expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task LeftBinaryOperandIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Func<int> h)
                                    {
                                        Func<int> f = {|RH3004:delegate|} () { return 1; } + h;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method with a block body that is the right operand of a binary operator is reported
    /// without a code fix, because a lambda does not parse as the right operand of a binary operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task RightBinaryOperandIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Action g)
                                    {
                                        Action f = g + {|RH3004:delegate|} () { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that is the right operand of one binary operator and the left operand of the next
    /// is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task MiddleBinaryOperandIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Func<int> g, Func<int> h)
                                    {
                                        Func<int> f = g + {|RH3004:delegate|} () { return 1; } + h;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that is the right operand of <c>??</c> is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task RightOperandOfNullCoalescingIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Action g)
                                    {
                                        Action f = g ?? {|RH3004:delegate|} () { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method with a block body that is the left operand of a binary operator is reported
    /// without a code fix, because a lambda block body in front of an operator draws a warning
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task LeftBinaryOperandWithBlockBodyIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Action g)
                                    {
                                        Action f = {|RH3004:delegate|} () { } + g;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the operands of a cast and of binary operators from the specification are reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task CastAndBinaryOperandsAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Func<int> h, Action g)
                                    {
                                        var a = (Func<int, int>){|RH3004:delegate|} (int x) { return x + 1; };
                                        var b = {|RH3004:delegate|} () { return 1; } + h;
                                        var c = g + {|RH3004:delegate|} () { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method inside parentheses that are the operand of a cast, and one that is the value of a
    /// compound assignment, are rewritten
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-005")]
    public async Task ParenthesizedCastOperandAndCompoundAssignmentValueAreFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Func<int> g)
                                    {
                                        var a = (Func<int, int>)({|RH3004:delegate|} (int x) { return x + 1; });
                                        g += {|RH3004:delegate|} () { return 1; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(Func<int> g)
                                     {
                                         var a = (Func<int, int>)((int x) => x + 1);
                                         g += () => 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an anonymous method that is a branch of a conditional expression is rewritten, because a lambda is a
    /// valid branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-005")]
    public async Task ConditionalBranchIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b, Func<int> h)
                                    {
                                        Func<int> f = b ? {|RH3004:delegate|} () { return 1; } : h;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b, Func<int> h)
                                     {
                                         Func<int> f = b ? () => 1 : h;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves an anonymous method that is a binary operand as written and reported, and rewrites the
    /// other anonymous methods of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-005")]
    public async Task FixAllLeavesABinaryOperandReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Func<int> h)
                                    {
                                        var b = {|RH3004:delegate|} () { return 1; } + h;
                                        Func<int> c = {|RH3004:delegate|} () { return 2; };
                                        Func<int> d = {|RH3004:delegate|} () { return 3; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(Func<int> h)
                                     {
                                         var b = {|RH3004:delegate|} () { return 1; } + h;
                                         Func<int> c = () => 2;
                                         Func<int> d = () => 3;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostics whose fix is withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the fix lays out a lambda whose block stood on lines of its own, with the opening brace on the line
    /// after the parameter list, as the formatter lays it out, and writes an expression body on the line of the arrow
    /// for a single <see langword="return"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task FixLaysOutABlockOnItsOwnLinesAsTheFormatter()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                        };
                                        Func<int, int> e = {|RH3004:delegate|} (int x)
                                        {
                                            return x + 1;
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(x);
                                                         };
                                         Func<int, int> e = (int x) => x + 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix lays out a lambda whose block stood on lines of its own as the formatter lays it out in a
    /// document whose line breaks are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixLaysOutABlockOnItsOwnLinesAsTheFormatterWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
                                                                 {
                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         Console.WriteLine(x);
                                                                                     };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the fix of an anonymous method whose block stood on one line gives the formatter nothing to change
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixOfAOneLineBlockMatchesTheFormatter()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Func<Task> c = async delegate () { await Task.Delay(1); };
                                    }
                                }
                                """;

        AssertFormattingChangesNothing(await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the fix keeps the layout of the statements it moves into the lambda, also token spacing and blank
    /// lines the formatter would change, and only shifts their indentation under the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task FixKeepsTheLayoutOfTheStatementsItMoves()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            int y = x+1;
                                            if(x>1) Console.WriteLine(y);


                                            Console.WriteLine(x);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             int y = x+1;
                                                             if(x>1) Console.WriteLine(y);


                                                             Console.WriteLine(x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix keeps the layout of the statements it moves in a document whose line breaks are CRLF, and
    /// keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsTheLayoutOfTheStatementsItMovesWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
                                                                 {
                                                                     int y = x+1;
                                                                     if(x>1) Console.WriteLine(y);


                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         int y = x+1;
                                                                                         if(x>1) Console.WriteLine(y);


                                                                                         Console.WriteLine(x);
                                                                                     };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the fix moves several statements that stood on the block's one line together onto one line between
    /// the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheStatementsOfAOneLineBlockOntoOneLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> c = {|RH3004:delegate|} (int x) { int y = x+1; Console.WriteLine(y); };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> c = (int x) =>
                                                         {
                                                             int y = x+1; Console.WriteLine(y);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix leaves statements that formatter phases would rewrite as written: an expression-bodied local
    /// function, an embedded statement without braces, an unbraced switch section with several statements, an
    /// anonymous object on one line, and several blank lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-006")]
    public async Task FixLeavesStatementsTheFormatterWouldRewriteAsWritten()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            int L() => x+1;
                                            if(x>1) Console.WriteLine(L());
                                            switch (x)
                                            {
                                                case 1:
                                                    Console.WriteLine(1);
                                                    Console.WriteLine(2);
                                                    break;
                                            }
                                            var q = new { A = 1, B = 2 };



                                            Console.WriteLine(q);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             int L() => x+1;
                                                             if(x>1) Console.WriteLine(L());
                                                             switch (x)
                                                             {
                                                                 case 1:
                                                                     Console.WriteLine(1);
                                                                     Console.WriteLine(2);
                                                                     break;
                                                             }
                                                             var q = new { A = 1, B = 2 };



                                                             Console.WriteLine(q);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix moves every line of a multi-line documentation comment inside the block by the same number of
    /// columns as the statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("RH3004-008")]
    public async Task FixMovesAMultiLineDocumentationCommentWithTheStatements()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int> f = {|RH3004:delegate|} ()
                                        {
                                            /// <summary>
                                            /// Text that continues
                                            /// </summary>
                                            int L() => 1;

                                            return L();
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int> f = () =>
                                                       {
                                                           /// <summary>
                                                           /// Text that continues
                                                           /// </summary>
                                                           int L() => 1;

                                                           return L();
                                                       };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a conditional chain wholly inside the block, with its directives in column
    /// 0 and <c>DEBUG</c> defined: the anonymous method holds directives, the diagnostic is reported, and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlockWithDebug()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #else
                                            Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the specification example of a conditional chain wholly inside the block, with its directives in column
    /// 0 and no symbol defined: the anonymous method holds directives and disabled text, the diagnostic is reported,
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #else
                                            Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the conditional chain inside the block in a document whose line breaks are CRLF, with <c>DEBUG</c>
    /// defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlockWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = {|RH3004:delegate|} (int x)
                                                                 {
                                                         #if DEBUG
                                                                     Console.WriteLine(x);
                                                         #else
                                                                     Console.WriteLine(-x);
                                                         #endif
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies a <c>#region</c> in column 0 and an indented conditional chain inside the block, with <c>DEBUG</c>
    /// defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionDirectivesAndIndentedDirectivesInsideTheBlockWithDebug()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #region Output
                                            #if DEBUG
                                            Console.WriteLine(x);
                                            #endif
                                #endregion
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies a <c>#region</c> in column 0 and an indented conditional chain inside the block whose only statement is
    /// disabled text: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionDirectivesAndIndentedDirectivesInsideTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #region Output
                                            #if DEBUG
                                            Console.WriteLine(x);
                                            #endif
                                #endregion
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block whose first statement is disabled text in front of an active statement in another column: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForADisabledStatementInFrontOfTheFirstActiveStatement()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #endif
                                        Console.WriteLine(-x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a <c>#nullable</c> directive in column 0 inside the block: the diagnostic is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForANullableDirectiveInsideTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string x)
                                        {
                                #nullable enable
                                            string? y = x;
                                #nullable restore
                                            Console.WriteLine(y);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment line in front of the block's first statement in another column does not decide the number
    /// of columns: the statement stands one level inside the braces, and the comment line moves by the same number
    /// of columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOnTheFirstStatementAndNotOnACommentLineInFrontOfIt()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                    // Writes the value
                                            Console.WriteLine(x);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                     // Writes the value
                                                             Console.WriteLine(x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the lines of a block without a statement move so that the block's first line stands one level inside
    /// the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOfABlockWithoutAStatementOnItsFirstLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            // Nothing to do
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             // Nothing to do
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies one Fix All application rewrites an anonymous method nested in the block of another anonymous method:
    /// the outer fix moves the inner one with its block, and the same application rewrites it where it now stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllRewritesAnAnonymousMethodNestedInTheBlockOfAnotherInTheSameApplication()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (int x)
                                        {
                                            Action<int> b = {|RH3004:delegate|} (int y)
                                            {
                                                Console.WriteLine(y);
                                            };
                                            b(x);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> a = (int x) =>
                                                         {
                                                             Action<int> b = (int y) =>
                                                                             {
                                                                                 Console.WriteLine(y);
                                                                             };
                                                             b(x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // One Fix All application corrects both diagnostics
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies the specification example of a block that holds a multi-line raw string literal: the anonymous method
    /// is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForABlockHoldingAMultiLineRawStringLiteral()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine("""
                                                a
                                                """);
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a block that holds a multi-line raw string literal with a line of only
    /// whitespace that is wider than the indentation of the closing quotes, whose extra whitespace is part of the
    /// literal's value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForABlockHoldingAMultiLineRawStringLiteralWithAWhitespaceOnlyLine()
    {
        var testCode = ExpandWhitespaceMarkers(""""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           var s = """
                                                               a
                                               ······················
                                                               b
                                                               """;
                                                           Console.WriteLine(s);
                                                       };
                                                   }
                                               }
                                               """");

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a block that holds a multi-line interpolated raw string literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForABlockHoldingAMultiLineInterpolatedRawStringLiteral()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine($"""
                                                {x}
                                                """);
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a block that holds a multi-line UTF-8 raw string literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForABlockHoldingAMultiLineUtf8RawStringLiteral()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            ReadOnlySpan<byte> s = """
                                                a
                                                """u8;
                                            Console.WriteLine(s.Length + x);
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block whose single return statement returns a multi-line raw string
    /// literal: the fix writes an expression body, the literal's lines move under its opening quotes, and its value
    /// stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    public async Task FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> f = {|RH3004:delegate|} ()
                                        {
                                            return """
                                                a
                                                  b
                                                """;
                                        };
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string> f = () => """
                                                                a
                                                                  b
                                                                """;
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that holds a raw string literal on one line: the fix is offered
    /// and lays out the lambda
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("RH3004-007")]
    public async Task FixRewritesABlockHoldingARawStringLiteralOnOneLine()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x) { Console.WriteLine("""a"""); };
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine("""a""");
                                                         };
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block holding a verbatim string over several lines: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForABlockHoldingAMultiLineVerbatimString()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(@"a
                                b");
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string whose continuation line starts with a hole: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLineStartsWithAHole()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello
                                {name}
                                done";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string whose continuation line starts with a hole, in a
    /// document whose line breaks are CRLF: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLineStartsWithAHoleWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<string> b = {|RH3004:delegate|} (string name)
                                                                 {
                                                                     var s = $@"Hello
                                                         {name}
                                                         done";
                                                                     Console.WriteLine(s);
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string whose closing quote starts a line: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringWhoseClosingQuoteStartsALine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello {name}
                                ";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block whose first statement shares the line of the opening brace and holds an interpolated verbatim
    /// string over several lines: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringBehindASharedOpeningBraceLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name) { var s = $@"Hello
                                {name}"; Console.WriteLine(s); };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding disabled text with an interpolated verbatim string over several lines: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringInDisabledText()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                #if NEVER
                                            var s = $@"Hello
                                {name}
                                ";
                                #endif
                                            Console.WriteLine(name);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a verbatim string whose closing quote starts a line: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAVerbatimStringWhoseClosingQuoteStartsALine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = @"Hello
                                ";
                                            Console.WriteLine(s + name);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string whose continuation lines start with text: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLinesStartWithText()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello
                                dear {name}
                                done";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block that only returns an interpolated verbatim string whose continuation line starts with a hole
    /// in column 0 is rewritten as an expression body, and the continuation line keeps its columns, so the string's
    /// value stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    public async Task FixRewritesASingleReturnOfAnInterpolatedVerbatimStringAsAnExpressionBodyAndKeepsItsValue()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string, string> b = {|RH3004:delegate|} (string name)
                                        {
                                            return $@"Hello
                                {name}";
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string, string> b = (string name) => $@"Hello
                                 {name}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the former specification example of interpolated verbatim strings whose lines start with a hole or a
    /// closing quote: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForInterpolatedVerbatimStringsWithHolesAndClosingQuotesThatStartLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello
                                {name}
                                ";
                                            var t = $@"Hello {
                                                name} done";
                                            Console.WriteLine(s + t);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string with a hole that spans lines: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAHoleThatSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var t = $@"Hello {
                                                name} done";
                                            Console.WriteLine(t);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the former specification example of a verbatim string that starts a line and continues on the next: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAVerbatimStringThatStartsALine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(
                                                @"a
                                b");
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the former specification example of a string over several lines behind a shared opening brace line: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringThatSpansLinesBehindASharedOpeningBraceLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> c = {|RH3004:delegate|} (string name) { var s = $@"Hello
                                {name}";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding disabled text whose string over several lines closes before the directive that ends the
    /// disabled text: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAStringOfDisabledTextThatClosesBeforeTheEndingDirective()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            Console.WriteLine(name);
                                #if DEBUG
                                            Console.WriteLine($@"Hello
                                {name}");
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a conditional chain whose active branch, with <c>DEBUG</c> defined, holds an
    /// interpolated verbatim string over several lines, in a document whose line breaks are CRLF: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAMultiLineStringOfAnActiveBranchWithDebugAndCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<string> b = {|RH3004:delegate|} (string name)
                                                                 {
                                                                     Console.WriteLine(name);
                                                         #if DEBUG
                                                                     Console.WriteLine($@"Hello
                                                         {name}");
                                                         #endif
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string that starts in disabled text and is not closed before
    /// the directive that ends that disabled text: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAStringThatDisabledTextLeavesOpen()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            Console.WriteLine(name);
                                #if A
                                            Console.WriteLine(@"Hello
                                #elif B
                                            world");
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string that the disabled text of the <c>#elif</c> branch would
    /// leave open, with <c>A</c> defined so the string is active code and spans the directive: the diagnostic is
    /// reported and no code fix is offered, as without the symbol
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAStringThatDisabledTextLeavesOpenWithTheFirstBranchActive()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            Console.WriteLine(name);
                                #if A
                                            Console.WriteLine(@"Hello
                                #elif B
                                            world");
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "A");
    }

    /// <summary>
    /// Verifies the specification example of a conditional chain wholly outside the anonymous method whose
    /// <c>#else</c> line lies inside a verbatim string of the block with <c>A</c> defined: the block holds no directive,
    /// the string holds a line that starts with <c>#</c>, the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringOfAnActiveBranchOpenedBeforeTheAnonymousMethodThatHoldsADirectiveLine()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            var s = @"x
                                #else
                                        Action a = () => { var s = @"
                                            ; // ";
                                            Console.WriteLine(s); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string holding a line that starts with <c>#</c> in a document
    /// without any conditional directive: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringThatHoldsALineStartingWithHashWithoutAnyDirective()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(@"a
                                #b");
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a verbatim string holding a line that starts with <c>#</c> in a document whose line breaks are CRLF:
    /// the diagnostic is reported and no code fix is offered, as with LF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringThatHoldsALineStartingWithHashWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action b = {|RH3004:delegate|} ()
                                                                 {
                                                                     Console.WriteLine(@"a
                                                         #b");
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a verbatim string holding a line whose first character other than whitespace is <c>#</c>, behind
    /// leading spaces: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringThatHoldsALineStartingWithHashBehindWhitespace()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(@"a
                                    #b");
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string over several lines whose second line holds <c>#</c>
    /// behind other text: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringWhoseLineHoldsHashBehindOtherText()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(
                                                @"a
                                b #c");
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a verbatim string over several lines whose second line holds <c>#</c> behind other text, in a document
    /// whose line breaks are CRLF: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringWhoseLineHoldsHashBehindOtherTextWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action b = {|RH3004:delegate|} ()
                                                                 {
                                                                     Console.WriteLine(
                                                                         @"a
                                                         b #c");
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that only returns a verbatim string over several lines whose
    /// later line starts with <c>#</c>, with no directive in the document: the fix would write an expression body,
    /// but the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAStringThatHoldsALineStartingWithHash()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> b = {|RH3004:delegate|} ()
                                        {
                                            return @"a
                                #b";
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines whose later line starts with
    /// <c>#</c>, in a document whose line breaks are CRLF: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAStringThatHoldsALineStartingWithHashWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Func<string> b = {|RH3004:delegate|} ()
                                                                 {
                                                                     return @"a
                                                         #b";
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines whose later line starts with a
    /// form feed in front of <c>#</c>: the form feed is whitespace to the lexer, so the line is a <c>#</c> line, and
    /// the diagnostic is reported with no code fix offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAStringWhoseHashLineStartsWithAFormFeed()
    {
        var template = """
                       using System;

                       class C
                       {
                           void M()
                           {
                               Func<string> b = {|RH3004:delegate|} ()
                               {
                                   return @"a
                       <form-feed>#b";
                               };
                           }
                       }
                       """;

        var testCode = template.Replace("<form-feed>", "\f", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines whose later line starts with spaces
    /// and a tab in front of <c>#</c>: the line is a <c>#</c> line, and the diagnostic is reported with no code fix
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAStringWhoseHashLineStartsWithSpacesAndATab()
    {
        var template = """
                       using System;

                       class C
                       {
                           void M()
                           {
                               Func<string> b = {|RH3004:delegate|} ()
                               {
                                   return @"a
                         <tab>#b";
                               };
                           }
                       }
                       """;

        var testCode = template.Replace("<tab>", "\t", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that only returns a verbatim string over several lines without a
    /// <c>#</c> line: the fix writes an expression body, and the later line keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBody()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> b = {|RH3004:delegate|} ()
                                        {
                                            return @"a
                                b";
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string> b = () => @"a
                                 b";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines without a <c>#</c> line, in a
    /// document whose line breaks are CRLF: the fix writes an expression body, and the later line keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBodyWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Func<string> b = {|RH3004:delegate|} ()
                                                                 {
                                                                     return @"a
                                                         b";
                                                                 };
                                                             }
                                                         }
                                                         """);

        var fixedCode = NormalizeToCarriageReturnLineFeed("""
                                                          using System;

                                                          class C
                                                          {
                                                              void M()
                                                              {
                                                                  Func<string> b = () => @"a
                                                          b";
                                                              }
                                                          }
                                                          """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines whose later line holds <c>#</c> only
    /// behind other text: that line is no <c>#</c> line, so the fix writes an expression body, and the later line
    /// keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAStringWhoseLineHoldsHashBehindOtherTextAsAnExpressionBody()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> b = {|RH3004:delegate|} ()
                                        {
                                            return @"a
                                b #c";
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string> b = () => @"a
                                 b #c";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a block holding the <c>#else</c> and <c>#endif</c> of a chain whose
    /// <c>#if</c> stands before <c>delegate</c>, with <c>A</c> defined: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedBeforeTheAnonymousMethodThatEndsInsideItsBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #else
                                            () =>
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "A");
    }

    /// <summary>
    /// Verifies the specification example of a block holding only the <c>#endif</c> of a chain whose <c>#if</c> and
    /// <c>#else</c> stand before <c>delegate</c>, with no symbol defined: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForABlockThatHoldsOnlyTheEndifOfAChainOpenedBeforeTheAnonymousMethod()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            () =>
                                            {
                                                Console.WriteLine(1);
                                #else
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block holding a verbatim string with a line that starts with
    /// <c>#</c> and the end of a chain opened before <c>delegate</c>, with <c>A</c> defined: the diagnostic is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedBeforeTheAnonymousMethodWhoseElseLiesInAStringOfTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                var s = @"a
                                #else
                                            () =>
                                            {
                                                var s = @"
                                            ; // ";
                                #endif
                                                Console.WriteLine(s);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding the <c>#if</c> of a chain whose <c>#endif</c> stands after the block's closing brace,
    /// without any symbol defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBrace()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(1);
                                #if A
                                        };
                                #else
                                        };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding the <c>#if</c> of a chain whose <c>#else</c> and <c>#endif</c> stand after the block's
    /// closing brace, with <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBraceWithTheFirstBranchActive()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(1);
                                #if A
                                        };
                                #else
                                        };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "A");
    }

    /// <summary>
    /// Verifies the specification example of a conditional chain wholly outside the anonymous method, with <c>A</c>
    /// defined: the block holds no directive and no string, the fix is offered, and the lambda is laid out as the
    /// formatter lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixRewritesAnAnonymousMethodInsideAChainThatLiesWhollyOutsideIt()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} () { Console.WriteLine(1); };
                                #else
                                        Action a = () => Console.WriteLine(2);
                                #endif
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #define A
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                 #if A
                                         Action a = () =>
                                                    {
                                                        Console.WriteLine(1);
                                                    };
                                 #else
                                         Action a = () => Console.WriteLine(2);
                                 #endif
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and later lines that
    /// start left of it: the later lines move together until the leftmost of them stands in the statement column, and
    /// keep their indentation relative to each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action<int> a)
                                    {
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Run({|RH3004:delegate|} (int x) { Foo(x);
                                            if (x > 0)
                                                Foo(x + 1);
                                        });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action<int> a)
                                     {
                                     }

                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Run((int x) =>
                                             {
                                                 Foo(x);
                                                 if (x > 0)
                                                     Foo(x + 1);
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and later lines that
    /// start left of it in a document whose line breaks are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumnWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void Run(Action<int> a)
                                                             {
                                                             }

                                                             void Foo(int x)
                                                             {
                                                             }

                                                             void M()
                                                             {
                                                                 Run(delegate (int x) { Foo(x);
                                                                     if (x > 0)
                                                                         Foo(x + 1);
                                                                 });
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void Run(Action<int> a)
                                                                 {
                                                                 }

                                                                 void Foo(int x)
                                                                 {
                                                                 }

                                                                 void M()
                                                                 {
                                                                     Run((int x) =>
                                                                         {
                                                                             Foo(x);
                                                                             if (x > 0)
                                                                                 Foo(x + 1);
                                                                         });
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and a later line that
    /// does not start left of it: the later line keeps its indentation relative to the first statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsTheLinesAfterASharedOpeningBraceLineRelativeToTheFirstStatement()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Foo(int x, int y)
                                    {
                                    }

                                    void M()
                                    {
                                        Action<int> c = {|RH3004:delegate|} (int x) { Foo(x,
                                                                               x + 1);
                                                                         };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Foo(int x, int y)
                                     {
                                     }

                                     void M()
                                     {
                                         Action<int> c = (int x) =>
                                                         {
                                                             Foo(x,
                                                                 x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a first statement on the line of the opening brace and a later statement that starts left of it, with
    /// the closing brace behind it: the later statement stands in the statement column, not left of the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesALaterStatementInFrontOfAClosingBraceOnItsLineIntoTheStatementColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Action<int> c = {|RH3004:delegate|} (int x) { Foo(x);
                                            Foo(x + 1); };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Action<int> c = (int x) =>
                                                         {
                                                             Foo(x);
                                                             Foo(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment line in column 0 after an opening brace that shares its line with the first statement stays
    /// in column 0 and does not count for the leftmost line: the later statement stands in the statement column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsAColumnZeroCommentLineAfterASharedOpeningBraceLineInColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action<int> a)
                                    {
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Run({|RH3004:delegate|} (int x) { Foo(x);
                                // Foo(x + 2);
                                            Foo(x + 1);
                                        });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action<int> a)
                                     {
                                     }

                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Run((int x) =>
                                             {
                                                 Foo(x);
                                 // Foo(x + 2);
                                                 Foo(x + 1);
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 0 in a block whose shift moves its lines to the
    /// left: the comment line stays in column 0, and the statement moves into the statement column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsAColumnZeroCommentLineInColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                 Foo();
                                 // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 8 in a block whose shift moves its lines to the
    /// left: no line would stand left of column 0, so every line moves by the measured shift, also the comment left of
    /// the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesACommentLineInAnotherColumnByTheMeasuredShift()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                        // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                 Foo();
                                     // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 2 in a block whose shift moves its lines to the
    /// left: the shift would put the comment left of column 0, so the lines move together until the comment stands in
    /// the statement column, and the statement keeps its offset to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesOfABlockSoTheLeftmostStandsInTheStatementColumnWhenTheShiftWouldCrossColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                  // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                                   Foo();
                                                 // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a conditional chain in column 0 inside a block whose lines the fix would move left, with <c>DEBUG</c>
    /// defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAColumnZeroConditionalChainInABlockWhoseLinesWouldMoveLeft()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                #if DEBUG
                                                    Foo();
                                #endif
                                                });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies <c>#region</c> and <c>#endregion</c> lines inside a block whose shift would cross column 0: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionDirectivesInABlockWhoseShiftWouldCrossColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                  #region Calls
                                                    Foo();
                                  #endregion
                                                });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block whose first statement stands in a conditional branch, active with <c>DEBUG</c> defined, in
    /// front of a statement in another column: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAConditionalStatementInFrontOfAStatementInAnotherColumnWithDebug()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #endif
                                                Console.WriteLine(-x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies a block whose first statement is disabled text in front of a statement in another column: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForADisabledStatementInFrontOfAStatementInAnotherColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #endif
                                                Console.WriteLine(-x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement line that starts with a comment in column 0: the line
    /// holds code, so it moves, the shift is measured from its first character, the comment, and the later statement
    /// keeps its offset to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentAndMeasuresTheShiftFromTheComment()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                /* note */ Console.WriteLine(x);
                                            Console.WriteLine(x + 1);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             /* note */ Console.WriteLine(x);
                                                                         Console.WriteLine(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement line that starts with a comment in column 0 in a
    /// document whose line breaks are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
                                                                 {
                                                         /* note */ Console.WriteLine(x);
                                                                     Console.WriteLine(x + 1);
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         /* note */ Console.WriteLine(x);
                                                                                                     Console.WriteLine(x + 1);
                                                                                     };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines that starts in column 0: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatStartsInColumnZeroAndSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                /* Console.WriteLine(x + 1);
                                   Console.WriteLine(x + 2); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines that starts in another column: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatStartsInAnotherColumnAndSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                            /* Console.WriteLine(x + 3);
                                               Console.WriteLine(x + 4); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the former specification example of two delimited comments over several lines, one starting in column 0
    /// and one in another column: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForTwoDelimitedCommentsThatSpanLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                /* Console.WriteLine(x + 1);
                                   Console.WriteLine(x + 2); */
                                            /* Console.WriteLine(x + 3);
                                               Console.WriteLine(x + 4); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment over several lines that, with <c>A</c> defined, holds
    /// the <c>#else</c> of a chain standing wholly outside the anonymous method: the diagnostic is reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            /* x
                                #else
                                        Action a = () => { var s = @"
                                            */ ";//".ToString();
                                            Console.WriteLine(1); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited documentation comment over several lines that, with <c>A</c> defined, holds the
    /// <c>#else</c> of a chain standing wholly outside the anonymous method: the diagnostic is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAMultiLineDocumentationCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            /** x
                                #else
                                        Action a = () => { var s = @"
                                            */ ";//".ToString();
                                            Console.WriteLine(1); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines that, with <c>A</c> defined, holds the <c>#else</c> of a chain
    /// inside the block: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainInsideTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                #if A
                                            /* x
                                #else
                                            var s = @"
                                            */ ";//".ToString();
                                #endif
                                            Console.WriteLine(1);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string over several lines whose <c>#else</c> line starts with a
    /// form feed, which is a directive line without <c>A</c>: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringWhoseDirectiveLineStartsWithAFormFeed()
    {
        var template = """
                       #define A
                       using System;

                       class C
                       {
                           void M()
                           {
                       #if A
                               Action a = {|RH3004:delegate|} ()
                               {
                                   var s = @"x
                       <form-feed>#else
                               Action a = () => { var s = @"
                                   ; // ";
                                   Console.WriteLine(s); };
                       #endif
                           }
                       }
                       """;

        var testCode = template.Replace("<form-feed>", "\f", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of blocks holding a verbatim string, a delimited comment, an interpolated
    /// verbatim string, and a raw string literal over several lines, with no directive in the document: each diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForStringsAndCommentsThatSpanLinesWithoutAnyDirective()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(
                                                @"a
                                b #c");
                                            /* first
                                               second */
                                        };
                                        Action<string> c = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello
                                {name}";
                                            Console.WriteLine(s);
                                        };
                                        Action<int> d = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine("""
                                                a
                                                """);
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that returns a verbatim string whose <c>#else</c> line ends the
    /// chain's first branch, with <c>A</c> defined: the fix would write an expression body and delete text that is
    /// the inside of a string without <c>A</c>, so the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAReturnedStringThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Func<string> f = {|RH3004:delegate|} () { return @"
                                #else
                                        Func<string> f = () =>
                                "; }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment in the parameter list whose <c>#else</c> line ends
    /// the chain's first branch, on the block-body path, with <c>A</c> defined: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheBlockBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action<int> a = {|RH3004:delegate|} (int x /*
                                #else
                                        var s = @"
                                        */)
                                        {
                                            Console.WriteLine(x);
                                        }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment in the parameter list whose <c>#else</c> line ends
    /// the chain's first branch, on the expression-body path, with <c>A</c> defined: the diagnostic is reported and
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheExpressionBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Func<int, int> a = {|RH3004:delegate|} (int x /*
                                #else
                                        var s = @"
                                        */) { return x; }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that only returns a raw string literal over several lines
    /// whose later line starts with <c>#</c> behind spaces, with no directive in the document: the fix would write an
    /// expression body, but the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAMultiLineRawStringLiteralThatHoldsALineStartingWithHash()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> c = {|RH3004:delegate|} ()
                                        {
                                            return """
                                                a
                                                #b
                                                """;
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment over several lines without a <c>#</c> line in the
    /// parameter list, on the block-body path, with no directive in the document: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> e = {|RH3004:delegate|} (int x /* first
                                                                                     second */)
                                        {
                                            Console.WriteLine(x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines without a <c>#</c> line in the parameter list, on the
    /// block-body path, in a document whose line breaks are CRLF: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPathWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> e = {|RH3004:delegate|} (int x /* first
                                                                                                              second */)
                                                                 {
                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines in the parameter list whose later line starts with <c>#</c>,
    /// on the expression-body path, with no directive in the document: the diagnostic is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (int x /* first
                                #second */)
                                        {
                                            return x;
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines in a document whose line breaks are CRLF: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatSpansLinesWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action b = {|RH3004:delegate|} ()
                                                                 {
                                                                     Console.WriteLine(1);
                                                                     /* first
                                                                        second */
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block whose strings and comments each stand on one line: the fix is
    /// offered and moves the lines as the layout requirement states
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            // Greets the user.
                                            Console.WriteLine("Hello " + name); /* done */
                                            Console.WriteLine($"{name}!");
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<string> b = (string name) =>
                                                            {
                                                                // Greets the user.
                                                                Console.WriteLine("Hello " + name); /* done */
                                                                Console.WriteLine($"{name}!");
                                                            };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block whose strings and comments each stand on one line in a document whose line breaks are CRLF: the
    /// fix is offered and keeps the line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLineWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<string> b = delegate (string name)
                                                                 {
                                                                     // Greets the user.
                                                                     Console.WriteLine("Hello " + name); /* done */
                                                                     Console.WriteLine($"{name}!");
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<string> b = (string name) =>
                                                                                        {
                                                                                            // Greets the user.
                                                                                            Console.WriteLine("Hello " + name); /* done */
                                                                                            Console.WriteLine($"{name}!");
                                                                                        };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the specification example of a chain that starts in the parameter list and ends in the block, with
    /// <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListIntoTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                #else
                                            int x) { Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends in the block, with no symbol defined: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListIntoTheBlockWithoutTheSymbol()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                #else
                                            int x) { Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends after the anonymous method, with <c>A</c> defined:
    /// the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListPastTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                        };
                                #else
                                            int x) { Console.WriteLine(-x); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends after an anonymous method whose block only returns a
    /// value, with <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListPastTheAnonymousMethodOnTheExpressionBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            return x;
                                        };
                                #else
                                            int x) { return -x; };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a chain wholly inside the parameter list of an anonymous method whose
    /// block only returns a value: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainInsideTheParameterListOnTheExpressionBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = {|RH3004:delegate|} (
                                #if A
                                            int x
                                #else
                                            int x
                                #endif
                                            ) { return x + 1; };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a <c>#region</c> before <c>delegate</c> whose <c>#endregion</c> stands in the block: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForARegionBeforeTheAnonymousMethodThatEndsInsideItsBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #region R
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #endregion
                                                Console.WriteLine(2);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a chain and a <c>#region</c> that start before <c>delegate</c> and end in
    /// the block, with <c>A</c> defined: each diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainAndARegionThatCrossTheDelegateKeyword()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #else
                                            () =>
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                        Action b =
                                #region Init
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #endregion
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a <c>#region</c> in the block whose <c>#endregion</c> stands after the anonymous method: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForARegionInsideTheBlockThatEndsAfterTheAnonymousMethod()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                                   {
                                                       Console.WriteLine(1);
                                #region R
                                                       Console.WriteLine(2);
                                                   };
                                #endregion
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of <c>#region</c> and <c>#pragma</c> directives inside the block: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionAndPragmaDirectivesInsideTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #region Output
                                #pragma warning disable CS0168
                                            Console.WriteLine(x);
                                #pragma warning restore CS0168
                                #endregion
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method inside a region whose <c>#region</c> and <c>#endregion</c> stand wholly outside it:
    /// the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixRewritesAnAnonymousMethodInsideARegionThatLiesWhollyOutsideIt()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #region Init
                                        Action a = {|RH3004:delegate|} () { Console.WriteLine(1); };
                                #endregion
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                 #region Init
                                         Action a = () =>
                                                    {
                                                        Console.WriteLine(1);
                                                    };
                                 #endregion
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a refused comment in the block of the anonymous method withholds the fix: the fix formats the
    /// lambda it builds, which keeps the block and so the comment, and the formatter refuses it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedWhenTheAnonymousMethodBodyHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              using System;

                              public class Test
                              {
                                  public Func<int, int> Run()
                                  {
                                      return {|RH3004:delegate|}(int x)
                                             {
                                                 int /* c */ y = x;

                                                 return y;
                                             };
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Defines the preprocessor symbol <c>DEBUG</c> for the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution DefineDebug(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("DEBUG"))
                   : solution;
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int, Func<int, int>> GetCurried()
                                    {
                                        return {|#0:delegate|}(int x)
                                        {
                                            return {|#1:delegate|}(int y) { return x + y; };
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int, Func<int, int>> GetCurried()
                                     {
                                         return (int x) => (int y) => x + y;
                                     }
                                 }
                                 """;

        // Verifying anonymous methods nested in expressions are reported and fixed
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax.", 2),
                                  config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}