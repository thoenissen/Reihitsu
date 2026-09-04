using System.Linq;
using System.Threading.Tasks;

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
    /// parameter started on a line below the opening parenthesis rather than next to it (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// the formatter splits exactly that shape onto separate lines (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// untouched, so the fix diff does not inherit unrelated whole-member reformatting (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// between two parameters lies outside every one of those spans, so offering the fix would delete it (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying that a parameter list carrying a comment after its closing parenthesis is reported (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying that a parameter list carrying a comment after its closing parenthesis is offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying the control case: the identical input without the trailing comment is reported (issue #650)
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
    /// Verifying the control case: the identical input without the trailing comment is offered a code fix (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// and RH5109 (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// offered a code fix like the space-separated shape (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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