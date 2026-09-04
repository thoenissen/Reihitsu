using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5101FirstArgumentShouldBeOnSameLineAnalyzer"/> and <see cref="RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests : BatchCodeFixTestsBase<RH5101FirstArgumentShouldBeOnSameLineAnalyzer, RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that valid argument placements produce no diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForValidCode()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        // All on one line
                                        Console.WriteLine("test1", "test2", "test3");

                                        // First arg on same line, rest on subsequent lines
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");

                                        // Single argument
                                        Console.WriteLine("test1");

                                        // No arguments
                                        Console.WriteLine();

                                        // Constructor call
                                        var obj = new System.Text.StringBuilder("initial",
                                                                                16);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that first argument on a new line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFirstArgumentOnNewLineIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine(
                                            {|#0:"test1"|},
                                            "test2");
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Console.WriteLine("test1",
                                                            "test2");
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that first argument on a new line in a constructor call is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFirstArgumentOnNewLineInConstructorCallIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Text;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var sb = new StringBuilder(
                                            {|#0:"initial"|},
                                            16);
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Text;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          var sb = new StringBuilder("initial",
                                                                     16);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that an argument list carrying a comment in the join gap is not flagged, because the formatter
    /// refuses to collapse the first argument across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedArgumentListIsNotFlagged()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine( // note
                                            "test1",
                                            "test2");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that no code fix action is registered for an argument list carrying a comment in the join gap,
    /// so the code fix does not offer a no-op action (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedArgumentListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   internal class TestClass
                                   {
                                       void Method()
                                       {
                                           Console.WriteLine( // note
                                               "test1",
                                               "test2");
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that an argument list carrying a preprocessor directive in the join gap is not flagged, because the
    /// formatter refuses to collapse the first argument across that directive (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveInArgumentGapIsNotFlagged()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine(
                                #if FEATURE
                                #endif
                                            "test1",
                                            "test2");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a documentation comment in the join gap is gated like other comments, so the gate and the
    /// formatter agree on what counts as a comment (issue #226)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentedArgumentListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   internal class TestClass
                                   {
                                       void Method()
                                       {
                                           Console.WriteLine(
                                               /// note
                                               "test1",
                                               "test2");
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that the argument list of the issue's literal example, which carries a comment after the closing
    /// parenthesis, is reported (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y) => x + y;

                                    private static int Use()
                                    {
                                        return Add(
                                            {|#0:1|},
                                            2) /* note */ + 3;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that the argument list of the issue's literal example, which carries a comment after the closing
    /// parenthesis, is offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y) => x + y;

                                       private static int Use()
                                       {
                                           return Add(
                                               1,
                                               2) /* note */ + 3;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifying the control case of the issue's literal example: the identical input without the trailing comment is
    /// reported (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWithoutTrailingCommentIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y) => x + y;

                                    private static int Use()
                                    {
                                        return Add(
                                            {|#0:1|},
                                            2) + 3;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying the control case of the issue's literal example: the identical input without the trailing comment is
    /// offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWithoutTrailingCommentIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y) => x + y;

                                       private static int Use()
                                       {
                                           return Add(
                                               1,
                                               2) + 3;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment after the closing parenthesis is fixed and the comment stays where it was written
    /// (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y) => x + y;

                                    private static int Use()
                                    {
                                        return Add(
                                            {|#0:1|},
                                            2) /* note */ + 3;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      private static int Add(int x, int y) => x + y;

                                      private static int Use()
                                      {
                                          return Add(1,
                                                     2) /* note */ + 3;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment written before the opening parenthesis still withholds the code fix. The rewrite is
    /// delegated to the shared formatter, which restores the first token's leading trivia only when that token does
    /// not start a line, so this region is not released together with the trailing one (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeTheOpeningParenthesisWithholdsTheCodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y) => x + y;

                                       private static int Use()
                                       {
                                           return Add
                                               /* note */ (
                                               1,
                                               2) + 3;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment written directly against the closing parenthesis, without a separating space, is
    /// offered a code fix like the space-separated shape (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAdjacentToTheClosingParenthesisIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y) => x + y;

                                       private static int Use()
                                       {
                                           return Add(
                                               1,
                                               2)/* note */ + 3;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    private static string Outer(string a, string b) => a + b;

                                    private static string Inner(string a, string b) => a + b;

                                    private static string Use()
                                    {
                                        return Outer(
                                            {|#0:Inner(
                                                {|#1:"a"|},
                                                "b")|},
                                            "c");
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     private static string Outer(string a, string b) => a + b;

                                     private static string Inner(string a, string b) => a + b;

                                     private static string Use()
                                     {
                                         return Outer(Inner("a",
                                                            "b"),
                                                      "c");
                                     }
                                 }
                                 """;

        // The outer diagnostic's fix reformats the whole outer argument list, whose span fully contains the inner
        // argument list rewritten by the inner diagnostic's fix, so the batch fixer discards the overlapping inner
        // change while the outer rewrite already carries the correctly formatted nested call
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}