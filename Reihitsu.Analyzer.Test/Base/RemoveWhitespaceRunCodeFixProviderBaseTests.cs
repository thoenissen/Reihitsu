using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Test methods driving <see cref="Reihitsu.Analyzer.CodeFixes.Base.RemoveWhitespaceRunCodeFixProviderBase"/> directly
/// with a diagnostic span placed inside a token's leading trivia, using
/// <see cref="RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider"/> as the concrete deriver under test
/// </summary>
[TestClass]
public class RemoveWhitespaceRunCodeFixProviderBaseTests : BatchCodeFixTestsBase<RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer, RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a diagnostic span placed inside a token's leading trivia (indentation on the line following an
    /// opening parenthesis) still causes the guard to inspect the gap that actually contains the diagnostic span's
    /// neighboring comment. The comment sits between the opening parenthesis and the numeric literal, directly
    /// adjacent to the whitespace run the diagnostic span points at; the gap after the numeric literal (towards the
    /// closing parenthesis) contains no comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenDiagnosticSpanIsInLeadingTriviaAndCommentPrecedesIt()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return (
                                            /* keep */    0);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var numericToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.NumericLiteralToken));
                                                       var lastLeadingTrivia = numericToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The token's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The whitespace run being deleted sits directly after a comment in the same token gap; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedWhenDiagnosticSpanIsInLeadingTriviaAndCommentPrecedesIt"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenDiagnosticSpanIsInLeadingTriviaAndCommentPrecedesItCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return (
                                            /* keep */    0);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData),
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var numericToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.NumericLiteralToken));
                                                       var lastLeadingTrivia = numericToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The token's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The whitespace run being deleted sits directly after a comment in the same token gap; the fix must be withheld.");
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return ({|#0: |}a + ({|#1: |}b));
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return (a + (b));
                                     }
                                 }
                                 """;

        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}