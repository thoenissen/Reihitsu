using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/> and <see cref="RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer, RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider>
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

                                        // Each on its own line
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");

                                        // Single argument
                                        Console.WriteLine("test1");

                                        // No arguments
                                        Console.WriteLine();

                                        // Multi-line argument (lambda) is ok
                                        Call("test1",
                                             Nested()
                                                 .Chain(),
                                             "test3");
                                    }

                                    string Call(string a, string b, string c) => a;
                                    string Nested() => "";
                                }

                                internal static class Extensions
                                {
                                    public static string Chain(this string s) => s;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that mixed argument placement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedArgumentPlacementIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine{|#0:("test1", "test2",
                                                          "test3")|};
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
                                                            "test2",
                                                            "test3");
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying that mixed argument placement with multiple groups is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedArgumentPlacementWithMultipleGroupsIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|#0:("test1", "test2",
                                             "test3", "test4")|};
                                    }

                                    void Call(string a, string b, string c, string d) { }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call("test1",
                                               "test2",
                                               "test3",
                                               "test4");
                                      }

                                      void Call(string a, string b, string c, string d) { }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying that an argument list carrying a comment in the join gap is reported without offering a code fix (issue #226)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedArgumentListIsReportedWithoutCodeFix()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine{|#0:("test1", "test2", // note
                                                          "test3")|};
                                    }
                                }
                                """;
        const string codeFixData = """
                                   using System;

                                   internal class TestClass
                                   {
                                       void Method()
                                       {
                                           Console.WriteLine("test1", "test2", // note
                                                             "test3");
                                       }
                                   }
                                   """;

        await Verify(testData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment in the join gap is gated like other comments, so the registration guard
    /// and the formatter agree on what counts as a comment (issue #226)
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
                                           Console.WriteLine("test1", "test2",
                                               /// note
                                               "test3");
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that an argument list carrying a comment after its closing parenthesis is reported (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y, int z) => x + y + z;

                                    private static int Use()
                                    {
                                        return Add{|#0:(1, 2,
                                                   3)|} /* note */ + 4;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying that an argument list carrying a comment after its closing parenthesis is offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y, int z) => x + y + z;

                                       private static int Use()
                                       {
                                           return Add(1, 2,
                                                      3) /* note */ + 4;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifying the control case: the identical input without the trailing comment is reported (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWithoutTrailingCommentIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y, int z) => x + y + z;

                                    private static int Use()
                                    {
                                        return Add{|#0:(1, 2,
                                                   3)|} + 4;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying the control case: the identical input without the trailing comment is offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWithoutTrailingCommentIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static int Add(int x, int y, int z) => x + y + z;

                                       private static int Use()
                                       {
                                           return Add(1, 2,
                                                      3) + 4;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
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
                                    private static int Add(int x, int y, int z) => x + y + z;

                                    private static int Use()
                                    {
                                        return Add{|#0:(1, 2,
                                                   3)|} /* note */ + 4;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      private static int Add(int x, int y, int z) => x + y + z;

                                      private static int Use()
                                      {
                                          return Add(1,
                                                     2,
                                                     3) /* note */ + 4;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
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
                                       private static int Add(int x, int y, int z) => x + y + z;

                                       private static int Use()
                                       {
                                           return Add
                                               /* note */ (1, 2,
                                               3) + 4;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
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
                                       private static int Add(int x, int y, int z) => x + y + z;

                                       private static int Use()
                                       {
                                           return Add(1, 2,
                                                      3)/* note */ + 4;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    #endregion // Tests
}