using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer"/> and <see cref="RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests : BatchCodeFixTestsBase<RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer, RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        int first,
                                        int second,
                                        int third)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [Requirement("RH5109-002")]
    [Requirement("RH5109-003")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that the continuation lines are aligned under the relocated first parameter when the original first
    /// parameter started on a line below the opening parenthesis rather than next to it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-003")]
    public async Task VerifyContinuationLinesAlignUnderRelocatedFirstParameter()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}
                                        int first, int second,
                                        int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-line parameter list whose parameters all start on the same line is detected, because
    /// the formatter splits exactly that shape onto separate lines.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyMultiLineParameterListWithSharedStartLineIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, System.Func<int,
                                                                              int> second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that a single parameter whose type spans multiple lines is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyMultiLineSingleParameterIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(System.Func<int,
                                                            int> only)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix formats only the rebuilt parameter list and leaves the surrounding member body
    /// untouched, so the fix diff does not inherit unrelated whole-member reformatting.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-003")]
    public async Task VerifyFixDoesNotReformatMemberBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                System.Console.WriteLine();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                 System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when the parameter list contains a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyFixIsNotOfferedWhenParameterListContainsComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a, int b, /* note */
                                                int c)
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a documentation comment inside the parameter list keeps the fix from being offered. The fix
    /// rebuilds the list from the raw text of each <see cref="ParameterListSyntax.Parameters"/> span, and a comment
    /// between two parameters lies outside every one of those spans, so offering the fix would delete it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyDocumentationCommentedParameterListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first, int second,
                                                   /// <summary>Third parameter.</summary>
                                                   int third)
                                       {
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParameterListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a documentation comment on the member itself does not block the fix. It sits in the leading
    /// trivia of the return type, outside the parameter list, so the rebuilt list cannot touch it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyDocumentedMemberStillGetsTheCodeFixAndKeepsItsComment()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       /// <summary>Does something.</summary>
                                       void Method(int first, int second,
                                                   int third)
                                       {
                                       }
                                   }
                                   """;

        var fixedCode = await ApplyCodeFixAsync(codeFixData);

        Assert.Contains("/// <summary>Does something.</summary>", fixedCode);
        Assert.AreNotEqual(codeFixData, fixedCode);
    }

    /// <summary>
    /// Verifying that a parameter list carrying a comment after its closing parenthesis is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third) /* note */
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifying that a parameter list carrying a comment after its closing parenthesis is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first, int second,
                                                   int third) /* note */
                                       {
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
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
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifying the control case: the identical input without the trailing comment is offered a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyWithoutTrailingCommentIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first, int second,
                                                   int third)
                                       {
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment after the closing parenthesis is fixed and the comment stays where it was written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-003")]
    [Requirement("RH5109-004")]
    public async Task VerifyTrailingCommentAfterClosingParenthesisIsFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third) /* note */
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method(int first,
                                                  int second,
                                                  int third) /* note */
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment written before the opening parenthesis still withholds the code fix. The replacement
    /// could not reach that region, but the guard covers it so the predicate reads identically across RH5101, RH5102
    /// and RH5109.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyCommentBeforeTheOpeningParenthesisWithholdsTheCodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method
                                           /* note */ (int first, int second,
                                                       int third)
                                       {
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment written directly against the closing parenthesis, without a separating space, is
    /// offered a code fix like the space-separated shape.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyCommentAdjacentToTheClosingParenthesisIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first, int second,
                                                   int third)/* note */
                                       {
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifies that a primary constructor's parameter list with parameters sharing a line is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyPrimaryConstructorParameterListIsDetected()
    {
        const string testData = """
                                internal class TestClass{|#0:(|}int first, int second,
                                                         int third)
                                {
                                    public int Sum => first + second + third;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that a parenthesized lambda's parameter list with parameters sharing a line is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifyParenthesizedLambdaParameterListIsDetected()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Func<int, int, int, int> sum = {|#0:(|}int first, int second,
                                                                        int third) => first + second + third;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that one diagnostic is reported per parameter list, however many parameters share a line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-002")]
    [Requirement("RH5109-003")]
    public async Task VerifyOneDiagnosticIsReportedForSeveralSharedLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third, int fourth)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third,
                                                 int fourth)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix puts every parameter on its own line under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-003")]
    public async Task VerifyIssueIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty conditional directive pair between two parameters does not keep the parameter list from
    /// being reported, with and without the symbol defined, and that no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyConditionalDirectiveBetweenParametersKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|RH5109:(|}int first, int second,
                                #if DEBUG
                                #endif
                                                int third)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that a parameter list is reported, and no code fix is offered, for every shape of conditional directive
    /// in a gap between its parameters: the analyzer judges the parameters the analyzed build compiles
    /// </summary>
    /// <param name="directives">The conditional directives, and the parameters they hold, between <c>int b,</c> and <c>int d</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n           int c,\n#endif", DisplayName = "Excluded parameter")]
    [DataRow("#if !NEVER\n           int c,\n#endif", DisplayName = "Active parameter")]
    [DataRow("#if NEVER\n           int c,\n#else\n           int e,\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n           int c,\n#elif !NEVER\n           int e,\n#endif", DisplayName = "If and elif")]
    [DataRow("#if false\n           int c,\n#endif", DisplayName = "If false")]
    [Requirement("RH5109-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyEveryShapeOfConditionalDirectiveBetweenParametersKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             void M{|RH5109:(|}int a, int b,
                         {{directives}}
                                    int d)
                             {
                             }
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters the analyzed build compiles all start on one line is not
    /// reported, also when a conditional directive in front of its closing parenthesis holds a parameter of another
    /// build
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyParametersOnOneLineWithAConditionalParameterInFrontOfTheClosingParenthesisAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a, int b
                                #if NEVER
                                           , int c
                                #endif
                                           )
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that Fix All lays out the parameter lists without a conditional directive in their gaps and leaves the
    /// one with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyFixAllLaysOutOnlyTheParameterListsWithoutAConditionalDirective()
    {
        const string testData = """
                                class C
                                {
                                    void M{|RH5109:(|}int a, int b,
                                #if FEATURE
                                #endif
                                           int c)
                                    {
                                    }

                                    void N{|RH5109:(|}int a, int b,
                                           int c)
                                    {
                                    }

                                    void O{|RH5109:(|}int a, int b,
                                           int c)
                                    {
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     void M{|RH5109:(|}int a, int b,
                                 #if FEATURE
                                 #endif
                                            int c)
                                     {
                                     }

                                     void N(int a,
                                            int b,
                                            int c)
                                     {
                                     }

                                     void O(int a,
                                            int b,
                                            int c)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a comment on the line of the token in front of the opening parenthesis does not withhold the
    /// code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    public async Task VerifyCommentBehindTokenBeforeOpeningParenthesisIsOfferedACodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method /* note */ {|#0:(|}int first, int second,
                                                          int third)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParameterListSyntax>()
                                                               .First()
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifies that a parameter list written on one line is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task VerifySingleLineParameterListIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first, int second, int third)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: a conditional directive or a pragma in a gap between the parameters, and a
    /// conditional directive inside a default value, withhold only the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyConditionalDirectiveInAGapOrInsideAParameterWithholdsOnlyTheFix()
    {
        const string testData = """
                                class C
                                {
                                    void M{|RH5109:(|}int a, int b,
                                #if DEBUG
                                #endif
                                           int c)
                                    {
                                    }

                                    void N{|RH5109:(|}int a, int b,
                                #pragma warning disable CS0168
                                           int c)
                                    {
                                    }

                                    void O{|RH5109:(|}int a, int b,
                                           int c = 1
                                #if DEBUG
                                               + 1
                                #endif
                                               + 2)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on one line below the opening parenthesis is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedOnlyBehindOpeningParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(
                                        int a, int b)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on the line of the opening parenthesis and whose closing parenthesis stands on a later line is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedOnlyInFrontOfClosingParenthesisIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N(int a, int b
                                          )
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on one line and whose closing parenthesis a comment holds on a later line is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseClosingParenthesisACommentHoldsOnALaterLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void Q(int a, int b // c
                                          )
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on one line, kept off the line of the opening parenthesis by a comment, is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseFirstParameterACommentKeepsOffTheOpeningParenthesisLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void O( // c
                                        int a, int b)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on one line, kept off the line of the opening parenthesis by a directive, is not reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWhoseFirstParameterADirectiveKeepsOffTheOpeningParenthesisLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void P(
                                #pragma warning disable CS0168
                                        int a, int b)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the parameter lists of every kind of owner whose parameters all start on one line below the opening parenthesis are not reported: constructor, local function, operator, delegate, record primary constructor, parenthesized lambda, and anonymous method, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListsWrappedOnlyBehindOpeningParenthesisAreNotReportedForEveryOwner(string endOfLine)
    {
        const string testData = """
                                using System;

                                record R(
                                    int A, int B);

                                delegate void D(
                                    int a, int b);

                                class C
                                {
                                    C(
                                        int a, int b)
                                    {
                                    }

                                    public static C operator +(
                                        C a, C b) => a;

                                    void M()
                                    {
                                        void L(
                                            int a, int b)
                                        {
                                        }

                                        Func<int, int, int> f = (
                                                                    int a, int b) => a + b;
                                        Func<int, int, int> g = delegate(
                                                                    int a, int b)
                                                                {
                                                                    return a + b;
                                                                };
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a parameter list wrapped behind its opening parenthesis is reported when a parameter on the shared line spans several lines, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWrappedBehindOpeningParenthesisWithASpanningParameterIsReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void N{|RH5109:(|}
                                        int a, Dictionary<int,
                                                          int> b)
                                    {
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: a list whose parameters start on two lines below the opening parenthesis is reported and that the fix, applied to every diagnostic, puts the first parameter behind the opening parenthesis and every further parameter on its own line, under LF and CRLF line endings, and that formatting the fix result changes nothing
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    [Requirement("RH5109-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyListWithParametersOnTwoLinesBelowOpeningParenthesisIsFixed(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M{|RH5109:(|}int a, int b,
                                           int c)
                                    {
                                    }

                                    void N{|RH5109:(|}
                                        int a, int b,
                                        int c)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                            int b,
                                            int c)
                                     {
                                     }

                                     void N(int a,
                                            int b,
                                            int c)
                                     {
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));

        AssertFormattingChangesNothing(ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the input of the conditional directive report: a list with a conditional directive whose excluded code holds a parameter is reported without a fix, with and without the symbol defined, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-004")]
    [Requirement("SHARED-DIAG-008")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyConditionalDirectiveExcludingAParameterKeepsTheReportAndWithholdsTheFix(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M{|RH5109:(|}int a, int b,
                                #if DEBUG
                                           int c,
                                #endif
                                           int d)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine), DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the union example of RH5109-001: a multi-line case-type list whose case types share a line is reported,
    /// and one case type per line is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5109-001")]
    public async Task UnionCaseTypeListWithCaseTypesSharingALineIsReported()
    {
        const string testCode = """
                                union U{|RH5109:(|}int, string,
                                        long);

                                union V(int,
                                        string,
                                        long);
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void MethodA{|#0:(|}int first, int second,
                                                int third)
                                    {
                                    }

                                    void MethodB{|#1:(|}int first, int second,
                                                int third)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void MethodA(int first,
                                                  int second,
                                                  int third)
                                     {
                                     }

                                     void MethodB(int first,
                                                  int second,
                                                  int third)
                                     {
                                     }
                                 }
                                 """;

        // Each fix rebuilds only its own declaration's parameter-list span, and a parameter list can never nest
        // inside another declaration's parameter list, so the two spans here, separated by a complete method body,
        // can never share or abut text; interference is structurally unreachable for this rule's fix, so this
        // proves both occurrences are corrected independently
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}