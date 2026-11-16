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
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests : BatchCodeFixTestsBase<RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer, RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that valid argument placements produce no diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
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
    [Requirement("RH5102-001")]
    [Requirement("RH5102-002")]
    [Requirement("RH5102-003")]
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
    [Requirement("RH5102-001")]
    [Requirement("RH5102-002")]
    [Requirement("RH5102-003")]
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
    /// Verifying that an argument list carrying a comment in the join gap is reported without offering a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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
    /// and the formatter agree on what counts as a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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
    /// Verifying that an argument list carrying a comment after its closing parenthesis is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
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
    /// Verifying that an argument list carrying a comment after its closing parenthesis is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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
    /// Verifying the control case: the identical input without the trailing comment is reported.
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
    /// Verifying the control case: the identical input without the trailing comment is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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
    /// Verifying that a comment after the closing parenthesis is fixed and the comment stays where it was written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-003")]
    [Requirement("RH5102-004")]
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
    /// not start a line, so this region is not released together with the trailing one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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
    /// offered a code fix like the space-separated shape.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
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

    /// <summary>
    /// Verifying that an argument sharing its line with a lambda that spans several lines is reported on the whole
    /// argument list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("RH5102-002")]
    public async Task VerifyArgumentSharingLineWithMultiLineLambdaIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|#0:(1, () =>
                                        {
                                        })|};
                                    }

                                    void Call(params object[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line argument list in which every argument starts its own line is not reported, although
    /// one argument spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task VerifyMultiLineLambdaArgumentOnItsOwnLineIsNotReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1,
                                             () =>
                                             {
                                             });
                                    }

                                    void Call(params object[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that the fix lays out the argument list under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-003")]
    public async Task VerifyMixedArgumentPlacementIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|#0:(1, 2,
                                             3)|};
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));
    }

    /// <summary>
    /// Verifying that an empty conditional directive pair between two arguments keeps the argument list from being
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyConditionalDirectiveBetweenArgumentsKeepsTheListFromBeingReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1, 2,
                                #if DEBUG
                                #endif
                                             3);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a comment on the line of the token in front of the opening parenthesis does not withhold the
    /// code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
    public async Task VerifyCommentBehindTokenBeforeOpeningParenthesisIsOfferedACodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y, int z) => x + y + z;

                                    private static int Use()
                                    {
                                        return Add /* note */ {|#0:(1, 2,
                                            3)|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the argument list it formats holds a lambda whose body declares a local with
    /// a comment between the declaration's own tokens, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedWhenTheArgumentListHoldsADeclarationTheFormatterRefuses()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|RH5102:(1, () =>
                                                         {
                                                             int /* c */ x = 1;
                                                             return x;
                                                         },
                                             3)|};
                                    }

                                    void Call(int a, Func<int> b, int c)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix that puts each argument on its own line lays out a raw string literal argument as formatting
    /// the document lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixLaysOutARawStringLiteralArgumentAsTheFormatterDoes()
    {
        const string testCode = """"
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(1, """
                                            a
                                            """)|};
                                    }

                                    void M(int a, string s)
                                    {
                                    }
                                }
                                """";

        await VerifyCodeFixMatchesFormatter(testCode);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private static int Add(int x, int y, int z) => x + y + z;

                                  private static int Inner(int x, int y, int z) => x + y + z;

                                  private static int Use()
                                  {
                                      return Add(Inner(1, 2,
                                                           3),
                                                  5, 6);
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifying the specification example: a conditional directive in a gap between the arguments keeps the list from
    /// being reported, a pragma there withholds only the fix, and a conditional directive inside an argument withholds
    /// only the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
    public async Task VerifyConditionalDirectiveInAGapKeepsTheListFromBeingReportedButNotOneInsideAnArgument()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(1, 2,
                                #if DEBUG
                                #endif
                                          3);
                                        M{|RH5102:(1, 2,
                                #pragma warning disable CS0168
                                          3)|};
                                        Run{|RH5102:(1, 2,
                                            () =>
                                            {
                                #if DEBUG
                                                System.Console.WriteLine();
                                #endif
                                            })|};
                                    }

                                    int M(params int[] values) => 0;

                                    void Run(int a, int b, System.Action action)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifying that a conditional directive with alternatives in front of the first argument keeps the argument list
    /// from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
    public async Task VerifyConditionalDirectiveWithAlternativesInFrontOfTheFirstArgumentKeepsTheListFromBeingReported()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                #if DEBUG
                                          0,
                                #else
                                          1,
                                #endif
                                          2, 3,
                                          4);
                                    }

                                    int M(params int[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that the fix of an argument list wrapped only in front of a comma moves the comma behind the argument in
    /// front of it and puts every argument on its own line, under LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-003")]
    [Requirement("FMT-BREAK-012")]
    public async Task FixMovesLeadingCommaBehindPreviousArgument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|RH5102:(1
                                             , 2, 3)|};
                                    }

                                    void Call(int a, int b, int c)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3);
                                      }

                                      void Call(int a, int b, int c)
                                      {
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, "\n");
        await VerifyCodeFixWithEndOfLine(testData, resultData, "\r\n");
    }

    /// <summary>
    /// Verifying that the fix of an argument list wrapped behind its opening parenthesis and in front of a comma joins the
    /// first argument, moves the comma behind it, and puts every argument on its own line, under LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-003")]
    [Requirement("FMT-BREAK-012")]
    public async Task FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|RH5102:(
                                            1
                                            , 2, 3)|};
                                    }

                                    void Call(int a, int b, int c)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3);
                                      }

                                      void Call(int a, int b, int c)
                                      {
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, "\n");
        await VerifyCodeFixWithEndOfLine(testData, resultData, "\r\n");
    }

    /// <summary>
    /// Verifying that two argument lists wrapped in front of a comma in one document are both fixed, one after the other
    /// and all at once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-003")]
    [Requirement("FMT-BREAK-012")]
    public async Task FixMovesLeadingCommasOfSeveralArgumentListsInOneDocument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|#0:(1
                                             , 2, 3)|};
                                        Call{|#1:(
                                            1
                                            , 2, 3)|};
                                    }

                                    void Call(int a, int b, int c)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3);
                                          Call(1,
                                               2,
                                               3);
                                      }

                                      void Call(int a, int b, int c)
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that an argument list laid out with one argument per line and a comma a comment keeps at the start of its
    /// line is not reported, as the formatter leaves it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task VerifyCommentHeldLeadingCommaWithOneArgumentPerLineIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1,
                                             2 // keep
                                             , 3);
                                    }

                                    void Call(int a, int b, int c)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the input of the conditional directive report: an argument list with a conditional directive whose excluded code holds an argument is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-004")]
    [Requirement("SHARED-DIAG-008")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyConditionalDirectiveExcludingAnArgumentKeepsTheListFromBeingReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N(int a, int b, int c, int d)
                                    {
                                        M(a, b,
                                #if DEBUG
                                          c,
                                #endif
                                          d);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line below the opening parenthesis is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedOnlyBehindOpeningParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            1, 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on the line of the opening parenthesis and whose closing parenthesis stands on a later line is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedOnlyInFrontOfClosingParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(1, 2
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line between a line break behind the opening parenthesis and a line break in front of the closing parenthesis is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningAndInFrontOfClosingParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            1, 2
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line and whose closing parenthesis a comment holds on a later line is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseClosingParenthesisACommentHoldsOnALaterLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(1, 2 // c
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line, kept off the line of the opening parenthesis by a single-line comment, is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseFirstArgumentACommentKeepsOffTheOpeningParenthesisLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M( // c
                                            1, 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line below the opening parenthesis, behind a delimited comment in front of the first argument, is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseArgumentsFollowADelimitedCommentBelowTheOpeningParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            /* c */ 1, 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose only line break lies inside a multi-line comment in front of the first argument is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseFirstArgumentFollowsAMultiLineCommentIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M( /* a
                                            b */ 1, 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line, kept off the line of the opening parenthesis by a pragma directive, is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseFirstArgumentADirectiveKeepsOffTheOpeningParenthesisLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                #pragma warning disable CS0168
                                            1, 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line between a region directive behind the opening parenthesis and its end directive in front of the closing parenthesis is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseArgumentsARegionEnclosesOnOneLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                #region R
                                            1, 2
                                #endregion // R
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the argument lists of every kind of owner whose arguments all start on one line below the opening parenthesis are not reported: object creation, target-typed object creation, base and this constructor initializers, and a primary constructor base type, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListsWrappedOnlyBehindOpeningParenthesisAreNotReportedForEveryOwner(string endOfLine)
    {
        const string testData = """
                                class B
                                {
                                    public B(int a, int b)
                                    {
                                    }
                                }

                                class C : B
                                {
                                    public C(int a, int b)
                                        : base(
                                            a, b)
                                    {
                                    }

                                    public C(int a)
                                        : this(
                                            a, a)
                                    {
                                    }

                                    void N()
                                    {
                                        _ = new B(
                                            1, 2);
                                        B b = new(
                                            1, 2);
                                    }
                                }

                                class D() : B(
                                    1, 2);
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that argument lists of named and out arguments that all start on one line below the opening parenthesis are not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNamedAndOutArgumentsWrappedOnlyBehindOpeningParenthesisAreNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            a: 1, b: 2);
                                        O(
                                            out var x, out var y);
                                    }

                                    void M(int a, int b)
                                    {
                                    }

                                    void O(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that argument lists in which every argument starts its own line are not reported, also when the first argument stands below the opening parenthesis, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListsWithEveryArgumentOnItsOwnLineAreNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(1,
                                          2);
                                        M(
                                            1,
                                            2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of lists that are not reported: arguments that all start on one line, behind a line break after the opening parenthesis, in front of the closing parenthesis, behind a comment or a directive that keeps the first argument off the line of the opening parenthesis, or in front of a comment that holds the closing parenthesis on a later line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task VerifySpecificationExampleOfListsWhoseArgumentsAllStartOnOneLineIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            1, 2);
                                        M(1, 2
                                          );
                                        M( // c
                                            1, 2);
                                        M(
                                #pragma warning disable CS0168
                                            1, 2);
                                        M(1, 2 // c
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of lists that are reported: arguments that start on two lines, or an argument that spans lines next to another argument, also when every argument stands below the opening parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task VerifySpecificationExampleOfReportedAndCompliantListsIsReportedAsMarked()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(1, 2,
                                          3)|};
                                        M{|RH5102:(1, () =>
                                        {
                                        })|};
                                        M{|RH5102:(
                                            1, 2,
                                            3)|};
                                        M{|RH5102:(
                                            1, () =>
                                            {
                                            })|};
                                        M(1, 2, 3);
                                        M(1,
                                          2,
                                          3);
                                        M(1,
                                          () =>
                                          {
                                          });
                                    }

                                    void M(params object[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an argument list whose arguments start on two lines below the opening parenthesis is reported and that the fix, applied to every diagnostic, puts the first argument behind the opening parenthesis and every further argument on its own line, under LF and CRLF line endings, and that formatting the fix result changes nothing
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("RH5102-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWithArgumentsOnTwoLinesBelowOpeningParenthesisIsFixed(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, 2,
                                            3)|};
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void N()
                                     {
                                         M(1,
                                           2,
                                           3);
                                     }

                                     void M(params int[] values)
                                     {
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));

        AssertFormattingChangesNothing(ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis whose second argument is a lambda with a block body is reported and fixed to one argument per line, under LF and CRLF line endings, and that formatting the fix result changes nothing
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("RH5102-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningLambdaIsFixed(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, () =>
                                            {
                                            })|};
                                    }

                                    void M(params object[] values)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void N()
                                     {
                                         M(1,
                                           () =>
                                           {
                                           });
                                     }

                                     void M(params object[] values)
                                     {
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));

        AssertFormattingChangesNothing(ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis is reported when an argument on the shared line is a verbatim string literal that spans lines, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningVerbatimStringIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, @"a
                                b")|};
                                    }

                                    void M(params object[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis is reported when an argument on the shared line is a raw string literal that spans lines, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningRawStringIsReported(string endOfLine)
    {
        const string testData = """"
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, """
                                                a
                                                """)|};
                                    }

                                    void M(params object[] values)
                                    {
                                    }
                                }
                                """";

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis is reported when an argument on the shared line is an interpolated string with a line break inside a hole, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningInterpolatedStringIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N(int x)
                                    {
                                        M{|RH5102:(
                                            1, $"{x
                                            }")|};
                                    }

                                    void M(params object[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis is reported when an argument on the shared line is a call whose own arguments each start their own line, while that inner list is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningNestedCallIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, P(2,
                                                 3))|};
                                    }

                                    void M(params object[] values)
                                    {
                                    }

                                    int P(int a, int b) => a + b;
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that an argument list wrapped behind its opening parenthesis is reported without a code fix when an argument on the shared line spans lines through a multi-line comment inside it, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("RH5102-004")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithACommentSpanningAnArgumentIsReportedWithoutCodeFix(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M{|RH5102:(
                                            1, 2 /* a
                                            b */ + 3)|};
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that fixing every diagnostic of a document lays out the reported list only and leaves the lists whose arguments all start on one line as written, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("RH5102-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixLeavesListsWhoseArgumentsAllStartOnOneLineAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(
                                            1, 2);
                                        M{|RH5102:(
                                            1, 2,
                                            3)|};
                                        M(1, 2
                                          );
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void N()
                                     {
                                         M(
                                             1, 2);
                                         M(1,
                                           2,
                                           3);
                                         M(1, 2
                                           );
                                     }

                                     void M(params int[] values)
                                     {
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the with element example of RH5102-001: the argument list of a with element whose arguments share a
    /// line while the list spans lines is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task WithElementArgumentListIsReported()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    static IEqualityComparer<string> Create(bool ignoreCase) => ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

                                    void M()
                                    {
                                        HashSet<string> a = [with{|RH5102:(4, Create(
                                                                                 true))|}];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y, int z) => x + y + z;

                                    private static int Inner(int x, int y, int z) => x + y + z;

                                    private static int Use()
                                    {
                                        return Add{|#0:(Inner{|#1:(1, 2,
                                                             3)|},
                                                    5, 6)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     private static int Add(int x, int y, int z) => x + y + z;

                                     private static int Inner(int x, int y, int z) => x + y + z;

                                     private static int Use()
                                     {
                                         return Add(Inner(1,
                                                          2,
                                                          3),
                                                    5,
                                                    6);
                                     }
                                 }
                                 """;

        // The outer diagnostic's fix reformats the whole outer argument list, whose span fully contains the inner
        // argument list rewritten by the inner diagnostic's fix, so the batch fixer discards the overlapping inner
        // change while the outer rewrite already carries the correctly formatted nested call
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5102MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}