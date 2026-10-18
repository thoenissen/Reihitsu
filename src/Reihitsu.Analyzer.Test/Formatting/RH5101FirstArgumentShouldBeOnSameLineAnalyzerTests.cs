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
    [Requirement("RH5101-001")]
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
    [Requirement("RH5101-001")]
    [Requirement("RH5101-002")]
    [Requirement("RH5101-003")]
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
    [Requirement("RH5101-001")]
    [Requirement("RH5101-002")]
    [Requirement("RH5101-003")]
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
    /// refuses to collapse the first argument across that comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
    [Requirement("SHARED-JOIN-001")]
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
    /// so the code fix does not offer a no-op action.
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
    /// formatter refuses to collapse the first argument across that directive.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
    [Requirement("SHARED-JOIN-001")]
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
    /// formatter agree on what counts as a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
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
    /// Verifying that an argument list which carries a comment after the closing parenthesis is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
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
    /// Verifying that an argument list which carries a comment after the closing parenthesis is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
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
    /// Verifying the control case: the identical input without the trailing comment is reported.
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
    /// Verifying the control case: the identical input without the trailing comment is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
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
    /// Verifying that a comment after the closing parenthesis is fixed and the comment stays where it was written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-003")]
    [Requirement("RH5101-004")]
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
    /// not start a line, so this region is not released together with the trailing one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
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
    /// offered a code fix like the space-separated shape.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
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

    /// <summary>
    /// Verifying that a first argument below the opening parenthesis of an implicit object creation is reported on
    /// the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
    [Requirement("RH5101-002")]
    public async Task VerifyFirstArgumentOnNewLineInImplicitObjectCreationIsReported()
    {
        const string testData = """
                                using System.Text;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        StringBuilder sb = new(
                                            {|#0:"initial"|});
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that a first argument below the opening parenthesis of a constructor initializer is reported on the
    /// first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
    [Requirement("RH5101-002")]
    public async Task VerifyFirstArgumentOnNewLineInConstructorInitializerIsReported()
    {
        const string testData = """
                                internal class BaseClass
                                {
                                    public BaseClass(params int[] values)
                                    {
                                    }
                                }

                                internal class TestClass : BaseClass
                                {
                                    public TestClass()
                                        : base(
                                            {|#0:1|})
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that a first argument below the opening parenthesis of a primary constructor's base type is reported
    /// on the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-001")]
    [Requirement("RH5101-002")]
    public async Task VerifyFirstArgumentOnNewLineInPrimaryConstructorBaseTypeIsReported()
    {
        const string testData = """
                                internal class BaseClass
                                {
                                    public BaseClass(params int[] values)
                                    {
                                    }
                                }

                                internal class TestClass(int a) : BaseClass(
                                    {|#0:a|})
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix lays out the argument list under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-003")]
    public async Task VerifyFirstArgumentOnNewLineIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            {|#0:1|},
                                            2);
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
                                               2);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment on a line of its own in front of the first argument keeps the list from being
    /// reported, because the gap in front of the first argument holds it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-JOIN-001")]
    public async Task VerifyCommentOnOwnLineBeforeFirstArgumentIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            // first
                                            1,
                                            2);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a block comment ending in front of the first argument on its line keeps the list from being
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-JOIN-001")]
    public async Task VerifyBlockCommentEndingBeforeFirstArgumentIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            /* first */ 1,
                                            2);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that disabled text in front of the first argument keeps the list from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-JOIN-001")]
    public async Task VerifyDisabledTextBeforeFirstArgumentIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                #if false
                                            0,
                                #endif
                                            1,
                                            2);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a comment behind an argument inside the list is reported and withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
    public async Task VerifyCommentInsideArgumentListWithholdsTheCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            {|#0:1|}, // first
                                            2);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
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
    /// Verifying that a preprocessor directive between two arguments is reported and withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
    public async Task VerifyDirectiveInsideArgumentListWithholdsTheCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            {|#0:1|},
                                #if DEBUG
                                #endif
                                            2);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
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
    /// Verifying that a comment on the line of the token in front of the opening parenthesis does not withhold the
    /// code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5101-004")]
    public async Task VerifyCommentBehindTokenBeforeOpeningParenthesisIsOfferedACodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Add(int x, int y) => x + y;

                                    private static int Use()
                                    {
                                        return Add /* note */ (
                                            {|#0:1|},
                                            2);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
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