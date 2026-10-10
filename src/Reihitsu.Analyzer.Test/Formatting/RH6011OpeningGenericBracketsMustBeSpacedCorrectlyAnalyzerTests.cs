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
/// Test methods for <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer, RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List{|#0: |}<int>();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new List<int>();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that a space before the opening generic bracket of a type parameter list is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeParameterListIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0: |}<T>(T value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<T>(T value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before an opening generic bracket is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeOpeningGenericBracketIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List{|#0:	|}<int> Method() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> Method() => new();
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an opening generic bracket does not broaden the diagnostic span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeOpeningGenericBracketDoesNotBroadenDiagnosticSpan()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List /* Keep. */{|#0: |}<int> Method() => new();
                                }
                                """;

        await Verify(testData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that an opening generic bracket on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineOpeningGenericBracketDoesNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                        <int> Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening generic bracket on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineOpeningGenericBracketDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                        <int> Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line opening generic bracket do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineOpeningGenericBracketDoNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                #if false
                                    disabled text
                                #endif
                                        <int> Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line opening generic bracket do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineOpeningGenericBracketDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                #if false
                                    disabled text
                                #endif
                                        <int> Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a continuation-line opening generic bracket preceded by a block comment still reports the
    /// diagnostic on the same-line whitespace run between the comment and the bracket, and that the fix is
    /// withheld rather than offered, because the guard must inspect the gap the whitespace run actually sits in
    /// (the multi-line gap between <c>List</c> and the comment), not the gap on the far side of the bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingComment()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                        /* keep */{|#0: |}<int> Method() => new();
                                }
                                """;

        await Verify(testData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastLeadingTrivia = typeArgumentList.LessThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingComment"/> with
    /// the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                        /* keep */{|#0: |}<int> Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastLeadingTrivia = typeArgumentList.LessThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingComment"/> for a
    /// type parameter list instead of a type argument list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingCommentOnTypeParameterList()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                        /* keep */{|#0: |}<T>(T value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeParameterList = root.DescendantNodes().OfType<TypeParameterListSyntax>().First();
                                                       var lastLeadingTrivia = typeParameterList.LessThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats
    /// <see cref="VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingCommentOnTypeParameterList"/>
    /// with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineOpeningGenericBracketWithPrecedingCommentOnTypeParameterListCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                        /* keep */{|#0: |}<T>(T value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeParameterList = root.DescendantNodes().OfType<TypeParameterListSyntax>().First();
                                                       var lastLeadingTrivia = typeParameterList.LessThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that <see cref="VerifyCommentBeforeOpeningGenericBracketDoesNotBroadenDiagnosticSpan"/>'s same-line
    /// shape also withholds the fix outright, pinning today's already-correct trailing-trivia decision so the
    /// corrected leading-trivia branch cannot change it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineOpeningGenericBracketWithPrecedingComment()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List /* Keep. */{|#0: |}<int> Method() => new();
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastTrailingTrivia = typeArgumentList.LessThanToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineOpeningGenericBracketWithPrecedingComment"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineOpeningGenericBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List /* Keep. */{|#0: |}<int> Method() => new();
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastTrailingTrivia = typeArgumentList.LessThanToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a space before a generic type argument list inside a documentation-comment cref is not
    /// flagged, because the formatter never rewrites inside a cref and node-kind dispatch reaches structured
    /// trivia that the previous tree walk never saw
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySpaceBeforeGenericArgumentListInsideCrefIsIgnored()
    {
        const string testData = """
                                using System.Collections.Generic;

                                /// <summary>
                                /// See <see cref="List {T}"/>.
                                /// </summary>
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List{|#0: |}<int> First() => new();
                                    List{|#1:	|}<string> Second() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> First() => new();
                                     List<string> Second() => new();
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}