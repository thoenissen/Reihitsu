using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer, RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                [System.Obsolete]
                                internal class TestClass
                                {
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
                                [System.Obsolete{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a closing attribute bracket is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeClosingAttributeBracketIsDetectedAndFixed()
    {
        const string testData = """
                                [System.Obsolete{|#0:	|}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a same-line closing attribute bracket still reports the diagnostic but
    /// withholds the fix, because deleting the whitespace run would glue the comment to the bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingAttributeBracketWithPrecedingComment()
    {
        const string testData = """
                                [System.Obsolete /* Keep. */{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var attributeList = root.DescendantNodes().OfType<AttributeListSyntax>().First();
                                                       var lastTrailingTrivia = attributeList.CloseBracketToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineClosingAttributeBracketWithPrecedingComment"/> with
    /// the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingAttributeBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                [System.Obsolete /* Keep. */{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var attributeList = root.DescendantNodes().OfType<AttributeListSyntax>().First();
                                                       var lastTrailingTrivia = attributeList.CloseBracketToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line closing attribute bracket preceded by a block comment still reports
    /// the diagnostic on the same-line whitespace run between the comment and the bracket, and that the fix is
    /// withheld rather than offered, because the guard must inspect the gap the whitespace run actually sits in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineClosingAttributeBracketWithPrecedingComment()
    {
        const string testData = """
                                [System.Obsolete
                                    /* keep */{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var attributeList = root.DescendantNodes().OfType<AttributeListSyntax>().First();
                                                       var lastLeadingTrivia = attributeList.CloseBracketToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats
    /// <see cref="VerifyFixIsNotOfferedForContinuationLineClosingAttributeBracketWithPrecedingComment"/> with
    /// the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineClosingAttributeBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                [System.Obsolete
                                    /* keep */{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var attributeList = root.DescendantNodes().OfType<AttributeListSyntax>().First();
                                                       var lastLeadingTrivia = attributeList.CloseBracketToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a closing attribute bracket on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingAttributeBracketDoesNotProduceDiagnostic()
    {
        const string testData = """
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing attribute bracket on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingAttributeBracketDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing attribute bracket do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingAttributeBracketDoNotProduceDiagnostic()
    {
        const string testData = """
                                [System.Obsolete
                                #if false
                                disabled text
                                #endif
                                    ]
                                internal class TestClass;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing attribute bracket do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingAttributeBracketDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                [System.Obsolete
                                #if false
                                disabled text
                                #endif
                                    ]
                                internal class TestClass;
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                [System.Obsolete{|#0: |}]
                                [System.CLSCompliant(true){|#1:	|}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 [System.CLSCompliant(true)]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}