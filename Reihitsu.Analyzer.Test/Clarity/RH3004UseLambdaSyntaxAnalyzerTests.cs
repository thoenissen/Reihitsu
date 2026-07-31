using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3004UseLambdaSyntaxAnalyzer"/> and <see cref="RH3004UseLambdaSyntaxCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3004UseLambdaSyntaxAnalyzerTests : AnalyzerTestsBase<RH3004UseLambdaSyntaxAnalyzer, RH3004UseLambdaSyntaxCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying anonymous methods with parameters are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
                                         return (string message) => {
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
                                         Func<int, int> square = (int x) => { return x * x; /* keep me */ };
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
    /// Verifying anonymous methods nested in expressions are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedAnonymousMethodIsReportedAndFixed()
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

        await Verify(testCode, fixedCode, onConfigure: config => config.NumberOfFixAllIterations = 2, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax.", 2));
    }

    /// <summary>
    /// Verifying that a documentation comment in the anonymous method body is preserved: the rewrite keeps
    /// the block instead of inlining the return expression and discarding the comment (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
                                         return () => {
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
    /// to neither, so offering the fix would delete it (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// offered. It lies outside both spans the rewrite copies, so it would be deleted (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}