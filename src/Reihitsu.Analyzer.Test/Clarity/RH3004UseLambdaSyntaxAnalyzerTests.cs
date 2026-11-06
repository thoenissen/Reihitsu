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
    /// Verifies the specification example of conditional directive lines in column 0 inside the moved block: the
    /// directive lines stay in column 0, and the line of the first statement, not the directive in front of it,
    /// decides the number of columns the statements move
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsConditionalDirectivesInColumnZeroAndShiftsTheStatements()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
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

        await Verify(testCode, fixedCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the conditional directive example without the symbol defined, so the first statement of the block is
    /// the one in the <c>#else</c> branch: the directive lines stay in column 0, and the statements move by the same
    /// number of columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsConditionalDirectivesInColumnZeroWhenTheFirstBranchIsDisabled()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
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

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the conditional directive example in a document whose line breaks are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsConditionalDirectivesInColumnZeroWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
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

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
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

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode, "DEBUG"));
    }

    /// <summary>
    /// Verifies the specification example of a <c>#region</c> and <c>#endregion</c> line in column 0 and an
    /// indented conditional directive inside the moved block: all of them move by the same number of columns as the
    /// statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesRegionDirectivesAndIndentedDirectivesWithTheStatements()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
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

        await Verify(testCode, fixedCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the <c>#region</c> example without the symbol defined, so the block's only statement is disabled text:
    /// the statement in disabled text still decides the number of columns, and the output is the same as with the
    /// symbol defined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesRegionDirectivesAndIndentedDirectivesWithTheStatementsWhenTheStatementIsDisabled()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
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

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a statement in an inactive <c>#if</c> branch in front of an active statement in another column
    /// decides the number of columns: the disabled statement's line stands one level inside the braces, and the
    /// active statement moves by the same number of columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOnADisabledStatementInFrontOfTheFirstActiveStatement()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                 #if DEBUG
                                                             Console.WriteLine(x);
                                 #endif
                                                         Console.WriteLine(-x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a <c>#nullable</c> directive line in column 0 inside the moved block stays in column 0, and the
    /// first statement's line decides the number of columns the other lines move
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsANullableDirectiveInColumnZero()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<string> b = (string x) =>
                                                            {
                                 #nullable enable
                                                                string? y = x;
                                 #nullable restore
                                                                Console.WriteLine(y);
                                                            };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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
    /// Verifies no code fix is offered for a block whose single return statement returns a multi-line raw string
    /// literal, which the fix would otherwise write as an expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    public async Task FixWithheldForASingleReturnOfAMultiLineRawStringLiteral()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, string> b = {|RH3004:delegate|} (int x)
                                        {
                                            return """
                                                a
                                                """;
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
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
    /// Verifies a block that holds a verbatim string literal over several lines is fixed, and the literal's
    /// continuation line keeps its columns, so the literal's value stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("RH3004-007")]
    public async Task FixKeepsTheContinuationLineOfAMultiLineVerbatimStringInItsColumns()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(@"a
                                 b");
                                                         };
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
    /// Verifies a conditional directive line in column 0 in a block whose shift moves its lines to the left stays in
    /// column 0 and does not count for the leftmost line: the statement moves into the statement column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsAColumnZeroDirectiveLineInColumnZeroWhenTheShiftMovesTheLinesLeft()
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
                                 #if DEBUG
                                                 Foo();
                                 #endif
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies a directive line in column 2 in front of the first statement counts for the leftmost line when the
    /// shift would put it left of column 0: the lines move together until the directive stands in the statement column,
    /// and the statement keeps its offset to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesADirectiveLineInAnotherColumnIntoTheStatementColumnWhenTheShiftWouldCrossColumnZero()
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
                                                 #region Calls
                                                                   Foo();
                                                 #endregion
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a statement in an #if branch in front of an active statement in another
    /// column with the symbol defined: the output is the same as without the symbol
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOnAnActiveStatementInFrontOfAStatementInAnotherColumn()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                 #if DEBUG
                                                             Console.WriteLine(x);
                                 #endif
                                                                 Console.WriteLine(-x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the specification example of a statement in an inactive #if branch in front of an active statement in
    /// another column without the symbol defined: the disabled statement decides the shift
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOnADisabledStatementInFrontOfAStatementInAnotherColumn()
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

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                 #if DEBUG
                                                             Console.WriteLine(x);
                                 #endif
                                                                 Console.WriteLine(-x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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