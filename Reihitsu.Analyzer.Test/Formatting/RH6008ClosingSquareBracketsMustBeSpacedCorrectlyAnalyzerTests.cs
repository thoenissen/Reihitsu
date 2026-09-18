using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer, RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        int[] values = [0];
                                        _ = values[0];
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
                                        int[] values = [0];
                                        _ = values[0{|#0: |}];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = [0];
                                         _ = values[0];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a same-line closing bracket still reports the diagnostic but withholds
    /// the fix, because deleting the whitespace run would glue the comment to the bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingBracketWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0 /* keep me */{|#0: |}];
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var closeBracketToken = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.CloseBracketToken));
                                                       var lastTrailingTrivia = closeBracketToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineClosingBracketWithPrecedingComment"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0 /* keep me */{|#0: |}];
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var closeBracketToken = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.CloseBracketToken));
                                                       var lastTrailingTrivia = closeBracketToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line closing bracket preceded by a block comment does not produce a
    /// diagnostic. RH6008's analyzer requires the immediately preceding token, not merely a preceding comment,
    /// to share the bracket's line, so a comment on a continuation line never enters the reported span here,
    /// unlike the shared <c>SameLinePrecedingWhitespaceAnalysis</c> a sibling rule such as RH6012 uses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingBracketWithPrecedingCommentDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0
                                            /* keep */];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyContinuationLineClosingBracketWithPrecedingCommentDoesNotProduceDiagnostic"/>
    /// with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingBracketWithPrecedingCommentDoesNotProduceDiagnosticCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0
                                            /* keep */];
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that multi-line collection expressions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionExpressionsDoNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        List<int> values =
                                        [
                                            1,
                                            2,
                                            3,
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
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
                                        int[] first = [0];
                                        int[] second = [0];
                                        _ = first[0{|#0: |}];
                                        _ = second[0{|#1: |}];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] first = [0];
                                         int[] second = [0];
                                         _ = first[0];
                                         _ = second[0];
                                     }
                                 }
                                 """;

        // Two element-access brackets sit on adjacent lines with no blank line between them; each fix only
        // removes its own leading whitespace run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}