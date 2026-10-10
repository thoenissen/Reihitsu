using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6003SemicolonsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer, RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider>
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
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
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
                                    void Method()
                                    {
                                        var value = 0{|#0: |};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab immediately before a semicolon is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0{|#0:	|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that continuation-line indentation before a semicolon does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIndentationDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            ;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that continuation-line indentation before a semicolon does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIndentationDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            ;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line semicolon do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineSemicolonDoNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if false
                                        int disabled = 0 ;
                                #endif
                                        ;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line semicolon do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineSemicolonDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if false
                                        int disabled = 0 ;
                                #endif
                                        ;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the shared whitespace-span policy reports only same-line runs and rejects continuation indentation
    /// </summary>
    [TestMethod]
    public void VerifySharedWhitespaceSpanPolicyIsLineBounded()
    {
        const string sameLine = "value \t;";
        const string continuation = "#if false\ndisabled ;\n#endif\n    ;";
        var sameLineText = SourceText.From(sameLine);
        var continuationText = SourceText.From(continuation);
        var carriageReturnLineFeedText = SourceText.From(NormalizeToCarriageReturnLineFeed(continuation));

        Assert.AreEqual(TextSpan.FromBounds(5, 7), SameLinePrecedingWhitespaceAnalysis.GetSpan(sameLineText, sameLine.Length - 1));
        Assert.IsNull(SameLinePrecedingWhitespaceAnalysis.GetSpan(continuationText, continuation.LastIndexOf(';')));
        Assert.IsNull(SameLinePrecedingWhitespaceAnalysis.GetSpan(carriageReturnLineFeedText, carriageReturnLineFeedText.ToString().LastIndexOf(';')));
    }

    /// <summary>
    /// Verifies that a comment before a same-line semicolon still reports the diagnostic but withholds the fix,
    /// because deleting the whitespace run would glue the comment to the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineSemicolonWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1 /* Keep. */{|#0: |};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var semicolonToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.SemicolonToken));
                                                       var lastTrailingTrivia = semicolonToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the semicolon.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineSemicolonWithPrecedingComment"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineSemicolonWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1 /* Keep. */{|#0: |};
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var semicolonToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.SemicolonToken));
                                                       var lastTrailingTrivia = semicolonToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the semicolon.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line semicolon preceded by a block comment still reports the diagnostic on
    /// the same-line whitespace run between the comment and the semicolon, and that the fix is withheld rather
    /// than offered, because the guard must inspect the gap the whitespace run actually sits in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineSemicolonWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            /* keep */{|#0: |};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var semicolonToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.SemicolonToken));
                                                       var lastLeadingTrivia = semicolonToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The semicolon's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForContinuationLineSemicolonWithPrecedingComment"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineSemicolonWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            /* keep */{|#0: |};
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var semicolonToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.SemicolonToken));
                                                       var lastLeadingTrivia = semicolonToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The semicolon's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that <see cref="RemoveWhitespaceRunCodeFixProviderBase"/> withholds its fix when the
    /// reported span is not whitespace-only, so a future analyzer defect degrades to no fix being offered
    /// instead of deleting non-whitespace text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonWhitespaceSpanWithholdsFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().First(token => token.Text == "value").GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an empty reported span is treated as whitespace-only and still registers a fix, matching
    /// today's unconditional deletion of an empty span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptySpanStillRegistersFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var token = root.DescendantTokens().First(item => item.Text == "value");

                                                       return Location.Create(root.SyntaxTree, new TextSpan(token.SpanStart, 0));
                                                   });

        Assert.HasCount(1, actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1{|#0: |};
                                        int second = 2{|#1:	|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 1;
                                         int second = 2;
                                     }
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}