using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
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

    /// <summary>
    /// Verifies that a diagnostic span placed inside a token's leading trivia is still offered as a fix when the
    /// enclosing token gap contains no comment or directive, and that applying it deletes only the reported
    /// whitespace run and nothing else. This is the fix-offered counterpart of
    /// <see cref="VerifyFixIsNotOfferedWhenDiagnosticSpanIsInLeadingTriviaAndCommentPrecedesIt"/>: it proves the
    /// leading-trivia branch is not simply withholding unconditionally, and that the corrected guard span still
    /// covers exactly the edited gap rather than a wider one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsOfferedWhenDiagnosticSpanIsInLeadingTriviaAndGapIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return (
                                            0);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         return (
                                 0);
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

        Assert.HasCount(1,
                        actions,
                        "No comment or directive shares the edited gap; the fix must be offered.");

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        var changedDocument = applyChanges.ChangedSolution.Projects.Single().Documents.Single();
        var changedText = (await changedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false)).ToString();

        Assert.AreEqual(fixedData,
                        changedText,
                        "Applying the fix must delete only the reported whitespace run, proving the guard span did not widen the edit.");
    }

    /// <summary>
    /// Verifies that a diagnostic span placed inside a token's trailing trivia (the whitespace run immediately
    /// after the opening parenthesis, with a comment later in the same gap) still causes the guard to withhold
    /// the fix. This is the counterpart of
    /// <see cref="VerifyFixIsNotOfferedWhenDiagnosticSpanIsInLeadingTriviaAndCommentPrecedesIt"/>: it pins the
    /// trailing-trivia branch to today's byte-identical decision so the newly added leading-trivia branch cannot
    /// leak into it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenDiagnosticSpanIsInTrailingTriviaAndCommentFollowsIt()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return ( /* keep */    0);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var openParenToken = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.OpenParenToken));
                                                       var firstTrailingTrivia = openParenToken.TrailingTrivia.First();

                                                       Assert.IsTrue(firstTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The token's trailing trivia must start with the whitespace run immediately following it.");

                                                       return Location.Create(root.SyntaxTree, firstTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The whitespace run being deleted is followed by a comment in the same token gap; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedWhenDiagnosticSpanIsInTrailingTriviaAndCommentFollowsIt"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenDiagnosticSpanIsInTrailingTriviaAndCommentFollowsItCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return ( /* keep */    0);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData),
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var openParenToken = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.OpenParenToken));
                                                       var firstTrailingTrivia = openParenToken.TrailingTrivia.First();

                                                       Assert.IsTrue(firstTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The token's trailing trivia must start with the whitespace run immediately following it.");

                                                       return Location.Create(root.SyntaxTree, firstTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The whitespace run being deleted is followed by a comment in the same token gap; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a diagnostic span whose resolved token has no following non-zero-width token (the reported
    /// whitespace run sits after the last real token of a truncated document) withholds the fix instead of
    /// throwing. Before this fix, the derivation computed <c>TextSpan.FromBounds(token.Span.End, default.SpanStart)</c>,
    /// which threw <see cref="ArgumentOutOfRangeException"/> because the default token's span starts at <c>0</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenNoTokenFollowsTheResolvedTokenInATruncatedDocument()
    {
        // A regular string literal is used here instead of the repository's usual raw string literal because
        // the scenario requires the document to end with trailing whitespace after the opening parenthesis, and
        // a raw string literal cannot represent trailing whitespace on a content line.
        const string testData = "internal class TestClass\n{\n    int Method()\n    {\n        return ( ";

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var openParenToken = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.OpenParenToken));
                                                       var firstTrailingTrivia = openParenToken.TrailingTrivia.First();

                                                       Assert.IsTrue(firstTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The opening parenthesis's trailing trivia must be the whitespace run at the end of the truncated document.");

                                                       return Location.Create(root.SyntaxTree, firstTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "No token follows the opening parenthesis in this truncated document; the fix must be withheld rather than throw.");
    }

    /// <summary>
    /// Verifies that a diagnostic span whose resolved token has no preceding token at all (the reported whitespace
    /// run sits in the leading trivia of the very first token in the document) withholds the fix instead of
    /// throwing. The gap carries no comment or directive, so this isolates the "no preceding token" guard from the
    /// comment guard: without it, the derivation would compute <c>TextSpan.FromBounds(0, token.SpanStart)</c>,
    /// which does not throw but is not a gap any two tokens bracket, and the fix would be wrongly offered because
    /// the comment guard alone finds nothing to withhold on
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenNoTokenPrecedesTheResolvedTokenAtTheStartOfTheDocument()
    {
        // A regular string literal keeps the leading whitespace explicit and comment-free, isolating the "no
        // preceding token" guard from the comment/directive guard the other base-level tests already cover.
        const string testData = "    internal class TestClass\n{\n}";

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var firstToken = root.DescendantTokens().First();
                                                       var lastLeadingTrivia = firstToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The first token's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "No token precedes the first token in the document; the fix must be withheld rather than treat position 0 as a bracketing token.");
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